using System;
using System.Threading;
using System.Threading.Tasks;
using Test.Common.Handlers;
using Twino.MQ;
using Twino.MQ.Delivery;
using Twino.MQ.Options;
using Twino.MQ.Queues;
using Twino.Server;

namespace Test.Common
{
    public class TestTwinoMQ
    {
        public TwinoMQ Server { get; private set; }

        public int OnQueueCreated { get; set; }
        public int OnQueueRemoved { get; set; }
        public int OnSubscribed { get; set; }
        public int OnUnsubscribed { get; set; }
        public int OnQueueStatusChanged { get; set; }

        public int OnReceived { get; set; }
        public int OnSendStarting { get; set; }
        public int OnBeforeSend { get; set; }
        public int OnAfterSend { get; set; }
        public int OnSendCompleted { get; set; }
        public int OnAcknowledge { get; set; }
        public int OnTimeUp { get; set; }
        public int OnAcknowledgeTimeUp { get; set; }
        public int OnRemove { get; set; }
        public int OnException { get; set; }
        public int SaveMessage { get; set; }

        public int ClientConnected { get; set; }
        public int ClientDisconnected { get; set; }

        public int Port { get; private set; }

        public bool SendAcknowledgeFromMQ { get; set; }
        
        public PutBackDecision PutBack { get; set; }


        public async Task Initialize()
        {
            TwinoMqOptions twinoMqOptions = new TwinoMqOptions();
            twinoMqOptions.AutoQueueCreation = true;
            twinoMqOptions.AcknowledgeTimeout = TimeSpan.FromSeconds(90);
            twinoMqOptions.MessageTimeout = TimeSpan.FromSeconds(12);
            twinoMqOptions.Status = QueueStatus.Broadcast;

            Server = TwinoMqBuilder.Create()
                                   .AddOptions(twinoMqOptions)
                                   .AddQueueEventHandler(new TestQueueHandler(this))
                                   .UseDeliveryHandler(d => Task.FromResult<IMessageDeliveryHandler>(new TestDeliveryHandler(this)))
                                   .AddClientHandler(new TestClientHandler(this))
                                   .AddAdminAuthorization<TestAdminAuthorization>()
                                   .Build();

            await Server.CreateQueue("broadcast-a", o => o.Status = QueueStatus.Broadcast);
            await Server.CreateQueue("push-a", o => o.Status = QueueStatus.Push);
            await Server.CreateQueue("push-a-cc", o => o.Status = QueueStatus.Push);
            await Server.CreateQueue("rr-a", o => o.Status = QueueStatus.RoundRobin);
            await Server.CreateQueue("pull-a", o => o.Status = QueueStatus.Pull);
            await Server.CreateQueue("cache-a", o => o.Status = QueueStatus.Cache);
        }

        public int Start(int pingInterval = 3, int requestTimeout = 4)
        {
            Random rnd = new Random();

            for (int i = 0; i < 50; i++)
            {
                try
                {
                    int port = rnd.Next(5000, 65000);
                    ServerOptions serverOptions = ServerOptions.CreateDefault();
                    serverOptions.Hosts[0].Port = port;
                    serverOptions.PingInterval = pingInterval;
                    serverOptions.RequestTimeout = requestTimeout;

                    TwinoServer server = new TwinoServer(serverOptions);
                    server.UseTwinoMQ(Server);
                    server.Start();
                    Port = port;
                    return port;
                }
                catch
                {
                    Thread.Sleep(2);
                }
            }

            return 0;
        }
    }
}