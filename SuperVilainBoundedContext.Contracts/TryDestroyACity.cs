using System;

namespace SuperVillainBoundedContext.Contracts
{
    public class TryDestroyACity
    {
        public Guid Id { get; set; }
        public Guid SuperVillainId { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
    }
    public class CityDestructionTried
    {
        public Guid Id { get; set; }
        public Guid SuperVillainId { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
    }
}