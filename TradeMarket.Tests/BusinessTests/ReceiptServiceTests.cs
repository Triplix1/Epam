using Business.Models;
using Business.Services;
using Data.Entities;
using Data.Interfaces;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Business.Validation;
using Library.Tests;

namespace TradeMarket.Tests.BusinessTests
{
    public class ReceiptServiceTests
    {
        [Test]
        public async Task ReceiptServiceGetAllReturnsAllReceipts()
        {
            //arrange
            var expected = GetTestReceiptsModels;
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ReceiptRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(GetTestReceiptsEntities.AsEnumerable());

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await receiptService.GetAllAsync();

            //assert
            actual.Should().BeEquivalentTo(expected);
        }

        [TestCase(5)]
        public async Task ReceiptServiceGetByIdReturnsReceiptModel(int id)
        {
            //arrange
            var expected = GetTestReceiptsModels.FirstOrDefault(x => x.Id == id);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(GetTestReceiptsEntities.FirstOrDefault(x => x.Id == id));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await receiptService.GetByIdAsync(id);

            //assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task ReceiptServiceAddAsyncAddsReceipt()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ReceiptRepository.AddAsync(It.IsAny<Receipt>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var receipt = GetTestReceiptsModels.Last();

            //act
            await receiptService.AddAsync(receipt);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptRepository.AddAsync(It.Is<Receipt>(c => c.Id == receipt.Id 
                && c.CustomerId == receipt.CustomerId && c.OperationDate == receipt.OperationDate)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ReceiptServiceUpdateAsyncUpdatesReceipt()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ReceiptRepository.Update(It.IsAny<Receipt>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var receipt = GetTestReceiptsModels.First();

            //act
            await receiptService.UpdateAsync(receipt);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptRepository.Update(It.Is<Receipt>(r => r.Id == receipt.Id && r.IsCheckedOut == receipt.IsCheckedOut && 
                r.CustomerId == receipt.CustomerId && r.OperationDate == receipt.OperationDate)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ReceiptServiceDeleteAsyncDeletesReceiptWithDetails(int receiptId)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var receipt = GetTestReceiptsEntities.First(x => x.Id == receiptId);
            var expectedDetailsLength = receipt.ReceiptDetails.Count;

            mockUnitOfWork.Setup(m => m.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(m => m.ReceiptRepository.DeleteByIdAsync(It.IsAny<int>()));
            mockUnitOfWork.Setup(m => m.ReceiptDetailRepository.Delete(It.IsAny<ReceiptDetail>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.DeleteAsync(receiptId);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptRepository.DeleteByIdAsync(It.Is<int>(x => x == receiptId)), Times.Once());
            mockUnitOfWork.Verify(x => x.ReceiptDetailRepository.Delete(It.Is<ReceiptDetail>(detail => detail.ReceiptId == receiptId)), 
                failMessage: "All existing receipt details must be deleted", times: Times.Exactly(expectedDetailsLength));
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once());
        }


        [TestCase(4)]
        [TestCase(5)]
        public async Task ReceiptServiceGetReceiptDetailsAsyncReturnsDetailsByReceiptId(int receiptId)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(GetTestReceiptsEntities.FirstOrDefault(x => x.Id == receiptId));
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await receiptService.GetReceiptDetailsAsync(receiptId);

            //assert
            var expected = GetTestReceiptsEntities.FirstOrDefault(x => x.Id == receiptId)?.ReceiptDetails;

            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.Product).Excluding(x => x.Receipt));
        }


        [TestCase("1965-1-1", "1965-2-1", new[] {1, 2})]
        [TestCase("1965-2-1", "1965-4-1", new[] {3, 4, 5})]
        public async Task ReceiptServiceGetReceiptsByPeriodAsyncReturnsReceiptsInPeriod(DateTime startDate, DateTime endDate, IEnumerable<int> expectedReceiptIds)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetAllWithDetailsAsync()).ReturnsAsync(GetTestReceiptsEntities.AsEnumerable());
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await receiptService.GetReceiptsByPeriodAsync(startDate, endDate);

            //assert
            var expected =  GetTestReceiptsModels.Where(x => expectedReceiptIds.Contains(x.Id));

            actual.Should().BeEquivalentTo(expected);
        }

        [TestCase(1, 99)]
        [TestCase(4, 546)]
        [TestCase(5, 1074)]
        public async Task ReceiptServiceToPayAsyncReturnsSumByReceiptIdWithDiscount(int receiptId, decimal expectedSum)
        {
            //arrange
            var receipt = GetTestReceiptsEntities.FirstOrDefault(x => x.Id == receiptId);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await receiptService.ToPayAsync(receiptId);

            //assert
            actual.Should().Be(expectedSum);
        }

        [TestCase(4, 30)]
        public async Task ReceiptServiceAddProductAsyncCreatesReceiptDetailIfProductWasNotAddedBefore(int productId, int discountValue)
        {
            //arrange 
            var receipt = new Receipt
            {
                Id = 1, Customer = new Customer { Id = 1, DiscountValue = discountValue },
                ReceiptDetails = new List<ReceiptDetail> {
                new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1 },
                new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 }
            }};

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(x => x.ProductRepository.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(ProductEntities.FirstOrDefault(x => x.Id == productId));
            mockUnitOfWork.Setup(x => x.ReceiptDetailRepository.AddAsync(It.IsAny<ReceiptDetail>()));
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.AddProductAsync(productId, 1, 8);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptDetailRepository.AddAsync(It.Is<ReceiptDetail>(receiptDetail => 
                receiptDetail.ReceiptId == receipt.Id && receiptDetail.ProductId == productId && receiptDetail.Quantity == 8)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [TestCase(1, 5, 7)]
        [TestCase(2, 1, 4)]
        [TestCase(3, 200, 201)]
        public async Task ReceiptServiceAddProductAsyncUpdatesQuantityIfProductWasAddedToReceipt(int productId, int quantity, int expectedQuantity)
        {
            //arrange 
            var receipt = new Receipt
            {
                Id = 2,
                ReceiptDetails = new List<ReceiptDetail> {
                new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1 },
                new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 }
                }
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(x => x.ReceiptDetailRepository.AddAsync(It.IsAny<ReceiptDetail>()));
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.AddProductAsync(productId, 2, quantity);

            //assert
            var actualQuantity = receipt.ReceiptDetails.First(x => x.ProductId == productId).Quantity;
            actualQuantity.Should().Be(expectedQuantity);
            mockUnitOfWork.Verify(x => x.ReceiptDetailRepository.AddAsync(It.IsAny<ReceiptDetail>()), Times.Never);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        
        [TestCase(1, 15, 15.3)]
        [TestCase(2, 20, 15.2)]
        [TestCase(3, 50, 5.0)]
        [TestCase(8, 99, 0.38)]
        public async Task ReceiptServiceAddProductSetsDiscountUnitPriceValueAccordingToCustomersDiscount(int productId, int discount, decimal expectedDiscountPrice)
        {
            //arrange
            var product = ProductEntities.FirstOrDefault(x => x.Id == productId);
            var receipt = new Receipt { Id = 1, CustomerId = 1, Customer = new Customer { Id = 1, DiscountValue = discount } };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(x => x.ProductRepository.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(product);
            mockUnitOfWork.Setup(x => x.ReceiptDetailRepository.AddAsync(It.IsAny<ReceiptDetail>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.AddProductAsync(productId, 1, 2);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptDetailRepository.AddAsync(It.Is<ReceiptDetail>(detail =>
                detail.ReceiptId == receipt.Id && detail.UnitPrice == product.Price && detail.ProductId == product.Id &&
                detail.DiscountUnitPrice == expectedDiscountPrice)), Times.Once);

            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }


        [TestCase(1)]
        [TestCase(2)]
        public async Task ReceiptServiceAddProductThrowsMarketExceptionIfProductDoesNotExist(int productId)
        {
            //arrange
            var receipt = new Receipt { Id = 1, CustomerId = 1 };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(x => x.ProductRepository.GetByIdAsync(It.IsAny<int>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            Func<Task> act = async () => await receiptService.AddProductAsync(productId, 1, 1);
            
            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ReceiptServiceAddProductThrowsMarketExceptionIfReceiptDoesNotExist(int receiptId)
        {
            //arrange

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>()));
            mockUnitOfWork.Setup(x => x.ProductRepository.GetByIdAsync(It.IsAny<int>()));

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            Func<Task> act = async () => await receiptService.AddProductAsync(1, receiptId, 1);
            
            //assert
            await act.Should().ThrowAsync<MarketException>();
        }


        [Test]
        public async Task ReceiptServiceCheckOutAsyncUpdatesCheckOutPropertyValueAndSavesChanges()
        {
            //arrange
            var receipt = new Receipt { Id = 6, IsCheckedOut = false };
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(receipt);

            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.CheckOutAsync(6);

            //assert
            receipt.IsCheckedOut.Should().BeTrue();
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }


        [TestCase(1, 1, 1)]
        [TestCase(2, 2, 1)]
        [TestCase(3, 3, 2)]
        public async Task ReceiptServiceRemoveProductAsyncUpdatesDetailQuantityValue(int productId, int quantity, int expectedQuantity)
        {
            //arrange
            var receipt = new Receipt 
            {
                Id = 1, ReceiptDetails = new List<ReceiptDetail> 
                {
                    new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                    new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1 },
                    new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 5, ReceiptId = 1 }
                }
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.RemoveProductAsync(productId, 1, quantity);

            //assert
            var actualQuantity = receipt.ReceiptDetails.First(x => x.ProductId == productId).Quantity;
            actualQuantity.Should().Be(expectedQuantity);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ReceiptServiceRemoveProductAsyncDeletesDetailIfQuantityEqualsZero()
        {

            //arrange
            var receipt = new Receipt
            {
                Id = 1, 
                ReceiptDetails = new List<ReceiptDetail>
                {
                    new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                    new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1 },
                    new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1 }
                }
            };

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.ReceiptRepository.GetByIdWithDetailsAsync(It.IsAny<int>())).ReturnsAsync(receipt);
            mockUnitOfWork.Setup(x => x.ReceiptDetailRepository.Delete(It.IsAny<ReceiptDetail>()));
            var receiptService = new ReceiptService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await receiptService.RemoveProductAsync(1, 1, 2);

            //assert
            mockUnitOfWork.Verify(x => x.ReceiptDetailRepository.Delete(It.Is<ReceiptDetail>(rd => rd.ReceiptId == receipt.Id && rd.ProductId == 1)), Times.Once());
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

      

        private static IEnumerable<Receipt> GetTestReceiptsEntities =>
          new List<Receipt>()
          {
                new Receipt
                {
                    Id = 1, CustomerId = 1, IsCheckedOut = false, OperationDate = new DateTime(1965, 1, 2),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 1, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 2, ReceiptId = 1 },
                        new ReceiptDetail { Id = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 19, Quantity = 3, ReceiptId = 1},
                        new ReceiptDetail { Id = 3, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 1}
                    }
                },
                new Receipt
                {
                    Id = 2, CustomerId = 2, IsCheckedOut = false, OperationDate = new DateTime(1965, 1, 15),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 4, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 10, ReceiptId = 2 },
                        new ReceiptDetail { Id = 5, ProductId = 3, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 1, ReceiptId = 2}
                    }
                },
                new Receipt
                {
                    Id = 3, CustomerId = 3, IsCheckedOut = false, OperationDate = new DateTime(1965, 2, 15),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 6, ProductId = 1, UnitPrice = 10, DiscountUnitPrice = 9, Quantity = 10, ReceiptId = 3 },
                        new ReceiptDetail { Id = 7, ProductId = 2, UnitPrice = 25, DiscountUnitPrice = 24, Quantity = 18, ReceiptId = 3}
                    }
                },
                new Receipt
                {
                    Id = 4, CustomerId = 4, IsCheckedOut = false, OperationDate = new DateTime(1965, 2, 28),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 8, ProductId = 5, UnitPrice = 10,  DiscountUnitPrice = 9, Quantity = 10, ReceiptId = 4 },
                        new ReceiptDetail { Id = 9, ProductId = 6, UnitPrice = 25,  DiscountUnitPrice = 24, Quantity = 19, ReceiptId = 4 }
                    }
                },
                new Receipt
                {
                    Id = 5, CustomerId = 1, IsCheckedOut = true, OperationDate = new DateTime(1965, 2, 22),
                    ReceiptDetails = new List<ReceiptDetail>()
                    {
                        new ReceiptDetail { Id = 10, ProductId = 8, UnitPrice = 20, DiscountUnitPrice = 15, Quantity = 30, ReceiptId = 5 },
                        new ReceiptDetail { Id = 11, ProductId = 9, UnitPrice = 35, DiscountUnitPrice = 26, Quantity = 24, ReceiptId = 5}
                    }
                }
          };

        private static IEnumerable<ReceiptModel> GetTestReceiptsModels =>
         new List<ReceiptModel>()
         {
            new ReceiptModel
            {
                Id = 1, CustomerId = 1, IsCheckedOut = false,  OperationDate = new DateTime(1965, 1, 2),
                ReceiptDetailsIds = new List<int>() { 1, 2, 3 }
            },
            new ReceiptModel
            {
                Id = 2, CustomerId = 2, IsCheckedOut = false,  OperationDate = new DateTime(1965, 1, 15),
                ReceiptDetailsIds = new List<int>() { 4, 5 }
            },
            new ReceiptModel
            {
                Id = 3, CustomerId = 3, IsCheckedOut = false, OperationDate = new DateTime(1965, 2, 15),
                ReceiptDetailsIds = new List<int>() { 6, 7 }
            },
            new ReceiptModel
            {
                Id = 4, CustomerId = 4, IsCheckedOut = false, OperationDate = new DateTime(1965, 2, 28),
                ReceiptDetailsIds = new List<int>() { 8, 9 }
            },
            new ReceiptModel
            {
                Id = 5, CustomerId = 1, IsCheckedOut = true, OperationDate =  new DateTime(1965, 2, 22),
                ReceiptDetailsIds = new List<int>() { 10, 11 }
            }
         };


        private static IEnumerable<Product> ProductEntities =>
            new List<Product>
            {
                new Product {Id = 1, ProductName = "Chai", ProductCategoryId = 1, Category = new ProductCategory { Id = 1, CategoryName = "Beverages" }, Price = 18.00m },
                new Product {Id = 2, ProductName = "Chang", ProductCategoryId = 1, Category = new ProductCategory { Id = 1, CategoryName = "Beverages" }, Price = 19.00m },
                new Product {Id = 3, ProductName = "Aniseed Syrup", ProductCategoryId = 2, Category = new ProductCategory { Id = 2, CategoryName = "Condiments" }, Price = 10.00m },
                new Product {Id = 4, ProductName = "Chef Anton's Cajun Seasoning", ProductCategoryId = 2, Category =  new ProductCategory { Id = 2, CategoryName = "Condiments" }, Price = 22.00m },
                new Product {Id = 5, ProductName = "Grandma's Boysenberry Spread", ProductCategoryId = 3, Category = new ProductCategory { Id = 3, CategoryName = "Confections" } , Price = 25.00m },
                new Product {Id = 6, ProductName = "Uncle Bob's Organic Dried Pears", ProductCategoryId = 4, Category =  new ProductCategory { Id = 4, CategoryName = "Dairy Products" },Price = 14.60m },
                new Product {Id = 7, ProductName = "Queso Cabrales", ProductCategoryId = 4, Category =  new ProductCategory { Id = 4, CategoryName = "Dairy Products" }, Price = 21.00m },
                new Product {Id = 8, ProductName = "Queso Manchego La Pastora", ProductCategoryId = 3, Category = new ProductCategory { Id = 3, CategoryName = "Confections" } ,Price = 38.00m },
                new Product {Id = 9, ProductName = "Tofu", ProductCategoryId = 2, Category = new ProductCategory { Id = 2, CategoryName = "Condiments" }, Price = 15.50m }
            };
    }
}
