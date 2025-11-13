using Microsoft.EntityFrameworkCore.Storage;
using SalesModel.IRepository;
using SalesModel.Models;
using SalesRepository.Data;

namespace SalesRepository.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SalesDBContext _db;
        private IDbContextTransaction _currentTransaction;

        public IRepository<ApplicationUser> ApplicationUser { get; private set; }
        public IRepository<ApplicationRole> ApplicationRole { get; private set; }
        public IRepository<TblBranch> Branch { get; private set; }
        public IRepository<TblVat> Vat { get; private set; }
        
        public IRepository<TblProduct_Category> Product_Category { get; private set; }
        public IRepository<TblStudent> TblStudent { get; private set; }
        public IRepository<TblClassRoom> TblClassRoom { get; private set; }
        public IRepository<TblClass> TblClass { get; private set; }
        public IRepository<TblSchoolSettings> TblSchoolSettings { get; private set; }


        public IRepository<AuditLog> AuditLog { get; private set; }





        public ISP_Call SP_Call { get; private set; }

        public UnitOfWork(SalesDBContext db)
        {
            _db = db;

            ApplicationUser = new Repository<ApplicationUser>(_db);
            ApplicationRole = new Repository<ApplicationRole>(_db);
            Branch = new Repository<TblBranch>(_db);
            Vat = new Repository<TblVat>(_db);
            
            Product_Category = new Repository<TblProduct_Category>(_db);
            TblStudent = new Repository<TblStudent>(_db);
            TblClassRoom = new Repository<TblClassRoom>(_db);
            TblClass = new Repository<TblClass>(_db);
            TblSchoolSettings = new Repository<TblSchoolSettings>(_db);

            AuditLog = new Repository<AuditLog>(_db);





            SP_Call = new SP_Call(_db);
        }

        public async Task<int> Complete()
        {
            return await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _currentTransaction = await _db.Database.BeginTransactionAsync();
            return _currentTransaction;
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
            }
        }


    }
}
