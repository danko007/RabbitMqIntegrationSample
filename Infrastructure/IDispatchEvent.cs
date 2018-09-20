using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public interface IDeclareEvent { }
    public interface IDeclareEvent<in T>: IDeclareEvent
    {
        //void Dispatch(T e);
    }

    public interface IDispatchEvent
    {
        void Dispatch(object e);
        
    }

    public interface ISubscribeToEvent
    {
        IDisposable Subscribe(object e);
    }
}
