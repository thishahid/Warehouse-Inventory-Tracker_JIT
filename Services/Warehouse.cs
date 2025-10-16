using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Linq;

namespace WarehouseInventoryTracker
{
    public class Warehouse
    {
        private readonly string _id;
        private readonly string _name;
        private readonly ConcurrentDictionary<string, Product> _products;
        private readonly List<IStockObserver> _observers;
        private readonly ReaderWriterLockSlim _observerLock = new ReaderWriterLockSlim();

        public string Id => _id;
        public string Name => _name;
        public IReadOnlyCollection<Product> Products => _products.Values.ToList().AsReadOnly();

        public Warehouse(string id, string name)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _products = new ConcurrentDictionary<string, Product>();
            _observers = new List<IStockObserver>();
        }

        public void AddProduct(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (!_products.TryAdd(product.Id, product))
                throw new InvalidOperationException($"Product with ID {product.Id} already exists");
        }

        public Product GetProduct(string productId)
        {
            if (productId == null)
                throw new ArgumentNullException(nameof(productId));

            if (!_products.TryGetValue(productId, out Product product))
                throw new KeyNotFoundException($"Product with ID {productId} not found");

            return product;
        }

        public void ReceiveShipment(string productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            Product product = GetProduct(productId);
            product.IncreaseQuantity(quantity);
        }

        public void FulfillOrder(string productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            Product product = GetProduct(productId);
            int previousQuantity = product.Quantity;

            product.DecreaseQuantity(quantity);

            // Check if we crossed the threshold
            if (previousQuantity > product.ReorderThreshold && product.IsBelowThreshold)
            {
                NotifyObservers(product);
            }
        }

        public void RegisterObserver(IStockObserver observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            _observerLock.EnterWriteLock();
            try
            {
                if (!_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
            }
            finally
            {
                _observerLock.ExitWriteLock();
            }
        }

        public void UnregisterObserver(IStockObserver observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            _observerLock.EnterWriteLock();
            try
            {
                _observers.Remove(observer);
            }
            finally
            {
                _observerLock.ExitWriteLock();
            }
        }

        private void NotifyObservers(Product product)
        {
            _observerLock.EnterReadLock();
            try
            {
                foreach (var observer in _observers)
                {
                    try
                    {
                        observer.OnStockBelowThreshold(product);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception but continue notifying other observers
                        System.Diagnostics.Debug.WriteLine($"Error notifying observer: {ex.Message}");
                    }
                }
            }
            finally
            {
                _observerLock.ExitReadLock();
            }
        }
    }
}