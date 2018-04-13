using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace ServiceFabric.Mocks.RemotingAbstraction
{
    /// <summary>
    /// Mock implementation of <see cref="IServiceRemotingListener"/>
    /// </summary>
    public class MockCommunicationListener : FabricTransportServiceRemotingListener
    {
        public MockCommunicationListener(ServiceContext serviceContext, 
            IService serviceImplementation, 
            FabricTransportRemotingListenerSettings remotingListenerSettings = null, 
            IServiceRemotingMessageSerializationProvider serializationProvider = null) 
            : base(serviceContext, serviceImplementation, remotingListenerSettings, serializationProvider)
        {
        }

        public MockCommunicationListener(ServiceContext serviceContext, 
            IServiceRemotingMessageHandler serviceRemotingMessageHandler, 
            FabricTransportRemotingListenerSettings remotingListenerSettings = null, 
            IServiceRemotingMessageSerializationProvider serializationProvider = null) 
            : base(serviceContext, serviceRemotingMessageHandler, remotingListenerSettings, serializationProvider)
        {
            RemotingMessageHandler = serviceRemotingMessageHandler;
        }

       

        //public bool IsOpen { get; private set; } 

        //public bool IsAborted { get; private set; }

        public IServiceRemotingMessageHandler RemotingMessageHandler { get; }

        //public MockCommunicationListener(MockServiceRemotingMessageHandler remotingMessageHandler)
        //{
        //    RemotingMessageHandler = remotingMessageHandler ?? throw new ArgumentNullException(nameof(remotingMessageHandler));
        //}

        //public Task<string> OpenAsync(CancellationToken cancellationToken)
        //{
        //    IsOpen = true;
        //    return Task.FromResult("mockendpoint:/");
        //}

        //public Task CloseAsync(CancellationToken cancellationToken)
        //{
        //    IsOpen = false;
        //    return Task.FromResult(true);
        //}

        //public void Abort()
        //{
        //    IsAborted = true;
        //    IsOpen = false;
        //}
    }
}