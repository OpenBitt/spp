using Errata;

namespace Spp
{
  public class Generator
  {
    readonly Report report;

    readonly MiddleRepresentation representation;

    readonly Emitter emitter;

    readonly Stack<(IDefinition.Fn Definition, IType.FnType Type)> fnDefinitions;

    readonly Stack<IValue> virtualStack;

    readonly Stack<Dictionary<string, IDefinition>> memories;
    
    public Generator(Report report, MiddleRepresentation representation)
    {
      this.report = report;
      this.representation = representation;

      emitter = new();
      fnDefinitions = new();
      virtualStack = new();
      memories = new();
    }

    IDefinition.Fn CurrentFn => fnDefinitions.Peek().Definition;

    IType.FnType CurrentFnType => fnDefinitions.Peek().Type;

    Dictionary<string, IDefinition> Memory => memories.Peek();

    public string Generate()
    {
      if (TryGetEntryPointMember(out var main))
        ProcessFnDefinition(main);

      return emitter.ToString();
    }

    void PushMemory()
    {
      // shallow copying the global scope
      // as base to the local scope
      memories.Push(new(representation.TopLevels));
    }

    void PopMemory()
    {
      memories.Pop();
    }

    bool TryGetEntryPointMember(out IDefinition.Fn definition)
    {
      definition = new();

      if (!representation.TopLevels.TryGetValue("main", out var opaqueDefinition))
        return false;
      
      if (opaqueDefinition is not IDefinition.Fn fnDefinition)
        return false;
      
      definition = fnDefinition;
      return true;
    }

    void ProcessFnDefinition(IDefinition.Fn definition)
    {
      PushMemory();
      PushFnDefinition(definition);
        ProcessBlock(CurrentFn.Body);
      PopFnDefinition();
      PopMemory();
    }

    void PopFnDefinition()
    {
      fnDefinitions.Pop();
    }

    void PushFnDefinition(IDefinition.Fn definition)
    {
      var type = (IType.FnType)GetDefinitionType(definition);
      fnDefinitions.Push((definition, type));
    }

    IType GetDefinitionType(IDefinition definition)
    {
      if (definition.Type is not null)
        return definition.Type;

      definition.Type = definition switch
      {
        IDefinition.Fn d => GetFnDefinitionType(d.Parameters, d.ReturnTypeExpression),
        IDefinition.Var d => MetaProcessBlockAndGetType(d.TypeExpression),
        _ => throw new NotImplementedException()
      };

      return definition.Type;
    }

    IDefinition GetName(string name, Position position)
    {
      if (Memory.TryGetValue(name, out var definition))
        return definition;
      
      report.AddDiagnostic(ReportHelper.UndefinedMember(
        name, position
      ));
      return new IDefinition.Poisoned(position, name);
    }

    void DeclareName(string name, IDefinition definition)
    {
      if (Memory.TryAdd(name, definition))
        return;
      
      report.AddDiagnostic(ReportHelper.MemberRedefinition(
        name, definition.Position
      ));

      report.AddDiagnostic(ReportHelper.MemberRedefinitionInfo(
        Memory[name].Position
      ));
    }

    IValue GetStaticValueFromDefinition(IDefinition definition, Position position)
    {
      var type = GetDefinitionType(definition);

      if (definition is IDefinition.Static staticDefinition)
        return new IValue.Static(
          staticDefinition.Value,
          type,
          position
        );

      if (definition.HasStaticValue)
        return new IValue.Static(
          definition,
          type,
          position
        );
      
      if (definition is IDefinition.Poisoned)
        return new IValue.Poisoned(type, definition.Position);
      
      report.AddDiagnostic(ReportHelper.NotAStaticValue(
        position
      ));
      return new IValue.Poisoned(
        type, position
      );
    }

    void MetaProcessBlock(CodeChunk block)
    {
      foreach (var instruction in block.Instructions)
        switch (instruction)
        {
          case Instruction.RetVoid i:
            CheckFnReturnType(new IType.Void(), i.Position);
            return;
          
          case Instruction.LoadName i:
            var definition = GetName(i.Name, i.Position);
            var value = GetStaticValueFromDefinition(definition, i.Position);
            VirtualLoad(value);
            break;
          
          default:
            throw new NotImplementedException();
        }
    }

    IValue VirtualPop()
    {
      return virtualStack.Pop();
    }

    void VirtualLoad(IValue value)
    {
      virtualStack.Push(value);
    }

    IType MetaProcessBlockAndGetType(CodeChunk typeExpression)
    {
      MetaProcessBlock(typeExpression);
      var value = VirtualPop();

      if (value is IValue.Static staticValue && value.Type is IType.Type type)
        return (IType)staticValue.Value;
      
      if (value is IValue.Poisoned)
        return new IType.Poisoned();
      
      report.AddDiagnostic(ReportHelper.NotAType(
        value.Type, value.Position
      ));
      return new IType.Poisoned();
    }

    private IType GetFnDefinitionType(
      Dictionary<string, IDefinition.Var> parameters,
      CodeChunk returnTypeExpression
    )
    {
      var parameterTypes = new IType[parameters.Count];

      for (var i = 0; i < parameters.Count; i++)
        parameterTypes[i] = GetDefinitionType(parameters.Values.ElementAt(i));
      
      var returnType = MetaProcessBlockAndGetType(returnTypeExpression);

      return new IType.FnType(
        parameterTypes, returnType
      );
    }

    void ProcessBlock(CodeChunk block)
    {
      foreach (var instruction in block.Instructions)
        ProcessInstruction(instruction);
    }

    void ProcessInstruction(Instruction instruction)
    {
      switch (instruction)
      {
        case Instruction.RetVoid i:
          CheckFnReturnType(new IType.Void(), i.Position);
          break;
        
        case Instruction.LoadImmediate i:
          VirtualLoad(new IValue.Static(
            i.Immediate,
            GetImmediateType(i.Immediate),
            i.Position
          ));
          break;
        
        case Instruction.Ret i:
          var value = VirtualPop();
          CheckFnReturnType(value.Type, value.Position);
          break;
        
        default:
          throw new NotImplementedException();
      }
    }

    IType GetImmediateType(object immediate)
    {
      return immediate switch
      {
        ulong => new IType.Int(),
        _ => throw new NotImplementedException()
      };
    }

    void CheckTypes(IType expected, IType actual, Position position)
    {
      if (expected.Equals(actual))
        return;
      
      report.AddDiagnostic(ReportHelper.TypesMismatch(
        expected, actual, position
      ));
    }

    void CheckFnReturnType(IType type, Position position)
    {
      CheckTypes(CurrentFnType.ReturnType, type, position);
    }
  }
}