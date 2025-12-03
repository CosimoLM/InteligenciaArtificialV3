using IA_V2.Core.Entities;
using IA_V2.Core.Interfaces;
using IA_V2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IA_V2.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly InteligenciaArtificialV2Context _context;
        private IDbContextTransaction _transaction;
        private readonly IDapperContext _dapper;
        public readonly ISecurityRepository _securityRepository;

        public UnitOfWork(InteligenciaArtificialV2Context context, IDapperContext dapper)
        {
            _context = context;
            _dapper = dapper;
        }

        public IUserRepository UserRepository => new UserRepository(_context,_dapper);
        public ITextRepository TextRepository => new TextRepository(_context,_dapper);
        public IBaseRepository<Prediction> PredictionRepository => new BaseRepository<Prediction>(_context);
        public ISecurityRepository SecurityRepository =>_securityRepository ?? new SecurityRepository(_context, _dapper);

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public IDbConnection? GetDbConnection()
        {
            return _context.Database.GetDbConnection();
        }

        public IDbTransaction? GetDbTransaction()
        {
            return _transaction?.GetDbTransaction();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }

}
