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
        //DB 연결용 함수
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
        //쿼리문 설정용 함수
        public void setQuery(string str)
        {
            query = str;
        }

        //숫자를 반환하는 함수
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
        //회원가입용 insertUser 함수
        public int insertUser()
        {
            try {
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
        //로그인 정보 반환용 selectUser 함수
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
        //아이 찾기용 selectUser 함수
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
        //비밀번호 변경용 updateuser 함수
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