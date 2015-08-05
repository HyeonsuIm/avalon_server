using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvalonServer
{
    public class RoomListInfo 
    {
        int roomMaxSize;
        int roomCount;
        RoomInfo[] roomInfo;
        bool[] roomNumberUsed;
        
        public RoomListInfo()
        {
            roomMaxSize = 20;
            roomInfo = new RoomInfo[roomMaxSize];
            roomNumberUsed = new bool[roomMaxSize];
        }
        
        public void addRoom(string name, int type, string password, string member)
        {
            int number;
            for (number = 0; number < roomMaxSize; number++) {
                if (roomNumberUsed[number] == false)
                {
                    roomNumberUsed[number] = true;
                    break;
                }
                    
            }
            roomInfo[number].createRoom(name, type, password, member);
        }

        public void removeRoom(int number)
        {
            roomInfo[number] = null;
            roomNumberUsed[number] = false;
        }

        public RoomInfo[] getRoomListInfo()
        {
            return roomInfo;
        }
    }

    public class RoomInfo
    {
        string name;
        int type;
        int number;
        string password;
        int memberCount;
        string[] memberList = new string[6];

        public void createRoom(string name, int type, string password, string member)
        {
            this.name = name;
            this.type = type;
            this.password = password;
            memberCount = 1;
            memberList[0] = member;
        }
        public string[] getRoomInfo()
        {
            string[] roomInfo = new string[11];
            roomInfo[0] = name;
            roomInfo[1] = type.ToString();
            roomInfo[2] = number.ToString();
            roomInfo[3] = password;
            roomInfo[4] = memberCount.ToString();
            for(int i = 0; i < memberList.Length; i++)
            {
                roomInfo[i+5] = memberList[i - 5];
            }
            return roomInfo;
        }
    }
}