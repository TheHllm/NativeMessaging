using NeoSmart.AsyncLock;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace NativeMessaging
{
    public class Client : IDisposable
    {
        private Process prog;
        private AsyncLock Lock = new();
        public Client(Host host)
        {
            prog = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    FileName = host.ExecutablePath,
                    CreateNoWindow = true,
                }
            };
            prog.Start();
        }

        public static async Task<Client> GetByName(string name)
        {
            return new Client(await Host.GetByName(name));
        }

        public async Task SendMessage(object message)
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(message);

            await prog.StandardInput.BaseStream.WriteAsync(BitConverter.GetBytes(bytes.Length));
            await prog.StandardInput.BaseStream.WriteAsync(bytes);
            await prog.StandardInput.BaseStream.FlushAsync();
        }

        public async Task<T> ReadMessage<T>()
        {
            using (IDisposable l = await Lock.LockAsync()) ;

            byte[] bLen = new byte[4];
            await prog.StandardOutput.BaseStream.ReadAsync(bLen, 0, 4);
            int len = BitConverter.ToInt32(bLen);
            byte[] msg = new byte[len];
            await prog.StandardOutput.BaseStream.ReadAsync(msg, 0, len);

            return JsonSerializer.Deserialize<T>(msg);
        }

        void IDisposable.Dispose()
        {
            prog.Kill();
            prog.WaitForExit();
        }
    }
}
