using System.Data;
using Docusign_Connect.DTO;
using Docusign_Connect.Libs;
using Npgsql;
using System.Linq;
using Docusign_Connect.Constants;

namespace Docusign_Connect.DatabaseConnectors
{
    internal class PostgresDB : IDatabase
    {
        private NpgsqlConnection _Connection = null;
        private NpgsqlCommand _Command = null;
        private string connectionString;
        public PostgresDB()
        {
            Provider config = LingkYaml.LingkYamlConfig.Providers.Where((prov) =>
            prov.Name.ToLower() == DBType.Postgres.ToString().ToLower()).FirstOrDefault();
            connectionString = "Host = " + config.Server + "; Username = " + config.UserName
            + "; Password = " + config.Password + ";Database=" + config.Database;
        }
        public IDbCommand Command
        {
            get
            {
                if (_Command == null)
                {
                    _Command = new NpgsqlCommand();
                    _Command.CommandType = CommandType.Text;
                    _Command.Connection = (NpgsqlConnection)Connection;
                }
                return _Command;
            }
        }

        public IDbConnection Connection
        {
            get
            {
                if (_Connection == null)
                {
                    _Connection = new NpgsqlConnection(connectionString);
                }
                return _Connection;
            }
        }
    }
}