using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Grocery_Sort
{
    // Represents a customizable store layout with category orer and favorite items
    public class StoreLayout : ObservableObject
    {        
        private int id;
        private string name = string.Empty;
        private ObservableCollection<string> categories = new ObservableCollection<string>();
        private ObservableCollection<GroceryItem> favorites = new ObservableCollection<GroceryItem>();

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

        // Category order determines how grocery items are sorted in lists
        public ObservableCollection<string> Categories
        {
            get { return categories; }
            set
            {
                categories = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<GroceryItem> Favorites
        {
            get { return favorites; }
            set
            {
                favorites = value;
                OnPropertyChanged();
            }
        }
    }
}
