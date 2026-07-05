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

using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Collections.ObjectModel;


namespace Grocery_Sort
{
    /// <summary>
    /// Interaktionslogik für CreateList.xaml
    /// </summary>
    public partial class CreateList : Window, INotifyPropertyChanged
    {
        /* =====   Private fields  ===== */
        private StoreLayout previous;
        private ObservableCollection<GroceryList> Lists { get; set; }
        private GroceryList OriginalList { get; set; }

        private bool editmode = false;

        /* ===== Public properties ===== */
        // Creates an event use by INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public GroceryList NewList { get; set; }

        public ObservableCollection<StoreLayout> LayoutList { get; set; }
              
        public ObservableCollection<GroceryItem> CurrentFavorites { get; set; }

        /* =====    Constructors   ===== */
        // Constructor used when creating a new grocery list
        public CreateList(ObservableCollection<GroceryList> AllLists, ObservableCollection<StoreLayout> Layouts)
        {
            InitializeComponent();

            LayoutList = Layouts;
            NewList = new GroceryList();
            Lists = AllLists;

            DataContext = this;
        }

        // Constructor used when editing an existing list
        public CreateList(ObservableCollection<GroceryList> AllLists, ObservableCollection<StoreLayout> Layouts, GroceryList existing)
        {
            InitializeComponent();

            LayoutList = Layouts;
            OriginalList = existing;
            NewList = new GroceryList { 
                Id = existing.Id,
                Name = existing.Name,
                Layout = existing.Layout,
                Items = new ObservableCollection<GroceryItem>(existing.Items)
            };
            Lists = AllLists;

            EnterListName.Text = NewList.Name;

            foreach (StoreLayout layout in LayoutList)
            {
                if (layout.Id == NewList.Layout.Id)
                {
                    SelectLayout.SelectedItem = layout;
                    break;
                }
            }

            Create.Text = "Edit List";

            editmode = true;

            DataContext = this;
        }

        // Validate required input and prevent duplicate list names
        private bool Validation(StoreLayout selectedLayout, string listname)
        {
            bool full = true;

            if (String.IsNullOrWhiteSpace(listname))
            {
                MessageBox.Show("Please enter a Listname!");
                full = false;
            }
            if (selectedLayout == null)
            {
                MessageBox.Show("Please choose a Layout.");
                full = false;
            }
            if (editmode == false)
            {
                foreach (GroceryList grocery in Lists)
                {
                    if (grocery.Name == listname && grocery != NewList)
                    {
                        MessageBox.Show("List name is already taken.");
                        full = false;
                        break;
                    }
                }
            }
            return full;
        }

        // Warn user before clearing items when switching to another layout
        private void Layout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            StoreLayout NewLayout = combo.SelectedItem as StoreLayout;

            if (NewLayout == null) return;

            if (previous == null)
            {
                previous = NewLayout;
            }

            if (NewLayout != previous && NewList.Items.Count > 0)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Changing layout will remove all current items from list. Continue?",
                    "Confirm Layout Change",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                    );

                if (result == MessageBoxResult.Yes)
                {
                    NewList.Items.Clear();
                    previous = NewLayout;

                }
                else
                {
                    combo.SelectedItem = previous;
                }
            }
            else
            {
                previous = NewLayout;
            }

            // Update favorites shown for the currently selected layout
            CurrentFavorites = NewLayout.Favorites;

            // CurrentFavorites changed: refresh UI
            OnPropertyChanged(nameof(CurrentFavorites));
        }

        // Triggers the PropertyChanged event; [CallerMemberName] automatically inserts property name
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // WPF refreshes bound controls automatically
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            StoreLayout selectedLayout = SelectLayout.SelectedItem as StoreLayout;
            string listname = EnterListName.Text;
            bool valid = Validation(selectedLayout, listname);

            if (valid)
            {
                NewList.Name = listname;
                NewList.Layout = selectedLayout;

                AddItem window = new AddItem(NewList);

                window.Owner = this;
                window.ShowDialog();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void SaveList_Click(object sender, RoutedEventArgs e)
        {
            StoreLayout selectedLayout = SelectLayout.SelectedItem as StoreLayout;
            string listname = EnterListName.Text;
            bool valid = Validation(selectedLayout, listname);

            if (valid)
            {
                NewList.Name = listname;
                NewList.Layout = selectedLayout;

                NewList.Items = SortList(NewList);

                // Save as new list or update existing list depending on current mode
                if (editmode)
                {
                    DatabaseHelper.UpdateList(NewList);

                    OriginalList.Name = NewList.Name;
                    OriginalList.Layout = NewList.Layout;
                    OriginalList.Items = NewList.Items;
                }
                else
                {
                    DatabaseHelper.SaveList(NewList);
                }
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Please select Layout and enter Listname!");
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            GroceryItem item = button?.DataContext as GroceryItem; 

            if (item != null)
            {
                NewList.Items.Remove(item);
            }
        }

        // Sort grocery items based on the selected layout category order
        private ObservableCollection<GroceryItem> SortList(GroceryList grocerylist)
        {
            ObservableCollection<GroceryItem> sortedGroceryList = new ObservableCollection<GroceryItem>();

            List<GroceryItem> list = grocerylist.Items.ToList();

            ObservableCollection<string> oc_categories = grocerylist.Layout.Categories;
            List<string> categories = oc_categories.ToList();

            foreach (string category in categories)
            {
                foreach (GroceryItem item in list)
                {
                    if (item.ItemCategory == category)
                    {
                        sortedGroceryList.Add(item);
                    }
                }
            }

            return sortedGroceryList;
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            GroceryItem item = button?.DataContext as GroceryItem;

            // Reuse AddItem window in edit mode by passing the selected item
            AddItem window = new AddItem(NewList, item);

            window.Owner = this;
            bool? result = window.ShowDialog();

            if (result == true)
            {
                NewList.Items = SortList(NewList);
            }
        }

        // Copy selected favorite items into the current grocery list
        private void AddFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (NewList.Items.Count == 0)
            {
                StoreLayout selectedLayout = SelectLayout.SelectedItem as StoreLayout;
                string listname = EnterListName.Text;
                bool valid = Validation(selectedLayout, listname);

                if (valid)
                {
                    NewList.Name = listname;
                    NewList.Layout = selectedLayout;
                }
                else { return; }
            }

            if (CurrentFavorites.Count == 0)
            {
                MessageBox.Show("No favorite items for this layout available.");
                return;
            }

            foreach (GroceryItem favorite in CurrentFavorites)
            {
                bool exists = false;

                if (favorite.IsSelected)
                {
                    GroceryItem copy = new GroceryItem
                    {
                        Name = favorite.Name,
                        ItemCategory = favorite.ItemCategory,
                        IsChecked = false,
                        Amount = favorite.Amount,
                        IsFavorite = true,
                        FavId = favorite.FavId,
                    };

                    // Prevent adding the same favorite item twice
                    foreach (GroceryItem item in NewList.Items)
                    {

                        if (item.FavId == favorite.FavId)
                        {

                            MessageBox.Show($"{item.Name} has already been added.");
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        NewList.Items.Add(copy);
                    }

                    favorite.IsSelected = false;
                }
            }
            NewList.Items = SortList(NewList);
        }

        // Toggle favorite state and update layout favorites
        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            GroceryItem item = button?.DataContext as GroceryItem;

            if (item != null)
            {
                item.IsFavorite = !item.IsFavorite;

                if (item.IsFavorite)
                {
                    item.FavId = DatabaseHelper.SaveFavoriteItem(NewList.Layout.Id, item);

                    GroceryItem favorite = new GroceryItem
                    {
                        Name = item.Name,
                        ItemCategory = item.ItemCategory,
                        Amount = item.Amount,
                        IsFavorite = true,
                        FavId = item.FavId
                    };
                    NewList.Layout.Favorites.Add(favorite);
                }
                else
                {
                    GroceryItem removefav = null;

                    if (item.FavId != null)
                    {

                        foreach (GroceryItem favorite in NewList.Layout.Favorites)
                        {
                            if (favorite.FavId == item.FavId)
                            {
                                removefav = favorite;
                                break;
                            }
                        }

                        if (removefav != null)
                        {
                            DatabaseHelper.DeleteFavoriteItem(removefav.FavId.Value);
                            NewList.Layout.Favorites.Remove(removefav);

                            item.FavId = null;
                        }
                    }
                }
            }
        }
    }
}
