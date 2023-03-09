using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop
{
    abstract public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } //نام
        public string Family { get; set; }//نام خانوادگی
        public string NationalCode { get; set; } // کد ملی
        public string Mobile { get; set; } // شماره موبایل
        public string Password { get; set; } // پسورد
        public bool IsActive { get; set; } // (فعال / غیرفعال)
        public DateTime RegisterDate { get; set; } // تاریخ ثبت نام
        public DateTime? LastLoginDate { get; set; } // اخرین بازدید کاربر یا ادمین از پنل کاربری



        public Person() { }
        public Person(string Name, string Family, string NationalCode, string Mobile, string Password)
        {
            this.Name = Name;
            this.Family = Family;
            this.NationalCode = NationalCode;
            this.Mobile = Mobile;
            this.Password = Password;
            IsActive = true;
            RegisterDate = DateTime.Now;
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
