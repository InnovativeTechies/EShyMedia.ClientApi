﻿using System;

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

    }
}