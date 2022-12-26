using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Eshop.Data;
using Eshop.Models;
using Microsoft.Extensions.Hosting;
using System.Security.Principal;

namespace Eshop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly EshopContext _context;
        private readonly IWebHostEnvironment _environment;
        Product products = new Product();
        public ProductsController(EshopContext context, IWebHostEnvironment environment)
        {
            _environment = environment;
            _context = context;
        }

        public async Task<IActionResult> Index(string SearchString)
        {
            ViewData["GetSearch"] = SearchString;
            ViewBag.loadNamePType = _context.ProductTypes.Where(x => x.Status);
            var search = from a in _context.Products select a;
            if (!String.IsNullOrEmpty(SearchString))
            {
                search = search.Where(x => x.Name.Contains(SearchString) || x.Description.Contains(SearchString));
            }
            return View(await search.AsNoTracking().ToListAsync());
        }
        [HttpPost]
        public IActionResult Index(Product product, int priceMin = 0, int priceMax = int.MaxValue)
        {
            CartsController carts = new CartsController(_context);
            var IdUser = HttpContext.Session.GetInt32("Id");
            if (IdUser != null)
            {
                ViewBag.loadCarts = carts.loadCartProduct(IdUser);
            }
            ViewBag.loadNamePType = _context.ProductTypes.Where(x => x.Status);
            if (product == null)
                return RedirectToAction("Index", "Home");
            if (priceMin < 0 || (priceMin > priceMax))
            {
                ViewBag.erorrPrice = "Nhập giá không hợp lệ";
                var searchProduct = _context.Products.ToList();
                return View(searchProduct);
            }
            if (product.ProductTypeId == 1 && product.Name == null)
            {
                var searchProduct = _context.Products.Where(x => (x.Price >= priceMin && x.Price <= priceMax)).ToList();
                return View(searchProduct);
            }
            else if (product.ProductTypeId == 1 && product.Name != null)
            {
                var searchProduct = _context.Products.Where(x => (x.Status && x.Name.Contains(product.Name))).Where(x => (x.Price >= priceMin && x.Price <= priceMax));
                return View(searchProduct);
            }
            else if (product.ProductTypeId != 1 && product.Name != null)
            {
                var searchProduct = _context.Products.Where(x => (x.Status && x.Name.Contains(product.Name) && x.ProductTypeId == product.ProductTypeId)).Where(x => (x.Price >= priceMin && x.Price <= priceMax));
                return View(searchProduct);
            }
            else
            {
                var searchProduct = _context.Products.Where(x => (x.Status && x.ProductTypeId == product.ProductTypeId)).Where(x => (x.Price >= priceMin && x.Price <= priceMax));
                return View(searchProduct);
            }
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.ProductType)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}


