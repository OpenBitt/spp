using Errata;

namespace Spp
{
  public static class ReportHelper
  {
    public static Diagnostic FoundTab(Position position) =>
      Diagnostic
        .Error("Bad formatting")
        .WithCode("SPP0")
        .WithNote("Replace it with 2 spaces instead\n")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Character '\\t' is not allowed"
        ));

    public static Diagnostic OddIndent(int indent, Position position) =>
      Diagnostic
        .Error("Bad formatting")
        .WithCode("SPP1")
        .WithNote("Indentation must be multiple of 2 spaces\n")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Indentation here is {indent} spaces, use {indent - 1} or {indent + 1} spaces instead"
        ));

    public static Diagnostic FoundCarriageReturn(Position position) =>
      Diagnostic
        .Error("Bad formatting")
        .WithCode("SPP2")
        .WithNote("Reformat the file using LR instead of CRLF\n")
        .WithLabel(new(
          position.Filename,
          new Location(position.Location.Row, position.Location.Column - 1),
          $"Character '\\r' is not allowed"
        ));

    public static Diagnostic BadTopLevelSyntax(TokenKind kind, Position position) =>
      Diagnostic
        .Error("Bad syntax for top level member definition")
        .WithCode("SPP3")
        .WithNote(
@"A top level member must be either a:
      * Function definition
      * Variable definition
      * Implementation definition
")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Expected a valid top level member definition, found token \"{kind}\""
        ));

    public static Diagnostic TopLevelRedefinition(string name, Position position) =>
      Diagnostic
        .Error("Redefinition of top level member")
        .WithCode("SPP4")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Top level member \"{name}\" is already declared, redefinition is not allowed"
        ));

    public static Diagnostic ExpectedToken(
      TokenKind expectedKind,
      TokenKind actualKind,
      Position position
    ) =>
      Diagnostic
        .Error("Syntax error")
        .WithCode("SPP5")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Expected token \"{expectedKind}\", got \"{actualKind}\""
        ));

    public static Diagnostic TopLevelRedefinitionInfo(Position position) =>
      Diagnostic
        .Info("Member previously defined here")
        .WithLabel(new(
          position.Filename,
          position.Location,
          ""
        ));
  }
}