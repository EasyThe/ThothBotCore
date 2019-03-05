using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using ThothBotCore.Storage.Models;

namespace ThothBotCore.Storage
{
    public class Database
    {
        private const string DbPath = "Data Source=.\\Storage\\ThothDb.db;Version=3;";

        public static List<Gods.God> LoadGod(string godname) // Get god by godname
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Gods.God>($"SELECT * FROM Gods WHERE Name LIKE '%{godname}%'", new DynamicParameters());
                return output.ToList();
            }
        }

        public static List<Gods.God> GetRandomGod() // Get random god for rgod command
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Gods.God>($"SELECT Name, godIcon_URL, Pantheon, Roles, Title, Type, DomColor FROM Gods", new DynamicParameters());
                return output.ToList();
            }
        }

        public static List<Gods.God> LoadGodsDomColor()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<Gods.God>($"SELECT id, godIcon_URL, DomColor FROM Gods", new DynamicParameters());
                return output.ToList();
            }
        }

        public static void SaveGods(Gods.God gods)
        {
            // TO DO....

            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("insert into Gods ", gods);
            }
        }

        public static void SaveDomColor(int id, int color)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute($"UPDATE Gods SET DomColor = {color} WHERE id = '{id}'");
            }
        }

        private static string LoadConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
    }
}
