using IA_V2.Core.CustomEntities;
using IA_V2.Core.Entities;
using IA_V2.Core.Interfaces;
using IA_V2.Core.ML;
using IA_V2.Core.ML.Data;
using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextTrainingData = IA_V2.Core.ML.Data.TextTrainingData;
namespace IA_V2.Core.Services
{
    public class ModeloIAService
    {
        private readonly MLContext _mlContext;
        private readonly string _modeloPath = "modeloIA.zip";
        private PredictionEngine<TextInput, TextPrediction> _predEngine;
        private readonly IUnitOfWork _unitOfWork;

        public ModeloIAService(IUnitOfWork unitOfWork)
        {
            _mlContext = new MLContext();
            _unitOfWork = unitOfWork;

            // Cargar o entrenar modelo
            if (!File.Exists(_modeloPath))
            {
                Console.WriteLine("No se encontró el modelo. Entrenando uno nuevo...");
                EntrenarYGuardarModelo();
            }

            // Cargar modelo existente
            CargarModelo();
        }

        public ModeloIAService()
        {
        }

        private void CargarModelo()
        {
            try
            {
                using var stream = File.OpenRead(_modeloPath);
                var loadedModel = _mlContext.Model.Load(stream, out _);
                _predEngine = _mlContext.Model.CreatePredictionEngine<TextInput, TextPrediction>(loadedModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando modelo: {ex.Message}");
                // Reentrenar si hay error
                EntrenarYGuardarModelo();
                CargarModelo();
            }
        }

        /// Entrena y guarda el modelo de IA con datos de la base de datos
        private void EntrenarYGuardarModelo()
        {
            try
            {
                // Obtener datos de entrenamiento de la base de datos
                var trainingData = ObtenerDatosEntrenamientoDesdeBD().Result;

                if (!trainingData.Any())
                {
                    // Datos de entrenamiento por defecto si no hay en BD
                    trainingData = new List<TextTrainingData>
                    {
                        new TextTrainingData { Texto = "El día está hermoso", Label = "Positivo" },
                        new TextTrainingData { Texto = "Odio este clima", Label = "Negativo" },
                        new TextTrainingData { Texto = "Amo mi trabajo", Label = "Positivo" },
                        new TextTrainingData { Texto = "Estoy cansado del tráfico", Label = "Negativo" },
                        new TextTrainingData { Texto = "Excelente servicio al cliente", Label = "Positivo" },
                        new TextTrainingData { Texto = "Pésima experiencia", Label = "Negativo" }
                    };
                }

                var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

                // Pipeline de procesamiento mejorado
                var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(TextTrainingData.Texto))
                    .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label"))
                    .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                        labelColumnName: "Label",
                        featureColumnName: "Features"))
                    .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

                // Entrenar el modelo
                var model = pipeline.Fit(dataView);

                // Guardar modelo
                _mlContext.Model.Save(model, dataView.Schema, _modeloPath);
                Console.WriteLine($"Modelo entrenado con {trainingData.Count} ejemplos y guardado en {_modeloPath}");

                // Recargar el modelo
                CargarModelo();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error entrenando el modelo: {ex.Message}", ex);
            }
        }

        /// Obtiene datos de entrenamiento desde la base de datos
        private async Task<List<TextTrainingData>> ObtenerDatosEntrenamientoDesdeBD()
        {
            var trainingData = new List<TextTrainingData>();

            try
            {
                // Obtener predicciones existentes como datos de entrenamiento
                var predictions = await _unitOfWork.PredictionRepository.GetAll();

                foreach (var prediction in predictions)
                {
                    // Buscar el texto asociado
                    var text = await _unitOfWork.TextRepository.GetById(prediction.TextId ?? 0);
                    if (text != null && !string.IsNullOrEmpty(text.Content))
                    {
                        trainingData.Add(new TextTrainingData
                        {
                            Texto = text.Content,
                            Label = prediction.Result ?? "Neutral"
                        });
                    }
                }

                Console.WriteLine($"Se obtuvieron {trainingData.Count} ejemplos de entrenamiento desde BD");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo datos de entrenamiento: {ex.Message}");
            }

            return trainingData;
        }

        /// Realiza una predicción y guarda el resultado en la base de datos
        public async Task<PredictionResult> PredecirYGuardarAsync(string texto, int? userId = null, int? textId = null)
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                // Realizar predicción
                var prediction = _predEngine.Predict(new TextInput { Texto = texto });

                watch.Stop();

                // Guardar en base de datos
                var predictionEntity = new Prediction
                {
                    TextId = textId,
                    UserId = userId,
                    Result = prediction.Categoria,
                    Probability = prediction.Confidencias?.Max() ?? 0.0,
                    Date = DateTime.UtcNow
                };

                await _unitOfWork.PredictionRepository.Add(predictionEntity);
                await _unitOfWork.SaveChangesAsync();

                return new PredictionResult
                {
                    Categoria = prediction.Categoria,
                    Confidencias = prediction.Confidencias,
                    PredictionId = predictionEntity.Id,
                    Probability = predictionEntity.Probability,
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en predicción: {ex.Message}", ex);
            }
        }

        /// Reentrena el modelo de forma asíncrona con nuevos datos
        private async Task ReentrenarModeloAsync()
        {
            try
            {
                Console.WriteLine("Iniciando reentrenamiento del modelo...");
                EntrenarYGuardarModelo();
                Console.WriteLine("Reentrenamiento completado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en reentrenamiento: {ex.Message}");
            }
        }

        /// Obtiene estadísticas del modelo y predicciones
        public async Task<ModelStats> GetEstadisticasAsync()
        {
            var predictions = await _unitOfWork.PredictionRepository.GetAll();
            var totalPredictions = predictions.Count();

            return new ModelStats
            {
                TotalPredicciones = totalPredictions,
                PrecisionPromedio = predictions.Any() ? predictions.Average(p => p.Probability) : 0,
                Categorias = predictions.GroupBy(p => p.Result)
                    .Select(g => new CategoriaStats
                    {
                        Nombre = g.Key ?? "Sin categoría",
                        Cantidad = g.Count(),
                        PrecisionPromedio = g.Average(p => p.Probability)
                    })
                    .OrderByDescending(c => c.Cantidad)
                    .ToList(),
                UltimoEntrenamiento = File.GetLastWriteTime(_modeloPath)
            };
        }

        // Método original mantenido para compatibilidad
        public TextPrediction Predecir(string texto)
        {
            return _predEngine.Predict(new TextInput { Texto = texto });
        }
    }

    // Clases adicionales para mejor manejo de resultados
    public class PredictionResult
    {
        public string Categoria { get; set; } = string.Empty;
        public float[] Confidencias { get; set; }
        public int PredictionId { get; set; }
        public double Probability { get; set; }
    }

    public class ModelStats
    {
        public int TotalPredicciones { get; set; }
        public double PrecisionPromedio { get; set; }
        public List<CategoriaStats> Categorias { get; set; } = new();
        public DateTime UltimoEntrenamiento { get; set; }
    }

    public class CategoriaStats
    {
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double PrecisionPromedio { get; set; }
    }
}
