using Data.Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace Library.Tests
{
    internal static class UnitTestHelper
    {
        public static DbContextOptions<TradeMarketDbContext> GetUnitTestDbOptions()
        {
            var options = new DbContextOptionsBuilder<TradeMarketDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using (var context = new TradeMarketDbContext(options))
            {
                SeedData(context);
            }

            return options;
        }

        public static void SeedData(TradeMarketDbContext context)
        {
            context.Persons.AddRange(
                new Person { Id = 1, Name = "Han", Surname = "Solo", BirthDate = new DateTime(1942, 7, 13) },
                new Person { Id = 2, Name = "Ethan", Surname = "Hunt", BirthDate = new DateTime(1964, 8, 18) });
            context.Customers.AddRange(
                new Customer { Id = 1, PersonId = 1, DiscountValue = 20 },
                new Customer { Id = 2, PersonId = 2, DiscountValue = 10 });
            context.ProductCategories.AddRange(
                new ProductCategory { Id = 1, CategoryName = "Dairy products" },
                new ProductCategory { Id = 2, CategoryName = "Fruit juices" });
            context.Products.AddRange(
                new Product { Id = 1, ProductCategoryId = 1, ProductName = "Milk", Price = 40 },
                new Product { Id = 2, ProductCategoryId = 2, ProductName = "Orange juice", Price = 20 });
            context.Receipts.AddRange(
                new Receipt { Id = 1, CustomerId = 1, OperationDate = new DateTime(2021, 7, 5), IsCheckedOut = true },
                new Receipt { Id = 2, CustomerId = 1, OperationDate = new DateTime(2021, 8, 10), IsCheckedOut = true },
                new Receipt { Id = 3, CustomerId = 2, OperationDate = new DateTime(2021, 10, 15), IsCheckedOut = false });
            context.ReceiptsDetails.AddRange(
                new ReceiptDetail { Id = 1, ReceiptId = 1, ProductId = 1, UnitPrice = 40, DiscountUnitPrice = 32, Quantity = 3 },
                new ReceiptDetail { Id = 2, ReceiptId = 1, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 16, Quantity = 1 },
                new ReceiptDetail { Id = 3, ReceiptId = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 32, Quantity = 2 },
                new ReceiptDetail { Id = 4, ReceiptId = 3, ProductId = 1, UnitPrice = 40, DiscountUnitPrice = 36, Quantity = 2 },
                new ReceiptDetail { Id = 5, ReceiptId = 3, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 18, Quantity = 5 });
            context.SaveChanges();
        }
    }
}
