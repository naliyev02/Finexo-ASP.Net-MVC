using Finexo.Contexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer("Server=DESKTOP-VRES0K5;Database=FinexoDb;Trusted_Connection=True;Trustservercertificate=true");
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "Default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);


app.Run();
