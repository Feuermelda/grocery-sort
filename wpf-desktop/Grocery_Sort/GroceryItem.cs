using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grocery_Sort
{
    // Represents a single grocery item with UI-reactive properties
    public class GroceryItem : ObservableObject
    {
        private int id;
        private string name;
        private string itemCategory;
        private bool isChecked;
        private string amount;
        private bool isFavorite;
        private int? favId; // Nullable favorite reference used for favorite item synchronization
        private bool isSelected;

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

        public string ItemCategory
        {
            get { return itemCategory; }
            set
            {
                itemCategory = value;
                OnPropertyChanged();
            }
        }

        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                OnPropertyChanged();
            }
        }

        public string Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                OnPropertyChanged();
            }
        }

        public bool IsFavorite
        {
            get { return isFavorite; }
            set
            {
                isFavorite = value;
                OnPropertyChanged();
            }
        }

        public int? FavId
        {
            get { return favId; }
            set
            {
                favId = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }
    }
}
