using NativeMessaging;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NativeMessagingTests
{
    public class Tests
    {
        [Test]
        public async Task CanList()
        {
            IEnumerable<Host> res = await Host.GetAvailableHosts();
        }

        private class TestMessage
        {
            string id = "Hi";
        }

        [Test]
        public async Task EchoTest()
        {
            var client = await Client.GetByName("com.google.chrome.example.echo");
            await client.SendMessage(new TestMessage());
            await Task.Delay(1000);
            object c = await client.ReadMessage<object>();
            object c1 = await client.ReadMessage<object>();
        }
    }
}