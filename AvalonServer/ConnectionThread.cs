using System;
using System.Net.Sockets;
using System.Text;

namespace AvalonServer
{
    class ConnectionThread
    {
        // 클라이언트 접속을 위한 객체
        public TcpListener threadListener;

        // 쓰레드들을 관리할 객체
        public ThreadPoolManage threadPoolManage;

        // 클라이언트 접속 유지를 위한 객체
        TcpClient client;

        // 클라이언트와의 데이터 전송을 위한 객체
        NetworkStream ns;

        // 클라이언트 ip
        String clientIp;

        // 데이터 송수신용
        byte[] data = new byte[1024];

        // 연결 수
        private static int connections = 0;

        public ConnectionThread() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listener">클라이언트 정보를 수신한 Listener</param>
        /// <param name="manage">클라이언트 정보를 관리하는 쓰레드풀</param>
        public ConnectionThread(ref TcpListener listener, ThreadPoolManage manage)
        {
            threadListener = listener;
            threadPoolManage = manage;
        }
        
        /// <summary>
        /// 클라이언트 연결
        /// </summary>
        void connect()
        {
            client = threadListener.AcceptTcpClient();

            connections++;
            threadPoolManage.addClient(client);

            ns = client.GetStream();
            clientIp = client.Client.RemoteEndPoint.ToString();
            Console.WriteLine("New Client connect\nip : {0}\nconnections : {1}\n", clientIp, connections);
            
            data = Encoding.UTF8.GetBytes("connection success");
            ns.Write(data, 0, data.Length);
        }
        
        /// <summary>
        /// 클라이언트 연결 종료
        /// </summary>
        void disConnect()
        {
            
            ns.Close();
            client.Close();
            threadPoolManage.removeClient(client);
            connections--;
            Console.WriteLine("Client disconnect\nip : {0}\nconnections : {1}\n", clientIp, connections);
        }

        /// <summary>
        /// 클라이언트와의 통신
        /// </summary>
        /// <param name="state"></param>
        public void HandleConnection(Object state)
        {
            int recv;
            connect();
            while (true)
            {
                data = new byte[1024];
                try {

                    recv = ns.Read(data, 0, data.Length);
                    if (recv == 0)
                        break;

                    
                    OpcodeAnalysor analysor = new OpcodeAnalysor(data);
                    
                    CommunicationForm comm = analysor.separateOpcode();
                    comm.threadPoolManage = threadPoolManage;
                    comm.connectionThread = this;
                    comm.DBC = threadPoolManage.DBC;
                    comm.roomListInfo = threadPoolManage.roomListInfo;
                    comm.process();
                    string receiveString = Encoding.UTF8.GetString(data).Trim('\0');
                    Console.WriteLine("{0} : {1}", clientIp, receiveString);
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("argument incorrect");

                }
                catch(Exception e)
                {
                    threadPoolManage.removeClient(client);
                    Console.WriteLine(e.Message);
                    break;
                }
            }
            Console.WriteLine();
            disConnect();
        }

        public void sendMessage(string data)
        {
            Console.WriteLine("send message : {0}",data);
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            ns.Write(byteData, 0, byteData.Length);
        }
    }
}