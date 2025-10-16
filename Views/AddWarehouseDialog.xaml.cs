using System.Windows;

namespace WarehouseInventoryTracker
{
    public partial class AddWarehouseDialog : Window
    {
        public string WarehouseId => WarehouseIdTextBox.Text.Trim();
        public string WarehouseName => WarehouseNameTextBox.Text.Trim();

        public AddWarehouseDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(WarehouseId))
            {
                MessageBox.Show("Please enter a warehouse ID", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WarehouseIdTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(WarehouseName))
            {
                MessageBox.Show("Please enter a warehouse name", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WarehouseNameTextBox.Focus();
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}