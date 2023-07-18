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
  var representation = parser.Parse();
  
  var generator = new Generator(report, representation);
  var cpp = generator.Generate();

  var canContinue = !ContainsErrors(report);

  if (canContinue)
  {
    Console.WriteLine(representation);
    Console.WriteLine();
    Console.WriteLine(cpp);
  }

  report.Render(AnsiConsole.Console);
}