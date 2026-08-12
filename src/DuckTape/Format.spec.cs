using DuckTape;
using static DuckTape.Tests;

Test("format: AddSpaces prepends six spaces", t =>
{
    t.Equal(Format.AddSpaces("hi"), "      hi");
    t.End();
});

Test("format: FormatOutput indents every line", t =>
{
    t.Equal(Format.FormatOutput("a\nb"), "      a\n      b");
    t.End();
});

Test("format: FormatOutput keeps empty input", t =>
{
    t.Equal(Format.FormatOutput(""), "");
    t.End();
});