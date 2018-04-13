using System;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;
using Microsoft.ServiceFabric.Services.Runtime;

namespace ServiceFabric.Mocks.RemotingAbstraction
{
    /// <summary>
    /// Mock implementation of <see cref="IServiceRemotingClient"/> v2. (returned from <see cref="RemotingV2.MockActorServiceRemotingClientFactory"/>)
    /// Defines the interface that must be implemented to provide a client for Service Remoting communication.
    /// </summary>
    public class MockServiceRemotingClient : IServiceRemotingClient
    {
        /// <summary>
        /// Null
        /// </summary>
        public ResolvedServiceEndpoint Endpoint { get; set; }

        /// <inheritdoc />
        public string ListenerName { get; set; }

        /// <summary>
        /// Null
        /// </summary>
        public ResolvedServicePartition ResolvedServicePartition { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ServicePartitionKey"/>.
        /// </summary>
        public ServicePartitionKey PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TargetReplicaSelector"/>.
        /// </summary>
        public TargetReplicaSelector TargetReplicaSelector { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OperationRetrySettings"/>.
        /// </summary>
        public OperationRetrySettings RetrySettings { get; set; }

        /// <summary>
        /// Gets or sets the wrapped <see cref="StatefulServiceBase"/>.
        /// </summary>
        public StatefulServiceBase WrappedService { get; }
        
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="wrappedService"></param>
        public MockServiceRemotingClient(StatefulServiceBase wrappedService)
        {
            WrappedService = wrappedService ?? throw new ArgumentNullException(nameof(wrappedService));
        }


        /// <inheritdoc />
        public Task<IServiceRemotingResponseMessage> RequestResponseAsync(IServiceRemotingRequestMessage requestRequestMessage)
        {
            var listener = (MockCommunicationListener)WrappedService.InvokeCreateServiceReplicaListeners()
                .Single(l => l.Name == "MockListenerName")
                .CreateCommunicationListener(WrappedService.Context);
            return listener.RemotingMessageHandler.HandleRequestResponseAsync(null, requestRequestMessage);
        }

        /// <inheritdoc />
        public void SendOneWay(IServiceRemotingRequestMessage requestMessage)
        {
            var listener = (MockCommunicationListener)WrappedService.InvokeCreateServiceReplicaListeners()
                .Single(l => l.Name == "MockListenerName")
                .CreateCommunicationListener(WrappedService.Context);
            listener.RemotingMessageHandler.HandleOneWayMessage(requestMessage);
        }
    }
}