using System;

namespace WarehouseInventoryTracker
{
    public class AlertService : IStockObserver
    {
        public void OnStockBelowThreshold(Product product)
        {
            // In a real application, this might send an email, SMS, or push notification
            // For this demo, we'll just write to the debug output
            string message = $"Low stock for {product.Name} (ID: {product.Id}) - only {product.Quantity} left! Threshold: {product.ReorderThreshold}";
            System.Diagnostics.Debug.WriteLine(message);

            // In a WPF application, we might want to raise an event that the UI can subscribe to
            StockAlert?.Invoke(this, new StockAlertEventArgs(product, message));
        }

        public event EventHandler<StockAlertEventArgs> StockAlert;
    }

    public class StockAlertEventArgs : EventArgs
    {
        public Product Product { get; }
        public string Message { get; }

        public StockAlertEventArgs(Product product, string message)
        {
            Product = product;
            Message = message;
        }
    }
}