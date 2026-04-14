using POS.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POS.Core.Interfaces
{
    public interface ICustomerService
    {
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer> GetByIdAsync(int id);
        Task<int> AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(int id);
    }
}
