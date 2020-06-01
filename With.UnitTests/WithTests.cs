using FluentAssertions;
using System;
using System.Linq.Expressions;
using Xunit;

namespace SvSoft.With
{
    public class WithTests
    {
        private static readonly DateTimeOffset SomeDateTime = new DateTimeOffset(2000, 1, 2, 3, 4, 5, TimeSpan.Zero);

        [Fact]
        public void With_returns_new_object_with_set_value()
        {
            var foo = new Foo(3, "second", SomeDateTime);

            var newFoo = foo.With(f => f.Value2, "two");

            foo.Value2.Should().Be("second");
            newFoo.Should().BeEquivalentTo(new
            {
                Value1 = 3,
                Value2 = "two",
                Value3 = SomeDateTime
            });
        }

        private class Foo
        {
            private static readonly Func<Foo, LambdaExpression, object, Foo> WithFunc = SvSoft.With.With.For<Foo>();

            public int Value1 { get; }
            public string Value2 { get; }
            public DateTimeOffset Value3 { get; }

            public Foo(int value1, string value2, DateTimeOffset value3)
            {
                Value1 = value1;
                Value2 = value2;
                Value3 = value3;
            }

            public Foo With<T>(Expression<Func<Foo, T>> fieldExpr, T value) =>
                WithFunc(this, fieldExpr, value);
        }
    }
}
