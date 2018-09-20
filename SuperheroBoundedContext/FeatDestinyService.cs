using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using SuperheroBoundedContext.Contracts;

namespace SuperheroBoundedContext
{
    public class FeatDestinyService : IHandleEvent<TryPerformFeatReported>, IDeclareEvent<FeatPerformed>, IDeclareEvent<FeatFailed>
    {
        private readonly IDispatchEvent _eventDipatcher;
        public FeatDestinyService(IDispatchEvent eventDipatcher)
        {
            _eventDipatcher = eventDipatcher;
        }
        public async Task HandleAsync(TryPerformFeatReported e)
        {
            var featSuccessProbability = 0.5
                ;

            if(new Random().NextDouble() < featSuccessProbability)
                _eventDipatcher.Dispatch(new FeatPerformed
                {
                    Id = e.Id, DisasterId = e.DisasterId, SuperheroId = e.SuperheroId, Date = e.Date, Description = e.Description + " - Done!"
                });
            else
            _eventDipatcher.Dispatch(new FeatFailed
                {
                    Id = e.Id, DisasterId = e.DisasterId, SuperheroId = e.SuperheroId, Date = e.Date, Description = e.Description + " - Failed!"
                });
        }
    }
}
