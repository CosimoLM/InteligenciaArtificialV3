using Dapper;
using IA_V2.Core.Entities;
using IA_V2.Core.Enum;
using IA_V2.Core.Interfaces;
using IA_V2.Core.QueryFilters;
using IA_V2.Infrastructure.Data;
using IA_V2.Infrastructure.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Infrastructure.Repositories
{
    public class TextRepository : BaseRepository<Text>, ITextRepository
    {
        private readonly IDapperContext _dapper;

        public TextRepository(InteligenciaArtificialV2Context context, IDapperContext dapper)
            : base(context)
        {
            _dapper = dapper;
        }

        public async Task<IEnumerable<Text>> GetAllTextsDapperAsync(int limit = 10)
        {
            try
            {
                var sql = _dapper.Provider switch
                {
                    DatabaseProvider.SqlServer => TextQueries.TextQuerySqlServer,
                    _ => throw new NotSupportedException("Provider no soportado")
                };

                return await _dapper.QueryAsync<Text>(sql, new { Limit = limit });
            }
            catch (Exception err)
            {
                throw new Exception($"Error al obtener textos con Dapper: {err.Message}", err);
            }
        }

        public async Task<Text> GetTextByIdDapperAsync(int id)
        {
            try
            {
                var sql = @"SELECT t.*, u.Name as UserName 
                       FROM Texts t 
                       LEFT JOIN Users u ON t.UserId = u.Id 
                       WHERE t.Id = @Id";
                return await _dapper.QueryFirstOrDefaultAsync<Text>(sql, new { Id = id });
            }
            catch (Exception err)
            {
                throw new Exception($"Error al obtener texto por ID con Dapper: {err.Message}", err);
            }
        }
        public async Task<IEnumerable<Text>> GetTextsByUserDapperAsync(int userId)
        {
            var sql = "SELECT * FROM Texts WHERE UserId = @UserId ORDER BY FechaEnvio DESC";
            return await _dapper.QueryAsync<Text>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<Text>> GetRecentTextsWithPredictionsAsync()
        {
            var sql = @"
            SELECT t.*, p.Result as PredictionResult, p.Probability 
            FROM Texts t
            LEFT JOIN Predictions p ON t.Id = p.TextId
            WHERE t.FechaEnvio >= DATEADD(day, -7, GETDATE())
            ORDER BY t.FechaEnvio DESC";

            return await _dapper.QueryAsync<Text>(sql);
        }

       

    }
}
