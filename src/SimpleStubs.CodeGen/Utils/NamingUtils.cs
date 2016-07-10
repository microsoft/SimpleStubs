using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Etg.SimpleStubs.CodeGen.Utils
{
    internal class NamingUtils
    {
        public string GetInterfaceStubName(string interfaceName)
        {
            return "Stub" + interfaceName;
        }

	    public static string GetSetupMethodName(IMethodSymbol methodSymbol)
	    {
		    if (methodSymbol.IsPropertyGetter())
		    {
			    return methodSymbol.Name.Substring(4) + "_Get";
		    }
		    if (methodSymbol.IsPropertySetter())
		    {
			    return methodSymbol.Name.Substring(4) + "_Set";
		    }
			return methodSymbol.GetGenericName();
	    }

	    public static string GetDelegateTypeName(IMethodSymbol methodSymbol, INamedTypeSymbol targetInterface)
        {
			string methodName = methodSymbol.Name;
			if (methodSymbol.IsPropertyGetter())
			{
				methodName = methodName.Substring(4) + "_Get";
			}
			else if (methodSymbol.IsPropertySetter())
			{
				methodName = methodName.Substring(4) + "_Set";
			}

			// only prefix inherited members
			if (targetInterface.GetGenericName() != methodSymbol.ContainingSymbol.GetGenericName())
			{
				methodName = SerializeName(methodSymbol.ContainingSymbol) + "_" + methodName;
			}

			if (methodSymbol.IsOrdinaryMethod())
			{
				if (methodSymbol.Parameters.Any())
				{
					methodName = methodName + "_" + string.Join("_", methodSymbol.Parameters.Select(SerializeName));
				}
			}

			methodName += "_Delegate";

			if (methodSymbol.IsGenericMethod)
			{
				methodName =
					$"{methodName}<{string.Join(",", methodSymbol.TypeParameters.Select(symbol => symbol.Name))}>";
			}
			return methodName;
        }


        public static string SerializeName(ISymbol param)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var part in param.ToDisplayParts(SymbolDisplayFormat.MinimallyQualifiedFormat))
            {
                switch (part.ToString())
                {
                    case "<":
                        sb.Append("Of");
                        break;
                    case ">":
                        break;
                    case "]":
                        break;
                    case "[":
                        sb.Append("Array");
                        break;
                    default:
                        if (part.Symbol != null && part.Kind != SymbolDisplayPartKind.ParameterName)
                        {
                            sb.Append(part.Symbol.Name);
                        }
                        break;
                }
            }

            return sb.ToString();
        }

        public static string GetStubName(string interfaceName)
        {
            return "Stub" + interfaceName;
        }
    }
}