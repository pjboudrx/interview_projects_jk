using System;
using System.Collections.Generic;
using System.Linq;
using Domain.DomainClasses;
using Domain.Repository.Interfaces;

namespace Domain.Repository.Implementation
{
    public class ItemRepository : IRepository<Item>
    {
        private readonly IList<Item> _items;

        public ItemRepository()
        {
            _items = new List<Item>();
        }

        public void Dispose()
        {
        
        }

        public IList<Item> GetAll()
        {
            return _items;
        }

        public bool Save(Item entity)
        {
            try
            {
                lock (_items)
                {
                    var id = _items.Select(item => item.id).Max();
                    entity.id = ++id;
                    _items.Add(entity);
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