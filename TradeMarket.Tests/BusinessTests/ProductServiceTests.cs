using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Moq;
using System.Linq;
using Business.Services;
using Business.Validation;
using Library.Tests;

namespace TradeMarket.Tests.BusinessTests
{
    public class ProductServiceTests
    {
        [Test]
        public async Task ProductServiceGetAllReturnsAllProducts()
        {
            //arrange
            var expected = ProductModels.ToList();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ProductRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(ProductEntities.AsEnumerable());

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetAllAsync();

            //assert
            actual.Should().BeEquivalentTo(expected, options =>
                options.Excluding(x => x.ReceiptDetailIds));
        }

        [Test]
        public async Task ProductServiceGetAllProductCategoriesAsyncReturnsAllCategories()
        {
            //arrange
            var expected = ProductCategoryModels;
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            
            mockUnitOfWork
                .Setup(x => x.ProductCategoryRepository.GetAllAsync())
                .ReturnsAsync(ProductCategoryEntities.AsEnumerable());
        
            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetAllProductCategoriesAsync();

            //assert
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.ProductIds));
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ProductServiceGetByIdReturnsProductModel(int id)
        {
            //arrange
            var expected = ProductModels.FirstOrDefault(x => x.Id == id);

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork
                .Setup(x => x.ProductRepository.GetByIdWithDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync(ProductEntities.FirstOrDefault(x => x.Id == id));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetByIdAsync(id);

            //assert
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.ReceiptDetailIds));
        }


        [Test]
        public async Task ProductServiceAddAsyncAddsProduct()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.AddAsync(It.IsAny<Product>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var product = new ProductModel { Id = 11, ProductName = "Orange juice", ProductCategoryId = 9, Price = 29.00m };

            //act
            await productService.AddAsync(product);

            //assert
            mockUnitOfWork.Verify(x => x.ProductRepository.AddAsync(It.Is<Product>(c => c.Id == product.Id && c.ProductCategoryId == product.ProductCategoryId && c.Price == product.Price && c.ProductName == product.ProductName)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ProductServiceAddCategoryAsyncAddsCategory()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductCategoryRepository.AddAsync(It.IsAny<ProductCategory>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var category = new ProductCategoryModel { Id = 98, CategoryName = "Foodstuff" };

            //act
            await productService.AddCategoryAsync(category);

            //add equality comparer
            //assert
            mockUnitOfWork.Verify(x => x.ProductCategoryRepository.AddAsync(It.Is<ProductCategory>(c => c.Id == category.Id && c.CategoryName == category.CategoryName)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ProductServiceAddAsyncThrowsMarketExceptionWithEmptyProductName()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.AddAsync(It.IsAny<Product>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var product = new ProductModel { Id = 1, ProductName = string.Empty, ProductCategoryId = 1, CategoryName = "Beverages", Price = 18.00m };

            //act
            Func<Task> act = async () => await productService.AddAsync(product);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [TestCase(-5000.50)]
        [TestCase(-500000)]
        [TestCase(-0.0001)]
        public async Task ProductServiceAddAsyncThrowsMarketExceptionIfPriceIsNegative(decimal price)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.AddAsync(It.IsAny<Product>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var product = new ProductModel { Id = 1, ProductName = "Cola", ProductCategoryId = 1, CategoryName = "Beverages", Price = price };

            //act
            Func<Task> act = async () => await productService.AddAsync(product);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }


        [Test]
        public async Task ProductServiceAddCategoryAsyncThrowsMarketExceptionWithEmptyCategoryName()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductCategoryRepository.AddAsync(It.IsAny<ProductCategory>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var category = new ProductCategoryModel { Id = 10, CategoryName = "" };

            //act
            Func<Task> act = async () => await productService.AddCategoryAsync(category);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ProductServiceDeleteAsyncDeletesProduct(int id)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.DeleteByIdAsync(It.IsAny<int>()));
            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await productService.DeleteAsync(id);

            //assert
            mockUnitOfWork.Verify(x => x.ProductRepository.DeleteByIdAsync(id), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task ProductServiceRemoveCategoryAsyncDeletesCategory(int id)
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductCategoryRepository.DeleteByIdAsync(It.IsAny<int>()));
            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            await productService.RemoveCategoryAsync(id);

            //assert
            mockUnitOfWork.Verify(x => x.ProductCategoryRepository.DeleteByIdAsync(id), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ProductServiceUpdateAsyncUpdatesProduct()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.Update(It.IsAny<Product>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var product = new ProductModel { Id = 9, ProductName = "Cabrales", ProductCategoryId = 45, CategoryName = "Household", Price = 29.00m };

            //act
            await productService.UpdateAsync(product);

            //assert
            mockUnitOfWork.Verify(x => x.ProductRepository.Update(It.Is<Product>(c => c.Id == product.Id && c.ProductCategoryId == product.ProductCategoryId && c.Price == product.Price && c.ProductName == product.ProductName)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ProductServiceUpdateAsyncThrowsMarketExceptionsWithEmptyName()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductRepository.Update(It.IsAny<Product>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var product = new ProductModel { Id = 3, ProductName = "", ProductCategoryId = 1, CategoryName = "Dairy Products", Price = 288.00m };

            //act
            Func<Task> act = async () => await productService.UpdateAsync(product);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

        [Test]
        public async Task ProductServiceUpdateCategoryAsyncUpdatesCategory()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductCategoryRepository.Update(It.IsAny<ProductCategory>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var category = new ProductCategoryModel { Id = 77, CategoryName = "Name" };

            //act
            await productService.UpdateCategoryAsync(category);

            //assert
            mockUnitOfWork.Verify(x => x.ProductCategoryRepository.Update(It.Is<ProductCategory>(c => c.Id == category.Id && category.CategoryName == c.CategoryName)), Times.Once);
            mockUnitOfWork.Verify(x => x.SaveAsync(), Times.Once);
        }

        [Test]
        public async Task ProductServiceUpdateCategoryThrowsMarketExceptionsWithEmptyName()
        {
            //arrange
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(m => m.ProductCategoryRepository.Update(It.IsAny<ProductCategory>()));

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());
            var category = new ProductCategoryModel { Id = 77, CategoryName = "" };

            //act
            Func<Task> act = async () => await productService.UpdateCategoryAsync(category);

            //assert
            await act.Should().ThrowAsync<MarketException>();
        }

       
        [TestCase(4, new[] {6, 7})]
        [TestCase(5, new[] {10, 11})]
        [TestCase(6, new[] {12, 13})]
        public async Task ProductServiceGetByFilterAsyncReturnsProductsByCategory(int categoryId, IEnumerable<int> expectedProductIds)
        {
            //arrange
            var expected = ProductModels.Where(x => expectedProductIds.Contains(x.Id));
            var filter = new FilterSearchModel { CategoryId = categoryId };

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ProductRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(ProductEntities.AsEnumerable());

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetByFilterAsync(filter);

            //assert
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.ReceiptDetailIds));
        }

        [TestCase(20, new[] { 4, 5, 7, 8, 10, 12, 13 })]
        [TestCase(37, new[] {8, 12, 13})]
        public async Task ProductServiceGetByFilterReturnsProductByMinPrice(int minPrice, IEnumerable<int> expectedProductIds)
        {
            //arrange
            var expected = ProductModels.Where(x => expectedProductIds.Contains(x.Id));
            var filter = new FilterSearchModel { MinPrice = minPrice };

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ProductRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(ProductEntities.AsEnumerable());

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetByFilterAsync(filter);

            //assert
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.ReceiptDetailIds));
        }

        [TestCase(4, 16, null, new[] { 7 })]
        [TestCase(2, 15, 25, new[] { 4, 9 })]
        [TestCase(6, 30, 40, new[] { 12 })]
        [TestCase(5, 10, 22, new[] { 10 })]
        public async Task ProductServiceGetByFilterReturnsProductByFilter(int? categoryId, int? minPrice, int? maxPrice, IEnumerable<int> expectedProductIds)
        {
            //arrange
            var expected = ProductModels.Where(x => expectedProductIds.Contains(x.Id)).ToList();
            var filter = new FilterSearchModel { CategoryId = categoryId, MinPrice = minPrice, MaxPrice = maxPrice };

            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUnitOfWork
                .Setup(x => x.ProductRepository.GetAllWithDetailsAsync())
                .ReturnsAsync(ProductEntities.AsEnumerable());

            var productService = new ProductService(mockUnitOfWork.Object, UnitTestHelper.CreateMapperProfile());

            //act
            var actual = await productService.GetByFilterAsync(filter);

            //assert
            actual.Should().BeEquivalentTo(expected, options => options.Excluding(x => x.ReceiptDetailIds));
        }

        #region Test Data

        private static IEnumerable<ProductModel> ProductModels =>
            new List<ProductModel>
            {
                new ProductModel { Id = 1, ProductName = "Chai", ProductCategoryId = 1, CategoryName =  "Beverages", Price = 18.00m },
                new ProductModel { Id = 2, ProductName = "Chang", ProductCategoryId = 1, CategoryName =  "Beverages", Price = 19.00m },
                new ProductModel { Id = 3, ProductName = "Aniseed Syrup", ProductCategoryId = 2,  CategoryName = "Condiments", Price = 10.00m },
                new ProductModel { Id = 4, ProductName = "Chef Anton's Cajun Seasoning", ProductCategoryId = 2,  CategoryName = "Condiments", Price = 22.00m },
                new ProductModel { Id = 5, ProductName = "Grandma's Boysenberry Spread", ProductCategoryId = 3, CategoryName = "Confections", Price = 25.00m },
                new ProductModel { Id = 6, ProductName = "Uncle Bob's Organic Dried Pears", ProductCategoryId = 4,  CategoryName = "Dairy Products", Price = 14.60m },
                new ProductModel { Id = 7, ProductName = "Queso Cabrales", ProductCategoryId = 4, CategoryName = "Dairy Products", Price = 21.00m },
                new ProductModel { Id = 8, ProductName = "Queso Manchego La Pastora", ProductCategoryId = 3, CategoryName = "Confections", Price = 38.00m },
                new ProductModel { Id = 9, ProductName = "Tofu", ProductCategoryId = 2, CategoryName = "Condiments", Price = 15.50m },
                new ProductModel { Id = 10, ProductName = "Gustaf's Knäckebröd", ProductCategoryId = 5, CategoryName = "Grains", Price = 21.00m },
                new ProductModel { Id = 11, ProductName = "Tunnbröd", ProductCategoryId = 5, CategoryName = "Grains", Price = 9.00m },
                new ProductModel { Id = 12, ProductName = "Alice Mutton", ProductCategoryId = 6, CategoryName = "Meat", Price = 39.00m },
                new ProductModel { Id = 13, ProductName = "Thüringer Rostbratwurst", ProductCategoryId = 6, CategoryName = "Meat", Price = 123.79m}

            };

        private  static IEnumerable<Product> ProductEntities =>
            new List<Product>
            {
                new Product {Id = 1, ProductName = "Chai", ProductCategoryId = 1, Category =  ProductCategoryEntities.ElementAt(0), Price = 18.00m },
                new Product {Id = 2, ProductName = "Chang", ProductCategoryId = 1, Category =  ProductCategoryEntities.ElementAt(0), Price = 19.00m },
                new Product {Id = 3, ProductName = "Aniseed Syrup", ProductCategoryId = 2, Category =  ProductCategoryEntities.ElementAt(1), Price = 10.00m },
                new Product {Id = 4, ProductName = "Chef Anton's Cajun Seasoning", ProductCategoryId = 2, Category =  ProductCategoryEntities.ElementAt(1), Price = 22.00m },
                new Product {Id = 5, ProductName = "Grandma's Boysenberry Spread", ProductCategoryId = 3, Category = ProductCategoryEntities.ElementAt(2) , Price = 25.00m },
                new Product {Id = 6, ProductName = "Uncle Bob's Organic Dried Pears", ProductCategoryId = 4, Category =  ProductCategoryEntities.ElementAt(3),Price = 14.60m },
                new Product {Id = 7, ProductName = "Queso Cabrales", ProductCategoryId = 4, Category = ProductCategoryEntities.ElementAt(3), Price = 21.00m },
                new Product {Id = 8, ProductName = "Queso Manchego La Pastora", ProductCategoryId = 3, Category = ProductCategoryEntities.ElementAt(2), Price = 38.00m },
                new Product {Id = 9, ProductName = "Tofu", ProductCategoryId = 2, Category = ProductCategoryEntities.ElementAt(1), Price = 15.50m },
                new Product {Id = 10, ProductName = "Gustaf's Knäckebröd", ProductCategoryId = 5, Category = ProductCategoryEntities.ElementAt(4), Price = 21.00m },
                new Product {Id = 11, ProductName = "Tunnbröd", ProductCategoryId = 5, Category = ProductCategoryEntities.ElementAt(4), Price = 9.00m },
                new Product {Id = 12, ProductName = "Alice Mutton", ProductCategoryId = 6, Category = ProductCategoryEntities.ElementAt(5), Price = 39.00m },
                new Product {Id = 13, ProductName = "Thüringer Rostbratwurst", ProductCategoryId = 6, Category = ProductCategoryEntities.ElementAt(5), Price = 123.79m}
            };

        private static IEnumerable<ProductCategory> ProductCategoryEntities =>
            new List<ProductCategory>
            {
                new ProductCategory 
                {
                    Id = 1, CategoryName = "Beverages",
                },
                new ProductCategory
                {
                    Id = 2, CategoryName = "Condiments",
                },
                new ProductCategory
                {
                    Id = 3, CategoryName = "Confections",
                },
                new ProductCategory
                {
                    Id = 4, CategoryName = "Dairy Products",
                },
                new ProductCategory
                {
                    Id = 5, CategoryName = "Grains",
                },
                new ProductCategory
                {
                    Id = 6, CategoryName = "Meat",
                }
            };

        private static IEnumerable<ProductCategoryModel> ProductCategoryModels =>
            new List<ProductCategoryModel>
            {
                new ProductCategoryModel { Id = 1, CategoryName = "Beverages" }, 
                new ProductCategoryModel { Id = 2, CategoryName = "Condiments" },
                new ProductCategoryModel { Id = 3, CategoryName = "Confections" },
                new ProductCategoryModel { Id = 4, CategoryName = "Dairy Products" },
                new ProductCategoryModel { Id = 5, CategoryName = "Grains" },
                new ProductCategoryModel { Id = 6, CategoryName = "Meat" }
            };

        #endregion
    }
}
