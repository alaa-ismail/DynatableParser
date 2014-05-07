using MvcDemo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using DynatableParser;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MvcDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public String EmployeeData()
        {
            var rawParams = Request.QueryString;
            var parameters = rawParams.Cast<string>().ToDictionary(k => k, v => rawParams[v]);
            
            AdventureWorksEntities dbContext = new AdventureWorksEntities();
            DynatableParser<Employee> parser = new DynatableParser<Employee>(parameters, dbContext.Employee);

            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            return JsonConvert.SerializeObject(parser.Result, camelCaseFormatter);
        }

     
    }
}