using System;

namespace SuperVillainBoundedContext.Contracts
{
    public class TryPoisonWater
    {
        public Guid Id { get; set; }
        public Guid SuperVillainId { get; set; }
        public string RiverName { get; set; }
        public string CountryCode { get; set; }
    }
    public class WaterPoisoningTried
    {
        public Guid Id { get; set; }
        public Guid SuperVillainId { get; set; }
        public string RiverName { get; set; }

        public string CountryCode { get; set; }
    }
}