using Application.Interfaces.Repositories;
using Database.Contexts;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext db) : base(db) { }

        public async Task<Customer> GetWithSalesAsync(int id)
        {
            return await _db.Customers
                            .Include(c => c.Sales)
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
