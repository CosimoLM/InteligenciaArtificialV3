using IA_V2.Core.CustomEntities;
using IA_V2.Core.Entities;
using IA_V2.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Core.Interfaces
{
    public interface ITextService
    {
        Task<IEnumerable<Text>> GetAllTextAsync();
        Task<ResponseData> GetAllTextAsync(TextQueryFilter filters);
        Task<Text> GetTextByIdAsync(int id);
        Task InsertTextAsync(Text text);
        Task UpdateTextAsync(Text text);
        Task DeleteTextAsync(int id);
    }
}
