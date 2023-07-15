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
        .WithNote("Number of spaces in indentation must be even\n")
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
    
    public static Diagnostic ParameterRedefinition(string name, Position position) =>
      Diagnostic
        .Error("Redefinition of function parameter")
        .WithCode("SPP6")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Parameter \"{name}\" is already declared, redefinition is not allowed"
        ));
    
    public static Diagnostic BadTokenMode(TokenMode expectedMode, Position position) =>
      Diagnostic
        .Error("Bad token mode")
        .WithCode("SPP7")
        .WithNote(
          expectedMode == TokenMode.OnTheSameLine
            ? "Move token to the previous line\n"
            : "Move token to a new line\n"
        )
        .WithLabel(new(
          position.Filename,
          position.Location,
          expectedMode == TokenMode.OnTheSameLine
            ? $"Expected token to be on the previous line, but it is on a new one"
            : $"Expected token to be on a new line"
        ));
    
    public static Diagnostic BadIndent(int expectedIndent, int actualIndent, Position position) =>
      Diagnostic
        .Error("Bad indentation")
        .WithCode("SPP8")
        .WithNote("Indentation levels must be 2 spaces each\n")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Expected indentation of {expectedIndent} spaces, got {actualIndent} spaces"
        ));
    
    public static Diagnostic BadStatementSyntax(TokenKind kind, Position position) =>
      Diagnostic
        .Error("Bad statement syntax")
        .WithCode("SPP9")
        .WithNote(
@"A statement must be either a:
      * Control flow statement (`return`, `break` ..)
      * Assignment statement
      * Selection statement (`if`, `while` ..)
      .
      .
      * Variable/Function/Implementation definition
")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Expected statement, got token \"{kind}\""
        ));
    
    public static Diagnostic BadExpressionSyntax(TokenKind kind, Position position) =>
      Diagnostic
        .Error("Bad expression syntax")
        .WithCode("SPP10")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Expected expression/term, got token \"{kind}\""
        ));
    
    public static Diagnostic MalformedNumber(string value, Position position) =>
      Diagnostic
        .Error("Malformed number token")
        .WithCode("SPP11")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Token \"Number\" with value \"{value}\" is malformed"
        ));
    
    public static Diagnostic ParameterRedefinitionInfo(Position position) =>
      Diagnostic
        .Info("Parameter previously defined here")
        .WithLabel(new(
          position.Filename,
          position.Location,
          ""
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