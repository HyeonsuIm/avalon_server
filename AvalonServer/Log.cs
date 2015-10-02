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
        }
        public void setIndex(int index)
        {
            this.userIndex = index;
        }

        public void log()
        {
            opcode = "";
            IP = "";
            messageCheck = 0;
            userIndex = -1;
            sendRecv = -1;
        }
        

        public void save()
        {
            if (opcode == "90200")
                return;
            ServerMain.DBC.createLog(userIndex,opcode,messageCheck,IP,sendRecv);
        }
    }

    
}
