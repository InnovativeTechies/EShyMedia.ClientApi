using System;

namespace EShyMedia.ClientApi.SimpleRestClient
{
    public static class StringExtensions
    {
        public static string UrlEncode(this string input)
        {
            return Uri.EscapeDataString(input);
        }
    }
}