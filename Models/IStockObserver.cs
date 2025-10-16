using System;

namespace WarehouseInventoryTracker
{
    public interface IStockObserver
    {
        void OnStockBelowThreshold(Product product);
    }
}