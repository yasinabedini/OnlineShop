using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop
{
    public class Product
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } // نام دسته بندی
        public string BrandName { get; set; } // نام برند
        public string ModelName { get; set; } // نام مدل محصول
        public string Description { get; set; } // توضیحات
        public decimal Price { get; set; } // قیمت 
        public int Inventory { get; set; } // موجودی
        public int SalesNumber { get; set; } = 0; // تعداد فروش محصول
        public bool IsActive { get; set; } // (فعال/غیرفعال)
        public bool InventoryStatus { get; set; } // وضعیت موحودی
        public DateTime RegisterDate { get; set; } // تاریخ ثبت نام
        public DateTime EditDate { get; set; } // اخرین تاریخ ویرایش


        public virtual ICollection<DisCount> DisCount { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual Admin Registrar { get; set; }
        public virtual Admin Editor { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<DisLike> DisLikes { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }




        public Product() { }
        public Product(string CategoryName, string BrandName, string ModelName, decimal Price, int Inventory, Admin Registrar, string Description)
        {
            this.CategoryName = CategoryName;
            this.BrandName = BrandName;
            this.ModelName = ModelName;
            this.Price = Price;
            this.Inventory = Inventory;
            this.Registrar = Registrar;
            this.Description = Description;
            IsActive = true;
            InventoryStatus = true;
            if (Inventory == 0)
            {
                InventoryStatus = false;
            }
            RegisterDate = DateTime.Now;
            EditDate = RegisterDate;
        }

        public decimal CalculatePrice()
        {
            if (DisCount != null && DisCount.Any(t => t.IsActive == true))
            {
                return Price - Price * (decimal)DisCount.First(t => t.IsActive == true).Precent;
            }
            else
            {
                return Price = Price;
            }
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
        public override string ToString()
        {
            return string.Format($" Id:{Id}     Brand:{BrandName}\t\tModel:{ModelName}\t\t\tPrice:{Price}\t\tInventory:{Inventory}");
        }
    }
}
