using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineShop
{
    class OnlineShopDbContext : DbContext
    {
        public OnlineShopDbContext(string name) : base(name) { }


        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<DisLike> DisLikes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<DisCount> DisCounts { get; set; }
        public DbSet<AdminNotification> AdminNotifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>().ToTable("Table_Role");

            modelBuilder.Entity<Role>().Property(t => t.Name)
                                                .HasMaxLength(30)
                                                .IsRequired()
                                                .HasColumnType("varchar");

            modelBuilder.Properties().Where(t => t.Name == "Id").Configure(t => t.IsKey());
            modelBuilder.Properties().Where(t => t.Name == "Name").Configure(t => t.HasMaxLength(30).IsRequired().HasColumnType("varchar"));
            modelBuilder.Properties().Where(t => t.Name == "Family").Configure(t => t.HasMaxLength(40).IsRequired().HasColumnType("varchar"));
            modelBuilder.Properties().Where(t => t.Name == "Mobile").Configure(t => t.HasMaxLength(11).IsRequired().HasColumnType("varchar"));
            modelBuilder.Properties().Where(t => t.Name == "Password").Configure(t => t.HasMaxLength(40).IsRequired().HasColumnType("varchar"));
            modelBuilder.Properties().Where(t => t.Name == "NationalCode").Configure(t => t.HasMaxLength(10).IsRequired().HasColumnType("varchar"));
            modelBuilder.Properties().Where(t => t.Name == "RegisterDate").Configure(t => t.IsRequired());
            modelBuilder.Properties().Where(t => t.Name == "IsActive").Configure(t => t.IsRequired());


            modelBuilder.Entity<User>().ToTable("Table_User");
            modelBuilder.Entity<User>().Property(t => t.Address)
                                                .HasMaxLength(500)
                                                .HasColumnType("varchar");
            modelBuilder.Entity<User>().Property(t => t.WalletBalance)
                                                .IsRequired();


            modelBuilder.Entity<Admin>().ToTable("Table_Admin");
            modelBuilder.Entity<Admin>().Property(t => t.Department)
                                                  .HasMaxLength(40)
                                                  .IsRequired()
                                                  .HasColumnType("varchar");


            modelBuilder.Entity<Product>().ToTable("Table_Product");
            modelBuilder.Entity<Product>().Property(t => t.BrandName)
                                                    .HasMaxLength(30)
                                                    .IsRequired()
                                                    .HasColumnType("varchar");
            modelBuilder.Entity<Product>().Property(t => t.CategoryName)
                                                  .HasMaxLength(30)
                                                  .IsRequired()
                                                  .HasColumnType("varchar");
            modelBuilder.Entity<Product>().Property(t => t.ModelName)
                                                   .HasMaxLength(50)
                                                   .IsRequired()
                                                   .HasColumnType("varchar");
            modelBuilder.Entity<Product>().Property(t => t.Description)
                                                   .HasMaxLength(500)
                                                   .IsRequired()
                                                   .HasColumnType("varchar");
            modelBuilder.Entity<Product>().Property(t => t.Price)
                                                   .IsRequired();
            modelBuilder.Entity<Product>().Property(t => t.SalesNumber)
                                                   .IsRequired();
            modelBuilder.Entity<Product>().Property(t => t.Inventory)
                                                   .IsRequired();
            modelBuilder.Entity<Product>().Property(t => t.InventoryStatus)
                                                   .IsRequired();
            modelBuilder.Entity<Product>().Property(t => t.IsActive)
                                                   .IsRequired();
            modelBuilder.Entity<Product>().Property(t => t.EditDate)
                                                  .IsOptional();
            modelBuilder.Entity<Product>().Property(t => t.RegisterDate)
                                                  .IsRequired();

            modelBuilder.Entity<Cart>().ToTable("Table_Cart");
            modelBuilder.Entity<Cart>().Property(t => t.RegisterDate)
                                                 .IsRequired();
            modelBuilder.Entity<Cart>().Property(t => t.IsActive)
                                                 .IsRequired();
            modelBuilder.Entity<Cart>().Property(t => t.IsPay)
                                                 .IsRequired();
            modelBuilder.Entity<Cart>().Property(t => t.PaymentMethod)
                                                .IsOptional();
            modelBuilder.Entity<Cart>().Property(t => t.DeliveryDate)
                                                .IsOptional();
            modelBuilder.Entity<Cart>().Property(t => t.EndPrice)
                                                .IsOptional();

            modelBuilder.Entity<Order>().ToTable("Table_Order");
            modelBuilder.Entity<Order>().Property(t => t.NumberOfProduct)
                                                .IsRequired();


            modelBuilder.Entity<Card>().ToTable("Table_Card");
            modelBuilder.Entity<Card>().Property(t => t.CardNumber)
                                                .HasMaxLength(16)
                                                .IsRequired()
                                                .HasColumnType("varchar");
            modelBuilder.Entity<Card>().Property(t => t.OwnerName)
                                                .HasMaxLength(30)
                                                .IsRequired()
                                                .HasColumnType("varchar");
            modelBuilder.Entity<Card>().Property(t => t.SecondPassword)
                                                .HasMaxLength(16)
                                                .IsRequired()
                                                .HasColumnType("varchar");
            modelBuilder.Entity<Card>().Property(t => t.Cvv2)
                                                .HasMaxLength(4)
                                                .IsRequired()
                                                .HasColumnType("varchar");
            modelBuilder.Entity<Card>().Property(t => t.IsActive)
                                                .IsRequired();
            modelBuilder.Entity<Card>().Property(t => t.ExpirationDate)
                                                .IsRequired();

            modelBuilder.Entity<UserNotification>().ToTable("Table_User_Notification");
            modelBuilder.Entity<UserNotification>().Property(t => t.IsActive)
                                                        .IsRequired();
            modelBuilder.Entity<UserNotification>().Property(t => t.IsRead)
                                                        .IsRequired();
            modelBuilder.Entity<UserNotification>().Property(t => t.Title)
                                                        .IsRequired()
                                                        .HasColumnType("varchar")
                                                        .HasMaxLength(50);
            modelBuilder.Entity<UserNotification>().Property(t => t.Text)
                                                       .IsRequired()
                                                       .HasColumnType("varchar")
                                                       .HasMaxLength(500);
            modelBuilder.Entity<UserNotification>().Property(t => t.RegisterDate)
                                                       .IsRequired();

            modelBuilder.Entity<AdminNotification>().ToTable("Table_Admin_Notification");
            modelBuilder.Entity<AdminNotification>().Property(t => t.IsActive)
                                                       .IsRequired();
            modelBuilder.Entity<AdminNotification>().Property(t => t.IsRead)
                                                        .IsRequired();
            modelBuilder.Entity<AdminNotification>().Property(t => t.Title)
                                                        .IsRequired()
                                                        .HasColumnType("varchar")
                                                        .HasMaxLength(50);
            modelBuilder.Entity<AdminNotification>().Property(t => t.Text)
                                                       .IsRequired()
                                                       .HasColumnType("varchar")
                                                       .HasMaxLength(500);
            modelBuilder.Entity<AdminNotification>().Property(t => t.RegisterDate)
                                                       .IsRequired();


            modelBuilder.Entity<Payment>().ToTable("Table_Payment");
            modelBuilder.Entity<Payment>().Property(t => t.PayDate)
                                                   .IsRequired();
            modelBuilder.Entity<Payment>().Property(t => t.PayPrice)
                                                   .IsRequired();
            modelBuilder.Entity<Payment>().Property(t => t.PaymentMethod)
                                                  .IsRequired()
                                                  .HasColumnType("varchar")
                                                  .HasMaxLength(10);


            modelBuilder.Entity<Like>().ToTable("Table_Like");


            modelBuilder.Entity<DisLike>().ToTable("Table_DisLike");


            modelBuilder.Entity<Comment>().ToTable("Table_Comment");
            modelBuilder.Entity<Comment>().Property(t => t.Text)
                                                   .HasMaxLength(1000)
                                                   .IsRequired()
                                                   .HasColumnType("varchar");
            modelBuilder.Entity<Comment>().Property(t => t.Confrimation)
                                                   .IsRequired();
            modelBuilder.Entity<Comment>().Property(t => t.ReportCount)
                                                   .IsRequired();

            modelBuilder.Entity<DisCount>().ToTable("Table_Discount");
            modelBuilder.Entity<DisCount>().Property(t => t.IsActive)
                                                    .IsRequired();
            modelBuilder.Entity<DisCount>().Property(t => t.Precent)
                                                    .IsRequired()
                                                    .HasColumnType("float");
            modelBuilder.Entity<DisCount>().Property(t => t.HourDuration)
                                                   .IsRequired();
            modelBuilder.Entity<DisCount>().Property(t => t.ExpirationDate)
                                                   .IsRequired();
            modelBuilder.Entity<DisCount>().Property(t => t.RegisterDate)
                                                   .IsRequired();




            modelBuilder.Entity<Role>()
                                     .HasMany(t => t.Users)
                                     .WithRequired(t => t.Role)
                                     .WillCascadeOnDelete(false);
            modelBuilder.Entity<Role>()
                                  .HasMany(t => t.Admins)
                                  .WithRequired(t => t.Role)
                                  .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                                     .HasMany(t => t.Likes);
            modelBuilder.Entity<User>()
                                     .HasMany(t => t.DisLikes);
            modelBuilder.Entity<User>()
                                     .HasMany(t => t.Comments);
            modelBuilder.Entity<User>()
                                      .HasMany(t => t.AllCart)
                                      .WithRequired(t => t.User)
                                      .WillCascadeOnDelete(true);

            modelBuilder.Entity<User>()
                                      .HasMany(t => t.MyNotification);


            modelBuilder.Entity<Admin>()
                                      .HasMany(t => t.RegisterProduct);
            modelBuilder.Entity<Admin>()
                                      .HasMany(t => t.EditProduct);
            modelBuilder.Entity<Admin>()
                                      .HasMany(t => t.RegistersDiscount)
                                      .WithRequired(t => t.Registrar)
                                      .WillCascadeOnDelete(false);
            modelBuilder.Entity<Admin>()
                                      .HasMany(t => t.CommentsApproved);
            modelBuilder.Entity<Admin>()
                                      .HasMany(t => t.RegisterNotifications);
            modelBuilder.Entity<Admin>()
                                      .HasMany(t => t.MyNotifications);



            modelBuilder.Entity<Product>()
                                        .HasRequired(t => t.Registrar)
                                        .WithMany(t => t.RegisterProduct);
            modelBuilder.Entity<Product>()
                                        .HasOptional(t => t.Editor);
            modelBuilder.Entity<Product>()
                                        .HasMany(t => t.Likes);
            modelBuilder.Entity<Product>()
                                        .HasMany(t => t.DisLikes);
            modelBuilder.Entity<Product>()
                                        .HasMany(t => t.Comments);
            modelBuilder.Entity<Product>()
                                        .HasMany(t => t.DisCount);                                                    


            modelBuilder.Entity<Cart>()
                                     .HasRequired(t => t.User)
                                     .WithMany(t => t.AllCart);
            modelBuilder.Entity<Cart>()
                                     .HasOptional(t => t.Payment)
                                     .WithRequired(t => t.Cart)
                                     .WillCascadeOnDelete(false);
            modelBuilder.Entity<Cart>()
                                     .HasMany(t => t.Orders);

            modelBuilder.Entity<Order>()
                                     .HasRequired(t => t.Cart);
            modelBuilder.Entity<Order>()
                                     .HasRequired(t => t.Product);


            modelBuilder.Entity<Like>()
                                     .HasRequired(t => t.Product);
            modelBuilder.Entity<Like>()
                                     .HasRequired(t => t.User);


            modelBuilder.Entity<DisLike>()
                                        .HasRequired(t => t.Product);
            modelBuilder.Entity<DisLike>()
                                        .HasRequired(t => t.User);


            modelBuilder.Entity<Comment>()
                                        .HasRequired(t => t.Product);
            modelBuilder.Entity<Comment>()
                                        .HasRequired(t => t.User);
            modelBuilder.Entity<Comment>()
                                        .HasOptional(t => t.Seconder);


            modelBuilder.Entity<Payment>()
                                        .HasRequired(t => t.Cart);
            modelBuilder.Entity<Payment>()
                                        .HasOptional(t => t.Card);


            modelBuilder.Entity<DisCount>()
                                         .HasRequired(t => t.Product);

            modelBuilder.Entity<DisCount>()
                                         .HasRequired(t => t.Registrar);


            modelBuilder.Entity<Card>()
                                     .HasRequired(t => t.Owner);
            modelBuilder.Entity<Card>()
                                     .HasMany(t => t.Payments);

            modelBuilder.Entity<UserNotification>()
                                                 .HasRequired(t => t.Registrar);
            modelBuilder.Entity<UserNotification>()
                                                 .HasRequired(t => t.User);

            modelBuilder.Entity<AdminNotification>()
                                                  .HasRequired(t => t.Admin);


            base.OnModelCreating(modelBuilder);
        }
    }
}
