using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazor.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Blazor.Services
{
    public partial class DBService
    {
        private readonly string _connectionString;

        public DBService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Setup User Table
        public async Task<string> SetupUserTable()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (
                    var command = new NpgsqlCommand(
                        @"CREATE TABLE IF NOT EXISTS users (
                            id SERIAL PRIMARY KEY, 
                            name VARCHAR(255), 
                            email VARCHAR(255), 
                            password VARCHAR(255))",
                        connection
                    )
                )
                {
                    await command.ExecuteNonQueryAsync();
                }
            }
            return "User table created successfully";
        }

        // Get All Users
        public async Task<List<Users>> GetAllUsers()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand("SELECT * FROM users", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    var users = new List<Users>();
                    while (await reader.ReadAsync())
                    {
                        users.Add(
                            new Users
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Name = reader.GetString(reader.GetOrdinal("name")),
                                Email = reader.GetString(reader.GetOrdinal("email")),
                                Password = reader.GetString(reader.GetOrdinal("password"))
                            }
                        );
                    }
                    return users;
                }
            }
        }
    }
}
