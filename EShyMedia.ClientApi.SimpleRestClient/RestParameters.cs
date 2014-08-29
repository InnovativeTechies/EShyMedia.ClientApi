using System.Collections.Generic;

namespace EShyMedia.ClientApi.SimpleRestClient
{
    public class RestParameters : List<RestParameter>
    {
        public void AddParameter(string name, object value, RestParameterTypes parameterType)
        {
            Add(new RestParameter
            {
                Name = name,
                Value = value,
                ParameterType = parameterType
            });
        }

        public void AddHeader(string name, string value)
        {
            Add(new RestParameter
            {
                Name = name,
                Value = value,
                ParameterType = RestParameterTypes.Header
            });
        }

        public void AddParameter(string name, object value)
        {
            Add(new RestParameter
            {
                Name = name,
                Value = value,
                ParameterType = RestParameterTypes.QueryString
            });
        }

        public void AddUrlSegment(string name, object value)
        {
            Add(new RestParameter
            {
                Name = name,
                Value = value,
                ParameterType = RestParameterTypes.UrlSegment
            });
        }



    }
}