using System.Windows;
using System.Windows.Controls;

namespace WarehouseInventoryTracker
{
    public partial class DeleteWarehouseDialog : Window
    {
        private readonly string _warehouseName;

        // Public property to check the result after the dialog closes
        public bool IsConfirmed { get; private set; }

        public DeleteWarehouseDialog(string warehouseName)
        {
            InitializeComponent();
            _warehouseName = warehouseName;
            WarehouseNameDisplay.Text = _warehouseName;
        }

        private void ConfirmationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Enable the delete button only if the text matches exactly
            DeleteButton.IsEnabled = ConfirmationTextBox.Text == _warehouseName;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }
    }
}