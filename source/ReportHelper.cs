using Errata;

// TODO: try removing all possible `.Note`s
//       they are ugly to see

namespace Spp
{
  public static class ReportHelper
  {
    public static Diagnostic FoundTab(Position position) =>
      Diagnostic
        .Error("Bad formatting")
        .WithCode("SPP0")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Character '\\t' is not allowed (Replace it with 2 spaces instead)"
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
        .WithLabel(new(
          position.Filename,
          new Location(position.Location.Row, position.Location.Column - 1),
          $"Character '\\r' is not allowed (Reformat the file using LR instead of CRLF)"
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

    public static Diagnostic MemberRedefinition(string name, Position position) =>
      Diagnostic
        .Error("Member redefinition")
        .WithCode("SPP4")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Member \"{name}\" is already defined, redefinition is not allowed"
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
          $"Parameter \"{name}\" is already defined, redefinition is not allowed"
        ));
    
    public static Diagnostic BadTokenMode(TokenMode expectedMode, Position position) =>
      Diagnostic
        .Error("Bad token mode")
        .WithCode("SPP7")
        .WithLabel(new(
          position.Filename,
          position.Location,
          expectedMode == TokenMode.OnTheSameLine
            ? $"Expected token to be on the previous line, but it is on a new one (Move token to the previous line)"
            : $"Expected token to be on a new line (Move token to a new line)"
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
    
    public static Diagnostic NotAType(IType type, Position position) =>
      Diagnostic
        .Error("Not a type")
        .WithCode("SPP12")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Expected static value of type \"type\", got type \"{type}\""
        ));

    public static Diagnostic UndefinedMember(string name, Position position) =>
      Diagnostic
        .Error("Member not defined")
        .WithCode("SPP13")
        .WithLabel(new(
          position.Filename,
          position.Location,
          // TODO: add "did you mean \"{suggestedName}\""
          $"Member \"{name}\" is not defined"
        ));

    public static Diagnostic NotAStaticValue(Position position) =>
      Diagnostic
        .Error("Not a static value")
        .WithCode("SPP14")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"In this context a static value is expected, but the given one is not"
        ));

    public static Diagnostic TypesMismatch(IType expectedType, IType actualType, Position position) =>
      Diagnostic
        .Error("Types mismatch")
        .WithCode("SPP15")
        .WithLabel(new(
          position.Filename,
          position.Location,
          $"Expected type \"{expectedType}\", got \"{actualType}\""
        ));
    
    public static Diagnostic ParameterRedefinitionInfo(Position position) =>
      Diagnostic
        .Info("Parameter previously defined here")
        .WithLabel(new(
          position.Filename,
          position.Location,
          ""
        ));

    public static Diagnostic MemberRedefinitionInfo(Position position) =>
      Diagnostic
        .Info("Member previously defined here")
        .WithLabel(new(
          position.Filename,
          position.Location,
          ""
        ));
  }
}