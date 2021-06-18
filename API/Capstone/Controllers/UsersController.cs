using Capstone.DAO;
using Capstone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : AuthorizedControllerBase
    {
        private readonly IUserDAO userDAO;

        public UsersController(IUserDAO userDAO)
        {
            this.userDAO = userDAO;
        }

        [HttpPut("{id}")]
        public ActionResult<User> UpdateUsername(int id, ReturnUser user)
        {
            User updatedUser = null;

            if (userDAO.UpdateUsername(user.Username, id) == 1)
            {
                updatedUser = userDAO.GetUsernameById(id);
                return Ok(updatedUser); 
            }
            
            else
            {
                return BadRequest();
            }

        }                                         

    }
}
