using System.Data;
using Microsoft.Data.SqlClient;
using MyWebApi.Domain.Interfaces;

namespace MyWebApi.Infrastructure.Repositories
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "連線字串 (Connection string) 不能為空。");
            }

            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
