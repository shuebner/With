using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SvSoft.With
{
    public class WithBenchmark
    {
        private Foo foo;

        [GlobalSetup]
        public void GlobalSetup()
        {
            foo = new Foo(42, "foo", new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero));
        }

        [Benchmark(Baseline = true)]
        public void ExplicitWithForValue2()
        {
            foo.ExplicitWithValue2("bar");
        }

        [Benchmark]
        public void ExplicitWithNullableParams()
        {
            foo.ExplicitWithNullableParams(null, "bar", null);
        }

        [Benchmark]
        public void Reflection()
        {
            foo.ReflectionWith(f => f.Value2, "bar");
        }

        [Benchmark]
        public void Generated()
        {
            foo.With(f => f.Value2, "bar");
        }

        private class Foo
        {
            private static readonly ConstructorInfo Ctor;
            private static readonly ParameterInfo[] CtorParamInfos;
            private static readonly ImmutableDictionary<string, PropertyInfo> PropertyByParamName;

            private static readonly Func<Foo, LambdaExpression, object, Foo> WithFunc;

            // eager initialize
            static Foo()
            {
                WithFunc = SvSoft.With.With.For<Foo>();

                // for reflection
                var thisType = typeof(Foo);
                Ctor = thisType.GetConstructors().Single();
                CtorParamInfos = Ctor.GetParameters();
                PropertyByParamName = thisType
                    .GetProperties()
                    .ToImmutableDictionary(property => ToParameterName(property.Name), property => property);
            }

            public Foo(int value1, string value2, DateTimeOffset value3)
            {
                Value1 = value1;
                Value2 = value2;
                Value3 = value3;
            }

            public int Value1 { get; }
            public string Value2 { get; }
            public DateTimeOffset Value3 { get; }

            public Foo With<T>(Expression<Func<Foo, T>> propertyExpr, T value) =>
                WithFunc(this, propertyExpr, value);

            public Foo ExplicitWithValue2(string newValue2) =>
                new Foo(Value1, newValue2, Value3);

            public Foo ExplicitWithNullableParams(
                int? newValue1 = null,
                string newValue2 = null,
                DateTimeOffset? newValue3 = null) =>
                new Foo(
                    newValue1 ?? Value1,
                    newValue2 ?? Value2,
                    newValue3 ?? Value3);

            public Foo ReflectionWith<T>(Expression<Func<Foo, T>> propertyExpr, T value)
            {
                string propertyName = ((MemberExpression)propertyExpr.Body).Member.Name;
                var ctorParameterName = ToParameterName(propertyName);
                var ctorParamValues = CtorParamInfos.Select(param => param.Name == ctorParameterName
                    ? value
                    : PropertyByParamName[param.Name].GetValue(this)).ToArray();

                return (Foo)Ctor.Invoke(ctorParamValues);
            }

            private static string ToParameterName(string memberName) =>
                string.Concat(char.ToLowerInvariant(memberName[0]), memberName.Substring(1));
        }
    }
}
