using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media.Effects;

namespace Grocery_Sort
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /* =====   Private fields  ===== */
        private GroceryList currentList;

        /* ===== Public properties ===== */
        public event PropertyChangedEventHandler PropertyChanged;
        
        public GroceryList CurrentList
        {
            get { return currentList; }
            set
            {
                currentList = value;
                OnPropertyChanged();
            }
        }
        public StoreLayout DefaultLayout { get; set; }

        public ObservableCollection<StoreLayout> AllLayouts { get; set; }

        public ObservableCollection<GroceryList> AllLists { get; set; }
            = new ObservableCollection<GroceryList>();

        public string DefaultName;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize database and load saved application data
            DatabaseHelper.CreateTables();
            AllLayouts = DatabaseHelper.LoadLayouts();
            AllLists = DatabaseHelper.LoadLists();

            DefaultLayout = new StoreLayout();
            DefaultName = string.Empty;

            // Restore the most recently created list on startup
            if (AllLists.Count > 0)
            {
                CurrentList = AllLists[AllLists.Count - 1];
            }
            else
            {
                // Create temporary empty list state if no saved lists exist
                CurrentList = new GroceryList
                {
                    Name = DefaultName,
                    Layout = DefaultLayout
                };
            }
            DataContext = this;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CreateList_Click(object sender, RoutedEventArgs e)
        {
            if (AllLayouts != null && AllLayouts.Count != 0)
            {
                // Open list creation dialog and pass existing layouts and lists
                CreateList window = new CreateList(AllLists, AllLayouts);
                window.Owner = this;
                bool? result = window.ShowDialog();

                if (result == true)
                {
                    GroceryList newlist = window.NewList;

                    AllLists.Add(newlist);
                    CurrentList = newlist;
                }
            }
            else
            {
                MessageBox.Show("No layout available. Please add layout before creating list.");
            }
        }

        private void ViewLists_Click(object sender, RoutedEventArgs e)
        {
            if (AllLists.Count != 0 && AllLists != null)
            {
                ViewLists window = new ViewLists(AllLists, AllLayouts);
                window.Owner = this;
                bool? result = window.ShowDialog();

                if (result == true && window.SelectedList != null)
                {
                    CurrentList = window.SelectedList;
                }

                // Reset current list if the previously selected list was deleted
                if (!AllLists.Contains(CurrentList))
                {
                    CurrentList = new GroceryList();
                }
            }
            else
            {
                MessageBox.Show("No lists created yet.");
            }
        }

        private void ManageLayouts_Click(object sender, RoutedEventArgs e)
        {
            ManageLayouts window = new ManageLayouts(AllLayouts, AllLists);
            window.Owner = this;
            bool? result = window.ShowDialog();

            // Add all created layouts to main
            if (window.AllLayouts != null)
            {
                AllLayouts = window.AllLayouts;
            }

            // Set default layout
            if (result == true && window.Layout != null)
            {
                CurrentList.Layout = window.Layout;
            }

            // Reset layout reference if the current layout was deleted
            if (!AllLayouts.Contains(CurrentList.Layout))
            {
                DefaultLayout = new StoreLayout();
            }
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (AllLists.Count == 0)
            {
                if (AllLayouts.Count == 0)
                {
                    MessageBox.Show("No layout available.");
                    return;
                }
                MessageBox.Show("No list available yet.");
                return;
            }
            else
            {
                AddItem window = new AddItem(CurrentList);

                window.Owner = this;
                bool? result = window.ShowDialog();
                if (result == true)
                {
                    // Re-sort items after adding a new entry
                    CurrentList.Items = SortList(CurrentList);
                    DatabaseHelper.UpdateList(CurrentList);
                }
            }
        }

        // Sort grocery items based on the category order of the selected layout
        private ObservableCollection<GroceryItem> SortList(GroceryList grocerylist)
        {
            ObservableCollection<GroceryItem> sortedGroceryList = new ObservableCollection<GroceryItem>();

            List<GroceryItem> list = grocerylist.Items.ToList();

            ObservableCollection<string> oc_categories = grocerylist.Layout.Categories;
            List<string> categories = oc_categories.ToList();

            // Add items in the same order as categories appear in the store layout
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

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button; // button = sender
            // Retrieve the clicked grocery item from the button's DataContext
            GroceryItem item = button?.DataContext as GroceryItem; // Only continue if button not null

            if (item != null)
            {
                CurrentList.Items.Remove(item);
                DatabaseHelper.UpdateList(CurrentList);
            }
        }

        private void ItemCheckedChanged(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            GroceryItem item = checkbox?.DataContext as GroceryItem;

            if (item != null)
            {
                item.IsChecked = checkbox.IsChecked == true;
                // Persist checkbox state changes immediately
                DatabaseHelper.UpdateItemChecked(item);
            }
        }

        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            GroceryItem item = button?.DataContext as GroceryItem;

            // Reuse AddItem window in edit mode by passing the selected item
            AddItem window = new AddItem(CurrentList, item);

            window.Owner = this;
            bool? result = window.ShowDialog();

            if (result == true)
            {
                // Re-sort list after item changes
                CurrentList.Items = SortList(CurrentList);
            }
        }

        // Add or remove reusable favorite items for the current layout
        private void Favorite_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            GroceryItem item = button?.DataContext as GroceryItem;

            if (item != null)
            {
                // Toggle favorite state
                item.IsFavorite = !item.IsFavorite;

                if (item.IsFavorite)
                {
                    item.FavId = DatabaseHelper.SaveFavoriteItem(CurrentList.Layout.Id, item);

                    // Create separate favorite item copy for layout favorites collection
                    GroceryItem favorite = new GroceryItem
                    {
                        Name = item.Name,
                        ItemCategory = item.ItemCategory,
                        Amount = item.Amount,
                        IsFavorite = true,
                        FavId = item.FavId
                    };
                    CurrentList.Layout.Favorites.Add(favorite);
                }
                else
                {
                    GroceryItem removefav = null;

                    if (item.FavId != null)
                    {

                        foreach (GroceryItem favorite in CurrentList.Layout.Favorites)
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
                            CurrentList.Layout.Favorites.Remove(removefav);

                            item.FavId = null;
                        }
                    }
                }
                DatabaseHelper.UpdateList(CurrentList);
            }
        }
    }
}