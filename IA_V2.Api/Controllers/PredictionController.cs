using AutoMapper;
using IA_V2.Api.Responses;
using IA_V2.Core.Entities;
using IA_V2.Core.Interfaces;
using IA_V2.Core.Services;
using IA_V2.Infrastructure.DTOs;
using IA_V2.Infrastructure.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using IA_V2.Core.ML;
using IA_V2.Core.ML.Data;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try

            {
                var predictions = await _predictionService.GetAllPredictionAsync();
                var result = _mapper.Map<IEnumerable<PredictionDTO>>(predictions);
                return Ok(result); 
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var validation = await _validationService.ValidateAsync(id);
                if (!validation.IsValid)
                    return BadRequest(new { errores = validation.Errors });
                var prediction = await _predictionService.GetPredictionByIdAsync(id);
                var predictionDto = _mapper.Map<PredictionDTO>(prediction);
                var response = new ApiResponse<PredictionDTO>(predictionDto);
                return Ok(response);
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }
        [HttpPost("predecir")]
        public IActionResult Predecir([FromBody] TextInput input)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(input.Texto))
                    return BadRequest(new { message = "Debe ingresar un texto para analizar." });

                var resultado = _modeloIAService.Predecir(input.Texto);

                return Ok(new
                {
                    TextoAnalizado = input.Texto,
                    Categoria = resultado.Categoria,
                    Confianza = resultado.Confidencias
                });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
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
    }
}


