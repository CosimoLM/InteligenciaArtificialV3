using AutoMapper;
using IA_V2.Api.Responses;
using IA_V2.Core.Entities;
using IA_V2.Core.Interfaces;
using IA_V2.Infrastructure.DTOs;
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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {

                var users = await _userService.GetAllUserAsync();
                var result = _mapper.Map<IEnumerable<UserDTO>>(users);
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
                var user = await _userService.GetUserAsync(id);
                var userDto = _mapper.Map<UserDTO>(user);
                var response = new ApiResponse<UserDTO>(userDto);
                return Ok(response);
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
           
        }
        [HttpPost]
        public async Task<IActionResult> Insert(UserDTO dto)
        {
            try
            {
                var validation = await _validationService.ValidateAsync(dto);
                if (!validation.IsValid)
                    return BadRequest(new { errores = validation.Errors });
                var user = _mapper.Map<User>(dto);
                user.PasswordHash = _passwordService.Hash(dto.Password);
                await _userService.InsertUserAsync(user);

                var response = _mapper.Map<UserDTO>(user);
                return Ok(response);
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
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
