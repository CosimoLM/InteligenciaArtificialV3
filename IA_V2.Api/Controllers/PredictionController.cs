using AutoMapper;
using IA_V2.Api.Responses;
using IA_V2.Core.CustomEntities;
using IA_V2.Core.Entities;
using IA_V2.Core.Enum;
using IA_V2.Core.Interfaces;
using IA_V2.Core.QueryFilters;
using IA_V2.Core.Services;
using IA_V2.Infrastructure.Data;
using IA_V2.Infrastructure.DTOs;
using IA_V2.Infrastructure.Validators;
using IA_V3.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IA_V2.Api.Controllers
{
    [Authorize(Roles = $"{nameof(RoleType.Administrator)},{nameof(RoleType.User)}")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PredictionController : ControllerBase
    {
        private readonly IPredictionService _predictionService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;
        private readonly IModeloIAService _modeloIAService;

        public PredictionController(IPredictionService predictionService, IMapper mapper, IValidationService validationService, IModeloIAService modeloIAService)
        {
            _predictionService = predictionService;
            _mapper = mapper;
            _validationService = validationService;
            _modeloIAService = modeloIAService;
        }

        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<PredictionDTO>>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] PredictionQueryFilter filters)
        {
            try
            {
                // Validación de paginación
                if (filters.PageNumber < 1) filters.PageNumber = 1;
                if (filters.PageSize < 1 || filters.PageSize > 100)
                    filters.PageSize = 10;

                // Obtener predicciones 
                var predictions = await _predictionService.GetAllPredictionAsync();

                // Aplicar filtros en memoria 
                if (filters.UserId.HasValue)
                    predictions = predictions.Where(p => p.UserId == filters.UserId.Value);

                if (filters.TextId.HasValue)
                    predictions = predictions.Where(p => p.TextId == filters.TextId.Value);

                if (!string.IsNullOrEmpty(filters.Result))
                    predictions = predictions.Where(p => p.Result != null && p.Result.Contains(filters.Result));

                if (filters.MinProbability.HasValue)
                    predictions = predictions.Where(p => p.Probability >= filters.MinProbability.Value);

                if (filters.FromDate.HasValue)
                    predictions = predictions.Where(p => p.Date >= filters.FromDate.Value);

                // Paginación (igual que UserController)
                var pagedPredictions = PagedList<Prediction>.Create(predictions, filters.PageNumber, filters.PageSize);
                var predictionsDto = _mapper.Map<IEnumerable<PredictionDTO>>(pagedPredictions);

                // Respuesta (igual que UserController)
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

                return Ok(response);
            }
            catch (Exception err)
            {
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
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<PredictionDTO>))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // Validación simple
                if (id <= 0)
                    return BadRequest(new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "ValidationError",
                            Description = "El ID debe ser mayor a 0"
                        }}
                    });

                // Obtener predicción
                var prediction = await _predictionService.GetPredictionByIdAsync(id);

                if (prediction == null)
                    return NotFound(new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "Warning",
                            Description = $"Predicción con ID {id} no encontrada"
                        }}
                    });

                var predictionDto = _mapper.Map<PredictionDTO>(prediction);

                // Validar DTO
                var validation = await _validationService.ValidateAsync(predictionDto);
                if (!validation.IsValid)
                    return BadRequest(new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "ValidationError",
                            Description = "Error de validación"
                        }}
                    });

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
                var response = new ApiResponse<PredictionDTO>(predictionDTO);
                //{
                //    Messages = new Message[] { new() {
                //        Type = "Success",
                //        Description = $"Texto analizado: {prediction.Result} ({prediction.Confidence:CO} confianza)"
                //    }}
                //};

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

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Prediction>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                //var dto = new PredictionDTO();
                //var validation = await _validationService.ValidateAsync(dto);
                //if (!validation.IsValid)
                //return BadRequest(new { errores = validation.Errors });
                if (id <= 0)
                    return BadRequest("El ID debe ser mayor a 0");


                await _predictionService.DeletePredictionAsync(id);
                return Ok(new { message = $"La prediccion con ID {id} fue eliminada correctamente." });
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }
    }

}


