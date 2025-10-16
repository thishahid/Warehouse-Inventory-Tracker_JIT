using System;
using System.Windows;

namespace WarehouseInventoryTracker
{
    public partial class AddProductDialog : Window
    {
        public string ProductId => ProductIdTextBox.Text.Trim();
        public string ProductName => ProductNameTextBox.Text.Trim();
        public int InitialQuantity { get; private set; }
        public int ReorderThreshold { get; private set; }

        public AddProductDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductId))
            {
                MessageBox.Show("Please enter a product ID", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProductIdTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ProductName))
            {
                MessageBox.Show("Please enter a product name", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ProductNameTextBox.Focus();
                return;
            }

            if (!int.TryParse(InitialQuantityTextBox.Text, out int initialQuantity) || initialQuantity < 0)
            {
                MessageBox.Show("Please enter a valid non-negative integer for initial quantity", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                InitialQuantityTextBox.Focus();
                return;
            }

            if (!int.TryParse(ReorderThresholdTextBox.Text, out int reorderThreshold) || reorderThreshold < 0)
            {
                MessageBox.Show("Please enter a valid non-negative integer for reorder threshold", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ReorderThresholdTextBox.Focus();
                return;
            }

            InitialQuantity = initialQuantity;
            ReorderThreshold = reorderThreshold;

            DialogResult = true;
            Close();
        }
    }
}