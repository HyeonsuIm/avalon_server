
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Text;

namespace AvalonServer
{
    
    class ThreadPoolManage
    {
        // 클라이언트 정보를 관리할 리스트
        List<ConnectionThread> clientList;

        public RoomListInfo roomListInfo
        {
            get;
            set;
        }
        /// <summary>
        /// 쓰레드풀관리를 위한 초기화
        /// </summary>
        public ThreadPoolManage(RoomListInfo roomListInfo)
        {
            ServerMain.DBC.connect();
            this.roomListInfo = roomListInfo;

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(ipep);
            serverSocket.Listen(10);

            new List<int>().Add(30);
            
            clientList = new List<ConnectionThread>();
            //client = new TcpListener(IPAddress.Any, 9050);
            //client.Start();

            while (true)
            {
                Socket clientSocket = serverSocket.Accept();

                ConnectionThread newConnection = new ConnectionThread(ref clientSocket, this);
                ThreadPool.QueueUserWorkItem(new WaitCallback(newConnection.HandleConnection));
            }
        }

        /// <summary>
        /// 현재 접속중인 사용자 모두에게 데이터 전송
        /// </summary>
        /// <param name="data">전송할 데이터</param>
        /// <param name="length">데이터 길이</param>
        public void sendToAll(string data)
        {
            for (int i = 0; i < clientList.Count; i++) {
                try {
                    clientList.ElementAt(i).sendMessage(data);
                }catch(ObjectDisposedException e)
                {
                    removeClient(i);
                    Console.WriteLine("Client abnormal disconnected : active connections\n");
                }
            }
        }


        
        /// <summary>
        /// 현재 접속중인 사용자 모두의 간략한 정보를 보냄
        /// </summary>
        /// <param name="data">전송할 데이터</param>

        public void currentUserInfo(ref int[] index, ref string[] nick)
        {
            index = new int[clientList.Count];
            nick = new string[clientList.Count];
            for(int i=0;i<clientList.Count;i++)
            {
                index[i] = clientList.ElementAt(i).userInfo.userIndex;
                nick[i] = clientList.ElementAt(i).userInfo.userNick;
            }
        }

        public void sendToUser(int memberIndex, string data)
        {
            for(int i = 0; i < clientList.Count; i++)
            {
                if(memberIndex == clientList.ElementAt(i).userInfo.userIndex)
                {
                    clientList.ElementAt(i).sendMessage(data);
                }
            }
        }
        public void sendToUser(int[] memberList, string data)
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                for (int j = 0; j < memberList.Length; j++)
                {
                    if (memberList[j] == clientList.ElementAt(i).userInfo.userIndex)
                        clientList.ElementAt(i).sendMessage(data);
                }
            }
        }

        public int findIndexToId(string id)
        {
            int index = -1;
            for(int i = 0; i < clientList.Count; i++)
            {
                if (id.Equals(clientList.ElementAt(i).userInfo.userId))
                {
                    index = clientList.ElementAt(i).userInfo.userIndex;
                    break;
                }

            }
            return index;
        }

        /// <summary>
        /// 리스트에 TcpClient객체 추가
        /// </summary>
        /// <param name="tcpClient">추가할 TcpClient 객체</param>
        public void addClient(ConnectionThread connectionThread)
        {
            clientList.Add(connectionThread);
        }

        /// <summary>
        /// 리스트에 있는 TcpClient객체 제거
        /// </summary>
        /// <param name="tcpClient">제거할 TcpClient 객체</param>
        public void removeClient(ConnectionThread connectionThread)
        {
            clientList.Remove(connectionThread);
        }

        /// <summary>
        /// n번째 리스트 제거
        /// </summary>
        /// <param name="n">리스트 인덱스</param>
        public void removeClient(int n)
        {
            try
            {
                clientList.RemoveAt(n);
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("<error>");
                Console.WriteLine(e.Message);
                Console.WriteLine("index range out\n");
            }
        }
    }
}