using System;
using System.Threading;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;

namespace ServiceFabric.Mocks.RemotingAbstraction
{
    /// <summary>
    /// Mock implementation of <see cref="IServiceRemotingResponseMessage"/>
    /// </summary>
    public class MockServiceRemotingResponseMessage : IServiceRemotingResponseMessage
    {
        public IServiceRemotingResponseMessageHeader Header { get; set; }

        public IServiceRemotingResponseMessageBody Body { get; set; }

        public IServiceRemotingResponseMessageHeader GetHeader()
        {
            IServiceRemotingPartitionClient client = (IServiceRemotingPartitionClient)GetType()
                .GetProperty("ServicePartitionClient2",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .GetValue(this);

            var commClient = client.Factory.GetClientAsync(new Uri("endpoint:/"),
                new ServicePartitionKey(1L),
                TargetReplicaSelector.Default,
                "MockListenerName",
                new OperationRetrySettings(),
                CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

            //commClient.
            return Header;
        }

        public IServiceRemotingResponseMessageBody GetBody()
        {
            return Body;
        }
    }
}