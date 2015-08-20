using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AvalonServer
{
    class ConnectionThread
    {
        // 쓰레드들을 관리할 객체
        public ThreadPoolManage threadPoolManage;

        // 클라이언트 접속 유지를 위한 객체
        Socket client;
        IPEndPoint clientIpep;

        //유저 정보
        public string userId;
        public string userNick;
        public int userIndex;

        // 데이터 송수신용
        byte[] data;

        // 연결 수
        private static int connections = 0;

        public ConnectionThread() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listener">클라이언트 정보를 수신한 Listener</param>
        /// <param name="manage">클라이언트 정보를 관리하는 쓰레드풀</param>
        public ConnectionThread(ref Socket client, ThreadPoolManage threadPool)
        {
            this.client = client;
            threadPoolManage = threadPool;
        }
        
        /// <summary>
        /// 클라이언트 연결
        /// </summary>
        void connect()
        {
            connections++;
            threadPoolManage.addClient(this);

            clientIpep = (IPEndPoint)client.RemoteEndPoint;
            Console.WriteLine("New Client connect\nip : {0}\nconnections : {1}\n", clientIpep.ToString(), connections);
            
            sendMessage("connection sucess");
        }
        
        /// <summary>
        /// 클라이언트 연결 종료
        /// </summary>
        void disConnect()
        {
            client.Close();
            threadPoolManage.removeClient(this);
            connections--;
            Console.WriteLine("Client disconnect\nip : {0}\nconnections : {1}\n", clientIpep.ToString(), connections);
        }

        /// <summary>
        /// 클라이언트와의 통신
        /// </summary>
        /// <param name="state"></param>
        public void HandleConnection(Object state)
        {
            connect();
            while (true)
            {
                try {
                    data = receiveVarData();

                    Console.WriteLine("\n********************message receive*********************");
                    OpcodeAnalysor analysor = new OpcodeAnalysor(data);
                    
                    CommunicationForm comm = analysor.separateOpcode();
                    comm.connectionThread = this;
                    comm.threadPoolManage = threadPoolManage;
                    comm.roomListInfo = threadPoolManage.roomListInfo;
                    comm.process();

                    if (comm.formNumber == 0 && comm.opcode == 10)
                    {
                       userNick = ((LoginForm)comm).getInfo(out userIndex, out userId);
                    }
                    if (comm.formNumber == 9 && comm.opcode == 0)
                    {
                        break;
                    }
                    string receiveString = Encoding.UTF8.GetString(data).Trim('\0');
                    Console.WriteLine("{0} : {1}", clientIpep.ToString(), receiveString);
                    
                }
                catch (ArgumentException e)
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
            Console.WriteLine("{0}\n", data);
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            sendVarData(byteData);
        }
        public void sendMessage(byte[] byteData)
        {
            Console.WriteLine("<send Message>");
            //Console.WriteLine("{0}\n", Encoding.ASCII.GetString(data));
            sendVarData(byteData);
        }

        private int sendVarData(byte[] data)
        {
            int total = 0;
            int size = data.Length;

            int dataleft = size;
            int sent;

            byte[] datasize = new byte[4];
            datasize = BitConverter.GetBytes(size);
            sent = client.Send(datasize);

            while (total < size)
            {
                sent = client.Send(data, total, dataleft, SocketFlags.None);
                total += sent;
                dataleft = -sent;
            }
            return total;
        }

        private byte[] receiveVarData(){
            int total = 0;
            int recv;
            byte[] datasize = new byte[4];
            recv = client.Receive(datasize,0,4,0);
            int size = BitConverter.ToInt32(datasize,0);
            int dataleft = size;
            byte[] data = new byte[size];

            while(total < size){
                recv = client.Receive(data,total,dataleft,0);
                if(recv == 0)
                {
                    data = Encoding.UTF8.GetBytes("exit");
                    break;
                }
                total += recv;
                dataleft -= recv;
            }
            return data;
        }
    }
}