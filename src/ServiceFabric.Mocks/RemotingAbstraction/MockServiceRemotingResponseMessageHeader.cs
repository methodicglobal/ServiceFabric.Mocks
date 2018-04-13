using System.Collections.Generic;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Mocks.RemotingAbstraction
{
    /// <summary>
    /// Mock implementation of <see cref="IServiceRemotingResponseMessageHeader"/>
    /// </summary>
    public class MockServiceRemotingResponseMessageHeader : IServiceRemotingResponseMessageHeader
    {
        public Dictionary<string, byte[]> HeaderData { get; } = new Dictionary<string, byte[]>();
 

        public void AddHeader(string headerName, byte[] headerValue)
        {
            HeaderData[headerName] = headerValue;
        }

        public bool TryGetHeaderValue(string headerName, out byte[] headerValue)
        {
            return HeaderData.TryGetValue(headerName, out headerValue);
        }
    }
}