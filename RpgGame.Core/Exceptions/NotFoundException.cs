using System.Net;

namespace RpgGame.Core.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string entityName, object key) 
            : base(HttpStatusCode.NotFound, 
                  "Resource not found",
                  $"Entity '{entityName}' with key '{key}' was not found.")
        {
        }

        public NotFoundException(string message) 
            : base(HttpStatusCode.NotFound, "Resource not found", message)
        {
        }
    }
}