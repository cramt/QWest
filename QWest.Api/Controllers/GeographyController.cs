using Microsoft.AspNetCore.Mvc;
using Model.Geographic;
using QWest.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QWest.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class GeographyController : ControllerBase {

        private DAO.IGeography _geographyRepo = null;
        public DAO.IGeography GeographyRepo {
            get {
                if (_geographyRepo == null) {
                    _geographyRepo = DAO.Geography;
                }
                return _geographyRepo;
            }
            set {
                _geographyRepo = value;
            }
        }

        [HttpGet("get/{alpha2}")]
        public async Task<ActionResult<GeopoliticalLocation>> Get(string alpha2) {
            alpha2 = alpha2.ToUpper();
            Console.WriteLine(alpha2);
            GeopoliticalLocation local = await GeographyRepo.GetAnyByAlpha2s(alpha2);
            if (local == null) {
                return NotFound();
            }
            else {
                return Ok(local);
            }
        }

        [HttpGet("get")]
        public async Task<ActionResult<IEnumerable<Country>>> Get() {
            return Ok(await GeographyRepo.FetchEverythingParsed());
        }
        
        [HttpGet("get/countries")]
        public async Task<ActionResult<IEnumerable<Country>>> GetCountries() {
            return Ok(await GeographyRepo.GetCountries());
        }

        [HttpGet("get/subdivisions")]
        public async Task<ActionResult<IEnumerable<Subdivision>>> GetSubdivisions(int superId) {
            var results = await GeographyRepo.GetSubdivisions(superId);
            if (results.Count() == 0) {
                return NotFound();
            }
            return Ok(results);
        }

        [HttpGet("search/{searchTerm}")]
        public async Task<ActionResult<IEnumerable<GeopoliticalLocation>>> NameSearch(string searchTerm) {
            return Ok(await GeographyRepo.NameSearch(searchTerm));
        }
    }
}
