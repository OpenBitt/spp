namespace Spp
{
  public interface IStaticType
  {
    public int BitSize { get; }

    public int ByteSize => (int)Math.Ceiling(BitSize / 8f);

    public string ToString();
  }

  public interface IStaticValue
  {
    public IStaticType Type { get; init; }
  }
}