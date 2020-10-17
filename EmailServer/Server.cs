using SmtpServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EmailServer {
    public class Server {
        private static Server _instance = null;
        public static Server Instance {
            get {
                if (_instance == null) {
                    _instance = new Server();
                }
                return _instance;
            }
        }

        private Task runner;

        private Server() {
            var options = new SmtpServerOptionsBuilder()
                .ServerName("localhost")
                .Port(587)
                .UserAuthenticator(new Authenticator())
                .Build();
            runner = new SmtpServer.SmtpServer(options).StartAsync(CancellationToken.None);
        }

        public void SendMail(string toAddress, string subject, string body) {
            MailMessage mail = new MailMessage();
            SmtpClient smtpServer = new SmtpClient("localhost");
            mail.From = new MailAddress("noreply@qwest.com", "noreply");
            mail.To.Add(toAddress);
            mail.Subject = subject;
            mail.Body = body;
            smtpServer.Port = 587;
            smtpServer.Credentials = new NetworkCredential(InternalProvider.Username, InternalProvider.Password);
            smtpServer.Send(mail);

        }
    }
}
