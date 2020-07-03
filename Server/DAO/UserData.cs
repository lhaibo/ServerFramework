using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SocketDemoProtocol;

namespace ServerFramework.DAO
{
    class UserData
    {
        private MySqlConnection sqlConnection;
        public UserData()
        {
            ConnectMysql();
        }

        private void ConnectMysql()
        {
            try
            {
                SqlConnectionStringBuilder sqlConnectionStr = new SqlConnectionStringBuilder();
                //先写死
                sqlConnectionStr.DataSource = "119.23.226.32";
                sqlConnectionStr.InitialCatalog = "Game";
                sqlConnectionStr.UserID = "haibo";
                sqlConnectionStr.Password = "haibo";
                
                this.sqlConnection = new MySqlConnection(sqlConnectionStr.ToString());
                sqlConnection.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("连接数据库失败!"+e.Message);
            }
        }

        public bool Logon(MainPack pack)
        {
            string username = pack.LoginPack.Username;
            string password = pack.LoginPack.Password;

            string insertSql = $"insert into user (username,password) values ('{username}','{password}')";

            
            try
            {
                if (sqlConnection.State != System.Data.ConnectionState.Open) sqlConnection.Open();
                MySqlCommand commd = new MySqlCommand(insertSql, sqlConnection);
                int len=commd.ExecuteNonQuery();
                if (len<1)
                {
                    throw new Exception("该账号已注册,请直接登录");
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;

            }
            finally
            {
                sqlConnection.Close();
                Console.WriteLine("数据库连接已关闭!");
            }        
            
        }

        public bool Login(MainPack pack)
        {
            string username = pack.LoginPack.Username;
            string password = pack.LoginPack.Password;

            string querySql = $"select * from user where username = '{username}' and password = '{password}';";


            try
            {
                if (sqlConnection.State != System.Data.ConnectionState.Open) sqlConnection.Open();
                MySqlCommand commd = new MySqlCommand(querySql, sqlConnection);
                
                MySqlDataReader reader = commd.ExecuteReader();
                
                if (reader.Read())
                {
                    return true;
                }
                throw new Exception("账号或密码错误!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;

            }
            finally
            {
                sqlConnection.Close();
                Console.WriteLine("数据库连接已关闭!");
            }

        }
    }
}
