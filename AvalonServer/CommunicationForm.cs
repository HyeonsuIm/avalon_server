﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AvalonServer
{
    
    abstract class CommunicationForm
    {
        public string delimiter = "\u0001";

        public CommunicationForm(){}

        public ThreadPoolManage threadPoolManage
        {
            get;
            set;
        }
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
        public DBConnection DBC
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

        public LoginForm() {
            formNumber = 0;
        }

        override public void process()
        {
            
            switch (opcode)
            {
                // 로그인 요청
                case 10:
                    DBC.selectUser(argumentList[0], argumentList[1], out result, out userNick);
                    if (result != "")
                    {
                        result = "" + formNumber + opcode + "01" + result;
                        connectionThread.sendMessage(result);
                        //닉네임 정보 찾기
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
                    DBC.setQuery(query);

                    result += "01" + DBC.executeNonQuery();

                    connectionThread.sendMessage(result);
                    break;
                // 닉네임 중복검사
                case 12:
                    query = "select U_id from user where U_Nick = '" + argumentList[0] + "'";
                    result = "" + formNumber + opcode;
                    DBC.setQuery(query);

                    result += "01" + DBC.executeNonQuery();

                    connectionThread.sendMessage(result);
                    break;
                // email 중복검사
                case 13:
                    query = "select U_id from user where U_Mail = '" + argumentList[0] + "'";
                    result = "" + formNumber + opcode;
                    DBC.setQuery(query);

                    result += "01" + DBC.executeNonQuery();

                    connectionThread.sendMessage(result);
                    break;
                // 회원가입
                case 14:
                    result = "" + formNumber + opcode + "01";
                    result += DBC.insertUser(argumentList, argumentList.Length);
                    connectionThread.sendMessage(result);
                    break;
                // 아이디 찾기
                case 15:
                    result = DBC.selectUser(argumentList[0]);
                    if (result == "")
                        result = "" + formNumber + opcode + "00";
                    else
                        result = "" + formNumber + opcode + "01" + result;
                    connectionThread.sendMessage(result);
                    break;
                case 16:
                    // 비밀번호 찾기
                    // 보류
                    break;

            }
        }

        public string getNick()
        {
            return userNick;
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
                    threadPoolManage.sendToAll("10001" + argumentList[0]);
                    break;
                // 귓속말
                case 1:
                    threadPoolManage.sendToUser("", "10101" + argumentList[0]);
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

