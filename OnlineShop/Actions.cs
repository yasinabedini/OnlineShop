using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json;

namespace OnlineShop
{
    static class Actions 
    {
        /// <summary>
        /// Just Print DateTime
        /// </summary>
        public static Action printDatetime = delegate
        {
            Console.Clear();
            Console.SetWindowSize(130, 35);
            PersianCalendar pc = new PersianCalendar();
            Console.WriteLine(" ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine($"   {pc.GetYear(DateTime.Now)}/{pc.GetMonth(DateTime.Now)}/{pc.GetDayOfMonth(DateTime.Now)}   {pc.GetHour(DateTime.Now)}:{pc.GetMinute(DateTime.Now)} \t\t\t\t\t<<Onlile Shop>>                                           Please Login");
            Console.WriteLine(" ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");

        };

        /// <summary>
        /// PrintDateTime And Admin/User's Name
        /// </summary>
        public static Action<dynamic, string> PrintDatetimeInPanel = delegate (dynamic user, string title)
         {
             Console.Clear();
             PersianCalendar pc = new PersianCalendar();
             Console.WriteLine(" ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
             Console.WriteLine($"   {pc.GetYear(DateTime.Now)}/{pc.GetMonth(DateTime.Now)}/{pc.GetDayOfMonth(DateTime.Now)}   {pc.GetHour(DateTime.Now)}:{pc.GetMinute(DateTime.Now)} \t\t\t\t\t<<Onlile Shop>>                                       {user.Name} {user.Family}");
             Console.WriteLine(" ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
             for (int i = 0; i < title.Length + 3; i++)
             {
                 Console.Write("-");
             }
             Console.WriteLine($"\n {title} |");
             for (int i = 0; i < title.Length + 3; i++)
             {
                 Console.Write("-");
             }
             Console.WriteLine("\n");

         };

        /// <summary>
        /// Show All Product With Group Name
        /// </summary>
        public static Action showAllProduct = delegate
        {
            using (OnlineShopDbContext dbShowAllProduct = new OnlineShopDbContext("onlineshopdbcontext"))
            {
                var Products = dbShowAllProduct.Products;
                foreach (var group in Products.GroupBy(t => t.CategoryName))
                {
                    Console.WriteLine($"\n|Category : {group.Key}|\n");
                    foreach (Product product in group)
                    {
                        if (product.InventoryStatus == false)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(product);
                            Console.ResetColor();
                            Console.WriteLine("\n________________________________________________________________________________________________________________________\n");
                        }
                        else
                        {
                            Console.WriteLine(product);
                            Console.WriteLine("\n________________________________________________________________________________________________________________________\n");
                        }
                    }
                }
            }
        };

        /// <summary>
        /// Show All User
        /// </summary>
        public static Action showAllUser = delegate
        {
            using (OnlineShopDbContext dbShowAllUser = new OnlineShopDbContext("onlineshopdbcontext"))
            {
                Console.WriteLine("\n User List : \n");
                foreach (User user in dbShowAllUser.Users.ToList())
                {
                    Console.WriteLine(user);
                    Console.WriteLine("____________________________________________________________________________________________________");
                }
            }
        };

        /// <summary>
        /// Show All Discount In Database
        /// </summary>
        public static Action<bool> showAllDiscount = delegate (bool justactive)
        {
            using (OnlineShopDbContext dbShowAlldiscount = new OnlineShopDbContext("onlineshopdbcontext"))
            {
                if (justactive == true)
                {
                    foreach (DisCount disCount in dbShowAlldiscount.DisCounts.Where(t => t.IsActive == true))
                    {
                        Console.WriteLine(disCount);
                        Console.WriteLine("_____________________________________________________________________________________________________________________________");
                    }
                }
                else
                {
                    foreach (DisCount disCount in dbShowAlldiscount.DisCounts)
                    {
                        Console.WriteLine(disCount);
                        Console.WriteLine("_____________________________________________________________________________________________________________________________");
                    }
                }
            }
        };

        /// <summary>
        /// Show All Admin
        /// </summary>
        public static Action showAllAdmin = delegate
        {
            using (OnlineShopDbContext dbShowAllAdmin = new OnlineShopDbContext("onlineshopdbcontext"))
            {
                Console.WriteLine("\n Admin List :\n");
                foreach (Admin admin in dbShowAllAdmin.Admins)
                {
                    Console.WriteLine(admin);
                    Console.WriteLine("_____________________________________________________________________________________________________________________________");
                }
            }
        };

        /// <summary>
        /// If User Or Admin Enter String or .... Instead 'Int'
        /// </summary>
        public static Action warningNumber = delegate
        {
            printDatetime();
            Console.Beep();
            Console.WriteLine("\n\tJust you Can Enter Number ....");
            Thread.Sleep(200);
        };

        /// <summary>
        /// If User Or Admin Enter Wrong Id And We Can't Find It
        /// </summary>
        public static Action<string, int, string> warningNotFound = delegate (string type, int id, string list)
          {
              printDatetime();
              Console.Beep();
              Console.WriteLine($"\n\tThere Isn't a {type} With This Is ({id}) In {list}");
              Thread.Sleep(2000);
          };

        /// <summary>
        /// This Action Is For ComeBack To Menu In User Or Admin Panel
        /// </summary>
        public static Action comeBackToMenu = delegate
        {
            Console.WriteLine("\n\n\n        \t\t\t\t\t--------------------------------------");
            Console.WriteLine("        \t\t\t\t\t| Press Any Key For ComeBack To Menu |");
            Console.WriteLine("        \t\t\t\t\t--------------------------------------");
            Console.ReadKey();
        };

        /// <summary>
        /// Show Notification List Of One User
        /// </summary>
        public static Action<int> showOneUserNotifiction = delegate (int userId)
        {
            using (OnlineShopDbContext dbNotifUser = new OnlineShopDbContext("onlineshopdbcontext"))
            {
                Console.WriteLine("\n  User Notification List : \n");
                if (dbNotifUser.Users.Any(t => t.Id == userId))
                {

                    foreach (UserNotification userNotification in dbNotifUser.UserNotifications.Where(t => t.User.Id == userId))
                    {
                        Console.WriteLine(userNotification);
                        Console.WriteLine("____________________________________________________________________________________________________");
                    }

                }
            }
        };

        /// <summary>
        /// Write In Json File For Register A Log
        /// </summary>
        public static Action<Admin, string> adminActivity = delegate (Admin admin, string activity)
         {
             using (StreamWriter log = new StreamWriter("Admin_Activity.json", true, Encoding.UTF8))
             {
                 string logMessage = $"({admin.Name} {admin.Family}) By This UserName {admin.Mobile} --> {activity} In {DateTime.Now}";
                 string write = JsonConvert.SerializeObject(logMessage);
                 log.WriteLine("\n"+write);
             }
         };

        /// <summary>
        /// Read Log File (.json)
        /// </summary>
        public static Action<string> getReport = delegate (string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string report = reader.ReadToEnd();
                Console.WriteLine("  "+report);
            }
        };
    }
}
