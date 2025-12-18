using IA_V2.Core.Entities;
using IA_V2.Core.Interfaces;
using IA_V2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Infrastructure.Repositories
{
    public class SecurityRepository : BaseRepository<Security>, ISecurityRepository
    {
        private readonly IDapperContext _dapper;
        public SecurityRepository(InteligenciaArtificialV2Context context, IDapperContext dapper) : base(context) 
        {
            _dapper = dapper;
        }

        public async Task<Security> GetLoginByCredentials(UserLogin login)
        {
            try
            {
                var sql = @"SELECT * FROM Securities WHERE Login = @Login";

                var security = await _dapper.QueryFirstOrDefaultAsync<Security>(
                    sql,
                    new { Login = login.User });

                if (security == null)
                {
                    throw new Exception($"Usuario '{login.User}' no encontrado");
                }

                return security;
            }
            catch (Exception err)
            {
                throw new Exception($"Error al obtener credenciales con Dapper: {err.Message}", err);
            }
        }
    }
}
