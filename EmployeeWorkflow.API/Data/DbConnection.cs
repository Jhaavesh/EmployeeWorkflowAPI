using MySql.Data.MySqlClient;

namespace EmployeeWorkflow.API.Data
{
    public class DbConnection
    {
        private readonly string _connectionString;

        public DbConnection(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found");
        }

        public MySqlConnection GetConnection()
        {
            try
            {
                return new MySqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create database connection: {ex.Message}", ex);
            }
        }
    }
}