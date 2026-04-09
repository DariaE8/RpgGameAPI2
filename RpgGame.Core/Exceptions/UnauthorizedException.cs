using System.Net;

namespace RpgGame.Core.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message) 
            : base(HttpStatusCode.Unauthorized, "Unauthorized", message)
        {
        }
    }
}