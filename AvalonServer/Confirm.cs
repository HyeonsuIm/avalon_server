using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AvalonServer
{
    class Confirm
    {
        private int confirmNum; // 비밀 번호 저장
        string Email;           // 인증 보낼 주소
        string EmailHead;       // 이메일 제목
        string EmailBody;       // 이메일 내용1
        string EmailBody2;      // 이메일 내용2      -> 내용1 + code + 내용2

        bool authorized = false;

        string SmtpID = "rikuo777@gmail.com";
        string SmtpPW = "hayate123";

        public Confirm(string Email)
        {
            this.Email = Email;
            EmailHead = "Avalon 비밀번호 찾기";
            EmailBody = "회원가입해 주셔서 감사합니다. </br> 가입을 위한 인증번호는 다음과 같습니다. </br>";
            EmailBody2 = "</br>본 이메일은 발신 전용 메일입니다. 관련된 문의는 ~~로 부탁드립니다.";

            Send();
        }

        private void ReSent_Click(object sender, EventArgs e)
        {
            Send();
        }

        private void Send()
        {

            Random rand = new Random();
            confirmNum = rand.Next(100000000, 999999999);   // 9자리의 난수입니다.

            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(SmtpID, SmtpPW),
                EnableSsl = true,
                Timeout = 20000,
            };

            MailMessage mail = new MailMessage();
            mail.BodyEncoding = Encoding.UTF8;
            mail.SubjectEncoding = Encoding.UTF8;
            mail.IsBodyHtml = true;                         // 메일 본문 내용을 html로 설정.
            mail.From = new MailAddress(SmtpID);            // 보내는 사람
            mail.To.Add(new MailAddress(Email));            // 받는 사람
            mail.Subject = EmailHead;
            mail.Body = EmailBody + confirmNum.ToString() + EmailBody2;
            
            try
            {
                smtp.Send(mail);
            }
            catch (Exception ep)
            {
                Console.WriteLine("전송에 실패하였습니다." + ep.Message);
                return;
            }
            finally
            {
                mail.Dispose();
                Console.WriteLine("전송하였습니다.");
            }
        }

        public int getPassword(){
            return confirmNum;
        }

        public bool IsAuthorized()
        {
            return authorized;
        }
    }
}
