using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Capstone.Controllers
{
    [ApiController]
    public class AuthorizedControllerBase : ControllerBase
    {
        #region Methods for getting the Logged-in user information
        /**************************************************************************************************
         *  Methods for getting the Logged-in user information
         *  1. For these methods to work, they MUST be called from a controller action that had the [Authorize] attribute.
         *  2. If you need access to this information from MORE THAN ONE controller, consider creating a controller
         *      base class that contains these methods (protected, not private), and derive each of your controllers
         *      from that base class.
         *************************************************************************************************/
        protected string UserName
        {
            get
            {
                return User.Identity.Name;
            }
        }

        protected int UserId
        {
            get
            {
                return Convert.ToInt32(User.FindFirst("sub")?.Value ?? "0");
            }
        }
        protected string UserRole
        {
            get
            {
                return User.Claims.FirstOrDefault(cl => cl.Type == ClaimTypes.Role)?.Value;
            }
        }
        /**************************************************************************************************
         *  END OF Methods for getting the Logged-in user information
         *************************************************************************************************/
        #endregion
    }
}
