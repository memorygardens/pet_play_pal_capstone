using Capstone.Models;
using System.Collections.Generic;

namespace Capstone.DAO
{
    public interface ILocationDAO
    {
        int AddLocation(Location locationToAdd);
        List<Location> GetAllLocations();
        int GetIdByLocation(Location location);
        Location GetLocationById(int locationId);
    }
}