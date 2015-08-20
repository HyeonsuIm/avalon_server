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

        public void insertUser(int userIndex)
        {
            query = "insert into winlate(U_index) values(" + userIndex + ")";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.ExecuteNonQuery();
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
                result = "0";
            }
            
            return result;

        }

        public int getWinLose(int index, out int win, out int lose, out int draw)
        {
            query = "select W_win,W_lose,W_draw from winlate where U_index=" + index + "";
            da = new MySqlDataAdapter(query, conn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                win = int.Parse(dr["W_win"].ToString());
                lose = int.Parse(dr["W_lose"].ToString());
                draw = int.Parse(dr["W_draw"].ToString());
            }
            else
            {
                win = 0;
                lose = 0;
                draw = 0;
                return 1;
            }
            return 0;
        }

        public int getUserNick(int receiveIndex, out string nick)
        {
            query = "select U_Nick from user where U_index=" + receiveIndex + "";
            da = new MySqlDataAdapter(query, conn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                nick = (string)dr["U_Nick"];
                return 0;
            }
            else
            {
                nick = "";
                return 1;
            }
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