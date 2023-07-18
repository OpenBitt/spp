namespace Spp
{
  public interface IType : IEquatable<IType>
  {
    public struct Void : IType
    {
      public int BitSize => 0;

      public bool Equals(IType? other)
      {
        return other is Poisoned or Void;
      }

      public override string ToString()
      {
        return "void";
      }
    }

    public struct Int : IType
    {
      public int BitSize => IntPtr.Size * 8;

      public bool Equals(IType? other)
      {
        return other is Poisoned or Int;
      }

      public override string ToString()
      {
        return "int";
      }
    }

    public struct Fn : IType
    {
      public IType[] ParameterTypes { get; init; }

      public IType ReturnType { get; init; }

      public Fn(IType[] parameterTypes, IType returnType)
      {
        ParameterTypes = parameterTypes;
        ReturnType = returnType;
      }

      public int BitSize => 0;

      public bool Equals(IType? other)
      {
        if (other is Poisoned)
          return true;

        if (other is not Fn otherType)
          return false;
        
        if (ParameterTypes.Length != otherType.ParameterTypes.Length)
          return false;
        
        for (var i = 0; i < ParameterTypes.Length; i++)
          if (!ParameterTypes[i].Equals(otherType.ParameterTypes[i]))
            return false;

        return ReturnType.Equals(otherType.ReturnType);
      }

      public override string ToString()
      {
        var parameterTypes = string.Join<IType>(
          ", ", ParameterTypes
        );
        
        return $"fn ({parameterTypes}) -> {ReturnType}";
      }
    }

    public struct Type : IType
    {
      public int BitSize => 0;

      public bool Equals(IType? other)
      {
        return other is Poisoned or Type;
      }

      public override string ToString()
      {
        return "type";
      }
    }

    public struct Poisoned : IType
    {
      public int BitSize => 0;

      public bool Equals(IType? other)
      {
        return true;
      }

      public override string ToString()
      {
        return "_";
      }
    }

    public int BitSize { get; }

    public int ByteSize => (int)Math.Ceiling(BitSize / 8f);

    public string ToString();
  }

  public interface IValue
  {
    public struct Static : IValue
    {
      public object Value { get; init; }
      public IType Type { get; init; }
      public Position Position { get; init; }

      public Static(object value, IType type, Position position)
      {
        Value = value;
        Type = type;
        Position = position;
      }

      public string Cpp(string type)
      {
        return $"({type})({Helper.ToRepresentationString(Value)})";
      }
    }

    public struct Poisoned : IValue
    {
      public IType Type { get; init; }
      public Position Position { get; init; }

      public Poisoned(IType type, Position position)
      {
        Type = type;
        Position = position;
      }

      public string Cpp(string type)
      {
        return "<?>";
      }
    }


    public string Cpp(string type);
    
    public IType Type { get; init; }

    public Position Position { get; init; }
  }
}