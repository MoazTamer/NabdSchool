using Microsoft.EntityFrameworkCore.Storage;
using NabdSchool.Core.Entities;
using NabdSchool.Core.Interfaces;
using NabdSchool.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NabdSchool.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;

        public IGenericRepository<Student> Students { get; }
        public IGenericRepository<AuditLog> AuditLogs { get; }

        public IGenericRepository<Grade> Grades { get; }

        public IGenericRepository<Class> Classes { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Students = new GenericRepository<Student>(_context);
            AuditLogs = new GenericRepository<AuditLog>(_context);
            Grades = new GenericRepository<Grade>(_context);
            Classes = new GenericRepository<Class>(_context);

        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}
