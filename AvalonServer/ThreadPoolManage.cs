
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

        // 클라이언트 접속 대기를 위한 객체
        private TcpListener client;

        // DB연동을 위한 DBConnection 객체
        public DBConnection DBC;

        public RoomListInfo roomListInfo;
        /// <summary>
        /// 쓰레드풀관리를 위한 초기화
        /// </summary>
        public ThreadPoolManage(DBConnection DBC, RoomListInfo roomListInfo)
        {
            this.DBC = DBC;
            this.roomListInfo = roomListInfo;
            DBC.connect();
            clientList = new List<ConnectionThread>();
            client = new TcpListener(IPAddress.Any, 9050);
            client.Start();

            while (true)
            {
                while (!client.Pending())
                {
                    Thread.Sleep(1000);
                }
                ConnectionThread newConnection = new ConnectionThread(ref client, this);
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
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            for (int i = 0; i < clientList.Count; i++) {
                try {
                    NetworkStream ns = clientList.ElementAt(i).ns;
                    ns.Write(byteData, 0, byteData.Length);
                }catch(ObjectDisposedException e)
                {
                    removeClient(i);
                    Console.WriteLine("Client abnormal disconnected : active connections");
                }
            }
        }

        public void sendToUser(string user, string data)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            for(int i=0;i<clientList.Count;i++){
                if (user == clientList.ElementAt(i).userNick)
                {
                    NetworkStream ns = clientList.ElementAt(i).ns;
                    ns.Write(byteData, 0, byteData.Length);
                }
            }
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
            clientList.RemoveAt(n);
        }
    }
}