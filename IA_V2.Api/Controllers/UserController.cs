using AutoMapper;
using IA_V2.Api.Responses;
using IA_V2.Core.CustomEntities;
using IA_V2.Core.Entities;
using IA_V2.Core.Exceptions;
using IA_V2.Core.Interfaces;
using IA_V2.Core.QueryFilters;
using IA_V2.Infrastructure.DTOs;
using IA_V2.Infrastructure.Repositories;
using IA_V2.Infrastructure.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IA_V2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IValidationService _validationService;
        private readonly IPasswordService _passwordService;

        public UserController(IUserService userService, IMapper mapper, IValidationService validationService, IPasswordService passwordService)
        {
            _userService = userService;
            _mapper = mapper;
            _validationService = validationService;
            _passwordService = passwordService;
        }
        [HttpGet("test-unitofwork")]
        public IActionResult TestUnitOfWork([FromServices] IUnitOfWork unitOfWork)
        {
            try
            {
                var diagnostics = new List<string>();

                // 1. Verificar UnitOfWork
                diagnostics.Add($"UnitOfWork: {(unitOfWork != null ? "✅ INYECTADO" : "❌ NULL")}");
                diagnostics.Add($"UnitOfWork Type: {unitOfWork?.GetType().Name}");

                // 2. Verificar PredictionRepository
                try
                {
                    var predictionRepo = unitOfWork?.PredictionRepository;
                    diagnostics.Add($"PredictionRepository: {(predictionRepo != null ? "✅ EXISTE" : "❌ NULL")}");
                    diagnostics.Add($"PredictionRepository Type: {predictionRepo?.GetType().Name}");
                }
                catch (Exception ex)
                {
                    diagnostics.Add($"PredictionRepository ERROR: {ex.Message}");
                }

                // 3. Verificar UserRepository (para comparar)
                try
                {
                    var userRepo = unitOfWork?.UserRepository;
                    diagnostics.Add($"UserRepository: {(userRepo != null ? "✅ EXISTE" : "❌ NULL")}");
                }
                catch (Exception ex)
                {
                    diagnostics.Add($"UserRepository ERROR: {ex.Message}");
                }

                return Ok(new { diagnostics });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryFilter filters)
        {
            try
            {
                // Validación de paginación (IMPORTANTE)
                if (filters.PageNumber < 1) filters.PageNumber = 1;
                if (filters.PageSize < 1 || filters.PageSize > 100)
                    filters.PageSize = 10;

                var users = await _userService.GetAllUsersDapperAsync(1000);

                if (!string.IsNullOrEmpty(filters.SearchName))
                    users = users.Where(u => u.Name.Contains(filters.SearchName));

                if (!string.IsNullOrEmpty(filters.SearchEmail))
                    users = users.Where(u => u.Email.Contains(filters.SearchEmail));

                var pagedUsers = PagedList<User>.Create(users, filters.PageNumber, filters.PageSize);
                var usersDto = _mapper.Map<IEnumerable<UserDTO>>(pagedUsers);

                // RESPONSE COMPLETA CON PAGINATION
                var response = new ApiResponse<IEnumerable<UserDTO>>(usersDto)
                {
                    Pagination = new Pagination
                    {
                        TotalCount = pagedUsers.TotalCount,
                        PageSize = pagedUsers.PageSize,
                        CurrentPage = pagedUsers.CurrentPage,
                        TotalPages = pagedUsers.TotalPages,
                        HasNextPage = pagedUsers.HasNextPage,
                        HasPreviousPage = pagedUsers.HasPreviousPage
                    },
                    Messages = new Message[] { new() {
                Type = "Information",
                Description = "Usuarios recuperados correctamente"
            }}
                };

                return Ok(response);
            }
            catch (Exception err)
            {
                // Error estructurado
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                    Type = "Error",
                    Description = err.Message
                }}
                    });
            }

        }

        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<UserDTO>))]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try 
            {
                

                var user = await _userService.GetUserDapperAsync(id);

                if (user == null)
                    return NotFound(new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "Warning",
                            Description = $"Usuario con ID {id} no encontrado"
                        }}
                    });

                var userDto = _mapper.Map<UserDTO>(user);
                var validation = await _validationService.ValidateAsync(userDto);
                if (!validation.IsValid)
                    return BadRequest(new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "ValidationError",
                            Description = "Error de validación"
                        }}
                    });
                var response = new ApiResponse<UserDTO>(userDto)
                {
                    Messages = new Message[] { new() {
                        Type = "Success",
                        Description = "Usuario recuperado correctamente"
                    }}
                };
                return Ok(response);
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
           
        }
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ApiResponse<UserDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Insert(UserDTO dto)
        {
            try
            {
                var validation = await _validationService.ValidateAsync(dto);
                if (!validation.IsValid)
                    return BadRequest(new ApiResponse<object>(null)
                    {
                        Messages = new Message[] { new() {
                            Type = "ValidationError",
                            Description = "Error de validación"
                        }}
                    });

                var user = _mapper.Map<User>(dto);
                await _userService.InsertUserAsync(user);

                var userDto = _mapper.Map<UserDTO>(user);
                var response = new ApiResponse<UserDTO>(userDto)
                {
                    Messages = new Message[] { new() {
                        Type = "Success",
                        Description = "Usuario creado correctamente"
                    }}
                };

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
                            Description = $"Error al crear usuario: {err.Message}"
                        }}
                    });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserDTO dto)
        {
            try
            {
                dto.Id = id;
                var validation = await _validationService.ValidateAsync(dto);
                if (!validation.IsValid)
                    return BadRequest(new { errores = validation.Errors });

                var user = await _userService.GetUserAsync(id);

                _mapper.Map(dto, user);

                await _userService.UpdateUserAsync(user);

                var userDto = _mapper.Map<UserDTO>(user);
                var response = new ApiResponse<UserDTO>(userDto);
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
                await _userService.DeleteUserAsync(id);
                return Ok(new { message = $"El usuario con ID {id} fue eliminado correctamente." });
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }
      
    }
}
