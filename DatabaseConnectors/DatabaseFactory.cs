using Lingk_SAML_Example.Constants;
using Lingk_SAML_Example.DTO;

namespace Lingk_SAML_Example.DatabaseConnectors
{
    public static class DatabaseFactory
    {
        public static Provider config;
        public static IDatabase CreateDatabase(DBType type)
        {
            switch (type)
            {
                case DBType.Postgres:
                    return new PostgresDB();

            }

            return null;
        }
    }

}