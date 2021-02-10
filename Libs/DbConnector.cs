using System;
using System.Data;
using Docusign_Connect.Constants;
using Docusign_Connect.DatabaseConnectors;
using Docusign_Connect.DTO;

namespace Docusign_Connect.Libs
{
    public class DbConnector
    {
        public static IDatabase database;
        public static string GetDataFromPostgres(Tab selectedTab, string providerId, string identifierValue)
        {
            try
            {
                var query = "Select " + selectedTab.SourceDataField.Trim().ToLower()
                 + " From " + selectedTab.Table.ToLower() +
                  " Where " + providerId + "='" + identifierValue + "'";
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                database.Connection.Close();
                return "";
            }
        }
    }
}