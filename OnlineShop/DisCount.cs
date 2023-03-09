using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop
{
    public class DisCount
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public float Precent { get; set; } // درصد تخفیف
        public DateTime RegisterDate { get; set; } // تاریخ ثبت نام
        public int HourDuration { get; set; } // مدت زمان تخفیف
        public DateTime ExpirationDate { get; set; } // تاریخ انقضا تخفیف
        public string ProductName { get; set; } // نام محصول مربوط به تخفیف


        public virtual Admin Registrar { get; set; }
        public virtual Product Product { get; set; }



        public DisCount() { }
        public DisCount(float Precent, int HourDuration, Product Product, Admin Registrar)
        {
            IsActive = true;
            this.Precent = Precent;
            this.HourDuration = HourDuration;
            this.Product = Product;
            this.Registrar = Registrar;
            RegisterDate = DateTime.Now;
            ProductName = Product.ModelName;
            ExpirationDate = RegisterDate.AddHours(HourDuration);
        }

        public override string ToString()
        {
            return string.Format($" {Id}    Product: {ProductName}\t\t\tPrecent : {Precent.ToString()}\t\t HourDuration: {HourDuration}\n\n       ExpirationDate: {ExpirationDate}      Active : {IsActive}");
        }

        public void ChangeAccess()
        {
            if (IsActive == true)
            {
                IsActive = false;
            }
            else
            {
                IsActive = true;
            }
        }
    }
}
