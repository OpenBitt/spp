using Spp;
using Errata;
using Spectre.Console;

var report = new Report(new Errata.InMemorySourceRepository());
var parser = new Parser(report, "sample.spp");
var midcode = parser.Parse();

report.Render(AnsiConsole.Console);