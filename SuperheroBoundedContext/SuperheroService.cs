using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using NLog;
using SuperheroBoundedContext.Contracts;

namespace SuperheroBoundedContext
{
    public class SuperheroService: IHandleCommand<AddSuperhero>, IHandleCommand<TryPerformFeat>, IHandleCommand<RetireSuperhero>, IDeclareEvent<SuperheroAdded>, IDeclareEvent<TryPerformFeatReported>, IDeclareEvent<RetireSuperhero>
    {

        private readonly IDispatchEvent _eventDispatcher;


        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public SuperheroService(IDispatchEvent eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }
        public async Task HandleAsync(AddSuperhero c)
        {
            log.Info($"Command to add {c.Name} Superhero with id {c.Id}");
            _eventDispatcher.Dispatch(new SuperheroAdded{Id = c.Id, Name = c.Name, Address = c.Address, Dob = c.Dob});
        }

        public async Task HandleAsync(TryPerformFeat c)
        {
            log.Info($"Command to create a feat by superhero with id {c.SuperheroId}!");
            _eventDispatcher.Dispatch(new TryPerformFeatReported { Id = c.Id,Description = c.Description, SuperheroId = c.SuperheroId, Date = c.Date?? DateTime.Now.Date, DisasterId = c.DisasterId});
        }

        public async Task HandleAsync(RetireSuperhero c)
        {
            log.Info($"Command to retire Superhero with id {c.Id}");
            _eventDispatcher.Dispatch(new SuperheroRetired { Id = c.Id, Timestamp = c.Timestamp?? DateTime.Now});
        }
    }
}
