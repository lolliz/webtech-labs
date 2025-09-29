using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace Lab_1_2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Строка подключения к базе данных
            string connectionString = "Server=localhost;Database=car_dealership;Uid=root;Pwd=1310ereL!;";

            // 1. Создать подключение к базе данных
            using (var connection = new MySqlConnection(connectionString))
            {
                // Весь код для работы с БД
                try
                {
                    connection.Open();
                    Console.WriteLine("Connection to MySQL database successful!");

                    // 2. Считать и десериализовать JSON-файл с автомобилями
                    string json = File.ReadAllText("cars.json");
                    dynamic data = JsonConvert.DeserializeObject(json);
                    var cars = data.cars.ToObject<List<Car>>();

                    // 3. Добавить каждый автомобиль в таблицу
                    foreach (var car in cars)
                    {
                        var sql = "INSERT INTO cars (firm, model, year, power, color, price) VALUES (@firm, @model, @year, @power, @color, @price)";
                        using (var command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@firm", car.Firm);
                            command.Parameters.AddWithValue("@model", car.Model);
                            command.Parameters.AddWithValue("@year", car.Year);
                            command.Parameters.AddWithValue("@power", car.Power);
                            command.Parameters.AddWithValue("@color", car.Color);
                            command.Parameters.AddWithValue("@price", car.Price);
                            command.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine("Data from cars.json successfully loaded into the database.");

                    // Загрузка данных дилеров
                    string dealersJson = File.ReadAllText("dealers.json");
                    var dealers = JsonConvert.DeserializeObject<List<Dealer>>(dealersJson);

                    foreach (var dealer in dealers)
                    {
                        var sql = "INSERT INTO dealers (name, city, address, area, rating) VALUES (@name, @city, @address, @area, @rating)";
                        using (var command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@name", dealer.Name);
                            command.Parameters.AddWithValue("@city", dealer.City);
                            command.Parameters.AddWithValue("@address", dealer.Address);
                            command.Parameters.AddWithValue("@area", dealer.Area);
                            command.Parameters.AddWithValue("@rating", dealer.Rating);
                            command.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine("Data from dealers.json successfully loaded into the database.");

                    // 4. Связывание автомобилей с дилерами
                    var dealerIds = new List<int>();
                    using (var cmd = new MySqlCommand("SELECT ID FROM dealers;", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dealerIds.Add(reader.GetInt32(0));
                        }
                    }

                    var carIds = new List<int>();
                    using (var cmd = new MySqlCommand("SELECT ID FROM cars;", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            carIds.Add(reader.GetInt32(0));
                        }
                    }

                    var random = new Random();
                    foreach (var carId in carIds)
                    {
                        int randomDealerId = dealerIds[random.Next(dealerIds.Count)];
                        var sql = "UPDATE cars SET DealerID = @dealerId WHERE ID = @carId";
                        using (var command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@dealerId", randomDealerId);
                            command.Parameters.AddWithValue("@carId", carId);
                            command.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine("Cars have been successfully assigned to dealers.");
                }
                catch (Exception ex)
                {
                    // Один общий catch для всех возможных ошибок
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
        }
    }

    // Классы Car и Dealer
    public class Car
    {
        public string Firm { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int Power { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
    }

    public class Dealer
    {
        public string Name { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Area { get; set; }
        public decimal Rating { get; set; }
    }
}