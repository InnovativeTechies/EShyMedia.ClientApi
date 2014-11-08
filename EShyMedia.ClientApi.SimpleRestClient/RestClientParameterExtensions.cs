using System;
using System.Text;

namespace EShyMedia.ClientApi.SimpleRestClient
{
    public static class RestClientParameterExtensions
    {
        public static RestParameters AddNoCache(this RestParameters parameters)
        {
            parameters.AddParameter("nocache", DateTime.UtcNow.Ticks);
            parameters.AddHeader("Cache-Control", "no-cache");
            parameters.AddHeader("Pragma", "no-cache");
            return parameters;
        }

        public static RestParameters AddPagingParameters(this RestParameters parameters, int pageNumber, int pageSize)
        {
            parameters.AddParameter("page", pageNumber);
            parameters.AddParameter("size", pageSize);
            return parameters;
        }

        public static RestParameters AddAuthorizationHeader(this RestParameters parameters, string securityMethod,
            string securityToken)
        {
            //TODO: If there's already an authorization param, replace it

            if (String.IsNullOrWhiteSpace(securityMethod) || String.IsNullOrWhiteSpace(securityToken))
            {
                //Can't add authorization with no token
                return parameters;
            }
            parameters.AddParameter(securityMethod, securityToken, RestParameterTypes.Authorization);
            return parameters;
        }

        public static RestParameters AddBasicAuthorization(this RestParameters parameters, string userName,
            string password)
        {
            var encoding = Encoding.GetEncoding("iso-8859-1");
            var userPass = String.Format("{0}:{1}", userName, password);
            var encoded = Convert.ToBase64String(encoding.GetBytes(userPass));
            return parameters.AddAuthorizationHeader("Basic", encoded);
        }

        //public static RestParameters AddOAuth2PasswordGrant(this RestParameters parameters, string userName,
        //    string password, string clientId)
        //{
        //    {"grant_type" = ""}
        //}
    }
}
