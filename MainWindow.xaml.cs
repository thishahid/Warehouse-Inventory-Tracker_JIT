using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; // Added for CollectionViewSource

namespace WarehouseInventoryTracker
{
    public partial class MainWindow : Window
    {
        private readonly WarehouseManager _warehouseManager;
        private Warehouse _currentWarehouse;
        private ObservableCollection<ProductViewModel> _products;
        private ICollectionView _productsView; // Added for search functionality

        public MainWindow()
        {
            InitializeComponent();
            _warehouseManager = new WarehouseManager();
            _warehouseManager.StockAlert += OnStockAlert;

            InitializeUI();
        }

        private void InitializeUI()
        {
            // Populate warehouse combo box
            foreach (var warehouse in _warehouseManager.GetAllWarehouses())
            {
                WarehouseComboBox.Items.Add(new WarehouseViewModel(warehouse));
            }

            if (WarehouseComboBox.Items.Count > 0)
            {
                WarehouseComboBox.SelectedIndex = 0;
            }
        }

        private void WarehouseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WarehouseComboBox.SelectedItem is WarehouseViewModel warehouseViewModel)
            {
                _currentWarehouse = warehouseViewModel.Warehouse;
                RefreshProductList();
                // Apply the current search text to the new warehouse's products
                ApplyFilter(SearchTextBox.Text);
            }
        }

        private void RefreshProductList()
        {
            if (_currentWarehouse == null) return;

            // Create or update the source collection
            _products = new ObservableCollection<ProductViewModel>(
                _currentWarehouse.Products.Select(p => new ProductViewModel(p)));

            // Create the collection view from the source collection
            _productsView = CollectionViewSource.GetDefaultView(_products);

            // Set the DataGrid's source to the view
            ProductDataGrid.ItemsSource = _productsView;
        }

        // New method to handle search filtering
        private void ApplyFilter(string searchText)
        {
            if (_productsView == null) return;

            // Set the filter predicate on the collection view
            _productsView.Filter = item =>
            {
                // If search text is empty, show all items
                if (string.IsNullOrWhiteSpace(searchText))
                    return true;

                // Check if the item is a ProductViewModel and if its ID or Name contains the search text
                if (item is ProductViewModel product)
                {
                    return product.Id.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                           product.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                }

                return false;
            };

            // Refresh the view to apply the filter
            _productsView.Refresh();
        }

        // New event handler for the search TextBox
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get the search text from the TextBox and apply the filter
            ApplyFilter(SearchTextBox.Text);
        }

        private async void AddWarehouseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddWarehouseDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var warehouse = _warehouseManager.CreateWarehouse(dialog.WarehouseId, dialog.WarehouseName);
                    WarehouseComboBox.Items.Add(new WarehouseViewModel(warehouse));
                    WarehouseComboBox.SelectedItem = WarehouseComboBox.Items.Cast<WarehouseViewModel>()
                        .FirstOrDefault(w => w.Warehouse.Id == warehouse.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding warehouse: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWarehouse == null)
            {
                MessageBox.Show("Please select a warehouse first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new AddProductDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var product = new Product(
                        dialog.ProductId,
                        dialog.ProductName,
                        dialog.InitialQuantity,
                        dialog.ReorderThreshold);

                    await _warehouseManager.AddProductAsync(_currentWarehouse.Id, product);
                    RefreshProductList(); // This will recreate the view
                    ApplyFilter(SearchTextBox.Text); // Re-apply the search filter
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ReceiveShipmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWarehouse == null)
            {
                MessageBox.Show("Please select a warehouse first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ProductDataGrid.SelectedItem is ProductViewModel selectedProduct)
            {
                var dialog = new QuantityDialog("Receive Shipment", "Quantity to receive:");
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        await _warehouseManager.ReceiveShipmentAsync(_currentWarehouse.Id, selectedProduct.Id, dialog.Quantity);
                        RefreshProductList();
                        ApplyFilter(SearchTextBox.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error receiving shipment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a product first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void FulfillOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWarehouse == null)
            {
                MessageBox.Show("Please select a warehouse first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ProductDataGrid.SelectedItem is ProductViewModel selectedProduct)
            {
                var dialog = new QuantityDialog("Fulfill Order", "Quantity to fulfill:");
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        await _warehouseManager.FulfillOrderAsync(_currentWarehouse.Id, selectedProduct.Id, dialog.Quantity);
                        RefreshProductList();
                        ApplyFilter(SearchTextBox.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error fulfilling order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a product first", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentWarehouse == null)
            {
                MessageBox.Show("Please select a warehouse first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ProductDataGrid.SelectedItem is ProductViewModel selectedProduct)
            {
                // Show a confirmation dialog
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the product '{selectedProduct.Name}' (ID: {selectedProduct.Id})?",
                    "Confirm Deletion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _warehouseManager.RemoveProductAsync(_currentWarehouse.Id, selectedProduct.Id);
                        RefreshProductList();
                        ApplyFilter(SearchTextBox.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                // This case should not happen due to the IsEnabled binding, but it's good practice.
                MessageBox.Show("Please select a product to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void DeleteWarehouseButton_Click(object sender, RoutedEventArgs e)
        {
            if (WarehouseComboBox.SelectedItem is WarehouseViewModel selectedWarehouseViewModel)
            {
                var warehouse = selectedWarehouseViewModel.Warehouse;

                // Show the custom confirmation dialog
                var dialog = new DeleteWarehouseDialog(warehouse.Name);
                if (dialog.ShowDialog() == true)
                {
                    // Check if the user confirmed the deletion in the dialog
                    if (dialog.IsConfirmed)
                    {
                        try
                        {
                            await _warehouseManager.RemoveWarehouseAsync(warehouse.Id);

                            // Remove the warehouse from the ComboBox
                            WarehouseComboBox.Items.Remove(selectedWarehouseViewModel);

                            // Check if there are any warehouses left
                            if (WarehouseComboBox.Items.Count > 0)
                            {
                                // Select the first available warehouse
                                WarehouseComboBox.SelectedIndex = 0;
                            }
                            else
                            {
                                // No warehouses left, clear the UI
                                _currentWarehouse = null;
                                ProductDataGrid.ItemsSource = null;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting warehouse: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                // This case should not happen due to the IsEnabled binding
                MessageBox.Show("Please select a warehouse to delete.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OnStockAlert(object sender, StockAlertEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                string alertMessage = $"[{DateTime.Now}] {e.Message}\n";
                AlertTextBox.Text = alertMessage + AlertTextBox.Text;

                // Update the entire list to ensure the status is updated correctly
                RefreshProductList();
                ApplyFilter(SearchTextBox.Text);
            });
        }
    }

    // View Models for data binding
    public class WarehouseViewModel
    {
        public Warehouse Warehouse { get; }

        public string DisplayText => $"{Warehouse.Name} ({Warehouse.Id})";

        public WarehouseViewModel(Warehouse warehouse)
        {
            Warehouse = warehouse;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }

    public class ProductViewModel : INotifyPropertyChanged
    {
        private readonly Product _product;

        public string Id => _product.Id;
        public string Name => _product.Name;
        public int Quantity => _product.Quantity;
        public int ReorderThreshold => _product.ReorderThreshold;

        private string _status;
        public string Status
        {
            get => _status;
            private set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public ProductViewModel(Product product)
        {
            _product = product;
            UpdateStatus();
        }

        public void UpdateStatus()
        {
            Status = _product.IsBelowThreshold
                ? $"LOW STOCK - {_product.Quantity} left (Threshold: {_product.ReorderThreshold})"
                : "In Stock";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


namespace WarehouseInventoryTracker
{
    public class NullToBooleanConverter : IValueConverter
    {
        // Converts the selected item (value) to a boolean (true if not null)
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        // Not needed for this scenario
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}