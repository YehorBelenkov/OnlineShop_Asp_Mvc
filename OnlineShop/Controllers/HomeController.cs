using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OnlineShop.Models;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace OnlineShop.Controllers
{
    public class HomeController : Controller
    {
        public OnlineShopContext db;
        private readonly ILogger<HomeController> _logger;
        public HomeController(OnlineShopContext db, ILogger<HomeController> logger)
        {
            this.db = db;
            _logger = logger;
        }
        public IActionResult logout()
        {
            CookieOptions options = new CookieOptions();
            options.Expires = DateTime.Now;
            options.IsEssential = true;
            options.Path = "/";

            HttpContext.Response.Cookies.Append("UserLoggedIn", "", options);

            return Redirect("Index");
        }
        public async Task<IActionResult> ShowProduct(int Id)
        {
            string UserLoggedIn = Request.Cookies["UserLoggedIn"];
            Users user = null;
            if (UserLoggedIn != null)
            {
                user = new Users();
                user = JsonSerializer.Deserialize<Users>(UserLoggedIn);

                Products prod = db.Products.Where(o => o.Id == Id).AsNoTracking().FirstOrDefault();
                ViewBag.Cut = (int)(prod.Price / 0.70);
                ViewBag.Product = prod;
                ViewBag.User = user;
                return View();
            }

            return Redirect("LoginRegister");
        }

        public IActionResult Index()
        {
            string UserLoggedIn = Request.Cookies["UserLoggedIn"];
            Users user = null;
            ViewBag.Id = "";
            if (UserLoggedIn != null && UserLoggedIn != "")
            {
                user = new Users();
                user = JsonSerializer.Deserialize<Users>(UserLoggedIn);
                ViewBag.User = user;
                ViewBag.Id = user.Id;
            }
            if (db.Users.AsNoTracking().Count() == 0)
            {
                db.Roles.Add(new Roles() { Name = "Admin" });
                db.Roles.Add(new Roles() { Name = "User" });
                db.Users.Add(new Users() { FullName = "Admin", Email = "admin@gmail.com", Password = "12345", PhoneNumber = "0000", RoleId = 1 });
                db.SaveChanges();
            }
            int result = 0;
            if (db.Products.Count() != null)
            {
                result = db.Products.Count() / 4;
                if (result == 0)
                {
                    result = 1;
                }
            }
            Products[,] arr = new Products[result, 8];
            var products = db.Products.ToArray();
            var prodPrice = db.Products.Select(o => (int)(o.Price / 0.70)).ToArray();
            int count1d = 0;
            int count2d = 0;
            int[,] amountOfPages = new int[result, 8];
            foreach (var item in products)
            {
                if (count2d != 8)
                {
                    arr[count1d, count2d] = item;
                    count2d++;
                }
                else
                {
                    count2d = 0;
                    count1d++;
                    arr[count1d, count2d] = item;
                }
            }
            count1d = 0;
            count2d = 0;
            foreach (var item in prodPrice)
            {
                if (count2d != 8)
                {
                    amountOfPages[count1d, count2d] = item;
                    count2d++;
                }
                else
                {
                    count2d = 0;
                    count1d++;
                    amountOfPages[count1d, count2d] = item;
                }
            }
            ViewBag.prodamount = db.Products.Count();
            ViewBag.products = arr;
            ViewBag.cutprice = amountOfPages;
            ViewBag.PageAmount = result;
            ViewBag.Count = 0;
            ViewBag.title = "Testing";
            var categories = db.Categories.ToList();
            ViewBag.categ = categories;
            return View();
        }
        public IActionResult LoginRegister()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            Users user = db.Users.Where(o => o.Email == Email && Password == Password).AsNoTracking().FirstOrDefault();
            if (user != null)
            {
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(45);
                options.IsEssential = true;
                options.Path = "/";

                string str = JsonSerializer.Serialize(user);

                HttpContext.Response.Cookies.Append("UserLoggedIn", str, options);
                return Redirect("Index");
                //return RedirectToAction("Index", "Main");
            }
            else
            {
                return View("LoginRegister");
            }
        }
        //[HttpPost]
        //public async Task<IActionResult> Login(string Email, string Password)
        //{
        //    Users user = db.Users.Where(o => o.Email == Email && Password == Password).AsNoTracking().FirstOrDefault();
        //    if (user != null)
        //    {
        //        CookieOptions options = new CookieOptions();
        //        options.Expires = DateTime.Now.AddMinutes(45);
        //        options.IsEssential = true;
        //        options.Path = "/";

        //        string str = JsonSerializer.Serialize(user);

        //        //HttpContext.Response.Cookies.Append("UserLoggedIn", str, options);

        //        var role = await db.Roles.FindAsync(user.RoleId);
        //        var claims = new List<Claim>
        //        {
        //            new Claim(ClaimsIdentity.DefaultNameClaimType, user.FullName),
        //            new Claim(ClaimsIdentity.DefaultRoleClaimType,role!.Name)
        //        };
        //        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        //        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        //        await HttpContext.SignInAsync(claimsPrincipal);
        //        return RedirectToAction("Index","Home");
        //        //return RedirectToAction("Index", "Main");
        //    }
        //    else
        //    {
        //        return View("LoginRegister");
        //    }
        //}
        //[HttpPost]
        //public async Task<IActionResult> Register(string FullName,string Email, double PhoneNumber, string Password, string ConfPassword)
        //{

        //}
        [HttpPost]
        public IActionResult Authentication(string FullName, string Email, double PhoneNumber, string Password, string ConfPassword)
        {
            if (ConfPassword == Password)
            {
                string key = "";
                Random rand = new Random();
                key = $"{key}{rand.Next(100000, 999999)}";
                Users tempUser = new Users() { FullName = FullName, Email = Email, Password = Password, PhoneNumber = PhoneNumber.ToString(), RoleId = 2 };
                string str = JsonSerializer.Serialize(tempUser);

                var fromAddress = new MailAddress("robottester51@gmail.com", "Online Shop");
                var toAddress = new MailAddress(tempUser.Email, tempUser.FullName);
                const string fromPassword = "iokkbczukalzztuv";
                const string subject = "Your Code";
                string body = $"Hello! This is the Authentication code for your registration account in Online Shop!\nHere is you're code: [{key}]";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                    Timeout = 20000
                };
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }

                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(20);
                options.IsEssential = true;
                options.Path = "/";

                HttpContext.Response.Cookies.Append("UserTemp", str, options);
                HttpContext.Response.Cookies.Append("UserKey", key, options);
                return View();
            }
            else
            {
                return Redirect("LoginRegister");
            }
        }
        [HttpPost]
        public async Task<IActionResult> checking(double code)
        {
            string key = Request.Cookies["UserKey"];
            if (key == code.ToString())
            {
                string user = Request.Cookies["UserTemp"];
                Users newUser = JsonSerializer.Deserialize<Users>(user);
                await db.Users.AddAsync(newUser);
                await db.SaveChangesAsync();

                newUser = db.Users.Where(o => o.Email == newUser.Email && o.Password == newUser.Password).AsNoTracking().FirstOrDefault();

                user = JsonSerializer.Serialize(newUser);
                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddMinutes(45);
                options.IsEssential = true;
                options.Path = "/";

                HttpContext.Response.Cookies.Append("UserLoggedIn", user, options);
                //return RedirectToAction("Index");
                //ViewBag.Id = newUser.Id;
                //ViewBag.Name = newUser.FullName;
                return Redirect("Index");
                //return RedirectToAction("Home", "Index", new { user });
            }
            else
            {
                return Redirect("Authentication");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(int prodId, int Amount)
        {
            string UserLoggedIn = Request.Cookies["UserLoggedIn"];
            Users user = null;
            if (UserLoggedIn != null)
            {
                user = JsonSerializer.Deserialize<Users>(UserLoggedIn);

                string cartCookie = Request.Cookies[$"Cart{user.Id}"];
                if(cartCookie == null)
                {
                    List<Carts> cartList = new List<Carts>();
                    Products prod = db.Products.Where(o => o.Id == prodId).AsNoTracking().FirstOrDefault();
                    Carts cart = new Carts() { ProdId = prod.Id, Amount = Amount};

                    cartList.Add(cart);
                    string newCookie = JsonSerializer.Serialize(cartList);

                    CookieOptions options = new CookieOptions();
                    options.Expires = DateTime.Now.AddHours(1);
                    options.IsEssential = true;
                    options.Path = "/";

                    HttpContext.Response.Cookies.Append($"Cart{user.Id}", newCookie, options);
                }
                else
                {
                    List<Carts> cartList = JsonSerializer.Deserialize<List<Carts>>(cartCookie);
                    Products prod = db.Products.Where(o => o.Id == prodId).AsNoTracking().FirstOrDefault();
                    
                    Carts cart = new Carts() { ProdId = prod.Id, Amount = Amount };
                    cartList.Add(cart);
                    string newCookie = JsonSerializer.Serialize(cartList);

                    CookieOptions options = new CookieOptions();
                    options.Expires = DateTime.Now.AddHours(1);
                    options.IsEssential = true;
                    options.Path = "/";

                    HttpContext.Response.Cookies.Append($"Cart{user.Id}", newCookie, options);
                }
                return Redirect("Index");
            }
            return Redirect("LoginRegister");
        }
        public IActionResult Go()
        {
            return RedirectToAction("ProductList", "AdminPanel");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}