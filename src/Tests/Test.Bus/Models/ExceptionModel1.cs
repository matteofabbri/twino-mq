using Twino.MQ.Client.Annotations;
using Twino.MQ.Client.Models;

namespace Test.Bus.Models
{
    [QueueName("ex-queue-1")]
    [RouterName("ex-route-1")]
    public class ExceptionModel1 : ITransportableException
    {
        public void Initialize(ExceptionContext context)
        {
        }
    }
}