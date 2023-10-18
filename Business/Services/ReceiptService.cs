using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Validation;
using Data.Entities;
using Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private const int MIN_YEAR = 1900;
        public ReceiptService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddAsync(ReceiptModel model)
        {
            if (model == null)
                throw new MarketException();

            if (model.OperationDate.Year < MIN_YEAR || model.OperationDate.Year > DateTime.Now.Year)
                throw new MarketException();

            var receipt = _mapper.Map<Receipt>(model);

            await _unitOfWork.ReceiptRepository.AddAsync(receipt);

            await _unitOfWork.SaveAsync();
        }

        public async Task AddProductAsync(int productId, int receiptId, int quantity)
        {
            var receipt = await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(receiptId);

            if (receipt == null)
                throw new MarketException();

            if (quantity <= 0)
                throw new MarketException();            

            var receiptDetailWithCurrentProduct = receipt.ReceiptDetails?.FirstOrDefault(rd => rd.ProductId == productId);

            if (receiptDetailWithCurrentProduct == null)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);

                if (product == null)
                    throw new MarketException();

                var discount = receipt.Customer.DiscountValue;

                var receiptDetails = new ReceiptDetail
                {
                    DiscountUnitPrice = product.Price * (100 - discount) / 100,
                    ProductId = productId,
                    Quantity = quantity,
                    ReceiptId = receiptId,
                    UnitPrice = product.Price,
                };

                await _unitOfWork.ReceiptDetailRepository.AddAsync(receiptDetails);
            }
            else
            {
                receiptDetailWithCurrentProduct.Quantity += quantity;
                _unitOfWork.ReceiptDetailRepository.Update(receiptDetailWithCurrentProduct);
            }            

            await _unitOfWork.SaveAsync();
        }

        public async Task CheckOutAsync(int receiptId)
        {
            var receipt = await _unitOfWork.ReceiptRepository.GetByIdAsync(receiptId);

            if (receipt == null)
                throw new MarketException();

            receipt.IsCheckedOut = true;

            _unitOfWork.ReceiptRepository.Update(receipt);

            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int modelId)
        {
            var receipt = await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(modelId);

            if (receipt == null)
                throw new MarketException();

            await _unitOfWork.ReceiptRepository.DeleteByIdAsync(modelId);

            foreach (var receiptDetails in receipt.ReceiptDetails)
            {
                _unitOfWork.ReceiptDetailRepository.Delete(receiptDetails);
            }
            
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<ReceiptModel>> GetAllAsync()
        {
            var receipts = await _unitOfWork.ReceiptRepository.GetAllWithDetailsAsync();
            return _mapper.Map<IEnumerable<ReceiptModel>>(receipts);
        }

        public async Task<ReceiptModel> GetByIdAsync(int id)
        {
            var receipt = await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(id);

            if (receipt == null)
                throw new MarketException();

            return _mapper.Map<ReceiptModel>(receipt);
        }

        public async Task<IEnumerable<ReceiptDetailModel>> GetReceiptDetailsAsync(int receiptId)
        {
            var receipt = await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(receiptId);

            if (receipt == null)
                throw new MarketException();

            return _mapper.Map<IEnumerable<ReceiptDetailModel>>(receipt.ReceiptDetails);
        }

        public async Task<IEnumerable<ReceiptModel>> GetReceiptsByPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var receipts = await _unitOfWork.ReceiptRepository.GetAllWithDetailsAsync();

            receipts = receipts.Where(r => r.OperationDate >= startDate && r.OperationDate <= endDate);

            return _mapper.Map<IEnumerable<ReceiptModel>>(receipts);
        }

        public async Task RemoveProductAsync(int productId, int receiptId, int quantity)
        {
            if (quantity <= 0)
                throw new MarketException();

            var receipt = await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(receiptId);

            if(receipt == null)
                throw new MarketException();

            var receiptDetails = receipt.ReceiptDetails.FirstOrDefault(r => r.ProductId == productId);

            if (receiptDetails == null)
                throw new MarketException();

            if (receiptDetails.Quantity <= quantity)
                _unitOfWork.ReceiptDetailRepository.Delete(receiptDetails);
            else
                receiptDetails.Quantity -= quantity;

            await _unitOfWork.SaveAsync();
        }

        public async Task<decimal> ToPayAsync(int receiptId)
        {
            var receipt = await _unitOfWork.ReceiptRepository.GetByIdWithDetailsAsync(receiptId);

            if (receipt == null)
                throw new MarketException();

            return receipt.ReceiptDetails.Sum(r => r.Quantity * r.DiscountUnitPrice);
        }

        public async Task UpdateAsync(ReceiptModel model)
        { 
            _unitOfWork.ReceiptRepository.Update(_mapper.Map<Receipt>(model));

            await _unitOfWork.SaveAsync();
        }
    }
}
