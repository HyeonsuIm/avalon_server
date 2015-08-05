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
                Console.WriteLine("DB connection success");
            }catch(MySqlException)
            {
                Console.WriteLine("DB connection error");
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
                Console.WriteLine("{0}", result);
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
                query = "insert into avalondb.user(U_Id,U_Pw,U_Nick,U_Email) values('" + argumentList[0] + "','"+Encoding.UTF8.GetBytes(argumentList[1])+"'";
                for (int i = 2; i < argumentCount; i++) {
                    query += ",'" + argumentList[i] + "'";
                }
                query += ")";
            
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("insert fail, argument count error");
                return 1;
            }
            catch (MySqlException)
            {
                Console.WriteLine("insert fail");
                return 2;
            }
            return 0;
        }

        public string selectUser(string id, string pwd)
        {
            string result;
            query = "select U_Id from avalondb.user where id='" + id + "','" + pwd + "'" ;
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

            ds.Dispose();
            da.Dispose();
            return result;
        }

        public string selectUser(string email) {
            string result;
            query = "select U_Id from avalondb.user where U_Email='" + email + "'";
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

            ds.Dispose();
            da.Dispose();
            
            return result;

        }

    }
}