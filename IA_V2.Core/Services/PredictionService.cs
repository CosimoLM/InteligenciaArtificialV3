using IA_V2.Core.Entities;
using IA_V2.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Core.Services
{
    public class PredictionService : IPredictionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDapperContext _dapper;

        // Constructor SIMPLE - solo necesita IBaseRepository<Prediction>
        public PredictionService(IUnitOfWork unitOfWork, IDapperContext dapperContext)
        {
            _unitOfWork = unitOfWork;
            _dapper = dapperContext;
        }

        public async Task<IEnumerable<Prediction>> GetAllPredictionAsync()
        {
            return await _unitOfWork.PredictionRepository.GetAll();
        }

        public async Task<Prediction> GetPredictionByIdAsync(int id)
        {
            try
            {
                var prediction = await _unitOfWork.PredictionRepository.GetById(id);
                if (prediction == null)
                    throw new Exception($"Predicción con ID {id} no encontrada");

                return prediction;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en PredictionService.GetPredictionByIdAsync: {ex.Message}", ex);
            }
        }


        public async Task DeletePredictionAsync(int id)
        {
            try
            {
                await _unitOfWork.PredictionRepository.Delete(id);
                await _unitOfWork.SaveChangesAsync();
            }   
            catch (Exception ex)
            {
                throw new Exception($"Error en PredictionService.DeletePredictionAsync: {ex.Message}", ex);
            }
        }
    }
}
