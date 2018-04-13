using System;
using System.Fabric;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.Mocks.RemotingAbstraction;

namespace ServiceFabric.Mocks.RuntimeAbstraction
{
    /// <summary>
    /// Helps mock away the runtime for services.
    /// </summary>
    public static class StatefulServiceReplicaAdapterFactory
    {
        /// <summary>
        /// Creates, initializes and returns a new instance of StatefulServiceReplicaAdapter.
        /// </summary>
        /// <returns></returns>
        public static async Task<object> CreateAndInitialize(StatefulServiceContext context, StatefulServiceBase service)
        {
            //Get type for StatefulServiceReplicaAdapter
            var systemAssm = Assembly.Load("Microsoft.ServiceFabric.Services");
            Type adapterType = systemAssm.GetType("Microsoft.ServiceFabric.Services.Runtime.StatefulServiceReplicaAdapter", true);

            //new up internal StatefulServiceReplicaAdapter(StatefulServiceContext context, IStatefulUserServiceReplica userServiceReplica)
            var instance = (IStatefulServiceReplica)adapterType
                .GetConstructors(Constants.InstanceNonPublic)
                .Single()
                .Invoke(new object[] { context, service });
            
            //init
            instance.Initialize(CreateStatefulServiceInitializationParameters());

            //open
            var partition = new MockStatefulServicePartition();

            await instance
                .OpenAsync(ReplicaOpenMode.New, partition, CancellationToken.None)
                .ConfigureAwait(false);

            await instance
                .ChangeRoleAsync(ReplicaRole.Primary, CancellationToken.None)
                .ConfigureAwait(false);
            
            return instance;
        }

        /// <summary>
        /// Creates a new instance of <see cref="StatefulServiceInitializationParameters"/>.
        /// </summary>
        /// <returns></returns>
        public static StatefulServiceInitializationParameters CreateStatefulServiceInitializationParameters()
        {
            return (StatefulServiceInitializationParameters)typeof(StatefulServiceInitializationParameters)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .First()
                .Invoke(new object[0]);
        }
    }
}
