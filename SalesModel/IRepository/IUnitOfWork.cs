using Microsoft.EntityFrameworkCore.Storage;
using SalesModel.Models;

namespace SalesModel.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<ApplicationUser> ApplicationUser { get; }
		IRepository<ApplicationRole> ApplicationRole { get; }
        IRepository<TblBranch> Branch { get; }
		IRepository<TblVat> Vat { get; }
        
		IRepository<TblProduct_Category> Product_Category { get; }
        IRepository<TblClassRoom> TblClassRoom { get; }
        IRepository<TblClass> TblClass { get; }
        IRepository<TblStudent> TblStudent { get; }
        IRepository<TblSchoolSettings> TblSchoolSettings { get; }
        IRepository<TblAttendance> TblAttendance { get; }
        IRepository<AuditLog> AuditLog { get; }



        ISP_Call SP_Call { get; }

        Task<int> Complete();

        Task<IDbContextTransaction> BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();

    }
}
