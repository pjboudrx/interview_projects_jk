using System;
using Castle.MicroKernel.ModelBuilder.Descriptors;

namespace Domain.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        bool Commit();
        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    }
}
