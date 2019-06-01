using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreRepository.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DotNetCoreRepository.DAL
{
    public abstract class RepositoryBase<T> where T : class
    {
        private ApplicationDbContext _database;
        private readonly DbSet<T> _dbSet;

        public RepositoryBase(IDatabaseFactory databaseFactory)
        {
            DatabaseFactory = databaseFactory;
            _dbSet = Database.Set<T>();
        }

        protected IDatabaseFactory DatabaseFactory
        {
            get;
            private set;
        }

        protected ApplicationDbContext Database
        {
            get { return _database ?? (_database = DatabaseFactory.Get()); }
        }

        // Override if Includes are needed
        public virtual IQueryable<T> AsQueryable()
        {
            return _dbSet;
        }

        public virtual IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public virtual IEnumerable<T> Get(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = "")
        {
            // create IQueryable object
            IQueryable<T> query = _dbSet;

            // apply filter expression if there is one.
            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // apply eager loading expressions after parsing comma-delim list
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                // apply orderBy expression if there is one, and return generic List collection.
                return orderBy(query).ToList();
            }
            else
            {
                // return IQueryable object as as generic List collection
                return query.ToList();
            }
        }

        public virtual T GetByID(object id)
        {
            return _dbSet.Find(id);
        }

        public virtual IEnumerable<T> Find(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Where(predicate);
        }

        public virtual T GetSingle(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Single(predicate);
        }

        public virtual T GetFirst(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.First(predicate);
        }

        public virtual T GetSingleOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.SingleOrDefault(predicate);
        }

        public virtual T GetFirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.FirstOrDefault(predicate);
        }

        public virtual void Insert(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            _dbSet.Add(entity);
        }

        public virtual void Delete(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (_database.Entry(entity).State == EntityState.Detached)
            {
                Attach(entity);
            }
            _dbSet.Remove(entity);
        }

        // overload lets you pass in id of entity to be deleted
        public virtual void Delete(object id)
        {
            T entity = _dbSet.Find(id);
            Delete(entity);
        }

        public virtual void Attach(T entity)
        {
            try
            {
                _dbSet.Attach(entity);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        public virtual void Update(T entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (_database.Entry(entity).State == EntityState.Detached)
            {
                Attach(entity);
            }
            _database.Entry(entity).State = EntityState.Modified;
        }

        public int Count()
        {
            return _dbSet.Count();
        }

        public int Count(Expression<Func<T, bool>> predicate)
        {
            return _dbSet.Count(predicate);
        }
    }
}
