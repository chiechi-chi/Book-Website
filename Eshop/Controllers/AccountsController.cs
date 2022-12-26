
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Eshop.Data;
using Eshop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using System.Text;
using System.Security.Principal;
using System.Net;
using XAct.Users;

namespace Eshop.Controllers
{
    public class AccountsController : Controller
    {
        private readonly EshopContext _context;
        private readonly IWebHostEnvironment _environment;
        Product products = new Product();

        public AccountsController(EshopContext context, IWebHostEnvironment environment)
        {
            _environment = environment;
            _context = context;
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Login");
            }
            var accounts = _context.Accounts.FirstOrDefault(x => x.Id == id);
            if (accounts == null)
                return NotFound();
            return View(accounts);
        }

        [HttpPost]
        public IActionResult Edit(int? id, [Bind("Id,Username,FullName,Email,Address,Phone,Password")] Account account)
        {

            if (account.Username == null || account.Email == null || account.Address == null || account.Phone == null || account.Password == null || account.FullName == null)
            {
                ViewBag.error = "Vui long nhap day du";
                return View();
            }
            account.Avatar = account.Username + ".jpg";
            _context.Accounts.Update(account);
            _context.SaveChanges();
            ViewBag.succes = "Thay doi thanh cong";
            return View();
        }
        private bool userNameExists(string username)
        {

            var user = _context.Accounts.FirstOrDefault(e => (e.Username.Contains(username)));
            if (user != null) return true;
            return false;
        }
        private bool emailExists(string email)
        {
            var user = _context.Accounts.FirstOrDefault(e => (e.Email.Contains(email)));
            if (user != null) return true;
            return false;
        }
        public IActionResult Login()
        {

            if (@HttpContext.Session.GetInt32("Id") == null)
            {
                ViewBag.loadProductTypes = new SelectList(_context.ProductTypes.Where(x => x.Status), "Id", "Name", products.ProductTypeId);
                return View();
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public IActionResult Login([Bind("Username,Password")] Account account)
        {
            var _account = _context.Accounts.SingleOrDefault(x => x.Username.Trim() == account.Username);
            var user = _context.Accounts.FirstOrDefault(x => x.Username == account.Username && account.Password == x.Password);

            if (_account.IsAdmin == true)
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            if (user != null)
            {
                CartsController carts = new CartsController(_context);
                HttpContext.Session.SetString("userName", user.Username);
                HttpContext.Session.SetInt32("Id", user.Id);
                HttpContext.Session.SetInt32("itemCount", carts.ItemsCart(user.Id));
                HttpContext.Session.SetInt32("CheckIsAdmin", user.IsAdmin ? 1 : 0);
                if (user.Avatar != null)
                {
                    HttpContext.Session.SetString("avatar", user.Avatar);
                }
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.loadProductTypes = new SelectList(_context.ProductTypes.Where(x => x.Status), "Id", "Name", products.ProductTypeId);
                ViewBag.error = "Tài khoản hoặc mật khẩu sai !!! Hãy kiểm tra lại";
                return View();
            }

        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Id");
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Register()
        {
            ViewBag.loadProductTypes = new SelectList(_context.ProductTypes.Where(x => x.Status), "Id", "Name", products.ProductTypeId);
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string ConfirmPassword, [Bind("Id, Username, Password, Email, FullName, Address, Phone,Avatar")] Account account)
        {
            if (userNameExists(account.Username))
            {
                ViewBag.loadProductTypes = new SelectList(_context.ProductTypes.Where(x => x.Status), "Id", "Name", products.ProductTypeId);
                ViewBag.errorUserName = "UserName đã tồn tại";
                return View();
            }
            else if (emailExists(account.Email))
            {
                ViewBag.loadProductTypes = new SelectList(_context.ProductTypes.Where(x => x.Status), "Id", "Name", products.ProductTypeId);
                ViewBag.errorEmail = "Email đã tồn tại";
                return View();
            }
            else
            {
                if (account.Password == ConfirmPassword)
                {
                    account.IsAdmin = false;
                    ViewBag.success = "Đăng kí thành công";
                    _context.Accounts.Add(account);
                    _context.SaveChanges();
                }
                else
                {
                    ViewBag.loadProductTypes = new SelectList(_context.ProductTypes.Where(x => x.Status), "Id", "Name", products.ProductTypeId);
                    ViewBag.error = "Nhập lại mật khẩu không Đúng";
                    return View();
                }
            }

            return RedirectToAction("Login");
        }

        public IActionResult UserDetails()
        {
            CartsController carts = new CartsController(_context);
            var IdUser = HttpContext.Session.GetInt32("Id");
            if (IdUser != null)
            {
                ViewBag.loadCarts = carts.loadCartProduct(IdUser);
                ViewBag.Bought = _context.Invoices.Where(x => x.AccountId == IdUser).Sum(x => x.Total);
            }
            else return NotFound();
            ViewBag.loadProductTypes = new SelectList(_context.ProductTypes.Where(x => x.Status), "Id", "Name", products.ProductTypeId);
            var user = _context.Accounts.FirstOrDefault(x => x.Id == IdUser);
            return View(user);
        }
        [HttpPost]
        public IActionResult UserDetails(int id, [Bind("Id,Username,Password,Email,Phone,Address,FullName,Avatar,IsAdmin,Status")] Account account)
        {
            if (id != account.Id)
            {
                return NotFound();
            }
            CartsController carts = new CartsController(_context);
            var IdUser = HttpContext.Session.GetInt32("Id");
            if (IdUser != null)
            {
                ViewBag.loadCarts = carts.loadCartProduct(IdUser);
            }
            else return NotFound();
            ViewBag.loadProductTypes = new SelectList(_context.ProductTypes.Where(x => x.Status), "Id", "Name", products.ProductTypeId);
            _context.Accounts.Update(account);
            _context.SaveChanges();
            return View(account);
        }
        public IActionResult Information()
        {
            return View();
        }
        public IActionResult Orders()
        {
            return View();
        }
        private static string GetMD5(string str)
        {
#pragma warning disable SYSLIB0021 // Type or member is obsolete
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
#pragma warning restore SYSLIB0021 // Type or member is obsolete
            byte[] bHash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

            StringBuilder sbHash = new StringBuilder();

            foreach (byte b in bHash)
            {

                sbHash.Append(String.Format("{0:x2}", b));

            }

            return sbHash.ToString();

        }

    }
}
