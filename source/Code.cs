using Newtonsoft.Json;
using System.Text;

namespace Spp
{
  public interface IDefinition
  {
    public Position Position { get; init; }

    public string Name { get; init; }

    public string ToString();
  }

  public struct VarDefinition : IDefinition
  {
    public Position Position { get; init; }

    public string Name { get; init; }

    public CodeChunk TypeExpression { get; init; }

    public VarDefinition(Position position, string name)
    {
      Position = position;
      Name = name;
      TypeExpression = new();
    }

    public override string ToString()
    {
      return $"TypeExpression: {{{TypeExpression.ToString()}}}";
    }
  }

  public struct FnDefinition : IDefinition
  {
    public Position Position { get; init; }

    public string Name { get; init; }

    public Dictionary<string, VarDefinition> Parameters { get; init; }

    public CodeChunk ReturnTypeExpression { get; init; }

    public CodeChunk Body { get; init; }

    public FnDefinition(Position position, string name)
    {
      Position = position;
      Name = name;
      Parameters = new();
      Body = new();
      ReturnTypeExpression = new();
    }

    public override string ToString()
    {
      var parameters = new StringBuilder("\n");

      foreach (var parameter in Parameters)
        parameters.AppendLine($"\"{parameter.Key}\" -> {parameter.Value},");

      return $"Parameters: {{{parameters}}}, ReturnTypeExpression: {{{ReturnTypeExpression}}}, Body: {{{Body}}}";
    }
  }

  public class MiddleRepresentation
  {
    public Dictionary<string, IDefinition> TopLevels { get; init; }

    public MiddleRepresentation()
    {
      TopLevels = new();
    }

    string ProduceIndent(int indent)
    {
      return new string(' ', indent);
    }

    public string ToJSON()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public override string ToString()
    {
      var b = new StringBuilder("TopLevels {\n");

      foreach (var toplevel in TopLevels)
      {
        b.AppendLine($"\"{toplevel.Key}\" -> {toplevel.Value}");
        b.AppendLine();
      }

      b.AppendLine("}");

      // reformatting the string representation (with indents)
      var lines = b.ToString().Split('\n');
      var formatted = new StringBuilder();
      var indent = 0;
      const int INDENT_STEP = 2;

      foreach (var line in lines)
      {
        if (line.StartsWith("}"))
          indent -= INDENT_STEP;

        formatted.AppendLine($"{ProduceIndent(indent)}{line}");
        
        if (line.EndsWith("{"))
          indent += INDENT_STEP;
      }
      
      return formatted.ToString();
    }
  }

  public class CodeChunk
  {
    public List<Instruction> Instructions { get; init; }

    public CodeChunk()
    {
      Instructions = new();
    }

    
    public override string ToString()
    {
      var b = new StringBuilder("\n");

      foreach (var i in Instructions)
        b.AppendLine(i.ToString());

      return b.ToString();
    }

    public Instruction Emit(Instruction instruction)
    {
      Instructions.Add(instruction);
      return instruction;
    }

    public void LoadName(string name, Position position)
    {
      Emit(new Instruction.LoadName(
        name, position
      ));
    }

    void LoadImmediate(object immediate, Position position)
    {
      Emit(new Instruction.LoadImmediate(
        immediate, position
      ));
    }

    public void LoadImmediate(ulong immediate, Position position) => LoadImmediate((object)immediate, position);

    public void LoadImmediate(string immediate, Position position) => LoadImmediate((object)immediate, position);

    public void LoadImmediate(double immediate, Position position) => LoadImmediate((object)immediate, position);

    public void LoadImmediate(char immediate, Position position) => LoadImmediate((object)immediate, position);

    public void Nop(Position position)
    {
      Emit(new Instruction.Nop(
        position
      ));
    }

    public void Ret(Position position)
    {
      Emit(new Instruction.Ret(
        position
      ));
    }

    public void RetVoid(Position position)
    {
      Emit(new Instruction.RetVoid(
        position
      ));
    }

    public void Pop(Position position)
    {
      Emit(new Instruction.Pop(
        position
      ));
    }

    public void Call(int argumentsCount, Position position)
    {
      Emit(new Instruction.Call(
        argumentsCount, position
      ));
    }

    public void LoadAttribute(string name, Position position)
    {
      Emit(new Instruction.LoadAttribute(
        name, position
      ));
    }

    public void Selection(CodeChunk then, CodeChunk otherwise, Position position)
    {
      Emit(new Instruction.Selection(
        then, otherwise, position
      ));
    }

    public void BinaryOp(TokenKind op, Position position)
    {
      Emit(op switch
      {
        TokenKind.Plus => new Instruction.Sum(position),
        TokenKind.Minus => new Instruction.Sub(position),
        TokenKind.Star => new Instruction.Mul(position),
        TokenKind.Slash => new Instruction.Div(position),
        TokenKind.Reminder => new Instruction.Rem(position),
        _ => throw new ArgumentException()
      });
    }

    public void Loop(CodeChunk loop, Position position)
    {
      Emit(new Instruction.Loop(
        loop, position
      ));
    }

    public void Break(Position position)
    {
      Emit(new Instruction.Break(
        position
      ));
    }

    public void Declare(bool mutable, string value, CodeChunk type, Position position)
    {
      Emit(new Instruction.Declare(
        mutable, value, type, position
      ));
    }
  }

  public interface Instruction
  {
    public struct LoadImmediate : Instruction
    {
      public object Immediate { get; init; }
      public Position Position { get; init; }

      public string ImmediateRepr {
        get
        {
          var s = Immediate.ToString();

          if (s is null)
            return "";

          return
            s
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\v", "\\v")
            .Replace("\0", "\\0");
        }
      }

      public LoadImmediate(object immediate, Position position)
      {
        Immediate = immediate;
        Position = position;
      }

      public override string ToString()
      {
        return Immediate switch
        {
          string => $"LoadImmediate \"{ImmediateRepr}\"",
          char => $"LoadImmediate '{ImmediateRepr}'",
          _ => $"LoadImmediate {Immediate}",
        };
      }
    }

    public struct LoadName : Instruction
    {
      public string Name { get; init; }
      public Position Position { get; init; }

      public LoadName(string name, Position position)
      {
        Name = name;
        Position = position;
      }

      public override string ToString()
      {
        return $"LoadName \"{Name}\"";
      }
    }

    public struct Nop : Instruction
    {
      public Position Position { get; init; }

      public Nop(Position position)
      {
        Position = position;
      }

      public override string ToString()
      {
        return "Nop";
      }
    }

    public struct Sum : Instruction
    {
      public Position Position { get; init; }

      public Sum(Position position)
      {
        Position = position;
      }

      public override string ToString()
      {
        return "Sum";
      }
    }

    public struct Sub : Instruction
    {
      public Position Position { get; init; }

      public Sub(Position position)
      {
        Position = position;
      }

      public override string ToString()
      {
        return "Sub";
      }
    }

    public struct Mul : Instruction
    {
      public Position Position { get; init; }

      public Mul(Position position)
      {
        Position = position;
      }

      public override string ToString()
      {
        return "Mul";
      }
    }

    public struct Div : Instruction
    {
      public Position Position { get; init; }

      public Div(Position position)
      {
        Position = position;
      }

      public override string ToString()
      {
        return "Div";
      }
    }

    public struct Rem : Instruction
    {
      public Position Position { get; init; }

      public Rem(Position position)
      {
        Position = position;
      }

      public override string ToString()
      {
        return "Rem";
      }
    }

    public struct Ret : Instruction
    {
      public Position Position { get; init; }

      public Ret(Position position)
      {
        Position = position;
      }

      public override string ToString()
      {
        return "Ret";
      }
    }

    public struct RetVoid : Instruction
    {
      public Position Position { get; init; }

      public RetVoid(Position position)
      {
        Position = position;
      }

      public override string ToString()
      {
        return "RetVoid";
      }
    }

    public struct Pop : Instruction
    {
      public Position Position { get; init; }

      public Pop(Position position)
      {
        Position = position;
      }

      public override string ToString()
      {
        return "Pop";
      }
    }

    public struct Call : Instruction
    {
      public int ArgumentsCount { get; init; }
      public Position Position { get; init; }

      public Call(int argumentsCount, Position position)
      {
        ArgumentsCount = argumentsCount;
        Position = position;
      }

      public override string ToString()
      {
        return $"Call {ArgumentsCount}";
      }
    }

    public struct LoadAttribute : Instruction
    {
      public string Name { get; init; }
      public Position Position { get; init; }

      public LoadAttribute(string name, Position position)
      {
        Name = name;
        Position = position;
      }

      public override string ToString()
      {
        return $"LoadAttribute \"{Name}\"";
      }
    }

    public struct Selection : Instruction
    {
      public CodeChunk Then { get; init; }

      public CodeChunk Otherwise { get; init; }

      public Position Position { get; init; }

      public Selection(CodeChunk then, CodeChunk otherwise, Position position)
      {
        Then = then;
        Otherwise = otherwise;
        Position = position;
      }

      public override string ToString()
      {
        return $"Selection then {{{Then}}} otherwise {{{Otherwise}}}";
      }
    }

    public struct Loop : Instruction
    {
      public CodeChunk Body { get; init; }

      public Position Position { get; init; }

      public Loop(CodeChunk body, Position position)
      {
        Body = body;
        Position = position;
      }

      public override string ToString()
      {
        return $"Loop {{{Body}}}";
      }
    }

    public struct Break : Instruction
    {
      public Position Position { get; init; }

      public Break(Position position)
      {
        Position = position;
      }

      public override string ToString()
      {
        return "Break";
      }
    }

    public struct Declare : Instruction
    {
      public bool Mutable { get; init; }

      public string Name { get; init; }

      public CodeChunk TypeExpression { get; init; }

      public Position Position { get; init; }

      public Declare(bool mutable, string name, CodeChunk typeExpression, Position position)
      {
        Mutable = mutable;
        Name = name;
        TypeExpression = typeExpression;
        Position = position;
      }

      public override string ToString()
      {
        return $"Declare Mutable: {Mutable}, Name: \"{Name}\", TypeExpression: {{{TypeExpression}}}";
      }
    }

    public Position Position { get; init; }

    public string ToString();
  }
}