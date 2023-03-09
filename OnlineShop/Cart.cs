using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop
{
    public class Cart
    {
        public int Id { get; set; }
        public DateTime RegisterDate { get; set; } // تاریخ ثبت نام
        public bool IsPay { get; set; }// وضعیت پرداخت
        public bool IsActive { get; set; }// (فعال/غیرفعال)
        public string PaymentMethod { get; set; } //روش پرداخت
        public decimal EndPrice { get; set; } //قیمت نهایی
        public DateTime DeliveryDate { get; set; } //تاریخ رسیدن محصول
        public bool IsReceive { get; set; } // وضعیت رسیدن کالا به دست مشتری
        public int OrderCount { get; set; } // تعداد سفارش 
        public string UserName { get; set; } // نام کاربر مربوط به این سبد خرید


        public virtual ICollection<Order> Orders { get; set; }
        public virtual Payment Payment { get; set; }
        public virtual User User { get; set; }


        public Cart() { }
        public Cart(User User)
        {
            this.User = User;
            IsPay = false;
            RegisterDate = DateTime.Now;
            DeliveryDate = DateTime.Now.AddDays(3);
            IsReceive = false;
            IsActive = true;
            UserName = User.Name + " " + User.Family;            
        }


        public decimal CalculateEndPrice()
        {
            using (OnlineShopDbContext dbEndPrice = new OnlineShopDbContext("onlineshopdbcontext"))
            {
                if (dbEndPrice.Carts.First(t=>t.Id==Id).Orders==null)
                {
                    return 0;
                }
                return dbEndPrice.Orders.Where(t => t.Cart.Id == Id).Sum(t => t.SumPrice);
            }          
        }

        public void ReceiveYourCart()
        {
            using (OnlineShopDbContext dbReceiveCart = new OnlineShopDbContext("onlineshopdbcontext"))
            {
                IsReceive = true;
                //Send Notification For User
                dbReceiveCart.UserNotifications.Add(new UserNotification("\n\tYour Cart Recive To You...\n\n\tThanks For Buy Us.", "Receive Your Cart", dbReceiveCart.Admins.First(t => t.Id == 1), dbReceiveCart.Users.First(t => t.Id == User.Id)));
                dbReceiveCart.SaveChanges();
            }
        }

        public override string ToString()
        {
            return string.Format($"Id {Id} \t UserName : {UserName}\t End Price : {CalculateEndPrice()} \tProducts : {OrderCount}\tPayment Method : {PaymentMethod}\tPay : {IsPay}");
        }
    }
}
