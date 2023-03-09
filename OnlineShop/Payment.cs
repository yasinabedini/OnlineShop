using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop
{
    public class Payment
    {
        public int Id { get; set; }
        public DateTime PayDate { get; set; } // تاریخ پرداخت
        public decimal PayPrice { get; set; } // قیمت پرداخت شده
        public string PaymentMethod { get; set; } // روش پرداخت

        public virtual Cart Cart { get; set; }
        public virtual Card Card { get; set; }


        public Payment() { }
        public Payment(Cart Cart)
        {           
            this.Cart = Cart;
            PaymentMethod = Cart.PaymentMethod;
            PayDate = Cart.DeliveryDate;
            PayPrice = Cart.CalculateEndPrice();
        }
        public Payment(Cart Cart, Card Card)
        {
            this.Cart = Cart;
            this.Card = Card;
            PaymentMethod = Cart.PaymentMethod;
            PayDate = DateTime.Now;
            PayPrice = Cart.CalculateEndPrice();
        }

        public override string ToString()
        {
            return string.Format($" Id : {Id}     PayDate : {PayDate.Year}/{PayDate.Month}/{PayDate.Day}\tPay Price : {PayPrice}\tPayment Method : {PaymentMethod}");
        }
    }
}
