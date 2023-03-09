using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop
{
    public class Order
    {
        public int Id { get; set; }
        public int NumberOfProduct { get; set; } // تعداد کالا
        public decimal SumPrice { get; set; } // قیمت نهایی سفارش

        public virtual Product Product { get; set; }
        public virtual Cart Cart { get; set; }


        public Order() { }
        public Order(int Id, int NumberOfProduct, Product Product, Cart Cart)
        {
            this.Id = Id;
            this.NumberOfProduct = NumberOfProduct;
            this.Product = Product;
            this.Cart = Cart;
            this.SumPrice = Product.Price * NumberOfProduct;
        }
        public Order(int NumberOfProduct, Product Product, Cart Cart)
        {            
            this.NumberOfProduct = NumberOfProduct;
            this.Product = Product;
            this.Cart = Cart;
            this.SumPrice = Product.Price * NumberOfProduct;
        }
      
        public override string ToString()
        {
            return string.Format($"Id : {Product.Id}\tProduct Model : {Product.ModelName}({Product.BrandName})\tPrice : {Product.Price}\tNumber : {NumberOfProduct}\tSum Price : {SumPrice}");
        }
    }
}
