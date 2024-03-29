﻿using NeoSmart.AsyncLock;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace NativeMessaging
{
    public class Client : IDisposable
    {
        private Process prog;
        private AsyncLock ReadLock = new AsyncLock();
        private AsyncLock WriteLock = new AsyncLock();

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
            using (var l = await ReadLock.LockAsync())
            {
                byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(message);

                var length = BitConverter.GetBytes(bytes.Length);
                await prog.StandardInput.BaseStream.WriteAsync(length, 0, length.Length);
                await prog.StandardInput.BaseStream.WriteAsync(bytes, 0, bytes.Length);
                await prog.StandardInput.BaseStream.FlushAsync();
            }
        }

        public async Task<T> ReadMessage<T>()
        {
            using (IDisposable l = await ReadLock.LockAsync())
            {
                byte[] bLen = new byte[4];
                await prog.StandardOutput.BaseStream.ReadAsync(bLen, 0, 4);
                int len = BitConverter.ToInt32(bLen, 0);
                byte[] msg = new byte[len];
                await prog.StandardOutput.BaseStream.ReadAsync(msg, 0, len);

                return JsonSerializer.Deserialize<T>(msg);
            }
        }

        void IDisposable.Dispose()
        {
            prog.Kill();
            prog.WaitForExit();
        }
    }
}
