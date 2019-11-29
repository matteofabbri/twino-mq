using Twino.Client.Connectors;
using Twino.Protocols.TMQ;

namespace Twino.Client.TMQ.Connectors
{
    /// <summary>
    /// Necessity connector for TMQ protocol.
    /// </summary>
    public class TmqNecessityConnector : NecessityConnector<TmqClient, TmqMessage>
    {
    }
}