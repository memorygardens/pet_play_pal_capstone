using Capstone.Controllers.searchfilters;
using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Capstone.DAO
{
    public class PlaydateDAO : IPlaydateDAO
    {
        private readonly string connectionString;
        private IPetDAO petDAO;
        private const string SQL_GET_ALL_PLAYDATES = "select * from fullPlaydate";
        private const string SQL_GETPLAYDATEBYID = "select * from fullPlaydate where playdate_id = @playdate_id;";
        private const string SQL_ADDPLAYDATE = "insert into playdate (start_date_time, end_date_time, user_id, location_id) values (@startDateTime, @endDateTime, @userId, @location_id); select @@IDENTITY;";
        private const string SQL_GET_PLAYDATES_BY_USERID = "select * from fullPlaydate where user_id = @userId;";
        //used to help build a fully featured playdate object
        private const string SQL_GET_PET_TYPES_PERMITTED_BY_PLAYDATE_ID = "select * from playdate_pet_type_permitted where playdate_id = @playdateId";
        private const string SQL_GET_PERSONALITIES_PERMITTED_BY_PLAYDATE_ID = "select * from playdate_personality_permitted where playdate_id = @playdateId";
        //these are for playdate filtering. They will need to be parameterized first before you can actually use them
        private const string SQL_GET_PLAYDATE_IDS_BY_PERMITTED_PET_TYPES_ARRAY = "select distinct playdate.playdate_id from playdate join playdate_pet_type_permitted as ppp on playdate.playdate_id = ppp.playdate_id where (((pet_type_id_is_permitted = 1) and (pet_type_id in ({0}))) or (-1 in ({0})))";
        private const string SQL_GET_PLAYDATE_IDS_BY_PROHIBITED_PET_TYPES_ARRAY = "select distinct playdate.playdate_id from playdate join playdate_pet_type_permitted as ppp on playdate.playdate_id = ppp.playdate_id where (((pet_type_id_is_permitted = 0) and (pet_type_id in ({0}))) or (-1 in ({0})))";
        private const string SQL_GET_PLAYDATE_IDS_BY_PERMITTED_PERSONALITIES_ARRAY = "select distinct playdate.playdate_id from playdate join playdate_personality_permitted as ppp on playdate.playdate_id = ppp.playdate_id where (((personality_id_is_permitted = 1) and(personality_id in ({0}))) or(-1 in ({0})))";
        private const string SQL_GET_PLAYDATE_IDS_BY_PROHIBITED_PERSONALITIES_ARRAY = "select distinct playdate.playdate_id from playdate join playdate_personality_permitted as ppp on playdate.playdate_id = ppp.playdate_id where (((personality_id_is_permitted = 0) and(personality_id in ({0}))) or(-1 in ({0})))";
        private const string SQL_GET_PLAYDATE_IDS_BY_DISTANCE_FROM_CENTER_POINT = "select playdate_id from (select *, (distance_km * 0.62137)as distance_mi from (select *,dbo.Haversine_km(@centerLat,@centerLng,lat,lng) as distance_km from fullPlaydate)as km) as fullPlaydate_and_distance where( (distance_km <= @radius) or (@radius =-1) )";//todo: maybe make this only use km on backend. we can convert to miles on front end
        //these are for inserting the restrictions for pet types and personalities
        private const string SQL_OVERWRITE_PLAYDATE_PERSONALITY_PERMITTED_BY_PLAYDATE_ID = "begin transaction;delete from playdate_personality_permitted where playdate_id = @playdateId; begin try insert into playdate_personality_permitted (playdate_id,personality_id,personality_id_is_permitted) values {0};end try begin catch end catch; commit transaction;";
        private const string SQL_OVERWRITE_PLAYDATE_PET_TYPE_PERMITTED_BY_PLAYDATE_ID = "begin transaction;delete from playdate_pet_type_permitted where playdate_id = @playdateId; begin try insert into playdate_pet_type_permitted (playdate_id, pet_type_id, pet_type_id_is_permitted) values {0};end try begin catch end catch; commit transaction;";
        //adding a pet to a playdate
        private const string SQL_IS_PET_ATTENDING_PLAYDATE = "select * from playdate_pet where playdate_id = @playdateId and pet_id = @petId;";
        private const string SQL_ADD_PET_TO_PLAYDATE = "insert into playdate_pet(playdate_id, pet_id) Values(@playdateId, @petId);";
        private const string SQL_REMOVE_PET_FROM_PLAYDATE = "delete from playdate_pet where playdate_id = @playdateId and pet_id = @petId;";
        private const string SQL_OVERWRITE_PLAYDATE_PET_BY_PLAYDATE_ID = "begin transaction; delete from playdate_pet where playdate_id = @playdateId; insert into playdate_pet (playdate_id, pet_id) values {0}; commit transaction";

        //update playdate

        public PlaydateDAO(string connectionString)
        {
            this.connectionString = connectionString;
            this.petDAO = new PetDAO(connectionString);
        }

        public List<Playdate> GetPlaydates(PlaydateSearchFilter filter)
        {
            List<Playdate> playdates = new List<Playdate>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;

                    //Lets start building this insane SQL query
                    StringBuilder queryBuilder = new StringBuilder(SQL_GET_ALL_PLAYDATES);
                    //start the big ol' WHERE clause
                    queryBuilder.Append(" where( ");
                    #region filter on userId

                    queryBuilder.Append("(@userId = -1 OR user_id = @userId)");
                    cmd.Parameters.AddWithValue("@userId", filter.userId);
                    #endregion

                    #region filter on allowed personalities
                    if (filter.allowedPersonalities.Count > 0)
                    {
                        ParameterizedSqlArray<int> allowedPersonalitiesArray = new ParameterizedSqlArray<int>(
                            $"and (playdate_id in ({SQL_GET_PLAYDATE_IDS_BY_PERMITTED_PERSONALITIES_ARRAY}))",
                            filter.allowedPersonalities,
                            "allowedPersonalities"
                            );
                        queryBuilder.Append(allowedPersonalitiesArray.Snippet);
                        cmd.Parameters.AddRange(allowedPersonalitiesArray.Parameters);
                    }

                    #endregion

                    #region filter on disallowed personalities
                    if (filter.disallowedPersonalities.Count > 0)
                    {
                        ParameterizedSqlArray<int> disallowedPersonalitiesArray = new ParameterizedSqlArray<int>(
                            $"and (playdate_id in ({SQL_GET_PLAYDATE_IDS_BY_PROHIBITED_PERSONALITIES_ARRAY}))",
                            filter.disallowedPersonalities,
                            "disallowedPersonalities"
                            );
                        queryBuilder.Append(disallowedPersonalitiesArray.Snippet);
                        cmd.Parameters.AddRange(disallowedPersonalitiesArray.Parameters);
                    }
                    #endregion

                    #region filter on allowed petTypes
                    if (filter.allowedPetTypes.Count > 0)
                    {
                        ParameterizedSqlArray<int> allowedPetTypesArray = new ParameterizedSqlArray<int>(
                            $"and (playdate_id in ({SQL_GET_PLAYDATE_IDS_BY_PERMITTED_PET_TYPES_ARRAY}))",
                            filter.allowedPetTypes,
                            "allowedPetTypes"
                            );
                        queryBuilder.Append(allowedPetTypesArray.Snippet);
                        cmd.Parameters.AddRange(allowedPetTypesArray.Parameters);
                    }
                    #endregion

                    #region filter on disallowed petTypes
                    if (filter.disallowedPetTypes.Count > 0)
                    {
                        ParameterizedSqlArray<int> disallowedPetTypesArray = new ParameterizedSqlArray<int>(
                            $"and (playdate_id in ({SQL_GET_PLAYDATE_IDS_BY_PROHIBITED_PET_TYPES_ARRAY}))",
                            filter.disallowedPetTypes,
                            "disallowedPetTypes"
                            );
                        queryBuilder.Append(disallowedPetTypesArray.Snippet);
                        cmd.Parameters.AddRange(disallowedPetTypesArray.Parameters);
                    }
                    #endregion

                    #region filter by location
                    queryBuilder.Append($"and (playdate_id in ({SQL_GET_PLAYDATE_IDS_BY_DISTANCE_FROM_CENTER_POINT}))");
                    cmd.Parameters.AddWithValue("@centerLat", filter.searchCenter.Lat);
                    cmd.Parameters.AddWithValue("@centerLng", filter.searchCenter.Lng);
                    cmd.Parameters.AddWithValue("@radius", filter.searchRadius);


                    #endregion

                    //end the big ol' where clause
                    queryBuilder.Append(");");

                    cmd.CommandText = queryBuilder.ToString();

                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        Playdate playdate = RowToObject(rdr);
                        playdates.Add(playdate);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return playdates;
        }


        //add a pet to a playdate
        public bool AddPetToPlaydate(int petId, int playdateId)
        {
            bool isSuccssful = false;
            try
            {
                if (IsPetAttendingPlaydate(petId, playdateId))
                {
                    isSuccssful = true;
                }
                else
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        //before we insert the pet and playdate, lets check if it dosnt already exist
                        SqlCommand cmd = new SqlCommand(SQL_ADD_PET_TO_PLAYDATE, conn);
                        cmd.Parameters.AddWithValue("@playdateId", playdateId);
                        cmd.Parameters.AddWithValue("@petId", petId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        isSuccssful = rowsAffected > 0;
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return isSuccssful;
        }
        //checks if a given pet is attending a given playdate
        public bool IsPetAttendingPlaydate(int petId, int playdateId)
        {
            bool attending = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();


                    SqlCommand cmd = new SqlCommand(SQL_IS_PET_ATTENDING_PLAYDATE, conn);
                    cmd.Parameters.AddWithValue("@playdateId", playdateId);
                    cmd.Parameters.AddWithValue("@petId", petId);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        attending = true;
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return attending;
        }

        //remove pet from a playdate
        public bool RemovePetFromPlaydate(int petId, int playdateId)
        {
            bool isSuccssful = false;
            try
            {

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    //before we insert the pet and playdate, lets check if it dosnt already exist
                    SqlCommand cmd = new SqlCommand(SQL_REMOVE_PET_FROM_PLAYDATE, conn);
                    cmd.Parameters.AddWithValue("@playdateId", playdateId);
                    cmd.Parameters.AddWithValue("@petId", petId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    isSuccssful = true;

                }
            }
            catch (SqlException)
            {
                throw;
            }
            return isSuccssful;

        }

        //update playdate
        public Playdate UpdatePlaydate(Playdate playdateToUpdate)
        {
            Playdate updatedPlaydate = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("update playdate set start_date_time = @startDateTime, end_date_time = @endDateTime, location_id = @locationId where playdate_id = @playdateId;", conn);
                    cmd.Parameters.AddWithValue("@startDateTime", playdateToUpdate.StartDateTime);
                    cmd.Parameters.AddWithValue("@endDateTime", playdateToUpdate.EndDateTime);
                    cmd.Parameters.AddWithValue("@locationId", playdateToUpdate.location.LocationId);
                    cmd.Parameters.AddWithValue("@playdateId", playdateToUpdate.PlaydateId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        updatedPlaydate = GetPlaydateById(playdateToUpdate.PlaydateId);
                    }

                }
            }
            catch (SqlException)
            {
                throw;
            }
            return updatedPlaydate;
        }





        //get a list of all playdates 
        public List<Playdate> GetAllPlaydates()
        {
            List<Playdate> playdates = new List<Playdate>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_ALL_PLAYDATES, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        Playdate playdate = RowToObject(rdr);
                        playdates.Add(playdate);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return playdates;
        }

        public List<Playdate> GetPlaydatesByUserId(int userId)
        {
            List<Playdate> playdates = new List<Playdate>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_PLAYDATES_BY_USERID, conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        Playdate playdate = RowToObject(rdr);
                        playdates.Add(playdate);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return playdates;
        }

        //get a list of all playdates after given date?? 

        //get a specific playdate by id
        public Playdate GetPlaydateById(int playdateId)
        {
            Playdate playdate = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GETPLAYDATEBYID, conn);
                    cmd.Parameters.AddWithValue("@playdate_id", playdateId);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        playdate = RowToObject(rdr);
                    }

                }

            }
            catch (SqlException)
            {
                throw;
            }

            return playdate;
        }

        //add a new playdate 
        public int AddPlaydate(Playdate playdateToAdd)
        {
            int playdateId = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_ADDPLAYDATE, conn);
                    cmd.Parameters.AddWithValue("@startDateTime", playdateToAdd.StartDateTime);
                    cmd.Parameters.AddWithValue("@endDateTime", playdateToAdd.EndDateTime);
                    cmd.Parameters.AddWithValue("@userId", playdateToAdd.UserId);
                    cmd.Parameters.AddWithValue("@location_id", playdateToAdd.location.LocationId);
                    playdateId = Convert.ToInt32(cmd.ExecuteScalar());
                    playdateToAdd.PlaydateId = playdateId;
                    bool petTypeSuccess = OverwritePlaydatePetTypePermittedByPlaydateId(playdateId, playdateToAdd.PetTypesPermitted);
                    bool personalitySuccess = OverwritePlaydatePersonalityPermittedByPlaydateId(playdateId, playdateToAdd.PersonalitiesPermitted);
                    if (!petTypeSuccess || !personalitySuccess)
                    {
                        playdateId = -1;
                    }

                }
            }
            catch (SqlException)
            {
                throw;
            }

            return playdateId;

        }

        //get petTypes permitted by playdateId
        public Dictionary<int, bool> GetPetTypesPermittedByPlaydateId(int playdateId)
        {
            Dictionary<int, bool> petTypesPermitted = new Dictionary<int, bool>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_PET_TYPES_PERMITTED_BY_PLAYDATE_ID, conn);
                    cmd.Parameters.AddWithValue("@playdateId", playdateId);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        int key = Convert.ToInt32(rdr["pet_type_id"]);
                        bool value = Convert.ToBoolean(rdr["pet_type_id_is_permitted"]);
                        petTypesPermitted[key] = value;
                    }

                }

            }
            catch (SqlException)
            {
                throw;
            }

            return petTypesPermitted;
        }

        //get personalities permitted by playdateId
        public Dictionary<int, bool> GetPersonalitiesPermittedByPlaydateId(int playdateId)
        {
            Dictionary<int, bool> personalitiesPermitted = new Dictionary<int, bool>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_PERSONALITIES_PERMITTED_BY_PLAYDATE_ID, conn);
                    cmd.Parameters.AddWithValue("@playdateId", playdateId);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        int key = Convert.ToInt32(rdr["personality_id"]);
                        bool value = Convert.ToBoolean(rdr["personality_id_is_permitted"]);
                        personalitiesPermitted[key] = value;
                    }

                }

            }
            catch (SqlException)
            {
                throw;
            }

            return personalitiesPermitted;
        }

        //overwrites the playdate_personaliy_permitted table with provided valyes
        public bool OverwritePlaydatePersonalityPermittedByPlaydateId(int playdateId, Dictionary<int, bool> personalitiesPermitted)
        {
            bool isSuccessful = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@playdateId", playdateId);
                    List<string> sqlValuesToInsert = new List<string>();
                    int i = 0;
                    foreach (KeyValuePair<int, bool> kvp in personalitiesPermitted)
                    {
                        sqlValuesToInsert.Add($"(@playdateId,@personalityId{i},@personalityIdIsPermitted{i})");
                        cmd.Parameters.AddWithValue($"@personalityId{i}", kvp.Key);
                        cmd.Parameters.AddWithValue($"@personalityIdIsPermitted{i}", kvp.Value);
                        i++;
                    }
                    cmd.CommandText = String.Format(SQL_OVERWRITE_PLAYDATE_PERSONALITY_PERMITTED_BY_PLAYDATE_ID, string.Join(",", sqlValuesToInsert));
                    int rowsAffected = cmd.ExecuteNonQuery();
                    isSuccessful = true;
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return isSuccessful;
        }

        //overwrites the playdate_pet_type_permitted table with provided values
        public bool OverwritePlaydatePetTypePermittedByPlaydateId(int playdateId, Dictionary<int, bool> petTypesPermitted)
        {
            bool isSuccessful = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@playdateId", playdateId);
                    List<string> sqlValuesToInsert = new List<string>();
                    int i = 0;
                    foreach (KeyValuePair<int, bool> kvp in petTypesPermitted)
                    {
                        sqlValuesToInsert.Add($"(@playdateId,@petTypeId{i},@petTypeIdIsPermitted{i})");
                        cmd.Parameters.AddWithValue($"@petTypeId{i}", kvp.Key);
                        cmd.Parameters.AddWithValue($"@petTypeIdIsPermitted{i}", kvp.Value);
                        i++;
                    }
                    cmd.CommandText = String.Format(SQL_OVERWRITE_PLAYDATE_PET_TYPE_PERMITTED_BY_PLAYDATE_ID, string.Join(",", sqlValuesToInsert));
                    int rowsAffected = cmd.ExecuteNonQuery();
                    isSuccessful = true;
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return isSuccessful;
        }

        //overwrite the playdate_pet table with the provided valyes. basically adding particiapnt pets to the playdate
        public bool OverwritePlaydatePetByPlaydateId(int playdateId, List<int> petIds)
        {
            bool isSuccessful = false;
            try
            {

                //;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue("@playdateId", playdateId);
                    List<string> sqlValuesToInsert = new List<string>();
                    int i = 0;
                    foreach (int petId in petIds)
                    {
                        sqlValuesToInsert.Add($"(@playdateId,@petId{i})");
                        cmd.Parameters.AddWithValue($"@petId{i}", petId);
                        i++;
                    }
                    cmd.CommandText = String.Format(SQL_OVERWRITE_PLAYDATE_PET_BY_PLAYDATE_ID, string.Join(",", sqlValuesToInsert));
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0) { isSuccessful = true; }
                }

            }
            catch (SqlException)
            {
                throw;
            }
            return isSuccessful;
        }

        private Playdate RowToObject(SqlDataReader rdr)
        {
            int playdateId = Convert.ToInt32(rdr["playdate_id"]);
            Playdate playdate = new Playdate()
            {
                PlaydateId = playdateId,
                StartDateTime = Convert.ToDateTime(rdr["start_date_time"]),
                EndDateTime = Convert.ToDateTime(rdr["end_date_time"]),
                UserId = Convert.ToInt32(rdr["user_id"]),
                location = new Location()
                {
                    LocationId = Convert.ToInt32(rdr["location_id"]),
                    Name = Convert.ToString(rdr["location_name"]),
                    Address = Convert.ToString(rdr["address"]),
                    Lat = Convert.ToSingle(rdr["lat"]),
                    Lng = Convert.ToSingle(rdr["lng"])
                },
                Description = Convert.ToString(rdr["description"]),
                PetTypesPermitted = GetPetTypesPermittedByPlaydateId(playdateId),
                PersonalitiesPermitted = GetPersonalitiesPermittedByPlaydateId(playdateId),
                UserName = Convert.ToString(rdr["username"]),
                Participants = petDAO.GetParticipantPetsByPlaydateId(playdateId)
            };


            return playdate;
        }

    }


}
