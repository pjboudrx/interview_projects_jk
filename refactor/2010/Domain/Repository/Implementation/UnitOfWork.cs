using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using Castle.Core.Internal;
using Domain.DomainClasses;
using Domain.Repository.Interfaces;

namespace Domain.Repository.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {     
        public UnitOfWork(
            IRepository<Order> orderRepository, 
            IRepository<Item> itemRepository, 
            IRepository<OrderItem> orderItemRepository  )
        {
            _repositories = new Dictionary<Type, object>();
            _repositories[typeof (Order)] = orderRepository;
            _repositories[typeof(Item)] = itemRepository;
            _repositories[typeof(OrderItem)] = orderItemRepository;
        }

        private Dictionary<Type, object> _repositories; 


        public void Dispose()
        {
            _repositories.Values.ForEach( repo => ((IDisposable)repo).Dispose() );
        }

        public bool Commit()
        {
            /*Not necessary for in-memory list implementation but absolutely 
            necessary if the abstraction is applied to a backing database.*/
            return true;
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories[typeof(TEntity)] != null)
            {
                return (IRepository<TEntity>)_repositories[typeof(TEntity)];
            }
            throw new InstanceNotFoundException("The requested repository type is not available.");
        }
    }
}
