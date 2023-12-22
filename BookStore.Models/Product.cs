using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookStore.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }


        [Required]
        [DisplayName("Price")]
        [Range(1, 10000000)]
        public double Price { get; set; }

        [Required]
        [DisplayName("List Discount")]
        [Range(1, 1000000)]
        public double PriceDiscount { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [DisplayName("Image URL1")]
        public string ImageUrl1 { get; set; }
        [DisplayName("Image URL2")]
        public string ImageUrl2 { get; set; }
        [DisplayName("Image URL3")]
        public string ImageUrl3 { get; set; }
        [DisplayName("Image URL4")]
        public string ImageUrl4 { get; set; }
        public string DescriptionDetail { get; set; }
    }
}
