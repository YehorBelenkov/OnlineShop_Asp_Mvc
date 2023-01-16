using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Models;
using System.Net;
using System.Text.Json;

namespace OnlineShop.Controllers
{
    public class CartController : Controller
    {
        public OnlineShopContext db;
        private readonly ILogger<CartController> _logger;
        public CartController(OnlineShopContext db, ILogger<CartController> logger)
        {
            this.db = db;
            _logger = logger;
        }
        public IActionResult CartList()
        {  
            /*string[] arr = CartCookies.Split(',','[',']').ToArray();*/ /*JsonSerializer.Deserialize<int[]>(str);*/
            string UserLoggedIn = Request.Cookies["UserLoggedIn"];
            Users user = new Users();
            if (UserLoggedIn != null)
            {
                try
                {
                    user = JsonSerializer.Deserialize<Users>(UserLoggedIn);
                    var CartJson = Request.Cookies[$"Cart{user.Id}"];
                    List<Carts> cart = JsonSerializer.Deserialize<List<Carts>>(CartJson);
                    List<Products> products = new List<Products>();
                    foreach (var item in cart)
                    {
                        products.Add(db.Products.Where(o => o.Id == item.ProdId).AsNoTracking().FirstOrDefault());
                    }
                    
                    double subtotal = 0;
                    products = CleanUp(products);
                    foreach(var item in products)
                    {
                        if(item != null)
                        {
                            var tempCart = cart.Where(o => o.ProdId == item.Id).FirstOrDefault();
                            if (tempCart.Amount > 1)
                            {
                                subtotal += item.Price * tempCart.Amount;
                                item.Price *= tempCart.Amount;
                            }
                            else
                            {
                                subtotal += item.Price;
                            }
                            item.Amount = cart.Where(o => o.ProdId == item.Id).FirstOrDefault().Amount;
                        }
                        else if(item == null)
                        {
                            products.Remove(item);
                        }
                    }
                    ViewBag.Count = products.Count();
                    ViewBag.Subtotal = subtotal;
                    ViewBag.Products = products;
                    ViewBag.User = user;
                }
                catch(Exception ex)
                {
                    ViewBag.User = user;
                }
                return View();
            }
            return Redirect("/Home/Index");
        }
        public List<Products> CleanUp(List<Products> prod)
        {
            for(int i = 0; i < prod.Count(); i++)
            {
                if (prod[i] == null)
                {
                    prod.Remove(prod[i]);
                }
            }
            return prod;
        }
        [HttpPost]
        public async Task<IActionResult> delCart(int prodId)
        {
            string UserLoggedIn = Request.Cookies["UserLoggedIn"];
            Users user = new Users();
            if(UserLoggedIn != null)
            {
                user = JsonSerializer.Deserialize<Users>(UserLoggedIn);

                var CartJson = Request.Cookies[$"Cart{user.Id}"];
                List<Carts> cart = JsonSerializer.Deserialize<List<Carts>>(CartJson);

                cart = RemoveCookie(cart, prodId);

                string newCookie = JsonSerializer.Serialize(cart);

                CookieOptions options = new CookieOptions();
                options.Expires = DateTime.Now.AddHours(1);
                options.IsEssential = true;
                options.Path = "/";

                HttpContext.Response.Cookies.Append($"Cart{user.Id}", newCookie, options);
                return Redirect("CartList");
            }
            return Redirect("/Home/Index");
        }
        public List<Carts> RemoveCookie(List<Carts> cart, int prodId)
        {
            for(int i = 0; i < cart.Count(); i++)
            {
                if (cart[i].ProdId == prodId)
                {
                    cart.Remove(cart[i]);
                }
            }
            return cart;
        }
        public IActionResult Checkout()
        {
            string UserLoggedIn = Request.Cookies["UserLoggedIn"];
            Users user = null;

            if (UserLoggedIn != null)
            {
                user = JsonSerializer.Deserialize<Users>(UserLoggedIn);
                string JsonCookie = Request.Cookies[$"Cart{user.Id}"];
                List<Carts> carts = JsonSerializer.Deserialize<List<Carts>>(JsonCookie);
                if (carts != null)
                {
                    string cart = JsonSerializer.Serialize(carts);
                    CookieOptions options = new CookieOptions();
                    options.Expires = DateTime.Now.AddMinutes(10);
                    options.IsEssential = true;
                    options.Path = "/";
                    HttpContext.Response.Cookies.Append("Cart", cart, options);
                    ViewBag.User = user;
                    return View();
                }
            }
            return Redirect("/Home/Index");
        }
        [HttpPost]
        public async Task<IActionResult> Buy(string address, string city,string state,
            int zipcode,string nameOnCard, int cardnumber, string expiredDate, int cvv)
        {
            string UserLoggedIn = Request.Cookies["UserLoggedIn"];
            Users user = JsonSerializer.Deserialize<Users>(UserLoggedIn);

            string Cart = Request.Cookies[$"Cart{user.Id}"];
            List<Carts> carts = JsonSerializer.Deserialize<List<Carts>>(Cart);
            ViewBag.Carts = carts;
            double price = 0;
            List<Products> products = db.Products.AsNoTracking().ToList();
            foreach(var item in carts)
            {
                foreach(var product in products)
                {
                    if(item.ProdId == product.Id)
                    {
                        price += product.Price;
                    }
                } 
            }
            ViewBag.Price = price;
            return View();
        }
        public IActionResult ThankYou()
        {
            return View();
        }
    }
}
