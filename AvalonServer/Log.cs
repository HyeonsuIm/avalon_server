using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvalonServer
{
    /// <summary>
    /// messageCheck : 성공실패여부를 기록하는 변수 (0 : 실패, 1: 성공)
    /// sendRecv : 수신,송신여부를 기록하는 변수 (0 : 송신, 1: 수신)
    /// </summary>
    class Log
    {
        string opcode;
        string IP;
        int messageCheck;
        int userIndex;
        int sendRecv;
        DBConnection DBC;

        public bool garbageCheck()
        {
            //Console.WriteLine(opcode);
            if (opcode == "" || opcode == null) { }
            else if (opcode.Equals("90200"))
                return false;
            else if (opcode.Substring(0, 3).Equals("103"))
                return false;

            return true;
        }
        public void setOperation(string OP, int sendRecv)
        {

            this.opcode = OP;
            this.sendRecv = sendRecv;
        }
        public void setLog(string IP, int userIndex)
        {
            this.userIndex = userIndex;
            if (IP != null)
                this.IP = IP.Split(':')[0];
        }
        public void setLog(string IP)
        {
            if (IP != null)
                this.IP = IP.Split(':')[0];
        }
        public void setSuccess(bool Check)
        {
            if (Check)
                this.messageCheck = 1;
            else
                this.messageCheck = 0;
        }
        public void setIndex(int index)
        {
            this.userIndex = index;
        }

        public Log()
        {
            opcode = "";
            IP = "";
            messageCheck = 0;
            userIndex = -1;
            sendRecv = -1;
        }
        

        public void save()
        {
            DBC = new DBConnection();
            DBC.connect();
            if (garbageCheck())
                DBC.createLog(userIndex,opcode,messageCheck,IP,sendRecv);
        }
    }

    
}
