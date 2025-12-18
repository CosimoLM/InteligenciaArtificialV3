using AutoMapper;
using IA_V2.Api.Responses;
using IA_V2.Core.CustomEntities;
using IA_V2.Core.Entities;
using IA_V2.Core.Interfaces;
using IA_V2.Core.QueryFilters;
using IA_V2.Core.Services;
using IA_V2.Infrastructure.Data;
using IA_V2.Infrastructure.DTOs;
using IA_V2.Infrastructure.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IA_V2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictionController : ControllerBase
    {
        private readonly IPredictionService _predictionService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;
        private readonly ModeloIAService _modeloIAService;

        public PredictionController(IPredictionService predictionService, IMapper mapper, IValidationService validationService, ModeloIAService modeloIAService)
        {
            _predictionService = predictionService;
            _mapper = mapper;
            _validationService = validationService;
            _modeloIAService = modeloIAService;
        }

        [HttpGet("minimal-working")]
        public IActionResult MinimalWorking()
        {
            try
            {
                return Ok(new
                {
                    success = true,
                    message = "✅ Controller funciona sin dependencias",
                    timestamp = DateTime.UtcNow,
                    test = "Paso 1 completado"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("test-services")]
        public IActionResult TestServices(
            [FromServices] IServiceProvider serviceProvider)
        {
            try
            {
                var results = new System.Text.StringBuilder();
                results.AppendLine("🔍 Probando servicios registrados:");

                // 1. Probando PredictionService
                try
                {
                    var predictionService = serviceProvider.GetService<IPredictionService>();
                    results.AppendLine($"IPredictionService: {(predictionService != null ? "✅ REGISTRADO" : "❌ NO REGISTRADO")}");
                }
                catch (Exception ex)
                {
                    results.AppendLine($"IPredictionService ERROR: {ex.Message}");
                }

                // 2. Probando BaseRepository<Prediction>
                try
                {
                    var predictionRepo = serviceProvider.GetService<IBaseRepository<Prediction>>();
                    results.AppendLine($"IBaseRepository<Prediction>: {(predictionRepo != null ? "✅ REGISTRADO" : "❌ NO REGISTRADO")}");
                }
                catch (Exception ex)
                {
                    results.AppendLine($"IBaseRepository<Prediction> ERROR: {ex.Message}");
                }

                // 3. Probando DbContext
                try
                {
                    var context = serviceProvider.GetService<InteligenciaArtificialV2Context>();
                    results.AppendLine($"DbContext: {(context != null ? "✅ REGISTRADO" : "❌ NO REGISTRADO")}");
                }
                catch (Exception ex)
                {
                    results.AppendLine($"DbContext ERROR: {ex.Message}");
                }

                return Ok(results.ToString());
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PredictionQueryFilter filters)
        {
            try
            {
                Console.WriteLine("🎯 GetAll Predictions llamado");

                // 1. Validación básica
                if (filters.PageNumber < 1) filters.PageNumber = 1;
                if (filters.PageSize < 1 || filters.PageSize > 100)
                    filters.PageSize = 10;

                // 2. Obtener predicciones
                Console.WriteLine("🔍 Llamando a GetAllPredictionAsync...");
                var predictions = await _predictionService.GetAllPredictionAsync();
                Console.WriteLine($"✅ Predicciones obtenidas: {predictions?.Count() ?? 0}");

                // 3. Aplicar filtros SI HAY predicciones
                if (predictions != null)
                {
                    var query = predictions.AsQueryable();

                    if (filters.UserId.HasValue)
                    {
                        Console.WriteLine($"🔍 Aplicando filtro UserId: {filters.UserId}");
                        query = query.Where(p => p.UserId == filters.UserId.Value);
                    }

                    if (filters.TextId.HasValue)
                    {
                        Console.WriteLine($"🔍 Aplicando filtro TextId: {filters.TextId}");
                        query = query.Where(p => p.TextId == filters.TextId.Value);
                    }

                    if (!string.IsNullOrEmpty(filters.Result))
                    {
                        Console.WriteLine($"🔍 Aplicando filtro Result: {filters.Result}");
                        query = query.Where(p => p.Result != null && p.Result.Contains(filters.Result));
                    }

                    if (filters.MinProbability.HasValue)
                    {
                        Console.WriteLine($"🔍 Aplicando filtro MinProbability: {filters.MinProbability}");
                        query = query.Where(p => p.Confidence >= filters.MinProbability.Value);
                    }

                    if (filters.FromDate.HasValue)
                    {
                        Console.WriteLine($"🔍 Aplicando filtro FromDate: {filters.FromDate}");
                        query = query.Where(p => p.Date >= filters.FromDate.Value);
                    }

                    predictions = query.ToList();
                }

                // 4. Paginación
                var pagedPredictions = PagedList<Prediction>.Create(predictions ?? new List<Prediction>(),
                    filters.PageNumber, filters.PageSize);

                // 5. Mapear
                var predictionsDto = _mapper.Map<IEnumerable<PredictionDTO>>(pagedPredictions);

                // 6. Respuesta
                var response = new ApiResponse<IEnumerable<PredictionDTO>>(predictionsDto)
                {
                    Pagination = new Pagination
                    {
                        TotalCount = pagedPredictions.TotalCount,
                        PageSize = pagedPredictions.PageSize,
                        CurrentPage = pagedPredictions.CurrentPage,
                        TotalPages = pagedPredictions.TotalPages,
                        HasNextPage = pagedPredictions.HasNextPage,
                        HasPreviousPage = pagedPredictions.HasPreviousPage
                    },
                    Messages = new Message[] { new() {
                        Type = "Success",
                        Description = $"Se encontraron {pagedPredictions.TotalCount} predicciones"
                    }}
                };

                Console.WriteLine($"✅ GetAll exitoso. Total: {pagedPredictions.TotalCount}");
                return Ok(response);
            }
            catch (Exception err)
            {
                Console.WriteLine($"💥 ERROR en GetAll: {err.Message}");
                Console.WriteLine($"📋 StackTrace: {err.StackTrace}");

                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "Error",
                            Description = $"Error al obtener predicciones: {err.Message}"
                        }}
                    });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                Console.WriteLine($"🎯 GetById Predictions llamado con ID: {id}");

                // 1. Validación simple
                if (id <= 0)
                {
                    Console.WriteLine("❌ ID inválido");
                    return BadRequest(new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "ValidationError",
                            Description = "El ID debe ser mayor a 0"
                        }}
                    });
                }

                // 2. Obtener predicción
                Console.WriteLine($"🔍 Llamando a GetPredictionByIdAsync({id})...");
                var prediction = await _predictionService.GetPredictionByIdAsync(id);

                if (prediction == null)
                {
                    Console.WriteLine($"⚠️ Predicción con ID {id} NO encontrada");
                    return NotFound(new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "Warning",
                            Description = $"Predicción con ID {id} no encontrada"
                        }}
                    });
                }

                Console.WriteLine($"✅ Predicción encontrada: ID={prediction.Id}, Result={prediction.Result}");

                // 3. Preparar respuesta
                var predictionDto = _mapper.Map<PredictionDTO>(prediction);
                var response = new ApiResponse<PredictionDTO>(predictionDto)
                {
                    Messages = new Message[] { new() {
                        Type = "Success",
                        Description = "Predicción recuperada correctamente"
                    }}
                };

                return Ok(response);
            }
            catch (Exception err)
            {
                Console.WriteLine($"💥 ERROR en GetById({id}): {err.Message}");
                Console.WriteLine($"📋 StackTrace: {err.StackTrace}");

                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "Error",
                            Description = $"Error al obtener predicción: {err.Message}"
                        }}
                    });
            }
        }
        //[HttpGet]
        //[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<PredictionDTO>>))]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> GetAll([FromQuery] PredictionQueryFilter filters)
        //{
        //    try
        //    {
        //        if (filters.PageNumber < 1) filters.PageNumber = 1;
        //        if (filters.PageSize < 1 || filters.PageSize > 100)
        //            filters.PageSize = 10;

        //        var predictions = await _predictionService.GetAllPredictionAsync();

        //        var pagedPredictions = PagedList<Prediction>.Create(predictions, filters.PageNumber, filters.PageSize);

        //        var predictionsDto = _mapper.Map<IEnumerable<PredictionDTO>>(pagedPredictions);

        //        var response = new ApiResponse<IEnumerable<PredictionDTO>>(predictionsDto)
        //        {
        //            Pagination = new Pagination
        //            {
        //                TotalCount = pagedPredictions.TotalCount,
        //                PageSize = pagedPredictions.PageSize,
        //                CurrentPage = pagedPredictions.CurrentPage,
        //                TotalPages = pagedPredictions.TotalPages,
        //                HasNextPage = pagedPredictions.HasNextPage,
        //                HasPreviousPage = pagedPredictions.HasPreviousPage
        //            },
        //            Messages = new Message[] { new() {
        //                Type = "Success",
        //                Description = $"Se encontraron {pagedPredictions.TotalCount} predicciones"
        //            }}
        //        };

        //        return Ok(response);
        //    }
        //    catch (Exception err)
        //    {
        //        return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
        //    }

        //}

        //[HttpGet("{id}")]
        //[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PredictionDTO>))]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        //public async Task<IActionResult> GetById(int id)
        //{
        //    try
        //    {


        //        var prediction = await _predictionService.GetPredictionByIdAsync(id);

        //        var predictionDto = _mapper.Map<PredictionDTO>(prediction);

        //        var validation = await _validationService.ValidateAsync(predictionDto);
        //        if (!validation.IsValid)
        //            return BadRequest(new { errores = validation.Errors });

        //        var response = new ApiResponse<PredictionDTO>(predictionDto)
        //        {
        //            Messages = new Message[] { new() {
        //                Type = "Success",
        //                Description = "Predicción recuperada correctamente"
        //            }}
        //        };
        //        return Ok(response);
        //    }
        //    catch (Exception err)
        //    {
        //        return StatusCode((int)HttpStatusCode.InternalServerError,
        //            new ApiResponse<object>(null)
        //            {
        //                Messages = new Message[] { new() {
        //                    Type = "Error",
        //                    Description = $"Error al obtener predicción: {err.Message}"
        //                }}
        //            });
        //    }
        //}

        /// <summary>
        /// Analiza un texto existente por su ID y genera una predicción
        /// </summary>
        /// <param name="textId">ID del texto a analizar</param>
        /// <returns>Predicción generada con categoría y confianza</returns>
        [HttpPost("predict/{textId}")]
        [ProducesResponseType(typeof(ApiResponse<Prediction>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> AnalyzeText(int textId)
        {
            try
            {
                // 1. Verificar que el usuario tiene acceso a este Text
                // (Opcional: validar que el Text pertenece al usuario logueado)

                // 2. Procesar con IA
                var prediction = await _modeloIAService.PredictTextByIdAsync(textId);

                // 3. Responder con la Prediction completa
                var predictionDTO=_mapper.Map<PredictionDTO>(prediction);
                var response = new ApiResponse<Prediction>(prediction)
                {
                    Messages = new Message[] { new() {
                        Type = "Success",
                        Description = $"Texto analizado: {prediction.Result} ({prediction.Confidence:CO} confianza)"
                    }}
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<Prediction>(null)
                {
                    Messages = new Message[] { new() {
                        Type = "Error",
                        Description = ex.Message
                    }}
                };
                return BadRequest(errorResponse);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PredictionDTO dto)
        {
            try
            {
                dto.Id = id;
                var validation = await _validationService.ValidateAsync(dto);
                if (!validation.IsValid)
                    return BadRequest(new { errores = validation.Errors });

                var prediction = await _predictionService.GetPredictionByIdAsync(id);

                _mapper.Map(dto, prediction);

                await _predictionService.UpdatePredictionAsync(prediction);

                var predictionDto = _mapper.Map<PredictionDTO>(prediction);
                var response = new ApiResponse<PredictionDTO>(predictionDto);
                return Ok(response);
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var validation = await _validationService.ValidateAsync(id);
                if (!validation.IsValid)
                    return BadRequest(new { errores = validation.Errors });
                await _predictionService.DeletePredictionAsync(id);
                return Ok(new { message = $"La prediccion con ID {id} fue eliminada correctamente." });
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }

        [HttpGet("diagnostic")]
        public async Task<IActionResult> Diagnostic()
        {
            try
            {
                var diagnostics = new
                {
                    Timestamp = DateTime.UtcNow,

                    // 1. Servicios
                    PredictionService = _predictionService != null ? "OK" : "NULL",
                    PredictionServiceType = _predictionService?.GetType().Name,
                    Mapper = _mapper != null ? "OK" : "NULL",

                    // 2. Base de datos - prueba simple
                    DatabaseTest = await TestDatabaseConnection(),

                    // 3. Configuración
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",

                    // 4. Ensamblados cargados
                    LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => a.FullName.Contains("IA_V2"))
                        .Select(a => a.GetName().Name)
                        .ToList()
                };

                return Ok(diagnostics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }

        private async Task<string> TestDatabaseConnection()
        {
            try
            {
                // Intenta obtener una predicción cualquiera
                var predictions = await _predictionService.GetAllPredictionAsync();
                return $"OK - {predictions?.Count() ?? 0} predicciones";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }
    }

}


