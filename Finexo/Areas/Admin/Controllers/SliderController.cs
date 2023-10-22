using Finexo.Areas.Admin.ViewModels;
using Finexo.Contexts;
using Finexo.Models;
using Finexo.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;

namespace Finexo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SliderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private int _count = 0;

        public SliderController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            var sliders = _context.Sliders.AsEnumerable();
            _count = sliders.Count();
        }

        public async Task<IActionResult> Index()
        {
            var sliders = await _context.Sliders.OrderByDescending(s => s.ModifiedAt).ToListAsync();
            ViewBag.SlidersCount = _count;

            List<SliderViewModel> slidersViewModels = new List<SliderViewModel>();

            foreach (var slider in sliders)
            {
                slidersViewModels.Add(new SliderViewModel()
                {
                    Id = slider.Id,
                    Title = slider.Title,
                    CreatedBy = slider.CreatedBy,
                    ModifiedBy = slider.ModifiedBy
                });
            }
            return View(slidersViewModels);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderViewModel sliderViewModel)
        {
            if (!ModelState.IsValid)
                return View();


            if (sliderViewModel.Image == null)
            {
                ModelState.AddModelError("Image", "File must not be empty");
                return View();
            }

            if (!sliderViewModel.Image.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Image", "File must be image");
                return View();
            }

            string fileName = $"{Guid.NewGuid()}-{sliderViewModel.Image.FileName}";
            string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", fileName);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await sliderViewModel.Image.CopyToAsync(stream);
            }

            Slider slider = new()
            {
                Title = sliderViewModel.Title,
                Description = sliderViewModel.Description,
                Image = fileName,
                CreatedBy = "Nicat",
                ModifiedBy = "Nicat"
            };

            await _context.Sliders.AddAsync(slider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            var slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider == null)
                return NotFound();

            var sliderViewModel = new SliderViewModel
            {
                Id = slider.Id,
                Title = slider.Title,
                Description = slider.Description
            };
            ViewBag.Image = slider.Image;
            return View(sliderViewModel);
        }


        public async Task<IActionResult> Update(int id)
        {
            var slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider == null)
                return NotFound();

            SliderViewModel sliderViewModel = new()
            {
                Id = slider.Id,
                Title = slider.Title,
                Description = slider.Description,
            };

            return View(sliderViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, SliderViewModel sliderViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            var slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider == null)
                return NotFound();

            if (sliderViewModel.Image != null)
            {

                if (!sliderViewModel.Image.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("Image", "File must be image");
                    return View();
                }

                FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", slider.Image);

                string fileName = $"{Guid.NewGuid()}-{sliderViewModel.Image.FileName}";
                var newPath = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", fileName);
                using (FileStream stream = new FileStream(newPath, FileMode.Create))
                {
                    await sliderViewModel.Image.CopyToAsync(stream);
                }

                slider.Image = fileName;
            }

            slider.Title = sliderViewModel.Title;
            slider.Description = sliderViewModel.Description;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (_count == 1)
                return BadRequest();

            var slider = _context.Sliders.FirstOrDefault(s => s.Id == id);
            if (slider == null)
                return NotFound();

            var sliderViewModel = new SliderViewModel
            {
                Id = slider.Id,
                Title = slider.Title,
                Description = slider.Description
            };
            ViewBag.Image = slider.Image;

            return View(sliderViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Delete))]
        public async Task<IActionResult> DeleteSlider(int id)
        {
            if (_count == 1)
                return BadRequest();

            var slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider == null)
                return NotFound();

            FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", slider.Image);

            _context.Sliders.Remove(slider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
