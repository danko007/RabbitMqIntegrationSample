using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperheroBoundedContext.Contracts
{
    public class RetireSuperhero
    {
        public Guid Id { get; set; }
        public DateTime? Timestamp  { get; set; }
    }

    public class SuperheroRetired
    {
        public Guid Id { get; set; }
        public DateTime? Timestamp { get; set; }
        
    }
}
