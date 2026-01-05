using IA_V2.Core.Entities;
using IA_V2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IA_V2.Core.Interfaces;

namespace IA_V2.Infrastructure.Repositories
{
    public class BaseRepository<T>
        : IBaseRepository<T> where T : BaseEntity
    {
        private readonly InteligenciaArtificialV2Context _context;
        protected readonly DbSet<T> _entities;
        public BaseRepository(InteligenciaArtificialV2Context context)
        {
            _context = context;
            _entities = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            try 
            {
                return await _entities.ToListAsync();
            }
            catch (Exception ex) 
            {
                throw new Exception($"Error al obtener entidades {typeof(T).Name}: {ex.Message}", ex);
            }    
        }

        public async Task<T> GetById(int id)
        {
            try
            {
                return await _entities.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener entidad {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        public async Task Add(T entity)
        {
            try
            {
                _entities.Add(entity);
                //await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar entidad {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        public async Task Update(T entity)
        {
            try 
            {
                _entities.Update(entity);
                //await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar entidad {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        public async Task Delete(int id)
        {
            try 
            {
                T entity = await GetById(id);
                _entities.Remove(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar entidad {typeof(T).Name}: {ex.Message}", ex);
            }
        }
    }
}
