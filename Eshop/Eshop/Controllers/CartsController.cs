using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Eshop.Data;
using Eshop.Models;
using System.Threading.Tasks.Dataflow;
using System.Net;
using System.Buffers;

namespace Eshop.Controllers
{
    public class CartsController : Controller
    {
        private readonly EshopContext _context;
        private Product products = new Product();

        public CartsController(EshopContext context)
        {
            _context = context;
        }
        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
        public IActionResult Index()
        {
            ViewBag.loadProductTypes = new SelectList(_context.ProductTypes.Where(x => x.Status), "Id", "Name", products.ProductTypeId);
            var IdUser = HttpContext.Session.GetInt32("Id");
            if (IdUser != null)
            {
                ViewBag.loadCarts = loadCartProduct(IdUser);

                return View(loadCartProduct(IdUser));
            }
            else
            {
                return View(loadCartProduct(IdUser));
            }
        }
        [HttpPost]
        public IActionResult Index([Bind("Id,ProductId,Quantity")] Cart cart, int? page)
        {
            if (_context.Products == null)
                return NotFound();
            var idUser = HttpContext.Session.GetInt32("Id");
            if (idUser != null)
            {
                var checkProducts = _context.Products.FirstOrDefault(x => (x.Id == cart.ProductId && x.Status)).Stock;
                var cartExits = _context.Carts.FirstOrDefault(x => (x.ProductId == cart.ProductId && x.AccountId == idUser));

                cart.AccountId = idUser.Value;
                if (cartExits == null)
                {
                    cart.AccountId = idUser.Value;
                    _context.Carts.Add(cart);
                }
                else
                {
                    if (cartExits.Quantity < checkProducts)
                    {
                        cartExits.Quantity += cart.Quantity;
                        _context.Carts.Update(cartExits);
                    }
                    else
                    {
                        HttpContext.Response.Cookies.Append("error", "Không thành công do số lượng sản phẩm không đủ", new CookieOptions { Expires = DateTimeOffset.Now.AddSeconds(1) });
                        return RedirectToAction("Index", "Products");
                    }
                }
                _context.SaveChanges();
                HttpContext.Session.SetInt32("itemCount", ItemsCart(idUser));
                HttpContext.Response.Cookies.Append("info", "Thêm thành công", new CookieOptions { Expires = DateTimeOffset.Now.AddSeconds(1) });
                if (page != null) return RedirectToAction("Details", "Products", new { id = page });
                else
                    return RedirectToAction("Index", "Products");
            }
            else return RedirectToAction("Login", "Accounts");

        }
        public IActionResult Minus(int? idCart)
        {
            if (_context.Carts == null)
                return NotFound();
            var carts = _context.Carts.FirstOrDefault(x => x.Id == idCart);
            if (carts == null)
                return RedirectToAction("Index");
            if (carts.Quantity > 1)
            {
                carts.Quantity -= 1;
                HttpContext.Response.Cookies.Append("info", "Giảm thành công", new CookieOptions { Expires = DateTimeOffset.Now.AddSeconds(1) });
            }
            else
            {
                _context.Carts.Remove(carts);
                HttpContext.Response.Cookies.Append("info", "Xóa thành công", new CookieOptions { Expires = DateTimeOffset.Now.AddSeconds(1) });
            }
            _context.SaveChanges();
            HttpContext.Session.SetInt32("itemCount", ItemsCart(HttpContext.Session.GetInt32("Id")));
            return RedirectToAction("Index");
        }
        public IActionResult Plus(int? idCart)
        {
            if (_context.Carts == null)
                return NotFound();
            var carts = _context.Carts.Include(p => p.Product).FirstOrDefault(x => x.Id == idCart);
            if (carts == null)
                return RedirectToAction("AddCarts");
            if (carts.Quantity < carts.Product.Stock)
            {
                carts.Quantity += 1;
                _context.SaveChanges();
                HttpContext.Response.Cookies.Append("info", "Tăng thành công", new CookieOptions { Expires = DateTimeOffset.Now.AddSeconds(1) });
            }
            else
            {
                HttpContext.Response.Cookies.Append("info", "Số lượng sản phẩm không đủ", new CookieOptions { Expires = DateTimeOffset.Now.AddSeconds(1) });
            }
            return RedirectToAction("Index", "Carts");
        }
        public IActionResult RemoveCart(int? idCart)
        {
            if (_context.Carts == null)
                return NotFound();
            var carts = _context.Carts.FirstOrDefault(x => x.Id == idCart);
            if (carts == null)
                return NotFound();
            else
            {
                _context.Carts.Remove(carts);
                _context.SaveChanges();
            }
            HttpContext.Session.SetInt32("itemCount", ItemsCart(HttpContext.Session.GetInt32("Id")));
            HttpContext.Response.Cookies.Append("info", "Xóa thành công", new CookieOptions { Expires = DateTimeOffset.Now.AddSeconds(1) });
            return RedirectToAction("Index");
        }
        public IActionResult RemoveAll()
        {
            if (_context.Carts == null)
                return NotFound();
            var IdUser = HttpContext.Session.GetInt32("Id");
            var carts = _context.Carts.Where(x => x.AccountId == IdUser).ToList();
            _context.Carts.RemoveRange(carts);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("itemCount", ItemsCart(HttpContext.Session.GetInt32("Id")));
            HttpContext.Response.Cookies.Append("info", "Xóa thành công", new CookieOptions { Expires = DateTimeOffset.Now.AddSeconds(1) });
            return RedirectToAction("Index");
        }
        [NonAction]
        public IQueryable<Cart> loadCartProduct(int? IdUser)
        {
            var cart = _context.Carts.Include(p => p.Product).Where(x => x.AccountId == IdUser);
            if (cart == null) return null;
            return cart;
        }
        public int ItemsCart(int? Id)
        {

            var carts = _context.Carts.Where(i => i.AccountId == Id);
            if (carts != null)
                return carts.Count();

            return 0;

        }
        public IActionResult Purchase()
        {
            return View();
        }
        public IActionResult PurchaseResult()
        {
            return View();
        }
    }
}
