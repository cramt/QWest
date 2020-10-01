using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Utilities {
    public static class Utilities {
        public static Task DynamicShell(string command, Action<string> onStdOut, string cwd = null) {
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
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                    onStdOut(e.Data);
                };
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            });
        }

        public static Task<string> Shell(string command, string cwd = null) {
            if(cwd == null) {
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
    }
}
