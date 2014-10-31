using System;
using System.Collections.Generic;
using System.Linq;
using Domain.DomainClasses;
using Domain.Repository.Interfaces;

namespace Domain.Repository.Implementation
{
    public class OrderRepository : IRepository<Order>
    {
        private readonly IList<Order> _orders;

        public OrderRepository()
        {
            _orders = new List<Order>();
        }

        public void Dispose()
        {
        
        }

        public IList<Order> GetAll()
        {
            return _orders.Select(order => order).ToList();
        }

        public bool Save(Order entity)
        {
            try
            {
                lock (_orders)
                {
                    if (_orders.Any())
                    {
                        var id = _orders.Select(order => order.id).Max();
                        entity.id = ++id;
                    }
                    _orders.Add(entity);
                } 

            }
            catch (Exception)
            {
                //TODO: Add log4net logging
                return false;
            }
            return true;
        }

    }
}