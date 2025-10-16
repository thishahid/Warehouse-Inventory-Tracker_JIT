using System;

namespace WarehouseInventoryTracker
{
    public class Product
    {
        public string Id { get; }
        public string Name { get; }
        public int Quantity { get; private set; }
        public int ReorderThreshold { get; }

        public Product(string id, string name, int quantity, int reorderThreshold)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Quantity = quantity;
            ReorderThreshold = reorderThreshold;
        }

        public void IncreaseQuantity(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            Quantity += amount;
        }

        public void DecreaseQuantity(int amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be positive", nameof(amount));

            if (Quantity < amount)
                throw new InvalidOperationException("Insufficient stock");

            Quantity -= amount;
        }

        public bool IsBelowThreshold => Quantity <= ReorderThreshold;
    }
}