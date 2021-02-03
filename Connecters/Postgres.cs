using System;
using System.Data;
using System.Data.Common;
using Lingk_SAML_Example.DTO;
using Newtonsoft.Json;
using Npgsql;

namespace Lingk_SAML_Example.Connecters
{
    public class Postgres
    {
        private NpgsqlConnection con;
        public NpgsqlConnection Connect(Provider config)
        {
            var connectionString = "Host=" + config.Server + ";Username=" + config.UserName + ";Password=" +
             config.Password + ";Database=" + config.Database;
            return new NpgsqlConnection(connectionString);
        }

        public string ExecuteQuery(Provider config, string query)
        {
            if (con == null)
            {
                con = Connect(config);
            }
            con.Open();

            using var cmd = new NpgsqlCommand(query, con);

            return cmd.ExecuteScalar().ToString();
        }
    }

}