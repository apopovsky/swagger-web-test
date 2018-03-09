using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace SwaggerWeb.Controllers
{
    [Route("api/{sector:sector}/[controller]")]
    public class ValuesController : Controller
    {
        /// <summary>
        /// Get by ID
        /// </summary>
        /// <param name="id">The value ID</param>
        /// <returns></returns>
        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }
    }
}
