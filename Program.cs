using Microsoft.EntityFrameworkCore;
using MiniDatingApp.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ===============================
// DATABASE - SQLite (Local + Render)
// ===============================
var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection")
                 ?? "Data Source=/tmp/app.db"; // fallback for safety

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(sqliteConn));

// ===============================
// SERVICES
// ===============================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<MiniDatingApp.Services.MatchService>();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(6);
});

if (builder.Environment.IsProduction())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var app = builder.Build();

// ===============================
// AUTO MIGRATION (Startup)
// ===============================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ===============================
// MIDDLEWARE
// ===============================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Profiles}/{action=Index}/{id?}");

app.Run();