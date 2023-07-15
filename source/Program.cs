using Spp;
using Errata;
using Spectre.Console;

var report = new Report(new Errata.InMemorySourceRepository());
var parser = new Parser(report, "sample.spp");
var code = parser.Parse();

report.Render(AnsiConsole.Console);
var canContinue = report.Diagnostics.Count(
  (diag) => diag.Category == "Error"
) == 0;

// if (canContinue)
//   Console.WriteLine(code);