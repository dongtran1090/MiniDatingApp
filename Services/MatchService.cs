using Microsoft.EntityFrameworkCore;
using MiniDatingApp.Data;
using MiniDatingApp.Models;

namespace MiniDatingApp.Services;

public class MatchService
{
    private readonly AppDbContext _db;
    public MatchService(AppDbContext db) => _db = db;

    public static (int low, int high) NormalizePair(int a, int b)
        => a < b ? (a, b) : (b, a);

    public async Task<(bool isMatch, int? matchId)> LikeAsync(int likerId, int likedId)
    {
        if (likerId == likedId) return (false, null);

        var exists = await _db.Likes.AnyAsync(x => x.LikerId == likerId && x.LikedId == likedId);
        if (!exists)
        {
            _db.Likes.Add(new Like { LikerId = likerId, LikedId = likedId });
            await _db.SaveChangesAsync();
        }

        var reciprocal = await _db.Likes.AnyAsync(x => x.LikerId == likedId && x.LikedId == likerId);
        if (!reciprocal) return (false, null);

        var (low, high) = NormalizePair(likerId, likedId);
        var match = await _db.Matches.FirstOrDefaultAsync(m => m.UserLowId == low && m.UserHighId == high);
        if (match == null)
        {
            match = new Match { UserLowId = low, UserHighId = high };
            _db.Matches.Add(match);
            await _db.SaveChangesAsync();
        }

        return (true, match.Id);
    }

    // Interval intersection: find first common overlap (sorted by start)
    public static (DateTime start, DateTime end)? FindFirstCommonSlot(
        List<AvailabilitySlot> a, List<AvailabilitySlot> b)
    {
        a = a.OrderBy(x => x.StartTime).ToList();
        b = b.OrderBy(x => x.StartTime).ToList();

        int i = 0, j = 0;
        while (i < a.Count && j < b.Count)
        {
            var start = a[i].StartTime > b[j].StartTime ? a[i].StartTime : b[j].StartTime;
            var end = a[i].EndTime < b[j].EndTime ? a[i].EndTime : b[j].EndTime;

            if (start < end) return (start, end);

            if (a[i].EndTime < b[j].EndTime) i++;
            else j++;
        }
        return null;
    }

    public async Task<(bool found, string message, DateSuggestion? suggestion)> ComputeSuggestionAsync(int matchId)
    {
        var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == matchId);
        if (match == null) return (false, "Match không tồn tại.", null);

        var now = DateTime.Now;
        var limit = now.AddDays(21);

        var slots = await _db.AvailabilitySlots
            .Where(s => s.MatchId == matchId && s.StartTime >= now && s.StartTime <= limit)
            .ToListAsync();

        var aSlots = slots.Where(s => s.ProfileId == match.UserLowId).ToList();
        var bSlots = slots.Where(s => s.ProfileId == match.UserHighId).ToList();

        if (aSlots.Count == 0 || bSlots.Count == 0)
            return (false, "Cả hai bên cần chọn thời gian rảnh trước.", null);

        var overlap = FindFirstCommonSlot(aSlots, bSlots);
        if (overlap == null)
            return (false, "Chưa tìm được thời gian trùng. Vui lòng chọn lại.", null);

        // upsert suggestion
        var existing = await _db.DateSuggestions.FirstOrDefaultAsync(x => x.MatchId == matchId);
        if (existing == null)
        {
            existing = new DateSuggestion
            {
                MatchId = matchId,
                StartTime = overlap.Value.start,
                EndTime = overlap.Value.end
            };
            _db.DateSuggestions.Add(existing);
        }
        else
        {
            existing.StartTime = overlap.Value.start;
            existing.EndTime = overlap.Value.end;
        }

        await _db.SaveChangesAsync();
        return (true, "OK", existing);
    }
}