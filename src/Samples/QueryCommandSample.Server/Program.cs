using System;
using Twino.MQ;

namespace QueryCommandSample.Server
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			TwinoMQ mq = TwinoMqBuilder.Create()
									   .UseJustAllowDeliveryHandler()
									   .Build();
		}
	}
}