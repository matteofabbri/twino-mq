using System;

namespace Twino.MQ.Client.Connectors
{
    /// <summary>
    /// Absolute connector for TMQ protocol.
    /// </summary>
    public class TmqAbsoluteConnector : TmqStickyConnector
    {
        /// <summary>
        /// Creates new absolute connector for TMQ protocol clients
        /// </summary>
        public TmqAbsoluteConnector(TimeSpan reconnectInterval, Func<TmqClient> createInstance = null)
            : base(reconnectInterval, createInstance)
        {
        }
    }
}