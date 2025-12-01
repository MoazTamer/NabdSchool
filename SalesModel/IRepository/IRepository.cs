using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query;
using SalesModel.ViewModels;
using System.Linq.Expressions;


namespace SalesModel.IRepository
{
    public interface IRepository<T> where T : class
    {
		T Add(T entity);

		Task<int> UpdateAll(Expression<Func<T, bool>>? filter = null,
			 Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>>? updateExpression = null);

		T GetById(string? id);

        T GetById(int? id);

        T GetFirstOrDefault(
			 Expression<Func<T, bool>>? filter = null, string[] includeProperties = null
			 );

		IEnumerable<T> GetAll(
			Expression<Func<T, bool>>? filter = null, string[] includeProperties = null,
			Expression<Func<T, object>> orderBy = null, string orderByDirection = OrderBy.Ascending
			);

		void DeleteByEntity(T entity);

		Task<int> DeleteAll(Expression<Func<T, bool>>? filter = null);

		Task<List<TResult>> ExecuteStoredProcedureQueryAsync<TResult>(
					string storedProcedureName,
					params SqlParameter[] parameters) where TResult : class;

        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> AnyAsync(Expression<Func<T, bool>> filter);
        IQueryable<T> Table { get; }
    }
}
