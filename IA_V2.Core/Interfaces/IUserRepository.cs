using System;
using IA_V2.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Core.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
      
        //Task<IEnumerable<User>> GetAllUserByIdAsync(int idUser);
        Task<User> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersWithTextsAsync();

        Task<IEnumerable<User>> GetAllUsersDapperAsync(int limit = 10);
        Task<User> GetUserByIdDapperAsync(int id);
        Task<IEnumerable<User>> GetUsersPagedAsync(int pageNumber, int pageSize);
        Task<int> GetUsersCountAsync();
    }
}
