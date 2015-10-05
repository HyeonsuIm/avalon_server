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
        public Log()
        {
            string sDirPath;
            sDirPath = System.ApplicationId.StartupPath + "\\images";
            DirectoryInfo di = new DirectoryInfo(sDirPath);
            if (di.Exists == false)
            {
                di.Create();
            }
            fileName = System.DateTime.Now.ToString("yyyy-MM-dd");
            filestreamer = File.Create(fileName);
        }
        
    }

    
}
