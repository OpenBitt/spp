using Errata;

namespace Spp
{
  public class Parser
  {
    readonly Report report;

    readonly Lexer lexer;
    
    readonly MiddleRepresentation representation;

    int indent;

    public Parser(Report report, string filename)
    {
      this.report = report;

      lexer = new(report, filename);
      representation = new();
      indent = 0;
    }

    void AddTopLevel(string name, IDefinition definition)
    {
      if (representation.TopLevels.TryAdd(name, definition))
        return;
      
      report.AddDiagnostic(ReportHelper.TopLevelRedefinition(
        name, definition.Position
      ));

      report.AddDiagnostic(ReportHelper.TopLevelRedefinitionInfo(
        representation.TopLevels[name].Position
      ));
    }

    bool MatchAdvance(TokenKind kind, TokenMode mode)
    {
      if (MatchNoAdvance(kind, mode))
      {
        // advancing only when matching
        EatToken();
        return true;
      }

      return false;
    }

    Token ExpectToken(TokenKind kind, TokenMode mode)
    {
      if (MatchAdvance(kind, mode))
        return lexer.Previous;
      
      if (lexer.Current.Kind == kind)
        report.AddDiagnostic(ReportHelper.BadTokenMode(
          mode, lexer.Current.Position
        ));
      else
        report.AddDiagnostic(ReportHelper.ExpectedToken(
          kind, lexer.Current.Kind, lexer.Current.Position
        ));
      
      return lexer.Current;
    }

    void ExpectTokenMode(TokenMode mode)
    {
      if (MatchTokenMode(mode))
        return;
      
      report.AddDiagnostic(ReportHelper.BadTokenMode(
        mode, lexer.Current.Position
      ));
    }

    bool MatchTokenMode(TokenMode mode)
    {
      return
        mode == TokenMode.NoMatter ||
        lexer.Current.Mode == mode;
    }

    bool MatchNoAdvance(TokenKind kind, TokenMode mode)
    {
      return
        lexer.Current.Kind == kind &&
        MatchTokenMode(mode);
    }

    bool MatchAdvanceMultiple(TokenKind[] kinds, TokenMode mode)
    {
      foreach (var kind in kinds)
        if (MatchAdvance(kind, mode))
          return true;
      
      return false;
    }

    void ParseBinaryExpression(
      CodeChunk expression,
      Action elementParser, params TokenKind[] operators
    )
    {
      elementParser();

      while (MatchAdvanceMultiple(operators, TokenMode.OnTheSameLine))
      {
        var operatorToken = lexer.Previous;

        elementParser();
        expression.BinaryOp(operatorToken.Kind, operatorToken.Position);
      }
    }

    void ParseTerm(CodeChunk expression)
    {
      var current = EatToken();
      switch (current.Kind)
      {
        case TokenKind.Identifier:
          expression.LoadName(current.Value, current.Position);
          break;

        case TokenKind.Number:
          expression.LoadImmediate(lexer.ParseNumberToken(current), current.Position);
          break;

        case TokenKind.String:
          expression.LoadImmediate(current.Value, current.Position);
          break;

        default:
          report.AddDiagnostic(ReportHelper.BadExpressionSyntax(
            current.Kind, current.Position
          ));
          return;
      }

      // matching operators (such as call, index, dot, ..)
      while (MatchAdvanceMultiple(
        new[] { TokenKind.LPar, TokenKind.Dot },
        TokenMode.OnTheSameLine
      ))
        switch (lexer.Previous.Kind)
        {
          case TokenKind.LPar:
            ParseCall(expression, lexer.Previous.Position);
            break;
          
          case TokenKind.Dot:
            ParseDot(expression);
            break;
          
          default:
            throw new NotImplementedException();
        };
    }

    void ParseDot(CodeChunk expression)
    {
      var attribute = ExpectToken(TokenKind.Identifier, TokenMode.NoMatter);
      expression.LoadAttribute(attribute.Value, attribute.Position);
    }

    void ParseCall(CodeChunk expression, Position position)
    {
      var argumentsCount = 0;

      do
      {
        if (MatchNoAdvance(TokenKind.RPar, TokenMode.NoMatter))
          break;

        ParseExpression(expression, TokenMode.NoMatter);
        argumentsCount++;
      }
      while (MatchAdvance(TokenKind.Comma, TokenMode.NoMatter));

      ExpectToken(TokenKind.RPar, TokenMode.NoMatter);
      expression.Call(argumentsCount, position);
    }

    void ParseRealExpression(CodeChunk expression)
    {
      ParseBinaryExpression(
        expression,
        () => ParseBinaryExpression(
          expression,
          () => ParseTerm(expression),
          TokenKind.Star, TokenKind.Slash, TokenKind.Reminder
        ),
        TokenKind.Plus, TokenKind.Minus
      );
    }

    void ParseExpression(CodeChunk expression, TokenMode mode)
    {
      ExpectTokenMode(mode);
      ParseRealExpression(expression);
    }

    void ParseTypeNotation(CodeChunk type)
    {
      ExpectToken(TokenKind.Colon, TokenMode.OnTheSameLine);
      ParseExpression(type, TokenMode.OnTheSameLine);
    }

    VarDefinition ParseFnParameterDefinition()
    {
      var nameToken = ExpectToken(TokenKind.Identifier, TokenMode.NoMatter);
      var definition = new VarDefinition(nameToken.Position, nameToken.Value);

      ParseTypeNotation(definition.TypeExpression);

      return definition;
    }

    void ParseFnReturnTypeNotation(CodeChunk type, Position fnPosition)
    {
      if (MatchAdvance(TokenKind.Arrow, TokenMode.NoMatter))
      {
        ParseExpression(type, TokenMode.OnTheSameLine);
        return;
      }

      // return type can be implicit
      // (defaulted to `void`)
      type.LoadName("void", fnPosition);
    }

    void AddParameter(ref FnDefinition fnDefinition, VarDefinition parameterDefinition)
    {
      var name = parameterDefinition.Name;

      if (fnDefinition.Parameters.TryAdd(name, parameterDefinition))
        return;
      
      report.AddDiagnostic(ReportHelper.ParameterRedefinition(
        name, parameterDefinition.Position
      ));

      report.AddDiagnostic(ReportHelper.ParameterRedefinitionInfo(
        fnDefinition.Parameters[name].Position
      ));
    }

    void ParseFnParametersList(ref FnDefinition definition)
    {
      ExpectToken(TokenKind.LPar, TokenMode.OnTheSameLine);

      do
      {
        if (MatchNoAdvance(TokenKind.RPar, TokenMode.NoMatter))
          break;
        
        AddParameter(ref definition, ParseFnParameterDefinition());
      }
      while (MatchAdvance(TokenKind.Comma, TokenMode.NoMatter));

      ExpectToken(TokenKind.RPar, TokenMode.NoMatter);
    }

    void ExpectTokenIndent()
    {
      if (lexer.Current.Indent == indent)
        return;
      
      report.AddDiagnostic(ReportHelper.BadIndent(
        indent, lexer.Current.Indent, lexer.Current.Position
      ));
    }

    void ParseStatement(CodeChunk body)
    {
      switch (lexer.Current.Kind)
      {
        case TokenKind.Pass:
          ParsePassStatement(body);
          break;

        case TokenKind.Return:
          ParseReturnStatement(body);
          break;
        
        default:
          var position = lexer.Current.Position;
          ParseExpression(body, TokenMode.OnNewLine);
          body.Pop(position);
          break;
      }
    }

    bool HasExpression()
    {
      return
        lexer.Current.Kind != TokenKind.Eof &&
        MatchTokenMode(TokenMode.OnTheSameLine);
    }

    void ParseReturnStatement(CodeChunk body)
    {
      var position = EatToken().Position;

      if (!HasExpression())
      {
        body.RetVoid(position);
        return;
      }
      
      ParseExpression(body, TokenMode.OnTheSameLine);
      body.Ret(position);
    }

    void ParsePassStatement(CodeChunk body)
    {
      body.Nop(EatToken().Position);
    }

    void ParseBlock(CodeChunk body)
    {
      ExpectToken(TokenKind.Colon, TokenMode.OnTheSameLine);
      ExpectTokenMode(TokenMode.OnNewLine);
      indent += 2;

      while (lexer.Current.Indent >= indent)
      {
        ExpectTokenIndent();
        ParseStatement(body);
      }
      
      indent -= 2;
    }

    void ParseFn(out string name, out IDefinition definition)
    {
      var nameToken = ExpectToken(TokenKind.Identifier, TokenMode.OnTheSameLine);
      name = nameToken.Value;

      var fnDefinition = new FnDefinition(nameToken.Position, name);
      definition = fnDefinition;
      
      ParseFnParametersList(ref fnDefinition);
      ParseFnReturnTypeNotation(fnDefinition.ReturnTypeExpression, fnDefinition.Position);
      ParseBlock(fnDefinition.Body);
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

      var current = EatToken();
      switch (current.Kind)
      {
        case TokenKind.Fn:
          ParseFn(out topLevelName, out definition);
          break;

        default:
          report.AddDiagnostic(ReportHelper.BadTopLevelSyntax(
            current.Kind, current.Position
          ));
          return;
      }

      AddTopLevel(topLevelName, definition);
    }

    public MiddleRepresentation Parse()
    {
      if (!lexer.HasNextToken)
        return representation;

      // fetching the first token
      // for the first top level definition
      lexer.NextToken();

      while (lexer.HasNextToken)
        ParseTopLevel();

      return representation;
    }
  }
}