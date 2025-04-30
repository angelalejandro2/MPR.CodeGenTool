// Analysis/Roslyn/Extensions/TypeSymbolExtensions.cs
using Microsoft.CodeAnalysis;

namespace MPR.CodeGenTool.Analysis.Roslyn.Extensions
{
    public static class TypeSymbolExtensions
    {
        public static bool IsPrimitive(this ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
                return false;
            
            var specialType = typeSymbol.SpecialType;
            
            return specialType == SpecialType.System_Boolean ||
                   specialType == SpecialType.System_Byte ||
                   specialType == SpecialType.System_Char ||
                   specialType == SpecialType.System_Double ||
                   specialType == SpecialType.System_Int16 ||
                   specialType == SpecialType.System_Int32 ||
                   specialType == SpecialType.System_Int64 ||
                   specialType == SpecialType.System_UInt16 ||
                   specialType == SpecialType.System_UInt32 ||
                   specialType == SpecialType.System_UInt64 ||
                   specialType == SpecialType.System_Single ||
                   specialType == SpecialType.System_String ||
                   specialType == SpecialType.System_DateTime;
        }
    }
}