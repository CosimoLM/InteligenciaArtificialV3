using IA_V2.Core.Entities;
using IA_V2.Core.Services;
using IA_V3_Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V3.Core.Interfaces
{
    public interface IModeloIAService
    {
        Task<Prediction> PredictTextByIdAsync(int textId);
    }
}
