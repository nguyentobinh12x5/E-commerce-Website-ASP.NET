using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class ShoppingCartVM
    {
        //được sử dụng để lưu trữ danh sách các đối tượng ShoppingCart
        //IEnumerable là một interface cho phép bạn lặp qua các phần tử trong một tập hợp.
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
