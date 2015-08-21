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
    }
}
