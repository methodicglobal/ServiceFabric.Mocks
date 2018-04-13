using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceFabric.Mocks.NetCoreTests.Actors;
using ServiceFabric.Mocks.NetCoreTests.ActorServices;
using ServiceFabric.Mocks.RemotingAbstraction;
using ServiceFabric.Mocks.RuntimeAbstraction;

namespace ServiceFabric.Mocks.NetCoreTests.ServiceRemoting
{
    [TestClass]
    public class ServiceRemotingTests
    {
        [TestMethod]
        public async Task TestRemotingFactoryAsync()
        {
            var service = new CustomActorService(MockStatefulServiceContextFactory.Default, ActorTypeInformation.Get(typeof(MyStatefulActor)));
            var factory = new ServiceFabric.Mocks.RemotingV2.MockActorServiceRemotingClientFactory(new RemotingAbstraction.MockServiceRemotingClient(service));
            var client = await factory.GetClientAsync(new Uri("fabric:/App/Service"), ServicePartitionKey.Singleton,
                TargetReplicaSelector.Default, "Listener", new OperationRetrySettings(), CancellationToken.None);

            Assert.IsInstanceOfType(factory, typeof(IServiceRemotingClientFactory));
            Assert.IsInstanceOfType(client, typeof(IServiceRemotingClient));
            Assert.IsInstanceOfType(client, typeof(MockServiceRemotingClient));
            Assert.AreEqual("Listener", client.ListenerName);
        }


        [TestMethod]
        public async Task TestActorRemotingAsync()
        {
            var payload = new Payload("content");
            var serviceContext = MockStatefulServiceContextFactory.Default;
            //var service = new CustomActorService(serviceContext, ActorTypeInformation.Get(typeof(RemotingEnabledActor)), 
            //    stateProvider: new MockActorStateProvider());
            //var factory = new ServiceFabric.Mocks.RemotingV2.MockActorServiceRemotingClientFactory(new RemotingAbstraction.MockActorServiceRemotingClient(service));
            //var responseMessageBody = new MockServiceRemotingResponseMessageBody()
            //{
            //    Response = payload
            //};
            //var requestMessageBody = new MockServiceRemotingRequestMessageBody();

            //var serviceRemotingMessageBodyFactory = new MockServiceRemotingMessageBodyFactory()
            //{
            //    Request = requestMessageBody,
            //    Response = responseMessageBody
            //};

            //factory.MockServiceRemotingMessageBodyFactory = serviceRemotingMessageBodyFactory;
            //factory.ServiceRemotingClient = new RemotingAbstraction.MockActorServiceRemotingClient(service)
            //{
            //    ServiceRemotingResponseMessage = new MockServiceRemotingResponseMessage
            //    {
            //        Body = responseMessageBody,
            //        Header = new MockServiceRemotingResponseMessageHeader()
            //    }
            //};

            //MockServiceRemotingMessageHandler.Default.MockServiceRemotingMessageBodyFactory = serviceRemotingMessageBodyFactory;


            var service = new RemotingEnabledService(serviceContext, new MockReliableStateManager());
            var adapter = await StatefulServiceReplicaAdapterFactory.CreateAndInitialize(serviceContext, service);

            var factory = new ServiceFabric.Mocks.RemotingV2.MockActorServiceRemotingClientFactory(
                new RemotingAbstraction.MockServiceRemotingClient(service));

            var responseMessageBody = new MockServiceRemotingResponseMessageBody()
            {
                Response = payload
            };
            var requestMessageBody = new MockServiceRemotingRequestMessageBody();

            var serviceRemotingMessageBodyFactory = new MockServiceRemotingMessageBodyFactory()
            {
                Request = requestMessageBody,
                Response = responseMessageBody
            };

            factory.MockServiceRemotingMessageBodyFactory = serviceRemotingMessageBodyFactory;


            var proxyFactory = new ServiceProxyFactory(handler => factory);
            var proxy = proxyFactory.CreateServiceProxy<IRemotingEnabledService>(new Uri("endpoint:/"));
            await proxy.DoStuff(payload);

            Assert.IsTrue(service.DidStuff);

            //var proxyFactory = new ActorProxyFactory(callbackClient => factory);
            //var proxy = proxyFactory.CreateActorProxy<IMyStatefulActor>(ActorId.CreateRandom(), "App", "Service", "Listener");

            //typeof(ActorProxy)
            //    .GetField("servicePartitionClientV2", BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.FlattenHierarchy)
            //    .SetValue(proxy, new MockActorServicePartitionClient(null, null));

            //await proxy.InsertAsync("state", payload);

            //Assert.IsInstanceOfType(proxy, typeof(IMyStatefulActor));
        }

        
    }

    public class RemotingEnabledService : StatefulService, IRemotingEnabledService
    {
        public bool DidStuff { get; private set; }

        
        public Task DoStuff(Payload payload)
        {
            DidStuff = true;
            return Task.CompletedTask;
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateMockServiceReplicaListener();
        }

        public RemotingEnabledService(StatefulServiceContext serviceContext) : base(serviceContext)
        {
        }

        public RemotingEnabledService(StatefulServiceContext serviceContext, IReliableStateManagerReplica reliableStateManagerReplica) : base(serviceContext, reliableStateManagerReplica)
        {
        }
    }

    public static class StatefulServiceCommsExtensions
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        private static MockCommunicationListener _mockCommunicationListenerInstance;

        /// <summary>
        /// Creates a ServiceReplicaListener that holds a singleton <see cref="MockCommunicationListener"/> .
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IEnumerable<ServiceReplicaListener> CreateMockServiceReplicaListener(
            this StatefulServiceBase service)
        {
            return new[]
            {
                new ServiceReplicaListener(ctx => CreateMockCommunicationListener(service.Context), "MockListenerName")
            };
        }

        /// <summary>
        /// Returns a singleton <see cref="MockCommunicationListener"/>.
        /// </summary>
        /// <returns></returns>
        public static MockCommunicationListener CreateMockCommunicationListener(ServiceContext context)
        {
            if (_mockCommunicationListenerInstance == null)
            {
                _mockCommunicationListenerInstance = new MockCommunicationListener(context, new MockServiceRemotingMessageHandler());
            }
            return _mockCommunicationListenerInstance;
        }
    }

    public interface IRemotingEnabledService : IService
    {
        Task DoStuff(Payload payload);
    }

    public class RemotingEnabledActor : Actor, IMyStatefulActor
    {
        public Payload State { get; set; }

        public RemotingEnabledActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public Task InsertAsync(string stateName, Payload value)
        {
            State = value;
            return Task.CompletedTask;
        }
    }

    [TestClass]
    public class ActorEventTests
    {
        protected static bool IsSuccess = false;

        public interface IExampleEvents : IActorEvents
        {
            void OnSuccess(string msg);
        }

        public interface IExampleActor : IActor, IActorEventPublisher<IExampleEvents>
        {
            Task ActorSomething(string msg);
        }

        public class ExampleActorMock : Actor, IExampleActor
        {
            public ExampleActorMock(ActorService actorService, ActorId actorId) : base(actorService, actorId)
            {
            }

            public Task ActorSomething(string msg)
            {
                Debug.WriteLine("Actor:" + msg);
                var ev = GetEvent<IExampleEvents>();
                ev.OnSuccess(msg);
                return Task.FromResult(true);
            }
        }

        public interface IExampleService : IService
        {
            Task DoSomething(Guid id, string msg);
        }


        public class ExampleClient : StatefulService, IExampleService, IExampleEvents
        {
            private readonly IActorEventSubscriptionHelper _subscriptionHelper;
            private readonly IActorProxyFactory _actorProxyFactory;

            public ExampleClient(StatefulServiceContext serviceContext, IActorEventSubscriptionHelper subscriptionHelper)
                : base(serviceContext)
            {
                _subscriptionHelper = subscriptionHelper ?? new ActorEventSubscriptionHelper();
            }

            public ExampleClient(StatefulServiceContext serviceContext, IReliableStateManagerReplica2 reliableStateManagerReplica,
                IActorEventSubscriptionHelper subscriptionHelper, IActorProxyFactory actorProxyFactory)
                : base(serviceContext, reliableStateManagerReplica)
            {
                if (actorProxyFactory == null) throw new ArgumentNullException(nameof(actorProxyFactory));
                _subscriptionHelper = subscriptionHelper ?? new ActorEventSubscriptionHelper();
                _actorProxyFactory = actorProxyFactory;
            }

            public async Task DoSomething(Guid id, string msg)
            {
                var proxy = _actorProxyFactory.CreateActorProxy<IExampleActor>(new ActorId(id), "App", "Service", "Listener");
                await _subscriptionHelper.SubscribeAsync<IExampleEvents>(proxy, this);
                //await proxy.SubscribeAsync<IExampleEvents>(this);  //crashes if the caller is not of type ActorProxy, which is not the case when mocked.
                await proxy.ActorSomething(msg);
            }

            public void OnSuccess(string msg)
            {
                Debug.WriteLine("Service: " + msg);
                IsSuccess = true;
            }
        }


        [TestMethod]
        public async Task TestSubscribe_Doesnt_CrashAsync()
        {
            //var service = new CustomActorService(MockStatefulServiceContextFactory.Default, ActorTypeInformation.Get(typeof(MyStatefulActor)));
            //var factory = new MockActorServiceRemotingClientFactory(service);
            //var proxyFactory = new ActorProxyFactory(callbackClient => factory)

            var guid = Guid.NewGuid();
            var id = new ActorId(guid);
            Func<ActorService, ActorId, ActorBase> factory = (service, actorId) => new ExampleActorMock(service, actorId);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<ExampleActorMock>(factory);
            var actor = svc.Activate(id);

            var mockProxyFactory = new MockActorProxyFactory();
            mockProxyFactory.RegisterActor(actor);

            var eventSubscriptionHelper = new MockActorEventSubscriptionHelper();
            var exampleService = new ExampleClient(MockStatefulServiceContextFactory.Default, new MockReliableStateManager(),
                eventSubscriptionHelper, mockProxyFactory);
            await exampleService.DoSomething(guid, "message text");

            Assert.IsTrue(eventSubscriptionHelper.IsSubscribed<IExampleEvents>(exampleService));
            Assert.IsFalse(IsSuccess);
            //Subscribe doesn't crash the test, but the Event is not really fired and processed at this time
        }
    }
}
