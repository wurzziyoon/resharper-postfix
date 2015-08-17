﻿using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.PostfixTemplates.Contexts.CSharp;
using JetBrains.ReSharper.Psi;

namespace JetBrains.ReSharper.PostfixTemplates.Templates.CSharp
{
  [PostfixTemplate(
    templateName: "parse",
    description: "Parses string as value of some type",
    example: "int.Parse(expr)")]
  public class ParseStringTemplate : ParseStringTemplateBase, IPostfixTemplate<CSharpPostfixTemplateContext>
  {
    public ILookupItem CreateItem(CSharpPostfixTemplateContext context)
    {
      foreach (var expressionContext in context.Expressions)
      {
        var expressionType = expressionContext.Type;
        if (expressionType.IsResolved && expressionType.IsString())
        {
          return new ParseItem("parse", expressionContext, isTryParse: false);
        }
      }

      return null;
    }
  }
}