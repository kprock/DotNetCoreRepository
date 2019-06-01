using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DotNetCoreRepository.DAL
{
    /// <summary>
    /// Generic Read-Only Repository Contract
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyRepository<T> where T : class
    {
        /// <summary>
        /// Gets an IQueryable sequence of entities of type T.
        /// Deferred execution. Allows you to operate and build queries that can be sent to a remote provider. EF and LINQ-to-SQL operates with IQueryable<T>
        /// because they will translate this to SQL and send it to the connected database for execution if possible
        /// Extension methods defined for IQueryable<T> take Expression objects instead of Func objects,
        /// meaning the delegate it receives is an expression tree instead of a method to invoke. 
        /// The Fluent API is based on IQueryables.
        /// </summary>
        /// <returns></returns>
        IQueryable<T> AsQueryable();

        /// <summary>
        /// Gets an IEnumerable sequence of all entites of type T.
        /// Deferred exectution. Uses LINQ-to-object: Allows you to execute queries and return collections in memory.
        /// Extension methods for IEnumerable<T> take Func objects, meaning the delegate it receives is a method to invoke.
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Get with optional filter, order by, and list of nav properties for eager loading.
        /// </summary>
        /// <param name="filter">Optional Lambda expression to allow filter condition.</param>
        /// <param name="orderBy">Optional Lambda expr. to allow order-by column.</param>
        /// <param name="includeProperties">Optional comma-delimited list of navigation properties for eager loading.</param>
        /// <returns>Collection optionally filtered, sorted and including eager-loaded nav properties</returns>
        IEnumerable<T> Get(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = "");

        /// <summary>
        /// Returns entity T with matching Id
        /// </summary>
        /// <param name="id">id of T</param>
        /// <returns>Entity of type T</returns>
        T GetByID(object id);

        /// <summary>
        /// Get an IEnumerable sequence of entities of type T filtered on the @where predicate.
        /// </summary>
        /// <param name="where">The where predicate.</param>
        /// <returns></returns>
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets a single entity in a sequence of entities of type T using the filtered @where predicate.
        /// </summary>
        /// <param name="where">The where predicate.</param>
        /// <returns></returns>
        T GetSingle(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets the first entity in a sequence of entities of type T using the filtered @where predicate.
        /// </summary>
        /// <param name="where">The where predicate.</param>
        /// <returns></returns>
        T GetFirst(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets a single entity (or default of entity of type T) in a sequence of entities of type T using the filtered @where predicate.
        /// </summary>
        /// <param name="where">The where predicate.</param>
        /// <returns></returns>
        T GetSingleOrDefault(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Gets a first entity (or default entity of type T) in a sequence of entities of type T using the filtered @where predicate.
        /// </summary>
        /// <param name="where">The where predicate.</param>
        /// <returns></returns>
        T GetFirstOrDefault(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Counts the specified entities.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns></returns>
        int Count();

        /// <summary>
        /// Counts entities with the specified criteria.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="criteria">The criteria.</param>
        /// <returns></returns>
        int Count(Expression<Func<T, bool>> predicate);
    }
}
