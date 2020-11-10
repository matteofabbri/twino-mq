using System;
using Twino.MQ;
using Twino.Server;

namespace QueryCommandSample.Server
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			TwinoMQ mq = TwinoMqBuilder.Create()
									   .AddClientHandler<ClientHandler>()
									   .AddQueueEventHandler<QueueEventHandler>()
									   .UseJustAllowDeliveryHandler()
									   .Build();


			TwinoServer server = new TwinoServer();
			server.UseTwinoMQ(mq);
			server.Run();
		}
	}

	internal static class TwinoMQExtensions
	{
		public static void AddRoute(this TwinoMQ mq)
		{
			
		}
	}
}