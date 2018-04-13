using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Runtime;

namespace ServiceFabric.Mocks.RemotingAbstraction
{
    /// <summary>
    /// Mock implementation of <see cref="IServiceRemotingMessageHandler"/>.
    /// </summary>
    public class MockServiceRemotingMessageHandler : IServiceRemotingMessageHandler
    {
        public MockServiceRemotingMessageBodyFactory MockServiceRemotingMessageBodyFactory { get; set; } = new MockServiceRemotingMessageBodyFactory();

        public static MockServiceRemotingMessageHandler Default { get; } = new MockServiceRemotingMessageHandler();

        public Task<IServiceRemotingResponseMessage> HandleRequestResponseAsync(IServiceRemotingRequestContext requestContext,
            IServiceRemotingRequestMessage requestMessage)
        {
            var header = new MockServiceRemotingResponseMessageHeader();
            var requestHeader = requestMessage.GetHeader();
            var requestHeaders = (Dictionary<string, byte[]>)requestHeader
                .GetType()
                .GetField("headers", Constants.InstanceNonPublic)
                .GetValue(requestHeader);

            foreach (var entry in requestHeaders)
            {
                header.AddHeader(entry.Key, entry.Value);
            }

            var body = new MockServiceRemotingResponseMessageBody();
            var requestBody = (MockServiceRemotingRequestMessageBody)requestMessage.GetBody();

            foreach (var entry in requestBody.StoredValues)
            {
                body.Set(entry.Value);
            }

            return Task.FromResult<IServiceRemotingResponseMessage>(new MockServiceRemotingResponseMessage
            {
                Header = header,
                Body = body
            });
        }

        public void HandleOneWayMessage(IServiceRemotingRequestMessage requestMessage)
        {
        }

        public IServiceRemotingMessageBodyFactory GetRemotingMessageBodyFactory()
        {
            return MockServiceRemotingMessageBodyFactory;
        }
    }
}
