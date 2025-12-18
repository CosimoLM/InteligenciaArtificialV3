using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Core.QueryFilters
{
    public class UserQueryFilter : PaginationQueryFilter
    {
        /// <summary>
        /// Buscar por nombre
        /// </summary>
        [SwaggerSchema("Texto a buscar en el nombre")]
        public string ?SearchName { get; set; }

        /// <summary>
        /// Buscar por email
        /// </summary>
        [SwaggerSchema("Texto a buscar en el email")]
        public string ?SearchEmail  { get; set; }

        /// <summary>
        /// Filtrar usuarios con textos
        /// </summary>
        [SwaggerSchema("Filtrar usuarios que tienen textos")]
        public bool? HasTexts { get; set; }
    }
}
