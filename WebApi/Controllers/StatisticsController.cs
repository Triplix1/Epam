using Business.Interfaces;
using Business.Models;
using Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticService _statisticService;

        public StatisticsController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [HttpGet("popularProducts")]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetMostPopularProducts([FromQuery] int productCount)
        {
            return Ok(await _statisticService.GetMostPopularProductsAsync(productCount));
        }

        [HttpGet("customer/{id}/{productCount}")]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetCustomersMostPopularProducts(int id, int productCount)
        {
            return Ok(await _statisticService.GetCustomersMostPopularProductsAsync(productCount,id));
        }

        [HttpGet("activity/{customerCount}")]
        public async Task<ActionResult<IEnumerable<CustomerActivityModel>>> GetActivity([FromRoute] int customerCount,[FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return Ok(await _statisticService.GetMostValuableCustomersAsync(customerCount,startDate,endDate));
        }

        [HttpGet("income/{categoryId}")]
        public async Task<ActionResult<decimal>> GetIncome([FromRoute] int categoryId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            return Ok(await _statisticService.GetIncomeOfCategoryInPeriod(categoryId, startDate, endDate));
        }
    }
}
