using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvalonServer
{
    class OpcodeAnalysor
    {
        enum FormNumber : int { Login, Lobby, Game, playerInfo = 8, Shutdown }; // 통신 규약에 따른 각 폼의 번호
        string dataString;


        public OpcodeAnalysor(byte[] data) {
            dataString = Encoding.UTF8.GetString(data).Trim('\0');
        }


        /// <summary>
        /// 수신 데이터를 각 영역별로 나눠 저장한다.
        /// </summary>
        /// <returns>각 영역별로 나뉜 데이터를 저장한 객체 반환</returns>
        public CommunicationForm separateOpcode()
        {
            Console.WriteLine("<received data>");
            Console.WriteLine(dataString + "\n");
            CommunicationForm form;
            FormNumber formNumber = (FormNumber)(dataString[0] - 48);

            // opcode 종류 확인
            switch (formNumber)
            {
                case FormNumber.Login:
                    form = new LoginForm();
                    break;
                case FormNumber.Lobby:
                    form = new LobbyForm();
                    break;
                case FormNumber.Game:
                    form = new LoginForm();
                    break;
                case FormNumber.Shutdown:
                    form = new ShutdownForm();
                    break;
                case FormNumber.playerInfo:
                    form = new PlayerInfoProcess();
                    break;
                default:
                    form = new LoginForm();
                    break;

            }

            // 수신 데이터에서 opcode를 분할한다
            form.opcode = int.Parse(dataString.Substring(1, 2));

            // 수신 데이터 에서 argument 갯수 정보를 분할한다.
            form.argumentCount = int.Parse(dataString.Substring(3, 2));

            Console.WriteLine("<separeated data>");
            Console.WriteLine("form number : {0}\nopcode : {1}\nargumentCount : {2}", form.formNumber, form.opcode, form.argumentCount);


            // 수신 데이터에서 argument를 분할한다.
            form.argumentList = divideArgument();

            
            return form;
        }

        /// <summary>
        /// 수신 데이터 내에 opcode에 따라 argument 분할
        /// </summary>
        /// <returns>분할된 argument 리스트 반환</returns>
        string[] divideArgument()
        {
            string argument = dataString.Substring(5);
            string[] argumentList;
            int argumentCount = int.Parse(dataString.Substring(3, 2));
            if (argumentCount == 0)
            {
                return null;
            }
            argumentList = argument.Split('\u0001');
            Console.WriteLine("argument : {0}\n", argument);
            
            if (argumentCount != argumentList.Length)
            {
                throw new ArgumentException();
            }

            return argumentList;
        }
    }
}
