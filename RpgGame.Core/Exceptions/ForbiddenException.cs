using System.Net;

namespace RpgGame.Core.Exceptions
{
    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string message) 
            : base(HttpStatusCode.Forbidden, "Forbidden", message)
        {
        }
    }
}