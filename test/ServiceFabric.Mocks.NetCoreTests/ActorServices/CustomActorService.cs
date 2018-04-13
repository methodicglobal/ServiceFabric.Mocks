using System;
using System.Collections.Generic;
using System.Fabric;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using ServiceFabric.Mocks.NetCoreTests.ServiceRemoting;
using ServiceFabric.Mocks.RemotingAbstraction;

namespace ServiceFabric.Mocks.NetCoreTests.ActorServices
{
    /// <summary>
    /// A custom <see cref="ActorService"/> that adds nothing much.
    /// </summary>
    public class CustomActorService : ActorService
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="actorTypeInfo"></param>
        /// <param name="actorFactory"></param>
        /// <param name="stateManagerFactory"></param>
        /// <param name="stateProvider"></param>
        /// <param name="settings"></param>
        public CustomActorService(StatefulServiceContext context,
            ActorTypeInformation actorTypeInfo,
            Func<ActorService, ActorId, ActorBase> actorFactory = null,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null)
            : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {

        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateMockServiceReplicaListener();
        }

        protected override Task OnChangeRoleAsync(ReplicaRole newRole, CancellationToken cancellationToken)
        {
            var actorManagerAdapter = typeof(ActorService)
                .GetField("actorManagerAdapter", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(this);

            actorManagerAdapter.GetType()
                .GetMethod("OpenAsync")
                .Invoke(actorManagerAdapter, new object[] { Partition, cancellationToken });

            typeof(ActorService)
                .GetField("replicaRole", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(this, newRole);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// A custom <see cref="ActorService"/> with a custom constructor.
    /// </summary>
    public class AnotherCustomActorService : ActorService
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="dummy">Additional ctor parameter, so the default parameter set doesn't fit.</param>
        /// <param name="context"></param>
        /// <param name="actorTypeInfo"></param>
        /// <param name="actorFactory"></param>
        /// <param name="stateManagerFactory"></param>
        /// <param name="stateProvider"></param>
        /// <param name="settings"></param>
        public AnotherCustomActorService(object dummy, StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorService, ActorId, ActorBase> actorFactory = null, Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null) : base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
        }
    }
}
