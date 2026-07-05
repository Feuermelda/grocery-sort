using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace Grocery_Sort
{
    /// <summary>
    /// Interaktionslogik für CreateLayout.xaml
    /// </summary>
    public partial class CreateLayout : Window
    {
        /* =====   Private fields  ===== */
        private string selectedCat;
        private int selectedIndex;
        private StoreLayout originalLayout;

        private bool editmode = false;
        private bool editcategory = false;

        /* ===== Public properties ===== */
        public StoreLayout NewLayout { get; set; }
        public ObservableCollection<StoreLayout> Layouts { get; set; } = new ObservableCollection<StoreLayout>();
        public string Category { get; set; }

        /* =====    Constructors   ===== */
        // Constructor used when creating a new layout
        public CreateLayout(ObservableCollection<StoreLayout> AllLayouts)
        {
            InitializeComponent();

            Layouts = AllLayouts;
            NewLayout = new StoreLayout();

            DataContext = this;
        }

        // Constructor used when editing an existing layout
        public CreateLayout(ObservableCollection<StoreLayout> AllLayouts, StoreLayout existing)
        {
            InitializeComponent();

            originalLayout = existing;

            Layouts = AllLayouts;
            NewLayout = new StoreLayout { 
                Id = existing.Id,
                Name = existing.Name,
                Categories = new ObservableCollection<string>(existing.Categories),
                Favorites = existing.Favorites
            };

            EnterLayoutName.Text = NewLayout.Name;
            Layout.Text = "Edit Layout";
            editmode = true;

            DataContext = this;
        }

        // Enable category editing mode when selecting an existing category
        private void CategorySelected(object sender, SelectionChangedEventArgs e)
        {
            string EditCategory = CategoryListBox.SelectedItem as string;

            if (EditCategory != null && CategoryListBox.SelectedIndex >= 0)
            {
                editcategory = true;
                EnterCategory.Text = EditCategory;
                EnterCategory.Focus();
                EnterCategory.CaretIndex = EnterCategory.Text.Length;
                BtnAdd.Content = "Apply";
            }
        }

        // Validate layout and category input before adding categories
        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {

            Category = EnterCategory.Text;
            NewLayout.Name = EnterLayoutName.Text;
            if (String.IsNullOrWhiteSpace(NewLayout.Name))
            {
                MessageBox.Show("Please enter Layout name before adding categories.");
                return;
            }
            else
            {
                // Prevent duplicate category names within the same layout
                if (NewLayout.Categories.Contains(Category) && NewLayout.Categories.IndexOf(Category) != CategoryListBox.SelectedIndex)
                {
                    MessageBox.Show($"{Category} already exists for Layout {NewLayout.Name}.");
                    return;
                }
                if (String.IsNullOrWhiteSpace(Category))
                {
                    MessageBox.Show("Category cannot be empty.");
                    return;
                }
                if (editmode == false)
                {
                    foreach (StoreLayout layout in Layouts)
                    {
                        if (layout.Name == NewLayout.Name && layout != NewLayout)
                        {
                            MessageBox.Show("Layout name is already taken.");

                            return;
                        }
                    }
                }
            }

            if (editcategory)
            {
                // Replace existing category in edit mode
                NewLayout.Categories[CategoryListBox.SelectedIndex] = Category;
                editcategory = false;
            }
            else
            {
                // Add new category to layout
                NewLayout.Categories.Add(Category);
            }

            EnterCategory.Text = "";
            EnterCategory.Focus();
            BtnAdd.Content = "Add";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            NewLayout.Name = EnterLayoutName.Text;
            if (String.IsNullOrWhiteSpace(NewLayout.Name))
            {
                MessageBox.Show("Please enter a Layout name.");
                return;
            }
            else if (NewLayout.Categories.Count == 0)
            {
                MessageBox.Show("Layout can't be empty.");
                return;
            }

            // Save new layout or update existing layout depending on current mode
            if (editmode)
            {
                DatabaseHelper.UpdateLayout(NewLayout);

                originalLayout.Name = NewLayout.Name;
                originalLayout.Categories = NewLayout.Categories;
            }
            else
            {
                DatabaseHelper.SaveLayout(NewLayout);
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Move selected category upward in the layout order
        private void Up_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            selectedCat = button?.DataContext as string;

            if (selectedCat != null)
            {
                selectedIndex = NewLayout.Categories.IndexOf(selectedCat);
                int nextIndex = selectedIndex - 1;

                if (selectedIndex == 0)
                {
                    MessageBox.Show("First Category can't be moved up any further.");
                    return;
                }

                NewLayout.Categories.Move(selectedIndex, nextIndex);
            }
        }

        // Move selected category downward in the layout order
        private void Down_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            selectedCat = button?.DataContext as string;

            if (selectedCat != null)
            {
                selectedIndex = NewLayout.Categories.IndexOf(selectedCat);
                int nextIndex = selectedIndex + 1;
                if (selectedIndex == NewLayout.Categories.Count - 1)
                {
                    MessageBox.Show("Last Category can't be moved down any further.");
                    return;
                }
                NewLayout.Categories.Move(selectedIndex, nextIndex);
            }
        }

        // Remove selected category from the layout
        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            selectedCat = button?.DataContext as string;

            if (selectedCat != null)
            {
                NewLayout.Categories.Remove(selectedCat);
            }
        }
    }
}
