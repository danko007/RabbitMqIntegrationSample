using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.RabbitMQ;
using SuperheroBoundedContext.Contracts;
using SuperVillainBoundedContext.Contracts;

namespace ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var bs = new BusSubscription())
                using (var d = new CommandDispatcher(bs))
                {
                    var superHeroId = Guid.NewGuid();
                    var superVillainId = Guid.NewGuid();

                    Thread.Sleep(1000);
                    Console.WriteLine("Adding Joker");
                    d.DispatchAsync(new AddSuperVillain() { Name = "Joker", Id = superVillainId }).Wait();

                Console.WriteLine("Adding Spiderman");
                    d.DispatchAsync(new AddSuperhero{Name = "Spiderman", Id = superHeroId, Address = "New York", Dob = new DateTime(1984,11,15)}).Wait();
                
                    Thread.Sleep(5000);
                    Console.WriteLine("Joker tries to demolish New York");
                d.DispatchAsync(new TryDestroyACity{Id = Guid.NewGuid(), SuperVillainId = superVillainId, CityName = "New York", CountryCode = "US"}).Wait();
                    Thread.Sleep(5000);
                    Console.WriteLine("Joker tries to poison Volga");
                d.DispatchAsync(new TryPoisonWater(){Id = Guid.NewGuid(), SuperVillainId = superVillainId, RiverName = "Volga", CountryCode = "RU"}).Wait();

                    Thread.Sleep(5000);

                    Console.WriteLine("Spiderman is too tired and retired!");
                d.DispatchAsync(new RetireSuperhero{Id=superHeroId});
                    Console.ReadLine();
                }
        }
    }
}