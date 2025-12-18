using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Core.QueryFilters
{
    public class PredictionQueryFilter : PaginationQueryFilter
    {
        /// <summary>
        /// Filtrar por usuario
        /// </summary>
        [SwaggerSchema("ID del usuario")]
        public int? UserId { get; set; }

        /// <summary>
        /// Filtrar por texto
        /// </summary>
        [SwaggerSchema("ID del texto")]
        public int? TextId { get; set; }

        /// <summary>
        /// Filtrar por categoría de resultado
        /// </summary>
        [SwaggerSchema("Categoría de predicción")]
        public string? Result { get; set; }

        /// <summary>
        /// Probabilidad mínima
        /// </summary>
        [SwaggerSchema("Probabilidad mínima (0-1)")]
        public double? MinProbability { get; set; }

        /// <summary>
        /// Filtrar por fecha desde
        /// </summary>
        [SwaggerSchema("Fecha desde para filtrar")]
        public DateTime? FromDate { get; set; }
    }
}
