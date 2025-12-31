using Microsoft.EntityFrameworkCore;
using SmartCollab.Models; // ou SmartCollab.Data si ton DbContext est là
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure DbContext
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    );

    // Configure Identity
    builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        // Password
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;

        // User
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


    builder.Services.AddControllersWithViews()
           .AddRazorRuntimeCompilation(); // pour recharger les vues sans redémarrage
    

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.SlidingExpiration = true;
    });


    var app = builder.Build();

    // Configure la pipeline HTTP
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();

    app.UseAuthentication(); // 🔹 Nécessaire pour Identity
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine("=== ERREUR AU DEMARRAGE ===");
    Console.WriteLine(ex.ToString());
    Console.WriteLine("===========================");
    throw;
}
