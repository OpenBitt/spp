using Errata;

namespace Spp
{
  public struct Position
  {
    public string Filename { get; init; }

    public Location Location { get; init; }

    public Position(string filename, Location location)
    {
      Filename = filename;
      Location = location;
    }

    public override string ToString()
    {
      return $"{Filename}:{Location.Row}:{Location.Column}";
    }
  }
}