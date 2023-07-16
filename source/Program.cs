using Spp;
using Errata;
using Spectre.Console;

Entry();

static bool ContainsErrors(Report report) =>
  report.Diagnostics.Count(
    (diag) => diag.Category == "Error"
  ) > 0;

static void Entry()
{
  var report = new Report(new InMemorySourceRepository());
  var parser = new Parser(report, "sample.spp");
  var code = parser.Parse();

  var canContinue = !ContainsErrors(report);

  if (canContinue)
  {
    Console.WriteLine(code);
    Console.WriteLine();
  }

  report.Render(AnsiConsole.Console);
}