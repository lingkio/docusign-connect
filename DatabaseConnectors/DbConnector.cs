using System.Data;
using Lingk_SAML_Example.Constants;
using Lingk_SAML_Example.DTO;

namespace Lingk_SAML_Example.DatabaseConnectors
{
    public class DbConnector
    {
        public static IDatabase database;
        public static string GetDataFromPostgres(Tab selectedTab, string providerId, string identifierValue)
        {
            var query = "Select " + selectedTab.SourceDataField.ToLower()
             + " From " + selectedTab.Table.ToLower() +
              " Where " + providerId + "='" + identifierValue.Split("|")[1] + "'";
            if (database == null)
            {
                database = DatabaseFactory.CreateDatabase(DBType.Postgres);
            }

            IDbCommand command = database.Command;
            command.CommandText = query;

            database.Connection.Open();
            var result = command.ExecuteScalar().ToString();
            database.Connection.Close();
            return result;
        }
    }
}