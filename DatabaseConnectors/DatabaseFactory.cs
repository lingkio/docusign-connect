using Docusign_Connect.Constants;
using Docusign_Connect.DTO;

namespace Docusign_Connect.DatabaseConnectors
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