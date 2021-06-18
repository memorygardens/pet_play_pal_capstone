using Capstone.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.DAO
{
    public class LocationSqlDAO : ILocationDAO
    {
        private const string SQL_GET_ALL_LOCATIONS = "select * from location";
        private const string SQL_GET_LOCATION_BY_ID = "select * from location where location_id = @locationId";
        private const string SQL_GET_ID_BY_LOCATION = "select location_id from location where name = @name AND address = @address AND lat = @lat AND lng = @lng";
        private const string SQL_ADD_NEW_LOCATION = "insert into location(name,address,lat,lng) values(@name,@address,@lat,@lng); select @@IDENTITY;";
        private readonly string connectionString;
        public LocationSqlDAO(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Gets a list of all locations in the database.
        /// </summary>
        /// <returns>a list of <see cref="Location">Locations</see> </returns>
        public List<Location> GetAllLocations()
        {
            List<Location> locationsToReturn = new List<Location>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_ALL_LOCATIONS, conn);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        locationsToReturn.Add(rowToObject(rdr));
                    }
                }

            }
            catch (SqlException)
            {
                throw;
            }
            return locationsToReturn;
        }


        /// <summary>
        /// Gets a location object by a given locationId.
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns> a <see cref="Location"/> object for a given locationID, or null of no such location is found</returns>
        public Location GetLocationById(int locationId)
        {
            Location location = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_LOCATION_BY_ID, conn);
                    cmd.Parameters.AddWithValue("@locationId", locationId);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        location = (rowToObject(rdr));
                    }
                }

            }
            catch (SqlException)
            {
                throw;
            }
            return location;
        }

        /// <summary>
        /// Given a location object, find it's location_id in the database. If no such location exists, return -1.
        /// The purpose of this method is to prevent duplicate locations from being added to the DB.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>the location_id of the location, or -1 if not found</returns>
        public int GetIdByLocation(Location location)
        {
            int locationId = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_GET_ID_BY_LOCATION, conn);
                    cmd.Parameters.AddWithValue("@name", location.Name);
                    cmd.Parameters.AddWithValue("@address", location.Address);
                    cmd.Parameters.AddWithValue("@lat", location.Lat);
                    cmd.Parameters.AddWithValue("@lng", location.Lng);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        locationId = Convert.ToInt32(rdr["location_id"]);
                    }
                }

            }
            catch (SqlException)
            {
                throw;
            }
            return locationId;
        }

        /// <summary>
        /// Adds a new location into the database
        /// </summary>
        /// <param name="locationToAdd">The location to add.</param>
        /// <returns>the ID of the newly added location</returns>
        public int AddLocation(Location locationToAdd)
        {
            int locationId = -1;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(SQL_ADD_NEW_LOCATION, conn);
                    cmd.Parameters.AddWithValue("@name", locationToAdd.Name);
                    cmd.Parameters.AddWithValue("@address", locationToAdd.Address);
                    cmd.Parameters.AddWithValue("@lat", locationToAdd.Lat);
                    cmd.Parameters.AddWithValue("@lng", locationToAdd.Lng);
                    locationId = Convert.ToInt32(cmd.ExecuteScalar());
                }

            }
            catch (SqlException)
            {
                throw;
            }
            return locationId;
        }

        private Location rowToObject(SqlDataReader rdr)
        {
            Location location = new Location()
            {
                LocationId = Convert.ToInt32(rdr["location_id"]),
                Name = Convert.ToString(rdr["name"]),
                Address = Convert.ToString(rdr["address"]),
                Lat = Convert.ToSingle(rdr["lat"]),
                Lng = Convert.ToSingle(rdr["lng"])

            };
            return location;
        }
    }
}
