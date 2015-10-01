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
        TcpUserInfo userInfo;
        public LobbyForm()
        {
            formNumber = 1;
        }
        
        override public void process()
        {
            BinaryFormatter bf;
            MemoryStream ms;
            int msSize;
            byte[] buffer;
            switch (opcode)
            {
                // 채팅
                case 0:
                    threadPoolManage.sendToAll(formNumber + "00" + "02" + argumentList[0] + delimiter + argumentList[1]);
                    break;
                
                // 방정보 요청
                // 직렬화
                case 2:
                    bf = new BinaryFormatter();
                    ms = new MemoryStream();
                    bf.Serialize(ms, threadPoolManage.roomListInfo); 

                    ms.Position = 0;

                    msSize = ms.ToArray().Length;
                    buffer = new byte[msSize+5];
                    Encoding.UTF8.GetBytes("10202").CopyTo(buffer, 0);
                    ms.ToArray().CopyTo(buffer,5);
                    connectionThread.sendMessage(buffer);
                    break;
                //플레이어 전체 정보 요청
                case 3:
                    int[] index = null;
                    string[] nick = null;
                    string data="";
                    userInfo = connectionThread.userInfo;
                    threadPoolManage.currentUserInfo(ref index, ref nick);

                    data += (formNumber*10000 + opcode*100 + (index.Length * 2));
                    data += index[0] + delimiter + nick[0];
                    for (int i = 1; i < index.Length;i++ )
                    {
                        data += delimiter + index[i] + delimiter + nick[i]; 
                    }

                    threadPoolManage.sendToUser(userInfo.userIndex, data);
                    break;
                    //방 생성
                case 4:
                    try{
                        userInfo = connectionThread.userInfo;
                        userInfo.State = (int)TcpUserInfo.state.GAME;
                        roomListInfo.addRoom(Int16.Parse(argumentList[0]), argumentList[1], argumentList[2],int.Parse(argumentList[4]), userInfo);
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
                        userInfo = connectionThread.userInfo;
                        userInfo.State = (int)TcpUserInfo.state.GAME;
                        RoomListProcess roomProcess = new RoomListProcess(roomListInfo);
                        int number = roomProcess.findRoomNumber(int.Parse(argumentList[1]));

                        roomListInfo.comeInRoom(number, argumentList[2], userInfo);
                        
                        bf = new BinaryFormatter();
                        ms = new MemoryStream();
                        bf.Serialize(ms, threadPoolManage.roomListInfo.roomInfo[number]);

                        ms.Position = 0;

                        msSize = ms.ToArray().Length;
                        buffer = new byte[msSize + 5];
                        Encoding.UTF8.GetBytes("" + formNumber + "05" + "01").CopyTo(buffer, 0);
                        ms.ToArray().CopyTo(buffer, 5);
                        connectionThread.sendMessage(buffer);
                        RoomInfo roomInfo = roomListInfo.roomInfo[number];
                        foreach (int memberIndex in roomInfo.getMemberIndexList())
                        {
                            if (memberIndex != userInfo.userIndex)
                            {
                                threadPoolManage.sendToUser(memberIndex, "" + formNumber + opcode + "02" + userInfo.userIndex.ToString() + delimiter + userInfo.userNick);
                            }
                        }

                        //threadPoolManage.sendToUser(roomProcess.getMemberIndexList(number), "" + formNumber + "05" + "01" + userInfo.userIndex.ToString());
                        
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        connectionThread.sendMessage("" + formNumber + "05" + "01" + "0");
                    }
                    break;
            }
        }
    }
    
    class RoomForm : CommunicationForm
    {
        TcpUserInfo userInfo;
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
                    userInfo.State = (int)TcpUserInfo.state.LOBBY;
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
                    //방장에게는 호스트 및 나머지 유저들의 ip정보를 받고,
                    //나머지에게는 방장의 ip정보를 준다.
                    RoomInfo roomInfo = roomListInfo.getRoomInfo(int.Parse(argumentList[1]));
                    int memberCount = roomInfo.getMemberCount();
                    string IPList = "";
                    for (int i = 1; i < memberCount; i++)
                    {
                        if (i != 1)
                            IPList += delimiter;
                        IPList  += roomInfo.memberInfo[i].IP;
                        threadPoolManage.sendToUser(roomInfo.memberInfo[i].userIndex, "" + formNumber + "15" + "01" + roomInfo.memberInfo[0].IP);
                    }
                    threadPoolManage.sendToUser(roomInfo.memberInfo[0].userIndex, "" + formNumber + "15" + "0" + (memberCount-1) + IPList);
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
                case 99:
                    DBConnection localDB = ServerMain.DBC;
                    int result;
                    result = localDB.setWinLose(argumentList);
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
                    // 귓속말
                case 4:
                    int targetIndex = threadPoolManage.findIndexToId(argumentList[1]);
                    threadPoolManage.sendToUser(targetIndex, formNumber + "01" + "02" + argumentList[0] + delimiter + argumentList[2]);
                    connectionThread.sendMessage(formNumber + "01" + "02" + argumentList[0] + delimiter + argumentList[2]);
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