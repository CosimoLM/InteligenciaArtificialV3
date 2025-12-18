using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;

namespace IA_V2.Core.QueryFilters
{
    public class TextQueryFilter : PaginationQueryFilter
    {
        /// <summary>
        /// Filtrar por usuario
        /// </summary>
        [SwaggerSchema("ID del usuario")]
        public int? UserId { get; set; }

        /// <summary>
        /// Buscar en contenido
        /// </summary>
        [SwaggerSchema("Texto a buscar en el contenido")]
        public string? SearchText { get; set; }

        /// <summary>
        /// Filtrar por fecha desde
        /// </summary>
        [SwaggerSchema("Fecha desde para filtrar")]
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Filtrar por fecha hasta
        /// </summary>
        [SwaggerSchema("Fecha hasta para filtrar")]
        public DateTime? ToDate { get; set; }
    }
}
