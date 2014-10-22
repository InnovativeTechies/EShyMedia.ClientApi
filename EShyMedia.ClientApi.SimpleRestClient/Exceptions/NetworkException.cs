using System;

namespace EShyMedia.ClientApi.SimpleRestClient.Exceptions
{
    public class NetworkException : Exception
    {
        public NetworkException(String message) : base(message)
        {
        }

        public NetworkException(String message, Exception innerException) : base(message,innerException)
        {
        }
    }
}
