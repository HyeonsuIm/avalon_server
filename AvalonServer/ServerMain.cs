using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace AvalonServer
{
    class ServerMain
    {
        static void Main(string[] args)
        {
            Console.WriteLine("---------------Server Start----------------");
            DBConnection DBC = new DBConnection();
            RoomListInfo roomListInfo = new RoomListInfo();
            ThreadPoolManage threadPool = new ThreadPoolManage(DBC , roomListInfo);
        }
    }
}