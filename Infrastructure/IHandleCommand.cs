using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public interface IHandleCommand { }

    public interface IHandleCommand<in T>: IHandleCommand where T: class
    {
        Task HandleAsync(T c);
    }

    public class CommandFailed
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
