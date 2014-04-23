using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExemploBdd.Testes.Funcionais
{
    public class IISExpressDriver : ProcessDriver
    {
        public string Url { get; private set; }

        public void Iniciar(string caminhoApplicationhostconfig)
        {
            Process.GetProcessesByName("iisexpress").ToList().ForEach(p => p.Kill());
            const string processFileName = @"c:\program files\IIS Express\IISExpress.exe";
            IniciarProcesso(processFileName, @"/systray:false /trace:none /config:" + caminhoApplicationhostconfig);
            var match = EsperarOutputConsole(@"Successfully registered URL ""([^""]*)""");
            Url = match.Groups[1].Value;
        }

        protected override void Finalizar()
        {
            try { Process.Kill(); }
            catch {}
            if (!Process.WaitForExit(10000)) throw new Exception("IISExpress did not halt within 10 seconds.");
            Process.Dispose();
        }
    }

    public abstract class ProcessDriver : CriticalFinalizerObject, IDisposable
    {
        protected BlockingCollection<string> Error = new BlockingCollection<string>(new ConcurrentQueue<string>());
        protected BlockingCollection<string> Input = new BlockingCollection<string>(new ConcurrentQueue<string>());
        protected BlockingCollection<string> Output = new BlockingCollection<string>(new ConcurrentQueue<string>());
        protected Process Process;
        private Task errorTask;
        private Task inputTask;
        private Task outputTask;
        private bool shouldStopWaitingForInput;

        public void Dispose()
        {
            Dispose(true);
        }

        protected void IniciarProcesso(string exePath, string arguments = "")
        {
            var psi = new ProcessStartInfo(exePath)
            {
                LoadUserProfile = false,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            Process = Process.Start(psi);

            outputTask = Task.Factory.StartNew(() =>
            {
                string line;
                while ((line = Process.StandardOutput.ReadLine()) != null) Output.Add(line);
            });

            errorTask = Task.Factory.StartNew(() =>
            {
                string line;
                while ((line = Process.StandardError.ReadLine()) != null) Error.Add(line);
            });

            inputTask = Task.Factory.StartNew(() =>
            {
                while (Process != null && Process.HasExited == false)
                {
                    string input;
                    if (Input.TryTake(out input, 2000)) if (input != null) Process.StandardInput.WriteLine(input);
                    if (shouldStopWaitingForInput) break;
                }
            });
        }

        protected virtual void Finalizar() {}

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                shouldStopWaitingForInput = true;
                Output.Dispose();
                Error.Dispose();
                if (Process != null)
                {
                    Finalizar();
                    outputTask.Wait();
                    errorTask.Wait();
                    inputTask.Wait();
                }
                Input.Dispose();
            }
        }

        ~ProcessDriver()
        {
            Dispose(false);
        }

        protected Match EsperarOutputConsole(string pattern, int msMaxWait = 10000, int msWaitInterval = 500)
        {
            var t = DateTime.UtcNow;

            var sb = new StringBuilder();
            Match match;
            while (true)
            {
                string nextLine;
                Output.TryTake(out nextLine, 100);

                if (nextLine == null)
                {
                    if ((DateTime.UtcNow - t).TotalMilliseconds > msMaxWait) throw new TimeoutException("Timeout waiting for regular expression " + pattern + Environment.NewLine + sb);

                    continue;
                }

                sb.AppendLine(nextLine);

                match = Regex.Match(nextLine, pattern);

                if (match.Success) break;
            }
            return match;
        }
    }
}