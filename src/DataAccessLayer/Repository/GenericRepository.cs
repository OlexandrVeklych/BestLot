using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;

namespace DataAccessLayer.Repository
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        private DbContext Context;
        private DbSet<T> DbSet;

        public GenericRepository(DbContext context)
        {
            Context = context;
            DbSet = Context.Set<T>();
        }

        public void Delete(int id)
        {
            DbSet.Remove(DbSet.Find(id));
        }

        public void Add(T item)
        {
            DbSet.Add(item);
        }

        public void Modify(int id, T newItem)
        {
            Context.Entry(DbSet.Find(id)).CurrentValues.SetValues(newItem);
        }

        public T Get(int id)
        {
            return DbSet.Find(id);
        }

        public T Get(int id, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = new List<T> {DbSet.Find(id)}.AsQueryable();
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty)).First();
        }

        public IEnumerable<T> GetAll()
        {
            return DbSet;
        }

        public IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbSet;
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public IQueryable<T> GetAll(Func<T, bool> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbSet.Where(predicate).AsQueryable();
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                    DbSet = null;
                }
            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
