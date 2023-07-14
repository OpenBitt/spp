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

    public List<IMidInstruction> TypeExpression { get; init; }

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

    public List<IMidInstruction> Body { get; init; }

    public FnDefinition(Position position, string name)
    {
      Position = position;
      Name = name;
      Parameters = new();
      Body = new();
    }
  }

  public struct MidCode
  {
    public Dictionary<string, IDefinition> TopLevels { get; init; } = new();

    public MidCode()
    {
      
    }
  }

  public interface IMidInstruction
  {
    public Position Position { get; init; }
  }

  public struct PushStaticValue : IMidInstruction
  {
    public IStaticValue Value { get; init; }
    public Position Position { get; init; }

    public PushStaticValue(IStaticValue value, Position position)
    {
      Value = value;
      Position = position;
    }
  }
}