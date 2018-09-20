using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using NLog;
using SuperheroBoundedContext.Contracts;
using SuperVillainBoundedContext.Contracts;

namespace SuperheroBoundedContext
{
    public class SuperVillainDirtyDeedsService : IHandleEvent<WaterPoisoningTried>, IHandleEvent<CityDestructionTried>
    {
        private readonly IDispatchCommand _commandDispatcher;

        public SuperVillainDirtyDeedsService(IDispatchCommand commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public async Task HandleAsync(WaterPoisoningTried e)
        {
            log.Info($"Someone tries to poison the {e.RiverName} river");
            await _commandDispatcher.DispatchAsync(new TryPerformFeat{Id = Guid.NewGuid(), Description = $"Protect the {e.RiverName} river!", SuperheroId = Guid.NewGuid()/*Should be taken from repository*/, DisasterId = e.Id});
        }

        public async Task HandleAsync(CityDestructionTried e)
        {
            log.Info($"Someone tries to destroy the {e.CityName}");
            await _commandDispatcher.DispatchAsync(new TryPerformFeat { Id = Guid.NewGuid(), Description = $"Save {e.CityName} City", SuperheroId = Guid.NewGuid()/*Should be taken from repository*/, DisasterId = e.Id });
        }
    }
}
