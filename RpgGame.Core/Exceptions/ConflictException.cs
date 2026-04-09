using System.Net;

namespace RpgGame.Core.Exceptions
{
    public class ConflictException : BaseException
    {
        public ConflictException(string message) 
            : base(HttpStatusCode.Conflict, "Conflict", message)
        {
        }
    }
}