using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

string Root()
{
    var dir = Directory.CreateTempSubdirectory("ducktape_glob_");
    Directory.CreateDirectory(Path.Combine(dir.FullName, "dir1"));
    Directory.CreateDirectory(Path.Combine(dir.FullName, "dir2", "sub"));
    File.WriteAllText(Path.Combine(dir.FullName, "a.spec.cs"), "");
    File.WriteAllText(Path.Combine(dir.FullName, "dir1", "b.spec.cs"), "");
    File.WriteAllText(Path.Combine(dir.FullName, "dir1", "c.txt"), "");
    File.WriteAllText(Path.Combine(dir.FullName, "dir2", "sub", "d.spec.cs"), "");
    return dir.FullName;
}

test("glob: single star matches top level only", t =>
{
    var r = Root();
    var files = Glob.Expand(Path.Combine(r, "*.spec.cs"));
    Directory.Delete(r, true);
    t.DeepEqual(files, new List<string> { Path.Combine(r, "a.spec.cs") });
    t.End();
});

test("glob: double star reaches nested files", t =>
{
    var r = Root();
    var files = Glob.Expand(Path.Combine(r, "**", "*.spec.cs"));
    Directory.Delete(r, true);
    t.DeepEqual(files,
        new List<string>
        {
            Path.Combine(r, "a.spec.cs"),
            Path.Combine(r, "dir1", "b.spec.cs"),
            Path.Combine(r, "dir2", "sub", "d.spec.cs"),
        });
    t.End();
});

test("glob: double star can pin a file name", t =>
{
    var r = Root();
    var files = Glob.Expand(Path.Combine(r, "**", "b.spec.cs"));
    Directory.Delete(r, true);
    t.DeepEqual(files, new List<string> { Path.Combine(r, "dir1", "b.spec.cs") });
    t.End();
});

test("glob: question mark matches a single char", t =>
{
    var r = Root();
    var files = Glob.Expand(Path.Combine(r, "?.spec.cs"));
    Directory.Delete(r, true);
    t.DeepEqual(files, new List<string> { Path.Combine(r, "a.spec.cs") });
    t.End();
});

test("glob: directory scoped pattern ignores siblings", t =>
{
    var r = Root();
    var files = Glob.Expand(Path.Combine(r, "dir1", "*.spec.cs"));
    Directory.Delete(r, true);
    t.DeepEqual(files, new List<string> { Path.Combine(r, "dir1", "b.spec.cs") });
    t.End();
});

test("glob: nested files are not top level matches", t =>
{
    var r = Root();
    var files = Glob.Expand(Path.Combine(r, "dir2", "*.spec.cs"));
    Directory.Delete(r, true);
    t.DeepEqual(files, new List<string>());
    t.End();
});

test("glob: missing root yields nothing", t =>
{
    var r = Root();
    var files = Glob.Expand(Path.Combine(r, "missing", "**", "*.spec.cs"));
    Directory.Delete(r, true);
    t.DeepEqual(files, new List<string>());
    t.End();
});

test("glob: double star alone matches everything under root", t =>
{
    var r = Root();
    var files = Glob.Expand(Path.Combine(r, "**"));
    Directory.Delete(r, true);
    t.Equal(files.Count, 4);
    t.End();
});

test("glob: other extensions are matched by pattern", t =>
{
    var r = Root();
    var files = Glob.Expand(Path.Combine(r, "*", "*.txt"));
    Directory.Delete(r, true);
    t.DeepEqual(files, new List<string> { Path.Combine(r, "dir1", "c.txt") });
    t.End();
});