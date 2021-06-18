using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Models
{
    public class Playdate
    {
        public int PlaydateId { get; set; }

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int UserId { get; set; }
        public Location location { get; set; }
        public string Description { get; set; }
        private Dictionary<int, bool> petTypesPermitted = new Dictionary<int, bool>();

        public Dictionary<int, bool> PetTypesPermitted
        {
            get
            {
                if (this.petTypesPermitted.Count == 0)
                {
                    return new Dictionary<int, bool>() { { -1, false } };
                }
                else
                {
                    return this.petTypesPermitted;
                }
            }
            set { this.petTypesPermitted = value; }
        }

        private Dictionary<int, bool> personalitiesPermitted = new Dictionary<int, bool>();
        public Dictionary<int, bool> PersonalitiesPermitted
        {
            get
            {
                if (this.personalitiesPermitted.Count == 0)
                {
                    return new Dictionary<int, bool>() { { -1, false } };
                }
                else
                {
                    return this.personalitiesPermitted;
                }
            }
            set { this.personalitiesPermitted = value; }
        }
        public List<Pet> Participants { get; set; }
        public string UserName { get; set; }



    }
}
