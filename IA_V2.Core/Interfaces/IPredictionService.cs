using IA_V2.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Core.Interfaces
{
    public interface IPredictionService
    {
        Task<IEnumerable<Prediction>> GetAllPredictionAsync();
        //Task<IEnumerable<Prediction>> GetAllPredictionsDapperAsync(int limit = 10);
        Task<Prediction> GetPredictionByIdAsync(int id);
        //Task<Prediction> GetPredictionDapperAsync(int id);
        Task UpdatePredictionAsync(Prediction prediction);
        Task DeletePredictionAsync(int id);
    }
}
