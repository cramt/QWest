using Microsoft.AspNetCore.Mvc;
using Model;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QWest.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressMapController : ControllerBase {

        private DAO.IProgressMap _progressMapRepo = null;
        public DAO.IProgressMap ProgressMapRepo {
            get {
                if (_progressMapRepo == null) {
                    _progressMapRepo = DAO.ProgressMap;
                }
                return _progressMapRepo;
            }
            set {
                _progressMapRepo = value;
            }
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<ProgressMap>> UserId(int? id = null) {
            if (id == null) {
                User user = Request.GetUser();
                if (user == null) {
                    return Unauthorized();
                }
                return Ok(await ProgressMapRepo.Get(user));
            }
            else {
                return Ok(await ProgressMapRepo.GetByUserId((int)id));
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ProgressMap>> Get(int id) {
            return Ok(await ProgressMapRepo.Get(id));
        }
        public class UpdateArgument {
            public int id;
            public List<int> additions;
            public List<int> subtractions;
        }
        [HttpPost("update")]
        public async Task<ActionResult<bool>> Update([FromBody] UpdateArgument argument) {
            User user = Request.GetUser();
            ProgressMap map = await ProgressMapRepo.Get(user);
            if (argument.id != map.Id) {
                return Unauthorized();
            }
            await ProgressMapRepo.Update(argument.id, argument.additions, argument.subtractions);
            return Ok(true);
        }
    }
}
