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
    public class ReceiptDetailRepository : IReceiptDetailRepository
    {
        private readonly TradeMarketDbContext _context;

        public ReceiptDetailRepository(TradeMarketDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(ReceiptDetail entity)
        {
            await _context.ReceiptsDetails.AddAsync(entity);
        }

        public void Delete(ReceiptDetail entity)
        {
            _context.ReceiptsDetails.Remove(entity);    
        }

        public async Task DeleteByIdAsync(int id)
        {
            var details = await _context.ReceiptsDetails.FirstOrDefaultAsync(d => d.Id == id);

            if (details != null)
            {
                _context.ReceiptsDetails.Remove(details);
            }
        }

        public async Task<IEnumerable<ReceiptDetail>> GetAllAsync()
        {
            return await _context.ReceiptsDetails.ToListAsync();
        }

        public async Task<IEnumerable<ReceiptDetail>> GetAllWithDetailsAsync()
        {
            return await _context.ReceiptsDetails
                .Include(d => d.Product)
                .ThenInclude(p => p.Category)
                .Include(d => d.Receipt)
                .ThenInclude(r => r.Customer)
                .ThenInclude(c => c.Person)
                .ToListAsync();
        }

        public async Task<ReceiptDetail> GetByIdAsync(int id)
        {
            return await _context.ReceiptsDetails.FirstOrDefaultAsync(d => d.Id == id);  
        }

        public void Update(ReceiptDetail entity)
        {
            _context.Update(entity);
        }
    }
}
