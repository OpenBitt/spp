using System.Text;

namespace Spp
{
  public class Emitter
  {
    readonly StringBuilder head;

    readonly StringBuilder body;

    readonly Stack<StringBuilder> fnInProgress;

    int indent;

    public Emitter()
    {
      head = new();
      body = new();
      fnInProgress = new();
      indent = 0;

      var intType =
        new IType.Int().BitSize == 64
          ? "int64_t"
          : "int32_t";

      head.AppendLine(@$"
// Spp Builtins
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

// Spp Head");
    }

    StringBuilder CurrentFn => fnInProgress.Peek();

    public override string ToString()
    {
      head.AppendLine("\n// Spp Body");
      head.Append(body);
      head.Append(@"
// Spp Entry
int main()
{
  (void)__main();
  return 0;
}");

      return head.ToString();
    }

    string ToCppType(IType type)
    {
      return type switch
      {
        IType.Void => "__spp_compiler_internals__::types::Void",
        IType.Int => "__spp_compiler_internals__::types::Int",
        IType.Poisoned => "<?>",
        _ => throw new NotImplementedException($"{type} cannot be converted")
      };
    }

    internal string SanitizeName(string name)
    {
      return $"__{name}";
    }

    public void PopFn()
    {
      indent -= Helper.INDENT_STEP;
      body.Append(fnInProgress.Pop());
      body.AppendLine("}");
    }

    public void PushFn(
      string name,
      IType.Fn type,
      string[] parameterNames
    )
    {
      head.AppendFormat("{0};\n", BuildFnPrototype(name, type, parameterNames: null));

      fnInProgress.Push(new(
        $"{BuildFnPrototype(name, type, parameterNames)} {{\n"
      ));

      indent += Helper.INDENT_STEP;
    }

    string JoinParameters(IType[] parameterTypes, string[] parameterNames)
    {
      var parameters = new StringBuilder();

      for (var i = 0; i < parameterTypes.Length; i++)
        parameters.AppendFormat(
          "{0} {1}",
          parameterTypes[i], SanitizeName(parameterNames[i])
        );

      return parameters.ToString();
    }

    string BuildFnPrototype(string name, IType.Fn type, string[]? parameterNames)
    {
      var parameters =
        parameterNames is not null
          ? JoinParameters(type.ParameterTypes, parameterNames!)
          : string.Join<IType>(", ", type.ParameterTypes);
      
      return $"{ToCppType(type.ReturnType)} {SanitizeName(name)}({parameters})";
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