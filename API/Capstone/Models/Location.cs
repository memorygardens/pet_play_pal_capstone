using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Models
{
    public class Location
    {
        public int LocationId { get; set; } = -1;
        public string Address { get; set; }
        public float Lat { get; set; }
        public float Lng { get; set; }
        public string Name { get; set; }



    }
}
