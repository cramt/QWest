using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utilities {
    public static class Utilities {

#if DEBUG
        public const bool DebugMode = true;
#else
        public const bool DebugMode = false;
#endif
        public static Process DynamicShell(string command, Action<string> onStdOut, string cwd = null) {
            if (cwd == null) {
                cwd = Directory.GetCurrentDirectory();
            }
            Process process = new Process {
                StartInfo = new ProcessStartInfo {
                    WorkingDirectory = cwd,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    FileName = @"C:\Windows\System32\cmd.exe",
                    Verb = "runas",
                    Arguments = "/c " + command,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                onStdOut(e.Data);
            };
            process.Start();
            process.BeginOutputReadLine();
            return process;
        }

        public static Task<string> Shell(string command, string cwd = null) {
            if (cwd == null) {
                cwd = Directory.GetCurrentDirectory();
            }
            return Task.Factory.StartNew(() => {
                Process process = new Process {
                    StartInfo = new ProcessStartInfo {
                        WorkingDirectory = cwd,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        FileName = @"C:\Windows\System32\cmd.exe",
                        Verb = "runas",
                        Arguments = "/c " + command,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };
                process.Start();
                process.WaitForExit();
                return process.StandardOutput.ReadToEnd();
            });
        }

        public static async Task KillOnPort(uint port) {
            Regex regex = new Regex(@"/\s+\S+\s+\S+\s+\S+\s+\S+\s+(\S+)/");
            List<string> shellOutput = (await Shell("netstat -ano | find \"LISTENING\" | find \"" + port + "\"")).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            await Task.WhenAll(
            shellOutput.Select(x => {
                var matches = regex.Matches(x);
                if (matches.Count == 0) {
                    return null;
                }
                return matches[0];
            }).Where(x => x != null).Select(x => x.Value).Distinct().Select(x => Shell("taskkill /F /pid " + x)));
        }

        public static void SendEmail(string toAddress, string subject, string body) {
            MailMessage mail = new MailMessage();
            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
            mail.From = new MailAddress("no_response@qwest.com");
            mail.To.Add(toAddress);
            mail.Subject = subject;
            mail.Body = body;
            smtpServer.Port = 587;
            smtpServer.Credentials = new NetworkCredential(Config.Config.Instance.GmailUsername, Config.Config.Instance.GmailPassword);
            smtpServer.EnableSsl = true;
            smtpServer.Send(mail);
        }
    }
}
