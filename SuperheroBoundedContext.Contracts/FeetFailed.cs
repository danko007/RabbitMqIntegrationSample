using System;

namespace SuperheroBoundedContext.Contracts
{
    public class FeatFailed
    {
        public Guid Id { get; set; }
        public Guid DisasterId { get; set; }
        public Guid SuperheroId { get; set; }
        public string Description { get; set; }
        public DateTime? Date { get; set; }

    }
}