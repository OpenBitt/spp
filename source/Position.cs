using Errata;

namespace Spp
{
  public struct Position
  {
    public string Filename { get; init; }

    public Range Range { get; init; }

    public Position(string filename, Range range)
    {
      Filename = filename;
      Range = range;
    }

    public override string ToString()
    {
      return $"{Filename}:{Range}";
    }
  }
}