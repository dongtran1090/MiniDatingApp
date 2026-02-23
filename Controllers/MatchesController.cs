using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniDatingApp.Data;
using MiniDatingApp.Models;
using MiniDatingApp.Services;
using MiniDatingApp.ViewModels;

namespace MiniDatingApp.Controllers;

public class MatchesController : Controller
{
    private readonly AppDbContext _db;
    private readonly MatchService _matchService;
    private const string SESSION_KEY = "CurrentProfileId";

    public MatchesController(AppDbContext db, MatchService matchService)
    {
        _db = db;
        _matchService = matchService;
    }

    public async Task<IActionResult> Index()
    {
        var currentId = HttpContext.Session.GetInt32(SESSION_KEY);
        if (currentId == null)
        {
            TempData["Toast"] = "Hãy chọn bạn đang là ai trước.";
            return RedirectToAction("Index", "Profiles");
        }

        var matches = await _db.Matches
            .Include(m => m.Suggestion)
            .Where(m => m.UserLowId == currentId || m.UserHighId == currentId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentProfileId = currentId.Value;
        return View(matches);
    }

    public async Task<IActionResult> Details(int id)
    {
        var currentId = HttpContext.Session.GetInt32(SESSION_KEY);
        if (currentId == null) return RedirectToAction("Index", "Profiles");

        var match = await _db.Matches
            .Include(m => m.Suggestion)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (match == null) return NotFound();
        if (match.UserLowId != currentId && match.UserHighId != currentId)
            return Forbid();

        var p1 = await _db.Profiles.FindAsync(match.UserLowId);
        var p2 = await _db.Profiles.FindAsync(match.UserHighId);

        ViewBag.P1 = p1;
        ViewBag.P2 = p2;

        var slots = await _db.AvailabilitySlots
            .Where(s => s.MatchId == id)
            .OrderBy(s => s.StartTime)
            .ToListAsync();

        ViewBag.Slots = slots;
        ViewBag.CurrentProfileId = currentId.Value;

        var vm = new AvailabilityVm
        {
            MatchId = id,
            ProfileId = currentId.Value,
            Date = DateOnly.FromDateTime(DateTime.Now),
            From = new TimeOnly(9, 0),
            To = new TimeOnly(10, 0)
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> AddAvailability(AvailabilityVm vm)
    {
        var currentId = HttpContext.Session.GetInt32(SESSION_KEY);
        if (currentId == null) return RedirectToAction("Index", "Profiles");
        if (currentId.Value != vm.ProfileId) return Forbid();

        if (!ModelState.IsValid) return RedirectToAction(nameof(Details), new { id = vm.MatchId });

        var start = vm.Date.ToDateTime(vm.From);
        var end = vm.Date.ToDateTime(vm.To);

        if (end <= start)
        {
            TempData["Toast"] = "Giờ kết thúc phải lớn hơn giờ bắt đầu.";
            return RedirectToAction(nameof(Details), new { id = vm.MatchId });
        }

        var now = DateTime.Now;
        var limit = now.AddDays(21);
        if (start < now || start > limit)
        {
            TempData["Toast"] = "Chỉ được chọn trong 3 tuần tới.";
            return RedirectToAction(nameof(Details), new { id = vm.MatchId });
        }

        // (Optional) chặn slot overlap trong cùng user/match
        var overlap = await _db.AvailabilitySlots.AnyAsync(s =>
            s.MatchId == vm.MatchId &&
            s.ProfileId == vm.ProfileId &&
            start < s.EndTime && end > s.StartTime);

        if (overlap)
        {
            TempData["Toast"] = "Slot bị trùng với slot bạn đã chọn trước đó.";
            return RedirectToAction(nameof(Details), new { id = vm.MatchId });
        }

        _db.AvailabilitySlots.Add(new AvailabilitySlot
        {
            MatchId = vm.MatchId,
            ProfileId = vm.ProfileId,
            StartTime = start,
            EndTime = end
        });

        await _db.SaveChangesAsync();
        TempData["Toast"] = "Đã lưu thời gian rảnh.";
        return RedirectToAction(nameof(Details), new { id = vm.MatchId });
    }

    [HttpPost]
    public async Task<IActionResult> Suggest(int matchId)
    {
        var currentId = HttpContext.Session.GetInt32(SESSION_KEY);
        if (currentId == null) return RedirectToAction("Index", "Profiles");

        var result = await _matchService.ComputeSuggestionAsync(matchId);
        TempData["Toast"] = result.found
            ? $"✅ Hai bạn có date hẹn vào: {result.suggestion!.StartTime:dd/MM/yyyy HH:mm} - {result.suggestion.EndTime:HH:mm}"
            : result.message;

        return RedirectToAction(nameof(Details), new { id = matchId });
    }
}