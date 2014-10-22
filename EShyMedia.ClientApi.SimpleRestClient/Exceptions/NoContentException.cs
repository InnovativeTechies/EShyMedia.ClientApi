using System;

namespace EShyMedia.ClientApi.SimpleRestClient.Exceptions
{
    public class NoContentException : NetworkException
    {
        public NoContentException(String message) : base(message)
        {
        }

        public NoContentException(String message, Exception innerException) : base(message,innerException)
        {
        }
    }
}