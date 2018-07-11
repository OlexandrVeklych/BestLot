using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Repository
{
    public interface IRepository<T> : IDisposable
    {
        void Delete(int id);
        void Add(T item);
        void Modify(int id, T newItem);
        T Get(int id);
        T Get(int id, params Expression<Func<T, object>>[] includeProperties);
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> GetAll(Func<T, bool> predicate, params Expression<Func<T, object>>[] includeProperties);
    }
}
