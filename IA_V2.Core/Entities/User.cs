using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace IA_V2.Core.Entities
{
    public class User : BaseEntity
    {
        //public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public virtual ICollection<Text> Texts { get; set; } = new List<Text>();
        public virtual ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
    }
}
