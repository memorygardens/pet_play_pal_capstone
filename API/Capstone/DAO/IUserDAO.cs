using System.Collections.Generic;
using Capstone.Models;

namespace Capstone.DAO
{
    public interface IUserDAO
    {
        User GetUser(string username);
        User AddUser(string username, string password, string role);
        int UpdateUsername(string username, int userId);
        User GetUsernameById(int id);
    }
}
