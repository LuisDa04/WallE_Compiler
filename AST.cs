using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE
{
    public class AST
    {
        public abstract class ASTNode
        {
            public int line {get; }

            public ASTNode(int line)
            {
                this.line = line;
            }
        }

        public abstract class InstructionNode: ASTNode, ISemanticNode
        {
            protected InstructionNode(int line) : baswe(line) {}

            public abstract void  Validate(SemanticContext context)
        }

        public abstract class ExpressionNode : ASTNode, ISemanticNode
        {
            protected ExpressionNode(int line) : base(line) {}

            public abstract string CheckType(SemanticContext semanticContext);

            public void Validate(SemanticContext context)
            {
                CheckType(context);
            }
        }

        
    }
}