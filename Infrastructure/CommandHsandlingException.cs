using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class CommandHandlingException : Exception
    {
        public CommandHandlingException(string message) : base(message)
        {
        }

        public CommandHandlingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
