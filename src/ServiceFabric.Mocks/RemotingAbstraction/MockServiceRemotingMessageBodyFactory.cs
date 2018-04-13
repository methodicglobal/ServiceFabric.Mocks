using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Mocks.RemotingAbstraction
{
    public class MockServiceRemotingMessageBodyFactory : IServiceRemotingMessageBodyFactory
    {
        /// <summary>
        /// Gets or sets the request to return from <see cref="CreateRequest"/>.
        /// </summary>
        public IServiceRemotingRequestMessageBody Request { get; set; }

        /// <summary>
        /// Gets or sets the response to return from <see cref="CreateResponse"/>.
        /// </summary>
        public IServiceRemotingResponseMessageBody Response { get; set; }

        public IServiceRemotingRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters)
        {
            return Request;
        }

        public IServiceRemotingResponseMessageBody CreateResponse(string interfaceName, string methodName)
        {
            return Response;
        }
    }
}