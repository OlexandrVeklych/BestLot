using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;

namespace BestLot.DataAccessLayer.Repository
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

        public void Delete(object id)
        {
            Context.Entry(DbSet.Find(id)).State = EntityState.Deleted;
        }

        public void Add(T item)
        {
            DbSet.Add(item);
        }

        public void Modify(object id, T newItem)
        {
            Context.Entry(DbSet.Find(id)).CurrentValues.SetValues(newItem);
            Context.Entry(DbSet.Find(id)).State = EntityState.Modified;
        }

        public T Get(object id, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = new List<T> {DbSet.Find(id)}.AsQueryable();
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty)).First();
        }

        public async Task<T> GetAsync(object id, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = new List<T> { DbSet.Find(id) }.AsQueryable();
            return await Task.FromResult(includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty)).First());
        }

        public IQueryable<T> GetAll(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbSet;
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
        }

        public async Task<IQueryable<T>> GetAllAsync(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbSet;
            return await Task.FromResult(includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty)));
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
