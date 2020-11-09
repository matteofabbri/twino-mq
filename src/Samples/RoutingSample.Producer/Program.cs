﻿using System;
using System.Threading.Tasks;
using RoutingSample.Models;
using Twino.MQ.Client;
using Twino.MQ.Client.Bus;
using Twino.MQ.Client.Connectors;
using Twino.Protocols.TMQ;

namespace RoutingSample.Producer
{
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			TmqStickyConnector connector = new TmqStickyConnector(TimeSpan.FromSeconds(2));
			connector.AddHost("tmq://localhost:15500");
			connector.ContentSerializer = new NewtonsoftContentSerializer();
			connector.Run();

			ITwinoRouteBus routeBus = connector.Bus.Route;

			while (true)
			{
				TwinoResult result = await routeBus.PublishJson(new SampleMessage(), true);
				Console.WriteLine($"Push: {result.Code}");
				await Task.Delay(5000);
			}
		}
	}
}