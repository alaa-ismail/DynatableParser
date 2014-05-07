using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace DynatableParser
{
    internal class SearchExpressionFactory
    {
        public Expression SearchExpression { get; set; }

        public SearchExpressionFactory(ParameterExpression parameter, PropertyInfo property,String searchText)
        {
            if (property.PropertyType == typeof(String))
                SearchExpression = CreateStringSearchExpression(parameter, property, searchText);

            if (property.PropertyType == typeof(Boolean) || property.PropertyType == typeof(Boolean?))
                SearchExpression = CreateBooleanSearchExpression(parameter, property, searchText);

            if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                SearchExpression = CreateDateTimeSearchExpression(parameter, property, searchText);

            if (property.PropertyType == typeof(Int16) || property.PropertyType == typeof(Int16?)   ||
                property.PropertyType == typeof(Int32) || property.PropertyType == typeof(Int32?)   ||
                property.PropertyType == typeof(Int64) || property.PropertyType == typeof(Int64?)   ||
                property.PropertyType == typeof(Double) || property.PropertyType == typeof(Double?) ||
                property.PropertyType == typeof(Single) || property.PropertyType == typeof(Single?) ||
                property.PropertyType == typeof(Decimal) || property.PropertyType == typeof(Decimal?)
               )
                SearchExpression = CreateNumericSearchExpression(parameter, property, searchText);    
        }


        private Expression CreateStringSearchExpression(ParameterExpression parameter, PropertyInfo property, String searchText)
        {
            var accessProperty = Expression.Property(parameter, property.Name);
            var searchUsingContains = Expression.Call(accessProperty, "Contains", null, Expression.Constant(searchText));
            return searchUsingContains;
        }


        private Expression CreateNumericSearchExpression(ParameterExpression parameter, PropertyInfo property, String searchText)
        {
            MethodInfo stringConversionMethod = typeof(SqlFunctions).GetMethod("StringConvert", new Type[] { typeof(double?) });
            Expression accessProperty = Expression.Property(parameter, property.Name);
            Expression convertAccessedPropertyToDouble = Expression.Convert(accessProperty, typeof(double?));
            Expression convertAccessedPropertyToString = Expression.Call(stringConversionMethod, new Expression[] { convertAccessedPropertyToDouble });
            Expression searchUsingContains = Expression.Call(convertAccessedPropertyToString, "Contains", null, Expression.Constant(searchText));
            return searchUsingContains;
        }

        private Expression CreateDateTimeSearchExpression(ParameterExpression parameter, PropertyInfo property, String searchText)
        {
            List<Expression> searchExpressions = new List<Expression>();

            // If the searchText can be parsed into a date, then add an expression that will test for equality
            DateTime dateInput = DateTime.MinValue;
            var isValidDate = DateTime.TryParse(searchText, out dateInput);
            if (isValidDate)
            {
                var accessProperty = Expression.Property(parameter, property.Name);
                var checkDateEquality = Expression.Equal(accessProperty, Expression.Constant(dateInput));
                searchExpressions.Add(checkDateEquality);
            }

            // If the searchText can be parsed into an integer, then add expression to search in Year, Month, Day properties
            int integerInput = 0;
            var isValidInt = Int32.TryParse(searchText, out integerInput);
            if (isValidInt)
            {
                var accessors = new List<Expression>();

                var checkYearEquality = Expression.Equal(
                                            Expression.Property(Expression.Property(parameter, property.Name), "Year"),
                                            Expression.Constant(integerInput)
                                        );
                var checkMonthEquality = Expression.Equal(
                                            Expression.Property(Expression.Property(parameter, property.Name), "Month"),
                                            Expression.Constant(integerInput)
                                        );
                var checkDayEquality = Expression.Equal(
                                            Expression.Property(Expression.Property(parameter, property.Name), "Day"),
                                            Expression.Constant(integerInput)
                                        );

                searchExpressions.Add(checkYearEquality);
                searchExpressions.Add(checkMonthEquality);
                searchExpressions.Add(checkDayEquality);
            }

            Expression searchExpression = Expression.Constant(false);
            foreach (Expression expression in searchExpressions)
                searchExpression = Expression.Or(searchExpression, expression);

            return searchExpression;
        }

        private Expression CreateBooleanSearchExpression(ParameterExpression parameter, PropertyInfo property, String searchText)
        {
            Expression expression = null;

            if (searchText.Equals("true",StringComparison.InvariantCultureIgnoreCase))
            {
                 var accessProperty = Expression.Property(parameter, property.Name);
                 expression = Expression.Equal(accessProperty, Expression.Constant(true));
            }
            else if (searchText.Equals("false",StringComparison.InvariantCultureIgnoreCase))
            {
                 var accessProperty = Expression.Property(parameter, property.Name);
                 expression = Expression.Equal(accessProperty, Expression.Constant(false));
            }

            return expression;
        }

    }
}