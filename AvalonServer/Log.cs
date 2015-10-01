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

        public void setLog(string op, string IP, int userIndex, int sendRecv )
        {
            this.opcode = op;
            this.IP = IP;
            this.sendRecv = sendRecv;
            this.userIndex = userIndex;
        }
        public void setLog(string op, string IP, int sendRecv)
        {
            this.opcode = op;
            this.IP = IP;
            this.sendRecv = sendRecv;
        }
        public void setSuccess(bool Check)
        {
            if (Check)
                this.messageCheck = 1;
        }


        public void log()
        {
            opcode = null;
            IP = null;
            messageCheck = 0;
            userIndex = -1;
            sendRecv = -1;
        }
        

        public void save()
        {
            ServerMain.DBC.createLog(userIndex,opcode,messageCheck,IP,sendRecv);
        }
    }

    
}
