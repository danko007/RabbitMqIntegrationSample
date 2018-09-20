using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperheroBoundedContext.Contracts
{
    public class AddSuperhero
    {
        public Guid Id { get; set; }
        public string  Name { get; set; }
        public string Address { get; set; }
        public DateTime Dob { get; set; }
    }

    public class SuperheroAdded
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public DateTime Dob { get; set; }
        
    }
}
