using System.Data;
namespace Lingk_SAML_Example.DatabaseConnectors
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