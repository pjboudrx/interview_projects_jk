using System;
using System.Collections.Generic;
using Domain.DomainClasses;
using Domain.Repository.Interfaces;

namespace Domain.Repository.Implementation
{
    public class OrderItemRepository : IRepository<OrderItem>
    {
        private readonly IList<OrderItem> _orderItems;

        public OrderItemRepository()
        {
            _orderItems = new List<OrderItem>();
        }

        public void Dispose()
        {
        
        }

        public IList<OrderItem> GetAll()
        {
            return _orderItems;
        }

        public bool Save(OrderItem entity)
        {
            try
            {
                _orderItems.Add(entity);
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