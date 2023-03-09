using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop
{
    public class Admin : Person
    {
     
        public string Department { get; set; } // دپارتمان

        public virtual Role Role { get; set; } 
        public virtual ICollection<Product> RegisterProduct { get; set; }
        public virtual ICollection<Product> EditProduct { get; set; }
        public virtual ICollection<DisCount> RegistersDiscount { get; set; }
        public virtual ICollection<Comment> CommentsApproved { get; set; }
        public virtual ICollection<UserNotification> RegisterNotifications { get; set; }
        public virtual ICollection<AdminNotification> MyNotifications { get; set; }


        public Admin() { }
        public Admin(string Name, string Family, string NationalCode, string Mobile, string Password, string Department,Role Role) : base(Name, Family, NationalCode, Mobile, Password)
        {
            this.Department = Department;
            this.Role = Role;
        }

        public override string ToString()
        {
            return string.Format($" {Id}    Name: {Name} {Family}\tNationalCode: {NationalCode}\tMobile: {Mobile}      Password: ****      \n\n      Department : {Department}\tLast Login : {LastLoginDate}");
        }
    }
}
