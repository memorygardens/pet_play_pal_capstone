using Capstone.Models;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.DAO
{
    public class PetDAO : IPetDAO
    {
        private readonly string connectionString;
        private const string SQL_ADDPET = "insert into pet(user_id, pet_name, birthday, sex, pet_type_id, pet_breed, color, bio) values (@userId, @petName, @birthday, @sex, @petTypeId, @petBreed, @color, @bio); select @@IDENTITY;";
        private const string SQL_GET_PETS_BY_USERID = "select * from fullPet where user_id = @userId";
        private const string SQL_GET_ALL_PETS = "select * from fullPet";
        private const string SQL_GETALLPERSONALITIES = "select * from personality";
        private const string SQL_GETPERSONALITIESFORPETBYID = "select personality_name,personality.personality_id from personality join personality_pet on personality.personality_id = personality_pet.personality_id where personality_pet.pet_id = @petId";
        private const string SQL_GETALLPETTYPES = "select * from pet_type";
        //private const string SQL_UPDATE_PET_BY_ID_WITH_PERSONALITIES = "begin transaction; update pets set pet_name = @petName, birthday = @birthday, sex = @sex, pet_type_id = @petTypeId, pet_breed = @petBreed, color = @color, bio = @bio where pet_id = @petId; delete from personality_pet where pet_id = @petId; insert into personality_pet select @petId, personality_id from personality where personality_id in ({0}); commit transaction;";
        private const string SQL_GET_PET_BY_ID = "select * from fullPet where pet_id = @petId";
        private const string SQL_OVERWRITE_PET_PERSONALITIES = "begin transaction; delete from personality_pet where pet_id = @petId; insert into personality_pet select @petId, personality_id from personality where personality_id in ({0}); commit transaction;";
        private const string SQL_UPDATE_PET_BY_ID = "update pet set pet_name = @petName, birthday = @birthday, sex = @sex, pet_type_id = @petTypeId, pet_breed = @petBreed, color = @color, bio = @bio where pet_id = @petId;";
        private const string SQL_GET_PETTYPE_ID_BY_PETTYPE = "select * from pet_type where pet_type_name = @petType";
        private const string SQL_GET_PETS_FOR_PLAYDATEID = "select pp.playdate_id,p.* from playdate_pet as pp join fullPet as p on pp.pet_id = p.pet_id where playdate_id = @playdateId";
        private const string SQL_UPDATE_PET_IMAGE_URL = "update pet set pet_image_url = @pet_image_url where pet_id = @pet_id";
        //private const string SQL_GET_PET_IMAGE_URL_BY_ID = "select pet_image_url from pet where pet_id = @pet_id";
        public PetDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }


        /// <summary>Gets a dictionary of valid personality types.</summary>
        /// <returns> a <see cref="Dictionary{int, String}"/>
        /// </returns>
        public Dictionary<int, string> GetPersonalityTypes()
        {
            Dictionary<int, string> personalities = new Dictionary<int, string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(SQL_GETALLPERSONALITIES, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        int id = Convert.ToInt32(rdr["personality_id"]);
                        string name = Convert.ToString(rdr["personality_name"]);
                        personalities[id] = name;
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return personalities;
        }

        /// <summary>
        /// Gets a list of all valid pet types and their IDs
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, string> GetPetTypes()
        {
            Dictionary<int, string> petTypes = new Dictionary<int, string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(SQL_GETALLPETTYPES, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        int id = Convert.ToInt32(rdr["pet_type_id"]);
                        string name = Convert.ToString(rdr["pet_type_name"]);
                        petTypes[id] = name;
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return petTypes;
        }

        /// <summary>
        /// gets all the pets on the database
        /// </summary>
        public List<Pet> GetAllPets()
        {
            List<Pet> usersPets = new List<Pet>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_ALL_PETS, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Pet pet = RowToObject(rdr);
                        usersPets.Add(pet);
                    }

                }

            }
            catch (SqlException)
            {
                return usersPets;
            }
            return usersPets;
        }

        public Pet GetPetById(int petId)
        {
            Pet pet = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_PET_BY_ID, conn);
                    cmd.Parameters.AddWithValue("@petId", petId);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        pet = RowToObject(rdr);
                    }

                }
            }
            catch (SqlException)
            {
                throw;
            }


            return pet;
        }

        /// <summary>
        /// Adds a new pet to the database
        /// </summary>
        /// <param name="petToAdd">The pet to add.</param>
        /// <returns>the ID of the newly added pet, or -1 if adding the pet was not successful</returns>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for AddPet
        public int AddPet(Pet petToAdd)
        {
            int petId = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_ADDPET, conn);
                    cmd.Parameters.AddWithValue("@userId", petToAdd.UserId);
                    cmd.Parameters.AddWithValue("@petId", petToAdd.PetId);
                    cmd.Parameters.AddWithValue("@petName", petToAdd.PetName);
                    cmd.Parameters.AddWithValue("@birthday", petToAdd.Birthday);
                    cmd.Parameters.AddWithValue("@sex", petToAdd.Sex);
                    cmd.Parameters.AddWithValue("@petTypeId", petToAdd.PetTypeId);
                    cmd.Parameters.AddWithValue("@petBreed", petToAdd.Breed);
                    cmd.Parameters.AddWithValue("@color", petToAdd.Color);
                    cmd.Parameters.AddWithValue("@bio", petToAdd.Bio);
                    petId = Convert.ToInt32(cmd.ExecuteScalar());
                    petToAdd.PetId = petId;
                    OverwritePetPersonalityByPetId(petToAdd.PetId, petToAdd.PersonalityIds);

                }
            }
            catch (SqlException)
            {
                throw;
            }

            return petId;
        }

        //edit a pet
        public Pet UpdatePet(Pet petToUpdate)
        {
            Pet updatedPet = null;

            try
            {
                //step 1: update the fields on the pets table
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_UPDATE_PET_BY_ID, conn);
                    cmd.Parameters.AddWithValue("@petId", petToUpdate.PetId);
                    cmd.Parameters.AddWithValue("@petName", petToUpdate.PetName);
                    cmd.Parameters.AddWithValue("@birthday", petToUpdate.Birthday);
                    cmd.Parameters.AddWithValue("@sex", petToUpdate.Sex);
                    cmd.Parameters.AddWithValue("@petTypeId", petToUpdate.PetTypeId);
                    cmd.Parameters.AddWithValue("@petBreed", petToUpdate.Breed);
                    cmd.Parameters.AddWithValue("@color", petToUpdate.Color);
                    cmd.Parameters.AddWithValue("@bio", petToUpdate.Bio);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        //step 2: update the fields on the pet_personality relator table
                        if (OverwritePetPersonalityByPetId(petToUpdate.PetId, petToUpdate.PersonalityIds))
                        {
                            updatedPet = GetPetById(petToUpdate.PetId);
                        }
                    }
                }


            }
            catch (SqlException)
            {
                throw;
            }

            return updatedPet;
        }

        /// <summary>
        /// Given a pet ID, fully overwrite all personality info in the database for that pet.
        /// </summary>
        /// <param name="petId">The ID of the pet to modify.</param>
        /// <param name="personalityIds">an array of personality IDs that the pet should have</param>
        /// <returns>true if the operation was successful, false if not.</returns>
        public bool OverwritePetPersonalityByPetId(int petId, int[] personalityIds)
        {
            bool successful = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@petId", petId);
                    //here we will add 1-by-1 a list of personality IDs to give to the updated Pet

                    //this will hold {"@personalityId0","@personalityId1","@personalityId2"....} as many personalities as there are in the petToUpdate
                    List<string> personalityIdParamNames = new List<string>();

                    //loop through each personality that we want to add
                    int i = 0;
                    foreach (int personalityId in personalityIds)
                    {
                        string paramName = $"@personalityId{i}";//create a prepared variable param for the SQL command
                        cmd.Parameters.AddWithValue(paramName, personalityId);//add the variable
                        personalityIdParamNames.Add(paramName);//...and add the variable to a list as well
                        i++;
                    }
                    cmd.CommandText = String.Format(SQL_OVERWRITE_PET_PERSONALITIES, string.Join(",", personalityIdParamNames));
                    int rowsAffected = cmd.ExecuteNonQuery();
                    successful = rowsAffected > 0;
                }

            }
            catch (SqlException)
            {
                throw;
            }
            return successful;
        }

        // get all pets for a registered user 
        public List<Pet> GetPetsByUserId(int userId)
        {

            List<Pet> usersPets = new List<Pet>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_PETS_BY_USERID, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Pet pet = RowToObject(rdr);
                        usersPets.Add(pet);
                    }

                }

            }
            catch (SqlException)
            {
                return usersPets;
            }
            return usersPets;


        }

        /// <summary>
        /// Given a pet type name (i.e. "Dog", "Cat" etc.) return it's corresponding petTypeId from the database.
        /// </summary>
        /// <param name="petType">Type of pet as a string</param>
        /// <returns>the id of that pet type, or -1 if not found</returns>
        public int GetPetTypeId(string petType)
        {
            int petTypeId = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_PETTYPE_ID_BY_PETTYPE, conn);
                    cmd.Parameters.AddWithValue("@petType", petType);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        petTypeId = Convert.ToInt32(rdr["pet_type_id"]);
                    }

                }

            }
            catch (SqlException)
            {
                throw;
            }
            return petTypeId;

        }
        public Dictionary<int, string> GetPersonalitiesByPetId(int petId)
        {
            Dictionary<int, string> personalities = new Dictionary<int, string>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GETPERSONALITIESFORPETBYID, conn);
                    cmd.Parameters.AddWithValue("@petId", petId);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        personalities[Convert.ToInt32(rdr["personality_id"])] = (Convert.ToString(rdr["personality_name"]));
                    }
                }

            }
            catch (SqlException)
            {
                throw;
            }

            return personalities;
        }


        /// <summary>
        /// Given a playdateId, get a list of pets that participate in that playdate. If no such playdate exists, returns an empty list.
        /// </summary>
        /// <param name="playdateId">the playdate id</param>
        /// <returns>a list of <see cref="Pet">Pets</see> that are participating in that playdate</returns>
        public List<Pet> GetParticipantPetsByPlaydateId(int playdateId)
        {
            List<Pet> participants = new List<Pet>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_PETS_FOR_PLAYDATEID, conn);
                    cmd.Parameters.AddWithValue("@playdateId", playdateId);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        participants.Add(RowToObject(rdr));
                    }

                }
            }
            catch (SqlException)
            {
                throw;
            }

            return participants;
        }

        //Inserts uploaded pet image URL from Cloudnary into database for a particular pet 
        public int UpdatePetImageUrl(int petId, string url)
        {
            int rowsAffected = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_UPDATE_PET_IMAGE_URL, conn);
                    cmd.Parameters.AddWithValue("@pet_image_url", url);
                    cmd.Parameters.AddWithValue("@pet_id", petId);
                    rowsAffected = cmd.ExecuteNonQuery();
                }

            }
            catch(SqlException)
            {
                throw;
            }

            return rowsAffected;
        }


       
        public Pet RowToObject(SqlDataReader rdr)
        {
            Pet pet = new Pet();
            pet.UserId = Convert.ToInt32(rdr["user_id"]);
            pet.PetId = Convert.ToInt32(rdr["pet_id"]);
            pet.PetName = Convert.ToString(rdr["pet_name"]);
            pet.Birthday = Convert.ToDateTime(rdr["birthday"]);
            pet.Sex = Convert.ToChar(rdr["sex"]);
            pet.PetTypeId = Convert.ToInt32(rdr["pet_type_id"]);
            pet.PetType = Convert.ToString(rdr["pet_type_name"]);
            pet.Breed = Convert.ToString(rdr["pet_breed"]);
            pet.Color = Convert.ToString(rdr["color"]);
            pet.Bio = Convert.ToString(rdr["bio"]);
            pet.imgUrl = Convert.ToString(rdr["pet_image_url"]);
            Dictionary<int, string> Personalities = GetPersonalitiesByPetId(pet.PetId);
            pet.Personalities = Personalities.Values.ToArray();
            pet.PersonalityIds = Personalities.Keys.ToArray();
            

            return pet;
        }

    }
}
