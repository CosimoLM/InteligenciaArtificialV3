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
    public class PredictionRepository : BaseRepository<Prediction>, IPredictionRepository
    {
        private readonly IDapperContext _dapper;

        public PredictionRepository(InteligenciaArtificialV2Context context, IDapperContext dapper)
            : base(context)
        {
            _dapper = dapper;
        }
        public async Task<IEnumerable<Prediction>> GetAllPredictionsDapperAsync(int limit = 10)
        {
            try
            {
                var sql = _dapper.Provider switch
                {
                    DatabaseProvider.SqlServer => PredictionQueries.PredictionQuerySqlServer,
                    _ => throw new NotSupportedException("Provider no soportado")
                };

                return await _dapper.QueryAsync<Prediction>(sql, new { Limit = limit });
            }
            catch (Exception err)
            {
                throw new Exception($"Error al obtener predicciones con Dapper: {err.Message}", err);
            }
        }

        public async Task<IEnumerable<Prediction>> GetFilteredPredictionsDapperAsync(PredictionQueryFilter filters)
        {
            try
            {
                var sqlBuilder = new StringBuilder(@"
            SELECT p.*, t.Content as TextContent, u.Name as UserName 
            FROM Predictions p
            LEFT JOIN Texts t ON p.TextId = t.Id
            LEFT JOIN Users u ON p.UserId = u.Id
            WHERE 1=1");

                var parameters = new DynamicParameters();

                if (filters.UserId.HasValue)
                {
                    sqlBuilder.Append(" AND p.UserId = @UserId");
                    parameters.Add("UserId", filters.UserId.Value);
                }

                if (filters.TextId.HasValue)
                {
                    sqlBuilder.Append(" AND p.TextId = @TextId");
                    parameters.Add("TextId", filters.TextId.Value);
                }

                if (!string.IsNullOrEmpty(filters.Result))
                {
                    sqlBuilder.Append(" AND p.Result LIKE @Result");
                    parameters.Add("Result", $"%{filters.Result}%");
                }

                if (filters.MinProbability.HasValue)
                {
                    sqlBuilder.Append(" AND p.Confidence >= @MinProbability");
                    parameters.Add("MinProbability", filters.MinProbability.Value);
                }

                if (filters.FromDate.HasValue)
                {
                    sqlBuilder.Append(" AND p.Date >= @FromDate");
                    parameters.Add("FromDate", filters.FromDate.Value);
                }

                sqlBuilder.Append(" ORDER BY p.Date DESC");

                // Paginación
                sqlBuilder.Append(@" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

                parameters.Add("Offset", (filters.PageNumber - 1) * filters.PageSize);
                parameters.Add("PageSize", filters.PageSize);

                return await _dapper.QueryAsync<Prediction>(sqlBuilder.ToString(), parameters);
            }
            catch (Exception err)
            {
                throw new Exception($"Error al obtener predicciones filtradas con Dapper: {err.Message}", err);
            }
        }
    }
}
