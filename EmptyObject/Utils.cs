using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EmptyObject;

public static class Utils
{
    public static string? GetType(this Compilation compilation, RecordDeclarationSyntax recordDeclarationSyntax)
    {
        SemanticModel semanticModel = compilation.GetSemanticModel(recordDeclarationSyntax.SyntaxTree);
        return semanticModel.GetDeclaredSymbol(recordDeclarationSyntax) is not INamedTypeSymbol symbol ? null : symbol.ToString();
    }
    public static string? GetType(this Compilation compilation, TypeSyntax typeSyntax)
    {
        return typeSyntax.ToFullString().Trim();
    }
}