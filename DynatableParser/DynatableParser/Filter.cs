using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace DynatableParser
{
    internal class Filter<T>
    {
        public IQueryable<T> FilteredData { set; get; }

        public Filter(IQueryable<T> data, String searchText)
        {
            // If there is no search text, return the input data
            if (String.IsNullOrWhiteSpace(searchText))
            {
                FilteredData = data;
                return;
            }

            var parameterExpression = Expression.Parameter(typeof(T), "x");
            var properties = typeof(T).GetProperties().Where(x => x.CanRead);

            // Create the body of our lambda expression which is the OR operation of various search expression
            // Each search expression is created based on its type
            Expression bodyExpression = Expression.Constant(false);
            foreach(var property in properties)
            {
                SearchExpressionFactory factory = new SearchExpressionFactory(parameterExpression, property, searchText);
                Expression searchExpression = factory.SearchExpression;
                if (searchExpression != null)
                    bodyExpression = Expression.Or(bodyExpression, searchExpression);
            }

            var funcType = Expression.GetFuncType(typeof(T), typeof(bool));

            // Creates the lambda expression of type Func<T,PropertyType>, body of x.PropertyName, and input parameter x (ie. x => x.Id)
            LambdaExpression lambdaExpression = Expression.Lambda(funcType, bodyExpression, parameterExpression);

            // Extract the Where method and replace the generic arguments with T
            String reflectionMethodName = "System.Linq.IQueryable`1[TSource] Where[TSource](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,System.Boolean]])";
            var method = typeof(Queryable).GetMethods().Single(x => x.ToString() == reflectionMethodName).MakeGenericMethod(typeof(T));

            // Invoke the where method passing in the lambda expression and data
            FilteredData = method.Invoke(null, new object[] { data, lambdaExpression }) as IQueryable<T>;
        }
    }
}