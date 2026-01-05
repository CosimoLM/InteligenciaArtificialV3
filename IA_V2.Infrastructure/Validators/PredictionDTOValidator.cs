using FluentValidation;
using IA_V2.Infrastructure.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Infrastructure.Validators
{
    public class PredictionDTOValidator : AbstractValidator<PredictionDTO>
    {
        public PredictionDTOValidator()
        {
          
            RuleFor(p => p.Result)
                .NotEmpty().WithMessage("El resultado de la predicción es obligatorio.");

            RuleFor(p => p.Probability)
                .InclusiveBetween(0, 1).WithMessage("La precisión debe estar entre 0 y 1.");

            RuleFor(p => p.TextId)
                .GreaterThan(0).WithMessage("Debe asociar la predicción a un texto válido.");
        }
    }
}
