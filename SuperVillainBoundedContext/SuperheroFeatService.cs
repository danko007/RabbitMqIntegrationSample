using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using NLog;
using SuperheroBoundedContext.Contracts;
using SuperVillainBoundedContext.Contracts;

namespace SuperVillainBoundedContext
{
    public class SuperheroFeatsService: IHandleEvent<SuperheroAdded>, IHandleEvent<SuperheroRetired>, IHandleEvent<FeatPerformed>, IHandleEvent<FeatFailed>
    {
        private readonly IDispatchCommand _commandDispatcher;

        public SuperheroFeatsService(IDispatchCommand commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public async Task HandleAsync(SuperheroAdded e)
        {
            var appropriateSuperVillainId = Guid.NewGuid();
            await _commandDispatcher.DispatchAsync(new LaughEvil{SuperVillainId = appropriateSuperVillainId, PunchLine = $"Oh, pity! { e.Name } is new superhero!" });
            
        }

        public async Task HandleAsync(SuperheroRetired e)
        {
            var appropriateSuperVillainId = Guid.NewGuid();
            await _commandDispatcher.DispatchAsync(new LaughEvil { SuperVillainId = appropriateSuperVillainId, PunchLine = "Hooray! He is out!" });
        }

        public async Task HandleAsync(FeatPerformed e)
        {
            log.Info($"How Sad! {e.Description}");
        }

        public async Task HandleAsync(FeatFailed e)
        {
            log.Info($"Well! {e.Description}");
        }
    }
}
