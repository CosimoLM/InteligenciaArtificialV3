using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Core.Entities
{
    public class Prediction : BaseEntity
    {
        //public int Id { get; set; }
        public int? TextId { get; set; }
        public Text? Text { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public string? Result { get; set; }
        public double Confidence { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
