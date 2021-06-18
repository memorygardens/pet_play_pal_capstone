using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Controllers.searchfilters
{
    public class PlaydateSearchFilter
    {
        public int userId { get; set; } = -1;

        public Dictionary<int, bool> personalitiesPermitted = new Dictionary<int, bool>();

        public List<int> allowedPersonalities
        {
            get
            {
                List<int> personalites = new List<int>();
                foreach (KeyValuePair<int, bool> kvp in this.personalitiesPermitted) { if (kvp.Value) { personalites.Add(kvp.Key); } }
                return personalites;
            }

        }

        public List<int> disallowedPersonalities
        {
            get
            {
                List<int> personalites = new List<int>();
                foreach (KeyValuePair<int, bool> kvp in this.personalitiesPermitted) { if (!kvp.Value) { personalites.Add(kvp.Key); } }
                return personalites;
            }
        }

        public Dictionary<int, bool> petTypesPermitted = new Dictionary<int, bool>();

        public List<int> allowedPetTypes
        {
            get
            {
                List<int> petTypes = new List<int>();
                foreach (KeyValuePair<int, bool> kvp in this.petTypesPermitted) { if (kvp.Value) { petTypes.Add(kvp.Key); } }
                return petTypes;
            }

        }

        public List<int> disallowedPetTypes
        {
            get
            {
                List<int> petTypes = new List<int>();
                foreach (KeyValuePair<int, bool> kvp in this.petTypesPermitted) { if (!kvp.Value) { petTypes.Add(kvp.Key); } }
                return petTypes;
            }
        }

        // Search Radius 

        public float searchRadius { set; get; } = -1;

        // Search Center (zip=> lat., lng.)
        public Location searchCenter { get; set; } = new Location() { Lat = 0, Lng = 0 };

    }
}
