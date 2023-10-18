using Data.Data;
using Data.Entities;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private readonly TradeMarketDbContext _context;

        public ProductCategoryRepository(TradeMarketDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(ProductCategory entity)
        {
            await _context.ProductCategories.AddAsync(entity);
        }

        public void Delete(ProductCategory entity)
        {
            _context.ProductCategories.Remove(entity);  
        }

        public async Task DeleteByIdAsync(int id)
        {
            var productCategory = await _context.ProductCategories.FirstOrDefaultAsync(c => c.Id == id);
           
            if (productCategory != null)
            {
                _context.ProductCategories.Remove(productCategory);
            }
        }

        public async Task<IEnumerable<ProductCategory>> GetAllAsync()
        {
            return await _context.ProductCategories.ToListAsync();
        }

        public Task<ProductCategory> GetByIdAsync(int id)
        {
            return _context.ProductCategories.FirstOrDefaultAsync(c => c.Id == id); 
        }

        public void Update(ProductCategory entity)
        {
            _context.ProductCategories.Update(entity);
        }
    }
}
