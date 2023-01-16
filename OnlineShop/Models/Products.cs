using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Products
    {
        public int Id { get; set; }
        [Required]
        public int Amount { get; set; }
        public string ProdName { get; set; }
        public double Price { get; set; }
        public int IdCategorie { get; set; }
        public string? HttpPathImg { get; set; }
        public string? Description { get; set; }
    }
}
