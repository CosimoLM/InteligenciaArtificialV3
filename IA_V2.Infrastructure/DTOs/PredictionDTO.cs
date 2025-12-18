using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Infrastructure.DTOs
{
    public class PredictionDTO
    {
        public int Id { get; set; }
        public string? Result { get; set; }
        public double Confidence { get; set; }
        public DateTime Date { get; set; }
        public int TextId { get; set; }
    }
}
