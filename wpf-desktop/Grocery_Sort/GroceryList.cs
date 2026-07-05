using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;

namespace Grocery_Sort
{
    // Represents a grocery list with a selected store layout and its items
    public class GroceryList : ObservableObject
    {
        private int id;
        private string name;
        private StoreLayout layout = new StoreLayout();
        // Collection automatically updates the UI when items are added or removed
        private ObservableCollection<GroceryItem> items = new ObservableCollection<GroceryItem>();

        public int Id
        {
            get { return id; }
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public StoreLayout Layout
        {
            get { return layout; }
            set
            {
                layout = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<GroceryItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                OnPropertyChanged();
            }
        }
    }
}
