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
  }

  public class MiddleRepresentation
  {
    public Dictionary<string, IDefinition> TopLevels { get; init; }

    public MiddleRepresentation()
    {
      TopLevels = new();
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
    }

    public struct Nop : Instruction
    {
      public Position Position { get; init; }

      public Nop(Position position)
      {
        Position = position;
      }
    }

    public struct Sum : Instruction
    {
      public Position Position { get; init; }

      public Sum(Position position)
      {
        Position = position;
      }
    }

    public struct Sub : Instruction
    {
      public Position Position { get; init; }

      public Sub(Position position)
      {
        Position = position;
      }
    }

    public struct Mul : Instruction
    {
      public Position Position { get; init; }

      public Mul(Position position)
      {
        Position = position;
      }
    }

    public struct Div : Instruction
    {
      public Position Position { get; init; }

      public Div(Position position)
      {
        Position = position;
      }
    }

    public struct Rem : Instruction
    {
      public Position Position { get; init; }

      public Rem(Position position)
      {
        Position = position;
      }
    }

    public Position Position { get; init; }
  }
}