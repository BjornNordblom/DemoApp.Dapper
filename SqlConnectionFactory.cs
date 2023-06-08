using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
}

internal sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly IConfiguration _configuration;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_configuration.GetConnectionString("Database"));
    }
}
