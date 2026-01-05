using IA_V2.Core.CustomEntities;
using IA_V2.Core.Entities;
using IA_V2.Core.Exceptions;
using IA_V2.Core.Interfaces;
using IA_V2.Core.QueryFilters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Core.Services
{
    public class TextService : ITextService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDapperContext _dapper;

        public TextService(IUnitOfWork unitOfWork, IDapperContext dapper)
        {
            _unitOfWork = unitOfWork;
            _dapper = dapper;
        }

        public async Task<IEnumerable<Text>> GetAllTextAsync()
        {
            return await _unitOfWork.TextRepository.GetAll();
        }

        public async Task<ResponseData> GetAllTextAsync(TextQueryFilter filters)
        {
            var texts = await _unitOfWork.TextRepository.GetAll();

            // Aplicar filtros
            if (filters.UserId.HasValue)
                texts = texts.Where(t => t.UserId == filters.UserId.Value);

            if (!string.IsNullOrEmpty(filters.SearchText))
                texts = texts.Where(t => t.Content.ToLower().Contains(filters.SearchText.ToLower()));

            if (filters.FromDate.HasValue)
                texts = texts.Where(t => t.FechaEnvio >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                texts = texts.Where(t => t.FechaEnvio <= filters.ToDate.Value);

            var pagedTexts = PagedList<object>.Create(texts, filters.PageNumber, filters.PageSize);

            if (pagedTexts.Any())
            {
                return new ResponseData()
                {
                    Messages = new Message[] { new() {
                    Type = "Information",
                    Description = "Textos recuperados correctamente"
                }},
                    Pagination = pagedTexts,
                    StatusCode = HttpStatusCode.OK
                };
            }
            else
            {
                return new ResponseData()
                {
                    Messages = new Message[] { new() {
                    Type = "Warning",
                    Description = "No se encontraron textos"
                }},
                    Pagination = pagedTexts,
                    StatusCode = HttpStatusCode.NotFound
                };
            }
        }

        public async Task<IEnumerable<Text>> GetAllTextDapperAsync(int limit = 10)
        {
            var texts = await _unitOfWork.TextRepository.GetAllTextsDapperAsync(limit);
            return texts;
        }

        public async Task<Text> GetTextByIdAsync(int id)
        {
            var text = await _unitOfWork.TextRepository.GetById(id);
            if (text == null)
                throw new BusinessException($"Texto con ID {id} no encontrado");
            return text;
        }

        public async Task InsertTextAsync(Text text)
        {
            try
            {
                // Validar que el usuario existe
                var user = await _unitOfWork.UserRepository.GetById(text.UserId.Value);
                if (user == null)
                    throw new BusinessException("El usuario no existe");

                // Validar contenido (mínimo de caracteres)
                if (string.IsNullOrWhiteSpace(text.Content) || text.Content.Length < 5)
                    throw new BusinessException("El texto debe tener al menos 5 caracteres");

                await _unitOfWork.TextRepository.Add(text);
                await _unitOfWork.SaveChangesAsync(); 
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al insertar texto: {ex.Message}", ex);
            }
        }

        public async Task UpdateTextAsync(Text text)
        {
            var existingText = await GetTextByIdAsync(text.Id);

            // Lógica de actualización
            existingText.Content = text.Content;

            await _unitOfWork.TextRepository.Update(existingText);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteTextAsync(int id)
        {
            var text = await GetTextByIdAsync(id);

            var predictions = await _unitOfWork.PredictionRepository.GetAll();
            var textPredictions = predictions.Where(p => p.TextId == id);
            foreach (var prediction in textPredictions)
            {
                await _unitOfWork.PredictionRepository.Delete(prediction.Id);
            }

            await _unitOfWork.TextRepository.Delete(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
