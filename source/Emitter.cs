using System.Text;

namespace Spp
{
  public class Emitter
  {
    readonly StringBuilder code;

    readonly Stack<StringBuilder> fnInProgress;

    int indent;

    public Emitter()
    {
      code = new();
      fnInProgress = new();
      indent = 0;

      var intType =
        new IType.Int().BitSize == 64
          ? "int64_t"
          : "int32_t";

      code.AppendLine(@$"

namespace __spp_compiler_internals__
{{
  namespace included
  {{
    #include <stdint.h>
  }}

  namespace types
  {{
    typedef void Void;
    typedef included::{intType} Int;
  }}
}}

");
    }

    StringBuilder CurrentFn => fnInProgress.Peek();

    public override string ToString()
    {
      return code.ToString();
    }

    string ToCppType(IType type)
    {
      return type switch
      {
        IType.Void => "__spp_compiler_internals__::types::Void",
        IType.Int => "__spp_compiler_internals__::types::Int",
        _ => throw new NotImplementedException()
      };
    }

    string SanitizeName(string name)
    {
      return $"__{name}";
    }

    public void PopFn()
    {
      indent -= Helper.INDENT_STEP;
      code.Append(fnInProgress.Pop());
      code.AppendLine("}");
    }

    public void PushFn(string name, IType.Fn type)
    {
      var parameterTypes = string.Join<IType>(", ", type.ParameterTypes);

      fnInProgress.Push(new(
        $"{ToCppType(type.ReturnType)} {SanitizeName(name)}({parameterTypes}) {{\n"
      ));

      indent += Helper.INDENT_STEP;
    }

    public void EmitStatement(string statement)
    {
      CurrentFn.AppendLine($"{Helper.ProduceIndent(indent)}{statement};");
    }

    public void Ret(IValue value)
    {
      EmitStatement($"return {value.Cpp(ToCppType(value.Type))}");
    }

    public void RetVoid()
    {
      EmitStatement("return");
    }
  }
}