using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop
{
    public class ViewPoint
    {
        public int Id { get; set; }

        public virtual Product Product { get; set; }
        public virtual User User { get; set; }
    }


    public class Like : ViewPoint
    {
        public Like() { }
        public Like(User User, Product Product)
        {
            this.User = User;
            this.Product = Product;
        }
    }


    public class DisLike : ViewPoint
    {
        public DisLike() { }
        public DisLike(User User, Product Product)
        {
            this.User = User;
            this.Product = Product;
        }
    }


    public class Comment : ViewPoint
    {
        public string Text { get; set; } // متن کامنت 
        public bool Confrimation { get; set; } // تایید شده یا نه ؟ 
        public int ReportCount { get; set; } = 0; // تعداد گزارش ها 
        public string ProductName { get; set; } // نام محصول مربوط به کامنت
        public bool IsActive { get; set; }

        public virtual Admin Seconder { get; set; } //تایید کننده

        public Comment() { }
        public Comment(User User, Product Product, string Text)
        {
            this.User = User;
            this.Product = Product;
            this.Text = Text;
            Confrimation = false;
            IsActive = true;
            ProductName = $"{Product.CategoryName}/{Product.BrandName}/{Product.ModelName}";
        }

        public void ChangeAccess() // برای تغییر سطح دسترسی
        {
            if (IsActive ==true)
            {
                IsActive = false;
            }
            else
            {
                IsActive = true;
            }
        }

        public override string ToString()
        {
            return string.Format($" Id : {Id}\t\t\tProduct : {ProductName}\n\n Text : {Text}");
        }
    }
}
