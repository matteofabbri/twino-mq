using System;
using System.Threading.Tasks;
using Twino.MQ;
using Twino.MQ.Clients;
using Twino.MQ.Queues;

namespace QueryCommandSample.Server
{
	public class ClientHandler : IClientHandler
	{
		public Task Connected(TwinoMQ server, MqClient client)
		{
			Console.WriteLine($"[TYPE:{client.Type}][ID:{client.UniqueId}] CONNECTED");
			return Task.CompletedTask;
		}

		public Task Disconnected(TwinoMQ server, MqClient client)
		{
			Console.WriteLine($"[TYPE:{client.Type}][ID:{client.UniqueId}] DISCONNECTED");
			return Task.CompletedTask;
		}
	}

	public class QueueEventHandler : IQueueEventHandler
	{
		public Task OnCreated(TwinoQueue queue)
		{
			Console.WriteLine($"QUEUE CREATED [{queue.Name}]");
			return Task.CompletedTask;
		}

		public Task OnRemoved(TwinoQueue queue)
		{
			Console.WriteLine($"QUEUE REMOVED [{queue.Name}]");
			return Task.CompletedTask;
		}

		public Task OnConsumerSubscribed(QueueClient client)
		{
			Console.WriteLine($"[TYPE:{client.Client.Type}][ID:{client.Client.UniqueId}] SUBSCRIBED TO [{client.Queue.Name}]");
			return Task.CompletedTask;
		}

		public Task OnConsumerUnsubscribed(QueueClient client)
		{
			Console.WriteLine($"[TYPE:{client.Client.Type}][ID:{client.Client.UniqueId}] UNSUBSCRIBED FROM [{client.Queue.Name}]");
			return Task.CompletedTask;
		}

		public Task OnStatusChanged(TwinoQueue queue, QueueStatus @from, QueueStatus to)
		{
			Console.WriteLine($"[{queue.Name}] STATUS CHANGED {@from} TO {to} ");
			return Task.CompletedTask;
		}
	}
}