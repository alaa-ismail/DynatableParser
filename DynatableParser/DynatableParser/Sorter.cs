using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace DynatableParser
{
    internal class Sorter<T>
    {
        public IQueryable<T> SortedData { set; get; }

        public Sorter(IQueryable<T> data, IEnumerable<KeyValuePair<string, string>> sortParameters)
        {        
            // If there are no sort parameters, sort by the first property available. 
            // At least 1 OrderBy expression is required for using the Skip operator for paging
            if(sortParameters.Count() == 0)
            {
                PropertyInfo property = typeof(T).GetProperties().First();
                var defaultSort = new KeyValuePair<string, string>(property.Name, "1");
                sortParameters = new List<KeyValuePair<string, string>>() { defaultSort };
            }

            for (int i = 0; i < sortParameters.Count();i++)
            {
                var parameter = sortParameters.ElementAt(i);

                // Lookup the property name. TODO: add code to accomedate special characters like _
                PropertyInfo property = typeof(T).GetProperties().Where(x => x.Name.Equals(parameter.Key, StringComparison.InvariantCultureIgnoreCase)).First();
                Type propertyType = property.PropertyType;

                Expression lambdaExpression = GetLambdaExpression(property);
                MethodInfo sortMethod = GetSortMethod(property, i+1, parameter.Value);

                // Invoke the sort method passing in the lambda expression and data. Since OrderBy is an extension method, the IQueryable object is actually a parameter in IL
                data = sortMethod.Invoke(null, new object[] { data, lambdaExpression }) as IQueryable<T>;
            }

            SortedData = data;
        }

        /// <summary>
        /// Creates the lambda expression to be passed into the OrderBy expression. 
        /// This is simply an expression that access the provided property (ie. x=>x.propertyName)
        /// </summary>
        private Expression GetLambdaExpression(PropertyInfo property)
        {
                // Create the parameter of the lambda expression
                var paramExp = Expression.Parameter(typeof(T), "x");

                // Create a new type of Func<T,PropertyType> (ie. if we are sorting by Employee Id (int) then we would create a Func<Employee,int>
                var funcType = Expression.GetFuncType(typeof(T), property.PropertyType);
                
                // Create the body of our lambda expression which simply access the property we want to sort on (ie. x.Id)
                var bodyExpression = Expression.Property(paramExp, property.Name);

                // Creates the lambda expression of type Func<T,PropertyType>, body of x.PropertyName, and input parameter x 
                LambdaExpression lambdaExpression = Expression.Lambda(funcType, bodyExpression, paramExp);

                return lambdaExpression;
        }

        /// <summary>
        /// Gets the sort method. For multi column sorting (sortRank > 1) we need to use ThenBy methods because OrderBy will override
        /// any previous OrderBy calls
        /// </summary>
        private MethodInfo GetSortMethod(PropertyInfo property,int sortRank,String direction)
        {
                String reflectionMethodName = null;
                if (sortRank == 1)
                {
                    if (direction == "1")
                        reflectionMethodName = "System.Linq.IOrderedQueryable`1[TSource] OrderBy[TSource,TKey](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])";
                    else
                        reflectionMethodName = "System.Linq.IOrderedQueryable`1[TSource] OrderByDescending[TSource,TKey](System.Linq.IQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])";
                }
                else
                {
                    if (direction == "1")
                        reflectionMethodName = "System.Linq.IOrderedQueryable`1[TSource] ThenBy[TSource,TKey](System.Linq.IOrderedQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])";
                    else
                        reflectionMethodName = "System.Linq.IOrderedQueryable`1[TSource] ThenByDescending[TSource,TKey](System.Linq.IOrderedQueryable`1[TSource], System.Linq.Expressions.Expression`1[System.Func`2[TSource,TKey]])";
                }

                // Get the sort method from the Queryable class and replace the generic arguments with T and PropertyType
                var method = typeof(Queryable).GetMethods().Single(x => x.ToString() == reflectionMethodName).MakeGenericMethod(typeof(T), property.PropertyType);

                return method;
        }
    }
}