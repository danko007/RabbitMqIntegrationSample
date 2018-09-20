using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using NLog;
using SuperVillainBoundedContext.Contracts;

namespace SuperVillainBoundedContext
{
    public class SuperVillainService: IHandleCommand<AddSuperVillain>, IHandleCommand<TryDestroyACity>, IHandleCommand<TryPoisonWater>, IHandleCommand<LaughEvil>,
        IDeclareEvent<SuperVillainAdded>, IDeclareEvent<CityDestructionTried>, IDeclareEvent<WaterPoisoningTried>
    {
        private readonly IDispatchEvent _eventDispatcher;
        
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public SuperVillainService(IDispatchEvent eventDispatcher)
        {
            _eventDispatcher = eventDispatcher;
        }

        public async Task HandleAsync(AddSuperVillain c)
        {
            log.Info($"Adding a super Villain with name: {c.Name}");
            //Add to repository or smth like that
            _eventDispatcher.Dispatch(new SuperVillainAdded{Name = c.Name, Id = c.Id});
        }

        public async Task HandleAsync(TryDestroyACity c)
        {
            log.Info($"Super Villain {c.SuperVillainId} tries to destroy {c.CityName}");
            _eventDispatcher.Dispatch(new CityDestructionTried{Id=c.Id, SuperVillainId = c.SuperVillainId, CityName = c.CityName, CountryCode = c.CountryCode});
        }

        public async Task HandleAsync(TryPoisonWater c)
        {
            log.Info($"Super Villain {c.SuperVillainId} tries to poison water");
            _eventDispatcher.Dispatch(new WaterPoisoningTried() {Id=c.Id, SuperVillainId = c.SuperVillainId, RiverName = c.RiverName, CountryCode = c.CountryCode });
        }

        public async Task HandleAsync(LaughEvil c)
        {
            log.Info($"Super Villain {c.SuperVillainId} performs an evil laughter! {c.PunchLine}");
        }
    }
}
