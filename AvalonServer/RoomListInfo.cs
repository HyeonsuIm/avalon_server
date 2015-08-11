using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvalonServer
{
    public class RoomListInfo 
    {
        // 방 개수 증가폭
        const int IDwidth = 20;

        // 방 최대 개수
        int roomMaxSize;

        // 현재 방 개수
        int roomCount;

        // 방 정보
        RoomInfo[] roomInfo;

        // 번호 사용 여부
        bool[] roomNumberUsed;
        
        public RoomListInfo()
        {
            roomMaxSize = IDwidth;
            roomInfo = new RoomInfo[roomMaxSize];
            roomNumberUsed = new bool[roomMaxSize];
        }
        
        /// <summary>
        /// 새로운 방을 추가
        /// </summary>
        /// <param name="name">방 제목</param>
        /// <param name="type">방 타입</param>
        /// <param name="password">방 비밀번호</param>
        /// <param name="member">방장 이름</param>
        public void addRoom(int type, string name, string password, string memberId)
        {
            int number;
            for (number = 0; number < roomMaxSize; number++) {
                if (roomNumberUsed[number] == false)
                {
                    roomNumberUsed[number] = true;
                    break;
                }
            }
            if (roomMaxSize == number)
            {
                roomMaxSize += IDwidth;
                Array.Resize<RoomInfo>(ref roomInfo, roomMaxSize);
                Array.Resize<bool>(ref roomNumberUsed, roomMaxSize);
            }
            roomInfo[number] = new RoomInfo();
            roomInfo[number].createRoom(name, type, password, memberId);
        }

        public void comeInRoom(string userId, int roomNumber, string password)
        {
            roomInfo[roomNumber].addUser(userId, password);
        }

        public void comeOutRoom(int roomNumber, string name)
        {
            roomInfo[roomNumber].removeUser(name);
        }

        private void removeRoom(int number)
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
        const int MAX_MEMBER_COUNT = 6;
        string name;
        int type;
        string password;
        int memberCount;
        string[] memberList = new string[MAX_MEMBER_COUNT];

        public void addUser(string userId, string password)
        {
            if (this.password.Equals(password))
            {
                memberList[memberCount++] = userId;
            }
            else
            {
                throw new Exception("room full");
            }
        }

        public void removeUser(string name)
        {
            int i;
            for (i = 0; i < memberCount; i++)
            {
                if (memberList[i].Equals(name))
                    break;
            }
            for (; i < memberCount; i++)
            {
                memberList[i] = memberList[i + 1];
            }
        }

        public void createRoom(string name, int type, string password, string memberId)
        {
            this.name = name;
            this.type = type;
            this.password = password;
            memberCount = 1;
            memberList[0] = memberId;
        }

        public string[] getRoomInfo()
        {
            string[] roomInfo = new string[11];
            roomInfo[0] = name;
            roomInfo[1] = type.ToString();
            roomInfo[2] = password;
            roomInfo[3] = memberCount.ToString();
            for(int i = 0; i < memberList.Length; i++)
            {
                roomInfo[i+4] = memberList[i - 5];
            }
            return roomInfo;
        }
    }
}