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
    [Route("[controller]")]
    [ApiController]
    public class SampleController : AuthorizedControllerBase
    {
        [HttpGet]
        public ActionResult GetSamples()
        {
            return Ok(new Object[]
            {
                new { Code = "AZ", Name="Arizona"},
                new { Code = "AK", Name="Alaska"},
                new { Code = "NH", Name="New Hampshire"},
                new { Code = "OH", Name="Ohio"},
            });
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult GetUserInfo()
        {
            string result = $"Your user name is {UserName}, user id is {UserId}, and your role is {UserRole}.";
            return Ok(result);
        }

    }
}
