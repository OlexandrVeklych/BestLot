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
        void Delete(int id);
        void Delete(string email);
        void Add(T item);
        void Modify(int id, T newItem);
        void Modify(string id, T newItem);
        T Get(int id);
        T Get(string email);
        T Get(int id, params Expression<Func<T, object>>[] includeProperties);
        T Get(string email, params Expression<Func<T, object>>[] includeProperties);
        IQueryable<T> GetAll();
        IQueryable<T> GetAll(params Expression<Func<T, object>>[] includeProperties);
    }
}
