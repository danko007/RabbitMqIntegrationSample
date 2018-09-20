using System.Threading.Tasks;

namespace Infrastructure
{
    public interface IHandleEvent { }
    public interface IHandleEvent<in T>: IHandleEvent where T : class
    {
        Task HandleAsync(T e);
    }
}