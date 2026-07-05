using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Runtime.CompilerServices;

namespace Grocery_Sort
{
    /// <summary>
    /// Interaktionslogik für ViewLists.xaml
    /// </summary>
    public partial class ViewLists : Window, INotifyPropertyChanged
    {
        /* =====   Private fields  ===== */
        private GroceryList selectedList;
        private bool? result = false;

        /* ===== Public properties ===== */
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<GroceryList> GroceryLists { get; set; }
        
        // List selected to be shown on the main window
        public GroceryList SelectedList
        {
            get { return selectedList; }
            set
            {
                selectedList = value;
                OnPropertyChanged();
            }
        }        

        public ObservableCollection<StoreLayout> Layouts { get; set; }
        public bool deleted = false;

        public ViewLists(ObservableCollection<GroceryList> AllLists, ObservableCollection<StoreLayout> AllLayouts)
        {
            InitializeComponent();
            DataContext = this;
            GroceryLists = AllLists;
            Layouts = AllLayouts;
        }

        // protected = this class and inherited classes can use it
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DeleteList_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            GroceryList list = button?.DataContext as GroceryList;

            if (list != null)
            {
                MessageBoxResult Msgresult = MessageBox.Show(
                    $"Are you sure you want to delete this list {list.Name}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                    );
                if (Msgresult == MessageBoxResult.Yes)
                {

                    // Delete list from databse and shared collection after confirmation
                    DatabaseHelper.DeleteList(list);
                    GroceryLists.Remove(list);

                    // Remember if the currently selected list was deleted
                    if (list == SelectedList)
                    {
                        deleted = true;
                    }
                }
            }
        }

        // Set selected list as the main list when returning to MainWindow
        private void SelectList_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            GroceryList selected = button?.DataContext as GroceryList;

            if (selected != null)
            {
                SelectedList = selected;
                result = true;
                MessageBox.Show($"{selected.Name} selected as main list.");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = result;
            Close();
        }

        // Open selected list in edit mode
        private void EditList_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            GroceryList list = button?.DataContext as GroceryList;
            CreateList window = new CreateList(GroceryLists, Layouts, list);
            bool? result = window.ShowDialog();
        }
    }
}
