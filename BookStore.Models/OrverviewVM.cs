using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class OverviewVM
    {
        //được sử dụng để lưu trữ danh sách các đối tượng ShoppingCart
        //IEnumerable là một interface cho phép bạn lặp qua các phần tử trong một tập hợp.
        //public IEnumerable<OrderHeader> OrderHeaderList { get; set; }
        // Do ben trong homecontroller trả về Tolist nên trong class này cần sử dụng IEnumerable
        public IEnumerable<OrderHeader> OrderHeaderList { get; set; }
        public IEnumerable<Product> ProductList { get; set; }
        public decimal TotalOrderAmount { get; set; }

    }
}
