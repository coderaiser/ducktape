using DuckTape;
using static DuckTape.Test;

var test = CreateTest();

test("test_loader: compiles and runs a spec file", t =>
{
    var dir = Directory.CreateTempSubdirectory("ducktape_loader_");
    var marker = Path.Combine(dir.FullName, "ran.txt");
    var spec = Path.Combine(dir.FullName, "hello.spec.cs");
    var markerJson = System.Text.Json.JsonSerializer.Serialize(marker);
    File.WriteAllText(spec, $"System.IO.File.WriteAllText({markerJson}, \"ran\");");
    TestLoader.Load(spec);
    var ran = File.ReadAllText(marker);
    Directory.Delete(dir.FullName, true);
    t.Equal(ran, "ran");
    t.End();
    return Task.CompletedTask;
});

test("test_loader: runs parameterless entry points", t =>
{
    var dir = Directory.CreateTempSubdirectory("ducktape_loader_");
    var marker = Path.Combine(dir.FullName, "ran.txt");
    var spec = Path.Combine(dir.FullName, "paramless.cs");
    var markerJson = System.Text.Json.JsonSerializer.Serialize(marker);
    File.WriteAllText(spec, $"class P {{ static void Main() {{ System.IO.File.WriteAllText({markerJson}, \"p\"); }} }}");
    TestLoader.Load(spec);
    var ran = File.ReadAllText(marker);
    Directory.Delete(dir.FullName, true);
    t.Equal(ran, "p");
    t.End();
    return Task.CompletedTask;
});

test("test_loader: compile errors are reported", t =>
{
    var dir = Directory.CreateTempSubdirectory("ducktape_loader_");
    var spec = Path.Combine(dir.FullName, "bad.spec.cs");
    File.WriteAllText(spec, "Test(\"loader: broken");
    var threw = false;
    try { TestLoader.Load(spec); }
    catch (InvalidOperationException ex) { threw = ex.Message.Contains("failed to compile"); }
    Directory.Delete(dir.FullName, true);
    t.Ok(threw);
    t.End();
    return Task.CompletedTask;
});

test("test_loader: missing entry point is reported", t =>
{
    var dir = Directory.CreateTempSubdirectory("ducktape_loader_");
    var spec = Path.Combine(dir.FullName, "lib.spec.cs");
    File.WriteAllText(spec, "namespace X { public class Y { } }");
    var threw = false;
    try { TestLoader.Load(spec); }
    catch (InvalidOperationException ex) { threw = ex.Message.Contains("failed to compile"); }
    Directory.Delete(dir.FullName, true);
    t.Ok(threw);
    t.End();
    return Task.CompletedTask;
});

test("test_loader: build references from tpa returns refs", t =>
{
    var refs = TestLoader.BuildReferences();
    t.Ok(refs.Count >= 1);
    t.End();
    return Task.CompletedTask;
});

test("test_loader: build references with non-string data falls back empty", t =>
{
    var refs = TestLoader.BuildReferences(42);
    t.Ok(refs.Count >= 1);
    t.End();
    return Task.CompletedTask;
});