﻿using JetBrains.ReSharper.Feature.Services.Lookup;

namespace JetBrains.ReSharper.PostfixTemplates.Templates
{
  [PostfixTemplate(
    templateName: "null",
    description: "Checks expression to be null",
    example: "if (expr == null)")]
  public class CheckIsNullTemplate : CheckForNullTemplateBase, IPostfixTemplate
  {
    public ILookupItem CreateItem(PostfixTemplateContext context)
    {
      var outerExpression = context.OuterExpression;
      if (outerExpression.CanBeStatement)
      {
        if (IsNullable(outerExpression))
        {
          return new CheckForNullStatementItem("null", outerExpression, "if($0==null)");
        }
      }
      else if (!context.IsAutoCompletion)
      {
        var innerExpression = context.InnerExpression;
        if (IsNullable(innerExpression))
        {
          return new CheckForNullExpressionItem("null", innerExpression, "$0==null");
        }
      }

      return null;
    }
  }
}