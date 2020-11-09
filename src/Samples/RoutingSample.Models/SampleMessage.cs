﻿using Twino.MQ.Client.Annotations;
using Twino.MQ.Client.Models;
using Twino.Protocols.TMQ;

namespace RoutingSample.Models
{
	[RouterName("SAMPLE-MESSAGE-ROUTER")]
	[QueueName("SAMPLE-MESSAGE-QUEUE")] // FOR QUEUE PUSH
	[ContentType(1001)]                 // FOR DIRECT PUSH
	[QueueStatus(MessagingQueueStatus.Push)]
	[Acknowledge(QueueAckDecision.WaitForAcknowledge)]
	public class SampleMessage
	{
		public string Content { get; set; }
	}
}