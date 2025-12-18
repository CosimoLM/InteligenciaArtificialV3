using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IA_V2.Core.Entities;

namespace IA_V2.Core.Interfaces
{
    public interface ITextRepository : IBaseRepository<Text>
    {
        Task<IEnumerable<Text>> GetAllTextsDapperAsync(int limit = 10);
        Task<Text> GetTextByIdDapperAsync(int id);
        Task<IEnumerable<Text>> GetTextsByUserDapperAsync(int userId);
        Task<IEnumerable<Text>> GetRecentTextsWithPredictionsAsync();
    }
}
