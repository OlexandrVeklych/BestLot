using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BestLot.DataAccessLayer.Repository
{
    public interface IRepository<T>
    {
        void Delete(object id);
        void Add(T item);
        void Modify(object id, T newItem);
        T Get(object id);
        Task<T> GetAsync(object id);
        IQueryable<T> GetAll();
        Task<IQueryable<T>> GetAllAsync();
    }
}
