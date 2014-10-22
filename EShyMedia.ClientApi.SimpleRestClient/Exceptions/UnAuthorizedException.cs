using System;

namespace EShyMedia.ClientApi.SimpleRestClient.Exceptions
{
    public class UnAuthorizedException : NetworkException
    {
        public UnAuthorizedException(String message) : base(message)
        {
        }

        public UnAuthorizedException(String message, Exception innerException) : base(message,innerException)
        {
        }
    }
}