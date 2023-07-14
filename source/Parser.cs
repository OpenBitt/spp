using Errata;

namespace Spp
{
  public class Parser
  {
    readonly Lexer lexer;
    
    MidCode code;

    Report report;

    public Parser(Report report, string filename)
    {
      this.report = report;

      lexer = new(report, filename);
      code = new();
    }

    void AddTopLevel(string name, IDefinition definition)
    {
      if (code.TopLevels.TryAdd(name, definition))
        return;
      
      report.AddDiagnostic(ReportHelper.TopLevelRedefinition(
        name, definition.Position
      ));

      report.AddDiagnostic(ReportHelper.TopLevelRedefinitionInfo(
        code.TopLevels[name].Position
      ));
    }

    bool MatchAdvance(TokenKind kind, bool mustBeOnTheSameLine)
    {
      var indentIsCorrect =
        mustBeOnTheSameLine
          ? lexer.Current.Indent == Lexer.NOT_INDENTED
          : true;

      if (lexer.Current.Kind == kind && indentIsCorrect)
      {
        EatToken();
        return true;
      }

      return false;
    }

    Token ExpectToken(TokenKind kind, bool mustBeOnTheSameLine = true)
    {
      if (MatchAdvance(kind, mustBeOnTheSameLine))
        return lexer.Previous;
      
      report.AddDiagnostic(ReportHelper.ExpectedToken(
        kind, lexer.Current.Kind, lexer.Current.Position
      ));
      return lexer.Current;
    }

    void ParseFn(out string name, out IDefinition definition)
    {
      // skipping "Fn"
      EatToken();
      var nameToken = ExpectToken(TokenKind.Identifier);
      name = nameToken.Value;

      var fnDefinition = new FnDefinition(nameToken.Position, name);
      definition = fnDefinition;
      
      // TODO: add parameters
    }

    Token EatToken()
    {
      var current = lexer.Current;
      lexer.NextToken();
      
      return current;
    }

    void ParseTopLevel()
    {
      string topLevelName;
      IDefinition definition;

      switch (lexer.Current.Kind)
      {
        case TokenKind.Fn:
          ParseFn(out topLevelName, out definition);
          break;

        default:
          EatToken();
          report.AddDiagnostic(ReportHelper.BadTopLevelSyntax(
            lexer.Current.Kind, lexer.Current.Position
          ));
          return;
      }

      AddTopLevel(topLevelName, definition);
    }

    public MidCode Parse()
    {
      if (!lexer.HasNextToken)
        return code;

      // fetching the first token
      // for the first top level definition
      lexer.NextToken();

      while (lexer.HasNextToken)
        ParseTopLevel();

      return code;
    }
  }
}