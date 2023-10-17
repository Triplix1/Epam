using Data.Interfaces;
using Data.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Data.Data
{
    public class UnitOfWork
    {
        public UnitOfWork(ICustomerRepository customerRepository,
            IPersonRepository personRepository,
            IProductCategoryRepository productCategoryRepository,
            IProductRepository productRepository,
            IReceiptDetailRepository receiptDetailRepository,
            IReceiptRepository receiptRepository)
        {
            CustomerRepository = customerRepository;
            PersonRepository = personRepository;
            ProductCategoryRepository = productCategoryRepository;
            ProductRepository = productRepository;
            ReceiptDetailRepository = receiptDetailRepository;
            ReceiptRepository = receiptRepository;
        }

        public ICustomerRepository CustomerRepository { get; set; }
        public IPersonRepository PersonRepository { get; set; }
        public IProductCategoryRepository ProductCategoryRepository { get; set; }
        public IProductRepository ProductRepository { get; set; }
        public IReceiptDetailRepository ReceiptDetailRepository { get; set; }
        public IReceiptRepository ReceiptRepository { get; set; }
    }
}
