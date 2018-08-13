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
            DbSet.Remove(DbSet.Find(id));
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

        public T Get(object id)
        {
            return DbSet.Find(id);
        }

        public async Task<T> GetAsync(object id)
        {
            return await Task.FromResult(DbSet.Find(id));
        }

        public IQueryable<T> GetAll()
        {
            return DbSet;
        }

        public async Task<IQueryable<T>> GetAllAsync()
        {
            return await Task.FromResult(DbSet);
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
