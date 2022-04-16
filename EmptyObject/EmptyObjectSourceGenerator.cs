using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EmptyObject;

[Generator]
public class EmptyObjectSourceGenerator : IIncrementalGenerator
{
    private const string EmptyPatternAttribute = "EmptyObject.Attributes.EmptyAttribute";
   // private static StreamWriter StreamWriter ;
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
      //  StreamWriter = new StreamWriter(@"c:/sourceGenerators/log.txt");
        IncrementalValuesProvider<RecordDeclarationSyntax?> recordDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);
        
        IncrementalValueProvider<(Compilation, ImmutableArray<RecordDeclarationSyntax>)> compilationAndRecords
            = context.CompilationProvider.Combine(recordDeclarations.Collect())!;
        
        context.RegisterSourceOutput(compilationAndRecords, 
            static (spc, source) => Execute(source!.Item1,source!.Item2, spc));
    }

    private static void Execute(Compilation compilation, ImmutableArray<RecordDeclarationSyntax> source,
        SourceProductionContext spc)
    {
        foreach (var recordDeclarationSyntax in source)
        {
           var defaultValues= recordDeclarationSyntax.ParameterList.Parameters.Select(syntax => GetDefaultValue(compilation,syntax.Type));
           var fullname = compilation.GetType(recordDeclarationSyntax);
           if(fullname==null)
               return;
           
           var tokens = fullname.Split('.');
           var type = tokens.Last();

           var stringBuilder = new StringBuilder();
           stringBuilder.AppendLine($"using System;");
           stringBuilder.AppendLine($"using System.Collections.Generic;");
           stringBuilder.AppendLine($"namespace {string.Join(".",tokens.Take(tokens.Length-1))};");
         
           stringBuilder.AppendLine($"public partial record {type}");
           stringBuilder.AppendLine("{");
           stringBuilder.AppendLine($"public {type}():this({string.Join(",",defaultValues)})");
           stringBuilder.AppendLine("{");
           stringBuilder.AppendLine("}");
           stringBuilder.AppendLine($"public static {type} Empty = new {type}();");
           stringBuilder.AppendLine("}");
          
           spc.AddSource($"{type}.g.cs", SourceText.From(stringBuilder.ToString(), Encoding.UTF8));
        }
    }

    private static string GetDefaultValue(Compilation compilation,TypeSyntax syntaxType)
    {
        var type = compilation.GetType(syntaxType);
        if (type.EndsWith("?"))
            return "null";
        if (type.EndsWith("[]"))
            return $"System.Array.Empty<{type.Replace("[]","")}>()";
        return type switch
        {
            "string" => "string.Empty",
            "int" => "0",
            "double" => "0",
            "short" => "0",
            "long" => "0",
            "decimal" => "0",
            _ => $"new {type}()"
        };
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
    {
        var isSyntaxTargetForGeneration = syntaxNode is RecordDeclarationSyntax { AttributeLists.Count: > 0  };
        return isSyntaxTargetForGeneration;
    }

    private static RecordDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext ctx)
    {
        var recordDeclarationSyntax = (RecordDeclarationSyntax)ctx.Node;
        foreach (var attributeListSyntax in recordDeclarationSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                var symbol = ctx.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
                IMethodSymbol? attributeSymbol = symbol as IMethodSymbol;
                if(attributeSymbol == null)
                    continue;
                
                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == EmptyPatternAttribute)
                {
                    return recordDeclarationSyntax;
                }

            }
        }
        return null;
    }

}