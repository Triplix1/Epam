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
    public class PersonRepository : IPersonRepository
    {
        private readonly TradeMarketDbContext _context;

        public PersonRepository(TradeMarketDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Person entity)
        {
            await _context.Persons.AddAsync(entity);

            await _context.SaveChangesAsync();
        }

        public void Delete(Person entity)
        {
            _context.Persons.Remove(entity);

            _context.SaveChanges();
        }

        public async Task DeleteByIdAsync(int id)
        {
            var person = _context.Persons.FirstOrDefault(x => x.Id == id);

            if (person != null)
            {
                _context.Persons.Remove(person);

                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _context.Persons.ToListAsync();
        }

        public async Task<Person> GetByIdAsync(int id)
        {
            return await _context.Persons.FirstOrDefaultAsync(p => p.Id == id);
        }

        public void Update(Person entity)
        {
            _context.Persons.Update(entity);

            _context.SaveChanges();
        }
    }
}
