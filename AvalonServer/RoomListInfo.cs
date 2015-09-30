using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvalonServer
{
    [Serializable]
    public class RoomListInfo
    {
        // 방 개수 증가폭
        public const int IDwidth = 20;

        // 방 최대 개수
        public int roomMaxSize;

        // 현재 방 개수
        public int roomCount;

        // 방 정보
        public RoomInfo[] roomInfo;

        // 번호 사용 여부
        public bool[] roomNumberUsed;

        public RoomListInfo()
        {
            roomCount = 0;
            roomMaxSize = IDwidth;
            roomInfo = new RoomInfo[roomMaxSize];
            roomNumberUsed = new bool[roomMaxSize];
        }

        public bool isNull()
        {
            if (roomCount == 0)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 새로운 방을 추가
        /// </summary>
        /// <param name="name">방 제목</param>
        /// <param name="type">방 타입</param>
        /// <param name="password">방 비밀번호</param>
        /// <param name="member">방장 이름</param>
        public void addRoom(int type, string name, string password, int maxPerson, UserInfo host)
        {
            int number;
            for (number = 0; number < roomMaxSize; number++)
            {
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
            roomInfo[number].createRoom(name, type, password, maxPerson, number, host);
            roomCount++;
        }

        public int comeInRoom(int roomNumber, string password, UserInfo user)
        {
            roomInfo[roomNumber].addUser(user, password);
            return roomNumber;
        }

        public void comeOutRoom(int roomNumber, int memberIndex)
        {
            roomInfo[roomNumber].removeUser(memberIndex);
        }

        private void removeRoom(int number)
        {
            roomInfo[number] = null;
            roomNumberUsed[number] = false;
            roomCount--;
        }

        public RoomInfo[] getRoomListInfo()
        {
            return roomInfo;
        }

        public int getRoomCount()
        {
            return roomCount;
        }
    }

    [Serializable]
    public class RoomInfo
    {
        int maxPerson;
        string name;
        int type;
        int num;
        string password;
        int memberCount;
        
        //string[] memberNickList;
        //int[] memberIndexList;
        //int[] memberIPList;
        
        UserInfo[] memberInfo;


        public int getMemberCount()
        {
            return memberCount;
        }
        public int getNumber()
        {
            return num;
        }

        public int[] getMemberIndexList()
        {
            int[] IndexList = new int[memberCount];
            for (int i = 0; i < memberCount;i++)
            {
                IndexList[i] = memberInfo[i].userIndex;
            }
            return IndexList;
        }
        //매개변수 부분 고쳐야될듯
        public void addUser(UserInfo addUser, string password)
        {
            if (this.password.Equals(password))
            {
                if (memberCount < maxPerson)
                {
                    memberInfo[memberCount] = addUser;
                    
                }
                else
                    throw new Exception("인원 수 초과");
            }
            else
            {
                throw new Exception("비밀번호 에러");
            }
        }

        public void removeUser(int memberIndex)
        {
            int i;
            for (i = 0; i < memberCount; i++)
            {
                if (memberInfo[i].userIndex == memberIndex)
                    break;
            }
            for (; i < memberCount; i++)
            {
                //앞으로 땡김
                memberInfo[i] = memberInfo[i + 1];
            }
        }

        public void createRoom(string name, int type, string password, int maxPerson, int number, UserInfo host)
        {
            num = number;
            this.maxPerson = maxPerson;
            
            //memberNickList = new string[maxPerson];
            //memberIndexList = new int[maxPerson];
            memberInfo = new UserInfo[maxPerson];

            this.name = name;
            this.type = type;
            this.password = password;
            memberCount = 1;
            memberInfo[0] = host;
        }

        public string[] getRoomInfo()
        {
            string[] roomInfo = new string[16];
            roomInfo[0] = name;
            roomInfo[1] = type.ToString();
            roomInfo[2] = password;
            roomInfo[3] = memberCount.ToString();
            roomInfo[4] = maxPerson.ToString();
            roomInfo[5] = num.ToString();
            for (int i = 0; i < memberInfo.Length; i++)
            {
                roomInfo[i + 6] = memberInfo[i].userNick;
            }
            return roomInfo;
        }

        public void setRoom(int type, string name, string password)
        {
            this.type = type;
            this.name = name;
            this.password = password;
        }
    }
}