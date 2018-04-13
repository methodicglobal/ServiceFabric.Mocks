using System;
using System.Collections.Generic;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace ServiceFabric.Mocks.RemotingAbstraction
{
    public class MockServiceRemotingRequestMessageBody : IServiceRemotingRequestMessageBody
    {
        //public int Positition { get; set; }

        //public string ParameName { get; set; }

        //public object Parameter { get; set; }

        public Dictionary<int, Dictionary<string, object>> StoredValues { get; } = new Dictionary<int, Dictionary<string, object>>();

        public void SetParameter(int position, string parameName, object parameter)
        {
            if (!StoredValues.TryGetValue(position, out var dict))
            {
                dict = new Dictionary<string, object>();
                StoredValues.Add(position, dict);
            }
            if (!dict.TryGetValue(parameName, out var val))
            {
                dict.Add(parameName, parameter);
            }
            else
            {
                dict[parameName] = val;
            }
        }

        public object GetParameter(int position, string parameName, Type paramType)
        {
            if (StoredValues.TryGetValue(position, out var dict)
                && dict.TryGetValue(parameName, out var val))
            {
                return val;
            }
            return null;
        }
    }
}