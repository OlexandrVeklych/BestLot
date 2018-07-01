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

        public GenericRepository(DbContext Context)
        {
            this.Context = Context;
            DbSet = Context.Set<T>();
        }

        public void Clear()
        {
            DbSet.RemoveRange(DbSet);
        }
        public void Delete(int Id)
        {
            DbSet.Remove(DbSet.Find(Id));
        }
        public void Delete(T Item)
        {
            Context.Entry(Item).State = EntityState.Deleted;
        }
        public void Add(T Item)
        {
            DbSet.Add(Item);
        }
        public void Modify(int Id, T Item)
        {
            Context.Entry(DbSet.Find(Id)).CurrentValues.SetValues(Item);
        }
        public T Get(int Id)
        {
            return DbSet.Find(Id);
        }

        public T GetByPosition(int Position)
        {
            return DbSet.ToList()[Position];
        }

        public List<T> GetAll()
        {
            return DbSet.AsNoTracking().ToList();
        }

        public List<T> GetAll(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = DbSet;
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty)).ToList();
        }

        public List<T> GetAll(Func<T, bool> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            var query = DbSet.Where(predicate).AsQueryable();
            return includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty)).ToList();
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
