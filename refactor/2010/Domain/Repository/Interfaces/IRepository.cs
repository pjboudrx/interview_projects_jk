using System;
using System.Collections.Generic;

namespace Domain.Repository.Interfaces
{
    /// <summary>
    /// Generic repository interface to fit all domain objects
    /// </summary>
    /// <typeparam name="TEntity">The domain object to wrap</typeparam>
    public interface IRepository<TEntity> : IDisposable where TEntity : class
    {
        IList<TEntity> GetAll();
        bool Save(TEntity entity);
    }

}
