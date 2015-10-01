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
        // 클라이언트 접속 IP
        public IPEndPoint clientIpep;

        //유저 정보
        public TcpUserInfo userInfo
        {
            get;
            set;
        }

        // 데이터 송수신용
        byte[] data;

        // 연결 수
        private static int connections = 0;

        public ConnectionThread() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client">클라이언트에 연결 된 소켓 객체</param>
        /// <param name="threadPool">클라이언트 연결을 관리할 쓰레드 풀</param>
        public ConnectionThread(ref Socket client, ThreadPoolManage threadPool)
        {
            userInfo = new TcpUserInfo();
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
            
            sendMessage("9" + "00" + "00");
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
            CommunicationForm comm = null;
            while (true)
            {
                try {
                    // 데이터 수신 대기 및 수신
                    data = receiveVarData();
                    if (data == null)
                        break;
                    Console.WriteLine("\n********************message receive*********************");

                    // 수신 된 데이터를 분석하기 위한 객체 생성
                    OpcodeAnalysor analysor = new OpcodeAnalysor(data);
                    
                    // 데이터를 양식에 맞게 분할
                    comm = analysor.separateOpcode();
                    comm.connectionThread = this;
                    comm.threadPoolManage = threadPoolManage;
                    comm.roomListInfo = threadPoolManage.roomListInfo;

                    // 분할된 데이터 처리
                    comm.process();

                    // 예외처리
                    if (comm.formNumber == 0 && comm.opcode == 10)
                    {
                       userInfo.userNick = ((LoginForm)comm).getInfo(out userInfo.userIndex, out userInfo.userId);
                       userInfo.IP = clientIpep.ToString();
                    }
                    if (comm.formNumber == 9 && comm.opcode == 0)
                    {
                        break;
                    } 

                    // 수신된 데이터 출력
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
            if (userInfo.State == (int)TcpUserInfo.state.GAME)
            {
                int roomMaxSize = comm.roomListInfo.roomMaxSize;
                bool findCheck = false;
                for (int roomIterator = 0; roomIterator < roomMaxSize; roomIterator++)
                {
                    if (comm.roomListInfo.roomInfo[roomIterator] != null)
                    {
                        RoomInfo roomInfo = comm.roomListInfo.roomInfo[roomIterator];
                        int memberCount = roomInfo.getMemberCount();
                        for (int memberIterator = 0; memberIterator < memberCount; memberIterator++)
                        {
                            if (userInfo.userIndex == roomInfo.memberInfo[memberIterator].userIndex)
                            {
                                findCheck = true;
                                comm.roomListInfo.comeOutRoom(roomIterator, userInfo.userIndex);
                                break;
                            }
                        }
                        if (findCheck)
                            break;
                    }
                }

            }
            // 연결 종료
            disConnect();
        }

        /// <summary>
        /// string 데이터 송신
        /// </summary>
        /// <param name="data">송신할 문자열</param>
        public void sendMessage(string data)
        {
            Console.WriteLine("<send Message>");
            Console.WriteLine("{0}\n", data);
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            sendVarData(byteData);
        }

        /// <summary>
        /// byte 데이터 송신
        /// </summary>
        /// <param name="byteData">송신할 데이터</param>
        public void sendMessage(byte[] byteData)
        {
            Console.WriteLine("<send Message>");
            Console.WriteLine("{0}\n", Encoding.ASCII.GetString(byteData));
            sendVarData(byteData);
        }

        /// <summary>
        /// 가변 데이터 송신
        /// </summary>
        /// <param name="data">송신할 데이터</param>
        /// <returns>송신 데이터의 크기</returns>
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

        /// <summary>
        /// 가변 데이터 수신
        /// </summary>
        /// <returns>수신된 데이터</returns>
        private byte[] receiveVarData(){
            int total = 0;
            int recv;
            byte[] datasize = new byte[4];
            recv = client.Receive(datasize,0,4,0);
            if (recv == 0)
                return null;
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