using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;
using ServiceFabric.Mocks.RemotingAbstraction;

// ReSharper disable once CheckNamespace
namespace ServiceFabric.Mocks.RemotingV2
{
    public class MockActorServiceRemotingClientFactory : IServiceRemotingClientFactory
    {
        private readonly Dictionary<IServiceRemotingClient, ExceptionInformation> _reportedExceptionInformation = new Dictionary<IServiceRemotingClient, ExceptionInformation>();
        private readonly Dictionary<IServiceRemotingClient, OperationRetryControl> _operationRetryControls = new Dictionary<IServiceRemotingClient, OperationRetryControl>();

        /// <summary>
        /// The <see cref="IServiceRemotingMessageBodyFactory"/> v2 to return from <see cref="GetRemotingMessageBodyFactory"/>.
        /// </summary>
        public IServiceRemotingMessageBodyFactory MockServiceRemotingMessageBodyFactory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IServiceRemotingClient"/> to return from GetClientAsync
        /// </summary>
        public IServiceRemotingClient ServiceRemotingClient { get; set; }

        /// <summary>
        /// Manually invoke using <see cref="OnClientConnected"/>
        /// </summary>
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientConnected;

        /// Manually invoke using <see cref="OnClientDisconnected"/>
        public event EventHandler<CommunicationClientEventArgs<IServiceRemotingClient>> ClientDisconnected;

        /// <summary>
        /// Contains the last reported exception information.
        /// </summary>
        public IReadOnlyDictionary<IServiceRemotingClient, ExceptionInformation> ReportedExceptionInformation => _reportedExceptionInformation;

        public MockActorServiceRemotingClientFactory(IServiceRemotingClient remotingClient)
        {
            ServiceRemotingClient = remotingClient;
        }

        public Task<IServiceRemotingClient> GetClientAsync(Uri serviceUri, ServicePartitionKey partitionKey, TargetReplicaSelector targetReplicaSelector,
            string listenerName, OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            //var remotingClient = ServiceRemotingClient ?? new RemotingAbstraction.MockActorServiceRemotingClient(_wrappedService)
            //{
            //    ListenerName = listenerName,
            //    PartitionKey = partitionKey,
            //    TargetReplicaSelector = targetReplicaSelector,
            //    RetrySettings = retrySettings
            //};

            return Task.FromResult<IServiceRemotingClient>(ServiceRemotingClient);
        }

        public Task<IServiceRemotingClient> GetClientAsync(ResolvedServicePartition previousRsp, TargetReplicaSelector targetReplicaSelector,
            string listenerName, OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            ServicePartitionKey partitionKey;
            switch (previousRsp.Info.Kind)
            {
                case ServicePartitionKind.Singleton:
                    partitionKey = new ServicePartitionKey();
                    break;
                case ServicePartitionKind.Int64Range:
                    partitionKey = new ServicePartitionKey(((Int64RangePartitionInformation)previousRsp.Info).LowKey);
                    break;
                case ServicePartitionKind.Named:
                    partitionKey = new ServicePartitionKey(((NamedPartitionInformation)previousRsp.Info).Name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return GetClientAsync(previousRsp.ServiceName, partitionKey,
                targetReplicaSelector, listenerName, retrySettings, cancellationToken);

        }

        public Task<OperationRetryControl> ReportOperationExceptionAsync(IServiceRemotingClient client, ExceptionInformation exceptionInformation,
            OperationRetrySettings retrySettings, CancellationToken cancellationToken)
        {
            _reportedExceptionInformation[client] = exceptionInformation;
            OperationRetryControl operationRetryControl;
            _operationRetryControls.TryGetValue(client, out operationRetryControl);
            return Task.FromResult(operationRetryControl);
        }

        /// <summary>
        /// Triggers the event <see cref="ClientConnected"/>
        /// </summary>
        /// <param name="e"></param>
        public void OnClientConnected(CommunicationClientEventArgs<IServiceRemotingClient> e)
        {
            ClientConnected?.Invoke(this, e);
        }

        /// <summary>
        /// Triggers the event <see cref="OnClientDisconnected"/>
        /// </summary>
        /// <param name="e"></param>
        public void OnClientDisconnected(CommunicationClientEventArgs<IServiceRemotingClient> e)
        {
            ClientDisconnected?.Invoke(this, e);
        }

        /// <summary>
        /// Registers an <see cref="OperationRetryControl"/> to return from <see cref="ReportOperationExceptionAsync"/>.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="control"></param>
        public void RegisterOperationRetryControl(IServiceRemotingClient client, OperationRetryControl control)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _operationRetryControls[client] = control;
        }

        public IServiceRemotingMessageBodyFactory GetRemotingMessageBodyFactory()
        {
            return MockServiceRemotingMessageBodyFactory;
        }

    }
}