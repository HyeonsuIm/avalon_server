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
        //로그 기록을 위한 Log 객체 선언
        //유저 정보
        public TcpUserInfo userInfo
        {
            get;
            set;
        }

        // 데이터 송수신용
        byte[] data;
        string opcode;
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
            clientIpep = (IPEndPoint)client.RemoteEndPoint;
            userInfo.State = -1;
            userInfo.IP = clientIpep.ToString();
            this.client = client;
            //client.SendTimeout = 10000;
            //client.ReceiveTimeout = 10000;
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
            try {
                client.Close();
                threadPoolManage.removeClient(this);
                connections--;
                Console.WriteLine("Client disconnect\nip : {0}\nconnections : {1}\n", clientIpep.ToString(), connections);
            }catch(Exception e)
            {
                Console.WriteLine("??????????????????????????????????????????????????????????????????????????????\n");
                Console.WriteLine(e.Message);

            }
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


                    if (userInfo != null && userInfo.userId != null)
                        ServerMain.Log.save(Encoding.UTF8.GetString(data).Trim('\0'), this.clientIpep.Address.ToString(), userInfo.userId, 1);
                    else
                        ServerMain.Log.save(Encoding.UTF8.GetString(data).Trim('\0'), this.clientIpep.Address.ToString(), "-1", 1);
                    opcode = Encoding.ASCII.GetString(data).Trim('\0').Substring(0, 5);
                    if (printCheck(opcode))
                        Console.WriteLine("\n********************message receive*********************");
                    // 수신 된 데이터를 분석하기 위한 객체 생성
                    OpcodeAnalysor analysor = new OpcodeAnalysor(data);
                    
                    // 데이터를 양식에 맞게 분할
                    comm = analysor.separateOpcode(printCheck(opcode));
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
                        userInfo.State = (int)TcpUserInfo.state.LOBBY;

                    }
                    if (comm.formNumber == 9 && comm.opcode == 0)
                    {
                        break;
                    } 

                    // 수신된 데이터 출력
                    
                    string receiveString = Encoding.UTF8.GetString(data).Trim('\0');
                    if (printCheck(Encoding.ASCII.GetString(data).Trim('\0')))
                        Console.WriteLine("{0} : {1}", clientIpep.ToString(), receiveString);

                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("<argument error>");
                    Console.WriteLine(ex.Message + "\n");

                    if (userInfo != null)
                        ServerMain.Log.save(Encoding.UTF8.GetString(data).Trim('\0'), this.clientIpep.Address.ToString(), userInfo.userId, 1, ex.Message);
                    else
                        ServerMain.Log.save(Encoding.UTF8.GetString(data).Trim('\0'), this.clientIpep.Address.ToString(), "-1", 1,ex.Message);

                }
                catch(SocketException e)
                {
                    threadPoolManage.removeClient(this);
                    Console.WriteLine("<socket error>\n" + e.Message + "\n");

                    if (userInfo != null)
                        ServerMain.Log.save(Encoding.UTF8.GetString(data).Trim('\0'), this.clientIpep.Address.ToString(), userInfo.userId, 1, e.Message);
                    else
                        ServerMain.Log.save(Encoding.UTF8.GetString(data).Trim('\0'), this.clientIpep.Address.ToString(), "-1", 1, e.Message);
                    break;
                }
                catch(Exception e)
                {
                    threadPoolManage.removeClient(this);
                    Console.WriteLine("<error>\n" + e.Message + "\n");

                    if (userInfo != null)
                        ServerMain.Log.save(Encoding.UTF8.GetString(data).Trim('\0'), this.clientIpep.Address.ToString(), userInfo.userId, 1, e.Message);
                    else
                        ServerMain.Log.save(Encoding.UTF8.GetString(data).Trim('\0'), this.clientIpep.Address.ToString(), "-1", 1, e.Message);

                    break;
                }

                if (printCheck(opcode))
                    Console.WriteLine("********************message process complete*********************\n");
            }
            if (userInfo.State == (int)TcpUserInfo.state.GAME)
            {
                comm.roomListInfo.comeOutRoom(userInfo.Number, userInfo.userIndex);
            }
            // 연결 종료
            disConnect();
        }

        bool printCheck(string opcode)
        {
            if (opcode == "90200")
                return false;
            else if (opcode == "10300")
                return false;
            return true;
        }

        /// <summary>
        /// string 데이터 송신
        /// </summary>
        /// <param name="data">송신할 문자열</param>
        public void sendMessage(string data)
        {
            if (printCheck(opcode))
            {
                Console.WriteLine("<send Message>");
                Console.WriteLine("{0}\n", data);
            }
            byte[] byteData = Encoding.UTF8.GetBytes(data);
            sendVarData(byteData);
            
        }

        /// <summary>
        /// byte 데이터 송신
        /// </summary>
        /// <param name="byteData">송신할 데이터</param>
        public void sendMessage(byte[] byteData)
        {
            if (printCheck(opcode))
            {
                Console.WriteLine("<send Message>");
                Console.WriteLine("{0}\n", Encoding.ASCII.GetString(byteData));
            } 
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

            if (userInfo != null)
                ServerMain.Log.save(Encoding.UTF8.GetString(data).Trim('\0'), this.clientIpep.Address.ToString(), userInfo.userId, 1);
            else
                ServerMain.Log.save(Encoding.UTF8.GetString(data).Trim('\0'), this.clientIpep.Address.ToString(), "-1", 1);
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
        private byte[] receiveVarData() {
            int total = 0;
            int recv;
            byte[] datasize = new byte[4];
            try
            {
                recv = client.Receive(datasize, 0, 4, 0);
            }
            catch (SocketException e)
            {
                throw e;
            }
            
                if (recv == 0)
                    return null;
                int size = BitConverter.ToInt32(datasize, 0);
                int dataleft = size;
                byte[] data = new byte[size];
            try
            {
                while (total < size)
                {
                    recv = client.Receive(data, total, dataleft, 0);
                    if (recv == 0)
                    {
                        data = Encoding.UTF8.GetBytes("exit");
                        break;
                    }
                    total += recv;
                    dataleft -= recv;
                }
            }
            catch (SocketException e)
            {
                throw e;
            }


            return data;
        }
    }
}