﻿using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Hotspots;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.LiveTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
using JetBrains.ReSharper.PostfixTemplates.Contexts;
using JetBrains.ReSharper.PostfixTemplates.Contexts.CSharp;
using JetBrains.ReSharper.PostfixTemplates.LookupItems;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Resolve.Filters;
using JetBrains.ReSharper.Psi.Naming.Extentions;
using JetBrains.ReSharper.Psi.Naming.Impl;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.TextControl;
using JetBrains.Util;

namespace JetBrains.ReSharper.PostfixTemplates.Templates.CSharp
{
  public abstract class ForLoopTemplateBase : IPostfixTemplate<CSharpPostfixTemplateContext>
  {
    public abstract PostfixTemplateInfo TryCreateInfo(CSharpPostfixTemplateContext context);

    protected bool CanBeLoopedOver([NotNull] CSharpPostfixTemplateContext context, [CanBeNull] out string lengthName)
    {
      lengthName = null;

      var expressionContext = context.InnerExpression;
      if (expressionContext == null || !expressionContext.CanBeStatement) return false;

      var expression = expressionContext.Expression;
      if (context.IsPreciseMode && !expression.IsPure()) return false;

      if (expressionContext.Type is IArrayType)
      {
        // todo: test with array of unresolved types
        lengthName = "Length";
        return true;
      }

      if (!expressionContext.Type.IsResolved) return false; // even in force mode

      if (expressionContext.Type.IsPredefinedIntegralNumeric())
      {
        if (expressionContext.PostfixContext.IsPreciseMode) return false;

        return true;
      }

      var psiModule = expressionContext.PostfixContext.PsiModule;
      var symbolTable = expressionContext.Type.GetSymbolTable(psiModule);

      var accessFilter = new AccessRightsFilter(new ElementAccessContext(expression));
      var publicProperties = symbolTable.Filter(myPropertyFilter, OverriddenFilter.INSTANCE, accessFilter);

      const string countPropertyName = "Count";

      var resolveResult = publicProperties.GetResolveResult(countPropertyName);

      var property = resolveResult.DeclaredElement as IProperty;
      if (property != null)
      {
        if (property.IsStatic) return false;
        if (!property.Type.IsInt()) return false;

        lengthName = countPropertyName;
        return true;
      }

      return false;
    }

    [NotNull] private readonly DeclaredElementTypeFilter myPropertyFilter =
      new DeclaredElementTypeFilter(ResolveErrorType.NOT_RESOLVED, CLRDeclaredElementType.PROPERTY);

    protected class ForLoopPostfixTemplateInfo : PostfixTemplateInfo
    {
      public ForLoopPostfixTemplateInfo(
        [NotNull] string text, [NotNull] PostfixExpressionContext expression, [CanBeNull] string lengthPropertyName)
        : base(text, expression)
      {
        LengthPropertyName = lengthPropertyName;
      }

      [CanBeNull] public string LengthPropertyName { get; private set; }
    }

    PostfixTemplateBehavior IPostfixTemplate<CSharpPostfixTemplateContext>.CreateBehavior(PostfixTemplateInfo info)
    {
      return CreateBehavior((ForLoopPostfixTemplateInfo) info);
    }

    [NotNull] protected abstract PostfixTemplateBehavior CreateBehavior([NotNull] ForLoopPostfixTemplateInfo info);

    protected abstract class CSharpForLoopStatementBehaviorBase : CSharpStatementPostfixTemplateBehavior<IForStatement>
    {
      [CanBeNull] private readonly string myLengthName;

      protected CSharpForLoopStatementBehaviorBase([NotNull] ForLoopPostfixTemplateInfo info) : base(info)
      {
        myLengthName = info.LengthPropertyName;
      }

      [CanBeNull] protected string LengthName { get { return myLengthName; } }

      protected override void AfterComplete(ITextControl textControl, IForStatement statement)
      {
        var variableNames = SuggestIteratorVariableNames(statement);

        var newStatement = PutStatementCaret(textControl, statement);
        if (newStatement == null) return;

        var condition = (IRelationalExpression) newStatement.Condition;
        var variable = (ILocalVariableDeclaration) newStatement.Initializer.Declaration.Declarators[0];
        var iterator = (IPostfixOperatorExpression) newStatement.Iterators.Expressions[0];

        var nameField = new TemplateField("name", new NameSuggestionsExpression(variableNames), 0);
        var variableNameInfo = new HotspotInfo(
          nameField, variable.NameIdentifier.GetDocumentRange(), condition.LeftOperand.GetDocumentRange(), iterator.Operand.GetDocumentRange());

        var endRange = new TextRange(textControl.Caret.Offset());
        var liveTemplatesManager = Info.ExecutionContext.LiveTemplatesManager;
        var session = liveTemplatesManager.CreateHotspotSessionAtopExistingText(
          newStatement.GetSolution(), endRange, textControl, LiveTemplatesManager.EscapeAction.LeaveTextAndCaret, variableNameInfo);

        session.Execute();
      }

      [NotNull]
      private static IList<string> SuggestIteratorVariableNames([NotNull] IForStatement statement)
      {
        var declarationMember = statement.Initializer.Declaration.Declarators[0];
        var iteratorDeclaration = (ILocalVariableDeclaration) declarationMember;
        var namingManager = statement.GetPsiServices().Naming;

        var policyProvider = namingManager.Policy.GetPolicyProvider(iteratorDeclaration.Language, iteratorDeclaration.GetSourceFile());
        var collection = namingManager.Suggestion.CreateEmptyCollection(PluralityKinds.Single, iteratorDeclaration.Language, true, policyProvider);

        var variableType = iteratorDeclaration.DeclaredElement.Type;
        if (variableType.IsResolved)
        {
          collection.Add(variableType, new EntryOptions {
            PluralityKind = PluralityKinds.Single,
            SubrootPolicy = SubrootPolicy.Decompose
          });
        }

        var suggestionOptions = new SuggestionOptions { UniqueNameContext = statement, DefaultName = "i" };
        collection.Prepare(iteratorDeclaration.DeclaredElement, suggestionOptions);

        return collection.AllNames();
      }
    }
  }
}