using Finexo.Contexts;
using Finexo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finexo.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var sliders = await _context.Sliders.ToListAsync();
            var services = await _context.Services.ToListAsync();

            HomeViewModel homeViewModel = new()
            {
                Sliders = sliders,
                Services = services
            };

            return View(homeViewModel);
        }
    }
}
