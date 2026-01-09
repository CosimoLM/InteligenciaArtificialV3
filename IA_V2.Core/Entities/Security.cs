using IA_V2.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Core.Entities
{
    public partial class Security : BaseEntity
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public string Name { get; set; }

        public RoleType? Role { get; set; }

        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
