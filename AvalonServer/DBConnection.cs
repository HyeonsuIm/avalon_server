using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySql.Data;
using System.Data;

namespace AvalonServer
{
    class DBConnection
    {
        MySqlConnection conn;
        MySqlDataAdapter da;
        DataSet ds;
        string query;
        string IP = "203.255.3.72";
        string ID = "avalon";
        string PW = "AvalonPw";
        string database = "avalon";

        public void connect()
        {
            try {
                conn = new MySqlConnection(@"server="+IP+";userid="+ID+";password="+PW+";database="+database+";");
                conn.Open();
                Console.WriteLine("DB connection success\n");
            }catch(MySqlException)
            {
                Console.WriteLine("error");
                Console.WriteLine("DB connection fail\n");
                Environment.Exit(0);
            }
        }

        void disConnect()
        {
            conn.Close();
        }

        public void setQuery(string str)
        {
            query = str;
        }

        public string executeQuery(string idpw) {

            string result;

            connect();

            da = new MySqlDataAdapter(query, conn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables.Count != 0)
            {
                result = ds.Tables[0].Rows[0][idpw].ToString();
            }
            else
            {
                result = "";
            }
            return result;
        }

        public string executeNonQuery()
        {

            string result;
            try
            {
                da = new MySqlDataAdapter(query, conn);
                ds = new DataSet();
                da.Fill(ds);

                if (ds.Tables[0].Rows.Count != 0)
                    result = "1";
                else
                    result = "0";
            }
            catch
            {
                result = "9";
            }
            finally{
                da.Dispose();
                ds.Dispose();
            }
            return result;
        }
        public int insertUser(string[] argumentList, int argumentCount)
        {
            try {
                query = "insert into user(U_Id,U_Pw,U_Nick,U_Mail) values('" + argumentList[0] + "'";
                for (int i = 1; i < argumentCount; i++) {
                    query += ",'" + argumentList[i] + "'";
                }
                query += ")";
            
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("<error>");
                Console.WriteLine("insert fail, argument count incorrect\n");
                return 1;
            }
            catch (MySqlException e)
            {
                Console.WriteLine("<error>");
                Console.WriteLine(e.Message);
                Console.WriteLine("insert fail\n");
                return 2;
            }
            return 0;
        }

        public void selectUser(string id, string pwd, out string result, out string nick)
        {
            query = "select U_Index,U_Nick from user where U_Id='"+id+"' and U_Pw='" + pwd+"'";
            da = new MySqlDataAdapter(query, conn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                result = dr["U_Index"].ToString();
                nick = (string)dr["U_Nick"];
            }
            else
            {
                result = "";
                nick = "";
            }
        }

        public string selectUser(string id, string email)
        {
            string result;
            query = "select U_Index from user where U_Id='" + id + "' and U_mail='" + email + "'";
            da = new MySqlDataAdapter(query, conn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                result = "1";
            }
            else
                result = "0";

            return result;
        }

        public string selectUser(string email) {
            string result;
            query = "select U_Id from user where U_Mail='" + email + "'";
            da = new MySqlDataAdapter(query, conn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                result = (string)dr["U_Id"];
            }
            else
            {
                result = "";
            }
            
            return result;

        }

        public void updateUser(string id, string password)
        {
            try
            {
                query = "update user set U_pw='" + password + "' where U_id='" + id + "'";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Console.WriteLine("<error>");
                Console.WriteLine(e.Message);
                Console.WriteLine("update fail\n");
            }
        }
    }
}