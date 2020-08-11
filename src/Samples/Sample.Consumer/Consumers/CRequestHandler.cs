using System;
using System.Threading.Tasks;
using Sample.Consumer.Models;
using Twino.Client.TMQ;
using Twino.Protocols.TMQ;

namespace Sample.Consumer.Consumers
{
    public class CRequestHandler : ITwinoRequestHandler<ModelC, ModelA>
    {
        public async Task<ModelA> Handle(ModelC request, TmqMessage rawMessage, TmqClient client)
        {
            Console.WriteLine("Model C consumed");
            await Task.CompletedTask;
            return null;
        }
    
        public async Task<ErrorResponse> OnError(Exception exception, ModelC request, TmqMessage rawMessage, TmqClient client)
        {
            throw new NotImplementedException();
        }
    }
}