DynatableParser
===============

DynatableParser is a C# library to handle server-side processing for Dynatable. It converts the AJAX request parameters sent 
by Dynatable into LINQ operations on the server-side dataset.

Installation
--------------
Use Nuget Package Manager to install 
```Install-Package DynatableParser ```

Usage
-------
To use DynatableParser, you need to supply a list key-value pairs representing the client-side query parameters and a dataset.

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
            
            // Create parser
            DynatableParser<Employee> parser = new DynatableParser<Employee>(parameters, dbContext.Employee);
            
            return parser.Result;
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
            
            // Create parser
            DynatableParser<Employee> parser = new DynatableParser<Employee>(parameters, dbContext.Employee);
            
            // Set up JSON formatter
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            
            
            return JsonConvert.SerializeObject(parser.Result, camelCaseFormatter);
        }
    }
```


