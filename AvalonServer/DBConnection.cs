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
        string IP = "203.255.3.112";
        string ID = "avalon";
        string PW = "AvalonPw";
        string database = "avalon";
        bool executeCheck = false;
        /// <summary>
        /// 서버 DB 접속
        /// </summary>
        public void connect()
        {
            try {
                conn = new MySqlConnection(@"server="+IP+";userid="+ID+";password="+PW+";database="+database+";");
                conn.Open();
                //Console.WriteLine("DB connection success\n");
            }catch(MySqlException)
            {
                Console.WriteLine("error");
                Console.WriteLine("DB connection fail\n");
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// 서버 DB 접속종료
        /// </summary>
        void disConnect()
        {
            conn.Close();
        }

        /// <summary>
        /// 쿼리문 설정
        /// </summary>
        /// <param name="str">쿼리</param>
        public void setQuery(string str)
        {
            query = str;
        }

        /// <summary>
        /// 설정된 쿼리문 수행
        /// </summary>
        /// <returns>쿼리실행 결과</returns>
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

        /// <summary>
        /// 새로운 유저 추가
        /// </summary>
        /// <param name="argumentList">유저 정보</param>
        /// <param name="argumentCount">매개변수 개수</param>
        /// <returns></returns>
        public int insertUser(string[] argumentList, int argumentCount)
        {
            try {
                query = "insert into user(U_Id,U_Pw,U_Nick,U_Mail) values('" + argumentList[0] + "'";
                for (int i = 1; i < argumentCount; i++) {
                    query += ",'" + argumentList[i] + "'";
                }
                query += ")";
            
                MySqlCommand cmd = new MySqlCommand(query, conn);
                execute(cmd);

                query ="select U_index from user where U_id = '"+ argumentList[0]+ "'" ;
                da = new MySqlDataAdapter(query, conn);
                ds = new DataSet();
                da.Fill(ds);
                DataRow dr = ds.Tables[0].Rows[0];
                query = "insert into winlate(U_index) values('" + dr["U_index"].ToString()+"')";
                cmd = new MySqlCommand(query, conn);
                execute(cmd);
                
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

        /// <summary>
        /// 새 유저 추가에 따른 승패 초기화
        /// </summary>
        /// <param name="userIndex">유저 index</param>
        public void insertUser(int userIndex)
        {
            query = "insert into winlate(U_index) values(" + userIndex + ")";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            execute(cmd);
        }

        /// <summary>
        /// 로그인을 위한 유저 정보 검색
        /// </summary>
        /// <param name="id">유저 id</param>
        /// <param name="pwd">유저 pwd</param>
        /// <param name="result">쿼리 수행 결과</param>
        /// <param name="nick">유저 닉네임</param>
        /// <param name="userId">유저 id</param>
        public void selectUser(string id, string pwd, out string result, out string nick, out string userId)
        {
            query = "select U_Index,U_Nick,U_Id from user where U_Id='"+id+"' and U_Pw='" + pwd+"'";
            da = new MySqlDataAdapter(query, conn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                result = dr["U_Index"].ToString();
                nick = dr["U_Nick"].ToString();
                userId = dr["U_Id"].ToString();
            }
            else
            {
                result = "";
                nick = "";
                userId = "";
            }
        }

        /// <summary>
        /// 이메일을 통한 유저 아이디 찾기
        /// </summary>
        /// <param name="email">유저 이메일</param>
        /// <returns></returns>
        public string selectUser(string email) {
            string result;
            query = "select U_Id from user where U_Mail='" + email + "'";
            da = new MySqlDataAdapter(query, conn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                result = dr["U_Id"].ToString();
            }
            else
            {
                result = "0";
            }
            
            return result;

        }

        /// <summary>
        /// 유저 승패 정보
        /// </summary>
        /// <param name="index">유저 index</param>
        /// <param name="win">승</param>
        /// <param name="lose">패</param>
        /// <param name="draw">무</param>
        /// <returns></returns>
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

        /// <summary>
        /// 유저 승패 갱신
        /// </summary>
        /// <param name="index">유저 index</param>
        /// <param name="win">승</param>
        /// <param name="lose">패</param>
        /// <param name="draw">무</param>
        /// <returns></returns>

        public int setWinLose(string[] argumentlist)
        {
            try
            {
                int result = 0;

                string winIndex = "";
                string loseIndex = "";
                string drawIndex = "";
                string sql = "";

                bool winCheck = false;
                bool loseCheck = false;
                bool drawCheck = false;
                for (int i = 0; i < argumentlist.Length; i += 2)
                {

                    switch (Int32.Parse(argumentlist[i + 1]))
                    {
                        case 0:
                            if (drawCheck)
                                drawIndex += ",";
                            drawIndex += argumentlist[i];
                            break;
                        case 1:
                            if (winCheck)
                                winIndex += ",";
                            winIndex += argumentlist[i];
                            break;
                        case 2:
                            if (loseCheck)
                                loseIndex += ",";
                            loseIndex += argumentlist[i];
                            break;
                    }
                }
                if (!drawCheck)
                    drawIndex = "''";
                if (!winCheck)
                    winIndex = "''";
                if (!loseCheck)
                    loseIndex = "''";

                sql = getSql(winIndex, loseIndex, drawIndex);

                MySqlCommand comm = new MySqlCommand(sql, conn);
                execute(comm);
                return result;
            }
            catch(Exception)
            {
                return 99;
            }

        }


        private string getSql(string win, string lose, string draw)
        {
            return "update winlate " +
                            "set W_win = case " +
                                    "when u_index in  (" + win + ") then w_win + 1" +
                                    "else W_win" +
                                    "end," +
                            "W_lose = case" +
                                    "when u_index in (" + lose + ") then w_lose + 1" +
                                    "else W_lose" +
                                "end," +
                            "W_draw = case" +
                                "when  u_index in (" + draw + ") then w_draw + 1" +
                                "else w_draw" +
                                "end" +
                         "where U_index in ("+win+","+lose+","+draw+")";
        }

        /// <summary>
        /// index를 통해 유저 닉네임 찾기
        /// </summary>
        /// <param name="receiveIndex">유저 index</param>
        /// <param name="nick">유저 닉네임</param>
        /// <returns></returns>
        public int getUserNick(int receiveIndex, out string nick)
        {
            query = "select U_Nick from user where U_index=" + receiveIndex + "";
            da = new MySqlDataAdapter(query, conn);
            ds = new DataSet();
            da.Fill(ds);

            if (ds.Tables[0].Rows.Count != 0)
            {
                DataRow dr = ds.Tables[0].Rows[0];
                nick = dr["U_Nick"].ToString();
                return 0;
            }
            else
            {
                nick = "";
                return 1;
            }
        }

        /// <summary>
        /// 유저 비밀번호 변경
        /// </summary>
        /// <param name="id">유저 id</param>
        /// <param name="password">유저 비밀번호</param>
        public void updateUser(string id, string password)
        {
            try
            {
                query = "update user set U_pw='" + password + "' where U_id='" + id + "'";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                execute(cmd);
            }
            catch (MySqlException e)
            {
                Console.WriteLine("<error>");
                Console.WriteLine(e.Message);
                Console.WriteLine("update fail\n");
            }
        }
        public void createLog(int index, string operation, int messageCheck, string IP, int sendRecv)
         {
            string afterQuery;
            query = "insert into avalon.log (L_Operation, L_MessageCheck, L_UserIP, L_SendRecv";

            afterQuery = ") values ('";


            if(operation != null)
                afterQuery += operation.Replace("\'", "\\\'");
            afterQuery +="',"
            + messageCheck + ",'"
            + IP + "',"
            + sendRecv;
            
            if (-1 != index)
            {
                query += ", L_UserIndex";
                afterQuery += "," + index; 
            }
            afterQuery += ");";
            MySqlCommand cmd = new MySqlCommand(query+afterQuery, conn);
            execute(cmd);
        }

        public void execute(MySqlCommand comm)
        {
            while (executeCheck) { }
            executeCheck = true;
            MySqlCommand cmd = comm;
            cmd.ExecuteNonQuery();
            disConnect();
            executeCheck = false;
        }
    }
}