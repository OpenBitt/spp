using System.Text;

namespace Spp
{
  public class Emitter
  {
    readonly StringBuilder code;

    public Emitter()
    {
      code = new();
    }

    public override string ToString()
    {
      return code.ToString();
    }
  }
}