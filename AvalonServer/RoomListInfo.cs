﻿using System;
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
        const int IDwidth = 20;

        // 방 최대 개수
        int roomMaxSize;

        // 현재 방 개수
        int roomCount;

        // 방 정보
        public RoomInfo[] roomInfo;

        // 번호 사용 여부
        bool[] roomNumberUsed;
        
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
        public void addRoom(int type, string name, string password, int memberIndex, string memberId, int maxPerson)
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
            roomInfo[number].createRoom(name, type, password, memberIndex, memberId, maxPerson, number);
            roomCount++;
        }

        public void comeInRoom(int memberIndex, string userId, int roomNumber, string password)
        {
            roomInfo[roomNumber].addUser(memberIndex, userId, password);
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
        string[] memberList;
        int[] memberIndexList;

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
            return memberIndexList;
        }

        public void addUser(int memberIndex, string userId, string password)
        {
            if (this.password.Equals(password))
            {
                memberIndexList[memberCount] = memberIndex;
                memberList[memberCount++] = userId;
            }
            else
            {
                throw new Exception("room full");
            }
        }

        public void removeUser(int memberIndex)
        {
            int i;
            for (i = 0; i < memberCount; i++)
            {
                if (memberIndexList[i] == memberIndex)
                    break;
            }
            for (; i < memberCount; i++)
            {
                memberIndexList[i] = memberIndexList[i + 1];
                memberList[i] = memberList[i + 1];
            }
        }

        public void createRoom(string name, int type, string password, int memberIndex, string memberId, int maxPerson, int number)
        {
            num = number;
            this.maxPerson = maxPerson;
            memberList = new string[maxPerson];
            this.name = name;
            this.type = type;
            this.password = password;
            memberCount = 1;
            memberIndexList[0] = memberIndex;
            memberList[0] = memberId;
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
            for (int i = 0; i < memberList.Length; i++)
            {
                roomInfo[i + 6] = memberList[i];
            }
            return roomInfo;
        }
    }
}