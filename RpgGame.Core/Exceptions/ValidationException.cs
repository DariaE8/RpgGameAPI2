using System.Net;

namespace RpgGame.Core.Exceptions
{
    public class ValidationException : BaseException
    {
        public IDictionary<string, string[]> Errors { get; }

        public ValidationException(IDictionary<string, string[]> errors) 
            : base(HttpStatusCode.BadRequest, "Validation error", "One or more validation errors occurred.")
        {
            Errors = errors;
        }

        public ValidationException(string propertyName, string errorMessage) 
            : base(HttpStatusCode.BadRequest, "Validation error", "One or more validation errors occurred.")
        {
            Errors = new Dictionary<string, string[]>
            {
                [propertyName] = new[] { errorMessage }
            };
        }
    }
}