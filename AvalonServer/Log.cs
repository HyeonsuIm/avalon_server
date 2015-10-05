using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

        string fileName;
        FileStream filestreamer;
        string sDirPath;
        string delimeter = "\u0002";
        bool used = false;
        public Log()
        {
            sDirPath = AppDomain.CurrentDomain.BaseDirectory;
            sDirPath = sDirPath +@"Log\";
            DirectoryInfo di = new DirectoryInfo(sDirPath);
            if (di.Exists == false)
            {
                di.Create();
            }
            sDirPath += System.DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
           
        }
        private bool garbageCheck(string data)
        {
            return data == "90200";
        }
        public void save(string data, string IP, string id, int sendRecv)
        {
            if (garbageCheck(data))
                return;
            while (used) { }
            used = true;

            filestreamer = new FileStream(this.sDirPath, FileMode.Append, FileAccess.Write);
            using(StreamWriter sWriter = new StreamWriter(filestreamer))
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                sWriter.WriteLine(time + delimeter + IP + delimeter + id + delimeter + sendRecv + delimeter + data + delimeter + "0");
                filestreamer.Flush();
            }
            filestreamer.Close();
            used = false;
           
        }
        public void save(string data, string IP, string id, int sendRecv, string exceptionMessage)
        {
            if (garbageCheck(data))
                return;
            while (used) { }
            used = true;

            filestreamer = new FileStream(this.sDirPath, FileMode.Append, FileAccess.Write);
            using (StreamWriter sWriter = new StreamWriter(filestreamer))
            {
                string time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                sWriter.WriteLine(time + delimeter + IP + delimeter + sendRecv + delimeter + data + delimeter + exceptionMessage);

                filestreamer.Flush();
            }
            filestreamer.Close();
            used = false;
        }
        
        ~Log()
        {
            filestreamer.Close();
        }
    }

    
}
