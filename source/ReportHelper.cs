using Errata;
using Spectre.Console;

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
          position.Range,
          $"character '\\t' is not allowed (Replace it with 2 spaces instead)"
        ));

    public static Diagnostic OddIndent(int indent, Position position) =>
      Diagnostic
        .Error("Bad formatting")
        .WithCode("SPP1")
        .WithNote("Number of spaces in indentation must be even\n")
        .WithLabel(new(
          position.Filename,
          position.Range,
          $"indentation here is {indent} spaces, use {indent - 1} or {indent + 1} spaces instead"
        ));

    public static Diagnostic FoundCarriageReturn(Position position) =>
      Diagnostic
        .Error("Bad formatting")
        .WithCode("SPP2")
        .WithLabel(new(
          position.Filename,
          new Range(position.Range.Start.Value - 1, position.Range.End.Value - 1),
          $"character '\\r' is not allowed (reformat the file using LR instead of CRLF)"
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
          position.Range,
          $"expected a valid top level member definition, found token \"{kind}\""
        ));

    public static Diagnostic MemberRedefinition(string name, Position position) =>
      Diagnostic
        .Error("Member redefinition")
        .WithCode("SPP4")
        .WithLabel(new(
          position.Filename,
          position.Range,
          $"member \"{name}\" is already defined, redefinition is not allowed"
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
          position.Range,
          $"expected token \"{expectedKind}\", got \"{actualKind}\""
        ));
    
    public static Diagnostic ParameterRedefinition(string name, Position position) =>
      Diagnostic
        .Error("Redefinition of function parameter")
        .WithCode("SPP6")
        .WithLabel(new(
          position.Filename,
          position.Range,
          $"parameter \"{name}\" is already defined, parameter redefinitions is not allowed"
        ));
    
    public static Diagnostic BadTokenMode(TokenMode expectedMode, Position position) =>
      Diagnostic
        .Error("Bad token mode")
        .WithCode("SPP7")
        .WithLabel(new(
          position.Filename,
          position.Range,
          expectedMode == TokenMode.OnTheSameLine
            ? $"expected token to be on the previous line, but it is on a new one (move token to the previous line)"
            : $"expected token to be on a new line (move token to a new line)"
        ));
    
    public static Diagnostic BadIndent(int expectedIndent, int actualIndent, Position position) =>
      Diagnostic
        .Error("Bad indentation")
        .WithCode("SPP8")
        .WithNote("Indentation levels must be 2 spaces each\n")
        .WithLabel(new(
          position.Filename,
          position.Range,
          $"expected indentation of {expectedIndent} spaces, got {actualIndent} spaces"
        ));
    
    public static Diagnostic BadExpressionSyntax(TokenKind kind, Position position) =>
      Diagnostic
        .Error("Bad expression syntax")
        .WithCode("SPP10")
        .WithLabel(new(
          position.Filename,
          position.Range,
          $"expected expression/term, got token \"{kind}\""
        ));
    
    public static Diagnostic MalformedNumber(string value, Position position) =>
      Diagnostic
        .Error("Malformed number token")
        .WithCode("SPP11")
        .WithLabel(new(
          position.Filename,
          position.Range,
          $"token \"Number\" with value \"{value}\" is malformed"
        ));
    
    public static Diagnostic NotAType(IType type, Position position) =>
      Diagnostic
        .Error("Not a type")
        .WithCode("SPP12")
        .WithLabel(new(
          position.Filename,
          position.Range,
          $"expected static value of type \"type\", got type \"{type}\""
        ));

    public static Diagnostic UndefinedMember(string name, Position position) =>
      Diagnostic
        .Error("Member not defined")
        .WithCode("SPP13")
        .WithLabel(new(
          position.Filename,
          position.Range,
          // TODO: add "did you mean \"{suggestedName}\""
          $"member \"{name}\" is not defined in the current context"
        ));

    public static Diagnostic NotAStaticValue(Position position) =>
      Diagnostic
        .Error("Not a static value")
        .WithCode("SPP14")
        .WithLabel(new(
          position.Filename,
          position.Range,
          $"the context expects a static value"
        ));

    public static Diagnostic TypesMismatch(
      IType expectedType,
      IType actualType,
      Position expectedPosition,
      Position actualPosition
    ) =>
      Diagnostic
        .Error("Types mismatch")
        .WithCode("SPP15")
        .WithLabel(
          new Label(
            expectedPosition.Filename,
            expectedPosition.Range,
            $"the context expected type \"{expectedType}\""
          )
          .WithColor(Color.Green)
        )
        .WithLabel(
          new Label(
            actualPosition.Filename,
            actualPosition.Range,
            $"got \"{actualType}\""
          )
          .WithColor(Color.Red)
        );

    public static Diagnostic UndefinedEntryPoint() =>
      Diagnostic
        .Error("Entry point not defined")
        .WithCode("SPP16");

    public static Diagnostic BadEntryPointType(IType expectedType, IType actualType, Position position) =>
      Diagnostic
        .Error("Entry point not correctly defined")
        .WithCode("SPP17")
        .WithNote("")
        .WithLabel(
          new Label(
            position.Filename,
            position.Range,
            $"must be defined as \"{expectedType}\""
          )
          .WithColor(Color.Green)
        )
        .WithLabel(
          new Label(
            position.Filename,
            position.Range,
          $"has type \"{actualType}\""
          )
          .WithColor(Color.Red)
        );
    
    public static Diagnostic ParameterRedefinitionInfo(Position position) =>
      Diagnostic
        .Info("Parameter previously defined here")
        .WithLabel(new(
          position.Filename,
          position.Range,
          ""
        ));

    public static Diagnostic MemberRedefinitionInfo(Position position) =>
      Diagnostic
        .Info("Member previously defined here")
        .WithLabel(new(
          position.Filename,
          position.Range,
          ""
        ));
  }
}