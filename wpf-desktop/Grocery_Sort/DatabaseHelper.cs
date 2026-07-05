using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Controls;
//using System.IO;

namespace Grocery_Sort
{
    public static class DatabaseHelper
    {
        // SQLite database file used for persistent local storage
        private static string connectionString = "Data Source=grocerysort.db";

        /* ===== Enable foreign keys ===== */
        public static void EnablePragma(SqliteConnection connection)
        {
            // Enables SQLite foreign key constraints for relational integrity
            var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();
        }

        /* ===== Create Tables ===== */
        public static void CreateTables()
        {
            // Create all required database tables if they do not already exist
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                var command = connection.CreateCommand();

                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS StoreLayouts (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS LayoutCategories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StoreLayoutId INTEGER references StoreLayouts(Id),
                    Name TEXT,
                    OrderIndex INTEGER
                );

                CREATE TABLE IF NOT EXISTS GroceryLists (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT,
                    StoreLayoutId INTEGER references StoreLayouts(Id)
                );

                CREATE TABLE IF NOT EXISTS GroceryItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    GroceryListId INTEGER references GroceryLists(Id),
                    Name TEXT,
                    ItemCategory TEXT,
                    IsChecked INTEGER,
                    Amount TEXT,
                    IsFavorite INTEGER,
                    FavId INTEGER
                );

                CREATE TABLE IF NOT EXISTS FavoriteItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StoreLayoutId INTEGER references StoreLayouts(Id),
                    Name TEXT,
                    ItemCategory TEXT,
                    Amount TEXT
                );
                ";

                command.ExecuteNonQuery();
            }
        }

        /* ===== Load Tables ===== */
        public static ObservableCollection<StoreLayout> LoadLayouts()
        {
            ObservableCollection<StoreLayout> layouts = new ObservableCollection<StoreLayout>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                var command = connection.CreateCommand();
                command.CommandText = "Select Id, Name FROM StoreLayouts;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StoreLayout layout = new StoreLayout();

                        layout.Id = reader.GetInt32(0);
                        layout.Name = reader.GetString(1);
                        
                        // Load related categories and favorites for each layout
                        layout.Categories = LoadCategoriesForLayout(layout.Id);
                        layout.Favorites = LoadFavoritesForLayout(layout.Id);

                        layouts.Add(layout);
                    }
                }
            }
            return layouts;
        }

        private static ObservableCollection<string> LoadCategoriesForLayout(int layoutId)
        {
            ObservableCollection<string> categories = new ObservableCollection<string>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                var command = connection.CreateCommand();
                // Preserve the original category order defined by the user
                command.CommandText = @"
                    SELECT Name
                    FROM LayoutCategories
                    WHERE StoreLayoutId = $layoutId
                    ORDER BY OrderIndex;
                ";

                command.Parameters.AddWithValue("$layoutId", layoutId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(reader.GetString(0));
                    }
                }
            }

            return categories;
        }

        public static ObservableCollection<GroceryList> LoadLists()
        {
            ObservableCollection<GroceryList> lists = new ObservableCollection<GroceryList>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                var command = connection.CreateCommand();
                command.CommandText = "Select Id, Name, StoreLayoutId FROM GroceryLists;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GroceryList list = new GroceryList();

                        list.Id = reader.GetInt32(0);
                        list.Name = reader.GetString(1);
                        int layoutId = reader.GetInt32(2);

                        list.Layout = LoadLayoutById(layoutId);
                        list.Items = LoadItemsForList(list.Id);

                        lists.Add(list);
                    }
                }
            }
            return lists;
        }

        public static StoreLayout LoadLayoutById(int layoutId)
        {
            StoreLayout layout = new StoreLayout();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, Name
                    FROM StoreLayouts
                    WHERE Id = $layoutId
                    ;
                ";

                command.Parameters.AddWithValue("$layoutId", layoutId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        layout.Id = reader.GetInt32(0);
                        layout.Name = reader.GetString(1);
                        layout.Categories = LoadCategoriesForLayout(layoutId);
                    }
                }
            }
            return layout;
        }

        public static ObservableCollection<GroceryItem> LoadItemsForList(int listId)
        {
            ObservableCollection<GroceryItem> items = new ObservableCollection<GroceryItem>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, Name, ItemCategory, IsChecked, Amount, IsFavorite, FavId
                    FROM GroceryItems
                    WHERE GroceryListId = $listId 
                    ;
                ";

                command.Parameters.AddWithValue("$listId", listId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GroceryItem item = new GroceryItem();

                        item.Id = reader.GetInt32(0);
                        item.Name = reader.GetString(1);
                        item.ItemCategory = reader.GetString(2);

                        // SQLite stores booleans as integeres (0 = false, 1 = true)
                        item.IsChecked = reader.GetInt32(3) == 1;

                        // Prevent null amount values
                        item.Amount = reader.IsDBNull(4) ? "" : reader.GetString(4);
                        item.IsFavorite = reader.GetInt32(5) == 1;

                        // Handle nullable favorite ID from database
                        item.FavId = reader.IsDBNull(6) ? null : reader.GetInt32(6);

                        items.Add(item);
                    }
                }
            }
            return items;
        }

        public static ObservableCollection<GroceryItem> LoadFavoritesForLayout(int layoutId)
        {
            ObservableCollection<GroceryItem> favorites = new ObservableCollection<GroceryItem>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                EnablePragma(connection);

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Id, Name, ItemCategory, Amount
                    FROM FavoriteItems
                    WHERE StoreLayoutId = $layoutId
                    ;
                ";
                command.Parameters.AddWithValue("$layoutId", layoutId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GroceryItem item = new GroceryItem
                        {
                            Name = reader.GetString(1),
                            ItemCategory = reader.GetString(2),
                            Amount = reader.IsDBNull(3) ? "" : reader.GetString(3), // if amount empty
                            IsFavorite = true,
                            FavId = reader.GetInt32(0)
                        };

                        favorites.Add(item);
                    }
                }
            }
            return favorites;
        }


        /* ===== Save Tables ===== */
        public static void SaveLayout(StoreLayout layout)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO StoreLayouts (Name)
                    VALUES ($layoutName);

                    SELECT last_insert_rowid();
                "; // SQLite function that returns the ID of the last row inserted


                command.Parameters.AddWithValue("$layoutName", layout.Name);

                long newId = (long)command.ExecuteScalar(); // Returns the result of last select, returns object, (long) convert to long, sqlite ids = 64bit
                layout.Id = (int)newId;

                for (int i = 0; i < layout.Categories.Count; i++)
                {
                    var categoryCommand = connection.CreateCommand();
                    categoryCommand.CommandText = @"
                        INSERT INTO LayoutCategories (StoreLayoutId, Name, OrderIndex)
                        VALUES ($layoutId, $categoryName, $orderIndex);
                    ";

                    categoryCommand.Parameters.AddWithValue("$layoutId", layout.Id);
                    categoryCommand.Parameters.AddWithValue("$categoryName", layout.Categories[i]);
                    categoryCommand.Parameters.AddWithValue("$orderIndex", i);

                    categoryCommand.ExecuteNonQuery();
                }
            }
        }

        public static void SaveList(GroceryList list)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO GroceryLists (Name, StoreLayoutId)
                    VALUES ($listName, $layoutId);

                    SELECT last_insert_rowid();
                "; // sqlite function that returns the ID of the last row inserted

                command.Parameters.AddWithValue("$listName", list.Name);
                command.Parameters.AddWithValue("$layoutId", list.Layout.Id);

                long newId = (long)command.ExecuteScalar(); // Returns the result of last select, returns object, (long) convert to long, sqlite ids = 64bit
                list.Id = (int)newId;

                for (int i = 0; i < list.Items.Count; i++)
                {
                    var itemCommand = connection.CreateCommand();
                    itemCommand.CommandText = @"
                        INSERT INTO GroceryItems (GroceryListId, Name, ItemCategory, IsChecked, Amount, IsFavorite, FavId)
                        VALUES ($listId, $itemName, $itemCategory, $checked, $amount, $isfavorite, $favid);
                    ";

                    itemCommand.Parameters.AddWithValue("$listId", list.Id);
                    itemCommand.Parameters.AddWithValue("$itemName", list.Items[i].Name);
                    itemCommand.Parameters.AddWithValue("$itemCategory", list.Items[i].ItemCategory);
                    itemCommand.Parameters.AddWithValue("$checked", list.Items[i].IsChecked ? 1 : 0);
                    itemCommand.Parameters.AddWithValue("$amount", list.Items[i].Amount);
                    itemCommand.Parameters.AddWithValue("$isfavorite", list.Items[i].IsFavorite ? 1 : 0);
                    itemCommand.Parameters.AddWithValue("$favid", list.Items[i].FavId.HasValue ? list.Items[i].FavId.Value : DBNull.Value);

                    itemCommand.ExecuteNonQuery();
                }
            }
        }

        public static int SaveFavoriteItem(int layoutId, GroceryItem item)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                EnablePragma(connection);

                var favorite = connection.CreateCommand();
                favorite.CommandText = @"
                    INSERT INTO FavoriteItems
                    (StoreLayoutId, Name, ItemCategory, Amount)
                    VALUES
                    ($layoutId, $name, $category, $amount);


                    SELECT last_insert_rowid();
                ";

                favorite.Parameters.AddWithValue("$layoutId", layoutId);
                favorite.Parameters.AddWithValue("$name", item.Name);
                favorite.Parameters.AddWithValue("$category", item.ItemCategory);
                favorite.Parameters.AddWithValue("$amount", item.Amount);

                long id = (long)favorite.ExecuteScalar();
                item.Id = (int)id;
            }

            return item.Id;
        }

        /* ===== Delete Tables ===== */
        public static void DeleteLayout(StoreLayout layout)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                // Ensures all database operations succeed together or fail together
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete related favorite items before deleting the layout itself
                        var deleteFavorites = connection.CreateCommand();
                        deleteFavorites.Transaction = transaction;
                        deleteFavorites.CommandText = @"
                            DELETE FROM FavoriteItems
                            WHERE StoreLayoutId = $layoutId;
                        ";

                        deleteFavorites.Parameters.AddWithValue("$layoutId", layout.Id);
                        deleteFavorites.ExecuteNonQuery();

                        var deleteCategories = connection.CreateCommand();
                        deleteCategories.Transaction = transaction;
                        deleteCategories.CommandText = @"
                            DELETE FROM LayoutCategories
                            WHERE StoreLayoutId = $layoutId;
                        ";

                        deleteCategories.Parameters.AddWithValue("$layoutId", layout.Id);
                        deleteCategories.ExecuteNonQuery();

                        var deleteLayout = connection.CreateCommand();
                        deleteLayout.Transaction = transaction;
                        deleteLayout.CommandText = @"
                            DELETE FROM StoreLayouts
                            WHERE Id = $layoutId
                            ;
                        ";

                        deleteLayout.Parameters.AddWithValue("$layoutId", layout.Id);
                        deleteLayout.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw; // Re-throw error after rollback and pass exception upward
                    }
                }
            }
        }

        public static void DeleteList(GroceryList list)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var deleteItems = connection.CreateCommand();
                        deleteItems.Transaction = transaction;
                        deleteItems.CommandText = @"
                            DELETE FROM GroceryItems
                            WHERE GroceryListId = $listId
                            ;
                        ";
                        deleteItems.Parameters.AddWithValue("$listId", list.Id);
                        deleteItems.ExecuteNonQuery();

                        var deleteList = connection.CreateCommand();
                        deleteList.Transaction = transaction;
                        deleteList.CommandText = @"
                            DELETE FROM GroceryLists
                            WHERE Id = $listId;
                        ";

                        deleteList.Parameters.AddWithValue("$listId", list.Id);
                        deleteList.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw; // Re-throw error after rollback and pass exception upward
                    }
                }
            }
        }

        public static void DeleteFavoriteItem(int favId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                EnablePragma(connection);

                var deletefav = connection.CreateCommand();
                deletefav.CommandText = @"
                    DELETE FROM FavoriteItems
                    WHERE Id = $favId
                    ;
                ";
                deletefav.Parameters.AddWithValue("$favId", favId);
                deletefav.ExecuteNonQuery();
            }
        }

        /* ===== Update Tables ===== */
        public static void UpdateLayout(StoreLayout layout)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var updateLayout = connection.CreateCommand();
                        updateLayout.Transaction = transaction;
                        updateLayout.CommandText = @"
                            UPDATE StoreLayouts
                            SET Name = $layoutName
                            WHERE Id = $layoutId
                            ;
                        ";
                        updateLayout.Parameters.AddWithValue("$layoutId", layout.Id);
                        updateLayout.Parameters.AddWithValue("$layoutName", layout.Name);
                        updateLayout.ExecuteNonQuery();

                        // Rebuild category order after layout changes
                        var deleteCategories = connection.CreateCommand();
                        deleteCategories.Transaction = transaction;
                        deleteCategories.CommandText = @"
                            DELETE FROM LayoutCategories
                            WHERE StoreLayoutId = $layoutId;
                        ";

                        deleteCategories.Parameters.AddWithValue("$layoutId", layout.Id);
                        deleteCategories.ExecuteNonQuery();

                        for (int i = 0; i < layout.Categories.Count; i++)
                        {
                            var categoryCommand = connection.CreateCommand();
                            categoryCommand.Transaction = transaction;
                            categoryCommand.CommandText = @"
                                INSERT INTO LayoutCategories (StoreLayoutId, Name, OrderIndex)
                                VALUES ($layoutId, $categoryName, $orderIndex);
                            ";

                            categoryCommand.Parameters.AddWithValue("$layoutId", layout.Id);
                            categoryCommand.Parameters.AddWithValue("$categoryName", layout.Categories[i]);
                            categoryCommand.Parameters.AddWithValue("$orderIndex", i);

                            categoryCommand.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw; // Re-throw error after rollback and pass exception upward
                    }
                }
            }
        }

        public static void UpdateList(GroceryList list)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var updateList = connection.CreateCommand();
                        updateList.Transaction = transaction;
                        updateList.CommandText = @"
                            UPDATE GroceryLists
                            SET Name = $listName,
                            StoreLayoutId = $layoutId
                            WHERE Id = $listId
                            ;
                        ";
                        updateList.Parameters.AddWithValue("$listName", list.Name);
                        updateList.Parameters.AddWithValue("$layoutId", list.Layout.Id);
                        updateList.Parameters.AddWithValue("$listId", list.Id);
                        updateList.ExecuteNonQuery();

                        var deleteItems = connection.CreateCommand();
                        deleteItems.Transaction = transaction;
                        deleteItems.CommandText = @"
                            DELETE FROM GroceryItems
                            WHERE GroceryListId = $listId
                            ;
                        ";
                        deleteItems.Parameters.AddWithValue("$listId", list.Id);
                        deleteItems.ExecuteNonQuery();

                        for (int i = 0; i < list.Items.Count; i++)
                        {
                            var itemCommand = connection.CreateCommand();
                            itemCommand.Transaction = transaction;
                            itemCommand.CommandText = @"
                                INSERT INTO GroceryItems (GroceryListId, Name, ItemCategory, IsChecked, Amount, IsFavorite, FavId)
                                VALUES ($listId, $itemName, $itemCategory, $checked, $amount, $isfavorite, $favid);
                            ";

                            itemCommand.Parameters.AddWithValue("$listId", list.Id);
                            itemCommand.Parameters.AddWithValue("$itemName", list.Items[i].Name);
                            itemCommand.Parameters.AddWithValue("$itemCategory", list.Items[i].ItemCategory);
                            itemCommand.Parameters.AddWithValue("$checked", list.Items[i].IsChecked ? 1 : 0);
                            itemCommand.Parameters.AddWithValue("$amount", list.Items[i].Amount);
                            itemCommand.Parameters.AddWithValue("$isfavorite", list.Items[i].IsFavorite ? 1 : 0);
                            itemCommand.Parameters.AddWithValue("$favid", list.Items[i].FavId.HasValue ? list.Items[i].FavId.Value : DBNull.Value);

                            itemCommand.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw; // Re-throw error after rollback and pass exception upward
                    }
                }
            }
        }

        public static void UpdateItemChecked(GroceryItem item)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                EnablePragma(connection);

                var updateItem = connection.CreateCommand();
                updateItem.CommandText = @"
                    UPDATE GroceryItems
                    SET IsChecked = $checked
                    WHERE Id = $itemId
                    ;
                ";
                updateItem.Parameters.AddWithValue("$checked", item.IsChecked ? 1 : 0);
                updateItem.Parameters.AddWithValue("$itemId", item.Id);
                updateItem.ExecuteNonQuery();
            }
        }
    }
}
