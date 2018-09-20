using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperheroBoundedContext.Contracts
{
    public class TryPerformFeat
    {
        public Guid Id { get; set; }
        public Guid SuperheroId { get; set; }
        public Guid DisasterId { get; set; }
        public string Description { get; set; }

        public DateTime? Date { get; set; }
    }

    public class TryPerformFeatReported
    {
        public Guid Id { get; set; }
        public Guid DisasterId { get; set; }
        public Guid SuperheroId { get; set; }
        public string Description { get; set; }
        public DateTime? Date { get; set; }

    }
}
