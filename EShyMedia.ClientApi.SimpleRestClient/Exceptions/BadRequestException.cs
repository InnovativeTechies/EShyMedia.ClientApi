using System;

namespace EShyMedia.ClientApi.SimpleRestClient.Exceptions
{
    public class BadRequestException : NetworkException
    {
        public BadRequestException(String message) : base(message)
        {
        }

        public BadRequestException(String message, Exception innerException) : base(message,innerException)
        {
        }
    }
}