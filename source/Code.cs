using Newtonsoft.Json;
using System.Text;

namespace Spp
{
  public interface IDefinition
  {
    public Position Position { get; init; }

    public string Name { get; init; }
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
      return Body.ToString();
    }
  }

  public class MiddleRepresentation
  {
    public Dictionary<string, IDefinition> TopLevels { get; init; }

    public MiddleRepresentation()
    {
      TopLevels = new();
    }

    public string ToJSON()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public override string ToString()
    {
      var b = new StringBuilder("TopLevels {\n");

      foreach (var toplevel in TopLevels)
        b.AppendLine($"  {toplevel.Key} {{\n{toplevel.Value}  }}\n");

      b.AppendLine("}");
      return b.ToString();
    }
  }

  public class CodeChunk
  {
    public List<Instruction> Instructions { get; init; }

    public CodeChunk()
    {
      Instructions = new();
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

    public override string ToString()
    {
      var b = new StringBuilder();

      foreach (var i in Instructions)
        b.AppendLine($"    {i}");

      return b.ToString();
    }
  }

  public interface Instruction
  {
    public struct LoadImmediate : Instruction
    {
      public object Immediate { get; init; }
      public Position Position { get; init; }

      public LoadImmediate(object immediate, Position position)
      {
        Immediate = immediate;
        Position = position;
      }

      public override string ToString()
      {
        return Immediate switch
        {
          string => $"LoadImmediate \"{Immediate}\"",
          char => $"LoadImmediate '{Immediate}'",
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

    public Position Position { get; init; }

    public string ToString();
  }
}