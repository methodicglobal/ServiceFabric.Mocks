using System;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Mocks.RemotingAbstraction
{
    public class MockServiceRemotingResponseMessageBody : IServiceRemotingResponseMessageBody
    {
        public object Response { get; set; }

        public void Set(object response)
        {
            Response = response;
        }

        public object Get(Type paramType)
        {
            return Response;
        }
    }
}