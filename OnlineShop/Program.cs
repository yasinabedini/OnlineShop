using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

namespace OnlineShop
{
    class Program
    {
        static void Main(string[] args)
        {
            #region DataBase Load Or Create
            using (OnlineShopDbContext dbOnlineShop = new OnlineShopDbContext("onlineshopdbcontext"))
            {
                Actions.printDatetime();
                Console.WriteLine("\n\tDataBase Is Creating ....\n\n\tPlease Wait ....");
                dbOnlineShop.Database.CreateIfNotExists();
                Actions.printDatetime();
                Console.WriteLine("\n\tDataBase Is Loading .....");
            }

            #endregion

            #region Default data
            using (OnlineShopDbContext dbDefaultData = new OnlineShopDbContext("onlineshopdbcontext"))
            {
                if (!dbDefaultData.Roles.Any())
                {
                    dbDefaultData.Roles.Add(new Role("admin"));
                    dbDefaultData.Roles.Add(new Role("user"));
                    dbDefaultData.SaveChanges();
                }

                if (!dbDefaultData.Admins.Any())
                {
                    dbDefaultData.Admins.Add(new Admin("yasin", "abedini", "0052525250", "09102587496", "0000", "admin", dbDefaultData.Roles.First(t => t.Name == "admin")));
                    dbDefaultData.SaveChanges();
                }

                if (!dbDefaultData.Users.Any())
                {
                    dbDefaultData.Users.Add(new User("Ali", "Moghadam Jah", "0052587896", "09100000000", "0000", dbDefaultData.Roles.First(t => t.Name == "user")));
                    dbDefaultData.SaveChanges();
                }

                if (!dbDefaultData.Products.Any())
                {
                    dbDefaultData.Products.Add(new Product("oil", "ladan", "golden semi-solid oil", 116000, 15, dbDefaultData.Admins.First(t => t.Id == 1), "without trans"));
                    dbDefaultData.SaveChanges();
                }

                if (!dbDefaultData.Cards.Any())
                {
                    dbDefaultData.Cards.Add(new Card("6063731068277988", "458", "01/04", "58222", dbDefaultData.Users.First(t => t.Id == 1)));
                    dbDefaultData.SaveChanges();
                }

            }

            List<string> departmentList = new List<string>() { "admin", "support", "storehouse", "financial" };

            List<string> productcategoryList = new List<string>() { "dairy", "oil", "beans", "drinks", "cereal", "breakfast", "conserve" };

            #endregion

            #region Check
            using (OnlineShopDbContext dbCheck = new OnlineShopDbContext("onlineshopdbcontext"))
            {
                //Recive Your Cart (Checked By DeliveryDate)
                foreach (Cart cart in dbCheck.Carts.Where(t => t.DeliveryDate <= DateTime.Now && t.IsReceive == false).ToList())
                {
                    dbCheck.Carts.First(t => t.Id == cart.Id).ReceiveYourCart();
                    dbCheck.SaveChanges();
                }


                //For Check Inperson Pay
                foreach (Cart receiveCart in dbCheck.Carts.Where(t => t.IsReceive == true && t.IsPay == false && t.PaymentMethod == "InPerson").ToList())
                {
                    Payment payment = new Payment(receiveCart);
                    dbCheck.Payments.Add(payment);
                    dbCheck.Carts.First(t => t.Id == receiveCart.Id).IsPay = true;
                    dbCheck.SaveChanges();
                }


                //If Inventory == 0 | Should Inventory Status == False
                foreach (Product product in dbCheck.Products.Where(t => t.Inventory == 0 && t.InventoryStatus == true).ToList())
                {
                    dbCheck.Products.First(t => t.Id == product.Id).InventoryStatus = false;
                    dbCheck.SaveChanges();

                    //Send Admin Notification For End Of Inventory.
                    foreach (Admin admin in dbCheck.Admins.ToList())
                    {
                        // If Admin Hasn't any Notification About The Over This Product Inventory And Or Has It But Don't Read It.
                        if (!admin.MyNotifications.Any(t => t.Title == "End Of Inventory" && t.Text == $"The Inventory Of {product.CategoryName} / {product.BrandName} / {product.ModelName} Is Over." && t.IsRead == false))
                        {
                            dbCheck.AdminNotifications.Add(new AdminNotification($"The Inventory Of {product.CategoryName} / {product.BrandName} / {product.ModelName} Is Over.", "End Of Inventory", dbCheck.Admins.First(t => t.Id == admin.Id)));
                            dbCheck.SaveChanges();
                        }
                    }
                }


                //If Product Has Inventory Shouls InventoryStatus = True
                foreach (Product product1 in dbCheck.Products.Where(t => t.Inventory != 0 && t.InventoryStatus == false).ToList())
                {
                    dbCheck.Products.First(t => t.Id == product1.Id).InventoryStatus = true;
                    dbCheck.SaveChanges();
                }


                //For Check DisCount Is Active
                foreach (DisCount disCount in dbCheck.DisCounts.Where(t => t.ExpirationDate < DateTime.Now && t.IsActive == true).ToList())
                {
                    dbCheck.DisCounts.First(t => t.Id == disCount.Id).IsActive = false;
                    dbCheck.SaveChanges();
                }
            }
            #endregion

            #region Pattern

            string validNationalCode = "^[0-9]{10}$";
            string validMobile = @"^((\+98|0)9\d{9})$";

        #endregion

        #region Loign Menu
        menuKey:
            Actions.printDatetime();
            Console.WriteLine("\n\n\n\n\t\t\t\t\t      ----------------------------------");
            Console.WriteLine("\t\t\t\t\t     |                                  |");
            Console.WriteLine("\t\t\t\t\t     | Login         ==>  Press |L| Key |");
            Console.WriteLine("\t\t\t\t\t     |                                  |");
            Console.WriteLine("\t\t\t\t\t     |                                  |");
            Console.WriteLine("\t\t\t\t\t     | Register User ==>  Press |R| Key |");
            Console.WriteLine("\t\t\t\t\t     |                                  |");
            Console.WriteLine("\t\t\t\t\t      ----------------------------------");
            ConsoleKeyInfo menuKey = Console.ReadKey();
            #endregion

            OnlineShopDbContext dbAllAdminUser = new OnlineShopDbContext("onlineshopdbcontext");
            var adminsUsers = dbAllAdminUser.Admins.Select(t => new { name = t.Name, family = t.Family, mobile = t.Mobile, password = t.Password, nationalCode = t.NationalCode, role = t.Role, access = t.IsActive }).Concat(dbAllAdminUser.Users.Select(t => new { name = t.Name, family = t.Family, mobile = t.Mobile, password = t.Password, nationalCode = t.NationalCode, role = t.Role, access = t.IsActive }));

            switch (menuKey.Key)
            {
                #region Login

                #region Login Form And Validaton
                case ConsoleKey.L:
                login:
                    Actions.printDatetime();
                    Console.Write("\n\t-Enter Your Mobile   : ");
                    string mobile = Console.ReadLine();
                    if (!Regex.IsMatch(mobile, validMobile))
                    {
                        Console.WriteLine("\n\t*Your Mobile Is False");
                        Console.WriteLine("\n\t*Try Again");
                        Thread.Sleep(2000);
                        goto login;
                    }
                enterPassword:
                    Console.Write("\n\t-Enter Your Password : ");
                    string password = Console.ReadLine();

                    //If This MobileNumber Is Not In DataBase
                    if (!adminsUsers.Any(t => t.mobile == mobile))
                    {
                        Actions.printDatetime();
                        //If There Is Not Mobile In DataBase
                        if (!adminsUsers.Any(t => t.mobile == mobile && t.role.Name == "user"))
                        {
                        wrongMobile:
                            Console.WriteLine("\n\tNo User Has Registered In The Online Shop With This MobileNumber");
                            Console.WriteLine("\t\t---------------------------------------------------------------------");
                            Console.WriteLine("\n\t\t|Press R For Register || Press T For Try Again || Press Esc For Exit|");
                            Console.WriteLine("\t\t---------------------------------------------------------------------");
                            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();
                            keyInfo = Console.ReadKey();
                            switch (keyInfo.Key)
                            {
                                case ConsoleKey.R: goto registerUser;

                                case ConsoleKey.T: goto login;

                                case ConsoleKey.Escape: goto menuKey;

                                default: goto wrongMobile;
                            }
                        }

                        //If Role == Admin
                        Console.WriteLine("\n\tNo Admin Has Registered In The Online Shop With This MobileNumber");
                        Thread.Sleep(2000);
                        goto menuKey;
                    }

                    //If Enter The MobileNumber Correctly But Enter Thr Password InCorrectly
                    if (adminsUsers.ToList().First(t => t.mobile == mobile).password != password)
                    {
                        Console.WriteLine("\n\tPassword Is Incorrect Try Again....");
                        Actions.printDatetime();
                        Console.WriteLine($"\n\t-Enter Your Mobile   : {mobile}");
                        goto enterPassword;
                    }

                    //if Mobile = true && Password == True But IsActive == False
                    if (adminsUsers.ToList().First(t => t.mobile == mobile).access == false)
                    {
                        Actions.printDatetime();
                        Console.WriteLine("\n\tYour Access Is Blocked ...");
                        Thread.Sleep(2000);
                        goto menuKey;
                    }
                    #endregion

                    switch (adminsUsers.First(t => t.mobile == mobile && t.password == password).role.Name)
                    {
                        #region Login Admin
                        case "admin":

                            Admin logedAdmin;
                            using (OnlineShopDbContext dbLogedAdmin = new OnlineShopDbContext("onlineshopdbcontext"))
                            {
                                logedAdmin = dbLogedAdmin.Admins.First(t => t.Mobile == mobile && t.Password == password);
                                dbLogedAdmin.Admins.First(t => t.Id == logedAdmin.Id).LastLoginDate = DateTime.Now;
                                dbLogedAdmin.SaveChanges();
                            }
                            //Log For Login Admin
                            using (StreamWriter log = new StreamWriter("Admin_LoginList.json", true, Encoding.UTF8))
                            {
                                string logMessage = $"(Login)(Admin) {logedAdmin.Name} {logedAdmin.Family} With This Mobile ({logedAdmin.Mobile}) Loged In {DateTime.Now};";
                                string write = JsonConvert.SerializeObject(logMessage);
                                log.WriteLine("\n" + write);
                            }

                        #region Admin Menu Operation
                        adminMenu:
                            Actions.PrintDatetimeInPanel(logedAdmin, "Admin Menu");
                            Console.WriteLine("\n");
                            Console.WriteLine("    ========================================================================================================================= ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    | A. View All Product         | H. View All Card            | O. Register Discount        | V. Change Viewpoint Access  |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    | B. View Product Inventory   | I. View All Admin           | P. Register Admin           | W. See My Notification      |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    | C. View All User            | J. Edit Discount            | Q. Register Product         | X. Delete Notification      |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    | D. View Unpaid Carts        | K. Edit Product (Adjustment)| R. Register Card            | Y. View All Logs            |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    | E. View Payments            | L. Edit User                | S. Register User            | Z. Search For SomeThing     |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    | F. View All Discount        | M. Edit Card                | T. Register Notification    | ESC. Exit Panel             |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    | G. View Product’s Viewpoint | N. Edit Admin               | U. Confirm Comment          |                             |   ");
                            Console.WriteLine("    |                             |                             |                             |                             |   ");
                            Console.WriteLine("    ========================================================================================================================= ");
                            Console.WriteLine("\n   Press The Key Of Your Desired Option......");
                            ConsoleKeyInfo adminMenukeyInfo = Console.ReadKey();
                            #endregion

                            switch (adminMenukeyInfo.Key)
                            {

                                #region A : View All Product
                                case ConsoleKey.A:
                                viewAllProduct:
                                    using (OnlineShopDbContext dbViewAllProduct = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "View All Product");
                                        Actions.showAllProduct();
                                        Console.WriteLine("\n\n\t\t\t\t------------------------------------------------------------------");
                                        Console.WriteLine("\t\t\t\t| Press M For ComeBack To Menu | Press D For See Product Details |");
                                        Console.WriteLine("\t\t\t\t------------------------------------------------------------------");
                                        ConsoleKeyInfo viewProductKey = Console.ReadKey();
                                        switch (viewProductKey.Key)
                                        {
                                            case ConsoleKey.M: goto adminMenu;

                                            //See Product Details
                                            case ConsoleKey.D:
                                                Console.Write("\n\n\tEnter Product Id For See Its Details : ");
                                                int productId;
                                                while (!int.TryParse(Console.ReadLine(), out productId))
                                                {
                                                    Actions.warningNumber();
                                                    goto viewAllProduct;
                                                }
                                                if (!dbViewAllProduct.Products.Any(t => t.Id == productId))
                                                {
                                                    Actions.warningNotFound("Product", productId, "Product List");
                                                    goto adminMenu;
                                                }
                                                Product productFind = dbViewAllProduct.Products.First(t => t.Id == productId);
                                            viewProductDetails:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "View Product Details");
                                                Console.WriteLine($"\n\tCategory : {productFind.CategoryName}");
                                                Console.WriteLine($"\n\tBrand : {productFind.BrandName}");
                                                Console.WriteLine($"\n\tModel : {productFind.ModelName}");
                                                Console.WriteLine($"\n\tPrice : {productFind.Price}");
                                                Console.WriteLine($"\n\tInventory : {productFind.Inventory}");
                                                Console.WriteLine($"\n\tAccess : {productFind.IsActive}");
                                                Console.WriteLine($"\n\tSells : {productFind.SalesNumber}");
                                                Console.WriteLine($"\n\tDescription : {productFind.Description}");
                                                Console.WriteLine($"\n\tComment : {productFind.Comments.Count()}      Like : {productFind.Likes.Count()}      DisLike : {productFind.DisLikes.Count()}");
                                                Console.WriteLine("\n\n    \t\t--------------------------------------------------------------------------------------");
                                                Console.WriteLine("    \t\t| Press M For ComeBack To Menu | Press V For View Comment | Press E For Edit Product | ");
                                                Console.WriteLine("    \t\t--------------------------------------------------------------------------------------");
                                                ConsoleKeyInfo viewProductDetailsKey = Console.ReadKey();
                                                switch (viewProductDetailsKey.Key)
                                                {
                                                    case ConsoleKey.M: goto adminMenu;

                                                    //See Product Comment
                                                    case ConsoleKey.V:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "See Product Comments");
                                                        foreach (Comment comment in dbViewAllProduct.Comments.Where(t => t.Product.Id == productId && t.Confrimation == true && t.Seconder != null))
                                                        {
                                                            Console.WriteLine(comment);
                                                            Console.WriteLine("___________________________________________________________________________");
                                                        }
                                                        Actions.comeBackToMenu();
                                                        goto adminMenu;

                                                    case ConsoleKey.E: goto editProduct;

                                                    default: goto viewProductDetails;
                                                }

                                            default: goto viewAllProduct;
                                        }
                                    }
                                #endregion

                                #region B : View Product Inventory
                                case ConsoleKey.B:
                                    using (OnlineShopDbContext dbShowInventory = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                    viewproductinventory:
                                        Actions.PrintDatetimeInPanel(logedAdmin, "View Product Inventory");
                                        foreach (Product product in dbShowInventory.Products.ToList())
                                        {
                                            Console.WriteLine($" Id:{product.Id}   Brand:{product.BrandName}\tModel:{product.ModelName}\tInventory:{product.Inventory}");
                                            Console.WriteLine("____________________________________________________________________________________________________");
                                        }
                                        Console.WriteLine("\n\n\n\t\t\t-------------------------------------------------------------------------------");
                                        Console.WriteLine("\t\t\t|Press M Key For Come Back To Menu || Press C key For Change Product Inventory|");
                                        Console.WriteLine("\t\t\t-------------------------------------------------------------------------------");
                                        ConsoleKeyInfo viewInventoryKey = Console.ReadKey();
                                        switch (viewInventoryKey.Key)
                                        {
                                            case ConsoleKey.M: goto adminMenu;

                                            case ConsoleKey.C: goto editProduct;

                                            default: goto viewproductinventory;
                                        }
                                    }
                                #endregion

                                #region C : View All User
                                case ConsoleKey.C:
                                viewAllUser:
                                    using (OnlineShopDbContext dbViewAllUser = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {

                                        Actions.PrintDatetimeInPanel(logedAdmin, "View All User");
                                        Actions.showAllUser();
                                        Console.WriteLine("\n\n\n\t\t   ---------------------------------------------------------------------------------------------");
                                        Console.WriteLine("\t\t   |Press M Key For Come Back To Menu || Press D key For Delete a User || Press E For Edit User|");
                                        Console.WriteLine("\t\t   ---------------------------------------------------------------------------------------------");
                                        ConsoleKeyInfo viewAllUserKey = Console.ReadKey();
                                        switch (viewAllUserKey.Key)
                                        {
                                            case ConsoleKey.M: goto adminMenu;

                                            case ConsoleKey.D: goto editUser;

                                            case ConsoleKey.E: goto editUser;

                                            default: goto viewAllUser;
                                        }
                                    }
                                #endregion

                                #region D : View Unpaid Cart
                                case ConsoleKey.D:

                                    using (OnlineShopDbContext dbViewOnpaidCart = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "View Unpaid Cart");
                                        foreach (Cart unPaidCart in dbViewOnpaidCart.Carts.Where(t => t.IsPay == false && t.Payment == null))
                                        {
                                            Console.WriteLine(unPaidCart);
                                            Console.WriteLine("____________________________________________________________________________________________________");
                                        }
                                        Actions.comeBackToMenu();
                                        goto adminMenu;
                                    }                                
                                #endregion

                                #region E : View payments
                                case ConsoleKey.E:
                                ViewPayments:
                                    using (OnlineShopDbContext dbShowPayment = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "View Payment");
                                        Console.WriteLine("\n\tA. View All Payments ");
                                        Console.WriteLine("\n\tB. View Today's Payments ");
                                        ConsoleKeyInfo viewPaymentKey = Console.ReadKey();
                                        switch (viewPaymentKey.Key)
                                        {
                                            //View All Payments
                                            case ConsoleKey.A:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "View All Payments");
                                                foreach (Payment payment in dbShowPayment.Payments)
                                                {
                                                    Console.WriteLine(" " + payment);
                                                    Console.WriteLine("------------------------------------------------------------------------------------------------------");

                                                }
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //View Today's Payments
                                            case ConsoleKey.B:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "View Today's Payments");
                                                foreach (Payment payment in dbShowPayment.Payments.Where(t => t.PayDate.Year == DateTime.Now.Year && t.PayDate.Month == DateTime.Now.Month && t.PayDate.Day == DateTime.Now.Day))
                                                {
                                                    Console.WriteLine(" " + payment);
                                                    Console.WriteLine("------------------------------------------------------------------------------------------------------");
                                                }
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            default: goto ViewPayments;
                                        }
                                    }
                                #endregion

                                #region F : View All Discount
                                case ConsoleKey.F:
                                viewDiscount:
                                    using (OnlineShopDbContext dbViewAllDiscount = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "View DisCount");
                                        Console.WriteLine("\n\tA. View All DisCount");
                                        Console.WriteLine("\n\tB. View Active DisCount");
                                        Console.WriteLine("\n\n\tPress Corresponding Key ...");
                                        ConsoleKeyInfo viewDiscountKey = Console.ReadKey();
                                        switch (viewDiscountKey.Key)
                                        {
                                            //All DisCount
                                            case ConsoleKey.A:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "View All DisCount");
                                                foreach (DisCount disCount in dbViewAllDiscount.DisCounts)
                                                {
                                                    Console.WriteLine(" " + disCount);
                                                    Console.WriteLine("------------------------------------------------------------------------------------------------------");
                                                }
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //Active Discount
                                            case ConsoleKey.B:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "View Active DisCount");
                                                foreach (DisCount disCount in dbViewAllDiscount.DisCounts.Where(t => t.IsActive == true))
                                                {
                                                    Console.WriteLine(" " + disCount);
                                                    Console.WriteLine("------------------------------------------------------------------------------------------------------");
                                                }
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            default: goto viewDiscount;
                                        }
                                    }
                                #endregion

                                #region G : View Product's Viewpoint
                                case ConsoleKey.G:

                                    using (OnlineShopDbContext dbComment = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                    viewComment:
                                        Actions.PrintDatetimeInPanel(logedAdmin, "View Comment");
                                        Console.WriteLine("\n\tA.View All Comment");
                                        Console.WriteLine("\n\tB.View comment by product");
                                        ConsoleKeyInfo viewCommentKey = Console.ReadKey();
                                        switch (viewCommentKey.Key)
                                        {
                                            //View All Comment With Confrimation==true
                                            case ConsoleKey.A:

                                                Actions.PrintDatetimeInPanel(logedAdmin, "View Confrimed Comment");
                                                foreach (Comment comment in dbComment.Comments.Where(t => t.Confrimation == true).ToList())
                                                {
                                                    Console.Write(" " + comment);
                                                    Console.WriteLine($"\n\n Confrim By : {comment.Seconder.Name} {comment.Seconder.Family}");
                                                    Console.WriteLine("------------------------------------------------------------------------------------------");
                                                }
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //View Comment By Product Id
                                            case ConsoleKey.B:
                                            ViewpointByProductId:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "View Comment By Product");
                                                Actions.showAllProduct();
                                                Console.Write("Enter Product Id For View It's Comment : ");
                                                int productId;
                                                while (!int.TryParse(Console.ReadLine(), out productId))
                                                {
                                                    Actions.showAllUser();
                                                    goto ViewpointByProductId;
                                                }
                                                if (!dbComment.Products.Any(t => t.Id == productId))
                                                {
                                                    Actions.warningNotFound("product", productId, "product List");
                                                    goto adminMenu;
                                                }
                                                Product productFind = dbComment.Products.First(t => t.Id == productId);

                                            productFindViewpoint:
                                                Actions.PrintDatetimeInPanel(logedAdmin, $"{productFind.ModelName}'s ViewPoints");
                                                Console.WriteLine("\t\t\t\t\t    ---------------------------------------");
                                                Console.WriteLine($"\t\t\t\t\t    Like : {productFind.Likes.Count}    DisLike : {productFind.DisLikes.Count}    Comment : {productFind.Comments.Count}");
                                                Console.WriteLine("\t\t\t\t\t    ---------------------------------------\n\n\n\n\n\n\n");
                                                Console.WriteLine("\t\t\t-----------------------------------------------------------------------------");
                                                Console.WriteLine("\t\t\t| Press Esc For Go To Admin Menu || Press C For View This Product's Comment | ");
                                                Console.WriteLine("\t\t\t-----------------------------------------------------------------------------");
                                                ConsoleKeyInfo viewpointKey = Console.ReadKey();
                                                switch (viewpointKey.Key)
                                                {
                                                    case ConsoleKey.Escape: goto adminMenu;

                                                    //See Comment
                                                    case ConsoleKey.C:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, $"See {productFind.ModelName}'s Comments");
                                                        if (productFind.Comments.Count == 0)
                                                        {
                                                            Console.WriteLine("\n\tFor This Product Not Registered Any Comment .....");
                                                            Thread.Sleep(2000);
                                                            goto adminMenu;
                                                        }
                                                        foreach (Comment comment in productFind.Comments.Where(t => t.Confrimation == true))
                                                        {
                                                            Console.WriteLine(" " + comment);
                                                            Console.WriteLine("------------------------------------------------------------------------------------------");
                                                        }
                                                        Actions.comeBackToMenu();
                                                        goto adminMenu;

                                                    default: goto productFindViewpoint;
                                                }

                                            default: goto viewComment;
                                        }
                                    }
                                #endregion

                                #region H : View All Card
                                case ConsoleKey.H:
                                    using (OnlineShopDbContext dbViewAllCard = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {

                                        Actions.PrintDatetimeInPanel(logedAdmin, "View All Cards");
                                        foreach (Card card in dbViewAllCard.Cards.ToList())
                                        {
                                            Console.WriteLine(" " + card);
                                            Console.WriteLine("______________________________________________________________________________________________________________________________");
                                        }
                                        Actions.comeBackToMenu();
                                        goto adminMenu;
                                    }
                                #endregion

                                #region I : View All Admin
                                case ConsoleKey.I:
                                viewAllAdmin:
                                    Actions.PrintDatetimeInPanel(logedAdmin, "View All Admins");
                                    Actions.showAllAdmin();
                                    Console.WriteLine("\n\n\n\t\t\t\t-----------------------------------------------------------------");
                                    Console.WriteLine("\t\t\t\t|Press M Key For Come Back To Menu || Press E key For Edit Admin|");
                                    Console.WriteLine("\t\t\t\t-----------------------------------------------------------------");
                                    ConsoleKeyInfo showAllAdminKey = Console.ReadKey();
                                    switch (showAllAdminKey.Key)
                                    {
                                        case ConsoleKey.M: goto adminMenu;
                                        case ConsoleKey.E: goto editAdmin;
                                        default: goto viewAllAdmin;
                                    }
                                #endregion

                                #region J : Edit Discount
                                case ConsoleKey.J:
                                editDiscount:
                                    using (OnlineShopDbContext dbEditDiscount = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit DisCount");
                                        //If There Is No Active Discount
                                        if (!dbEditDiscount.DisCounts.Where(t=>t.IsActive==true).Any())
                                        {
                                            Console.WriteLine("\n\tThere Is No Active DisCount");
                                            Console.WriteLine("\n\tTry Again In Other Time");
                                            Thread.Sleep(2000);
                                            goto adminMenu;
                                        }
                                        Actions.showAllDiscount(true);
                                        Console.WriteLine("* \n\tYou Can Just Edit Active DisCount ");
                                        Console.Write("\n\n\t-Enter Discount Id For Edit It's :");
                                        int discountId;
                                        while (!int.TryParse(Console.ReadLine(), out discountId))
                                        {
                                            Actions.warningNumber();
                                            goto editDiscount;
                                        }
                                        //If Admin Enter Wrong Discount Id
                                        if (!dbEditDiscount.DisCounts.Any(t => t.Id == discountId && t.IsActive == true))
                                        {
                                            Actions.warningNotFound("Discount", discountId, "Discount List");
                                            goto adminMenu;
                                        }
                                        DisCount disCountFind = dbEditDiscount.DisCounts.First(t => t.Id == discountId);
                                    WhichPart:
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit DisCount");
                                        Console.WriteLine("\n\tA.Edit Precent");
                                        Console.WriteLine("\n\tB.Add discount hours");
                                        Console.WriteLine($"\n\tC.Change Access (Active : {disCountFind.IsActive})");
                                        ConsoleKeyInfo editDiscountKey = Console.ReadKey();
                                        switch (editDiscountKey.Key)
                                        {
                                            //Edit Precent
                                            case ConsoleKey.A:
                                            editPrecent:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Precent Of DisCount");
                                                Console.WriteLine($"\n\tLast Precent : {disCountFind.Precent}");
                                                Console.Write("\n\tEnter New Precent For This Discount : ");
                                                float newPrecent;
                                                while (!float.TryParse(Console.ReadLine(), out newPrecent))
                                                {
                                                    Actions.warningNumber();
                                                    goto editPrecent;
                                                }
                                                if (newPrecent >= 1)
                                                {
                                                    Console.WriteLine("\n\n\t*You Enterd A False Precent..... \n\n\tTry Again...");
                                                    Thread.Sleep(2000);
                                                    goto editPrecent;
                                                }
                                                dbEditDiscount.DisCounts.First(t => t.Id == disCountFind.Id).Precent = newPrecent;
                                                dbEditDiscount.SaveChanges();
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Precent Of DisCount");
                                                Console.WriteLine($"\n\tRegistered A {newPrecent} Precent For This DisCount");
                                                Actions.comeBackToMenu();
                                                Actions.adminActivity(logedAdmin, $"Edit DisCount's Precent (DisCount ID : {disCountFind.Id})"); // log
                                                goto adminMenu;

                                            //Edit Hour Duration
                                            case ConsoleKey.B:
                                            editHour:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Hour Duration Of DisCount");
                                                Console.WriteLine($"\n\tExpirationDate : {disCountFind.ExpirationDate}");
                                                Console.Write("\n\tHow Many Hours Do You Want Add To tExpirationDate : ");
                                                int addHour;
                                                while (!int.TryParse(Console.ReadLine(), out addHour))
                                                {
                                                    Actions.warningNumber();
                                                    goto editHour;
                                                }
                                                dbEditDiscount.DisCounts.First(t => t.Id == disCountFind.Id).ExpirationDate = disCountFind.ExpirationDate.AddHours(addHour);
                                                dbEditDiscount.SaveChanges();
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Hour Duration Of DisCount");
                                                Console.WriteLine($"\n\t{addHour} Hour Add To ExpirationDate");
                                                Actions.comeBackToMenu();
                                                Actions.adminActivity(logedAdmin, $"Edit DisCount's Hour Duration (DisCount ID : {disCountFind.Id})"); //log
                                                goto adminMenu;

                                            //Edit Discount Access
                                            case ConsoleKey.C:
                                                dbEditDiscount.DisCounts.First(t => t.Id == disCountFind.Id).ChangeAccess();
                                                dbEditDiscount.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit DisCount's Access (DisCount ID : {disCountFind.Id})"); // log
                                                goto adminMenu;

                                            default: goto WhichPart;
                                        }
                                    }
                                #endregion

                                #region K : Edit Product
                                case ConsoleKey.K:
                                editProduct:
                                    using (OnlineShopDbContext dbEditProduct = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {

                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit Product");
                                        Actions.showAllProduct();
                                        Console.Write("\n\tEnter Product Id For Edit It's Information : ");
                                        int productId;
                                        while (!int.TryParse(Console.ReadLine(), out productId))
                                        {
                                            Actions.warningNumber();
                                            goto editProduct;
                                        }
                                        if (!dbEditProduct.Products.Any(t => t.Id == productId))
                                        {
                                            Actions.warningNotFound("product", productId, "Product List");
                                            goto adminMenu;
                                        }
                                        Product productFind = dbEditProduct.Products.First(t => t.Id == productId);
                                    editMenu:
                                        Actions.PrintDatetimeInPanel(logedAdmin, $"Edit Product ({productFind.BrandName}/{productFind.ModelName})");
                                        Console.WriteLine("\n\tA.Edit Category");
                                        Console.WriteLine("\n\tB.Edit Brand");
                                        Console.WriteLine("\n\tC.Edit Model");
                                        Console.WriteLine("\n\tD.Change Inventory");
                                        Console.WriteLine("\n\tE.Edit Price");
                                        Console.WriteLine("\n\tF.Edit Description");
                                        Console.WriteLine($"\n\tG.Change Access : (Active : {productFind.IsActive})");
                                        ConsoleKeyInfo editProductKey = Console.ReadKey();
                                        dbEditProduct.Products.First(t => t.Id == productFind.Id).Editor = dbEditProduct.Admins.First(t => t.Id == logedAdmin.Id);
                                        dbEditProduct.Products.First(t => t.Id == productFind.Id).EditDate = DateTime.Now;
                                        switch (editProductKey.Key)
                                        {
                                            //Edit Category
                                            case ConsoleKey.A:
                                            editCategory:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Product Category");
                                                Console.WriteLine($"\n\tLast Category Name : {productFind.CategoryName}");
                                                Console.Write("\n\tEnter New Categoty Name : ");
                                                Console.Write("(");
                                                foreach (var item in productcategoryList)
                                                {
                                                    Console.Write(" " + item + " ");
                                                }
                                                Console.Write(") :");
                                                string newCategory = Console.ReadLine();
                                                if (!productcategoryList.Any(t => t.Contains(newCategory)))
                                                {
                                                    Console.WriteLine("\n\t*Category Name Is False ... ");
                                                    goto editCategory;
                                                }
                                                dbEditProduct.Products.First(t => t.Id == productFind.Id).CategoryName = newCategory;
                                                dbEditProduct.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit Product Category (Product Id {productFind.Id})"); //log
                                                Console.WriteLine($"\n\n\t*Categoty Name Changed");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //Edit Brand
                                            case ConsoleKey.B:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Product Brand");
                                                Console.WriteLine($"\n\tLast Brand Name : {productFind.BrandName}");
                                                Console.Write("\n\tEnter New Brand Name : ");
                                                dbEditProduct.Products.First(t => t.Id == productFind.Id).BrandName = Console.ReadLine();
                                                dbEditProduct.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit Product Brand (Product Id {productFind.Id})"); //log
                                                Console.WriteLine($"\n\n\t*Brand Name Changed");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //Edit Model
                                            case ConsoleKey.C:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Product Model");
                                                Console.WriteLine($"\n\tLast Model Name : {productFind.ModelName}");
                                                Console.Write("\n\tEnter New Model Name : ");
                                                dbEditProduct.Products.First(t => t.Id == productFind.Id).ModelName = Console.ReadLine();
                                                dbEditProduct.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit Product Model (Product Id {productFind.Id})"); //log
                                                Console.WriteLine($"\n\n\t*Model Name Changed");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //Change Inventory
                                            case ConsoleKey.D:
                                            changeInventory:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Product Inventory");
                                                Console.WriteLine("\n\tA. Inventory Increase");
                                                Console.WriteLine("\n\tB. Inventory Reduction");
                                                ConsoleKeyInfo changeInventoryKey = Console.ReadKey();
                                                switch (changeInventoryKey.Key)
                                                {
                                                    //increaseInventory
                                                    case ConsoleKey.A:
                                                    increaseInventory:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Increase Product Inventory");
                                                        Console.WriteLine($"\n\tCurrent Product Inventory : {productFind.Inventory}");
                                                        Console.Write("\n\tHow Many Do You Want To Add This Product : ");
                                                        int increaseNumber;
                                                        while (!int.TryParse(Console.ReadLine(), out increaseNumber))
                                                        {
                                                            Actions.warningNumber();
                                                            goto increaseInventory;
                                                        }
                                                        dbEditProduct.Products.First(t => t.Id == productFind.Id).Inventory += increaseNumber;
                                                        dbEditProduct.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Increase Product Inventory (Product Id {productFind.Id})"); //log
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Increase Product Inventory");
                                                        Console.WriteLine($"\n\tProduct Inventory Increased To {productFind.Inventory + increaseNumber}");
                                                        Actions.comeBackToMenu();
                                                        goto adminMenu;

                                                    //ReductionInventory
                                                    case ConsoleKey.B:
                                                    ReductionInventory:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Reduction Product Inventory");
                                                        Console.WriteLine($"\n\tCurrent Product Inventory : {productFind.Inventory}");
                                                        Console.Write("\n\tHow Many Do You Want To Reduce This Product : ");
                                                        int ReductionNumber;
                                                        while (!int.TryParse(Console.ReadLine(), out ReductionNumber))
                                                        {
                                                            Actions.warningNumber();
                                                            goto ReductionInventory;
                                                        }
                                                        dbEditProduct.Products.First(t => t.Id == productFind.Id).Inventory -= ReductionNumber;
                                                        dbEditProduct.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Reduction Product Inventory (Product Id {productFind.Id})"); // Log
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Reduction Product Inventory");
                                                        Console.WriteLine($"\n\tProduct Inventory Reduction To {productFind.Inventory - ReductionNumber}");
                                                        Actions.comeBackToMenu();
                                                        goto adminMenu;

                                                    default: goto changeInventory;
                                                }

                                            //Edit Price
                                            case ConsoleKey.E:
                                            editPrice:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Product Price");
                                                Console.WriteLine($"\n\tLast Price : {productFind.Price}");
                                                Console.Write("\n\tEnter New Price : ");
                                                decimal newPrice;
                                                while (!decimal.TryParse(Console.ReadLine(), out newPrice))
                                                {
                                                    Actions.warningNumber();
                                                    goto editPrice;
                                                }
                                                dbEditProduct.Products.First(t => t.Id == productFind.Id).Price = newPrice;
                                                dbEditProduct.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit Product Price (Product Id {productFind.Id})"); //log
                                                Console.WriteLine($"\n\n\t*Price Changed To {newPrice}");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //Edit Description
                                            case ConsoleKey.F:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Product Description");
                                                Console.WriteLine($"\n\tLast Description : {productFind.Description}");
                                                Console.Write("\n\tEnter New Description : ");
                                                dbEditProduct.Products.First(t => t.Id == productFind.Id).Description = Console.ReadLine();
                                                dbEditProduct.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit Product Description (Product Id {productFind.Id})"); //log
                                                Console.WriteLine($"\n\n\t*Description Change ....");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //Change Access
                                            case ConsoleKey.G:
                                                dbEditProduct.Products.First(t => t.Id == productFind.Id).ChangeAccess();
                                                dbEditProduct.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Change Product Access (Product Id {productFind.Id})"); //log
                                                productFind.ChangeAccess();
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Change Product access");
                                                Console.WriteLine($"\n\tAccess Change To {productFind.IsActive}");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            default: goto editMenu;
                                        }
                                    }
                                #endregion 

                                #region L : Edit User                                                                  
                                case ConsoleKey.L:
                                editUser:
                                    using (OnlineShopDbContext dbEditUser = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit User");
                                        Actions.showAllUser();
                                        Console.Write("\n\t-Enter User Id For Edit Her/Him Information : ");
                                        int userId;
                                        while (!int.TryParse(Console.ReadLine(), out userId))
                                        {
                                            Actions.warningNumber();
                                            goto editUser;
                                        }
                                        if (!dbEditUser.Users.Any(t => t.Id == userId))
                                        {
                                            Actions.warningNotFound("User", userId, "User List");
                                            goto adminMenu;
                                        }

                                    editWhichPart:
                                        User userFind = dbEditUser.Users.First(t => t.Id == userId);
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit User");
                                        Console.WriteLine("\n\tA.Edit Name");
                                        Console.WriteLine("\n\tB.Edit Family");
                                        Console.WriteLine("\n\tC.Edit Password");
                                        Console.WriteLine("\n\tD.Edit Address");
                                        Console.WriteLine($"\n\tE.Change Access (Active : {userFind.IsActive})");
                                        Console.WriteLine("\n\tF.Delete User");
                                        ConsoleKeyInfo editUserKey = Console.ReadKey();
                                        switch (editUserKey.Key)
                                        {
                                            //Edit Name
                                            case ConsoleKey.A:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Name");
                                                Console.WriteLine($"\n\t-Her/Him Perviues Name : {userFind.Name}");
                                                Console.Write($"\n\t-Enter New Name : ");
                                                dbEditUser.Users.First(t => t.Id == userFind.Id).Name = Console.ReadLine();
                                                dbEditUser.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit User Name (User Id : {userFind.Id})"); //log
                                                userFind.Name = dbEditUser.Users.First(t => t.Id == userFind.Id).Name;
                                                Console.WriteLine($"\n\n\t *User Name Change To {userFind.Name}");
                                                Thread.Sleep(2000);
                                                goto adminMenu;

                                            //Edit Family
                                            case ConsoleKey.B:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Family");
                                                Console.WriteLine($"\n\t-Her/Him Perviues Family : {userFind.Family}");
                                                Console.Write($"\n\t-Enter New Name : ");
                                                dbEditUser.Users.First(t => t.Id == userFind.Id).Family = Console.ReadLine();
                                                dbEditUser.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit User Family (User Id : {userFind.Id})"); //log
                                                userFind.Family = dbEditUser.Users.First(t => t.Id == userFind.Id).Family;
                                                Console.WriteLine($"\n\n\t *User Family Change To {userFind.Family}");
                                                Thread.Sleep(2000);
                                                goto adminMenu;

                                            //Edit Password
                                            case ConsoleKey.C:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Password");
                                                Console.WriteLine($"\n\t-Her/Him Perviues Password : {userFind.Password}");
                                                Console.Write($"\n\t-Enter New Name : ");
                                                dbEditUser.Users.First(t => t.Id == userFind.Id).Password = Console.ReadLine();
                                                dbEditUser.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit User Password (User Id : {userFind.Id})"); //log
                                                userFind.Password = dbEditUser.Users.First(t => t.Id == userFind.Id).Password;
                                                Console.WriteLine($"\n\n\t *User Password Change To {userFind.Password}");
                                                Thread.Sleep(2000);
                                                goto adminMenu;

                                            //Edit Department
                                            case ConsoleKey.D:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Department");
                                                Console.WriteLine($"\n\t-Her/Him Perviues Address : {userFind.Address}");
                                                Console.Write($"\n\t-Enter New Department Name : ");
                                                dbEditUser.Users.First(t => t.Id == userFind.Id).Address = Console.ReadLine();
                                                dbEditUser.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit User Department (User Id : {userFind.Id})"); //log
                                                userFind.Address = dbEditUser.Users.First(t => t.Id == userFind.Id).Address;
                                                Console.WriteLine($"\n\n\t *User Address Change To {userFind.Address}");
                                                Thread.Sleep(2000);
                                                goto adminMenu;

                                            //Change Access
                                            case ConsoleKey.E:
                                                dbEditUser.Users.First(t => t.Id == userFind.Id).ChangeAccess();
                                                dbEditUser.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Change User Access (User Id : {userFind.Id})"); //log
                                                userFind.ChangeAccess();
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Change User Access");
                                                Console.WriteLine($"\n\t{userFind.Name} {userFind.Family}'s Access Is {userFind.IsActive}");
                                                Thread.Sleep(2000);
                                                goto adminMenu;

                                            //Delete User
                                            case ConsoleKey.F:
                                                if (dbEditUser.Users.First(t => t.Id == userId).AllCart.Count != 0 && dbEditUser.Users.First(t => t.Id == userId).Comments.Count != 0 && dbEditUser.Users.First(t => t.Id == userId).Likes.Count != 0 && dbEditUser.Users.First(t => t.Id == userId).DisLikes.Count != 0)
                                                {
                                                    Actions.PrintDatetimeInPanel(logedAdmin, "Delete User");
                                                    Console.WriteLine("\n\tYou Cant's Delete This User But You Can Change Access");
                                                    Thread.Sleep(3000);
                                                    goto editWhichPart;
                                                }
                                            areYouSure:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Delete User");
                                                Console.WriteLine($"\n\tAre You Sure For Delete This User : A. Yes  B. No\n\n\tName : {userFind.Name} {userFind.Family}\n\n\tNational Code : {userFind.NationalCode}\n\n\tMobile : {userFind.Mobile}\n\n\tPassword : {userFind.Password}\n\n\tActive : {userFind.IsActive}");

                                                ConsoleKeyInfo areYouSureKy = Console.ReadKey();
                                                switch (areYouSureKy.Key)
                                                {
                                                    case ConsoleKey.A:
                                                        dbEditUser.Users.Remove(dbEditUser.Users.First(t => t.Id == userFind.Id));
                                                        dbEditUser.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Delete User (User Id : {userFind.Id})"); //log
                                                        Console.WriteLine("\n\n\t*Delete This User*");
                                                        Actions.comeBackToMenu();
                                                        goto adminMenu;

                                                    case ConsoleKey.B: goto adminMenu;

                                                    default: goto areYouSure;
                                                }

                                            default: goto editWhichPart;
                                        }
                                    }
                                #endregion

                                #region M : Edit Card
                                case ConsoleKey.M:
                                editCard:
                                    using (OnlineShopDbContext dbEditCard = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit Card");
                                        foreach (Card card in dbEditCard.Cards.ToList())
                                        {
                                            Console.WriteLine(card);
                                            Console.WriteLine("_________________________________________________________");
                                        }
                                        Console.Write("\n\tEnter Card ID For edit Its : ");
                                        int cardId;
                                        while (!int.TryParse(Console.ReadLine(), out cardId))
                                        {
                                            Actions.warningNumber();
                                            goto editCard;
                                        }
                                        if (!dbEditCard.Cards.Any(t => t.Id == cardId))
                                        {
                                            Actions.warningNotFound("Card", cardId, "Wrning! ");
                                            goto adminMenu;
                                        }
                                        Card findCard = dbEditCard.Cards.First(t => t.Id == cardId);
                                    whichPart:
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit Card");
                                        Console.WriteLine($"\n\tCard Number : {findCard.CardNumber}\n\n\tCvv2 : {findCard.Cvv2}\n\n\tSecond Password : {findCard.SecondPassword}\n\n\tExpiration Date : {findCard.ExpirationDate}");
                                        Console.WriteLine("\n-----------------------------------------------------------");
                                        Console.WriteLine("\n\n\n\tA. Edit Card Number");
                                        Console.WriteLine("\n\tB. Edit Cvv2");
                                        Console.WriteLine("\n\tC. Edit Second Password");
                                        Console.WriteLine("\n\tD. Edit Expiration Date ");
                                        ConsoleKeyInfo editCardKey = Console.ReadKey();
                                        switch (editCardKey.Key)
                                        {
                                            //Edit Card Number
                                            case ConsoleKey.A:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Card Number");
                                                Console.WriteLine($"\n\tPrevious Card Number : {findCard.CardNumber}");
                                                Console.Write("\n\tEnter New card Number : ");
                                                string cardNumber = Console.ReadLine();
                                                findCard.CardNumber = cardNumber;
                                                dbEditCard.Cards.First(t => t.Id == findCard.Id).CardNumber = cardNumber;
                                                dbEditCard.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit Card Number (Card Id : {findCard.Id})"); //log
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Card Number");
                                                Console.WriteLine($"\n\tCard Number Changed To {cardNumber}");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //Edit Cvv2
                                            case ConsoleKey.B:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Cvv2");
                                                Console.WriteLine($"\n\tPrevious Cvv2 : {findCard.Cvv2}");
                                                Console.Write("\n\tEnter New Cvv2 : ");
                                                string cvv2 = Console.ReadLine();
                                                findCard.Cvv2 = cvv2;
                                                dbEditCard.Cards.First(t => t.Id == findCard.Id).Cvv2 = cvv2;
                                                dbEditCard.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit Cvv2 (Card Id : {findCard.Id})"); //log
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Cvv2");
                                                Console.WriteLine($"\n\tCvv2 Changed To {cvv2}");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //Edit Second Password
                                            case ConsoleKey.C:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Second Password");
                                                Console.WriteLine($"\n\tPrevious Second Password : {findCard.SecondPassword}");
                                                Console.Write("\n\tEnter New Second Password : ");
                                                string secondPassword = Console.ReadLine();
                                                findCard.SecondPassword = secondPassword;
                                                dbEditCard.Cards.First(t => t.Id == findCard.Id).SecondPassword = secondPassword;
                                                dbEditCard.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit Second Password (Card Id : {findCard.Id})"); //log
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Second Password");
                                                Console.WriteLine($"\n\tSecond Password Changed To {secondPassword}");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            //Edit Expiration Date
                                            case ConsoleKey.D:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Expiration Date");
                                                Console.WriteLine($"\n\tPrevious Expiration Date : {findCard.ExpirationDate}");
                                                Console.Write("\n\tEnter Expiration Date (--/--) : ");
                                                string expirationDate = Console.ReadLine();
                                                findCard.ExpirationDate = expirationDate;
                                                dbEditCard.Cards.First(t => t.Id == findCard.Id).ExpirationDate = expirationDate;
                                                dbEditCard.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Edit Expiration Date (Card Id : {findCard.Id})"); //log
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Expiration Date");
                                                Console.WriteLine($"\n\tExpiration Date Password Changed To {expirationDate}");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            default: goto whichPart;
                                        }
                                    }
                                #endregion

                                #region N : Edit Admin
                                case ConsoleKey.N:
                                editAdmin:
                                    using (OnlineShopDbContext dbEditAdmin = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit Admin");
                                        Console.WriteLine("\n\tA. Edit My Information");
                                        Console.WriteLine("\n\tB. Edit Other Admin's Information");
                                        Console.WriteLine("\n   Press The Key Of Your Desired Option......");
                                        ConsoleKeyInfo editAdminKey = Console.ReadKey();
                                        switch (editAdminKey.Key)
                                        {
                                            #region Edit My Information
                                            case ConsoleKey.A:
                                            editMyInformation:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit My Information");
                                                Console.WriteLine("\n\tA.Edit Name");
                                                Console.WriteLine("\n\tB.Edit Family");
                                                Console.WriteLine("\n\tC.Edit Password");
                                                ConsoleKeyInfo editMyInformationKey = Console.ReadKey();
                                                switch (editMyInformationKey.Key)
                                                {
                                                    //Edit Name
                                                    case ConsoleKey.A:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit My Name");
                                                        Console.WriteLine($"\n\t-Your Perviues Name : {logedAdmin.Name}");
                                                        Console.Write($"\n\t-Enter Your New Name : ");
                                                        dbEditAdmin.Admins.First(t => t.Id == logedAdmin.Id).Name = Console.ReadLine();
                                                        dbEditAdmin.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Edit HerSelf/HimSelf Name"); //log
                                                        logedAdmin.Name = dbEditAdmin.Admins.First(t => t.Id == logedAdmin.Id).Name;
                                                        Console.WriteLine($"\n\n\t *Your Name Change To {logedAdmin.Name}");
                                                        Thread.Sleep(2000);
                                                        goto adminMenu;

                                                    //Edit Family
                                                    case ConsoleKey.B:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit My Family");
                                                        Console.WriteLine($"\n\t-Your Perviues Family : {logedAdmin.Family}");
                                                        Console.Write($"\n\t-Enter Your New Name : ");
                                                        dbEditAdmin.Admins.First(t => t.Id == logedAdmin.Id).Family = Console.ReadLine();
                                                        dbEditAdmin.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Edit HerSelf/HimSelf Family"); //log
                                                        logedAdmin.Family = dbEditAdmin.Admins.First(t => t.Id == logedAdmin.Id).Family;
                                                        Console.WriteLine($"\n\n\t *Your Family Change To {logedAdmin.Family}");
                                                        Thread.Sleep(2000);
                                                        goto adminMenu;

                                                    //Edit Password
                                                    case ConsoleKey.C:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit My Password");
                                                        Console.WriteLine($"\n\t-Your Perviues Password : {logedAdmin.Password}");
                                                        Console.Write($"\n\t-Enter Your New Name : ");
                                                        dbEditAdmin.Admins.First(t => t.Id == logedAdmin.Id).Password = Console.ReadLine();
                                                        dbEditAdmin.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Edit HerSelf/HimSelf Password"); //log
                                                        logedAdmin.Password = dbEditAdmin.Admins.First(t => t.Id == logedAdmin.Id).Password;
                                                        Console.WriteLine($"\n\n\t *Your Password Change To {logedAdmin.Password}");
                                                        Thread.Sleep(2000);
                                                        goto adminMenu;

                                                    default: goto editMyInformation;
                                                }
                                            #endregion

                                            #region Edit Other Admin's Information
                                            case ConsoleKey.B:
                                            editOtherAdminInformation:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Admin");
                                                Actions.showAllAdmin();
                                                Console.Write("\n\t-Enter Admin Id For Edit Her/Him Information : ");
                                                int adminId;
                                                while (!int.TryParse(Console.ReadLine(), out adminId))
                                                {
                                                    Actions.warningNumber();
                                                    goto editOtherAdminInformation;
                                                }
                                                if (!dbEditAdmin.Admins.Any(t => t.Id == adminId))
                                                {
                                                    Actions.warningNotFound("Admin", adminId, "Admin List");
                                                    goto adminMenu;
                                                }
                                                if (adminId == logedAdmin.Id)
                                                {
                                                    goto editMyInformation;
                                                }
                                            editWhichPart:
                                                Admin adminFind = dbEditAdmin.Admins.First(t => t.Id == adminId);
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Edit Admin");
                                                Console.WriteLine("\n\tA.Edit Name");
                                                Console.WriteLine("\n\tB.Edit Family");
                                                Console.WriteLine("\n\tC.Edit Password");
                                                Console.WriteLine("\n\tD.Edit Department");
                                                Console.WriteLine($"\n\tE.Change Access (Active : {adminFind.IsActive})");
                                                Console.WriteLine("\n\tF.Delete Admin");
                                                ConsoleKeyInfo editOtherAdminKey = Console.ReadKey();
                                                switch (editOtherAdminKey.Key)
                                                {
                                                    //Edit Name
                                                    case ConsoleKey.A:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit Admin Name");
                                                        Console.WriteLine($"\n\t-Her/Him Perviues Name : {adminFind.Name}");
                                                        Console.Write($"\n\t-Enter New Name : ");
                                                        dbEditAdmin.Admins.First(t => t.Id == adminFind.Id).Name = Console.ReadLine();
                                                        dbEditAdmin.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Edit Admin Name (Admin Id : {adminFind.Id})"); //log
                                                        adminFind.Name = dbEditAdmin.Admins.First(t => t.Id == adminFind.Id).Name;
                                                        Console.WriteLine($"\n\n\t *Admin Name Change To {adminFind.Name}");
                                                        Thread.Sleep(2000);
                                                        goto adminMenu;

                                                    //Edit Family
                                                    case ConsoleKey.B:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit Admin Family");
                                                        Console.WriteLine($"\n\t-Her/Him Perviues Family : {adminFind.Family}");
                                                        Console.Write($"\n\t-Enter New Name : ");
                                                        dbEditAdmin.Admins.First(t => t.Id == adminFind.Id).Family = Console.ReadLine();
                                                        dbEditAdmin.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Edit Admin Family (Admin Id : {adminFind.Id})"); //log
                                                        adminFind.Family = dbEditAdmin.Admins.First(t => t.Id == adminFind.Id).Family;
                                                        Console.WriteLine($"\n\n\t *Admin Family Change To {adminFind.Family}");
                                                        Thread.Sleep(2000);
                                                        goto adminMenu;

                                                    //Edit Password
                                                    case ConsoleKey.C:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit Admin Password");
                                                        Console.WriteLine($"\n\t-Her/Him Perviues Password : {adminFind.Password}");
                                                        Console.Write($"\n\t-Enter New Name : ");
                                                        dbEditAdmin.Admins.First(t => t.Id == adminFind.Id).Password = Console.ReadLine();
                                                        dbEditAdmin.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Edit Admin Password (Admin Id : {adminFind.Id})"); //log
                                                        adminFind.Password = dbEditAdmin.Admins.First(t => t.Id == adminFind.Id).Password;
                                                        Console.WriteLine($"\n\n\t *Admin Password Change To {adminFind.Password}");
                                                        Thread.Sleep(2000);
                                                        goto adminMenu;

                                                    //Edit Department
                                                    case ConsoleKey.D:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Edit Admin Department");
                                                        Console.WriteLine($"\n\t-Her/Him Perviues Department : {adminFind.Department}");
                                                        Console.Write($"\n\t-Enter New Department Name : ");
                                                        dbEditAdmin.Admins.First(t => t.Id == adminFind.Id).Department = Console.ReadLine();
                                                        dbEditAdmin.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Edit Admin Department (Admin Id : {adminFind.Id})"); //log
                                                        adminFind.Department = dbEditAdmin.Admins.First(t => t.Id == adminFind.Id).Department;
                                                        Console.WriteLine($"\n\n\t *Admin Department Change To {adminFind.Department}");
                                                        Thread.Sleep(2000);
                                                        goto adminMenu;

                                                    //Change Access
                                                    case ConsoleKey.E:
                                                        dbEditAdmin.Admins.First(t => t.Id == adminFind.Id).ChangeAccess();
                                                        dbEditAdmin.SaveChanges();
                                                        Actions.adminActivity(logedAdmin, $"Change Product Access (Admin Id : {adminFind.Id})"); //log
                                                        adminFind.ChangeAccess();
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Change Admin Access");
                                                        Console.WriteLine($"\n\t{adminFind.Name} {adminFind.Family}'s Access Is {adminFind.IsActive}");
                                                        Thread.Sleep(2000);
                                                        goto adminMenu;

                                                    //Delete Admin
                                                    case ConsoleKey.F:
                                                        if (dbEditAdmin.Admins.First(t => t.Id == adminId).RegisterNotifications.Count != 0 && dbEditAdmin.Admins.First(t => t.Id == adminId).RegisterProduct.Count != 0 && dbEditAdmin.Admins.First(t => t.Id == adminId).RegistersDiscount.Count != 0 && dbEditAdmin.Admins.First(t => t.Id == adminId).CommentsApproved.Count != 0 && dbEditAdmin.Admins.First(t => t.Id == adminId).RegisterProduct.Count != 0)
                                                        {
                                                            Actions.PrintDatetimeInPanel(logedAdmin, "Delete Admin");
                                                            Console.WriteLine("\n\tYou Cant's Delete This User But You Can Change Access");
                                                            Thread.Sleep(3000);
                                                            goto editWhichPart;
                                                        }
                                                    areYouSure:
                                                        Actions.PrintDatetimeInPanel(logedAdmin, "Delete Admin");
                                                        Console.WriteLine($"\n\tAre You Sure For Delete This Admin : A. Yes   B. No\n\n\tName : {adminFind.Name} {adminFind.Family}\n\n\tNational Code : {adminFind.NationalCode}\n\n\tMobile : {adminFind.Mobile}\n\n\tPassword : {adminFind.Password}\n\n\tActive : {adminFind.IsActive}");
                                                        ConsoleKeyInfo areYouSureKy = Console.ReadKey();
                                                        switch (areYouSureKy.Key)
                                                        {
                                                            case ConsoleKey.A:
                                                                dbEditAdmin.Admins.Remove(dbEditAdmin.Admins.First(t => t.Id == adminFind.Id));
                                                                dbEditAdmin.SaveChanges();
                                                                Actions.adminActivity(logedAdmin, $"Delete Admin (Admin Id : {adminFind.Id})"); //log
                                                                Console.WriteLine("\n\n\t*Delete This Admin*");
                                                                Actions.comeBackToMenu();
                                                                goto adminMenu;

                                                            case ConsoleKey.B: goto adminMenu;

                                                            default: goto areYouSure;
                                                        }

                                                    default: goto editWhichPart;
                                                }
                                            default: goto editAdmin;
                                                #endregion
                                        }
                                    }
                                #endregion

                                #region O : Register Discount
                                case ConsoleKey.O:
                                registerDiscount:
                                    using (OnlineShopDbContext dbRegisterDiscount = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register DisCount");
                                        Actions.showAllProduct();
                                        Console.Write("\n\tEnter the productId for Add DisCount For It's : ");
                                        int productId;
                                        while (!int.TryParse(Console.ReadLine(), out productId))
                                        {
                                            Actions.warningNumber();
                                            goto registerDiscount;
                                        }
                                        //If There Is No Product By This ProductId
                                        if (!dbRegisterDiscount.Products.Any(t => t.Id == productId))
                                        {
                                            Actions.warningNotFound("product", productId, "Product List");
                                            goto adminMenu;
                                        }
                                        //If Product Has A Active DisCount
                                        if (dbRegisterDiscount.Products.First(t => t.Id == productId).DisCount.Any(t => t.IsActive == true))
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning ! ");
                                            Console.WriteLine("\n\tThis Product Has A Active Discount Now .....");
                                            Thread.Sleep(2000);
                                            goto adminMenu;
                                        }

                                        Product productFind = dbRegisterDiscount.Products.First(t => t.Id == productId);
                                    enterPrecent:
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register DisCount");
                                        Console.Write("\n\tEnter Precent For This Discount (0/01 --> 0/99) : ");
                                        float DiscountPercent;
                                        while (!float.TryParse(Console.ReadLine(), out DiscountPercent))
                                        {
                                            Actions.warningNumber();
                                            goto enterPrecent;
                                        }
                                        if (DiscountPercent >= 1)
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning!");
                                            Console.WriteLine("\n\tYou entered the wrong Precent ....");
                                            Thread.Sleep(1000);
                                            goto enterPrecent;
                                        }
                                        Console.Write("\n\tHow many hours do you want this discount to be active ؟ ");
                                        int DiscountDuration;
                                        while (!int.TryParse(Console.ReadLine(), out DiscountDuration))
                                        {
                                            Actions.warningNumber();
                                        }

                                        dbRegisterDiscount.DisCounts.Add(new DisCount(DiscountPercent, DiscountDuration, dbRegisterDiscount.Products.First(t => t.Id == productFind.Id), dbRegisterDiscount.Admins.First(t => t.Id == logedAdmin.Id)));
                                        dbRegisterDiscount.SaveChanges();

                                    sendNotification:
                                        Actions.adminActivity(logedAdmin, $"Register DisCount"); //log
                                        Console.WriteLine("\n\tDo You Want Sent Notification For User : A. Yes      B. No");
                                        ConsoleKeyInfo sendNotificationKey = Console.ReadKey();
                                        switch (sendNotificationKey.Key)
                                        {
                                            case ConsoleKey.A:
                                                //When We Register Discount, A Message Is Sent To All Users
                                                foreach (User user in dbRegisterDiscount.Users)
                                                {
                                                    dbRegisterDiscount.UserNotifications.Add(new UserNotification($"A Discount For {productFind.ModelName} Activated For {DiscountDuration} Hours.", "Activated Descount", dbRegisterDiscount.Admins.First(t => t.Id == logedAdmin.Id), dbRegisterDiscount.Users.First(t => t.Id == user.Id)));
                                                    dbRegisterDiscount.SaveChanges();
                                                }
                                                break;

                                            case ConsoleKey.B: break;

                                            default: goto sendNotification;
                                        }

                                        Actions.adminActivity(logedAdmin, $"Register DisCount"); //log
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register DisCount");
                                        Console.WriteLine($"\n\tFor {productFind.ModelName} Register A DisCount By {DiscountPercent} For {DiscountDuration} Hour ");
                                        Actions.comeBackToMenu();
                                        goto adminMenu;
                                    };
                                #endregion

                                #region P : Register Admin
                                case ConsoleKey.P:

                                    using (OnlineShopDbContext dbRegisterAdmin = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register Admin");                                        
                                        Console.Write("\n\tEnter Admin Name : ");
                                        string adminName = Console.ReadLine();
                                        Console.Write("\n\tEnter Admin Family : ");
                                        string adminFamily = Console.ReadLine();
                                    enterNationalCode:
                                        Console.Write("\n\tEnter Admin National Code : ");
                                        string adminNationalCode = Console.ReadLine();
                                        while (!Regex.IsMatch(adminNationalCode, validNationalCode))
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Register Admin");
                                            Console.WriteLine("\n\t*You did not enter the national code correctly, try again ..... ");
                                            Thread.Sleep(2000);
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Register Admin");
                                            Console.WriteLine($"\n\tEnter Admin Name : {adminName}");
                                            Console.WriteLine($"\n\tEnter Admin Family : {adminFamily}");
                                            goto enterNationalCode;
                                        }
                                        if (adminsUsers.Any(t => t.nationalCode == adminNationalCode))
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning!");
                                            Console.WriteLine("\n\tRegister A Admin With National Code....");
                                            Thread.Sleep(2000);
                                            goto adminMenu;
                                        }
                                    entermobile:
                                        Console.Write("\n\tEnter Admin Mobile : ");
                                        string adminMobile = Console.ReadLine();
                                        while (!Regex.IsMatch(adminMobile, validMobile))
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning! ");
                                            Console.WriteLine("\n\tYou did not enter the Mobile correctly, try again ..... ");
                                            Thread.Sleep(2000);
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Register Admin");
                                            Console.WriteLine($"\n\tEnter Admin Name : {adminName}");
                                            Console.WriteLine($"\n\tEnter Admin Family : {adminFamily}");
                                            Console.WriteLine($"\n\tEnter Admin National Code : {adminNationalCode}");
                                            goto entermobile;
                                        }
                                        if (adminsUsers.Any(t => t.mobile == adminMobile))
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning! ");
                                            Console.WriteLine("\n\tRegister A Admin With Mobile....");
                                            Thread.Sleep(2000);
                                            goto adminMenu;
                                        }
                                    enterAdminPassword:
                                        Console.Write("\n\tEnter Passwrod : ");
                                        string adminPassword = Console.ReadLine();
                                        Console.Write("\n\tEnter Repeat Passwrod : ");
                                        string repeatPassword = Console.ReadLine();
                                        if (adminPassword != repeatPassword)
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning! ");
                                            Console.WriteLine("\n\tPassword And It's Repeat Is Incorrect ... ");
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Register Admin");
                                            Console.WriteLine($"\n\tEnter Admin Name : {adminName}");
                                            Console.WriteLine($"\n\tEnter Admin Family : {adminFamily}");
                                            Console.WriteLine($"\n\tEnter Admin National Code : {adminNationalCode}");
                                            Console.WriteLine($"\n\tEnter Admin Moblie : {adminMobile}");
                                            goto enterAdminPassword;
                                        }
                                        Console.Write("\n\tEnter Department (admin,Support,....) : ");
                                        string adminDepartment = Console.ReadLine().ToLower();
                                        dbRegisterAdmin.Admins.Add(new Admin(adminName, adminFamily, adminNationalCode, adminMobile, adminPassword, adminDepartment, dbRegisterAdmin.Roles.First(t => t.Name == "admin")));
                                        dbRegisterAdmin.SaveChanges();
                                        Actions.adminActivity(logedAdmin, $"Register Admin"); //log
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register Admin");
                                        //Log For register New Admin
                                        using (StreamWriter log = new StreamWriter("Admin_SignUp.json", true, Encoding.UTF8))
                                        {
                                            string logMessage = $"(Register)(Admin) {adminName} {adminFamily} With This Mobile ({adminMobile}) Register In {DateTime.Now};";
                                            string write = JsonConvert.SerializeObject(logMessage);
                                            log.WriteLine("\n" + write);
                                        }
                                        Console.WriteLine($"\n\tAdmin With This Information Registerd :\n\n\n\t Name : {adminName} {adminFamily}\n\n\t NationalCode : {adminNationalCode}\n\n\t Mobile : {adminMobile}\n\n\t Password : {adminPassword}\n\n\t department : {adminDepartment}");
                                        Actions.comeBackToMenu();
                                        goto adminMenu;
                                    };
                                #endregion

                                #region Q : Register Product
                                case ConsoleKey.Q:
                                registerProduct:
                                    using (OnlineShopDbContext dbRegisterProduct = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register Product");                                        
                                        Console.Write("\n\tEnter Category Name  ");
                                        Console.Write("(");
                                        foreach (var item in productcategoryList)
                                        {
                                            Console.Write(" " + item + " ");
                                        }
                                        Console.Write(") :");
                                        string categoryName = Console.ReadLine();
                                        if (!productcategoryList.Any(t => t.Contains(categoryName)))
                                        {
                                            Console.WriteLine("\n\t*Category Name Is False ... ");
                                            goto registerProduct;
                                        }
                                        Console.Write("\n\tEnter Brand Name : ");
                                        string brandName = Console.ReadLine();
                                        Console.Write("\n\tEnter Model Name : ");
                                        string modeldName = Console.ReadLine();
                                        Console.Write("\n\tEnter description If You Want : ");
                                        string description = Console.ReadLine();
                                    enterPrice:
                                        Console.Write("\n\tEnter product Price : ");
                                        decimal price;
                                        while (!decimal.TryParse(Console.ReadLine(), out price))
                                        {
                                            Actions.warningNumber();
                                            Console.Write($"\n\tEnter Category Name : {categoryName}");
                                            Console.Write($"\n\tEnter Brand Name : {brandName}");
                                            Console.Write($"\n\tEnter Model Name : {modeldName}");
                                            Console.Write($"\n\tEnter description If You Want : {description}");
                                            goto enterPrice;
                                        }
                                        Console.Write("\n\tEnter The Initial Inventory : ");
                                        int inventory;
                                        while (!int.TryParse(Console.ReadLine(), out inventory))
                                        {
                                            Actions.warningNumber();
                                            Console.WriteLine($"\n\tEnter Category Name : {categoryName}");
                                            Console.WriteLine($"\n\tEnter Brand Name : {brandName}");
                                            Console.WriteLine($"\n\tEnter Model Name : {modeldName}");
                                            Console.WriteLine($"\n\tEnter description If You Want : {description}");
                                            Console.WriteLine($"\n\tEnter product Price : {price}");
                                            goto enterPrice;
                                        }

                                        dbRegisterProduct.Products.Add(new Product(categoryName, brandName, modeldName, price, inventory, dbRegisterProduct.Admins.First(t => t.Id == logedAdmin.Id), description));
                                        dbRegisterProduct.SaveChanges();
                                        Actions.adminActivity(logedAdmin, $"Register Product"); //log
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register Product");
                                        Console.WriteLine($"\n\tRegisterd A Product With This Information : \n\n\tCategory : {categoryName}\n\n\tBrand Name : {brandName}\n\n\tModel Name : {modeldName}\n\n\tPrice : {price}\n\n\tInventory : {inventory}\n\n\tDescription : {description}");
                                        Actions.comeBackToMenu();
                                        goto adminMenu;
                                    }
                                #endregion

                                #region R : Register Card
                                case ConsoleKey.R:
                                registerCard:
                                    using (OnlineShopDbContext dbCardRegister = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register Card");
                                        Actions.showAllUser();
                                        Console.Write("Enter User Id For This Card's Owner : ");
                                        int userId;
                                        while (!int.TryParse(Console.ReadLine(), out userId))
                                        {
                                            Actions.warningNumber();
                                            goto registerCard;
                                        }
                                        if (!dbCardRegister.Users.Any(t => t.Id == userId))
                                        {
                                            Actions.warningNotFound("User", userId, "User List");
                                            goto adminMenu;
                                        }
                                        User userFind = dbCardRegister.Users.First(t => t.Id == userId);
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register Card");
                                        Console.WriteLine($"Register Card For {userFind.Name} {userFind.Family}");
                                        Console.Write("\n\tEnter Card Number : ");
                                        string cardNumber = Console.ReadLine();
                                        if (dbCardRegister.Cards.Any(t => t.CardNumber == cardNumber))
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning! ");
                                            Console.WriteLine("\n\tThere Is A Card With This CardNumber.....");
                                            Thread.Sleep(2000);
                                            goto adminMenu;
                                        }
                                        Console.Write("\n\tEnter Card's cvv2 : ");
                                        string cvv2 = Console.ReadLine();
                                        Console.Write("\n\tEnter Expiration Date (--/--)");
                                        string ExpirationDate = Console.ReadLine();
                                        Console.Write("\n\tEnter your second's card password : ");
                                        string secondPassword = Console.ReadLine();
                                        dbCardRegister.Cards.Add(new Card(cardNumber, cvv2, ExpirationDate, secondPassword, dbCardRegister.Users.First(t => t.Id == userId)));
                                        dbCardRegister.SaveChanges();
                                        Actions.adminActivity(logedAdmin, $"Register Card"); //log
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register Card");
                                        Console.WriteLine($"\n\tRegistered A New Card By This Information : \n\n\tCard Number : {cardNumber}\n\n\tCvv2 : {cvv2}\n\n\tOwner : {userFind.Name} {userFind.Family}\n\n\tSecond PassWord : {secondPassword}\n\n\tExpirationDate : {ExpirationDate}");
                                        Actions.comeBackToMenu();
                                        goto adminMenu;
                                    }
                                #endregion

                                #region S : Register User
                                case ConsoleKey.S:

                                    using (OnlineShopDbContext dbUserRegister = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register User");
                                        Console.Write("\n\tEnter Name : ");
                                        string userName = Console.ReadLine();
                                        Console.Write("\n\tEnter Family : ");
                                        string userFamily = Console.ReadLine();
                                    enterNationalCode:
                                        Console.Write("\n\tEnter National Code : ");
                                        string userNationalCode = Console.ReadLine();
                                        while (!Regex.IsMatch(userNationalCode, validNationalCode))
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning! ");
                                            Console.WriteLine("\n\tNational Code Is False ....");
                                            Thread.Sleep(2000);
                                            Console.WriteLine($"\n\tEnter Name : {userName}");
                                            Console.WriteLine($"\n\tEnter Family : {userFamily}");
                                            goto enterNationalCode;
                                        }
                                        if (adminsUsers.Any(t => t.nationalCode == userNationalCode))
                                        {
                                            // Actions.PrintDatetimeInPanel(logedUser);
                                            Console.WriteLine("\n\tThere Is A User By This NationalCode");
                                            Thread.Sleep(2000);
                                            goto adminMenu;
                                        }

                                    enterMobileNumber:
                                        Console.Write("\n\tEnter mobile number : ");
                                        string userMobileNumber = Console.ReadLine();
                                        while (!Regex.IsMatch(userMobileNumber, validMobile))
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning! ");
                                            Console.WriteLine("\n\tNational Code Is False ....");
                                            Thread.Sleep(2000);
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Register User");
                                            Console.Write($"\n\tEnter Name : {userName}");
                                            Console.Write($"\n\tEnter Family : {userFamily}");
                                            Console.Write($"\n\tEnter National Code : {userNationalCode}");
                                            goto enterMobileNumber;
                                        }
                                        if (adminsUsers.Any(t => t.mobile == userMobileNumber))
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning");
                                            Console.WriteLine("\n\tThere Is A User By This Mobile");
                                            Thread.Sleep(2000);
                                            goto adminMenu;
                                        }
                                    EnterUserPassword:
                                        Console.Write("\n\tEnter password : ");
                                        string userPassword = Console.ReadLine();
                                        Console.Write("\n\tEnter Repeat Passwrod : ");
                                        string repeatPassword = Console.ReadLine();
                                        if (userPassword != repeatPassword)
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning");
                                            Console.WriteLine("\n\tPassword With It's Repeat Is Incorrect ... ");
                                            Thread.Sleep(2000);
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Register User");
                                            Console.Write($"\n\tEnter Name : {userName}");
                                            Console.Write($"\n\tEnter Family : {userFamily}");
                                            Console.Write($"\n\tEnter National Code : {userNationalCode}");
                                            Console.Write($"\n\tEnter Moblie : {userMobileNumber}");
                                            goto EnterUserPassword;
                                        }
                                        Console.Write("\n\tEnter Your Address : ");
                                        string userAddress = Console.ReadLine();
                                        dbUserRegister.Users.Add(new User(userName, userFamily, userNationalCode, userMobileNumber, userPassword, dbUserRegister.Roles.First(t => t.Name == "user"))).Address = userAddress;
                                        dbUserRegister.SaveChanges();
                                        Actions.adminActivity(logedAdmin, $"Register User"); //log
                                        //log For SignUp New User
                                        using (StreamWriter log = new StreamWriter("User_SignUp.json", true, Encoding.UTF8))
                                        {
                                            string logMessage = $"(Register)(User) {userName} {userFamily} With This Mobile ({userMobileNumber}) Register In {DateTime.Now};";
                                            string write = JsonConvert.SerializeObject(logMessage);
                                            log.WriteLine("\n" + write);
                                        }
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register User");
                                        Console.WriteLine($"\n\tRegistered a User With This Information : \n\n\n\tName : {userName} {userFamily}\n\n\tNational Code : {userNationalCode}\n\n\tMobile : {userMobileNumber}\n\n\tPassword : {userPassword}\n\n\tAddress : {userAddress}");
                                        Actions.comeBackToMenu();
                                        goto adminMenu;
                                    }
                                #endregion

                                #region T : Register Notification
                                case ConsoleKey.T:
                                registerNotifiction:
                                    using (OnlineShopDbContext dbRegisterNotifiction = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {

                                        Actions.PrintDatetimeInPanel(logedAdmin, "Register Notification");                                        
                                        Console.WriteLine("\n\tA.Register A Notifiction For All User");
                                        Console.WriteLine("\n\tB.Register A Notifiction Spacific User");
                                        ConsoleKeyInfo registerNotifictionKey = Console.ReadKey();
                                        switch (registerNotifictionKey.Key)
                                        {
                                            //Register Notifiction For All User
                                            case ConsoleKey.A:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Register Notification For All User");
                                                Console.Write("\n\tEnter Title For Notifiction : ");
                                                string notifictionsTitleForAllUser = Console.ReadLine();
                                                Console.Write("\n\tEnter Text For Notifiction : ");
                                                string notifictionsTextForAllUser = Console.ReadLine();
                                                foreach (User user in dbRegisterNotifiction.Users.ToList())
                                                {
                                                    dbRegisterNotifiction.Users.First(t => t.Id == user.Id).MyNotification.Add(new UserNotification(notifictionsTextForAllUser, notifictionsTitleForAllUser, dbRegisterNotifiction.Admins.First(t => t.Id == logedAdmin.Id), user));
                                                    dbRegisterNotifiction.SaveChanges();
                                                }
                                                Actions.adminActivity(logedAdmin, "Register Notification For All User"); //log
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Register Notification For All User");
                                                Console.WriteLine("\n\tRegister Notifiction For All User");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;


                                            //Register Notifiction For a User
                                            case ConsoleKey.B:
                                            registerNotifictionForUser:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Register Notification");
                                                Actions.showAllUser();
                                                Console.Write("Enter User Id For Registere a Notifiction For Her/Him : ");
                                                int userId;
                                                while (!int.TryParse(Console.ReadLine(), out userId))
                                                {
                                                    Actions.warningNumber();
                                                    goto registerNotifictionForUser;
                                                }
                                                if (!dbRegisterNotifiction.Users.Any(t => t.Id == userId))
                                                {
                                                    Actions.warningNotFound("User", userId, "User List");
                                                    goto adminMenu;
                                                }
                                                User userFind = dbRegisterNotifiction.Users.First(t => t.Id == userId);
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Register Notification");
                                                Console.Write("\n\tEnter Title For Notifiction : ");
                                                string notifictionsTitleForaUser = Console.ReadLine();
                                                Console.Write("\n\tEnter Text For Notifiction : ");
                                                string notifictionsTextForaUser = Console.ReadLine();
                                                dbRegisterNotifiction.Users.First(t => t.Id == userFind.Id).MyNotification.Add(new UserNotification(notifictionsTextForaUser, notifictionsTitleForaUser, dbRegisterNotifiction.Admins.First(t => t.Id == logedAdmin.Id), dbRegisterNotifiction.Users.First(t => t.Id == userFind.Id)));
                                                dbRegisterNotifiction.SaveChanges();
                                                Actions.adminActivity(logedAdmin, $"Register Notification For User By This ID ({userFind.Id})"); //log
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Register Notification");
                                                Console.WriteLine($"\n\tRegistered a Notifiction For {userFind.Name} {userFind.Family}");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            default: goto registerNotifiction;
                                        }
                                    }
                                #endregion

                                #region U : Confirm Comment
                                case ConsoleKey.U:
                                confrimComment:
                                    using (OnlineShopDbContext dbConfrimComment = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Confirm Comment");
                                        //Show Comment With Not Confirm
                                        foreach (Comment comment in dbConfrimComment.Comments.Where(t => t.Confrimation == false))
                                        {
                                            Console.WriteLine(" " + comment);
                                            Console.WriteLine("__________________________________________________________________________________________________");
                                        }
                                        Console.Write("\n\tEnter UnVerified Comment Id : ");
                                        int commentId;
                                        while (!int.TryParse(Console.ReadLine(), out commentId))
                                        {
                                            Actions.warningNumber();
                                            goto confrimComment;
                                        }
                                        if (!dbConfrimComment.Comments.Where(t => t.Confrimation == false).Any(t => t.Id == commentId))
                                        {
                                            Actions.warningNotFound("comment", commentId, "UnVerified Comment List");
                                            goto adminMenu;
                                        }
                                        dbConfrimComment.Comments.First(t => t.Id == commentId).Confrimation = true;
                                        dbConfrimComment.Comments.First(t => t.Id == commentId).Seconder = dbConfrimComment.Admins.First(t => t.Id == logedAdmin.Id);
                                        dbConfrimComment.SaveChanges();
                                        Actions.adminActivity(logedAdmin, $"Confirm Commnet By This Comment ID ({commentId})"); //log
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Confirm Comment");
                                        Console.WriteLine("\n\tComment Has Been Approved ..");
                                        Actions.comeBackToMenu();
                                        goto adminMenu;
                                    }
                                #endregion

                                #region V : Change Viewpoint Access
                                case ConsoleKey.V:
                                changeViewPointAccess:
                                    using (OnlineShopDbContext dbChangeAccess = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        //* We Can Change Comment Access If => Comment.confrimtion == true
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Change ViewPoint Access (only show Confirm Comment)");
                                        foreach (Comment comment in dbChangeAccess.Comments.Where(t => t.Confrimation == true).ToList())
                                        {
                                            Console.Write(" " + comment);
                                            Console.WriteLine($"\n\n Confrim By : {comment.Seconder.Name} {comment.Seconder.Family}\tAccess : {comment.IsActive}");
                                            Console.WriteLine("------------------------------------------------------------------------------------------");
                                        }
                                        Console.Write("Enter Comment Id : ");
                                        int commentId;
                                        while (!int.TryParse(Console.ReadLine(), out commentId))
                                        {
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Warning! ");
                                            goto changeViewPointAccess;
                                        }
                                        if (!dbChangeAccess.Comments.Where(t => t.Confrimation == true).Any(t => t.Id == commentId))
                                        {
                                            Actions.warningNotFound("Comment", commentId, "Comment List");
                                            goto adminMenu;
                                        }
                                        dbChangeAccess.Comments.Find(commentId).ChangeAccess();
                                        dbChangeAccess.SaveChanges();
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Change ViewPoint Access (only show Confirm Comment)");
                                        Console.WriteLine("\n\tChanged Access ....");
                                        Thread.Sleep(2000);
                                        goto adminMenu;
                                    }
                                #endregion

                                #region W : See My Notification
                                case ConsoleKey.W:
                                SeeMyNotification:
                                    using (OnlineShopDbContext dbSeeMyNotification = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        var allNotification = dbSeeMyNotification.Admins.First(t => t.Id == logedAdmin.Id).MyNotifications.ToList();
                                        var onReadNotification = dbSeeMyNotification.Admins.First(t => t.Id == logedAdmin.Id).MyNotifications.Where(t => t.IsRead == false).ToList();

                                        Actions.PrintDatetimeInPanel(logedAdmin, "See My Notification");
                                        Console.WriteLine("\n\tA. See OnRead Notification");
                                        Console.WriteLine("\n\tB. See All Notification");
                                        ConsoleKeyInfo seeMyNotificationKey = Console.ReadKey();
                                        switch (seeMyNotificationKey.Key)
                                        {
                                            #region See Onread Notification
                                            case ConsoleKey.A:
                                            seeOnreadNotification:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "See Onread Notification");
                                                if (onReadNotification.Count() == 0)
                                                {
                                                    Actions.PrintDatetimeInPanel(logedAdmin, "Warning! ");
                                                    Console.WriteLine("\n\tYou Dont's Have Any Onread Notification");
                                                    Thread.Sleep(2000);
                                                    goto adminMenu;
                                                }
                                                foreach (AdminNotification notification in onReadNotification)
                                                {
                                                    Console.WriteLine(notification);
                                                    Console.WriteLine("_______________________________________________");
                                                }
                                                Console.Write("Enter Notification Id For See It : ");
                                                int onreadNotificationId;
                                                while (!int.TryParse(Console.ReadLine(), out onreadNotificationId))
                                                {
                                                    Actions.warningNumber();
                                                    goto seeOnreadNotification;
                                                }
                                                if (!onReadNotification.Any(t => t.Id == onreadNotificationId))
                                                {
                                                    Actions.warningNotFound("Notification", onreadNotificationId, "Notification List");
                                                    goto adminMenu;
                                                }
                                                AdminNotification onreadAdminNotification = dbSeeMyNotification.AdminNotifications.First(t => t.Id == onreadNotificationId);
                                                Actions.PrintDatetimeInPanel(logedAdmin, "See Onread Notification");
                                                Console.WriteLine("\n\t" + onreadAdminNotification);
                                                Console.WriteLine($"\n\n\t Text : {onreadAdminNotification.Text}");
                                                // When User See Onread Notification We Sholud => notification.Isread == true 
                                                // And Its Notification Doesn't show In OnRead Notification List
                                                dbSeeMyNotification.AdminNotifications.First(t => t.Id == onreadNotificationId).IsRead = true;
                                                dbSeeMyNotification.SaveChanges();
                                                Actions.comeBackToMenu();
                                                goto adminMenu;
                                            #endregion

                                            #region See All Notification
                                            case ConsoleKey.B:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "See All Notification");
                                                if (allNotification.Count() == 0)
                                                {
                                                    Actions.PrintDatetimeInPanel(logedAdmin, "Warning! ");
                                                    Console.WriteLine("\n\tYou Dont's Have Any Onread Notification");
                                                    Thread.Sleep(2000);
                                                    goto adminMenu;
                                                }
                                                foreach (AdminNotification notification in allNotification)
                                                {
                                                    Console.WriteLine(notification);
                                                    Console.WriteLine("________________________________________________________________");
                                                }
                                                Console.Write("\n\tEnter Notifiction Id For See Its Text : ");
                                                int notificationId;
                                                while (!int.TryParse(Console.ReadLine(), out notificationId))
                                                {
                                                    Actions.warningNumber();
                                                    goto seeOnreadNotification;
                                                }
                                                if (!onReadNotification.Any(t => t.Id == notificationId))
                                                {
                                                    Actions.warningNotFound("Notification", notificationId, "Notification List");
                                                    goto adminMenu;
                                                }
                                                AdminNotification adminNotification = allNotification.First(t => t.Id == notificationId);
                                                Actions.PrintDatetimeInPanel(logedAdmin, "See All Notification");
                                                Console.WriteLine("\n\t" + adminNotification);
                                                Console.WriteLine($"\n\n\t Text : {adminNotification.Text}");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;
                                            #endregion

                                            default: goto SeeMyNotification;
                                        }
                                    }
                                #endregion

                                #region X : Delete Notification
                                case ConsoleKey.X:
                                deletNotification:
                                    using (OnlineShopDbContext dbDeletNotification = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Delete Notification");
                                        Console.WriteLine("\n\tA.See All Notifications");
                                        Console.WriteLine("\n\tB.See Notifictions By User");
                                        ConsoleKeyInfo deletenotifictionkey = Console.ReadKey();
                                        switch (deletenotifictionkey.Key)
                                        {
                                            case ConsoleKey.A:
                                            seeAllNotification:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Delete Notification");
                                                foreach (UserNotification notification in dbDeletNotification.UserNotifications)
                                                {
                                                    Console.WriteLine(notification);
                                                    Console.WriteLine("__________________________________________________________________________________________");
                                                }
                                                Console.Write("\n\tEnter Notification Id For Delete It : ");
                                                int notificationId;
                                                while (!int.TryParse(Console.ReadLine(), out notificationId))
                                                {
                                                    Actions.warningNumber();
                                                    goto seeAllNotification;
                                                }
                                                if (!dbDeletNotification.UserNotifications.Any(t => t.Id == notificationId))
                                                {
                                                    Actions.warningNotFound("notification", notificationId, "Notification List");
                                                    goto adminMenu;
                                                }

                                                UserNotification userNotificationFind = dbDeletNotification.UserNotifications.First(t => t.Id == notificationId);
                                                dbDeletNotification.UserNotifications.Remove(dbDeletNotification.UserNotifications.First(t => t.Id == notificationId));
                                                dbDeletNotification.SaveChanges();
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Delete Notification");
                                                Console.WriteLine($"\n\tDelete Notification By This Information : \n\n\tTitle : {userNotificationFind.Title}\n\n\tText : {userNotificationFind.Title}\n\n\tUser Name : {userNotificationFind.UserName}");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            case ConsoleKey.B:
                                            seeUserNotifiction:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Delete Notification");
                                                Actions.showAllUser();
                                                Console.Write("\n\tEnter User Id For See Her/Him Notifiction : ");
                                                int userId;
                                                while (!int.TryParse(Console.ReadLine(), out userId))
                                                {
                                                    Actions.warningNumber();
                                                    goto seeUserNotifiction;
                                                }
                                                if (!dbDeletNotification.Users.Any(t => t.Id == userId))
                                                {
                                                    Actions.warningNotFound("User", userId, "User List");
                                                    goto adminMenu;
                                                }
                                                User userFind = dbDeletNotification.Users.First(t => t.Id == userId);
                                                if (userFind.MyNotification == null)
                                                {
                                                    Actions.PrintDatetimeInPanel(logedAdmin, "Delete Notification");
                                                    Console.WriteLine("\n\tThere Is No Notifiction In This User's Notification List .....");
                                                    Thread.Sleep(2000);
                                                    goto adminMenu;
                                                }
                                            enterNotificationId:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Delete Notification");
                                                Actions.showOneUserNotifiction(userId);
                                                Console.Write("\n\tEnter Notifiction Id For Delete It : ");
                                                int notificationIdForDelete;
                                                while (!int.TryParse(Console.ReadLine(), out notificationIdForDelete))
                                                {
                                                    Actions.warningNumber();
                                                    goto enterNotificationId;
                                                }
                                                if (!userFind.MyNotification.Any(t => t.Id == notificationIdForDelete))
                                                {
                                                    Actions.warningNotFound("Notifiction", notificationIdForDelete, $"{userFind.Name} {userFind.Family}'s Notification List");
                                                    goto adminMenu;
                                                }
                                                UserNotification OneUserNotificationFind = dbDeletNotification.UserNotifications.First(t => t.Id == notificationIdForDelete);
                                                dbDeletNotification.UserNotifications.Remove(dbDeletNotification.UserNotifications.First(t => t.Id == notificationIdForDelete));
                                                dbDeletNotification.SaveChanges();
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Delete Notification");
                                                Console.WriteLine($"\n\tDelete a Notification By This Information From {userFind.Name} {userFind.Family}'s Notification List\n\n\tTitle : {OneUserNotificationFind.Title}\n\n\tText : {OneUserNotificationFind.Text}");
                                                Actions.comeBackToMenu();
                                                goto adminMenu;

                                            default:
                                                goto deletNotification;

                                        }
                                    }
                                #endregion

                                #region Y : View All Logs
                                case ConsoleKey.Y:
                                viewAllLog:
                                    Actions.PrintDatetimeInPanel(logedAdmin, "View All Logs");
                                    Console.WriteLine("\n\tA. Admin Login Report");
                                    Console.WriteLine("\n\tB. User Login Report");
                                    Console.WriteLine("\n\tC. Admin Activity Report");
                                    Console.WriteLine("\n\tD. Admin Registration Report");
                                    Console.WriteLine("\n\tE. User Registration Report");
                                    ConsoleKeyInfo viewReportKey = Console.ReadKey();
                                    switch (viewReportKey.Key)
                                    {
                                        case ConsoleKey.A:
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Admin Login Report");
                                            Actions.getReport("Admin_LoginList.json");
                                            break;

                                        case ConsoleKey.B:
                                            Actions.PrintDatetimeInPanel(logedAdmin, "User Login Report");
                                            Actions.getReport("User_LoginList.json");
                                            break;

                                        case ConsoleKey.C:
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Admin Activity Report");
                                            Actions.getReport("Admin_Activity.json");
                                            break;

                                        case ConsoleKey.D:
                                            Actions.PrintDatetimeInPanel(logedAdmin, "Admin Registration Report");
                                            Actions.getReport("Admin_SignUp.json");
                                            break;

                                        case ConsoleKey.E:
                                            Actions.PrintDatetimeInPanel(logedAdmin, "User Registration Report");
                                            Actions.getReport("User_SignUp.json");
                                            break;

                                        default: goto viewAllLog;
                                    }
                                    Actions.comeBackToMenu();
                                    goto adminMenu;
                                #endregion

                                #region Z :  Search For SomeThing
                                case ConsoleKey.Z:
                                Search:
                                    using (OnlineShopDbContext dbSearch = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedAdmin, "Search");
                                        Console.WriteLine("\n\tA. Search Admin");
                                        Console.WriteLine("\n\tB. Search User");
                                        Console.WriteLine("\n\tC. Search Product");
                                        Console.WriteLine("\n\tD. Search Comment");
                                        ConsoleKeyInfo searchKey = Console.ReadKey();
                                        switch (searchKey.Key)
                                        {
                                            #region Search Admin
                                            case ConsoleKey.A:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Search Admin");
                                                Console.Write("\n\tEnter The Phrase You Want : ");
                                                string searchAdmin = Console.ReadLine();
                                                if (!dbSearch.Admins.Any(t => t.Name.Contains(searchAdmin) || t.Family.Contains(searchAdmin) || t.NationalCode.Contains(searchAdmin) || t.Mobile.Contains(searchAdmin) || t.Department.Contains(searchAdmin)))
                                                {
                                                    Actions.PrintDatetimeInPanel(logedAdmin, "Search Admin");
                                                    Console.WriteLine("\n\tWe Dont Find This Word .......");
                                                    Thread.Sleep(2000);
                                                    goto adminMenu;
                                                }
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Search Admin");
                                                if (dbSearch.Admins.Any(t => t.Name.Contains(searchAdmin)))
                                                {
                                                    Console.WriteLine("\n | Find In Admin Name | \n");
                                                    foreach (Admin admin in dbSearch.Admins.Where(t => t.Name.Contains(searchAdmin)))
                                                    {
                                                        Console.WriteLine(admin);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                if (dbSearch.Admins.Any(t => t.Family.Contains(searchAdmin)))
                                                {
                                                    Console.WriteLine("\n | Find In Admin Family | \n");
                                                    foreach (Admin admin in dbSearch.Admins.Where(t => t.Family.Contains(searchAdmin)))
                                                    {
                                                        Console.WriteLine(admin);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                if (dbSearch.Admins.Any(t => t.Mobile.Contains(searchAdmin)))
                                                {
                                                    Console.WriteLine("\n | Find In Admin Mobile | \n");
                                                    foreach (Admin admin in dbSearch.Admins.Where(t => t.Mobile.Contains(searchAdmin)))
                                                    {
                                                        Console.WriteLine(admin);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                if (dbSearch.Admins.Any(t => t.NationalCode.Contains(searchAdmin)))
                                                {
                                                    Console.WriteLine("\n | Find In Admin NationalCode | \n");
                                                    foreach (Admin admin in dbSearch.Admins.Where(t => t.NationalCode.Contains(searchAdmin)))
                                                    {
                                                        Console.WriteLine(admin);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                if (dbSearch.Admins.Any(t => t.Department.Contains(searchAdmin)))
                                                {
                                                    Console.WriteLine("\n | Find In Admin Department | \n");
                                                    foreach (Admin admin in dbSearch.Admins.Where(t => t.Department.Contains(searchAdmin)))
                                                    {
                                                        Console.WriteLine(admin);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                Actions.comeBackToMenu();
                                                goto adminMenu;
                                            #endregion

                                            #region Search User
                                            case ConsoleKey.B:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Search User");
                                                Console.Write("\n\tEnter The Phrase You Want : ");
                                                string searchUser = Console.ReadLine();
                                                if (!dbSearch.Users.Any(t => t.Name.Contains(searchUser) || t.Family.Contains(searchUser) || t.NationalCode.Contains(searchUser) || t.Mobile.Contains(searchUser)))
                                                {
                                                    Actions.PrintDatetimeInPanel(logedAdmin, "Search User");
                                                    Console.WriteLine("\n\tWe Dont Find This Word .......");
                                                    Thread.Sleep(2000);
                                                    goto adminMenu;
                                                }
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Search User");
                                                if (dbSearch.Users.Any(t => t.Name.Contains(searchUser)))
                                                {
                                                    Console.WriteLine("\n | Find In User Name | \n");
                                                    foreach (User user in dbSearch.Users.Where(t => t.Name.Contains(searchUser)))
                                                    {
                                                        Console.WriteLine(user);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                if (dbSearch.Users.Any(t => t.Family.Contains(searchUser)))
                                                {
                                                    Console.WriteLine("\n | Find In User Family | \n");
                                                    foreach (User user in dbSearch.Users.Where(t => t.Family.Contains(searchUser)))
                                                    {
                                                        Console.WriteLine(user);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                if (dbSearch.Users.Any(t => t.Mobile.Contains(searchUser)))
                                                {
                                                    Console.WriteLine("\n | Find In User Mobile | \n");
                                                    foreach (User user in dbSearch.Users.Where(t => t.Name.Contains(searchUser)))
                                                    {
                                                        Console.WriteLine(user);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                if (dbSearch.Users.Any(t => t.NationalCode.Contains(searchUser)))
                                                {
                                                    Console.WriteLine("\n | Find In User NationalCode | \n");
                                                    foreach (User user in dbSearch.Users.Where(t => t.Name.Contains(searchUser)))
                                                    {
                                                        Console.WriteLine(user);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                Actions.comeBackToMenu();
                                                goto adminMenu;
                                            #endregion

                                            #region Search Product
                                            case ConsoleKey.C:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Search Product");
                                                Console.Write("\n\tEnter The Phrase You Want : ");
                                                string searchProduct = Console.ReadLine();
                                                if (!dbSearch.Products.Any(t => t.CategoryName.Contains(searchProduct) || t.BrandName.Contains(searchProduct) || t.ModelName.Contains(searchProduct)))
                                                {
                                                    Actions.PrintDatetimeInPanel(logedAdmin, "Search Product");
                                                    Console.WriteLine("\n\tWe Dont Find This Word .......");
                                                    Thread.Sleep(2000);
                                                    goto adminMenu;
                                                }
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Search Product");
                                                if (dbSearch.Products.Any(t => t.CategoryName.Contains(searchProduct)))
                                                {
                                                    Console.WriteLine("\n | Find In Product Category | \n");
                                                    foreach (Product product in dbSearch.Products.Where(t => t.CategoryName.Contains(searchProduct)))
                                                    {
                                                        Console.WriteLine(product);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                if (dbSearch.Products.Any(t => t.BrandName.Contains(searchProduct)))
                                                {
                                                    Console.WriteLine("\n | Find In Product Brand | \n");
                                                    foreach (Product product in dbSearch.Products.Where(t => t.BrandName.Contains(searchProduct)))
                                                    {
                                                        Console.WriteLine(product);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                if (dbSearch.Products.Any(t => t.ModelName.Contains(searchProduct)))
                                                {
                                                    Console.WriteLine("\n | Find In Product Model | \n");
                                                    foreach (Product product in dbSearch.Products.Where(t => t.ModelName.Contains(searchProduct)))
                                                    {
                                                        Console.WriteLine(product);
                                                        Console.WriteLine("_______________________________________________________________________________________________");
                                                    }
                                                }
                                                Actions.comeBackToMenu();
                                                goto adminMenu;
                                            #endregion

                                            #region Search Comment
                                            case ConsoleKey.D:
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Search Comment");
                                                Console.Write("\n\tEnter The Phrase You Want : ");
                                                string searchComment = Console.ReadLine();
                                                if (!dbSearch.Comments.Any(t => t.Text.Contains(searchComment)))
                                                {
                                                    Actions.PrintDatetimeInPanel(logedAdmin, "Search Comment");
                                                    Console.WriteLine("\n\tWe Dont Find This Word .......");
                                                    Thread.Sleep(2000);
                                                    goto adminMenu;
                                                }
                                                Actions.PrintDatetimeInPanel(logedAdmin, "Search Product");
                                                Console.WriteLine("\n | Find In Product Category | \n");
                                                foreach (Comment comment in dbSearch.Comments.Where(t => t.Text.Contains(searchComment) && t.Confrimation == true))
                                                {
                                                    Console.WriteLine(comment + $"\t\tConfirm Access : {comment.Confrimation}");

                                                    Console.WriteLine("_______________________________________________________________________________________________");
                                                }
                                                Actions.comeBackToMenu();
                                                goto adminMenu;
                                            #endregion

                                            default: goto Search;
                                        }
                                    }
                                #endregion

                                #region Esc : Exit Panel
                                case ConsoleKey.Escape:
                                    goto menuKey;
                                #endregion

                                #region Default
                                default:
                                    goto adminMenu;
                                    #endregion
                            }
                        #endregion

                        #region Login User
                        case "user":

                            User logedUser;
                            using (OnlineShopDbContext dbLogedUser = new OnlineShopDbContext("onlineshopdbcontext"))
                            {
                                logedUser = dbLogedUser.Users.First(t => t.Mobile == mobile && t.Password == password);
                                dbLogedUser.SaveChanges();

                            }
                            //log For Login User
                            using (StreamWriter log = new StreamWriter("User_LoginList.json", true, Encoding.UTF8))
                            {
                                string logMessage = $"(Login)(User) {logedUser.Name} {logedUser.Family} With This Mobile ({logedUser.Mobile}) Loged In {DateTime.Now};";
                                string write = JsonConvert.SerializeObject(logMessage);
                                log.WriteLine("\n" + write);
                            }



                        #region User Menu Operation
                        userMenu:
                            Actions.PrintDatetimeInPanel(logedUser, "User Menu");
                            Console.WriteLine("\n\n\n");
                            Console.WriteLine("                  ===========================================================================================");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  | A. Add New Product To Cart  | G. View My Notification     | M. Comment For Product      |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  | B. Add New Card             | H. View My Order            | N. Like Product             |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  | C. Edit My Cart             | I. View My Current Cart     | O. DisLike Product          |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  | D. Edit My Cards            | J. Pay Current Cart         | P. Best Sellers             |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  | E. View All Product         | K. Search Product           | Q. User account information |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  | F. View Discount And Offer  | L. See my views             | ESC. Exit Panel             |");
                            Console.WriteLine("                  |                             |                             |                             |");
                            Console.WriteLine("                  ===========================================================================================");
                            Console.WriteLine("\n\n                  Press The Key Of Your Desired Option......");
                            ConsoleKeyInfo userMenukeyInfo = Console.ReadKey();
                            #endregion

                            switch (userMenukeyInfo.Key)
                            {
                                
                                #region A : Add New Product To Cart
                                case ConsoleKey.A:
                                addNewProductToCart:
                                    using (OnlineShopDbContext dbAddProduct = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "Add New Product To My Cart");
                                        Console.WriteLine("Add Product To My Cart");
                                        Actions.showAllProduct();
                                        Console.Write("\n\tEnter Product Id For Add Its To Your Cart : ");
                                        int productId;
                                        while (!int.TryParse(Console.ReadLine(), out productId))
                                        {
                                            Actions.warningNumber();
                                            goto addNewProductToCart;
                                        }
                                        if (!dbAddProduct.Products.Any(t => t.Id == productId))
                                        {
                                            Actions.warningNotFound("Product", productId, "Product List");
                                            goto userMenu;
                                        }
                                    howMany:
                                        Actions.PrintDatetimeInPanel(logedUser, "Add New Product To My Cart");
                                        Console.Write("\n\tHow Many Do You Want : ");
                                        int howMany;
                                        while (!int.TryParse(Console.ReadLine(), out howMany))
                                        {
                                            Actions.warningNumber();
                                            goto howMany;
                                        }
                                        if (dbAddProduct.Products.First(t => t.Id == productId).Inventory < howMany)
                                        {
                                            Console.WriteLine("\n\t*This Product Has Not Inventory !!!!!");
                                            Thread.Sleep(3000);
                                            goto userMenu;
                                        }
                                        if (dbAllAdminUser.Users.Find(logedUser.Id).AllCart == null)
                                        {
                                            Cart newCart = new Cart(logedUser);
                                            dbAddProduct.Users.First(t => t.Id == logedUser.Id).AllCart.Add(newCart);
                                            logedUser.AllCart.Add(newCart);
                                            dbAddProduct.SaveChanges();
                                        }
                                        if (!dbAddProduct.Users.First(t => t.Id == logedUser.Id).AllCart.Any(t => t.IsPay == false && t.PaymentMethod == null))
                                        {
                                            dbAddProduct.Carts.Add(new Cart(dbAddProduct.Users.First(t => t.Id == logedUser.Id)));
                                            dbAddProduct.SaveChanges();
                                        }
                                        Cart currentCart = dbAddProduct.Carts.FirstOrDefault(t => t.User.Id == logedUser.Id && t.IsPay == false && t.PaymentMethod==null);
                                        if (currentCart.Orders == null)
                                        {
                                            dbAddProduct.Carts.First(t => t.Id == currentCart.Id).Orders = new List<Order>();
                                            dbAddProduct.SaveChanges();
                                        }
                                        if (currentCart.Orders.Any(t => t.Product.Id == productId))
                                        {
                                            currentCart.Orders.First(t => t.Product.Id == productId).NumberOfProduct += howMany;
                                        }
                                        if (!currentCart.Orders.Any(t => t.Product.Id == productId))
                                        {
                                            dbAddProduct.Carts.First(t => t.Id == currentCart.Id).Orders.Add(new Order(howMany, dbAddProduct.Products.First(t => t.Id == productId), dbAddProduct.Carts.First(t => t.Id == currentCart.Id)));
                                        }
                                        dbAddProduct.Products.First(t => t.Id == productId).Inventory -= howMany;
                                        dbAddProduct.Products.First(t => t.Id == productId).SalesNumber += howMany;
                                        dbAddProduct.Carts.First(t => t.Id == currentCart.Id).OrderCount++;
                                        dbAddProduct.SaveChanges();
                                        Actions.PrintDatetimeInPanel(logedUser, "Add New Product To My Cart");
                                        Console.WriteLine($"\n\n\tThis Product Add To Your Cart ({howMany})");
                                        Actions.comeBackToMenu();
                                        goto userMenu;
                                    }
                                #endregion

                                #region B : Add New Card
                                case ConsoleKey.B:
                                    using (OnlineShopDbContext dbAddNewCard = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "Add New Card");
                                        Console.Write("\n\t-Enter Card Number : ");
                                        string cardNumber = Console.ReadLine();
                                        if (dbAddNewCard.Cards.Any(t => t.CardNumber == cardNumber))
                                        {
                                            Actions.PrintDatetimeInPanel(logedUser, "Warning! ");
                                            Console.WriteLine("\n\tTHere Is a Card With This Card Number ...... ");
                                            Actions.comeBackToMenu();
                                            goto userMenu;
                                        }
                                        Console.Write("\n\tEnter Cvv2 : ");
                                        string cvv2 = Console.ReadLine();
                                        Console.Write("\n\tEnter ExpirationDate (--/--) : ");
                                        string expirationDate = Console.ReadLine();
                                        Console.Write("\n\tEnter SecondPassword : ");
                                        string secondPassword = Console.ReadLine();
                                        dbAddNewCard.Cards.Add(new Card(cardNumber, cvv2, expirationDate, secondPassword, dbAddNewCard.Users.First(t => t.Id == logedUser.Id)));
                                        dbAddNewCard.SaveChanges();
                                        Actions.PrintDatetimeInPanel(logedUser, "Add New Card");
                                        Console.WriteLine($"\n\tAdd New Card By This Information : \n\n\tCard Number : {cardNumber}\n\tSecond Password : {secondPassword}\n\n\tCvv2 : {cvv2}\n\n\tExpiration Date : {expirationDate}");
                                        Actions.comeBackToMenu();
                                        goto userMenu;
                                    }
                                #endregion
                                
                                #region c : Edit My Cart
                                case ConsoleKey.C:
                                editCart:
                                    using (OnlineShopDbContext dbEditMyCart = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {

                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Cart");
                                        Console.WriteLine("Edit Cart\n");
                                        if (!dbEditMyCart.Users.First(t => t.Id == logedUser.Id).AllCart.Any(t => t.IsPay == false && t.PaymentMethod == null))
                                        {
                                            Console.WriteLine("\n\tYou Dont Have Any Cart With Not Payment .... ");
                                            Thread.Sleep(300);
                                            goto userMenu;
                                        }
                                        Cart currentCart = dbEditMyCart.Carts.First(t => t.User.Id == logedUser.Id && t.IsPay == false && t.PaymentMethod == null);
                                        Console.WriteLine(currentCart + "\n\n");
                                        foreach (Order order in currentCart.Orders)
                                        {
                                            Console.WriteLine($" {order.Product.Id}\t{order.Product.ModelName}\t{order.Product.CategoryName}\t{order.Product.Price}\tNumber : {order.NumberOfProduct} Sum Price : {order.SumPrice}");
                                            Console.WriteLine("-----------------------------------------------------------------------------------------");
                                        }
                                        Console.WriteLine("\n\n\t\t*You Can Just Edit Current Cart ...");
                                        Console.WriteLine("\n\t\t-------------------------------------------------------------------------------------------");
                                        Console.WriteLine("\t\t|Press D For Delete Product From List | Press P For Pay THis Cart | Press M For Go To Menu|");
                                        Console.WriteLine("\t\t-------------------------------------------------------------------------------------------");
                                        ConsoleKeyInfo editCartKey = Console.ReadKey();
                                        switch (editCartKey.Key)
                                        {
                                            case ConsoleKey.D: /*First The Bug Of In A. Part Must Be Fixes Then Coding This Part ... */ goto userMenu;

                                            case ConsoleKey.P: goto payCurrentCart;

                                            case ConsoleKey.M: goto userMenu;

                                            default: goto editCart;
                                        }
                                    }
                                #endregion

                                #region D : Edit My Cards
                                case ConsoleKey.D:
                                editMyCard:
                                    using (OnlineShopDbContext dbEditCard = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Cards");
                                        Console.WriteLine("Edit My Card");
                                        if (!dbEditCard.Users.First(t => t.Id == logedUser.Id).Cards.Any())
                                        {
                                            Console.WriteLine("\n\tYou Dont Have Any Cart");
                                            Thread.Sleep(2000);
                                            goto userMenu;
                                        }
                                        foreach (Card card in dbEditCard.Users.First(t => t.Id == logedUser.Id).Cards)
                                        {
                                            Console.WriteLine(card);
                                            Console.WriteLine("-------------------------------------------------------------------------");
                                        }
                                        Console.Write("\n\tEnter Card Id For Edit Its : ");
                                        int cardId;
                                        while (!int.TryParse(Console.ReadLine(), out cardId))
                                        {
                                            Actions.warningNumber();
                                            goto editMyCard;
                                        }
                                        if (!dbEditCard.Users.First(t => t.Id == logedUser.Id).Cards.Any(t => t.Id == cardId))
                                        {
                                            Actions.warningNotFound("Card", cardId, "In Your Card List");
                                            goto userMenu;
                                        }
                                        Card findCard = dbEditCard.Cards.Find(cardId);
                                    whichPart:
                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Cards");
                                        Console.WriteLine("\n\n" + findCard);
                                        Console.WriteLine("\n\tA. Edit Card Number");
                                        Console.WriteLine("\n\tB. Edit Cvv2");
                                        Console.WriteLine("\n\tC. Edit Expiretion Date");
                                        Console.WriteLine("\n\tD. Edit Second Password");
                                        ConsoleKeyInfo editMyCardkey = Console.ReadKey();
                                        switch (editMyCardkey.Key)
                                        {
                                            //Edit Card Number
                                            case ConsoleKey.A:
                                                Actions.PrintDatetimeInPanel(logedUser, "Edit Card Number");
                                                Console.WriteLine("Edit Card Number");
                                                Console.WriteLine($"\n\tYour Previous Card Number : {findCard.CardNumber}");
                                                Console.Write("\n\tEnter New Card Number : ");
                                                string newCardNumber = Console.ReadLine();
                                                findCard.CardNumber = newCardNumber;
                                                dbEditCard.Cards.Find(findCard.Id).CardNumber = newCardNumber;
                                                dbEditCard.SaveChanges();
                                                Actions.PrintDatetimeInPanel(logedUser, "Edit Card Number");
                                                Console.WriteLine($"\n\tCard Number Change To {newCardNumber}");
                                                Actions.comeBackToMenu();
                                                goto userMenu;
                                            //Edit Cvv2
                                            case ConsoleKey.B:
                                                Actions.PrintDatetimeInPanel(logedUser, "Edit Cvv2");
                                                Console.WriteLine("Edit Cvv2");
                                                Console.WriteLine($"\n\tYour Previous Cvv2 : {findCard.Cvv2}");
                                                Console.Write("\n\tEnter New Cvv2 : ");
                                                string newCvv2 = Console.ReadLine();
                                                findCard.Cvv2 = newCvv2;
                                                dbEditCard.Cards.Find(findCard.Id).Cvv2 = newCvv2;
                                                dbEditCard.SaveChanges();
                                                Actions.PrintDatetimeInPanel(logedUser, "Edit Cvv2");
                                                Console.WriteLine($"\n\tcvv2 Change To {newCvv2}");
                                                Actions.comeBackToMenu();
                                                goto userMenu;
                                            //Edit Expiration Date
                                            case ConsoleKey.C:
                                                Actions.PrintDatetimeInPanel(logedUser, "Edit Expiration Date");
                                                Console.WriteLine("Edit Expiration Date");
                                                Console.WriteLine($"\n\tYour Previous Expiration Date : {findCard.ExpirationDate}");
                                                Console.Write("\n\tEnter New Expiration Date (--/--) : ");
                                                string newExpirationDate = Console.ReadLine();
                                                findCard.ExpirationDate = newExpirationDate;
                                                dbEditCard.Cards.Find(findCard.Id).ExpirationDate = newExpirationDate;
                                                dbEditCard.SaveChanges();
                                                Actions.PrintDatetimeInPanel(logedUser, "Edit Expiration Date");
                                                Console.WriteLine($"\n\tExpiration Date Change To {newExpirationDate}");
                                                Actions.comeBackToMenu();
                                                goto userMenu;
                                            //Edit Second Password
                                            case ConsoleKey.D:
                                                Actions.PrintDatetimeInPanel(logedUser, "Edit Second Password");
                                                Console.WriteLine("Edit Second Password");
                                                Console.WriteLine($"\n\tYour Previous Second Password : {findCard.SecondPassword}");
                                                Console.Write("\n\tEnter New Second Password : ");
                                                string newSecondPassword = Console.ReadLine();
                                                findCard.SecondPassword = newSecondPassword;
                                                dbEditCard.Cards.Find(findCard.Id).SecondPassword = newSecondPassword;
                                                dbEditCard.SaveChanges();
                                                Actions.PrintDatetimeInPanel(logedUser, "Edit Second Password");
                                                Console.WriteLine($"\n\tSecond Password Change To {newSecondPassword}");
                                                Actions.comeBackToMenu();
                                                goto userMenu;

                                            default: goto whichPart;
                                        }
                                    }
                                #endregion
                                
                                #region E : View All Product
                                case ConsoleKey.E:
                                    using (OnlineShopDbContext dbViewAllProduct = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "View All Product");
                                        Actions.showAllProduct();
                                        Actions.comeBackToMenu();
                                        goto userMenu;
                                    }
                                #endregion

                                #region F : View Discount And Offer
                                case ConsoleKey.F:
                                    Actions.PrintDatetimeInPanel(logedUser, "View Discount And Offer");
                                    Console.WriteLine("\n\tView Discount(offer!)");
                                    using (OnlineShopDbContext dbViewdiscount = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        if (dbViewdiscount.DisCounts.Any(t => t.IsActive == true))
                                        {
                                            foreach (DisCount offer in dbViewdiscount.DisCounts.Where(t => t.IsActive == true))
                                            {
                                                Console.WriteLine($" Product id : {offer.Product.Id}\t Product Name : {offer.ProductName}\t percent : {offer.Precent}\t HourDuration : {offer.HourDuration}");
                                                Console.WriteLine("______________________________________________________________________________________________________________");
                                            }
                                        }
                                    }
                                    Actions.comeBackToMenu();
                                    goto userMenu;
                                #endregion

                                #region G : View My Notificatioan
                                case ConsoleKey.G:
                                ViewMyNotificatioan:
                                    using (OnlineShopDbContext dbViewMyNotification = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        var OnReadNotificationList = dbViewMyNotification.Users.First(t => t.Id == logedUser.Id).MyNotification.Where(t => t.IsRead == false).ToList();
                                        var NotificationList = dbViewMyNotification.Users.First(t => t.Id == logedUser.Id).MyNotification.ToList();

                                        Actions.PrintDatetimeInPanel(logedUser, "View My Notificatioan");
                                        Console.WriteLine("View My Notification");
                                        Console.WriteLine($"\n\tA. View My OnRead Notification ({OnReadNotificationList.Count()})");
                                        Console.WriteLine("\n\tB. View My All Notification");
                                        ConsoleKeyInfo viewMyNotificationKey = Console.ReadKey();
                                        switch (viewMyNotificationKey.Key)
                                        {
                                            //View Onread Notification
                                            case ConsoleKey.A:
                                            viewMyOnreadNotification:
                                                Actions.PrintDatetimeInPanel(logedUser, "View My Onread Notificatioan");
                                                if (OnReadNotificationList.Count() == 0)
                                                {
                                                    Console.WriteLine("\n\tYou Have'nt Any Onread Notification");
                                                    Thread.Sleep(2000);
                                                    goto userMenu;
                                                }
                                                Console.WriteLine("View My Onread Notification\n\n");
                                                foreach (UserNotification notification in OnReadNotificationList)
                                                {
                                                    Console.WriteLine($" \t\t{notification.Id}\t{notification.Title}      \t\t{notification.RegisterDate.Year}/{notification.RegisterDate.Month}/{notification.RegisterDate.Day}");
                                                    Console.WriteLine("  \t\t_______________________________________");
                                                }
                                                Console.Write("\n\tEnter Notification Id For Show It :");

                                                int onReadNotificationId;
                                                while (!int.TryParse(Console.ReadLine(), out onReadNotificationId))
                                                {
                                                    Actions.warningNumber();
                                                    goto viewMyOnreadNotification;
                                                }
                                                if (!OnReadNotificationList.Any(t => t.Id == onReadNotificationId))
                                                {
                                                    Actions.warningNotFound("Onread Notifacation", onReadNotificationId, "Onread Notification List");
                                                    goto userMenu;
                                                }
                                                UserNotification OnReadNotificationFind = OnReadNotificationList.First(t => t.Id == onReadNotificationId);
                                            seeOnreadNotification:
                                                Actions.PrintDatetimeInPanel(logedUser, "View My Onread Notificatioan");
                                                OnReadNotificationList = dbViewMyNotification.Users.First(t => t.Id == logedUser.Id).MyNotification.Where(t => t.IsRead == false).ToList();
                                                Console.WriteLine("\n" + OnReadNotificationFind);

                                                //If User See One Notification, That Notification Must : Isread = true and No Longer Appear On Onread Notification List.
                                                dbViewMyNotification.UserNotifications.First(t => t.Id == onReadNotificationId).IsRead = true;
                                                dbViewMyNotification.SaveChanges();
                                                Console.WriteLine("\n\n\t\t\t\t-------------------------------------------------------------------");
                                                Console.WriteLine("\t\t\t\t|Press M For Goto Menu | Press N For See Other Onread Notification|");
                                                Console.WriteLine("\t\t\t\t-------------------------------------------------------------------");
                                                ConsoleKeyInfo backFromOnreadNotifacation = Console.ReadKey();
                                                switch (backFromOnreadNotifacation.Key)
                                                {
                                                    case ConsoleKey.M:
                                                        goto userMenu;

                                                    case ConsoleKey.N:
                                                        goto viewMyOnreadNotification;

                                                    default:
                                                        goto seeOnreadNotification;
                                                }


                                            //View All Notification
                                            case ConsoleKey.B:
                                            viewMyAllNotification:
                                                Actions.PrintDatetimeInPanel(logedUser, "View My All Notificatioan");
                                                if (NotificationList.Count() == 0)
                                                {
                                                    Console.WriteLine("\n\tYou Have'nt Any Notification");
                                                    Thread.Sleep(2000);
                                                    goto userMenu;
                                                }
                                                Console.WriteLine("View My All Notification\n\n");
                                                foreach (UserNotification notification in NotificationList)
                                                {
                                                    Console.WriteLine($" \t\t{notification.Id}\t{notification.Title}      \t\t{notification.RegisterDate.Year}/{notification.RegisterDate.Month}/{notification.RegisterDate.Day}");
                                                    Console.WriteLine("  \t\t_______________________________________");
                                                }
                                                Console.Write("\n\tEnter Notification Id For Show It :");
                                                int NotificationId;
                                                while (!int.TryParse(Console.ReadLine(), out NotificationId))
                                                {
                                                    Actions.warningNumber();
                                                    goto viewMyAllNotification;
                                                }
                                                if (!NotificationList.Any(t => t.Id == NotificationId))
                                                {
                                                    Actions.warningNotFound("Notifacation", NotificationId, "Notification List");
                                                    goto userMenu;
                                                }
                                            seeNotification:
                                                Actions.PrintDatetimeInPanel(logedUser, "View My All Notificatioan");
                                                dbViewMyNotification.UserNotifications.First(t => t.Id == NotificationId).IsRead = true;
                                                UserNotification NotificationFind = NotificationList.First(t => t.Id == NotificationId);
                                                Console.WriteLine("\n" + NotificationFind);
                                                dbViewMyNotification.SaveChanges();
                                                Console.WriteLine("\n\n\t\t\t\t-------------------------------------------------------------");
                                                Console.WriteLine("\t\t\t\t|Press M For Goto Menu | Press N For See Other Notification|");
                                                Console.WriteLine("\t\t\t\t-------------------------------------------------------------");
                                                ConsoleKeyInfo backFromAllNotifacation = Console.ReadKey();
                                                switch (backFromAllNotifacation.Key)
                                                {
                                                    case ConsoleKey.M:
                                                        goto userMenu;

                                                    case ConsoleKey.N:
                                                        goto viewMyAllNotification;

                                                    default:
                                                        goto seeNotification;
                                                }

                                            default:
                                                goto ViewMyNotificatioan;
                                        }



                                    }
                                #endregion

                                #region H : View My Order
                                case ConsoleKey.H:
                                viewMyOrder:
                                    using (OnlineShopDbContext dbViewAllOrder = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "View My Order");
                                        if (dbViewAllOrder.Users.First(t => t.Id == logedUser.Id).AllCart.ToList().Count() == 0)
                                        {
                                            Console.WriteLine("\n\tYou Don't Have Any Cart....");
                                            Thread.Sleep(2000);
                                            goto userMenu;
                                        }
                                        Console.WriteLine("View My Oreders");
                                        foreach (Cart cart in dbViewAllOrder.Carts.Where(t => t.User.Id == logedUser.Id && t.PaymentMethod != null))
                                        {
                                            Console.WriteLine($"\n\t{cart.Id}\tPayment Method : {cart.PaymentMethod}\tPay : {cart.IsPay}\tReceive : {cart.IsReceive}\tEnd Price : {cart.EndPrice}");
                                        }
                                        Console.WriteLine("\n\n\t\t--------------------------------------------------------------");
                                        Console.WriteLine("\t\t|Press M For Goto Menu | Press V For View All Details Of Cart|");
                                        Console.WriteLine("\t\t--------------------------------------------------------------");
                                        ConsoleKeyInfo viewOrderKey = Console.ReadKey();
                                        switch (viewOrderKey.Key)
                                        {
                                            case ConsoleKey.M:
                                                goto userMenu;

                                            case ConsoleKey.V:
                                            viewCartDetails:
                                                Console.Write("\n\tEnter Cart Id For View All Details : ");
                                                int cartId;
                                                while (!int.TryParse(Console.ReadLine(), out cartId))
                                                {
                                                    Actions.warningNumber();
                                                    goto viewCartDetails;
                                                }
                                                if (!dbViewAllOrder.Users.First(t => t.Id == logedUser.Id).AllCart.Any(t => t.Id == cartId))
                                                {
                                                    Actions.warningNotFound("Cart", cartId, "Cart List");
                                                    goto userMenu;
                                                }
                                                Cart cartFind = dbViewAllOrder.Carts.First(t => t.Id == cartId);
                                                Actions.PrintDatetimeInPanel(logedUser, "View Cart Details");
                                                Console.WriteLine("Your Order");
                                                if (dbViewAllOrder.Carts.First(t => t.Id == cartFind.Id).Orders == null)
                                                {
                                                    Actions.PrintDatetimeInPanel(logedUser, "Warning! ");
                                                    Console.WriteLine("\n\tThis Cart Has'nt Any Product ...");
                                                    Thread.Sleep(2000);
                                                    goto userMenu;
                                                }
                                                if (dbViewAllOrder.Carts.Find(cartId).Orders.Count() == 0)
                                                {
                                                    Actions.PrintDatetimeInPanel(logedUser, "Warning! ");
                                                    Console.WriteLine("\n\tThis Cart Has'nt Any Product ...");
                                                    Thread.Sleep(2000);
                                                    goto userMenu;
                                                }
                                                foreach (Order order in dbViewAllOrder.Carts.First(t => t.Id == cartFind.Id).Orders)
                                                {
                                                    Console.WriteLine("\n" + order);
                                                }
                                                Actions.comeBackToMenu();
                                                goto userMenu;

                                            default: goto viewMyOrder;
                                        }
                                    }
                                #endregion

                                #region I : View My Current Cart
                                //********************************************************************
                                //Each User Any Can Have One Cart Without Payment And He/She Can See That Cart In This Section
                                //********************************************************************
                                case ConsoleKey.I:
                                    using (OnlineShopDbContext dbViewCurrentCart = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "View My Current Cart");
                                        if (!dbViewCurrentCart.Users.First(t => t.Id == logedUser.Id).AllCart.Any(t => t.IsPay == false && t.PaymentMethod == null))
                                        {
                                            Console.WriteLine("\n\tYou Dont Have Any Cart With Not Payment .... ");
                                            Thread.Sleep(300);
                                            goto userMenu;
                                        }
                                        Cart currentCart = dbViewCurrentCart.Carts.First(t => t.User.Id == logedUser.Id && t.IsPay == false && t.PaymentMethod == null);
                                        Actions.PrintDatetimeInPanel(logedUser, "View My Current Cart");
                                        Console.WriteLine(currentCart + "\n\n");
                                        foreach (Order order in currentCart.Orders)
                                        {
                                            Console.WriteLine($" {order.Product.Id}\tProduct : {order.Product.ModelName}\t\tPrice : {order.Product.Price} Rial\tNumber : {order.NumberOfProduct}\tSum Price : {order.SumPrice}");
                                            Console.WriteLine("-----------------------------------------------------------------------------------------");
                                        }
                                        Actions.comeBackToMenu();
                                        goto userMenu;
                                    }
                                #endregion

                                #region J : Pay Current Cart
                                case ConsoleKey.J:
                                payCurrentCart:
                                    using (OnlineShopDbContext dbPayCart = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "Pay Cuurent Cart");
                                        if (!dbPayCart.Carts.Any(t => t.User.Id == logedUser.Id && t.IsPay == false && t.PaymentMethod == null))
                                        {
                                            Console.WriteLine("\n\tYou Dont Have Any UnPaid Cart ....");
                                            Actions.comeBackToMenu();
                                            goto userMenu;
                                        }
                                        Cart unPaidCart = dbPayCart.Users.First(t => t.Id == logedUser.Id).AllCart.First(t => t.IsPay == false && t.PaymentMethod == null);
                                        unPaidCart.EndPrice = unPaidCart.CalculateEndPrice();
                                        Console.WriteLine(unPaidCart);
                                        Console.WriteLine($"End Price : {unPaidCart.EndPrice}");
                                        Console.WriteLine("\n\n\t\t\t---------------------------------------------------");
                                        Console.WriteLine("\t\t\t|Press E For Edit Cart | Press P For Pay This Cart|");
                                        Console.WriteLine("\t\t\t---------------------------------------------------");
                                        ConsoleKeyInfo payCartKey = Console.ReadKey();

                                        switch (payCartKey.Key)
                                        {
                                            case ConsoleKey.E:
                                                goto editCart;

                                            case ConsoleKey.P:
                                            payCart:
                                                Actions.PrintDatetimeInPanel(logedUser, "Pay Current Cart");
                                                unPaidCart.EndPrice = unPaidCart.Orders.Sum(t => t.SumPrice);
                                                dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).EndPrice = unPaidCart.Orders.Sum(t => t.SumPrice);
                                                Console.WriteLine($"Id {unPaidCart.Id}     Products : {unPaidCart.Orders.Count()}\tEnd Price : {unPaidCart.EndPrice}");
                                                Console.WriteLine($"\n\t{logedUser.Address}");
                                                Console.WriteLine($"\n\tDo You Want Receive Your Order To This Address ({logedUser.Address}) : A.Yes  B.No");
                                                ConsoleKeyInfo addressKey = Console.ReadKey();
                                                switch (addressKey.Key)
                                                {
                                                    case ConsoleKey.A:
                                                        break;

                                                    case ConsoleKey.B:
                                                        Console.Write("\n\n\tEnter New Address For Send Your Order : ");
                                                        logedUser.Address = Console.ReadLine();
                                                        dbPayCart.Users.First(t => t.Id == logedUser.Id).Address = logedUser.Address;
                                                        dbPayCart.SaveChanges();
                                                        break;

                                                    default:
                                                        goto payCart;
                                                }
                                            choosePaymentMethod:
                                                Actions.PrintDatetimeInPanel(logedUser, "Pay Current Cart");
                                                Console.WriteLine($"Id {unPaidCart.Id}     Products : {unPaidCart.Orders.Count()}\tEnd Price : {unPaidCart.EndPrice}");
                                                Console.WriteLine($"\n\n\tAddress : {logedUser.Address}");
                                                Console.WriteLine("\n\n\tChoose The Payment Method : ");
                                                Console.WriteLine("\n\tA. Online Pay");
                                                Console.WriteLine("\n\tB. In Person Pay");
                                                Console.WriteLine("\n\tC. Pay From Wallet");
                                                ConsoleKeyInfo payMethosKey = Console.ReadKey();
                                                switch (payMethosKey.Key)
                                                {
                                                    #region Online Pay
                                                    case ConsoleKey.A:
                                                    onlinePay:
                                                        Actions.PrintDatetimeInPanel(logedUser, "Online Pay Current Cart");
                                                        Console.WriteLine("Online Pay");
                                                        Console.WriteLine("\n\tA. Choose Card From My Cards");
                                                        Console.WriteLine("\n\tB. Enter New Card For Pay");
                                                        ConsoleKeyInfo onlinePayKey = Console.ReadKey();
                                                        Card payCard;
                                                        switch (onlinePayKey.Key)
                                                        {
                                                            // Choose Pay Card From Card List
                                                            case ConsoleKey.A:
                                                            chooseFromCards:
                                                                Actions.PrintDatetimeInPanel(logedUser, "Online Pay Current Cart");
                                                                if (dbPayCart.Users.First(t => t.Id == logedUser.Id).Cards.Count() == 0)
                                                                {
                                                                    Console.WriteLine("\n\tYou Don't Have Any Card");
                                                                    Thread.Sleep(2000);
                                                                    goto userMenu;
                                                                }
                                                                Console.WriteLine();
                                                                foreach (Card card in dbPayCart.Users.First(t => t.Id == logedUser.Id).Cards)
                                                                {
                                                                    Console.WriteLine($"\tId : {card.Id}\tCard Number : {card.CardNumber}\tOwner : {card.OwnerName}\n");
                                                                }
                                                                Console.Write("\n\tEnter Card Id For Choose It For Py : ");
                                                                int cardId;
                                                                while (!int.TryParse(Console.ReadLine(), out cardId))
                                                                {
                                                                    Actions.warningNumber();
                                                                    goto chooseFromCards;
                                                                }
                                                                if (!dbPayCart.Cards.Any(t => t.Id == cardId))
                                                                {
                                                                    Actions.warningNotFound("Card", cardId, "Card List");
                                                                    goto userMenu;
                                                                }
                                                                payCard = dbPayCart.Cards.First(t => t.Id == cardId);
                                                                break;

                                                            //Add New Card Then Choose It
                                                            case ConsoleKey.B:
                                                                Actions.PrintDatetimeInPanel(logedUser, "Online Pay Current Cart");
                                                                Console.WriteLine("New Card");
                                                                Console.Write("\n\tEnter Card Number : ");
                                                                string cardNumber = Console.ReadLine();
                                                                Console.Write("\n\tEnter Cvv2 : ");
                                                                string cvv2 = Console.ReadLine();
                                                                Console.Write("\n\tEnter Expiration Date (--/--)");
                                                                string expirationDate = Console.ReadLine();
                                                                Console.Write("\n\tEnter your second's card password : ");
                                                                string secondPassword = Console.ReadLine();
                                                                Card NewCard = new Card(cardNumber, cvv2, expirationDate, secondPassword, dbPayCart.Users.First(t => t.Id == logedUser.Id));
                                                                dbPayCart.Cards.Add(NewCard);
                                                                dbPayCart.SaveChanges();
                                                                payCard = NewCard;
                                                                goto chooseFromCards;



                                                            default:
                                                                goto onlinePay;
                                                        }
                                                        Actions.PrintDatetimeInPanel(logedUser, "Online Pay Current Cart");
                                                        Console.WriteLine($"\n\tAddress : {logedUser.Address}");
                                                        Console.WriteLine($"\n\tPayCard : {payCard.CardNumber}");
                                                        Console.Write("\n\tEnter Cvv2 : ");
                                                        string cvv2ForPay = Console.ReadLine();
                                                        Console.Write("\n\tEnter Second Password : ");
                                                        string secondPasswordForPay = Console.ReadLine();
                                                        Console.Write("\n\tEnter ExpiretionDate (--/--) : ");
                                                        string expiretionDateForPay = Console.ReadLine();
                                                        if (payCard.Cvv2 != cvv2ForPay || payCard.SecondPassword != secondPasswordForPay || payCard.ExpirationDate != expiretionDateForPay)
                                                        {
                                                            Console.WriteLine("\n\n\t*Card Information IN Not True ......");
                                                            Thread.Sleep(2000);
                                                            goto onlinePay;
                                                        }

                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).PaymentMethod = "Online";
                                                        dbPayCart.SaveChanges();
                                                        dbPayCart.Payments.Add(new Payment(dbPayCart.Carts.First(t => t.Id == unPaidCart.Id), dbPayCart.Cards.First(t => t.Id == payCard.Id)));
                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).EndPrice = unPaidCart.CalculateEndPrice();
                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).IsPay = true;
                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).DeliveryDate = DateTime.Now.AddDays(3);
                                                        dbPayCart.SaveChanges();
                                                        Actions.PrintDatetimeInPanel(logedUser, "Online Pay Current Cart");
                                                        Console.WriteLine($"\n\tCart By This Id ({unPaidCart.Id}) Paid Online With This Card ({payCard.CardNumber}) \n\n\tIn {unPaidCart.Payment.PayDate} And You Will Receive It In Next Three Days ... ");
                                                        Actions.comeBackToMenu();
                                                        goto userMenu;
                                                    #endregion

                                                    #region In Person Pay
                                                    case ConsoleKey.B:
                                                        Actions.PrintDatetimeInPanel(logedUser, "InPerson Pay Current Cart");
                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).PaymentMethod = "InPerson";
                                                        dbPayCart.SaveChanges();
                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).EndPrice = unPaidCart.CalculateEndPrice();
                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).DeliveryDate = DateTime.Now.AddDays(3);
                                                        Console.WriteLine("\n\tYou Should Pay This Cart In Person \n\n\tIf You Don't Pay We Cant Recive Your Cart To You ....");
                                                        Console.WriteLine($"\n\n\tPay Price : {unPaidCart.EndPrice}\n\n\tDelivery Date : {unPaidCart.DeliveryDate}");
                                                        Actions.comeBackToMenu();
                                                        goto userMenu;
                                                    #endregion

                                                    #region Wallet Pay
                                                    case ConsoleKey.C:
                                                        Actions.PrintDatetimeInPanel(logedUser, "Pay Current Cart From Wallet");
                                                        Console.WriteLine($"Wallet Balance : {logedUser.WalletBalance} ");

                                                        if (logedUser.WalletBalance < unPaidCart.EndPrice)
                                                        {
                                                            Actions.PrintDatetimeInPanel(logedUser, "Warning!  ");
                                                            Console.WriteLine("\n\t* Your Wallet Balance Is Not Enough .... ");
                                                            Console.WriteLine("\n\tPlease Increase Your Wallet Balance ... ");
                                                            Thread.Sleep(2000);
                                                            goto userMenu;
                                                        }

                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).PaymentMethod = "Wallet";
                                                        dbPayCart.SaveChanges();
                                                        dbPayCart.Payments.Add(new Payment(dbPayCart.Carts.First(t => t.Id == unPaidCart.Id)));
                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).EndPrice = unPaidCart.CalculateEndPrice();
                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).IsPay = true;
                                                        dbPayCart.Carts.First(t => t.Id == unPaidCart.Id).DeliveryDate = DateTime.Now.AddDays(3);
                                                        dbPayCart.SaveChanges();
                                                        Console.WriteLine($"\n\tCart By This Id ({unPaidCart.Id}) Paid With Your Wallet In {unPaidCart.Payment.PayDate} \n\n\tAnd You Will Receive It In Next Three Days ... ");
                                                        Actions.comeBackToMenu();
                                                        goto userMenu;
                                                    #endregion

                                                    default:
                                                        goto choosePaymentMethod;
                                                }
                                            default:
                                                goto payCurrentCart;
                                        }
                                    }
                                #endregion

                                #region K : Search Product
                                case ConsoleKey.K:
                                searchProduct:
                                    using (OnlineShopDbContext dbSearchProduct = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "Search Product");
                                        Console.WriteLine("Search Product");
                                        Console.WriteLine("\n\tA. Search By Categoty");
                                        Console.WriteLine("\n\tB. Search By Model");
                                        Console.WriteLine("\n\tC. Search By Brand");
                                        ConsoleKeyInfo searchProductKey = Console.ReadKey();
                                        switch (searchProductKey.Key)
                                        {
                                            //Search By Category
                                            case ConsoleKey.A:
                                                Actions.PrintDatetimeInPanel(logedUser, "Search Product By Category Name");
                                                Console.Write("\n\tEnter Category Name : ");
                                                string categorySearch = Console.ReadLine();
                                                if (!dbSearchProduct.Products.Any(t => t.CategoryName.Contains(categorySearch)))
                                                {
                                                    Console.WriteLine($"\n\t*We Dont Find Any Product By This Categoty Name : {categorySearch}");
                                                    Thread.Sleep(3000);
                                                    goto userMenu;
                                                }
                                                foreach (Product product in dbSearchProduct.Products.Where(t => t.CategoryName.Contains(categorySearch)))
                                                {
                                                    Console.WriteLine("\n" + product);
                                                }
                                                Actions.comeBackToMenu();
                                                goto userMenu;

                                            //Search By Model
                                            case ConsoleKey.B:
                                                Actions.PrintDatetimeInPanel(logedUser, "Search Product By Model Name");
                                                Console.Write("\n\tEnter Model Name : ");
                                                string modelSearch = Console.ReadLine();
                                                if (!dbSearchProduct.Products.Any(t => t.ModelName.Contains(modelSearch)))
                                                {
                                                    Console.WriteLine($"\n\t*We Dont Find Any Product By This Model Name : {modelSearch}");
                                                    Thread.Sleep(3000);
                                                    goto userMenu;
                                                }
                                                foreach (Product product in dbSearchProduct.Products.Where(t => t.ModelName.Contains(modelSearch)))
                                                {
                                                    Console.WriteLine("\n" + product);
                                                }
                                                Actions.comeBackToMenu();
                                                goto userMenu;

                                            //Search By Brand
                                            case ConsoleKey.C:
                                                Actions.PrintDatetimeInPanel(logedUser, "Search Product By Brand Name");
                                                Console.Write("\n\tEnter Brand Name : ");
                                                string brandSearch = Console.ReadLine();
                                                if (!dbSearchProduct.Products.Any(t => t.BrandName.Contains(brandSearch)))
                                                {
                                                    Console.WriteLine($"\n\t*We Dont Find Any Product By This Brand Name : {brandSearch}");
                                                    Thread.Sleep(3000);
                                                    goto userMenu;
                                                }
                                                foreach (Product product in dbSearchProduct.Products.Where(t => t.BrandName.Contains(brandSearch)))
                                                {
                                                    Console.WriteLine("\n" + product);
                                                }
                                                Actions.comeBackToMenu();
                                                goto userMenu;

                                            default: goto searchProduct;
                                        }

                                    }
                                #endregion

                                #region L : See My ViewPoints
                                case ConsoleKey.L:
                                SeeMyViews:
                                    using (OnlineShopDbContext dbSeeMyViews = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "See My ViewPoints");
                                        Console.WriteLine("See My Views");
                                        Console.WriteLine("\n\tA. See My Comment");
                                        Console.WriteLine("\n\tB. See My DisLike");
                                        Console.WriteLine("\n\tC. See My Like");
                                        ConsoleKeyInfo seeViewsKey = Console.ReadKey();
                                        switch (seeViewsKey.Key)
                                        {
                                            // See My Comment
                                            case ConsoleKey.A:
                                                Actions.PrintDatetimeInPanel(logedUser, "see My Comments");
                                                Console.WriteLine("My Comments ");
                                                if (!dbSeeMyViews.Comments.Any(t => t.User.Id == logedUser.Id))
                                                {
                                                    Console.WriteLine("\n\tYou Dont Registered Any Comment");
                                                    Thread.Sleep(2500);
                                                    goto userMenu;
                                                }
                                                foreach (Comment comment in dbSeeMyViews.Comments.Where(t => t.User.Id == logedUser.Id))
                                                {
                                                    Console.WriteLine(comment);
                                                    Console.WriteLine("________________________________________________________________________________________");
                                                }
                                                Actions.comeBackToMenu();
                                                goto userMenu;

                                            // See My DisLike
                                            case ConsoleKey.B:
                                                Actions.PrintDatetimeInPanel(logedUser, "see My DisLikes");
                                                Console.WriteLine("My DisLike ");
                                                if (!dbSeeMyViews.DisLikes.Any(t => t.User.Id == logedUser.Id))
                                                {
                                                    Console.WriteLine("\n\tYou Dont Registered Any DisLike");
                                                    Thread.Sleep(2500);
                                                    goto userMenu;
                                                }
                                                foreach (DisLike disLike in dbSeeMyViews.DisLikes.Where(t => t.User.Id == logedUser.Id))
                                                {
                                                    Console.WriteLine(disLike);
                                                    Console.WriteLine("________________________________________________________________________________________");
                                                }
                                                Actions.comeBackToMenu();
                                                goto userMenu;

                                            // See My Like
                                            case ConsoleKey.C:
                                                Actions.PrintDatetimeInPanel(logedUser, "see My Likes");
                                                Console.WriteLine("My Like ");
                                                if (!dbSeeMyViews.Likes.Any(t => t.User.Id == logedUser.Id))
                                                {
                                                    Console.WriteLine("\n\tYou Dont Registered Any Like");
                                                    Thread.Sleep(2500);
                                                    goto userMenu;
                                                }
                                                foreach (Like like in dbSeeMyViews.Likes.Where(t => t.User.Id == logedUser.Id))
                                                {
                                                    Console.WriteLine(like);
                                                    Console.WriteLine("________________________________________________________________________________________");
                                                }
                                                Actions.comeBackToMenu();
                                                goto userMenu;

                                            default: goto SeeMyViews;
                                        }
                                    }
                                #endregion

                                #region M : Comment For Product
                                case ConsoleKey.M:
                                commnetForProduct:
                                    using (OnlineShopDbContext dbCommentForProduct = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "Comment For Product");
                                        Console.WriteLine("Comment For Product");
                                        Actions.showAllProduct();
                                        Console.Write("\n\tEnter Product Id For Register Comment For Its : ");
                                        int productId;
                                        while (!int.TryParse(Console.ReadLine(), out productId))
                                        {
                                            Actions.warningNumber();
                                            goto commnetForProduct;
                                        }
                                        if (!dbCommentForProduct.Products.Any(t => t.Id == productId))
                                        {
                                            Actions.warningNotFound("Product", productId, "Product List");
                                            goto userMenu;
                                        }
                                        Product productFind = dbCommentForProduct.Products.First(t => t.Id == productId);
                                        Actions.PrintDatetimeInPanel(logedUser, "Comment For Product");
                                        Console.WriteLine($"Comment for {productFind.ModelName}");
                                        Console.Write("\n\tEnter Commnet Text : ");
                                        string commentText = Console.ReadLine();
                                        dbCommentForProduct.Comments.Add(new Comment(dbCommentForProduct.Users.First(t => t.Id == logedUser.Id), dbCommentForProduct.Products.First(t => t.Id == productId), commentText));
                                        dbCommentForProduct.SaveChanges();
                                        Actions.PrintDatetimeInPanel(logedUser, "Comment For Product");
                                        Console.WriteLine($"\n\tRegister Comment For {productFind.ModelName} With This Text : \n\n\t{commentText}\n\n\tIf Admin Confrim Your Comment We Show Its In Comment List.");
                                        Actions.comeBackToMenu();
                                        goto userMenu;
                                    }
                                #endregion

                                #region N : Like Product
                                case ConsoleKey.N:
                                likeProduct:
                                    using (OnlineShopDbContext dbLikeProduct = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "Like Product");
                                        Console.WriteLine("Like For Product");
                                        Actions.showAllProduct();
                                        Console.Write("\n\tEnter Product Id For Register Like For Its : ");
                                        int productId;
                                        while (!int.TryParse(Console.ReadLine(), out productId))
                                        {
                                            Actions.warningNumber();
                                            goto likeProduct;
                                        }
                                        if (!dbLikeProduct.Products.Any(t => t.Id == productId))
                                        {
                                            Actions.warningNotFound("Product", productId, "Product List");
                                            goto userMenu;
                                        }
                                        Product productFind = dbLikeProduct.Products.First(t => t.Id == productId);
                                        dbLikeProduct.Likes.Add(new Like(dbLikeProduct.Users.First(t => t.Id == logedUser.Id), dbLikeProduct.Products.First(t => t.Id == productFind.Id)));
                                        dbLikeProduct.SaveChanges();
                                        Actions.PrintDatetimeInPanel(logedUser, "Like Product");
                                        Console.WriteLine($"\n\tRegister Like For {productFind.ModelName}");
                                        Thread.Sleep(2000);
                                        goto userMenu;
                                    }
                                #endregion

                                #region O : DisLike Product
                                case ConsoleKey.O:
                                disLikeProduct:
                                    using (OnlineShopDbContext dbDisLikeProduct = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {
                                        Actions.PrintDatetimeInPanel(logedUser, "DisLike Product");
                                        Console.WriteLine("Like For Product");
                                        Actions.showAllProduct();
                                        Console.Write("\n\tEnter Product Id For Register Like For Its : ");
                                        int productId;
                                        while (!int.TryParse(Console.ReadLine(), out productId))
                                        {
                                            Actions.warningNumber();
                                            goto disLikeProduct;
                                        }
                                        if (!dbDisLikeProduct.Products.Any(t => t.Id == productId))
                                        {
                                            Actions.warningNotFound("Product", productId, "Product List");
                                            goto userMenu;
                                        }
                                        Product productFind = dbDisLikeProduct.Products.First(t => t.Id == productId);
                                        dbDisLikeProduct.DisLikes.Add(new DisLike(dbDisLikeProduct.Users.First(t => t.Id == logedUser.Id), dbDisLikeProduct.Products.First(t => t.Id == productFind.Id)));
                                        dbDisLikeProduct.SaveChanges();
                                        Actions.PrintDatetimeInPanel(logedUser, "DisLike Product");
                                        Console.WriteLine($"\n\tRegister DisLike For {productFind.ModelName}");
                                        Thread.Sleep(2000);
                                        goto userMenu;
                                    }
                                #endregion

                                #region P : Best Sellers
                                //*****************************************************************
                                //In This Section, The Best-Selling Products Are Shown According To The |SalesNumber| In 'Product' Class
                                //*****************************************************************
                                case ConsoleKey.P:
                                    using (OnlineShopDbContext dbBestSeller = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {

                                        Actions.PrintDatetimeInPanel(logedUser, "Best Sellers");
                                        Console.WriteLine("Best Sellers");
                                        foreach (Product product in dbBestSeller.Products.Where(t => t.IsActive == true && t.SalesNumber < 20))
                                        {
                                            Console.WriteLine(product);
                                        }
                                        Actions.comeBackToMenu();
                                        goto userMenu;
                                    }
                                #endregion

                                #region Q : User account information
                                case ConsoleKey.Q:
                                userAccountInformation:
                                    using (OnlineShopDbContext dbUserAccountInformation = new OnlineShopDbContext("onlineshopdbcontext"))
                                    {

                                        Actions.PrintDatetimeInPanel(logedUser, "User account information");
                                        Console.Write("User Account Information");
                                        Console.WriteLine("\n\tA. Edit My Information ");
                                        Console.WriteLine($"\n\tB. Increase Wallet Balance (Wallet Balance : {logedUser.WalletBalance})");
                                        Console.WriteLine("\n\n\tPress the Corresponding Key.");
                                        ConsoleKeyInfo userAccountInformationKey = Console.ReadKey();
                                        switch (userAccountInformationKey.Key)
                                        {
                                            #region Edit My Information
                                            case ConsoleKey.A:
                                            editMyInformation:
                                                Actions.PrintDatetimeInPanel(logedUser, "Edit My Information");
                                                Console.WriteLine("Edit My Information");
                                                Console.WriteLine("\n\tA. Edit My Name");
                                                Console.WriteLine("\n\tB. Edit My Family");
                                                Console.WriteLine("\n\tC. Edit My Password");
                                                Console.WriteLine("\n\tD. Edit My Address");
                                                Console.WriteLine("\n\n\tPress the Corresponding Key.");
                                                ConsoleKeyInfo editMyInformationKey = Console.ReadKey();
                                                switch (editMyInformationKey.Key)
                                                {
                                                    //Edit Name
                                                    case ConsoleKey.A:
                                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Name");
                                                        Console.WriteLine($"\n\tYour Previous Name : {logedUser.Name}");
                                                        Console.Write("Enter New Name : ");
                                                        string newName = Console.ReadLine();
                                                        dbUserAccountInformation.Users.First(t => t.Id == logedUser.Id).Name = newName;
                                                        dbUserAccountInformation.SaveChanges();
                                                        logedUser.Name = newName;
                                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Name");
                                                        Console.WriteLine($"\n\tYour Name Change To {newName}");
                                                        Thread.Sleep(2000);
                                                        goto userMenu;

                                                    //Edit Family
                                                    case ConsoleKey.B:
                                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Family");
                                                        Console.WriteLine($"\n\tYour Previous Family : {logedUser.Family}");
                                                        Console.Write("Enter New Family : ");
                                                        string newFamily = Console.ReadLine();
                                                        dbUserAccountInformation.Users.First(t => t.Id == logedUser.Id).Family = newFamily;
                                                        dbUserAccountInformation.SaveChanges();
                                                        logedUser.Family = newFamily;
                                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Family");
                                                        Console.WriteLine($"\n\tYour Family Change To {newFamily}");
                                                        Thread.Sleep(2000);
                                                        goto userMenu;

                                                    //Edit Password
                                                    case ConsoleKey.C:
                                                    editPassword:
                                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Password");
                                                        Console.WriteLine($"\n\tYour Previous Password : {logedUser.Password}");
                                                        Console.Write("Enter New Password : ");
                                                        string newPassword = Console.ReadLine();
                                                        Console.Write("Enter Repaet Password : ");
                                                        if (Regex.IsMatch(Console.ReadLine(), newPassword))
                                                        {
                                                            Console.WriteLine("\n\tRepeat Password Is False...\n\n\tTry Again");
                                                            Thread.Sleep(2000);
                                                            Actions.PrintDatetimeInPanel(logedUser, "Warning! ");
                                                            goto editPassword;
                                                        }
                                                        dbUserAccountInformation.Users.First(t => t.Id == logedUser.Id).Password = newPassword;
                                                        dbUserAccountInformation.SaveChanges();
                                                        logedUser.Password = newPassword;
                                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Password");
                                                        Console.WriteLine($"\n\tYour Password Change To {newPassword}");
                                                        Thread.Sleep(2000);
                                                        goto userMenu;

                                                    //Edit Address
                                                    case ConsoleKey.D:
                                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Address");
                                                        Console.WriteLine($"\n\tYour Previous Address : \n\n\t{logedUser.Address}");
                                                        Console.Write("Enter New Adress : ");
                                                        string newAddress = Console.ReadLine();
                                                        dbUserAccountInformation.Users.First(t => t.Id == logedUser.Id).Address = newAddress;
                                                        dbUserAccountInformation.SaveChanges();
                                                        logedUser.Address = newAddress;
                                                        Actions.PrintDatetimeInPanel(logedUser, "Edit My Address");
                                                        Console.WriteLine($"\n\tYour Address Change To {newAddress}");
                                                        Thread.Sleep(2000);
                                                        goto userMenu;

                                                    default:
                                                        goto editMyInformation;
                                                }
                                            #endregion

                                            #region Increase Wallet Balance
                                            case ConsoleKey.B:
                                            IncreaseWalletBalance:
                                                Actions.PrintDatetimeInPanel(logedUser, "Increase Wallet Balance");
                                                Console.WriteLine("Increase Wallet Balance");
                                                Console.WriteLine("\n\tA. 50000 $");
                                                Console.WriteLine("\n\tB. 75000 $");
                                                Console.WriteLine("\n\tC. 100000 $");
                                                Console.WriteLine("\n\tD. 150000 $");
                                                Console.WriteLine("\n\tE. 200000 $");
                                                Console.WriteLine("\n\tF. 300000 $");
                                                Console.WriteLine("\n\tG. 400000 $");
                                                Console.WriteLine("\n\tH. 500000 $");
                                                Console.WriteLine("\n\tI. 1000000 $");
                                                Console.WriteLine("\n\tJ. Other Amount $");
                                                Console.WriteLine("\n\n\tPress the Corresponding Key.");
                                                ConsoleKeyInfo increaseBalanceKey = Console.ReadKey();
                                                int amountIncrease;
                                                switch (increaseBalanceKey.Key)
                                                {
                                                    case ConsoleKey.A:
                                                        amountIncrease = 50000;
                                                        break;
                                                    case ConsoleKey.B:
                                                        amountIncrease = 75000;
                                                        break;
                                                    case ConsoleKey.C:
                                                        amountIncrease = 100000;
                                                        break;
                                                    case ConsoleKey.D:
                                                        amountIncrease = 150000;
                                                        break;
                                                    case ConsoleKey.E:
                                                        amountIncrease = 200000;
                                                        break;
                                                    case ConsoleKey.F:
                                                        amountIncrease = 300000;
                                                        break;
                                                    case ConsoleKey.G:
                                                        amountIncrease = 400000;
                                                        break;
                                                    case ConsoleKey.H:
                                                        amountIncrease = 500000;
                                                        break;
                                                    case ConsoleKey.I:
                                                        amountIncrease = 1000000;
                                                        break;

                                                    case ConsoleKey.J:
                                                    otherAmount:
                                                        Actions.PrintDatetimeInPanel(logedUser, "Increase Wallet Balance");
                                                        Console.Write("\n\n\t*Enter Amount : ");
                                                        while (!int.TryParse(Console.ReadLine(), out amountIncrease))
                                                        {
                                                            Actions.warningNumber();
                                                            goto otherAmount;
                                                        }
                                                        break;

                                                    default:
                                                        goto IncreaseWalletBalance;
                                                }
                                            choosePayCard:
                                                Actions.PrintDatetimeInPanel(logedUser, "Increase Wallet Balance");
                                                if (dbUserAccountInformation.Users.First(t => t.Id == logedUser.Id).Cards.ToList().Count == 0)
                                                {
                                                    Console.WriteLine("\n\tYou Dont Have Any Card ..");
                                                    Console.WriteLine("\n\tYou Should Add New Card.");
                                                    Thread.Sleep(2000);
                                                    goto userMenu;
                                                }
                                                foreach (Card card in dbUserAccountInformation.Users.First(t => t.Id == logedUser.Id).Cards.ToList())
                                                {
                                                    Console.WriteLine(card);
                                                }
                                                Console.Write("\n\tEnter Card ID:");
                                                int cardId;
                                                while (!int.TryParse(Console.ReadLine(), out cardId))
                                                {
                                                    Actions.warningNumber();
                                                    goto choosePayCard;
                                                }
                                                if (!dbUserAccountInformation.Cards.Any(t => t.Id == cardId))
                                                {
                                                    Actions.warningNotFound("Card", cardId, "Card List");
                                                    goto userMenu;
                                                }
                                                Card payCard = dbUserAccountInformation.Cards.First(t => t.Id == cardId);
                                                Actions.PrintDatetimeInPanel(logedUser, "Increase Wallet Balance");
                                                Console.Write("\n\tEnter Cvv2 : ");
                                                string cvv2 = Console.ReadLine();
                                                Console.Write("\n\tEnter Second Password : ");
                                                string secondPassword = Console.ReadLine();
                                                Console.Write("\n\tEnter ExpirationDate (--/--) : ");
                                                string expirationDate = Console.ReadLine();
                                                if (!Regex.IsMatch(payCard.SecondPassword, secondPassword) && Regex.IsMatch(payCard.Cvv2, cvv2) && Regex.IsMatch(payCard.ExpirationDate, expirationDate))
                                                {
                                                    Actions.PrintDatetimeInPanel(logedUser, "Warning! ");
                                                    Console.WriteLine("\n\tThe Information Entered Is Incorrect");
                                                }
                                                dbUserAccountInformation.Users.First(t => t.Id == logedUser.Id).WalletBalance += amountIncrease;
                                                dbUserAccountInformation.SaveChanges();
                                                logedUser.WalletBalance += amountIncrease;
                                                Actions.PrintDatetimeInPanel(logedUser, "Increase Wallet Balance");
                                                Console.WriteLine($"\n\tWallet Balance {amountIncrease} Increase With This Card {payCard.CardNumber}");
                                                Actions.comeBackToMenu();
                                                goto userMenu;
                                            #endregion

                                            default:
                                                goto userAccountInformation;
                                        }
                                    }

                                #endregion

                                #region ESC : Exit Panel
                                case ConsoleKey.Escape:
                                    goto menuKey;
                                #endregion

                                #region Default
                                default:
                                    goto userMenu;
                                    #endregion
                            }
                        #endregion

                        default: goto login;
                    }
                #endregion

                #region Register User
                case ConsoleKey.R:
                registerUser:
                    using (OnlineShopDbContext dbUserRegister = new OnlineShopDbContext("onlineshopdbcontext"))
                    {
                        Actions.printDatetime();
                        Console.Write("\n\tEnter Name : ");
                        string userName = Console.ReadLine();
                        Console.Write("\n\tEnter Family : ");
                        string userFamily = Console.ReadLine();
                    enterNationalCode:
                        Console.Write("\n\tEnter National Code : ");
                        string userNationalCode = Console.ReadLine();
                        while (!Regex.IsMatch(userNationalCode, validNationalCode))
                        {
                            Actions.printDatetime();
                            Console.WriteLine("\n\tNational Code Is False ....");
                            Thread.Sleep(2000);
                            Console.WriteLine($"\n\tEnter Name : {userName}");
                            Console.WriteLine($"\n\tEnter Family : {userFamily}");
                            goto enterNationalCode;
                        }
                        if (adminsUsers.Any(t => t.nationalCode == userNationalCode))
                        {
                            Actions.printDatetime();
                            Console.WriteLine("\n\tThere Is A User By This NationalCode");
                            Thread.Sleep(2000);
                            goto menuKey;
                        }

                    enterMobileNumber:
                        Console.Write("\n\tEnter mobile number : ");
                        string userMobileNumber = Console.ReadLine();
                        while (!Regex.IsMatch(userMobileNumber, validMobile))
                        {
                            Actions.printDatetime();
                            Console.WriteLine("\n\tNational Code Is False ....");
                            Thread.Sleep(2000);
                            Actions.printDatetime();
                            Console.Write($"\n\tEnter Name : {userName}");
                            Console.Write($"\n\tEnter Family : {userFamily}");
                            Console.Write($"\n\tEnter National Code : {userNationalCode}");
                            goto enterMobileNumber;
                        }
                        if (adminsUsers.Any(t => t.mobile == userMobileNumber))
                        {
                            Actions.printDatetime();
                            Console.WriteLine("\n\tThere Is A User By This Mobile");
                            Thread.Sleep(2000);
                            goto menuKey;
                        }
                    EnterUserPassword:
                        Console.Write("\n\tEnter password : ");
                        string userPassword = Console.ReadLine();
                        Console.Write("\n\tEnter Repeat Passwrod : ");
                        string repeatPassword = Console.ReadLine();
                        if (userPassword != repeatPassword)
                        {
                            Actions.printDatetime();
                            Console.WriteLine("\n\tPassword With It's Repeat Is Incorrect ... ");
                            Thread.Sleep(2000);
                            Actions.printDatetime();
                            Console.Write($"\n\tEnter Name : {userName}");
                            Console.Write($"\n\tEnter Family : {userFamily}");
                            Console.Write($"\n\tEnter National Code : {userNationalCode}");
                            Console.Write($"\n\tEnter Moblie : {userMobileNumber}");
                            goto EnterUserPassword;
                        }
                        Console.Write("\n\tEnter Your Address : ");
                        string userAddress = Console.ReadLine();
                        dbUserRegister.Users.Add(new User(userName, userFamily, userNationalCode, userMobileNumber, userPassword, dbUserRegister.Roles.First(t => t.Name == "user"))).Address = userAddress;
                        dbUserRegister.SaveChanges();
                        Actions.printDatetime();
                        Console.WriteLine($"\n\tRegistered a User With This Information : \n\n\n\tName : {userName} {userFamily}\n\n\tNational Code : {userNationalCode}\n\n\tMobile : {userMobileNumber}\n\n\tPassword : {userPassword}\n\n\tAddress : {userAddress}");
                        Console.WriteLine("\n\n\n\tPress Any Key For Go To Login Panel ....");
                        Console.ReadKey();

                        //Log For SignUp User
                        using (StreamWriter log = new StreamWriter("User_SignUp.json", true, Encoding.UTF8))
                        {
                            string logMessage = $"(Login)(User) {userName} {userFamily} With This Mobile ({userMobileNumber}) Loged In {DateTime.Now} In Register User.";
                            string write = JsonConvert.SerializeObject(logMessage);
                            log.WriteLine("\n" + write);
                        }
                        goto login;
                    }
                #endregion

                default: goto menuKey;
            }
        }
    }
}
