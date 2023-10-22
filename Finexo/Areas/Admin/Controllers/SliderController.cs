﻿using Finexo.Areas.Admin.ViewModels;
using Finexo.Contexts;
using Finexo.Models;
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

        public SliderController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var sliders = await _context.Sliders.ToListAsync();
            int slidersCount = sliders.Count;
            ViewBag.SlidersCount = slidersCount;

            List<SliderViewModel> slidersViewModels = new List<SliderViewModel>();

            foreach (var slider in sliders)
            {
                slidersViewModels.Add(new SliderViewModel()
                {
                    Id = slider.Id,
                    Title = slider.Title,
                    CreatedBy = "Nicat",
                    ModifiedBy = "Nicat"
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


                string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", slider.Image);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }

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
            var slider = await _context.Sliders.FirstOrDefaultAsync(s => s.Id == id);
            if (slider == null)
                return NotFound();

            string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", slider.Image);

            _context.Sliders.Remove(slider);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


    }
}
