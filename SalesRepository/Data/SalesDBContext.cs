using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SalesModel.Models;

namespace SalesRepository.Data
{
	public class SalesDBContext : IdentityDbContext
	{
		public SalesDBContext(DbContextOptions<SalesDBContext> options)
			: base(options)
		{
		}

		public DbSet<ApplicationUser> ApplicationUser { get; set; }
		public DbSet<ApplicationRole> ApplicationRole { get; set; }
        public DbSet<TblBranch> TblBranch { get; set; }
		public DbSet<TblVat> TblVat { get; set; }
        
		public DbSet<TblProduct_Category> TblProduct_Category { get; set; }
		public DbSet<TblStudent> TblStudent { get; set; }
        public DbSet<TblClass> TblClass { get; set; }
		public DbSet<TblClassRoom> TblClassRoom { get; set; }
		public DbSet<TblSchoolSettings> TblSchoolSettings { get; set; }

        public DbSet<AuditLog> AuditLog { get; set; }
    }
}
