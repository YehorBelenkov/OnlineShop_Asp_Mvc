using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using OnlineShop.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace OnlineShop.Controllers
{
    public class AdminPanelController : Controller
    {
        public OnlineShopContext db;
        private readonly ILogger<AdminPanelController> _logger;
        public AdminPanelController(OnlineShopContext db, ILogger<AdminPanelController> logger)
        {
            this.db = db;
            _logger = logger;
        }
        //[Authorize(Roles = "Admin")]
        public IActionResult ProductList()
        {
            string UserLoggedIn = Request.Cookies["UserLoggedIn"];
            Users user = null;
            user = JsonSerializer.Deserialize<Users>(UserLoggedIn);
            if (user.Id == 1)
            {
                ViewBag.Products = db.Products.AsNoTracking().ToList();
                ViewBag.Category = db.Categories.AsNoTracking().ToList();
                ViewBag.User = user;
                return View();
            }
            else
            {
                return Redirect("ErrorPage");
            }
        }
        async public Task<IActionResult> Add()
        {
            string UserLoggedIn = Request.Cookies["UserLoggedIn"];
            Users user = null;
            user = JsonSerializer.Deserialize<Users>(UserLoggedIn);
            if (user.Id == 1)
            {
                ViewBag.Categ = db.Categories.AsNoTracking().ToList();
                return View();
            }
            else
            {
                return Redirect("ErrorPage");
            }
        }
        [HttpPost]
        async public Task<IActionResult> Save(string ProdName, double ProdPrice,int ProdAmount,string http, int CategId,string description)
        {
            Products newprod = new Products() { ProdName = ProdName, Price = ProdPrice,HttpPathImg = http,
                Amount = ProdAmount, IdCategorie = CategId, Description = description};
            db.Products.Add(newprod);
            await db.SaveChangesAsync();

            return Redirect("ProductList");
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Product = db.Products.Where(o => o.Id == id).AsNoTracking().FirstOrDefault();
            ViewBag.Categ = db.Categories.AsNoTracking().ToList();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> EditCateg(int id)
        {
            ViewBag.Category = db.Categories.Where(o => o.Id == id).AsNoTracking().FirstOrDefault();
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> deleteCateg(int id)
        {
            Categories categories = db.Categories.Where(o => o.Id == id).AsNoTracking().FirstOrDefault();
            db.Remove(categories);
            await db.SaveChangesAsync();
            return Redirect("ProductList");
        }

        [HttpPost]
        public async Task<IActionResult> SaveCategEdit(int id,string CategName)
        {
            Categories category = db.Categories.Where(o => o.Id == id).AsNoTracking().FirstOrDefault();
            category.CategName = CategName;
            db.Update(category);
            await db.SaveChangesAsync();
            return Redirect("ProductList");
        }
        [HttpPost]
        async public Task<IActionResult> SaveEdits(int Id,string ProdName, double ProdPrice,int ProdAmount,int CategId,string description)
        {
            Products product = db.Products.Where(o => o.Id == Id).AsNoTracking().FirstOrDefault();
            product.ProdName = ProdName;
            product.Price = ProdPrice;
            product.IdCategorie = CategId;
            product.Description = description;
            db.Update(product);
            await db.SaveChangesAsync();

            return Redirect("ProductList");
        }
        public async Task<IActionResult> AddCateg()
        {
            string UserLoggedIn = Request.Cookies["UserLoggedIn"];
            Users user = null;
            user = JsonSerializer.Deserialize<Users>(UserLoggedIn);
            if (user.Id == 1)
            {
                ViewBag.Categ = db.Categories.AsNoTracking().ToList();
                return View();
            }
            return Redirect("/Home/Index");
        }
        [HttpPost]
        public async Task<IActionResult> SaveCateg(string CategName)
        {
            Categories category = new Categories() { CategName = CategName };
            await db.AddAsync(category);
            db.SaveChangesAsync();
            return Redirect("ProductList");
        }
        [HttpPost]
        async public Task<IActionResult> Delete(int id)
        {
            Products delete = db.Products.Where(o => o.Id == id).AsNoTracking().FirstOrDefault();
            db.Remove(delete);
            await db.SaveChangesAsync();
            return Redirect("ProductList");
            //return RedirectToAction("ProductList", "AdminPanel", new {id = 1}, );
        }
    }
}
