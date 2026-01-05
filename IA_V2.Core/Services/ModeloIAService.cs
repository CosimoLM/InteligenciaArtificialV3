using IA_V2.Core.CustomEntities;
using IA_V2.Core.Entities;
using IA_V2.Core.Interfaces;
using IA_V3.Core.Interfaces;
using IA_V3_Core;
using Microsoft.Extensions.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Text = IA_V2.Core.Entities.Text;
namespace IA_V2.Core.Services
{
    public class ModeloIAService : IModeloIAService
    {
        private readonly PredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput> _pool;
        private readonly IUnitOfWork _unitOfWork;
        public ModeloIAService(PredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput> pool, IUnitOfWork unitOfWork)
        {
            _pool = pool;
            _unitOfWork = unitOfWork;
        }

        public async Task<Prediction> PredictTextByIdAsync(int textId)
        {
            try 
            {
                var text = await _unitOfWork.TextRepository.GetById(textId);
                if (text == null)
                    throw new Exception($"Text con ID {textId} no encontrado");

                var modelInput = new MLModel1.ModelInput
                {
                    Col3 = text.Content ?? "", // El texto a analizar
                    Col0 = 0,
                    Col1 = "",
                    Col2 = ""
                };

                var mlResult= _pool.Predict(modelInput);

                var prediction = new Prediction
                {
                    TextId = text.Id,          // ← RELACIÓN con Text
                    UserId = text.UserId,      // ← RELACIÓN con User (del Text)
                    Result = mlResult.PredictedLabel, // "Positive", "Negative", etc.
                    Probability = mlResult.Score?.Max() ?? 0f,
                    Date = DateTime.UtcNow
                };

                await _unitOfWork.PredictionRepository.Add(prediction);
                await _unitOfWork.SaveChangesAsync();

                return prediction;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error analizando texto {textId}: {ex.Message}", ex);
            }
        }

    }
}
/*// 1. Convertir texto a formato del modelo ML
            var modelInput = new MLModel1.ModelInput
            {
                Col3 = text.Content, // El comentario del usuario
                Col0 = 0,
                Col1 = "",
                Col2 = ""
            };

            // 2. Procesar con el modelo ML
            var mlResult = _pool.Predict(modelInput);

            // 3. Convertir resultado ML a tu entidad Prediction
            var prediction = new Prediction
            {
                TextId = text.Id,      // ← RELACIÓN: Text ya guardado
                UserId = text.UserId,  // ← RELACIÓN: User del Text
                Result = mlResult.PredictedLabel,
                Confidence = mlResult.Score?.Max() ?? 0f,
                Date = DateTime.UtcNow
            };

            // 4. Guardar en BD automáticamente
            await _unitOfWork.PredictionRepository.Add(prediction);
            await _unitOfWork.SaveChangesAsync();

            return prediction;*/
