using System;
using System.Windows;

namespace WarehouseInventoryTracker
{
    public partial class QuantityDialog : Window
    {
        public string Title { get; set; }
        public string Prompt { get; set; }
        public int Quantity { get; private set; }

        public QuantityDialog(string title, string prompt)
        {
            Title = title;
            Prompt = prompt;
            InitializeComponent();
            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid positive integer", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                QuantityTextBox.Focus();
                return;
            }

            Quantity = quantity;
            DialogResult = true;
            Close();
        }
    }
}