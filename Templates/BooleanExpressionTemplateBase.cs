﻿using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;

// todo: disable in cases like typeReference.Name == NotNullAttribute.if

namespace JetBrains.ReSharper.PostfixTemplates.Templates
{
  public abstract class BooleanExpressionTemplateBase
  {
    public ILookupItem CreateItem(PostfixTemplateContext context)
    {
      foreach (var expressionContext in context.Expressions)
      {
        if (expressionContext.Type.IsBool() || IsBooleanExpression(expressionContext.Expression))
        {
          var lookupItem = CreateBooleanItem(expressionContext);
          if (lookupItem != null) return lookupItem;
        }
      }

      if (context.IsForceMode)
      {
        foreach (var expressionContext in context.Expressions)
        {
          var lookupItem = CreateBooleanItem(expressionContext);
          if (lookupItem != null) return lookupItem;
        }
      }

      return null;
    }

    private static bool IsBooleanExpression([CanBeNull] ICSharpExpression expression)
    {
      return expression is IRelationalExpression
          || expression is IEqualityExpression
          || expression is IConditionalAndExpression
          || expression is IConditionalOrExpression
          || expression is IUnaryOperatorExpression // TODO: check with +expr and other non-boolean unary
          || expression is IIsExpression;
    }

    [CanBeNull]
    protected abstract ILookupItem CreateBooleanItem([NotNull] PrefixExpressionContext expression);
  }
}