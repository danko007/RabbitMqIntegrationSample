using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperVillainBoundedContext.Contracts
{
    public class AddSuperVillain
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class SuperVillainAdded
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
