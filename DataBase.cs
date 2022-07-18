using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    class DataBase
    {
        public static MySqlConnectionStringBuilder connection;

        public static string sqlcmd;
        public static DataTable dt_clients;
        public void strcon()
        {
            connection = new MySqlConnectionStringBuilder()
            {
                Server = "a0697344.xsph.ru", //"sql11.freesqldatabase.com", //"a0686088.xsph.ru",
                Port = 3306,
                Database = "a0697344_test", //"sql11507065", //"a0697344_test",
                UserID = "a0697344", //"sql11507065", //"a0686088",
                Password = "azwaehitsa", //"SHa2WYRQ7n", //"dipabeutuw"
            };
        }

        // Соединение с БД:
        public DataTable Connect(string selectSQL)
        {
            DataTable dt = new DataTable();

            try
            {
                MySqlConnection conn = new MySqlConnection(connection.ConnectionString);
                conn.Open();
                MySqlCommand sqlCommand = new MySqlCommand(sqlcmd, conn);
                MySqlDataAdapter da = new MySqlDataAdapter(sqlCommand);
                da.Fill(dt);
                conn.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка подключения!");
            }

            return dt;
        }
    }

}
