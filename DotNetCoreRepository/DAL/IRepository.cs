using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreRepository.DAL
{
    /// <summary>
    /// Generic Repository Contract.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRepository<T> : IReadOnlyRepository<T> where T : class
    {
        /// <summary>
        /// Adds specified entity to the respository of type T.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        void Insert(T entity);

        /// <summary>
        /// Deletes specified entity from the respository of type T.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void Delete(T entity);

        /// <summary>
        /// Deletes entity with matching id from the repository
        /// </summary>
        /// <param name="id">The id of entity to delete</param>
        void Delete(object id);

        /// <summary>
        /// Attaches specified entity to the respository of type T.
        /// </summary>
        /// <param name="entity">The entity to attach.</param>
        void Attach(T entity);

        /// <summary>
        /// Updates entity of type T
        /// </summary>
        /// <param name="entity"></param>
        void Update(T entity);
    }
}
