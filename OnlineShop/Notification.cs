using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop
{
    abstract public class Notification
    {
        public int Id { get; set; }
        public string Title { get; set; } // موضوع اعلان
        public string Text { get; set; } // متن اعلان
        public bool IsRead { get; set; } // وضعیت خوانده شده یا نشدن
        public bool IsActive { get; set; }
        public DateTime RegisterDate { get; set; } // تاریخ ثبت نام


        public Notification() { }
        public Notification(string Text, string Title)
        {
            this.Title = Title;
            IsActive = true;
            IsRead = false;
            this.Text = Text;
            RegisterDate = DateTime.Now;
        }
    }

    public class AdminNotification : Notification
    {
        public virtual Admin Admin { get; set; }

        public AdminNotification() { }
        public AdminNotification(string Text, string Title) : base(Text, Title)
        {            
        }
        public AdminNotification(string Text, string Title, Admin Admin) : base(Text, Title)
        {
            this.Admin = Admin;
        }

        public override string ToString()
        {
            return string.Format($" Id : {Id}\t\tTitle : {Title}\t\tFor Admin : {Admin.Name} {Admin.Family}");
        }
    }

    public class UserNotification : Notification
    {
        public string UserName { get; set; }
        public virtual Admin Registrar { get; set; }
        public virtual User User { get; set; }



        public UserNotification() { }
        public UserNotification(string Text, string Title, Admin Registrar, User User) : base(Text, Title)
        {
            this.User = User;
            this.Registrar = Registrar;
            UserName = User.Name + " " + User.Family;
        }

        public override string ToString()
        {
            return string.Format($" {Id}     Title : {Title}\t\tUser : {UserName}\n\n        Text : {Text}");
        }
    }
}
