using IA_V2.Core.Entities;
using IA_V2.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace IA_V2.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ISecurityServices _securityServices;
        private readonly IPasswordService _passwordService;

        public TokenController(IConfiguration configuration,
            ISecurityServices securityServices,
            IPasswordService passwordService)
        {
            _configuration = configuration;
            _securityServices = securityServices;
            _passwordService = passwordService;
        }

        [HttpPost]
        public async Task<IActionResult> Authentication([FromBody] UserLogin userLogin)
        {
            try
            {
                //Si es usuario valido
                var validation = await IsValidUser(userLogin);
                if (validation.Item1)
                {
                    var token = GenerateToken(validation.Item2);
                    return Ok(new { token });
                }

                return NotFound("Credenciales no válidas");
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }

        private async Task<(bool, Security)> IsValidUser(UserLogin userLogin)
        {
            var user = await _securityServices.GetLoginByCredentials(userLogin);
            var isValid = _passwordService.Check(user.Password, userLogin.Password);
            return (isValid, user);
        }

        private string GenerateToken(Security security)
        {
            string secretKey = _configuration["Authentication:SecretKey"];

            //Header
            var symmetricSecurityKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials =
                new SigningCredentials(symmetricSecurityKey,
                    SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(signingCredentials);

            //Body Payload (Claims)
            var claims = new[]
            {
                new Claim("Name", security.Name),
                new Claim("Login", security.Login),
                new Claim(ClaimTypes.Role, security.Role.ToString())
            };
            var payload = new JwtPayload(
                issuer: _configuration["Authentication:Issuer"],
                audience: _configuration["Authentication:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(60)
                );

            //Firma
            var token = new JwtSecurityToken(header, payload);

            //Serializar el token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("Config")]
        public async Task<IActionResult> GetConfig()
        {
            try
            {
                var connectionStringSqlServer = _configuration["ConnectionStrings:ConnectionSqlServer"];

                var result = new
                {
                    connectionStringSqlServer = connectionStringSqlServer ?? "SQL SERVER NO CONFIGURADO",
                    AllConnectionStrings = _configuration.GetSection("ConnectionStrings").GetChildren().Select(x => new { Key = x.Key, Value = x.Value }),
                    Environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "ASPNETCORE_ENVIRONMENT NO CONFIGURADO",
                    Authentication = _configuration.GetSection("Authentication").GetChildren().Select(x => new { Key = x.Key, Value = x.Value })
                };

                return Ok(result);
            }
            catch (Exception err)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, err.Message);
            }
        }

        [HttpGet("TestConeccion")]
        public async Task<IActionResult> TestConeccion()
        {
            var result = new
            {
                ConnectionSqlServer = _configuration["ConnectionStrings:ConnectionSqlServer"]
            };

            return Ok(result);
        }



    }
}
