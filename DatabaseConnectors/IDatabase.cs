using System.Data;
namespace Docusign_Connect.DatabaseConnectors
{
    public interface IDatabase
    {
        IDbCommand Command
        {
            get;
        }

        IDbConnection Connection
        {
            get;
        }
    }
}