using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Category Name")]
        [MaxLength(30, ErrorMessage = "Tên bắt buộc phải dưới 30 ký tự")]
        public string Name { get; set; }

        [DisplayName("Display Order")]
        [Range(1, 100, ErrorMessage = "Thứ tự phải dưới 100")]
        public int DisplayOrder { get; set; }
    }
}
