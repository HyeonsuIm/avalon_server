using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvalonServer
{
    class UserInfo
    {
        //유저 정보
        public string userId;
        public string userNick;
        public int userIndex;
        public string IP;

        public enum state { LOBBY = 0, ROOM, GAME };
        //유저 상태
        public int State{
            get;
            set;
        }
        public int Number{
            get;
            set;
        }

    }
}
