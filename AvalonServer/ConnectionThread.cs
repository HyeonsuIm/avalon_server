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
        public NetworkStream ns
        {
            get;
            set;
        }

        // 클라이언트 ip
        String clientIp;

        // 유저 닉네임
        public string userNick
        {
            get;
            set;
        }

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
            threadPoolManage.addClient(this);

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
            threadPoolManage.removeClient(this);
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

                    Console.WriteLine("\n********************message receive*********************");
                    OpcodeAnalysor analysor = new OpcodeAnalysor(data);
                    
                    CommunicationForm comm = analysor.separateOpcode();
                    comm.connectionThread = this;
                    comm.process();

                    if (comm.formNumber == 0)
                    {
                        userNick = ((LoginForm)comm).getNick();
                    }
                    if (comm.formNumber == 9 && comm.opcode == 0)
                    {
                        break;
                    }
                    string receiveString = Encoding.UTF8.GetString(data).Trim('\0');
                    Console.WriteLine("{0} : {1}", clientIp, receiveString);
                    
                }
                catch (ArgumentException)
                {
                    Console.WriteLine("<error>");
                    Console.WriteLine("argument count incorrect\n");

                }
                catch(Exception e)
                {
                    threadPoolManage.removeClient(this);
                    Console.WriteLine(e.Message);
                    break;
                }
                Console.WriteLine("********************message process complete*********************\n");
            }
            Console.WriteLine("********************message process complete*********************\n");
            disConnect();
        }

        public void sendMessage(string data)
        {
            Console.WriteLine("<send Message>");
            Console.WriteLine("{0}\n",data);
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            ns.Write(byteData, 0, byteData.Length);
        }
    }
}