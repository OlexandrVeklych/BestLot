using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.DataAccessLayer.Repository
{
    public interface IRepository<T> : IDisposable
    {
        void Delete(object id);
        void Add(T item);
        void Modify(object id, T newItem);
        T Get(object id, params Expression<Func<T, object>>[] includeProperties);
        Task<T> GetAsync(object id, params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> GetAll(params Expression<Func<T, object>>[] includeProperties);
        Task<IQueryable<T>> GetAllAsync(params Expression<Func<T, object>>[] includeProperties);
    }
}
