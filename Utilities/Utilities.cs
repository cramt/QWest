using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utilities {
    public static class Utilities {

#if DEBUG
        public const bool DebugMode = true;
#else
        public const bool DebugMode = false;
#endif

        public static string SolutionLocation { get; } = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent.Parent.Parent.FullName;

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
            List<string> shellOutput = (await Shell("netstat -ano | find \"LISTENING\" | find \"" + port + "\"")).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            await Task.WhenAll(
            shellOutput.Select(x => {
                List<char> listChars = x.ToCharArray().ToList();
                Stack<char> chars = new Stack<char>(collection: listChars);
                List<char> pid = new List<char>();
                try {
                    while (chars.Peek() == ' ') {
                        chars.Pop();
                    }
                    while (chars.Peek() != ' ') {
                        pid.Add(chars.Pop());
                    }
                }
                catch (InvalidOperationException) {
                    return null;
                }
                pid.Reverse();
                return new string(pid.ToArray());

            }).Where(x => x != null).Distinct().Select(x => Shell("taskkill /F /pid " + x)));
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

        public static async Task SendEmail(EmailArgument email) {
            string emailString = JsonConvert.SerializeObject(email);
            string url = $"http://localhost:{Config.Config.Instance.EmailPort}/send";
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PostAsync(url,
                new StringContent(emailString, Encoding.UTF8, "application/json")
            );
        }

        public static DateTime ToDate(this uint u) {
            return Config.Config.Instance.StartDate.AddSeconds(u);
        }

        public static uint ToUint(this DateTime date) {
            return (uint)date.Subtract(Config.Config.Instance.StartDate).TotalSeconds;
        }

        public static uint ToUnsigned(this int i) {
            return unchecked((uint)i);
        }

        public static int ToSigned(this uint u) {
            return unchecked((int)u);
        }
    }
}
