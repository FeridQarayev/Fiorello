using Fiorello.DAL;
using Fiorello.Helpers;
using Fiorello.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static Fiorello.Helpers.Helper;

namespace Fiorello.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public ProductsController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _db.Products.Include(x=>x.Category).ToListAsync();
            return View(products);
        }
        public async Task<IActionResult> Create()
        {
            ViewBag.Category = await _db.Categories.ToListAsync();
            return View();
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(Product product,int categoryId)
        {
            ViewBag.Category = await _db.Categories.ToListAsync();
            if (!ModelState.IsValid)
            {
                return View();
            }
            bool isExist = await _db.Products.AnyAsync(x => x.Name == product.Name);
            if (isExist)
            {
                ModelState.AddModelError("Title", "This Product is already exists!");
                return View();
            }
            if (product.Photo == null)
            {
                ModelState.AddModelError("Photo", "Please choose photo");
                return View();
            }
            if (!product.Photo.IsImage())
            {
                ModelState.AddModelError("Photo", "Please choose Image file");
                return View();
            }
            if (product.Photo.IsOlderTwoMB())
            {
                ModelState.AddModelError("Photo", "Image max 2MB");
                return View();
            }
            string folder = Path.Combine(_env.WebRootPath, "img");
            product.Image = await product.Photo.SaveFileAsync(folder);
            product.CategoryId = categoryId;
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
                return NotFound();
            Product dbproduct = await _db.Products.FirstOrDefaultAsync(x=>x.Id==id);
            if (dbproduct == null)
                return BadRequest();
            ViewBag.Category = await _db.Categories.ToListAsync();
            return View(dbproduct);
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Update(int? id,Product product, int categoryId)
        {
            if (id == null)
                return NotFound();
            Product dbproduct = await _db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (dbproduct == null)
                return BadRequest();
            ViewBag.Category = await _db.Categories.ToListAsync();
            if (!ModelState.IsValid)
            {
                return View(dbproduct);
            }
            bool isExist = await _db.Products.AnyAsync(x => x.Name == product.Name&&x.Id!=id);
            if (isExist)
            {
                ModelState.AddModelError("Name", "This Product is already exists!");
                return View(dbproduct);
            }
            if (product.Photo != null)
            {
                if (!product.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please choose Image file");
                    return View(dbproduct);
                }
                if (product.Photo.IsOlderTwoMB())
                {
                    ModelState.AddModelError("Photo", "Image max 2MB");
                    return View(dbproduct);
                }
                string folder = Path.Combine(_env.WebRootPath, "img");
                dbproduct.Image = await product.Photo.SaveFileAsync(folder);
            }
            dbproduct.CategoryId = categoryId;
            dbproduct.Name = product.Name;
            dbproduct.Price = product.Price;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
