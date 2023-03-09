using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop
{
    public class Card
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } // شماره کارت
        public string OwnerName { get; set; } // نام صاحب حساب
        public string Cvv2 { get; set; } // cvv2
        public string ExpirationDate { get; set; } // تاریخ انقضا
        public bool IsActive { get; set; }
        public DateTime RegisterDate { get; set; } // تاریخ ثبت نام
        public string SecondPassword { get; set; } // رمز دوم


        public virtual User Owner { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }

        public Card() { }
        public Card(string CardNumber, string Cvv2,string ExpirationDate, string SecondPassword, User Owner)
        {
            this.CardNumber = CardNumber;
            this.Cvv2 = Cvv2;            
            this.SecondPassword = SecondPassword;
            this.Owner = Owner;
            this.ExpirationDate = ExpirationDate;
            OwnerName = Owner.Name;
            IsActive = true;          
            RegisterDate = DateTime.Now;            
            Payments = new List<Payment>();
        }

        public override string ToString()
        {
            return string.Format($"Id : {Id}    Owner : {Owner.Name} {Owner.Family}\tCard Number : {CardNumber}    Cvv2 : {Cvv2}\tExpiration : {ExpirationDate}    Password : ******");
        }
    }
}
