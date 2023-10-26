using Finexo.Areas.Admin.ViewModels;
using Finexo.Contexts;
using Finexo.Models;
using Finexo.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finexo.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class WhyUsItemController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private int _count = 0;

        public WhyUsItemController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            var whyUsItems = _context.WhyUsItems.AsEnumerable();
            _count = whyUsItems.Count();

        }

        public async Task<IActionResult> Index()
        {
            var whyUsItems = await _context.WhyUsItems.ToListAsync();
            ViewBag.ItemsCount = whyUsItems.Count();

            List<WhyUsItemViewModel> whyUsItemViewModels = new List<WhyUsItemViewModel>();
            foreach (var item in whyUsItems)
            {
                whyUsItemViewModels.Add(new WhyUsItemViewModel
                {
                    Id = item.Id,
                    Title = item.Title,
                    CreatedBy = item.CreatedBy,
                    ModifiedBy = item.ModifiedBy

                });
            }
            return View(whyUsItemViewModels);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WhyUsItemViewModel whyUsItemViewModel)
        {
            if (!ModelState.IsValid)
                return View();

            if (whyUsItemViewModel.Image == null)
            {
                ModelState.AddModelError("Image", "File must not be empty");
                return View();
            }

            if (!whyUsItemViewModel.Image.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("Image", "File must be image");
                return View();
            }

            string fileName = $"{Guid.NewGuid()}-{whyUsItemViewModel.Image.FileName}";
            string path = Path.Combine(_webHostEnvironment.WebRootPath,"assets","images","website-images","whyus-images",fileName);

            using (FileStream stream = new FileStream(path,FileMode.Create))
            {
                await whyUsItemViewModel.Image.CopyToAsync(stream);
            }

            WhyUsItem whyUsItem = new()
            {
                Title = whyUsItemViewModel.Title,
                Description = whyUsItemViewModel.Description,
                Image = fileName,
                CreatedBy = "Nicat",
                ModifiedBy = "Nicat"
            };

            await _context.WhyUsItems.AddAsync(whyUsItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detail(int id)
        {
            var item = await _context.WhyUsItems.FirstOrDefaultAsync(w => w.Id == id);
            if (item == null) 
                return NotFound();

            WhyUsItemViewModel whyUsItemViewModel = new()
            {
                Title = item.Title,
                Description = item.Description,
            };
            ViewBag.Image = item.Image;

            return View(whyUsItemViewModel);
        }

        public async Task<IActionResult> Update(int id)
        {
            var item = await _context.WhyUsItems.FirstOrDefaultAsync(w => w.Id == id);
            if (item == null)
                return NotFound();

            //WhyUsItemViewModel whyUsItemViewModel = new()
            //{
            //    Title = item.Title,
            //    Description = item.Description,
            //};

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, WhyUsItemViewModel whyUsItemViewModel)
        {
            var item = await _context.WhyUsItems.FirstOrDefaultAsync(w => w.Id == id);
            if (item == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View();


            if (whyUsItemViewModel.Image != null)
            {
                if (!whyUsItemViewModel.Image.ContentType.Contains("image/"))
                {
                    ModelState.AddModelError("Image", "File must be image");
                    return View();
                }

                FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", "whyus-images", item.Image);


                string fileName = $"{Guid.NewGuid()}-{whyUsItemViewModel.Image.FileName}";
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", "whyus-images", fileName);

                using (FileStream stream = new FileStream(path,FileMode.Create))
                {
                    await whyUsItemViewModel.Image.CopyToAsync(stream);
                }

                item.Image = fileName;
            }

            item.Title = whyUsItemViewModel.Title;
            item.Description = whyUsItemViewModel.Description;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (_count == 1)
                return BadRequest();

            var item = await _context.WhyUsItems.FirstOrDefaultAsync(w => w.Id == id);
            if (item == null)
                return NotFound();

            WhyUsItemViewModel whyUsItemViewModel = new()
            {
                Title = item.Title,
                Description = item.Description,
            };
            ViewBag.Image = item.Image;

            return View(whyUsItemViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName(nameof(Delete))]
        public async Task<IActionResult> DeleteItem(int id)
        {
            if (_count == 1)
                return BadRequest();

            var item = await _context.WhyUsItems.FirstOrDefaultAsync(w => w.Id == id);
            if (item == null)
                return NotFound();

            FileService.DeleteFile(_webHostEnvironment.WebRootPath, "assets", "images", "website-images", "whyus-images", item.Image);

            _context.WhyUsItems.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }





    }
}
