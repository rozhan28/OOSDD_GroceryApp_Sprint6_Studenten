using Grocery.Core.Data;
using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        private readonly List<Product> products = new();

        public ProductRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS Product (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [Name] NVARCHAR(120) NOT NULL,
                            [Stock] INTEGER NOT NULL,
                            [ShelfLife] DATE,
                            [Price] REAL NOT NULL)");

            List<string> insertQueries = new()
            {
                @"INSERT OR IGNORE INTO Product(Id, Name, Stock, ShelfLife, Price) VALUES(1, 'Melk', 300, '2025-09-25', 0.95)",
                @"INSERT OR IGNORE INTO Product(Id, Name, Stock, ShelfLife, Price) VALUES(2, 'Kaas', 100, '2025-09-30', 7.98)",
                @"INSERT OR IGNORE INTO Product(Id, Name, Stock, ShelfLife, Price) VALUES(3, 'Brood', 400, '2025-09-12', 2.19)",
                @"INSERT OR IGNORE INTO Product(Id, Name, Stock, ShelfLife, Price) VALUES(4, 'Cornflakes', 0, '2025-12-31', 1.48)"
            };

            InsertMultipleWithTransaction(insertQueries);
            GetAll();
        }

        public List<Product> GetAll()
        {
            products.Clear();
            string selectQuery = "SELECT Id, Name, Stock, date(ShelfLife), Price FROM Product";

            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);

                    DateOnly shelfLife = default;
                    if (!reader.IsDBNull(3))
                    {
                        DateTime dt = reader.GetDateTime(3);
                        shelfLife = DateOnly.FromDateTime(dt);
                    }

                    decimal price = Convert.ToDecimal(reader.GetDouble(4)); 
                    products.Add(new Product(id, name, stock, shelfLife, price));
                }
            }
            CloseConnection();

            return products;
        }

        public Product Add(Product item)
        {
            string insertQuery = @"INSERT INTO Product(Name, Stock, ShelfLife, Price)
                                   VALUES(@Name, @Stock, @ShelfLife, @Price) Returning RowId;";

            OpenConnection();
            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("Name", item.Name ?? string.Empty);
                command.Parameters.AddWithValue("Stock", item.Stock);
                if (item.ShelfLife != default)
                    command.Parameters.AddWithValue("ShelfLife", item.ShelfLife.ToDateTime(new TimeOnly(0)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                else
                    command.Parameters.AddWithValue("ShelfLife", DBNull.Value);

                command.Parameters.AddWithValue("Price", Convert.ToDouble(item.Price));

                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            CloseConnection();

            products.Add(item);
            return item;
        }

        public Product? Delete(Product item)
        {
            string deleteQuery = $"DELETE FROM Product WHERE Id = {item.Id};";
            OpenConnection();
            Connection.ExecuteNonQuery(deleteQuery);
            CloseConnection();

            var existing = products.FirstOrDefault(p => p.Id == item.Id);
            if (existing != null) products.Remove(existing);

            return item;
        }

        public Product? Get(int id)
        {
            string selectQuery = $"SELECT Id, Name, Stock, date(ShelfLife), Price FROM Product WHERE Id = {id}";
            Product? product = null;
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int Id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly shelfLife = default;
                    if (!reader.IsDBNull(3))
                    {
                        DateTime dt = reader.GetDateTime(3);
                        shelfLife = DateOnly.FromDateTime(dt);
                    }
                    decimal price = Convert.ToDecimal(reader.GetDouble(4));
                    product = new Product(Id, name, stock, shelfLife, price);
                }
            }
            CloseConnection();
            return product;
        }

        public Product? Update(Product item)
        {
            string updateQuery = @"UPDATE Product
                                   SET Name = @Name,
                                       Stock = @Stock,
                                       ShelfLife = @ShelfLife,
                                       Price = @Price
                                   WHERE Id = @Id;";
            OpenConnection();
            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("Name", item.Name ?? string.Empty);
                command.Parameters.AddWithValue("Stock", item.Stock);
                if (item.ShelfLife != default)
                    command.Parameters.AddWithValue("ShelfLife", item.ShelfLife.ToDateTime(new TimeOnly(0)).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                else
                    command.Parameters.AddWithValue("ShelfLife", DBNull.Value);
                command.Parameters.AddWithValue("Price", Convert.ToDouble(item.Price));
                command.Parameters.AddWithValue("Id", item.Id);

                command.ExecuteNonQuery();
            }
            CloseConnection();

            var existing = products.FirstOrDefault(p => p.Id == item.Id);
            if (existing != null)
            {
                existing.Name = item.Name;
                existing.Stock = item.Stock;
                existing.ShelfLife = item.ShelfLife;
                existing.Price = item.Price;
            }
            return item;
        }
    }
}