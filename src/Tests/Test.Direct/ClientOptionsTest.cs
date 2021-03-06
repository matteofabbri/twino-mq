using System.IO;
using System.Text;
using System.Threading.Tasks;
using Test.Common;
using Test.Common.Models;
using Twino.MQ.Client;
using Twino.Protocols.TMQ;
using Xunit;

namespace Test.Direct
{
    public class ClientOptionsTest
    {
        /// <summary>
        /// If true, every message must have an id even user does not set
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UseUniqueMessageId(bool enabled)
        {
            TestTwinoMQ server = new TestTwinoMQ();
            await server.Initialize();
            server.Server.Options.UseMessageId = enabled;
            server.Server.FindQueue("push-a").Options.UseMessageId = false;
            int port = server.Start();

            TmqClient client = new TmqClient();
            client.UseUniqueMessageId = false;

            await client.ConnectAsync("tmq://localhost:" + port);
            Assert.True(client.IsConnected);

            TwinoResult joined = await client.Queues.Subscribe("push-a", true);
            Assert.Equal(TwinoResultCode.Ok, joined.Code);
            await Task.Delay(250);

            TwinoMessage received = null;
            client.MessageReceived += (c, m) => received = m;

            QueueMessageA a = new QueueMessageA("A");
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(a);
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(serialized));
            TwinoResult sent = await client.Queues.Push("push-a", ms, false);
            Assert.Equal(TwinoResultCode.Ok, sent.Code);

            await Task.Delay(1000);

            Assert.NotNull(received);

            if (enabled)
                Assert.NotNull(received.MessageId);
            else
                Assert.Null(received.MessageId);
        }

        /// <summary>
        /// Subscribes message received event of TmqClient.
        /// Sends a message and waits for response.
        /// If catching response is enabled, response message should trigger message received event.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CatchResponseMessages(bool enabled)
        {
            TestTwinoMQ server = new TestTwinoMQ();
            await server.Initialize();
            int port = server.Start();

            TmqClient client1 = new TmqClient();
            TmqClient client2 = new TmqClient();

            client1.ClientId = "client-1";
            client2.ClientId = "client-2";
            client1.CatchResponseMessages = enabled;

            await client1.ConnectAsync("tmq://localhost:" + port);
            await client2.ConnectAsync("tmq://localhost:" + port);

            Assert.True(client1.IsConnected);
            Assert.True(client2.IsConnected);

            bool responseCaught = false;
            client1.MessageReceived += (c, m) => responseCaught = true;
            client2.MessageReceived += async (c, m) =>
            {
                TwinoMessage rmsg = m.CreateResponse(TwinoResultCode.Ok);
                rmsg.SetStringContent("Response!");
                await ((TmqClient) c).SendAsync(rmsg);
            };

            TwinoMessage msg = new TwinoMessage(MessageType.DirectMessage, "client-2");
            msg.WaitResponse = true;
            msg.SetStringContent("Hello, World!");

            TwinoMessage response = await client1.Request(msg);
            await Task.Delay(500);
            Assert.NotNull(response);
            Assert.Equal(msg.MessageId, response.MessageId);
            Assert.Equal(enabled, responseCaught);
        }
    }
}