using AutoMapper;
using IA_V2.Api.Responses;
using IA_V2.Core.Entities;
using IA_V2.Core.Enum;
using IA_V2.Core.Interfaces;
using IA_V2.Infrastructure.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IA_V2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly ISecurityServices _securityServices;
        private readonly IMapper _mapper;
        private readonly IPasswordService _passwordService;
        public SecurityController(ISecurityServices securityServices,
            IMapper mapper,
            IPasswordService passwordService)
        {
            _securityServices = securityServices;
            _mapper = mapper;
            _passwordService = passwordService;
        }

        [Authorize(Roles = $"{nameof(RoleType.Administrator)},{nameof(RoleType.User)}")]
        [HttpPost]
        public async Task<IActionResult> Post(SecurityDTO securityDto)
        {
            var security = _mapper.Map<Security>(securityDto);
            security.Password = _passwordService.Hash(security.Password);
            await _securityServices.RegisterUser(security);

            securityDto = _mapper.Map<SecurityDTO>(security);
            var response = new ApiResponse<SecurityDTO>(securityDto);
            return Ok(response);
        }


    }
}
