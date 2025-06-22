using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE
{
    public interface ISemanticNode
    {
        void Validate(SemanticContext context);
    }

    public class SemanticContext
    {
        public Dictionary<string, string> VariablesTable { get; } = new Dictionary<string, string>();

        public Dictionary<string, int> LabelsTable { get; } = new Dictionary<string, int>();

        public string[]ColorsTable { get; } = new string[]
        {
            "Red", "Green", "Blue", "Yellow", "Black", "White", "Orange", "Purple", "Transparent"
        };

        public List<SemanticError> Errors { get; } = new List<SemanticError>();

        public void GetErrors(string message, int line) => Errors.Add(new SemanticError(message, line));
    }

    public class SemanticError
    {
        public string Message { get; }
        public int Line { get; }

        public SemanticError(string message, int line)
        {
            Message = message;

            Line = line;
        }

        public override string ToString() => $"[linea : {Line} {Message}]";
    }

    public static class SemanticHelp
    {
        public static void CheckParams(SemanticContext semanticContext, string instruccionName, string expType, params (string name, ExpressionNode expressionNode)[] parameters)
        {
            foreach (var item in parameters)
            {
                var actualType = item.expressionNode.CheckType(semanticContext);

                if (actualType != expType)
                    semanticContext.GetErrors($"'{instruccionName}': el parametro '{item.name}', debe ser '{expType}' y no '{actualType}'.", item.expressionNode.Line);
            }
        }
    }
}