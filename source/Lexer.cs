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
    Pass,
    Comma,
    Arrow,
    Minus,
    Plus,
    Star,
    Slash,
    Reminder,
    Return
  }

  public enum TokenMode
  {
    OnTheSameLine,
    OnNewLine,
    NoMatter
  }
  
  public struct Token
  {
    public TokenKind Kind { get; init; }
    
    public string Value { get; init; }

    public int Indent { get; init; }

    public TokenMode Mode { get; init; }

    public Position Position { get; init; }

    public Token(TokenKind kind, string value, int indent, TokenMode mode, Position position)
    {
      Kind = kind;
      Value = value;
      Indent = indent;
      Mode = mode;
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
      ["return"] = TokenKind.Return,
    };

    readonly Dictionary<string, TokenKind> DOUBLE_PUNCTUATIONS = new()
    {
      ["=="] = TokenKind.EqEq,
      ["->"] = TokenKind.Arrow,
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
      [','] = TokenKind.Comma,
      ['+'] = TokenKind.Plus,
      ['-'] = TokenKind.Minus,
      ['*'] = TokenKind.Star,
      ['/'] = TokenKind.Slash,
      ['%'] = TokenKind.Reminder,
    };

    public Token Previous { get; private set; }

    public Token Current { get; private set; }

    public Token Next { get; private set; }

    readonly Report report;
    readonly string filename;
    readonly string code;

    int index;
    int indent;
    TokenMode mode;
    int row;
    int indexOfLineStart;

    public Lexer(Report report, string filename)
    {
      this.report = report;
      this.filename = filename;

      code = File.ReadAllText(filename);
      index = 0;
      indent = 0;
      mode = TokenMode.OnNewLine;
      row = 0;
      indexOfLineStart = 0;

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

    TokenMode ConsumeTokenMode()
    {
      var curMode = mode;
      mode = TokenMode.OnTheSameLine;

      return curMode;
    }

    int ConsumeIndent(TokenMode mode, Position position)
    {
      var curIndent = indent;
      indent = 0;

      // the token's indent can't be odd
      if (mode == TokenMode.OnNewLine && curIndent % 2 != 0)
      {
        report.AddDiagnostic(ReportHelper.OddIndent(
          curIndent, position
        ));
        curIndent -= 1;
      }

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
            indent++;
            break;
          
          case '\n':
            row++;
            indexOfLineStart = index + 1;
            indent = 0;
            mode = TokenMode.OnNewLine;
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

    void IsKeywordToken(string identifier, ref TokenKind kind)
    {
      foreach (var kw in KEYWORDS)
        if (kw.Key == identifier)
        {
          kind = kw.Value;
          return;
        }
    }

    Token CollectWordToken()
    {
      var position = CurrentPosition;
      var length = 0;
      var startingIndex = index;
      var mode = ConsumeTokenMode();
      var indent = ConsumeIndent(mode, position);

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

      var kind = TokenKind.Identifier;
      IsNumberToken(value, ref kind);
      IsKeywordToken(value, ref kind);

      return new(kind, value, indent, mode, position);
    }

    void IsNumberToken(string value, ref TokenKind kind)
    {
      if (char.IsNumber(value[0]))
        kind = TokenKind.Number;
    }

    bool IsDoublePunctuationToken(ref TokenKind kind, ref string value)
    {
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

    void IsSinglePunctuationToken(ref TokenKind kind, ref string value)
    {
      value = char.ToString(CurrentChar);

      foreach (var singlep in SINGLE_PUNCTUATIONS)
        if (singlep.Key == CurrentChar)
        {
          kind = singlep.Value;
          return;
        }
    }

    Token CollectPunctuationTokenOrBad()
    {
      var position = CurrentPosition;
      var mode = ConsumeTokenMode();
      var indent = ConsumeIndent(mode, position);
      var kind = TokenKind.Bad;
      var value = char.ToString(CurrentChar);

      if (!IsDoublePunctuationToken(ref kind, ref value))
        IsSinglePunctuationToken(ref kind, ref value);
      
      return new(kind, value, indent, mode, position);
    }

    Token Tokenize()
    {
      EatWhite();

      Token token;
      if (!HasChar)
        return new(
          TokenKind.Eof, "", 0,
          TokenMode.NoMatter, Current.Position
        );

      if (MatchWordChar())
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

    public ulong ParseNumberToken(Token token)
    {
      if (ulong.TryParse(token.Value, out var result))
        return result;
      
      report.AddDiagnostic(ReportHelper.MalformedNumber(
        token.Value, token.Position
      ));
      return 0;
    }
  }
}