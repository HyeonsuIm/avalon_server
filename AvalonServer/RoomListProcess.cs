using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvalonServer
{
    class RoomListProcess
    {
        RoomListInfo roomListInfo;

        public RoomListProcess(RoomListInfo roomListInfo)
        {
            this.roomListInfo = roomListInfo;
        }

        public int findRoomNumber(int findNumber)
        {
            int num;
            for (num = 0; num < roomListInfo.roomInfo.Length; num++)
            {
                if (roomListInfo.roomInfo[num].getNumber() == findNumber)
                    break;
            }

            if (num == roomListInfo.roomInfo.Length)
                return -1;
            else
                return num;
            
        }

        public int[] getMemberIndexList(int number)
        {
            RoomInfo roomInfo = roomListInfo.roomInfo[number];

            int[] memberList = roomInfo.getMemberIndexList();
            return memberList;
        }

        public void setRoom(int type, string name, string password, int roomNumber)
        {
            roomListInfo.roomInfo[roomNumber].setRoom(type, name, password);
        }

        public void removeRoom(int number)
        {
            roomListInfo.roomInfo[number] = null;
            roomListInfo.roomNumberUsed[number] = false;

            int count = 0;
            int roomListMax = roomListInfo.getRoomCount() / 20 * 20;
            do
            {
                for (int i = roomListMax; i < roomListMax + 20; i++)
                {
                    if (roomListInfo.roomNumberUsed[i] == true)
                        count++;
                }
                if (count == 0)
                {
                    roomListInfo.roomMaxSize -= RoomListInfo.IDwidth;
                    Array.Resize<RoomInfo>(ref roomListInfo.roomInfo, roomListInfo.roomMaxSize);
                    Array.Resize<bool>(ref roomListInfo.roomNumberUsed, roomListInfo.roomMaxSize);
                }
            }while(count == 0);
        }
    }
}
