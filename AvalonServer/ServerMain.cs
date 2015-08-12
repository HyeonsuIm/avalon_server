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
        public static ThreadPoolManage threadPool;
        static void Main(string[] args)
        {
            Console.WriteLine("---------------Server Start----------------");
            RoomListInfo roomListInfo = new RoomListInfo();
            threadPool = new ThreadPoolManage();
            threadPool.roomListInfo = roomListInfo;
            
        }
    }
}