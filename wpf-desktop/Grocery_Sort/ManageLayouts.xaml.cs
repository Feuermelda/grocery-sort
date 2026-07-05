using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaktionslogik für ManageLayouts.xaml
    /// </summary>
    public partial class ManageLayouts : Window, INotifyPropertyChanged
    {
        /* =====   Private fields  ===== */
        private StoreLayout currentLayout;
        private bool? dialogResult = false;

        /* ===== Public properties ===== */
        public event PropertyChangedEventHandler PropertyChanged;

        public StoreLayout CurrentLayout
        {
            get
            {
                return currentLayout;
            }
            set
            {
                currentLayout = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<StoreLayout> AllLayouts { get; set; }
        = new ObservableCollection<StoreLayout>();

        public ObservableCollection<GroceryList> AllLists { get; set; }

        // Currently selected or newly created layout
        public StoreLayout Layout { get; set; }
        
        public ManageLayouts(ObservableCollection<StoreLayout> LayoutList, ObservableCollection<GroceryList> AllGroceryLists)
        {
            InitializeComponent();

            AllLayouts = LayoutList;
            Layout = new StoreLayout();
            AllLists = AllGroceryLists;

            DataContext = this;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Open layout creation window and add the new layout to the shared collection
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            CreateLayout window = new CreateLayout(AllLayouts);
            window.Owner = this;
            bool? result = window.ShowDialog();

            if (result == true)
            {
                Layout = window.NewLayout;
                AllLayouts.Add(Layout);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = dialogResult;
            Close();
        }

        private void DeleteLayout_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StoreLayout layout = button?.DataContext as StoreLayout;

            if (layout != null)
            {
                // Prevent deleting layouts that are still used by existing grocery lists
                foreach (GroceryList list in AllLists)
                {
                    if (list.Layout.Id == layout.Id)
                    {
                        MessageBox.Show("This layout is currently used by one or more lists and cannot be deleted.");
                        return;
                    }
                }
                MessageBoxResult result = MessageBox.Show(
                $"Are you sure you want to delete this layout \"{layout.Name}\"?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning 
                );
                if (result == MessageBoxResult.Yes)
                {
                    // Delete layout from database and UI collection after confirmation
                    DatabaseHelper.DeleteLayout(layout);
                    AllLayouts.Remove(layout);
                }
            }
        }

        // Open layout editor for the selected layout
        private void EditLayout_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            StoreLayout layout = button?.DataContext as StoreLayout;
            CreateLayout window = new CreateLayout(AllLayouts, layout);
            bool? result = window.ShowDialog();
        }
    }
}
