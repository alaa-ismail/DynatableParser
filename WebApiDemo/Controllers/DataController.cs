using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApiDemo.Models;
using DynatableParser;

namespace WebApiDemo.Controllers
{
    public class DataController : ApiController
    {
        public DynatableResult<Employee> Get()
        {
            AdventureWorksEntities dbContext = new AdventureWorksEntities();
            DynatableParser<Employee> parser = new DynatableParser<Employee>(Request.GetQueryNameValuePairs(), dbContext.Employee);
            return parser.Result;
        }

    }
}