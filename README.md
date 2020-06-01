# With
Creates a dynamically compiled type-safe With function for immutable types that does not use slow reflection at call time

# Usage
```
public class Foo
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
...
var updatedFoo = foo.With(f => f.Value2, "bar");
```

# Performance
Terrible, but at least twice as fast as reflection.
For some reason, EtwProfiler is missing the symbols, so I could not yet see where all the time is spent.
```
|                     Method |         Mean |      Error |     StdDev |  Ratio | RatioSD |
|--------------------------- |-------------:|-----------:|-----------:|-------:|--------:|
|               ExplicitWith |     6.769 ns |  0.0259 ns |  0.0242 ns |   1.00 |    0.00 |
| ExplicitWithNullableParams |    14.385 ns |  0.0551 ns |  0.0515 ns |   2.13 |    0.01 |
|                 Reflection | 2,260.028 ns |  3.8653 ns |  3.6156 ns | 333.90 |    1.43 |
|                  Generated | 1,197.876 ns | 11.0513 ns | 10.3374 ns | 176.98 |    1.76 |
```
