using Newtonsoft.Json;
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

        public static string NodeProjectLocation { get; } = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\QWest.Web";

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

        [Serializable]
        public class EmailArgument {
            [JsonProperty("from")]
            public string From { get; }
            [JsonProperty("to")]
            public string To { get; }
            [JsonProperty("subject")]
            public string Subject { get; }
            [JsonProperty("html")]
            public string Html { get; }

            public EmailArgument(string to, string subject, string html) :
                this("qwestbsns@gmail.com", to, subject, html) {

            }

            public EmailArgument(string to, IEnumerable<string> subject, string html) :
                this(to, string.Join(", ", subject.ToArray()), html) {

            }

            public EmailArgument(string from, string to, string subject, string html) {
                From = from;
                To = to;
                Subject = subject;
                Html = html;
            }

            public EmailArgument(string from, string to, IEnumerable<string> subject, string html) :
                this(from, to, string.Join(", ", subject.ToArray()), html) {

            }
        }

        public static Task SendEmail(EmailArgument email) {
            string emailString = JsonConvert.SerializeObject(JsonConvert.SerializeObject(email));
            return Shell($"npm run send_email {emailString}", NodeProjectLocation);
        }
    }
}
