using Capstone.Models;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Capstone.DAO
{
    public interface IPetDAO
    {
        int AddPet(Pet petToAdd);
        Dictionary<int, string> GetPersonalityTypes();
        Pet GetPetById(int petId);
        Dictionary<int, string> GetPetTypes();
        List<Pet> GetPetsByUserId(int userId);
        bool OverwritePetPersonalityByPetId(int petId, int[] personalityIds);
        Pet RowToObject(SqlDataReader rdr);
        Pet UpdatePet(Pet petToUpdate);
        int GetPetTypeId(string petType);
        List<Pet> GetAllPets();
        List<Pet> GetParticipantPetsByPlaydateId(int playdateId);
        int UpdatePetImageUrl(int petId, string url);
    }
}