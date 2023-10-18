using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Validation;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public StatisticService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductModel>> GetCustomersMostPopularProductsAsync(int productCount, int customerId)
        {
            if (productCount <= 0)
                throw new MarketException();

            var receipts = await _unitOfWork.ReceiptRepository.GetAllWithDetailsAsync();

            var receipt = receipts.Where(r => r.CustomerId == customerId);

            if (receipt == null)
                throw new MarketException();

            var products = receipt
                .SelectMany(r => r.ReceiptDetails)
                .GroupBy(r => r.Product)                
                .OrderByDescending(g => g.Sum(p => p.Quantity))
                .Select(g => g.Key)
                .Take(productCount);

            return _mapper.Map<IEnumerable<ProductModel>>(products);
        }

        public async Task<decimal> GetIncomeOfCategoryInPeriod(int categoryId, DateTime startDate, DateTime endDate)
        {
            var receipts = await _unitOfWork.ReceiptRepository.GetAllWithDetailsAsync();

            return receipts
                .Where(r => r.OperationDate >= startDate && r.OperationDate <= endDate)
                .SelectMany(r => r.ReceiptDetails)
                .Where(r => r.Product.ProductCategoryId == categoryId)
                .Sum(r => r.DiscountUnitPrice * r.Quantity);
        }

        public async Task<IEnumerable<ProductModel>> GetMostPopularProductsAsync(int productCount)
        {
            if (productCount <= 0)
                throw new MarketException();

            var receiptDetails = await _unitOfWork.ReceiptDetailRepository.GetAllWithDetailsAsync();

            var products = receiptDetails
                .GroupBy(rd => rd.Product)
                .OrderByDescending(g => g.Sum(rd => rd.Quantity))
                .Select(g => g.Key)
                .Take(productCount);

            return _mapper.Map<IEnumerable<ProductModel>>(products);
        }

        public async Task<IEnumerable<CustomerActivityModel>> GetMostValuableCustomersAsync(int customerCount, DateTime startDate, DateTime endDate)
        {
            var receipts = await _unitOfWork.ReceiptRepository.GetAllWithDetailsAsync();

            var customers = receipts
                .Select(g => new 
                {
                    Customer = g.Customer,
                    ReceiptSum = g.ReceiptDetails.Sum(rd => rd.Quantity * rd.DiscountUnitPrice)
                })
                .GroupBy(rd => rd.Customer)
                .Select(g => new CustomerActivityModel
                {
                    CustomerId = g.Key.Id,
                    CustomerName = g.Key.Person.Name + " " + g.Key.Person.Surname,
                    ReceiptSum = g.Sum(r => r.ReceiptSum)
                })
                .OrderByDescending(cam => cam.ReceiptSum)
                .Take(customerCount);

            return customers;
        }
    }
}
