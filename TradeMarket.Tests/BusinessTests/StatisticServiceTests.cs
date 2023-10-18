using Data.Entities;
using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Interfaces;
using Moq;
using System.Linq;
using Business.Models;
using Business.Services;
using Library.Tests;

namespace TradeMarket.Tests.BusinessTests
{
    public class StatisticServiceTests
    {
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public async Task StatisticServiceGetMostPopularProductsReturnsMostPopularProductsOrderedBySalesCount(int productsCount)
        {
            //arrange 
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptDetailRepository.GetAllWithDetailsAsync()).ReturnsAsync(ReceiptDetailEntities.AsEnumerable());
            var statisticService = new StatisticService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await statisticService.GetMostPopularProductsAsync(productsCount);

            //assert
            var expected = ExpectedMostPopularProducts.Take(productsCount);
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.ReceiptDetailIds));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task StatisticServiceGetMostValuableCustomersAsyncReturnsCustomersOrderedByAllReceiptsSum(int customerCount)
        {
            //arrange 
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetAllWithDetailsAsync()).ReturnsAsync(ReceiptEntities.AsEnumerable());
            var statisticService = new StatisticService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await statisticService.GetMostValuableCustomersAsync(customerCount, new DateTime(1986, 1, 1), new DateTime(2022, 2, 27));

            //assert
            var expected = ExpectedCustomerActivityModels.Take(customerCount);
            actual.Should().BeInDescendingOrder(c => c.ReceiptSum).And.BeEquivalentTo(expected);
        }

        [Test]
        public async Task StatisticServiceGetCustomersMostPopularProductsAsyncReturnsCustomersTopProductsOrderedBySalesCount()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetAllWithDetailsAsync()).ReturnsAsync(ReceiptEntities.AsEnumerable());
            var statisticService = new StatisticService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await statisticService.GetCustomersMostPopularProductsAsync(3, 1);
            
            //assert
            actual.Should().BeEquivalentTo(ExpectedCustomersMostPopularProducts, options => options.Excluding(x => x.ReceiptDetailIds));
        }

        [TestCase(1, 734)]
        [TestCase(2, 744)]
        [TestCase(3, 795)]
        [TestCase(55, 0)]
        public async Task StatisticServiceGetIncomeOfCategoryInPeriodReturnsSumOfCategoryProductsSalesInPeriod(int categoryId, decimal sum)
        {
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetAllWithDetailsAsync()).ReturnsAsync(ReceiptEntities.AsEnumerable());
            var statisticService = new StatisticService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await statisticService.GetIncomeOfCategoryInPeriod(categoryId, new DateTime(1986, 1, 1), new DateTime(1986, 2, 24));

            //assert
            Assert.That(actual, Is.EqualTo(sum));
        }

        private static readonly IEnumerable<Product> ProductEntities =
            new List<Product>
            {
                new Product { Id = 1, ProductName = "Achi", ProductCategoryId = 1, Category = new ProductCategory { Id = 1, CategoryName = "Beverages" }, Price = 18.00m },
                new Product { Id = 2, ProductName = "Chang", ProductCategoryId = 1, Category = new ProductCategory { Id = 1, CategoryName = "Beverages" }, Price = 19.00m },
                new Product { Id = 3, ProductName = "Aniseed Juice", ProductCategoryId = 2, Category = new ProductCategory { Id = 2, CategoryName = "Condiments" }, Price = 10.00m },
                new Product { Id = 4, ProductName = "Chef Anton's Cajun Seasoning", ProductCategoryId = 2, Category = new ProductCategory { Id = 2, CategoryName = "Condiments" }, Price = 22.00m },
                new Product { Id = 5, ProductName = "Grandma's Boysenberry Spread", ProductCategoryId = 3, Category = new ProductCategory { Id = 3, CategoryName = "Confections" }, Price = 25.00m },
                new Product { Id = 6, ProductName = "Uncle Bob's Organic Dried Pears", ProductCategoryId = 4, Category = new ProductCategory { Id = 4, CategoryName = "Dairy Products" }, Price = 14.60m },
                new Product { Id = 7, ProductName = "Queso Cabrales", ProductCategoryId = 4, Category = new ProductCategory { Id = 4, CategoryName = "Dairy Products" }, Price = 21.00m },
                new Product { Id = 8, ProductName = "Queso Manchego La Pastora", ProductCategoryId = 3, Category = new ProductCategory { Id = 3, CategoryName = "Confections" }, Price = 38.00m },
                new Product { Id = 9, ProductName = "Tofu", ProductCategoryId = 2, Category = new ProductCategory { Id = 2, CategoryName = "Condiments" }, Price = 15.50m }
            };

        private static IEnumerable<ProductModel> ProductModels =>
            new List<ProductModel>
            {
                new ProductModel { Id = 1, ProductName = "Achi", ProductCategoryId = 1, CategoryName =  "Beverages", Price = 18.00m },
                new ProductModel { Id = 2, ProductName = "Chang", ProductCategoryId = 1, CategoryName =  "Beverages", Price = 19.00m },
                new ProductModel { Id = 3, ProductName = "Aniseed Juice", ProductCategoryId = 2,  CategoryName = "Condiments", Price = 10.00m },
                new ProductModel { Id = 4, ProductName = "Chef Anton's Cajun Seasoning", ProductCategoryId = 2,  CategoryName = "Condiments", Price = 22.00m },
                new ProductModel { Id = 5, ProductName = "Grandma's Boysenberry Spread", ProductCategoryId = 3, CategoryName = "Confections", Price = 25.00m },
                new ProductModel { Id = 6, ProductName = "Uncle Bob's Organic Dried Pears", ProductCategoryId = 4,  CategoryName = "Dairy Products", Price = 14.60m },
                new ProductModel { Id = 7, ProductName = "Queso Cabrales", ProductCategoryId = 4, CategoryName = "Dairy Products", Price = 21.00m },
                new ProductModel { Id = 8, ProductName = "Queso Manchego La Pastora", ProductCategoryId = 3, CategoryName = "Confections", Price = 38.00m },
                new ProductModel { Id = 9, ProductName = "Tofu", ProductCategoryId = 2, CategoryName = "Condiments", Price = 15.50m },
            };

        private static readonly IEnumerable<ProductModel> ExpectedMostPopularProducts =
            new List<ProductModel>()
{
                ProductModels.ElementAt(1), ProductModels.ElementAt(7), ProductModels.ElementAt(8), ProductModels.ElementAt(4), ProductModels.ElementAt(0)
            };

        private static readonly IEnumerable<ReceiptDetail> ReceiptDetailEntities =
            new List<ReceiptDetail>()
            {
                new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, Product = ProductEntities.ElementAt(0), DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, Product = ProductEntities.ElementAt(1), DiscountUnitPrice = 19, Quantity = 8, ReceiptId = 1},
                new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, Product = ProductEntities.ElementAt(2), DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 },
                new ReceiptDetail { Id = 4, ProductId = 1, UnitPrice = 10, Product = ProductEntities.ElementAt(0), DiscountUnitPrice = 9, Quantity = 10, ReceiptId = 2 },
                new ReceiptDetail { Id = 5, ProductId = 3, UnitPrice = 25, Product = ProductEntities.ElementAt(2), DiscountUnitPrice = 24, Quantity = 4, ReceiptId = 2},
                new ReceiptDetail { Id = 6, ProductId = 5, UnitPrice = 10, Product = ProductEntities.ElementAt(4), DiscountUnitPrice= 9, Quantity = 5, ReceiptId = 1},
                new ReceiptDetail { Id = 7, ProductId = 2, UnitPrice = 25, Product = ProductEntities.ElementAt(1), DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 3 },
                new ReceiptDetail { Id = 8, ProductId = 5, UnitPrice = 10, Product = ProductEntities.ElementAt(4), DiscountUnitPrice = 9, Quantity = 15, ReceiptId = 4 },
                new ReceiptDetail { Id = 9, ProductId = 6, UnitPrice = 25, Product = ProductEntities.ElementAt(5), DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 4 },
                new ReceiptDetail { Id = 10, ProductId = 8, UnitPrice = 20, Product = ProductEntities.ElementAt(7), DiscountUnitPrice = 15,  Quantity = 50, ReceiptId = 5 },
                new ReceiptDetail { Id = 11, ProductId = 9, UnitPrice = 35, Product = ProductEntities.ElementAt(8), DiscountUnitPrice = 26, Quantity = 24, ReceiptId = 5},
                new ReceiptDetail { Id = 12, ProductId = 2, UnitPrice = 20, Product = ProductEntities.ElementAt(1), DiscountUnitPrice = 18, Quantity = 25, ReceiptId = 5}
            };

        private static readonly IEnumerable<Customer> CustomerEntities =
            new List<Customer>()
            {
                new Customer { Id = 1, PersonId = 1, Person = new Person { Id = 1, Name = "Vittor", Surname = "Zhuk", BirthDate = new DateTime(1995, 1, 2) }, DiscountValue = 10},
                new Customer { Id = 2, PersonId = 2, Person = new Person { Id = 2, Name = "Peter", Surname = "Taleb", BirthDate = new DateTime(1965, 5, 12) }, DiscountValue = 15},
                new Customer { Id = 3, PersonId = 3, Person =  new Person { Id = 3, Name = "Dezmond", Surname = "Morris",  BirthDate = new DateTime(1955, 4, 12) }, DiscountValue = 5},
                new Customer { Id = 4, PersonId = 4, Person =  new Person { Id = 4, Name = "Stephen", Surname = "Curry", BirthDate = new DateTime(1983, 12, 31) }, DiscountValue = 12}
            };

        private static readonly IEnumerable<Receipt> ReceiptEntities =
            new List<Receipt>()
            {
                new Receipt
                {
                    Id = 1, CustomerId = 1, Customer = CustomerEntities.ElementAt(0), IsCheckedOut = false, OperationDate = new DateTime(1986, 1, 2),
                    ReceiptDetails = new List<ReceiptDetail>()
                    { 
                        ReceiptDetailEntities.ElementAt(0), ReceiptDetailEntities.ElementAt(1), ReceiptDetailEntities.ElementAt(2)
                    }
                },
                new Receipt
                {
                    Id = 2, CustomerId = 2, Customer = CustomerEntities.ElementAt(1), IsCheckedOut = false, OperationDate = new DateTime(1986, 1, 15),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        ReceiptDetailEntities.ElementAt(3), ReceiptDetailEntities.ElementAt(4)
                    }
                },
                new Receipt
                {
                    Id = 3, CustomerId = 1, Customer = CustomerEntities.ElementAt(0), IsCheckedOut = false, OperationDate = new DateTime(1986, 2, 15),
                    ReceiptDetails = new List<ReceiptDetail>() 
                    { 
                        ReceiptDetailEntities.ElementAt(5), ReceiptDetailEntities.ElementAt(6)
                    }
                },
                new Receipt
                {
                    Id = 4, CustomerId = 3, Customer = CustomerEntities.ElementAt(2), IsCheckedOut = false, OperationDate = new DateTime(1986, 2, 28),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        ReceiptDetailEntities.ElementAt(7), ReceiptDetailEntities.ElementAt(8)
                    }
                },
                new Receipt
                {
                    Id = 5, CustomerId = 1, Customer = CustomerEntities.ElementAt(0), IsCheckedOut = true, OperationDate = new DateTime(1986, 2, 22),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        ReceiptDetailEntities.ElementAt(9), ReceiptDetailEntities.ElementAt(10), ReceiptDetailEntities.ElementAt(11)
                    }
                }
            };

        private static readonly IEnumerable<CustomerActivityModel> ExpectedCustomerActivityModels =
            new List<CustomerActivityModel>()
            {
                new CustomerActivityModel { CustomerId = 1, CustomerName = "Vittor Zhuk", ReceiptSum = 2087 },
                new CustomerActivityModel { CustomerId = 2, CustomerName = "Peter Taleb", ReceiptSum = 186 },
                new CustomerActivityModel { CustomerId = 3, CustomerName = "Dezmond Morris", ReceiptSum = 159 }
            };

        public static IEnumerable<ProductModel> ExpectedCustomersMostPopularProducts =>
            new List<ProductModel>()
            {
               ProductModels.ElementAt(7), ProductModels.ElementAt(1), ProductModels.ElementAt(8)
            };
    }
}
