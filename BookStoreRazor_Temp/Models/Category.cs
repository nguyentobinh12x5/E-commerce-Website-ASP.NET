using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BookStoreRazor_Temp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30, ErrorMessage = "Name must be under 30 Letters")]
        [DisplayName("Category Name")]
        public string Name { get; set; }
        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Name must be under 30 Letters")]
        public int DisplayOrder { get; set; }
    }
}
