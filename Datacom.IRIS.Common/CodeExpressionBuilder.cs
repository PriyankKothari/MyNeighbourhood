using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Compilation;
using System.Web.UI;

namespace Datacom.IRIS.Common
{
    [ExpressionPrefix("Code")]
    public class CodeExpressionBuilder : ExpressionBuilder
    {
        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            return new CodeSnippetExpression(entry.Expression);
        }
    }
}
