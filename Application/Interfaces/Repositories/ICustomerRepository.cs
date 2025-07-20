using Database.Entities;

namespace Application.Interfaces.Repositories
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer> GetWithSalesAsync(int id);
    }
}
