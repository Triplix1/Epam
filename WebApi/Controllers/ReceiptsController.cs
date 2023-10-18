using Business.Interfaces;
using Business.Models;
using Business.Services;
using Business.Validation;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private readonly IReceiptService _receiptService;
        public ReceiptsController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        // GET: api/receipts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReceiptModel>>> Get()
        {
            return Ok(await _receiptService.GetAllAsync());
        }


        // GET: api/receipts/period
        [HttpGet("period")]
        public async Task<ActionResult<IEnumerable<ReceiptModel>>> Get([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return Ok(await _receiptService.GetReceiptsByPeriodAsync(startDate, endDate));
        }


        //GET: api/receipts/1
        [HttpGet("{id}")]
        public async Task<ActionResult<ReceiptModel>> GetById(int id)
        {
            var receipt = await _receiptService.GetByIdAsync(id);

            if (receipt == null)
                return NotFound();

            return Ok(receipt);
        }

        //GET: api/products/1/details
        [HttpGet("{id}/details")]
        public async Task<ActionResult<ReceiptDetailModel>> GetByDetails(int id)
        {
            return Ok(await _receiptService.GetReceiptDetailsAsync(id));
        }

        //GET: api/products/1/sum
        [HttpGet("{id}/sum")]
        public async Task<ActionResult<ReceiptModel>> GetSumById(int id)
        {
            return Ok(await _receiptService.ToPayAsync(id));
        }

        // POST: api/receipts
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ReceiptModel value)
        {
            try
            {
                await _receiptService.AddAsync(value);
            }
            catch (MarketException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(value);
        }

        // PUT: api/receipts/1
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int Id, [FromBody] ReceiptModel value)
        {
            if (Id != value.Id)
                throw new MarketException();

            await _receiptService.UpdateAsync(value);
            return Ok();
        }

        // PUT: api/receipts/1/products/remove/1/1
        [HttpPut("{id}/products/remove/{productId}/{quantity}")]
        public async Task<ActionResult> UpdateRemoveProductFromReceipt(int id, int productId, int quantity)
        {
            await _receiptService.RemoveProductAsync(productId, id, quantity);
            return Ok();
        }

        // PUT: api/receipts/1/products/add/1/1
        [HttpPut("{id}/products/add/{productId}/{quantity}")]
        public async Task<ActionResult> UpdateAddProductFromReceipt(int id, int productId, int quantity)
        {
            await _receiptService.AddProductAsync(productId, id, quantity);
            return Ok();
        }

        // PUT: api/receipts/1/checkout
        [HttpPut("{id}/checkout")]
        public async Task<ActionResult> CheckOut(int id)
        {
            await _receiptService.CheckOutAsync(id);
            return Ok();
        }

        // DELETE: api/receipts/1
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _receiptService.DeleteAsync(id);
            return Ok();
        }
    }
}
