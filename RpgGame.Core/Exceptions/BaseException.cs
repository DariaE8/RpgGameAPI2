using System.Net;

namespace RpgGame.Core.Exceptions
{
    public abstract class BaseException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string Title { get; }
        public string Detail { get; }

        protected BaseException(
            HttpStatusCode statusCode,
            string title,
            string detail) : base(detail)
        {
            StatusCode = statusCode;
            Title = title;
            Detail = detail;
        }
    }
}