using Errata;

namespace Spp
{
  public enum TokenKind
  {
    Bad,
    Eof,
    Identifier,
    Fn,
    Number,
    EqEq,
    Colon,
    LPar,
    RPar,
    LBracket,
    RBracket,
    LBrace,
    RBrace,
    Pass
  }
  
  public struct Token
  {
    public TokenKind Kind { get; init; }
    
    public string Value { get; init; }

    public int Indent { get; init; }

    public Position Position { get; init; }

    public Token(TokenKind kind, string value, int indent, Position position)
    {
      Kind = kind;
      Value = value;
      Indent = indent;
      Position = position;
    }

    public override string ToString()
    {
      return $"Token(Kind: {Kind}, Value: \"{Value}\", Indent: {Indent}, Position: {Position})";
    }
  }

  public class Lexer
  {
    readonly Dictionary<string, TokenKind> KEYWORDS = new()
    {
      ["fn"] = TokenKind.Fn,
      ["pass"] = TokenKind.Pass,
    };

    readonly Dictionary<string, TokenKind> DOUBLE_PUNCTUATIONS = new()
    {
      ["=="] = TokenKind.EqEq,
    };

    readonly Dictionary<char, TokenKind> SINGLE_PUNCTUATIONS = new()
    {
      [':'] = TokenKind.Colon,
      ['('] = TokenKind.LPar,
      [')'] = TokenKind.RPar,
      ['['] = TokenKind.LBracket,
      [']'] = TokenKind.RBracket,
      ['{'] = TokenKind.LBrace,
      ['}'] = TokenKind.RBrace,
    };

    public const int NOT_INDENTED = -1;

    public Token Previous { get; private set; }

    public Token Current { get; private set; }

    public Token Next { get; private set; }

    readonly Report report;
    readonly string filename;
    readonly string code;

    int index;
    int row;
    int indent;
    int indexOfLineStart;

    public Lexer(Report report, string filename)
    {
      this.report = report;
      this.filename = filename;

      code = File.ReadAllText(filename);
      index = 0;
      row = 0;
      indent = 0;
      indexOfLineStart = 0;

      Previous = Current = Next = new(TokenKind.Bad, "", 0, new());
      NextToken(); // now `Next` is something

      var repo = report.Repository as InMemorySourceRepository;
      repo!.Register(filename, code);
    }

    public bool HasNextToken => Next.Kind != TokenKind.Eof;

    bool HasChar => index < code.Length;

    char CurrentChar => code[index];

    bool HasNextChar => index + 1 < code.Length;

    char NextChar => code[index + 1];

    Position CurrentPosition => new(filename, new(row + 1, index - indexOfLineStart + 1));

    int ConsumeIndent(Position position)
    {
      var curIndent = indent;
      indent = NOT_INDENTED;

      // when the token is indented
      // and the indent is not multiple of 2
      if (curIndent != NOT_INDENTED && curIndent % 2 != 0)
        report.AddDiagnostic(ReportHelper.OddIndent(curIndent, position));

      return curIndent;
    }

    void Advance(int count = 1)
    {
      index += count;
    }

    void EatWhite()
    {
      while (HasChar)
      {
        switch (CurrentChar)
        {
          case '\t':
            report.AddDiagnostic(ReportHelper.FoundTab(CurrentPosition));
            break;

          case ' ':
            if (indent != NOT_INDENTED)
              indent++;
            break;
          
          case '\n':
            row++;
            indexOfLineStart = index + 1;
            indent = 0;
            break;
          
          case '\r':
            report.AddDiagnostic(ReportHelper.FoundCarriageReturn(CurrentPosition));
            break;

          default:
            return;
        }

        Advance();
      }
    }

    bool MatchWordChar()
    {
      return
        char.IsLetterOrDigit(CurrentChar) ||
        CurrentChar == '_';
    }

    bool IsKeyword(string identifier, out TokenKind kind)
    {
      foreach (var kw in KEYWORDS)
        if (kw.Key == identifier)
        {
          kind = kw.Value;
          return true;
        }
      
      kind = TokenKind.Bad;
      return false;
    }

    Token CollectWordToken()
    {
      var position = CurrentPosition;
      var length = 0;
      var startingIndex = index;
      var indent = ConsumeIndent(position);

      while (HasChar && MatchWordChar())
      {
        Advance();
        length++;
      }

      // going back to the last word's char
      // so that we don't loose any token
      Advance(-1);

      var value = new string(
        code.ToCharArray(),
        startingIndex,
        length
      );

      if (char.IsNumber(value[0]))
        return new(TokenKind.Number, value, indent, position);
      
      if (IsKeyword(value, out var kind))
        return new(kind, value, indent, position);
      
      return new(TokenKind.Identifier, value, indent, position);
    }

    bool IsDoublePunctuation(out TokenKind kind, out string value)
    {
      kind = TokenKind.Bad;
      value = "";
      
      if (!HasNextChar)
        return false;
      
      value = char.ToString(CurrentChar) + char.ToString(NextChar);

      foreach (var doublep in DOUBLE_PUNCTUATIONS)
        if (doublep.Key == value)
        {
          Advance();
          kind = doublep.Value;
          return true;
        }
      
      return false;
    }

    bool IsSinglePunctuation(out TokenKind kind, out string value)
    {
      kind = TokenKind.Bad;
      value = char.ToString(CurrentChar);

      foreach (var singlep in SINGLE_PUNCTUATIONS)
        if (singlep.Key == CurrentChar)
        {
          kind = singlep.Value;
          return true;
        }
      
      return false;
    }

    Token CollectPunctuationTokenOrBad()
    {
      var position = CurrentPosition;
      var indent = ConsumeIndent(position);

      if (IsDoublePunctuation(out var kind, out var value))
        return new(kind, value, indent, position);
      
      if (IsSinglePunctuation(out kind, out value))
        return new(kind, value, indent, position);
      
      return new(TokenKind.Bad, value, indent, position);
    }

    Token Tokenize()
    {
      EatWhite();

      Token token;
      if (!HasChar)
        token = new(TokenKind.Eof, "", 0, CurrentPosition);
      else if (MatchWordChar())
        token = CollectWordToken();
      else
        token = CollectPunctuationTokenOrBad();
      
      Advance();
      return token;
    }

    public Token NextToken()
    {
      Previous = Current;
      Current = Next;
      Next = Tokenize();

      return Current;
    }
  }
}