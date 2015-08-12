﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
                    localDB.selectUser(argumentList[0], argumentList[1], out result, out userNick);
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
                    result = localDB.selectUser(argumentList[0]);
                    if (result == "")
                        result = "" + formNumber + opcode + "01" + "0";
                    else
                        result = "" + formNumber + opcode + "01" + result;
                    connectionThread.sendMessage(result);
                    break;
                // 비밀번호 찾기
                case 16:
                    result = localDB.selectUser(argumentList[0], argumentList[1]);
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

        public string getNick()
        {
            return userNick;
        }

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
        ThreadPoolManage threadPoolManage = ServerMain.threadPool;
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
                //방정보 요청
                case 2:
                    RoomInfo[] roomInfo = roomListInfo.getRoomListInfo();
                    foreach(RoomInfo i in roomInfo)
                    {
                        //i.getRoonInfo();
                    }
                    connectionThread.sendMessage("" + formNumber +"02" + "04" + "01" + delimiter + "roomnumber" + delimiter + "roomname" + delimiter + "roomperson");
                    break;
                case 3:
                    break;
                case 4:
                    try{
                        roomListInfo.addRoom(Int16.Parse(argumentList[0]), argumentList[1], argumentList[2], argumentList[3]);
                        connectionThread.sendMessage("" + formNumber + "04" + "01" + "1");
                    }catch(Exception e){
                        Console.WriteLine(e.Message);
                        connectionThread.sendMessage("" + formNumber + "04" + "01" + "0");
                    }
                    
                    break;
                case 5:
                    try
                    {
                        roomListInfo.comeInRoom(argumentList[0], Int16.Parse(argumentList[1]), argumentList[2]);
                        connectionThread.sendMessage("" + formNumber + "05" + "01" + "1");
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
                case 0:
                    string nick;
                    localDB.getUserNick(int.Parse(argumentList[0]), out nick);
                    message = formNumber + "00" + "02" + nick + delimiter + argumentList[0];
                    break;
                    // 호스트 IP
                case 1:
                    break;
                    // 플레이어 전적
                case 2:
                    int win, lose;
                    result = localDB.getWinLose(int.Parse(argumentList[0]),out win, out lose);
                    message = formNumber + "02" + "02" + win.ToString() + delimiter + lose.ToString();
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
                    break;
                case 3:
                    break;
            }
        }
    }
}