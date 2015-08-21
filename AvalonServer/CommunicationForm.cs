using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AvalonServer
{
    
    abstract class CommunicationForm
    {
        public string delimiter = "\u0001";

        public CommunicationForm(){}

        public ConnectionThread connectionThread
        {
            get;
            set;
        }
        public int formNumber
        {
            get;
            set;
        }
        public int opcode
        {
            get;
            set;
        }   
        public int argumentCount
        {
            get;
            set;
        }
        public string[] argumentList
        {
            get;
            set;
        }
        public RoomListInfo roomListInfo
        {
            get;
            set;
        }
        
        public ThreadPoolManage threadPoolManage{
            get;
            set;
        }
        abstract public void process();

    }

    /// <summary>
    /// LoginForm 요청 처리
    /// </summary>
    class LoginForm : CommunicationForm
    {
        string query;
        string result;
        string userNick;
        string userId;
        DBConnection localDB = ServerMain.DBC;

        public LoginForm() {
            formNumber = 0;
        }

        override public void process()
        {
            
            switch (opcode)
            {
                // 로그인 요청
                case 10:
                    localDB.selectUser(argumentList[0], argumentList[1], out result, out userNick, out userId);
                    if (result != "")
                    {
                        result = "" + formNumber + opcode + "01" + result;
                        connectionThread.sendMessage(result);
                    }
                    else
                    {
                        result = "" + formNumber + opcode + "01" + "0";
                        connectionThread.sendMessage("010010");
                    }
                    
                    break;
                // id 중복검사
                case 11:
                    query = "select U_id from user where U_id = '"+argumentList[0]+"'";
                    result = "" + formNumber + opcode;
                    localDB.setQuery(query);

                    result += "01" + localDB.executeNonQuery();

                    connectionThread.sendMessage(result);
                    break;
                // 닉네임 중복검사
                case 12:
                    query = "select U_id from user where U_Nick = '" + argumentList[0] + "'";
                    result = "" + formNumber + opcode;
                    localDB.setQuery(query);

                    result += "01" + localDB.executeNonQuery();

                    connectionThread.sendMessage(result);
                    break;
                // email 중복검사
                case 13:
                    query = "select U_id from user where U_Mail = '" + argumentList[0] + "'";
                    result = "" + formNumber + opcode;
                    localDB.setQuery(query);

                    result += "01" + localDB.executeNonQuery();

                    connectionThread.sendMessage(result);
                    break;
                // 회원가입
                case 14:
                    result = "" + formNumber + opcode + "01";
                    result += localDB.insertUser(argumentList, argumentList.Length);
                    connectionThread.sendMessage(result);
                    break;
                // 아이디 찾기
                case 15:
                    result = "" + formNumber + opcode + "01" + localDB.selectUser(argumentList[0]);
                    connectionThread.sendMessage(result);
                    break;
                // 비밀번호 찾기
                case 16:
                    query = "select U_Index from user where U_Id='" + argumentList[0] + "' and U_mail='" + argumentList[1] + "'";
                    localDB.setQuery(query);
                    result = localDB.executeNonQuery();
                    if (result == "1")
                    {
                        Confirm confirm = new Confirm(argumentList[1]);
                        localDB.updateUser(argumentList[0], Encryption(confirm.getPassword().ToString()));
                        result = "" + formNumber + opcode + "01" + "1";
                    }
                    else
                    {
                        Console.WriteLine("<error>");
                        Console.WriteLine("아이디 또는 이메일 오류\n");
                        result = "" + formNumber + opcode + "01" + "0";
                    }
                    connectionThread.sendMessage(result);
                    break;

            }
        }
        /// <summary>
        /// 로그인 정보를 쓰레드에 저장
        /// </summary>
        /// <param name="index">index가 저장될 변수</param>
        /// <param name="Id">id가 저장될 변수</param>
        /// <returns></returns>
        public string getInfo(out int index,out string Id)
        {
            index = Int32.Parse(result.Substring(5));
            Id = userId;
            return userNick;
        }

        /// <summary>
        /// 암호화
        /// </summary>
        /// <param name="getValue">암호화 할 데이터</param>
        /// <returns></returns>
        static public string Encryption(string getValue)
        {
            SHA512 sha = new SHA512Managed();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(getValue));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}:", b);
            }
            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// LobbyForm 요청 처리
    /// </summary>
    class LobbyForm : CommunicationForm
    {
        public LobbyForm()
        {
            formNumber = 1;
        }
        
        override public void process()
        {
            switch (opcode)
            {
                // 채팅
                case 0:
                    threadPoolManage.sendToAll(formNumber + "00" + "02" + argumentList[0] + delimiter + argumentList[1]);
                    break;
                // 귓속말
                case 1:
                    threadPoolManage.sendToUser(argumentList[1], formNumber + "01" + "02" + argumentList[0] + delimiter + argumentList[2]);
                    connectionThread.sendMessage(formNumber + "01" + "02" + argumentList[0] + delimiter + argumentList[2]);
                    break;
                // 방정보 요청
                // 직렬화
                case 2:
                    BinaryFormatter bf = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream();
                    bf.Serialize(ms, threadPoolManage.roomListInfo); 

                    ms.Position = 0;

                    int msSize = ms.ToArray().Length;
                    byte[] buffer = new byte[msSize+5];
                    Encoding.UTF8.GetBytes("10202").CopyTo(buffer, 0);
                    ms.ToArray().CopyTo(buffer,5);
                    connectionThread.sendMessage(buffer);
                    break;
                //플레이어 전체 정보 요청
                case 3:
                    int[] index = null;
                    string[] nick = null;
                    string data="";
  
                    threadPoolManage.currentUserInfo(ref index, ref nick);

                    data += formNumber + opcode + (index.Length*2) + argumentList[0];
                    for (int i = 0; i < index.Length;i++ )
                    {
                        data += delimiter + index[i] + delimiter + nick[i]; 
                    }

                    threadPoolManage.sendToUser(connectionThread.userNick, data);
                    break;
                    //방 생성
                case 4:
                    try{
                        roomListInfo.addRoom(Int16.Parse(argumentList[0]), argumentList[1], argumentList[2], userInfo.userIndex, argumentList[3], int.Parse(argumentList[4]));
                        connectionThread.sendMessage("" + formNumber + "04" + "01" + "1");
                    }catch(Exception e){
                        Console.WriteLine(e.Message);
                        connectionThread.sendMessage("" + formNumber + "04" + "01" + "0");
                    }
                    
                    break;
                    //방 접속
                case 5:
                    try
                    {
                        int roomNumber = roomListInfo.comeInRoom(userInfo.userIndex, argumentList[0], int.Parse(argumentList[1]), argumentList[2]);
                        connectionThread.sendMessage("" + formNumber + "05" + "01" + roomNumber);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        connectionThread.sendMessage("" + formNumber + "05" + "01" + "-1");
                    }
                    break;
            }
        }
    }
    
    class RoomForm : CommunicationForm
    {
        UserInfo userInfo;
        RoomListProcess roomProcess;
        public RoomForm()
        {
            formNumber = 2;
        }

        public override void process()
        {
             roomProcess = new RoomListProcess(roomListInfo);
            userInfo = connectionThread.userInfo;
            switch (opcode)
            {
                case 0:
                    threadPoolManage.sendToUser(roomProcess.getMemberIndexList(userInfo.Number), "" + formNumber + "00" + "02" + argumentList[0] + delimiter + argumentList[1]);
                        break;
                case 2:
                    break;
                case 10:
                    break;
                case 11:
                    threadPoolManage.sendToUser(roomProcess.getMemberIndexList(userInfo.Number), "" + formNumber + "11" + "01" + userInfo.userNick);
                    roomListInfo.comeOutRoom(userInfo.Number, userInfo.userIndex);
                    userInfo.Number = -1;
                    userInfo.State = (int)UserInfo.state.LOBBY;
                    break;
                case 12:

                    break;
                case 13:
                    try
                    {
                        roomProcess.setRoom(int.Parse(argumentList[0]), argumentList[1], argumentList[2], roomProcess.findRoomNumber(userInfo.Number));

                        connectionThread.sendMessage("" + formNumber + "13" + "01" + "1");
                    }
                    catch (Exception)
                    {
                        connectionThread.sendMessage("" + formNumber + "13" + "01" + "0");
                    }
                    break;
                case 14:
                    
                    roomProcess.removeRoom(userInfo.Number);
                    threadPoolManage.sendToUser(roomProcess.getMemberIndexList(userInfo.Number), "" + formNumber + "14" + "01" + userInfo.userIndex);
                    break;
                case 15:
                    threadPoolManage.sendToUser(roomProcess.getMemberIndexList(userInfo.Number), "" + formNumber + "15" + "00");
                    break;

            }
        }
    }

    class GameForm : CommunicationForm
    {
        public GameForm()
        {
            formNumber = 3;
        }

        public override void process()
        {
            switch (opcode)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
            }
        }
    }

    class PlayerInfoProcess : CommunicationForm
    {
        DBConnection localDB = ServerMain.DBC;
        int result;
        string message;
        public PlayerInfoProcess()
        {
            formNumber = 8;
        }

        override public void process()
        {
            switch (opcode)
            {
                //플레이어 정보
                case 1:
                    string nick;
                    localDB.getUserNick(int.Parse(argumentList[0]), out nick);
                    message = formNumber + "01" + "02" + nick + delimiter + argumentList[0];
                    break;
                    // 호스트 IP
                case 2:
                    break;
                    // 플레이어 전적
                case 3:
                    int win, lose , draw;
                    result = localDB.getWinLose(int.Parse(argumentList[0]),out win, out lose, out draw);
                    message = formNumber + "03" + "03" + win.ToString() + delimiter + lose.ToString() + delimiter + draw.ToString();
                    break;
            }

            if (result == 0)
                connectionThread.sendMessage(message);
            else if (result == 1)
                connectionThread.sendMessage(formNumber + "03" + "01" + "colum missing");
        }
        
    }


    /// <summary>
    /// 종료 요청 처리
    /// </summary>
    class ShutdownForm : CommunicationForm
    {
        public ShutdownForm()
        {
            formNumber = 9;
        }

        override public void process()
        {
            switch (opcode)
            {
                case 0:
                    connectionThread.sendMessage("" + formNumber + "00" + "00");
                    break;
                case 1:
                    connectionThread.sendMessage("" + formNumber + "01" + "00");
                    break;
                case 2:
                    connectionThread.sendMessage("" + formNumber + "02" + "00");
                    break;
                case 3:
                    break;
            }
        }
    }
}