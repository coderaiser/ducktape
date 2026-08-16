using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

test("format: AddSpaces prepends six spaces", t =>
{
    t.Equal(Format.AddSpaces("hi"), "      hi");
    t.End();
});

test("format: FormatOutput indents every line", t =>
{
    t.Equal(Format.FormatOutput("a\nb"), "      a\n      b");
    t.End();
});

test("format: FormatOutput keeps empty input", t =>
{
    t.Equal(Format.FormatOutput(""), "");
    t.End();
});