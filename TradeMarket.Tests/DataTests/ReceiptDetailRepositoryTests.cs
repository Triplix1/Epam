using Data.Data;
using Data.Entities;
using Data.Repositories;
using Library.Tests;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TradeMarket.Tests.DataTests
{
    [TestFixture]
    internal class ReceiptDetailRepositoryTests
    {
        [TestCase(1)]
        [TestCase(5)]
        public async Task ReceiptDetailRepositoryGetByIdAsyncReturnsSingleValue(int id)
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptDetailRepository = new ReceiptDetailRepository(context);
            var receiptDetail = await receiptDetailRepository.GetByIdAsync(id);

            var expected = ExpectedReceiptsDetails.FirstOrDefault(x => x.Id == id);

            Assert.That(receiptDetail, Is.EqualTo(expected).Using(new ReceiptDetailEqualityComparer()), message: "GetByIdAsync method works incorrect");
        }

        [Test]
        public async Task ReceiptDetailRepositoryGetAllAsyncReturnsAllValues()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptDetailRepository = new ReceiptDetailRepository(context);
            var receiptDetails = await receiptDetailRepository.GetAllAsync();

            Assert.That(receiptDetails, Is.EqualTo(ExpectedReceiptsDetails).Using(new ReceiptDetailEqualityComparer()), message: "GetAllAsync method works incorrect");
        }

        [Test]
        public async Task ReceiptDetailRepositoryAddAsyncAddsValueToDatabase()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptDetailRepository = new ReceiptDetailRepository(context);
            var receiptDetail = new ReceiptDetail { Id = 6, ReceiptId = 2, ProductId = 1 };

            await receiptDetailRepository.AddAsync(receiptDetail);
            await context.SaveChangesAsync();

            Assert.That(context.ReceiptsDetails.Count(), Is.EqualTo(6), message: "AddAsync method works incorrect");
        }

        [Test]
        public async Task ReceiptDetailRepositoryDeleteByIdAsyncDeletesEntity()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptDetailRepository = new ReceiptDetailRepository(context);

            await receiptDetailRepository.DeleteByIdAsync(1);
            await context.SaveChangesAsync();

            Assert.That(context.ReceiptsDetails.Count(), Is.EqualTo(4), message: "DeleteByIdAsync works incorrect");
        }

        [Test]
        public async Task ReceiptDetailRepositoryUpdateUpdatesEntity()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptDetailRepository = new ReceiptDetailRepository(context);
            var receiptDetail = new ReceiptDetail
            {
                Id = 1,
                ReceiptId = 1,
                ProductId = 1,
                UnitPrice = 20,
                DiscountUnitPrice = 16,
                Quantity = 5
            };

            receiptDetailRepository.Update(receiptDetail);
            await context.SaveChangesAsync();

            Assert.That(receiptDetail, Is.EqualTo(new ReceiptDetail
            {
                Id = 1,
                ReceiptId = 1,
                ProductId = 1,
                UnitPrice = 20,
                DiscountUnitPrice = 16,
                Quantity = 5
            }).Using(new ReceiptDetailEqualityComparer()), message: "Update method works incorrect");
        }

        [Test]
        public async Task ReceiptDetailRepositoryGetAllWithDetailsAsyncReturnsWithIncludedEntities()
        {
            using var context = new TradeMarketDbContext(UnitTestHelper.GetUnitTestDbOptions());

            var receiptDetailRepository = new ReceiptDetailRepository(context);

            var receiptDetails = await receiptDetailRepository.GetAllWithDetailsAsync();
            var receiptDetail = receiptDetails.FirstOrDefault(x => x.Id == 1);

            Assert.That(receiptDetails, Is.EqualTo(ExpectedReceiptsDetails).Using(new ReceiptDetailEqualityComparer()), message: "GetAllWithDetailsAsync method works incorrect");
            Assert.That(receiptDetail.Product, Is.Not.Null, message: "GetAllWithDetailsAsync method doesnt't return included entities");
            Assert.That(receiptDetail.Receipt, Is.Not.Null, message: "GetAllWithDetailsAsync method doesnt't return included entities");
            Assert.That(receiptDetail.Product.Category, Is.Not.Null, message: "GetAllWithDetailsAsync method doesnt't return included entities");
        }

        private static IEnumerable<ReceiptDetail> ExpectedReceiptsDetails =>
            new[]
            {
                new ReceiptDetail { Id = 1, ReceiptId = 1, ProductId = 1, UnitPrice = 40, DiscountUnitPrice = 32, Quantity = 3 },
                new ReceiptDetail { Id = 2, ReceiptId = 1, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 16, Quantity = 1 },
                new ReceiptDetail { Id = 3, ReceiptId = 2, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 32, Quantity = 2 },
                new ReceiptDetail { Id = 4, ReceiptId = 3, ProductId = 1, UnitPrice = 40, DiscountUnitPrice = 36, Quantity = 2 },
                new ReceiptDetail { Id = 5, ReceiptId = 3, ProductId = 2, UnitPrice = 20, DiscountUnitPrice = 18, Quantity = 5 }
            };
    }
}
