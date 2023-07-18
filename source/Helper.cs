namespace Spp
{
  public static class Helper
  {
    public const int INDENT_STEP = 2;

    public static string ToRepresentationString(object o)
    {
      return o switch
      {
        null => "<null>",
        string s => $"\"{UnescapeString(s)}\"",
        char c => $"'{UnescapeString(c.ToString())}'",
        _ => o.ToString()!
      };
    }

    public static string UnescapeString(string s)
    {
      return
        s
        .Replace("\n", "\\n")
        .Replace("\r", "\\r")
        .Replace("\t", "\\t")
        .Replace("\v", "\\v")
        .Replace("\0", "\\0");
    }

    public static string ProduceIndent(int indent)
    {
      return new string(' ', indent);
    }
  }
}