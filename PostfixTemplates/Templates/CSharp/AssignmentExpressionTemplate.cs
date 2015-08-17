using JetBrains.Annotations;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Hotspots;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.LiveTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Templates;
using JetBrains.ReSharper.PostfixTemplates.Contexts.CSharp;
using JetBrains.ReSharper.PostfixTemplates.LookupItems;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;

namespace JetBrains.ReSharper.PostfixTemplates.Templates.CSharp
{
  [PostfixTemplate(
    templateName: "to",
    description: "Assigns current expression to some variable",
    example: "lvalue = expr;")]
  public class AssignmentExpressionTemplate : IPostfixTemplate<CSharpPostfixTemplateContext>
  {
    public void PopulateTemplates(CSharpPostfixTemplateContext context, IPostfixTemplatesCollector collector)
    {
      if (context.IsAutoCompletion) return;

      var outerExpression = context.OuterExpression;
      if (outerExpression == null || !outerExpression.CanBeStatement) return;

      for (ITreeNode node = outerExpression.Expression;;)
      {
        var assignmentExpression = node.GetContainingNode<IAssignmentExpression>();
        if (assignmentExpression == null) break;

        // disable 'here.to = "abc";'
        if (assignmentExpression.Dest.Contains(node)) return;

        node = assignmentExpression;
      }

      collector.Consume(new AssignmentItem(outerExpression));
    }

    private class AssignmentItem : StatementPostfixLookupItem<IExpressionStatement>
    {
      [NotNull] private readonly LiveTemplatesManager myTemplatesManager;

      public AssignmentItem([NotNull] CSharpPostfixExpressionContext context) : base("to", context)
      {
        var executionContext = context.PostfixContext.ExecutionContext;
        myTemplatesManager = executionContext.LiveTemplatesManager;
      }

      protected override IExpressionStatement CreateStatement(CSharpElementFactory factory, ICSharpExpression expression)
      {
        return (IExpressionStatement) factory.CreateStatement("target = $0;", expression);
      }

      protected override void AfterComplete(ITextControl textControl, IExpressionStatement statement)
      {
        var expression = (IAssignmentExpression) statement.Expression;
        var templateField = new TemplateField("target", 0);
        var hotspotInfo = new HotspotInfo(templateField, expression.Dest.GetDocumentRange());

        var endRange = statement.GetDocumentRange().EndOffsetRange().TextRange;
        var session = myTemplatesManager.CreateHotspotSessionAtopExistingText(
          statement.GetSolution(), endRange, textControl,
          LiveTemplatesManager.EscapeAction.RestoreToOriginalText, hotspotInfo);

        session.Execute();
      }
    }
  }
}