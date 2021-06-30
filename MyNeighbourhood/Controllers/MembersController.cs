using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyNeighbourhood.Persistent.DbServices;

namespace MyNeighbourhood.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/forecast")]
    public class MembersController : Controller
    {
        private readonly IMembersService _membersService;

        public MembersController(IMembersService membersService)
        {
            this._membersService = membersService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var result = await this._membersService.GetMembers();

                if (result.Any())
                    return Ok(result);

                return StatusCode((int)HttpStatusCode.NotFound, "We can't find any members");
            }
            catch (Exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "We malfunctioned. Not your fault.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            throw new NotImplementedException();
        }

        [HttpPut]
        public async Task<IActionResult> Put()
        {
            throw new NotImplementedException();
        }

        [HttpPatch]
        public async Task<IActionResult> Patch()
        {
            throw new NotImplementedException();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            throw new NotImplementedException();
        }
    }
}