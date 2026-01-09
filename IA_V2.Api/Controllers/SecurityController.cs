using AutoMapper;
using IA_V2.Api.Responses;
using IA_V2.Core.CustomEntities;
using IA_V2.Core.Entities;
using IA_V2.Core.Enum;
using IA_V2.Core.Exceptions;
using IA_V2.Core.Interfaces;
using IA_V2.Infrastructure.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using System.Data;
using System.Net;

namespace IA_V2.Api.Controllers
{
    [Authorize(Roles = $"{nameof(RoleType.Administrator)}")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityServices _securityServices;
        private readonly IMapper _mapper;
        private readonly IPasswordService _passwordService;
        private readonly IUserService _userService;
        public SecurityController(ISecurityServices securityServices,
            IMapper mapper,
            IPasswordService passwordService,
            IUserService userService)
        {
            _securityServices = securityServices;
            _mapper = mapper;
            _passwordService = passwordService;
            _userService = userService;
        }

        //[Authorize(Roles = $"{nameof(RoleType.Administrator)},{nameof(RoleType.User)}")]
        [HttpPost]
        public async Task<IActionResult> Post(SecurityDTO securityDto)
        {
            try
            {
                var security = _mapper.Map<Security>(securityDto);
                security.Password = _passwordService.Hash(security.Password);
                await _securityServices.RegisterUser(security);

                securityDto = _mapper.Map<SecurityDTO>(security);

                var response = new ApiResponse<SecurityDTO>(securityDto)
                {
                    Messages = new Message[] { new() {
                    Type = "Success",
                    Description = "Credenciales creadas exitosamente"
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
                        Description = $"Error al crear credenciales: {err.Message}"
                    }}
                    });
            }
        }
        //Authorize(RolesType =$" {nameof(RoleType.Administrator)}")]
        //[HttpPost("admin/create")]
        //public async Task<IActionResult> CreateCredentials(SecurityDTO securityDto)
        //{
        //    // Misma lógica pero solo para administradores
        //    return await Register(securityDto);
        //}
    }
}
