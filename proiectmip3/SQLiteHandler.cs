using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace proiectmip3
{
    public class SQLiteHandler
    {
        private SQLiteConnection conn;

        // true if the connection is open
        public bool IsConnected
        {
            get { return conn != null && conn.State == System.Data.ConnectionState.Open; }
        }

        public void ConnectToDb()
        {
            // already connected, nothing to do
            if (IsConnected) return;

            // the db file gets created automatically if it doesnt exist
            conn = new SQLiteConnection("Data Source=keywords.db;Version=3;");
            conn.Open();

            // create the table only if its not there yet
            string sql = "CREATE TABLE IF NOT EXISTS Keywords " +
                         "(id INTEGER PRIMARY KEY AUTOINCREMENT, keyword TEXT)";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        public void DisconnectFromDb()
        {
            if (IsConnected)
                conn.Close();
        }

        public void InsertKeyword(string k)
        {
            // @k is a parameter, safer than putting the text in the sql directly
            string sql = "INSERT INTO Keywords (keyword) VALUES (@k)";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@k", k);
            cmd.ExecuteNonQuery();
        }

        public List<string> GetAllKeywords()
        {
            List<string> list = new List<string>();

            string sql = "SELECT keyword FROM Keywords";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();

            // read row by row
            while (reader.Read())
                list.Add(reader.GetString(0));

            reader.Close();
            return list;
        }
    }
}
