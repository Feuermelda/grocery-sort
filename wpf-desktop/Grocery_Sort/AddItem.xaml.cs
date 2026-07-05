using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Grocery_Sort
{
    /// <summary>
    /// Interaktionslogik für AddItem.xaml
    /// </summary>
    public partial class AddItem : Window
    {
        /* =====   Private fields  ===== */
        bool editmode = false;

        /* ===== Public properties ===== */
        public GroceryList CurrentList { get; set; }
        public GroceryItem CurrentItem { get; set; }

        /* =====    Constructors   ===== */
        // Constructor used when adding a new grocery item
        public AddItem(GroceryList currentList)
        {
            InitializeComponent();
            CurrentList = currentList;

            DataContext = this;
            EnterItem.Focus();
        }

        // Constructor used when editing an existing grocery item
        public AddItem(GroceryList currentList, GroceryItem currentItem)
        {
            InitializeComponent();
            CurrentList = currentList;
            CurrentItem = currentItem;

            EnterItem.Text = CurrentItem.Name;
            SelectedCategory.SelectedItem = CurrentItem.ItemCategory;
            EnterAmount.Text = CurrentItem.Amount;
            Add.Text = "Edit Item";

            EnterItem.Focus();
            EnterItem.CaretIndex = EnterItem.Text.Length;

            editmode = true;

            DataContext = this;
        }

        // Validate item input and either create or update item
        private bool AddNextItem()
        {
            if (string.IsNullOrWhiteSpace(EnterItem.Text))
            {
                MessageBox.Show("Please enter an item first.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(EnterAmount.Text))
            {
                MessageBox.Show("Please enter an amount.");
                return false;
            }

            var item = new GroceryItem
            {
                Name = EnterItem.Text,
                ItemCategory = SelectedCategory.SelectedItem?.ToString(),
                Amount = EnterAmount.Text
            };
            bool exists = false;

            if (SelectedCategory.SelectedItem == null)
            {
                MessageBox.Show("Please select a category.");
                return false;
            }

            //Prevent duplicate items with the same name and category
            foreach (GroceryItem groceryitem in CurrentList.Items)
            {
                if (groceryitem.Name == item.Name && groceryitem.ItemCategory == item.ItemCategory && editmode == false)
                {
                    exists = true;
                    MessageBox.Show("Item has already been added.");
                    return false;
                }
            }

            if (!exists && !editmode)
            {
                // Create and add a new grocery item to the current list
                GroceryItem newItem = new GroceryItem
                {
                    Name = item.Name,
                    ItemCategory = item.ItemCategory,
                    IsChecked = false,
                    Amount = item.Amount
                };
                CurrentList.Items.Add(newItem);
            }

            if (editmode)
            {
                // Update the existing item in edit mode
                CurrentItem.Name = EnterItem.Text;
                CurrentItem.ItemCategory = SelectedCategory.SelectedItem.ToString();
                CurrentItem.Amount = EnterAmount.Text;
            }
            return true;
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            bool added = AddNextItem();

            if (added)
            {
                // Clear input fields after successfully adding an item
                EnterItem.Text = "";
                EnterAmount.Text = "";
                SelectedCategory.SelectedIndex = -1;
                EnterItem.Focus();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            bool added = true;

            if (EnterItem.Text != "")
            {
                added = AddNextItem();
            }
            if (added)
            {
                // Save current item and close dialog
                DialogResult = true;
                Close();
            }
        }
    }
}
