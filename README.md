DynatableParser
===============

DynatableParser is a C# library to handle server-side processing for [Dynatable]. It converts the AJAX request parameters sent by Dynatable into LINQ operations on the server-side dataset.

Installation
--------------
Use the Nuget Package Manager to install.

```Install-Package DynatableParser ```

Usage
-------
Provide the DynatableParser constructor with a list of key-value pairs containing the client-side query and an EF dataset.

#####ASP.NET Web API Example:
```
    public class DataController : ApiController
    {
        public DynatableResult<Employee> Get()
        {
            // Get query parameters        
            var parameters = Request.GetQueryNameValuePairs();
            
            // Entity Framework context 
            AdventureWorksEntities dbContext = new AdventureWorksEntities();
            
            // Create the parser
            DynatableParser<Employee> parser = new DynatableParser<Employee>(parameters, dbContext.Employee);
            
            return parser.Result;
        }
    }
```
#####ASP.NET MVC Example:
```
    public class HomeController : Controller
    {
        public String EmployeeData()
        {
            var rawParams = Request.QueryString;
            
            // Convert parameters to a dictionary
            var parameters = rawParams.Cast<string>().ToDictionary(k => k, v => rawParams[v]);
            
            // Entity Framework context
            AdventureWorksEntities dbContext = new AdventureWorksEntities();
            
            // Create the parser
            DynatableParser<Employee> parser = new DynatableParser<Employee>(parameters, dbContext.Employee);
            
            // Set up JSON formatter
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            
            return JsonConvert.SerializeObject(parser.Result, camelCaseFormatter);
        }
    }
```

Issues
-------
Please feel free to submit any issues here on Github.

https://github.com/alaa-ismail/DynatableParser/issues



[Dynatable]:https://www.dynatable.com/
