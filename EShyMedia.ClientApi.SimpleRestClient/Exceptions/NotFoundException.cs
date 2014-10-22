using System;

namespace EShyMedia.ClientApi.SimpleRestClient.Exceptions
{
    public class NotFoundException : NetworkException
    {
        public NotFoundException(String message) : base(message)
        {
        }

        public NotFoundException(String message, Exception innerException) : base(message,innerException)
        {
        }
    }
}