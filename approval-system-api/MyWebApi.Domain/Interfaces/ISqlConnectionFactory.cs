using System.Data;

namespace MyWebApi.Domain.Interfaces
{
    public interface ISqlConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
