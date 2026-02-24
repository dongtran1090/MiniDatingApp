using Microsoft.EntityFrameworkCore;
using MiniDatingApp.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();


if (builder.Environment.IsProduction())
{
    // Render/Deploy: SQLite (use /tmp to ensure writable)
    var sqliteConn = builder.Configuration.GetConnectionString("SqliteConnection")
                    ?? "Data Source=/tmp/app.db";

    builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(sqliteConn));
}
else
{
    // Local: SQL Server
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<MiniDatingApp.Services.MatchService>();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(6);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

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
