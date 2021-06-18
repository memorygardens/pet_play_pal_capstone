using Capstone.Controllers.searchfilters;
using Capstone.DAO;
using Capstone.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Controllers
{
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class PlaydatesController : AuthorizedControllerBase
    {
        private readonly IPlaydateDAO playdateDao;
        private readonly ILocationDAO locationDao;
        public PlaydatesController(IPlaydateDAO playdateDAO, ILocationDAO locationDAO, IPetDAO petDAO)
        {
            this.playdateDao = playdateDAO;
            this.locationDao = locationDAO;
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult<List<Playdate>> GetAllPlaydates()
        {
            List<Playdate> playdates = playdateDao.GetAllPlaydates();


            if (playdates == null)
            {
                return NoContent();
            }
            return Ok(playdates);
        }

        /// <summary>
        /// Gets a list of playdates that meet the search criteria. If a search criteria is left blank, it is not used.
        /// </summary>
        /// <returns>a list of all <see cref="Playdate"/> objects in the database</returns>
        [AllowAnonymous]
        [HttpPost("search")]
        public ActionResult<List<Playdate>> GetPlaydates(PlaydateSearchFilter filter)
        {
            List<Playdate> playdates = playdateDao.GetPlaydates(filter);


            if (playdates == null)
            {
                return NoContent();
            }
            return Ok(playdates);
        }

        /// <summary>
        /// Gets a playdate by ID
        /// </summary>
        /// <param name="id">The Id of the playdate to get</param>
        /// <returns>a <see cref="Playdate"/> object</returns>
        [HttpGet("{id}")]
        public ActionResult<Playdate> getPlaydateById(int id)
        {
            Playdate playdate = playdateDao.GetPlaydateById(id);
            if (playdate != null)
            {
                return Ok(playdate);
            }
            else
            {
                return NoContent();
            }
        }

        //add playdate
        [HttpPost()]
        public ActionResult<Playdate> CreatePlaydate(Playdate playdateToAdd)
        {
            //check if the playdate object is fully formed
            if (playdateToAdd.location == null)
            {
                return BadRequest("The playdate did not have a valid location");
            }

            //if a location ID was not specified, but location data was, we need to check if a similar location already exists in the DB
            if (playdateToAdd.location.LocationId == -1)
            {
                //lets look for a similar location
                int LocationIdOnDB = locationDao.GetIdByLocation(playdateToAdd.location);
                if (LocationIdOnDB == -1)
                {
                    //this means that the location is NOT in the database, so we must add it
                    LocationIdOnDB = locationDao.AddLocation(playdateToAdd.location);
                }
                playdateToAdd.location.LocationId = LocationIdOnDB;

            }
            //set the userID of the new playdate to the currently logged in user
            playdateToAdd.UserId = this.UserId;

            //now the location of the playdate should be properly in the DB

            int playdateId = playdateDao.AddPlaydate(playdateToAdd);
            if (playdateId < 1)
            {
                return StatusCode(500, "Something went wrong when trying to add your new playdate!");
            }
            else return Ok(playdateToAdd);
        }

        [HttpPut("{playdateId}")]
        public ActionResult<Playdate> UpdatePlaydate(int playdateId, Playdate playdateToUpdate)
        {
            Playdate updatedPlaydate = null;
            //first, check if shenanigans are afoot
            if (playdateId != playdateToUpdate.PlaydateId)
            {
                return BadRequest("The playdateID indicated does not match the actual ID of the playdate.");
            }
            #region update location

            //lets look for a similar location
            int LocationIdOnDB = locationDao.GetIdByLocation(playdateToUpdate.location);
            if (LocationIdOnDB == -1)
            {
                //this means that the location is NOT in the database, so we must add it
                LocationIdOnDB = locationDao.AddLocation(playdateToUpdate.location);
            }
            playdateToUpdate.location.LocationId = LocationIdOnDB;
            #endregion

            #region update allowed pet types
            //next update allowed pet types
            bool successfulPetTypesUpdate = playdateDao.OverwritePlaydatePetTypePermittedByPlaydateId(playdateId, playdateToUpdate.PetTypesPermitted);
            if (!successfulPetTypesUpdate)
            {
                return StatusCode(500, "Internal Error. Something went wrong when trying to update your playdate! (unsuccessful pet type update)");
            }
            #endregion

            #region update allowed personalities
            bool successfulPersonalitiesUpdate = playdateDao.OverwritePlaydatePersonalityPermittedByPlaydateId(playdateId, playdateToUpdate.PersonalitiesPermitted);
            if (!successfulPersonalitiesUpdate)
            {
                return StatusCode(500, "Internal Error. Something went wrong when trying to update your playdate! (unsuccessful personality update)");
            }
            #endregion

            #region update participants
            bool successfulParticipantUpdate = playdateDao.OverwritePlaydatePetByPlaydateId(playdateId, playdateToUpdate.Participants.Select(pet => pet.PetId).ToList());//yeah lambda!
            if (!successfulParticipantUpdate)
            {
                return StatusCode(500, "Internal Error. Something went wrong when trying to update your playdate! (unsuccessful participant update)");
            }
            #endregion

            //finally, re-read the playdate from the DB and send it back

            updatedPlaydate = playdateDao.GetPlaydateById(playdateId);

            return Ok(updatedPlaydate);

        }

    }
}
