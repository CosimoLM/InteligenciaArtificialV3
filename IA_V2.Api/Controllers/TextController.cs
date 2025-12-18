using AutoMapper;
using IA_V2.Api.Responses;
using IA_V2.Core.CustomEntities;
using IA_V2.Core.Entities;
using IA_V2.Core.Exceptions;
using IA_V2.Core.Interfaces;
using IA_V2.Core.QueryFilters;
using IA_V2.Infrastructure.DTOs;
using IA_V2.Infrastructure.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace IA_V2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextController : ControllerBase
    {
        private readonly ITextService _textService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;

        public TextController(ITextService textService, IMapper mapper, IValidationService validationService)
        {
            _textService = textService;
            _mapper = mapper;
            _validationService = validationService;
        }
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<IEnumerable<TextDTO>>))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TextQueryFilter filters)
        {
            try
            {
                if (filters.PageNumber < 1) filters.PageNumber = 1;
                if (filters.PageSize < 1 || filters.PageSize > 100)
                    filters.PageSize = 10;

                var texts = await _textService.GetAllTextAsync();

                // Aplicar filtros
                if (filters.UserId.HasValue)
                    texts = texts.Where(t => t.UserId == filters.UserId.Value);

                if (!string.IsNullOrEmpty(filters.SearchText))
                    texts = texts.Where(t => t.Content.Contains(filters.SearchText));

                if (filters.FromDate.HasValue)
                    texts = texts.Where(t => t.FechaEnvio >= filters.FromDate.Value);

                if (filters.ToDate.HasValue)
                    texts = texts.Where(t => t.FechaEnvio <= filters.ToDate.Value);

                // Aplicar paginación
                var pagedTexts = PagedList<Text>.Create(texts, filters.PageNumber, filters.PageSize);
                var textsDto = _mapper.Map<IEnumerable<TextDTO>>(pagedTexts);

                var response = new ApiResponse<IEnumerable<TextDTO>>(textsDto)
                {
                    Pagination = new Pagination
                    {
                        TotalCount = pagedTexts.TotalCount,
                        PageSize = pagedTexts.PageSize,
                        CurrentPage = pagedTexts.CurrentPage,
                        TotalPages = pagedTexts.TotalPages,
                        HasNextPage = pagedTexts.HasNextPage,
                        HasPreviousPage = pagedTexts.HasPreviousPage
                    },
                    Messages = new Message[] { new() {
                        Type = "Success",
                        Description = $"Se encontraron {pagedTexts.TotalCount} textos"
                    }}
                };

                return Ok(response);
            }
            catch (Exception err)
            {
                var responsePost = new ResponseData()
                {
                    Messages = new Message[] { new() { Type = "Error", Description = err.Message } },
                };
                return StatusCode(500, responsePost);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<TextDTO>))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                
                var text = await _textService.GetTextByIdAsync(id);
                var textDto = _mapper.Map<TextDTO>(text);
                var validation = await _validationService.ValidateAsync(textDto);
                if (!validation.IsValid)
                    return BadRequest(new { errores = validation.Errors });
                var response = new ApiResponse<TextDTO>(textDto)
                {
                    Messages = new Message[] { new() {
                        Type = "Success",
                        Description = "Texto recuperado correctamente"
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
                            Description = $"Error al obtener texto: {err.Message}"
                        }}
                    });
            }
        }
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created, Type = typeof(ApiResponse<TextDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Insert(TextDTO dto)
        {
            try
            {
                var validation = await _validationService.ValidateAsync(dto);
                if (!validation.IsValid)
                    return BadRequest(new { errores = validation.Errors });

                var text = _mapper.Map<Text>(dto);
                await _textService.InsertTextAsync(text);

                var response = _mapper.Map<TextDTO>(text);
                return Ok(response);
            }
            catch (BusinessException bex)
            {
                return BadRequest(new ApiResponse<object>(null)
                {
                    Messages = new Message[] { new() {
                        Type = "BusinessError",
                        Description = bex.Message
                    }}
                });
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "Error",
                            Description = $"Error al crear texto: {err.Message}"
                        }}
                    });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<TextDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] TextDTO dto)
        {
            try
            {
                dto.Id = id;
                var validation = await _validationService.ValidateAsync(dto);
                if (!validation.IsValid)
                    return BadRequest(new { errores = validation.Errors });

                var text = await _textService.GetTextByIdAsync(id);

                _mapper.Map(dto, text);

                await _textService.UpdateTextAsync(text);

                var textDto = _mapper.Map<TextDTO>(text);
                var response = new ApiResponse<TextDTO>(textDto);
                return Ok(response);
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var validation = await _validationService.ValidateAsync(id);
                if (!validation.IsValid)
                    return BadRequest(new { errores = validation.Errors });
                await _textService.DeleteTextAsync(id);
                return Ok(new { message = $"El texto con ID {id} fue eliminado correctamente." });
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }
    }
}
