using Grocery.Core.Data;
using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        private readonly List<GroceryListItem> groceryListItems = [];

        public GroceryListItemsRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS GroceryListItem (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [GroceryListId] INTEGER NOT NULL,
                            [ProductId] INTEGER NOT NULL,
                            [Amount] INTEGER NOT NULL)");

            List<string> insertQueries =
            [
                @"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, 1, 3)",
                @"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, 2, 1)",
                @"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, 3, 4)",
                @"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(2, 1, 2)",
                @"INSERT OR IGNORE INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(2, 2, 5)"
            ];

            InsertMultipleWithTransaction(insertQueries);
            GetAll();
        }

        public List<GroceryListItem> GetAll()
        {
            groceryListItems.Clear();
            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem";

            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);

                    groceryListItems.Add(new GroceryListItem(id, groceryListId, productId, amount));
                }
            }
            CloseConnection();

            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int id)
        {
            return GetAll().Where(i => i.GroceryListId == id).ToList();
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            string insertQuery = @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount)
                                   VALUES(@GroceryListId, @ProductId, @Amount) Returning RowId;";
            OpenConnection();
            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("ProductId", item.ProductId);
                command.Parameters.AddWithValue("Amount", item.Amount);

                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            CloseConnection();
            return item;
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            string deleteQuery = $"DELETE FROM GroceryListItem WHERE Id = {item.Id};";
            OpenConnection();
            Connection.ExecuteNonQuery(deleteQuery);
            CloseConnection();
            return item;
        }

        public GroceryListItem? Get(int id)
        {
            string selectQuery = $"SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE Id = {id}";
            GroceryListItem? item = null;
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int Id = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);

                    item = new GroceryListItem(Id, groceryListId, productId, amount);
                }
            }
            CloseConnection();
            return item;
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            string updateQuery = @"UPDATE GroceryListItem
                                   SET GroceryListId = @GroceryListId,
                                       ProductId = @ProductId,
                                       Amount = @Amount
                                   WHERE Id = @Id;";

            OpenConnection();
            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("ProductId", item.ProductId);
                command.Parameters.AddWithValue("Amount", item.Amount);
                command.Parameters.AddWithValue("Id", item.Id);

                command.ExecuteNonQuery();
            }
            CloseConnection();

            return item;
        }
    }
}
