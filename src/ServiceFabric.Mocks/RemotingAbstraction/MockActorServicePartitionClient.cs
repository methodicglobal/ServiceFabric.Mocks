using System;
using System.Fabric;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.V2.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;

namespace ServiceFabric.Mocks.RemotingAbstraction
{
    /// <summary>
    /// Mock implementation of <see cref="IActorServicePartitionClient"/>
    /// </summary>
    public class MockActorServicePartitionClient : IActorServicePartitionClient
    {
        /// <summary>
        /// Gets or sets the <see cref="ResolvedServicePartition"/> to return from <see cref="TryGetLastResolvedServicePartition"/>.
        /// </summary>
        public ResolvedServicePartition ServicePartitionToResolve { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ICommunicationClientFactory{IServiceRemotingClient}" /> to  return from <see cref="Factory"/>
        /// </summary>
        public ICommunicationClientFactory<IServiceRemotingClient> CommunicationClientFactoryToReturn { get; set; }


        public MockActorServicePartitionClient(ResolvedServicePartition servicePartitionToResolve, ICommunicationClientFactory<IServiceRemotingClient> communicationClientFactoryToReturn)
        {
            ServicePartitionToResolve = servicePartitionToResolve;
            CommunicationClientFactoryToReturn = communicationClientFactoryToReturn;
        }

        public bool TryGetLastResolvedServicePartition(out ResolvedServicePartition resolvedServicePartition)
        {
            resolvedServicePartition = ServicePartitionToResolve;
            return ServicePartitionToResolve != null;
        }

        public Uri ServiceUri { get; set; }

        public ServicePartitionKey PartitionKey { get; set; }

        public TargetReplicaSelector TargetReplicaSelector { get; set; }

        public string ListenerName { get; set; }

        public ICommunicationClientFactory<IServiceRemotingClient> Factory { get; set; }

        public ActorId ActorId { get; set; }
    }


}
