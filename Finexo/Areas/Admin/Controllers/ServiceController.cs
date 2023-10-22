using Finexo.Areas.Admin.ViewModels;
using Finexo.Contexts;
using Finexo.Models;
using Finexo.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finexo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private int _count = 0;

        public ServiceController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;

            var services = _context.Services.AsEnumerable();
            _count = services.Count();
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.OrderByDescending(s => s.ModifiedAt).ToListAsync();
            ViewBag.SlidersCount = _count;

            List<ServiceViewModel> serviceViewModels = new List<ServiceViewModel>();
            foreach (var service in services)
            {
                serviceViewModels.Add(new ServiceViewModel
                {
                    Id = service.Id,
                    Title = service.Title,
                    CreatedBy = service.CreatedBy,
                    ModifiedBy = service.ModifiedBy

                });
            }

            return View(serviceViewModels);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceViewModel serviceViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            if (serviceViewModel.Image == null)
            {
                ModelState.AddModelError("Image", "Image cannot be empty");
                return View();
            }

            if (!serviceViewModel.Image.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Image", "The file must be of image type");
                return View();
            }

            string fileName = $"{Guid.NewGuid()}-{serviceViewModel.Image.FileName}";
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", fileName);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await serviceViewModel.Image.CopyToAsync(stream);
            }

            Service service = new()
            {
                Title = serviceViewModel.Title,
                Description = serviceViewModel.Description,
                Image = fileName,
                CreatedBy = "Nicat",
                ModifiedBy = "Nicat"
            };

            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                return View();

            ServiceViewModel serviceViewModel = new()
            {
                Title = service.Title,
                Description = service.Description,
            };
            ViewBag.Image = service.Image;

            return View(serviceViewModel);
        }

        public async Task<IActionResult> Update(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                return View();

            ServiceViewModel serviceViewModel = new()
            {
                Title = service.Title,
                Description = service.Description,
            };

            return View(serviceViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, ServiceViewModel serviceViewModel)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View();

            if (serviceViewModel.Image != null)
            {
                if (serviceViewModel.Image == null)
                {
                    ModelState.AddModelError("Image", "Image cannot be empty");
                    return View();
                }

                if (!serviceViewModel.Image.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("Image", "The file must be of image type");
                    return View();
                }

                FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", service.Image);

                string fileName = $"{Guid.NewGuid()}-{serviceViewModel.Image.FileName}";
                var newPath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", fileName);
                using (FileStream stream = new FileStream(newPath, FileMode.Create))
                {
                    await serviceViewModel.Image.CopyToAsync(stream);
                }
                service.Image = fileName;
            }

            service.Title = serviceViewModel.Title;
            service.Description = serviceViewModel.Description;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (_count == 1)
                return BadRequest();

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                return NotFound();

            ServiceViewModel serviceViewModel = new()
            {
                Title = service.Title,
                Description = service.Description,
            };

            ViewBag.Image = service.Image;
            return View(serviceViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Delete))]
        public async Task<IActionResult> DeleteService(int id)
        {
            if (_count == 1)
                return BadRequest();

            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
                return NotFound();

            FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", service.Image);

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



    }
}
