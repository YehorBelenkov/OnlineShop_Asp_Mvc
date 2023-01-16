using System.ComponentModel.DataAnnotations;

namespace OnlineShop.Models
{
    public class Categories
    {
        public int Id { get; set; }
        [Required]
        public string CategName { get; set; }
        //public ICollection<Products> Products { get; set; } = null!;
    }
}
