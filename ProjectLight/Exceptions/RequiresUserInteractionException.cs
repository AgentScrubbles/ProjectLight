using System;

namespace ProjectLight.Exceptions
{
    public class RequiresUserInteractionException : Exception
    {
        public RequiresUserInteractionException() : base()
        {
            
        }

        public RequiresUserInteractionException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}
