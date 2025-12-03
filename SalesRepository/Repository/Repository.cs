using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SalesModel.IRepository;
using SalesModel.ViewModels;
using SalesRepository.Data;

namespace SalesRepository.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly SalesDBContext _db;
        internal DbSet<T> dbSet;

		public Repository(SalesDBContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

		public T Add(T entity)
		{
			dbSet.Add(entity);
			return entity;
		}

		public async Task<int> UpdateAll(Expression<Func<T, bool>>? filter = null,
			  Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>? updateExpression = null)
		{
			IQueryable<T> query = dbSet;
			query = query.Where(filter);
			return updateExpression != null ? await query.ExecuteUpdateAsync(updateExpression) : 0;
		}

		public T GetById(string? id)
		{
			return dbSet.Find(id);
		}

        public T GetById(int? id)
        {
            return dbSet.Find(id);
        }

        public T GetFirstOrDefault(Expression<Func<T, bool>>? filter = null, string[] includeProperties = null)
		{
			IQueryable<T> query = dbSet;
			if (filter != null)
			{
				query = query.Where(filter);
			}

			if (includeProperties != null)
			{
				foreach (var include in includeProperties)
				{
					query = query.Include(include);
				}
			}

			return query.FirstOrDefault();
		}

		public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string[] includeProperties = null, Expression<Func<T, object>> orderBy = null, string orderByDirection = OrderBy.Ascending)
		{
			IQueryable<T> query = dbSet;
			if (filter != null)
			{
				query = query.Where(filter);
			}

			if (includeProperties != null)
			{
				foreach (var include in includeProperties)
				{
					query = query.Include(include);
				}
			}

			if (orderBy != null)
			{
				if (orderByDirection == OrderBy.Ascending)
					query = query.OrderBy(orderBy);
				else
					query = query.OrderByDescending(orderBy);
			}
			return query.ToList();
		}

		public void DeleteByEntity(T entity)
		{
			dbSet.Remove(entity);
		}

		public async Task<int> DeleteAll(Expression<Func<T, bool>>? filter = null)
		{
			IQueryable<T> query = dbSet;
			query = query.Where(filter);
			return await query.ExecuteDeleteAsync();
		}

		public async Task<List<TResult>> ExecuteStoredProcedureQueryAsync<TResult>(string storedProcedureName, params SqlParameter[] parameters) where TResult : class
		{
			CancellationToken cancellationToken = default;

			if (string.IsNullOrWhiteSpace(storedProcedureName))
				throw new ArgumentException("Stored procedure name is required.", nameof(storedProcedureName));

			string sql;
			object[] dbParams;

			if (parameters == null || parameters.Length == 0)
			{
				sql = $"EXEC {storedProcedureName}";
				dbParams = Array.Empty<object>();
			}
			else
			{
				// اتأكد إن أسماء الباراميتر فيها @
				var names = string.Join(", ",
					parameters.Select(p =>
						p.ParameterName.StartsWith("@") ? p.ParameterName : "@" + p.ParameterName));

				sql = $"EXEC {storedProcedureName} {names}";
				dbParams = parameters.Cast<object>().ToArray();
			}

			// SqlQueryRaw بيسمح تستخدم DTO عادي من غير ما تضيفه في الـ Model
			var res = await _db.Set<TResult>()
						   .FromSqlRaw(sql, dbParams)
						   .ToListAsync(cancellationToken);
			return  res; 
		}



        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            if (predicate == null)
                return await dbSet.CountAsync();

            return await dbSet.CountAsync(predicate);
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
        {
            return dbSet.AnyAsync(filter);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                return;

            await dbSet.AddRangeAsync(entities);
        }


        public IQueryable<T> Table => dbSet;

    }
}
