using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } 


        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Admin> Admins { get; set; }


        public Role() { }
        public Role(string Name)
        {
            this.Name = Name;
        }
    }
}
