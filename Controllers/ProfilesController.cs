using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniDatingApp.Data;
using MiniDatingApp.Models;
using MiniDatingApp.Services;
using MiniDatingApp.ViewModels;

namespace MiniDatingApp.Controllers;

public class ProfilesController : Controller
{
    private readonly AppDbContext _db;
    private readonly MatchService _matchService;
    private const string SESSION_KEY = "CurrentProfileId";

    public ProfilesController(AppDbContext db, MatchService matchService)
    {
        _db = db;
        _matchService = matchService;
    }

    public async Task<IActionResult> Index()
    {
        var profiles = await _db.Profiles.OrderByDescending(p => p.CreatedAt).ToListAsync();
        ViewBag.CurrentProfileId = HttpContext.Session.GetInt32(SESSION_KEY);
        return View(profiles);
    }

    [HttpPost]
    public IActionResult SetCurrent(int profileId)
    {
        HttpContext.Session.SetInt32(SESSION_KEY, profileId);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Create() => View(new ProfileCreateVm());

    [HttpPost]
    public async Task<IActionResult> Create(ProfileCreateVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var exists = await _db.Profiles.AnyAsync(p => p.Email == vm.Email);
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.Email), "Email đã tồn tại.");
            return View(vm);
        }

        var p = new Profile
        {
            Name = vm.Name,
            Age = vm.Age,
            Gender = vm.Gender,
            Bio = vm.Bio,
            Email = vm.Email
        };

        _db.Profiles.Add(p);
        await _db.SaveChangesAsync();

        // auto set current user
        HttpContext.Session.SetInt32(SESSION_KEY, p.Id);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Like(int targetId)
    {
        var currentId = HttpContext.Session.GetInt32("CurrentProfileId");
        if (currentId == null)
        {
            TempData["Toast"] = "Hãy chọn 'Bạn đang là ai?' trước khi Like.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ check currentId còn tồn tại trong DB (tránh session rác)
        var currentExists = await _db.Profiles.AnyAsync(p => p.Id == currentId.Value);
        if (!currentExists)
        {
            HttpContext.Session.Remove("CurrentProfileId");
            TempData["Toast"] = "Phiên đã cũ (user không còn tồn tại). Vui lòng chọn lại.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ check targetId tồn tại
        var targetExists = await _db.Profiles.AnyAsync(p => p.Id == targetId);
        if (!targetExists)
        {
            TempData["Toast"] = "Profile bạn muốn Like không tồn tại.";
            return RedirectToAction(nameof(Index));
        }

        var (isMatch, matchId) = await _matchService.LikeAsync(currentId.Value, targetId);
        TempData["Toast"] = isMatch ? "It’s a Match! 🎉" : "Đã like ❤️";
        return RedirectToAction(nameof(Index));
    }
}