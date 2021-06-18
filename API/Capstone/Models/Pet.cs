using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Models
{
    public class Pet
    {
        public int PetId { get; set; }
        public int UserId { get; set; }
        public string PetName { get; set; }
        public DateTime Birthday { get; set; }
        public char Sex { get; set; }
        public int PetTypeId { get; set; }
        public string PetType { get; set; }
        public string Breed { get; set; }
        public string Color { get; set; }
        public string Bio { get; set; }
        public int[] PersonalityIds { get; set; }
        public string[] Personalities { get; set; }
        public string imgUrl { get; set; }

        public static bool AreEquivalent(Pet a, Pet b)
        {
            //i'd like to do this in a lambda but me dont know how
            bool arePersonalitiesEqual = false;
            if(a.PersonalityIds.Length == b.PersonalityIds.Length)
            {
                Array.Sort(a.PersonalityIds);
                Array.Sort(b.PersonalityIds);
                arePersonalitiesEqual = true;//assume its true for now. If we find discrepancies, we'll set it to false.
                for(int i = 0; i < a.PersonalityIds.Length; i++)
                {
                    if (a.PersonalityIds[i] != b.PersonalityIds[i])
                    {
                        arePersonalitiesEqual = false;
                        break;
                    }
                }

            }
            return true &&
            arePersonalitiesEqual &&
            a.UserId == b.UserId &&
            a.PetName == b.PetName &&
            a.Birthday == b.Birthday &&
            a.Sex == b.Sex &&
            a.PetTypeId == b.PetTypeId &&
            a.Breed == b.Breed &&
            a.Color == b.Color &&
            a.Bio == b.Bio;

        }

    }



}
