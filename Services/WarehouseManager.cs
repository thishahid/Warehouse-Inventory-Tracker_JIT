using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WarehouseInventoryTracker
{
    public class WarehouseManager
    {
        private readonly Dictionary<string, Warehouse> _warehouses;
        private readonly AlertService _alertService;
        private readonly string _dataFilePath;

        public WarehouseManager(string dataFilePath = "warehouse_data.txt")
        {
            _warehouses = new Dictionary<string, Warehouse>();
            _alertService = new AlertService();
            _dataFilePath = dataFilePath;

            LoadData();
        }

        public event EventHandler<StockAlertEventArgs> StockAlert
        {
            add { _alertService.StockAlert += value; }
            remove { _alertService.StockAlert -= value; }
        }

        public Warehouse CreateWarehouse(string id, string name)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Warehouse ID cannot be empty", nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Warehouse name cannot be empty", nameof(name));

            if (_warehouses.ContainsKey(id))
                throw new InvalidOperationException($"Warehouse with ID {id} already exists");

            var warehouse = new Warehouse(id, name);
            warehouse.RegisterObserver(_alertService);
            _warehouses.Add(id, warehouse);

            return warehouse;
        }

        public Warehouse GetWarehouse(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Warehouse ID cannot be empty", nameof(id));

            if (!_warehouses.TryGetValue(id, out Warehouse warehouse))
                throw new KeyNotFoundException($"Warehouse with ID {id} not found");

            return warehouse;
        }

        public IReadOnlyCollection<Warehouse> GetAllWarehouses()
        {
            return _warehouses.Values.ToList().AsReadOnly();
        }

        public async Task ReceiveShipmentAsync(string warehouseId, string productId, int quantity)
        {
            await Task.Run(() =>
            {
                var warehouse = GetWarehouse(warehouseId);
                warehouse.ReceiveShipment(productId, quantity);
                SaveData();
            });
        }

        public async Task FulfillOrderAsync(string warehouseId, string productId, int quantity)
        {
            await Task.Run(() =>
            {
                var warehouse = GetWarehouse(warehouseId);
                warehouse.FulfillOrder(productId, quantity);
                SaveData();
            });
        }

        public async Task AddProductAsync(string warehouseId, Product product)
        {
            await Task.Run(() =>
            {
                var warehouse = GetWarehouse(warehouseId);
                warehouse.AddProduct(product);
                SaveData();
            });
        }

        public async Task RemoveProductAsync(string warehouseId, string productId)
        {
            await Task.Run(() =>
            {
                var warehouse = GetWarehouse(warehouseId);
                warehouse.RemoveProduct(productId);
                SaveData(); // Persist the change
            });
        }

        public async Task RemoveWarehouseAsync(string warehouseId)
        {
            await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(warehouseId))
                    throw new ArgumentException("Warehouse ID cannot be empty.", nameof(warehouseId));

                if (!_warehouses.Remove(warehouseId))
                {
                    // If Remove returns false, the warehouse ID was not found.
                    throw new KeyNotFoundException($"Warehouse with ID {warehouseId} not found.");
                }

                SaveData(); // Persist the change
            });
        }

        private void LoadData()
        {
            if (!File.Exists(_dataFilePath))
                return;

            try
            {
                var lines = File.ReadAllLines(_dataFilePath);
                string currentWarehouseId = null;

                foreach (var line in lines)
                {
                    if (line.StartsWith("WAREHOUSE:"))
                    {
                        var parts = line.Substring(10).Split('|');
                        if (parts.Length >= 2)
                        {
                            currentWarehouseId = parts[0].Trim();
                            var warehouseName = parts[1].Trim();
                            CreateWarehouse(currentWarehouseId, warehouseName);
                        }
                    }
                    else if (line.StartsWith("PRODUCT:") && !string.IsNullOrEmpty(currentWarehouseId))
                    {
                        var parts = line.Substring(8).Split('|');
                        if (parts.Length >= 4)
                        {
                            var productId = parts[0].Trim();
                            var productName = parts[1].Trim();
                            if (int.TryParse(parts[2].Trim(), out int quantity) &&
                                int.TryParse(parts[3].Trim(), out int threshold))
                            {
                                var product = new Product(productId, productName, quantity, threshold);
                                GetWarehouse(currentWarehouseId).AddProduct(product);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void SaveData()
        {
            try
            {
                using (var writer = new StreamWriter(_dataFilePath, false))
                {
                    foreach (var warehouse in _warehouses.Values)
                    {
                        writer.WriteLine($"WAREHOUSE:{warehouse.Id}|{warehouse.Name}");

                        foreach (var product in warehouse.Products)
                        {
                            writer.WriteLine($"PRODUCT:{product.Id}|{product.Name}|{product.Quantity}|{product.ReorderThreshold}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving data: {ex.Message}");
            }
        }
    }
}