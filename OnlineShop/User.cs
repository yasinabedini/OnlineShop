using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop
{
    public class User : Person
    {
        public string Address { get; set; } // ادرس
        public int WalletBalance { get; set; } = 0; // موجودی کیف پول

        public virtual Role Role { get; set; }
        public virtual ICollection<Card> Cards { get; set; }
        public virtual ICollection<Like> Likes { get; set; }
        public virtual ICollection<DisLike> DisLikes { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Cart> AllCart { get; set; }
        public virtual ICollection<UserNotification> MyNotification { get; set; }

        public User() { }
        public User(string Name, string Family, string NationalCode, string Mobile, string Password, Role Role) : base(Name, Family, NationalCode, Mobile, Password)
        {            
            AllCart = new List<Cart>();
            Likes = new List<Like>();
            DisLikes = new List<DisLike>();
            Comments = new List<Comment>();
            this.Role = Role;
        }


        public override string ToString()
        {
            return string.Format($" {Id}    Name: {Name} {Family}\tNationalCode: {NationalCode}\tMobile: {Mobile}      Password: ****      Active : {IsActive}");
        }

    }
}
