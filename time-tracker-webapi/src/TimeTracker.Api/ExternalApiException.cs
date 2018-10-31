using System;

namespace TimeTracker.Api
{
    public class ExternalApiException : Exception
    {
        public int StatusCode { get; set; }

        public string EntityDescription { get; set; }

        public string Content { get; set; }
    }
}