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
            protected InstructionNode(int line) : base(line) {}

            public abstract void  Validate(SemanticContext context);
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

        public class ProgramNode : ASTNode, ISemanticNode
        {
            public List<InstructionNode> Instructions { get; }

            public ProgramNode(IEnumerable<InstructionNode> instructions, int line) : base(line)
            {
                Instructions = instructions.ToList();
            }

            public void Validate(SemanticContext context)
            {
                var spawnCount = Instructions.Count(inst => inst is SpawnNode);

                foreach (var item in Instructions)
                {
                    if (item is AssignmentNode assignmentNode)
                        context.VariablesTable.TryAdd(assignmentNode.Identifier, "desconocido");
                }

                foreach (var item in Instructions)
                {
                    item.Validate(context);
                }
            }
        }

        public class BinaryExpressionNode : ExpressionNode
        {
            public ExpressionNode LeftExpressionNode { get; }
            public BinaryOperator Operator { get; }
            public ExpressionNode RightExpressionNode { get; }

            public BinaryExpressionNode(ExpressionNode leftExpressionNode, BinaryOperator op, ExpressionNode rightExpressionNode, int line) : base(line)
            {
                LeftExpressionNode = leftExpressionNode;

                Operator = op;

                RightExpressionNode = rightExpressionNode;
            }

            public override string CheckType(SemanticContext semanticContext)
            {
                string leftType = LeftExpressionNode.CheckType(semanticContext);

                string rightType = RightExpressionNode.CheckType(semanticContext);

                bool Both(string type) => leftType == type && rightType == type;

                switch (Operator)
                {
                    case BinaryOperator.Suma:

                    case BinaryOperator.Resta:

                    case BinaryOperator.Multiplicacion:

                    case BinaryOperator.Division:

                    case BinaryOperator.Modulo:

                    case BinaryOperator.Potencia:
                        if (!Both("int"))
                            return Error($"Operador '{Operator}' requiere enteros", semanticContext);
                        return "int";

                    case BinaryOperator.IgualQue:
                        if (leftType != rightType)
                            return Error($"'==' requiere tipos iguales, pero recibió {leftType} y {rightType}", semanticContext);
                        return "bool";

                    
                    case BinaryOperator.MenorQue:

                    case BinaryOperator.MenorIgualQue:

                    case BinaryOperator.MayorQue:

                    case BinaryOperator.MayorIgualQue:
                        if (!Both("int"))
                            return Error($"Operador '{Operator}' requiere enteros", semanticContext);
                        return "bool";

                    case BinaryOperator.And:

                    case BinaryOperator.Or:
                        if (!Both("bool"))
                            return Error($"Operador lógico '{Operator}' requiere booleanos", semanticContext);
                        return "bool";

                    default:
                        return Error($"Operador binario no soportado: {Operator}", semanticContext);
                }
            }

            private string Error(string message, SemanticContext semanticContext)
            {
                semanticContext.GetErrors(message, Line);
                return "desconocido";
            }

        }

        public class UnaryExpressionNode : ExpressionNode
        {
            public UnaryOperator Operator { get; }
            public ExpressionNode MiddleExpression { get; }

            public UnaryExpressionNode(UnaryOperator op, ExpressionNode middleExpression, int line) : base(line)
            {
                Operator = op;
                MiddleExpression = middleExpression;
            }

            public override string CheckType(SemanticContext semanticContext)
            {
                string type = MiddleExpression.CheckType(semanticContext);

                return Operator switch
                {
                    UnaryOperator.Resta => type == "int" ? "int" : Error("Operador '-' requiere tipo int ", semanticContext),
                    UnaryOperator.Not => type == "bool" ? "bool" : Error("Operador '!' requiere tipo bool ", semanticContext),
                    _ => Error("Operador unario desconocido", semanticContext)
                };
            }

            string Error(string message, SemanticContext semanticContext)
            {
                semanticContext.GetErrors(message, line);

                return "desconocido";
            }
        }

        public class LiteralNode : ExpressionNode
        {

            public object Value { get; }

            public LiteralNode(object value, int line) : base(line)
            {
                Value = value;
            }

            public override string CheckType(SemanticContext semanticContext) => Value switch
            {
                int => "int",
                bool => "bool",
                string => "string",
                _ => "desconocido"
            };
        }

        public class VariableNode : ExpressionNode
        {
            public string Name { get; }

            public VariableNode(string name, int line) : base(line)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Variable name cannot be null or whitespace.", nameof(name));

                Name = name;
            }

            public override string CheckType(SemanticContext semanticContext)
            {
                if (!semanticContext.VariablesTable.TryGetValue(Name, out var type))
                {
                    semanticContext.GetErrors($"Variable {Name} no declarada", line);

                    return "desconocido";
                }

                return type;
            }
        }

        public class BuiltInFunctionNode : ExpressionNode
        {
            public FunctionKind FunctionKind { get; }
            public List<ExpressionNode> Arguments { get; }

            public BuiltInFunctionNode(FunctionKind functionKind, IEnumerable<ExpressionNode> param, int line) : base(line)
            {
                FunctionKind = functionKind;

                Arguments = param.ToList();
            }

            public override string CheckType(SemanticContext semanticContext)
            {
                if
                (
                    FunctionKind == FunctionKind.GetActualX ||

                    FunctionKind == FunctionKind.GetActualY ||

                    FunctionKind == FunctionKind.GetCanvasSize
                )

                {
                    if (Arguments.Count != 0)
                        semanticContext.GetErrors($"'{FunctionKind}' no recibe argumentos", line);

                    return "int";
                }

                if (FunctionKind == FunctionKind.GetColorCount)
                {
                    if (Arguments.Count != 5)
                    {
                        semanticContext.GetErrors($"GetColorCount requiere 5 argumentos, pero se pasaron {Arguments.Count}", line);
                        return "desconocido";
                    }

                    if (Arguments[0].CheckType(semanticContext) != "string")
                        semanticContext.GetErrors("GetColorCount: primer argumento debe ser string", line);


                    for (int i = 1; i < 5; i++)
                        if (Arguments[i].CheckType(semanticContext) != "int")
                            semanticContext.GetErrors($"GetColorCount: argumento #{i + 1} debe ser int", line);

                    return "int";
                }


                if (FunctionKind == FunctionKind.IsBrushColor)
                {
                    if (Arguments.Count != 1)
                    {
                        semanticContext.GetErrors($"IsBrushColor requiere 1 argumento, pero se pasaron {Arguments.Count}", line);
                        return "desconocido";
                    }

                    if (Arguments[0].CheckType(semanticContext) != "string")
                        semanticContext.GetErrors("IsBrushColor: argumento debe ser string", line);

                    return "int";
                }


                if (FunctionKind == FunctionKind.IsBrushSize)
                {
                    if (Arguments.Count != 1)
                    {
                        semanticContext.GetErrors($"IsBrushSize requiere 1 argumento, pero se pasaron {Arguments.Count}", line);
                        return "desconocido";
                    }
                    if (Arguments[0].CheckType(semanticContext) != "int")
                        semanticContext.GetErrors("IsBrushSize: argumento debe ser int", line);

                    return "int";
                }


                if (FunctionKind == FunctionKind.IsCanvasColor)
                {
                    if (Arguments.Count != 3)
                    {
                        semanticContext.GetErrors($"IsCanvasColor requiere 3 argumentos, pero se pasaron {Arguments.Count}", line);
                        return "desconocido";
                    }

                    if (Arguments[0].CheckType(semanticContext) != "string")
                        semanticContext.GetErrors("IsCanvasColor: primer argumento debe ser string", line);

                    if (Arguments[1].CheckType(semanticContext) != "int")
                        semanticContext.GetErrors("IsCanvasColor: segundo argumento debe ser int", line);

                    if (Arguments[2].CheckType(semanticContext) != "int")
                        semanticContext.GetErrors("IsCanvasColor: tercer argumento debe ser int", line);

                    return "int";
                }


                semanticContext.GetErrors($"Función desconocida '{FunctionKind}'", line);

                return "desconocido";
            }
        }

        public class InvalidExpressionNode : ExpressionNode
        {
            public string Why { get; }

            public InvalidExpressionNode(string why, int line) : base(line)
            {
                Why = why;
            }

            public override string CheckType(SemanticContext semanticContext)
            {
                semanticContext.GetErrors($"Expresión inválida: {Why}", line);
                return "desconocido";
            }
        }

        public class SpawnNode : InstructionNode
        {
            public ExpressionNode XExpression { get; }
            public ExpressionNode YExpression { get; }

            public SpawnNode(ExpressionNode xExpression, ExpressionNode yExpression, int line) : base(line)
            {
                XExpression = xExpression;
                YExpression = yExpression;
            }

            public override void Validate(SemanticContext context)
            {
                SemanticHelp.CheckParams(context, "Spawn", "int",
                    ("X", XExpression),
                    ("Y", YExpression));
            }
        }

        public class ColorNode : InstructionNode
        {
            public ExpressionNode expression;

            public ColorNode(ExpressionNode expressionNode, int line) : base(line)
            {
                expression = expressionNode;
            }

            public override void Validate(SemanticContext context)
            {
                var tipo = expression.CheckType(context);

                if (tipo != "string")
                    context.GetErrors($"Color espera un literal string, recibió '{tipo}'", line);

                 if (expression is LiteralNode lit && lit.Value is string colName)
                {
                    if (!context.ColorsTable.Contains(colName))
                        context.GetErrors($"Color '{colName}' no declarado", line);
                }
            }
        }

        public class SizeNode : InstructionNode
        {
            public ExpressionNode SizeExpression { get; }
            public SizeNode(ExpressionNode sizeExpression, int line) : base(line)
            {
                SizeExpression = sizeExpression;
            }

            public override void Validate(SemanticContext context)
            {
                SemanticHelp.CheckParams(context, "Size", "int",
                    ("size", SizeExpression));
            }
        }

        public class DrawLineNode : InstructionNode
        {
            public ExpressionNode DirXExpression { get; }

            public ExpressionNode DirYExpression { get; }

            public ExpressionNode DistanceExpression { get; }

            public DrawLineNode(ExpressionNode dirXExpression, ExpressionNode dirYExpression, ExpressionNode distanceExpression, int line) : base(line)
            {
                DirXExpression = dirXExpression;
                DirYExpression = dirYExpression;
                DistanceExpression = distanceExpression;
            }

            public override void Validate(SemanticContext context)
            {
                SemanticHelp.CheckParams(context, "DrawLine", "int",
                    ("Coordenada X", DirXExpression),
                    ("Coordenada Y", DirYExpression),
                    ("Distancia", DistanceExpression));

                if (DirXExpression is LiteralNode litX && litX.Value is int corX && (corX > 1 || corX < -1))
                    context.GetErrors($"En DrawLine, el primer parametro debe ser 0, 1 o -1 ", line);

                if (DirYExpression is LiteralNode litY && litY.Value is int corY && (corY > 1 || corY < -1))
                    context.GetErrors($"En DrawLine, el segundo parametro debe ser 0, 1 o -1 ", line);

            }
        }

        public class DrawCircleNode : InstructionNode
        {
            public ExpressionNode DirXExpression { get; }
            public ExpressionNode DirYExpression { get; }
            public ExpressionNode RadiusExpression { get; }

            public DrawCircleNode(ExpressionNode dirXExpression, ExpressionNode dirYExpression, ExpressionNode radiusExpression, int line) : base(line)
            {
                DirXExpression = dirXExpression;

                DirYExpression = dirYExpression;

                RadiusExpression = radiusExpression;
            }

            public override void Validate(SemanticContext context)
            {
                SemanticHelp.CheckParams(context, "DrawCircle", "int",
                    ("Coordenada X", DirXExpression),
                    ("Coordenada Y", DirYExpression),
                    ("Radio", RadiusExpression));
            }
        }

        public class DrawRectangleNode : InstructionNode
        {
            public ExpressionNode DirXExpression { get; }
            public ExpressionNode DirYExpression { get; }
            public ExpressionNode DistanceExpression { get; }
            public ExpressionNode WidthExpression { get; }
            public ExpressionNode HeightExpression { get; }

            public DrawRectangleNode(ExpressionNode dirXExpression, ExpressionNode dirYExpression, ExpressionNode distanceExpression, ExpressionNode widthExpression, ExpressionNode heightExpression, int line) : base(line)
            {
                DirXExpression = dirXExpression;
                DirYExpression = dirYExpression;
                DistanceExpression = distanceExpression;
                WidthExpression = widthExpression;
                HeightExpression = heightExpression;
            }

            public override void Validate(SemanticContext context)
            {
                SemanticHelp.CheckParams(context, "DrawRectangle", "int",
                    ("Coordenada X", DirXExpression),
                    ("Coordenada Y", DirYExpression),
                    ("Distancia", DistanceExpression),
                    ("Ancho", WidthExpression),
                    ("Alto", HeightExpression));
            }
        }

        public class FillNode : InstructionNode
        {
            public FillNode(int line) : base(line) { }

            public override void Validate(SemanticContext context) { }
        }

 
        public class AssignmentNode : InstructionNode
        {
            public string Identifier { get; }
            public ExpressionNode Expression { get; }

            public AssignmentNode(string identifier, ExpressionNode expression, int line) : base((line))
            {
                if (string.IsNullOrEmpty(identifier))
                    throw new ArgumentException("Variable invalida");

                Identifier = identifier;

                Expression = expression;
            }

            public override void Validate(SemanticContext context)
            {
                string expressionType = Expression.CheckType(context);

                if (expressionType == "string")
                {
                    context.GetErrors($"Asignacion no valida, no es posible asignar valores de tipo string", line);
                    return;
                }

                if (context.VariablesTable.TryGetValue(Identifier, out var variableType) && variableType != "desconocido" && variableType != expressionType)
                    context.GetErrors($"Variable {Identifier} ya es de tipo {variableType}, no se puede asignar '{expressionType}' ", line);

                else
                    context.VariablesTable[Identifier] = expressionType;
            }
        }

        public class LabelNode : InstructionNode
        {
            public string LabelName { get; }

            public LabelNode(string label, int line) : base(line)
            {
                if (string.IsNullOrEmpty(label))
                    throw new ArgumentException("Label invalido");

                LabelName = label;
            }

            public override void Validate(SemanticContext context) { }
        }

        public class GoToNode : InstructionNode
        {
            public string Label { get; }
            public ExpressionNode Condition { get; }

            public GoToNode(string label, ExpressionNode condition, int line) : base(line)
            {
                if (string.IsNullOrEmpty(label))
                    throw new ArgumentException("Label invalido");

                if (condition == null)
                    throw new ArgumentNullException(nameof(condition), "Condition cannot be null");

                Label = label;
                Condition = condition;
            }

            public override void Validate(SemanticContext context)
            {
                if (!context.LabelsTable.ContainsKey(Label))
                {
                    context.GetErrors($"Label {Label} no declarado", line);
                }

                string conditionType = Condition.CheckType(context);

                if (conditionType != "bool")
                    context.GetErrors($"Condicion de tipo {conditionType}, se esperaba 'bool'", line);
            }
        }

        public enum BinaryOperator
        {
            Suma,
            Resta,
            Multiplicacion,
            Potencia,
            Division,
            Modulo,
            MenorQue,
            MayorQue,
            MenorIgualQue,
            MayorIgualQue,
            IgualQue,
            And,
            Or
        }

        public enum UnaryOperator
        {
            Resta,
            Not
        }

        public enum FunctionKind
        {
            GetActualX,
            GetActualY,
            GetCanvasSize,
            GetColorCount,
            IsBrushColor,
            IsBrushSize,
            IsCanvasColor
        }
    }
}