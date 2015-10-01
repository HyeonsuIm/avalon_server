using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace AvalonServer
{
    
    class ServerMain
    {
        public static DBConnection DBC = new DBConnection();
        public static Log Sendlog = new Log();
        public static Log Recvlog = new Log();
        static void Main(string[] args)
        {
            Console.WriteLine("---------------Server Start----------------");
            RoomListInfo roomListInfo = new RoomListInfo();
            ThreadPoolManage threadPool = new ThreadPoolManage(roomListInfo);
            
        }
    }
}