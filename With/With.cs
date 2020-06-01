using System;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SvSoft.With
{
    public static class With
    {
        public static Func<T, LambdaExpression, object, T> For<T>()
        {
            var type = typeof(T);
            ConstructorInfo ctor = type.GetConstructors().Single();
            ParameterInfo[] ctorParamInfos = ctor.GetParameters();
            ImmutableDictionary<string, Func<T, object, T>> createNewObjectByPropertyName = type
                .GetProperties()
                .ToImmutableDictionary(
                    property => property.Name,
                    property => GetConstructorWithParameterValue<T>(ctor, ctorParamInfos, property.Name));

            ParameterExpression obj = Expression.Parameter(typeof(T), "obj");
            ParameterExpression memberExpr = Expression.Parameter(typeof(LambdaExpression), "memberExpr");
            ParameterExpression newValue = Expression.Parameter(typeof(object), "newValue");

            var getMemberName =
                Expression.Property(
                    Expression.Property(
                        Expression.Convert(
                            Expression.Property(
                                memberExpr,
                                nameof(LambdaExpression.Body)),
                            typeof(MemberExpression)),
                        nameof(MemberExpression.Member)),
                    nameof(MemberInfo.Name));

            var getCtorFuncExpression =
                Expression.Property(
                    Expression.Constant(createNewObjectByPropertyName),
                    "Item",
                    getMemberName);

            var getNewObjWithNewValueExpression =
                Expression.Invoke(
                    getCtorFuncExpression,
                    obj,
                    newValue);

            var with = Expression.Lambda<Func<T, LambdaExpression, object, T>>(
                getNewObjWithNewValueExpression,
                obj,
                memberExpr,
                newValue);

            return with.Compile();
        }

        private static Func<T, object, T> GetConstructorWithParameterValue<T>(ConstructorInfo ctor, ParameterInfo[] ctorParamInfos, string propertyName)
        {
            var originalObjExpr = Expression.Parameter(typeof(T), "original");
            var property = typeof(T).GetProperty(propertyName);
            var newValueExpr = Expression.Parameter(typeof(object), "value");

            var parameterName = ToParameterName(propertyName);
            var ctorExpression = Expression.New(ctor,
                ctorParamInfos.Select(param => param.Name != parameterName
                    ? (Expression)Expression.Property(originalObjExpr, typeof(T).GetProperty(ToMemberName(param.Name)))
                    : Expression.Convert(newValueExpr, property.PropertyType)));

            return Expression.Lambda<Func<T, object, T>>(ctorExpression, originalObjExpr, newValueExpr).Compile();
        }

        private static string ToParameterName(string memberName) =>
            string.Concat(char.ToLowerInvariant(memberName[0]), memberName.Substring(1));

        private static string ToMemberName(string parameterName) =>
            string.Concat(char.ToUpperInvariant(parameterName[0]), parameterName.Substring(1));
    }
}
