using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AST = WallE.AST;
using InstructionNode = WallE.AST.InstructionNode;
using ExpressionNode = WallE.AST.ExpressionNode;
using ProgramNode = WallE.AST.ProgramNode;
using UnaryOperator = WallE.AST.UnaryOperator;
using FunctionKind = WallE.AST.FunctionKind;
using LabelNode = WallE.AST.LabelNode;
using GoToNode = WallE.AST.GoToNode;
using BinaryExpressionNode = WallE.AST.BinaryExpressionNode;
using LiteralNode = WallE.AST.LiteralNode;
using DrawCircleNode = WallE.AST.DrawCircleNode;
using DrawLineNode = WallE.AST.DrawLineNode;
using DrawRectangleNode = WallE.AST.DrawRectangleNode;
using SizeNode = WallE.AST.SizeNode;
using InvalidExpressionNode = WallE.AST.InvalidExpressionNode;
using UnaryExpressionNode = WallE.AST.UnaryExpressionNode;
using BuiltInFunctionNode = WallE.AST.BuiltInFunctionNode;
using VariableNode = WallE.AST.VariableNode;
using BinaryOperator = WallE.AST.BinaryOperator;
using AssignmentNode = WallE.AST.AssignmentNode;
using SpawnNode = WallE.AST.SpawnNode;
using ColorNode = WallE.AST.ColorNode;
using FillNode = WallE.AST.FillNode;


namespace WallE
{
    public class Parser
    {

        public IReadOnlyDictionary<string, int> labelsOfParse => labelsTable;
        List<Token> Tokens;
        Dictionary<string, int> labelsTable = new Dictionary<string, int>();
        int Position;
        Token Current => LookAhead(0);
        bool IsAtEnd => Current.token == TokenType.EOF;

        public Parser(IEnumerable<Token> tokens)
        {
            Tokens = tokens.ToList();

            Position = 0;
        }

        Token LookAhead(int advance)
        {
            int index = Position + advance;

            if (index < Tokens.Count)
                return Tokens[index];

            return Tokens.Last();
        }

        Token NextToken()
        {
            var tok = Current;
            Position++;
            return tok;
        }

        Token Match(TokenType kind, string errorMsg)
        {
            if (Current.token == kind)
                return NextToken();

            Error.SetError("SYNTAX", "Error en el analisis de la cadena", Current.line);

            var wrong = Current;

            while (!IsAtEnd && Current.token != TokenType.NewLine)
                NextToken();

            return wrong;
        }

        public ProgramNode ParseProgram()
        {
            var instructions = new List<InstructionNode>();

            while (Current.token == TokenType.WhiteEspace)
                NextToken();
                
            if (Current.token != TokenType.Spawn)
                Error.SetError("SYNTAX", "El programa debe comenzar con Spawn", Current.line);
            
            else
                instructions.Add(ParseSpawn());

            while (!IsAtEnd)
            {
                if (Current.token == TokenType.NewLine)
                {
                    NextToken();
                    continue;
                }

                var instr = ParseInstruction();

                if (instr != null)
                    instructions.Add(instr);
            }
            return new ProgramNode(instructions, Current.line);
        }
        
        InstructionNode ParseInstruction()
        {

            if (Current.token == TokenType.Identificador && LookAhead(1).token == TokenType.NewLine)
            {
                var labelToken = NextToken();

                NextToken();

                if (!labelsTable.TryAdd(labelToken.text, labelToken.line))
                    Error.SetError("SYNTAX", $"La etiqueta '{labelToken.text}' ya esta definida", labelToken.line);
                
                return new LabelNode(labelToken.text, labelToken.line);
            }

            switch (Current.token)
                {
                    case TokenType.Spawn: return ParseSpawn();
                    case TokenType.Color: return ParseColor();
                    case TokenType.Size: return ParseSize();
                    case TokenType.DrawLine: return ParseDrawLine();
                    case TokenType.DrawCircle: return ParseDrawCircle();
                    case TokenType.DrawRectangle: return ParseDrawRectangle();
                    case TokenType.Fill: return ParseFill();
                    case TokenType.Identificador when LookAhead(1).token == TokenType.Asignacion: return ParseAssignment();
                    case TokenType.GoTo: return ParseGoTo();


                    default:
                        Error.SetError("SYNTAX", $"Instruccion desconocida: {Current.text}", Current.line);

                        while (!IsAtEnd && Current.token != TokenType.NewLine)
                            NextToken();

                        return null!;
                }
        }


        List<ExpressionNode> ParseParameters()
        {
            Match(TokenType.ParentesisAbre, "Se esperaba un '(' ");

            List<ExpressionNode> args = new List<ExpressionNode>();

            if (Current.token != TokenType.ParentesisCierra)
            {
                args.Add(ParseExpression());

                while (Current.token == TokenType.Coma)
                {
                    NextToken(); 
                    args.Add(ParseExpression());
                }
            }

            Match(TokenType.ParentesisCierra, "Se esperaba un ')' antes de finalizar ");

            return args;
        }
        
        private InstructionNode ParseGoTo()
        {
            Match(TokenType.GoTo, "Se esperaba 'GoTo'");

            Match(TokenType.CorcheteAbre, "Se esperaba '['");

            var labelToken = Match(TokenType.Identificador, "Se esperaba un identifier dentro de GoTo");

            if (!labelsTable.ContainsKey(labelToken.text))
                Error.SetError("SEMANTIC", $"La etiqueta {labelToken.text} no existe en el contexto actual", Current.line);
            

            Match(TokenType.CorcheteCierra, "Se esperaba ']'");

            Match(TokenType.ParentesisAbre, "Se esperaba '(' tras el label");

            var condition = ParseExpression();

            Match(TokenType.ParentesisCierra, "Se esperaba ')'");

            return new GoToNode(labelToken.text, condition, labelToken.line);

        }


        InstructionNode ParseFill()
        {
            Match(TokenType.Fill, "Se esperaba 'Fill' ");

            if (Current.token == TokenType.ParentesisAbre)
            {
                var args = ParseParameters();
                if (args.Count > 0)
                    Error.SetError("SYNTAX", "Fill no recibe parámetros", Current.line);
            }

            else
                Error.SetError("SYNTAX", "Se esperaba () despues de Fill", Current.line);

            return new FillNode(Current.line);
        }

        ExpressionNode ParseExpression(int parentPrecedence = 0)
        {
            return ParseBinaryExpression(parentPrecedence);
        }

        ExpressionNode ParseBinaryExpression(int parentPrecedence)
        {
            ExpressionNode left = ParseUnaryOrPrimary();

            while (true)
            {
                int precedence = Current.token.GetBinaryOperatorPrecedence();

                if (precedence == 0 || precedence < parentPrecedence)
                    break;

                var opToken = Current;

                NextToken();

                if (!EsOperadorBinarioValido(opToken.token))
                {
                    Error.SetError("SYNTAX", $"Operador binario inesperado: '{opToken.text}'", opToken.line);

                    return new InvalidExpressionNode($"Operador inesperado: '{opToken.text}'", opToken.line);
                }

                var right = ParseBinaryExpression(precedence + 1);

                var op = MapToBinaryOperator(opToken.token);

                left = new BinaryExpressionNode(left, op, right, opToken.line);
            }

            return left;
        }

        bool EsOperadorBinarioValido(TokenType type)
        {
            return type == TokenType.Suma
                || type == TokenType.Resta
                || type == TokenType.Multiplicacion
                || type == TokenType.Division
                || type == TokenType.Modulo
                || type == TokenType.MenorQue
                || type == TokenType.MenorIgualQue
                || type == TokenType.MayorQue
                || type == TokenType.MayorIgualQue
                || type == TokenType.IgualQue
                || type == TokenType.And
                || type == TokenType.Or
                || type == TokenType.Potencia;
        }

        ExpressionNode ParseUnaryOrPrimary()
        {
            var unaryPrec = Current.token.GetUnaryOperatorPrecedence();

            if (unaryPrec > 0)
            {
                var opToken = Current;
                var opKind = (Current.token == TokenType.Resta)? UnaryOperator.Resta: UnaryOperator.Not;

                NextToken();

                var operand = ParseUnaryOrPrimary();

                if (operand is InvalidExpressionNode)
                    return new InvalidExpressionNode("Error en operador unario", opToken.line);

                return new UnaryExpressionNode(opKind, operand, opToken.line);
            }

            return ParsePrimary();
        }

        private ExpressionNode ParsePrimary()
        {
            if (Current.token == TokenType.Numero)
            {
                var text = Current.text;

                var token = Current;

                NextToken();

                if (int.TryParse(text, out int value))
                    return new LiteralNode(value, Current.line);

                Error.SetError("SYNTAX", $"Número inválido '{text}'", Current.line);

                return new InvalidExpressionNode($"Número inválido '{text}'", Current.line);
            }

            if (Current.token == TokenType.String)
            {
                var raw = Current.text;

                var token = Current;

                NextToken();

                var str = raw[1..^1];

                return new LiteralNode(str, Current.line);
            }

            if (Current.token == TokenType.TrueTok || Current.token == TokenType.FalseTok)
            {
                bool val = Current.token == TokenType.TrueTok;

                var token = Current;

                NextToken();

                return new LiteralNode(val, Current.line);
            }

            if
            (
                Current.token == TokenType.Identificador
                || Current.token == TokenType.GetActualX
                || Current.token == TokenType.GetActualY
                || Current.token == TokenType.GetCanvasSize
                || Current.token == TokenType.GetColorCount
                || Current.token == TokenType.IsBrushColor
                || Current.token == TokenType.IsBrushSize
                || Current.token == TokenType.IsCanvasColor
            )
            {
                var name = Current.text;

                var token = Current;

                NextToken();

                if (Current.token == TokenType.ParentesisAbre)
                {
                    var args = ParseParameters();  

                    if (Enum.TryParse<FunctionKind>(name, out var kind))
                        return new BuiltInFunctionNode(kind, args, token.line);

                    Error.SetError("SYNTAX", $"Función desconocida '{name}'", token.line);
                    return new InvalidExpressionNode($"Función desconocida '{name}'", token.line);
                }

                return new VariableNode(name, Current.line);
            }

            if (Current.token == TokenType.ParentesisAbre)
            {
                var token = Current;

                NextToken();

                var expr = ParseExpression();

                Match(TokenType.ParentesisCierra, "Se esperaba ')'");

                return expr;
            }

            var wrong = Current;

            Error.SetError("SYNTAX", $"Se esperaba expresión primaria, encontró '{wrong.text}'", wrong.line);

            NextToken();

            return new InvalidExpressionNode($"Token inesperado '{wrong.text}'", wrong.line);
        }

        InstructionNode ParseAssignment()
        {
            var idToken = Match(TokenType.Identificador, "Se esperaba identifier");

            Match(TokenType.Asignacion, "Se esperaba '<-'");

            var expr = ParseExpression();

            return new AssignmentNode(idToken.text, expr, idToken.line);
        }

        InstructionNode ParseSpawn()
        {
            Match(TokenType.Spawn, "Se esperaba un 'Spawn' ");

            var parameters = ParseParameters();

            if (parameters.Count != 2)
                Error.SetError("SYNTAX", "Spawn requiere solo 2 parametros", Current.line);


            var xExpr = parameters.ElementAtOrDefault(0) ?? new LiteralNode(0, Current.line);

            var yExpr = parameters.ElementAtOrDefault(1) ?? new LiteralNode(0, Current.line);

            return new SpawnNode(xExpr, yExpr, Current.line);
        }

        InstructionNode ParseColor()
        {
            Match(TokenType.Color, "Se esperaba 'Color' ");

            var parameters = ParseParameters();

            var expression = parameters.FirstOrDefault() ?? new LiteralNode("Transparent",Current.line);

            return new ColorNode(expression, expression.line);
        }

        InstructionNode ParseSize()
        {
            Match(TokenType.Size, "Se esperaba 'Size' ");

            var parameters = ParseParameters();

            if (parameters.Count != 1)
                Error.SetError("SYNTAX", "Size solo requiere un parametro", Current.line);

            var sizeArg = parameters.ElementAtOrDefault(0) ?? new LiteralNode(1, Current.line);

            return new SizeNode(sizeArg, Current.line);
        }

        InstructionNode ParseDrawLine()
        {
            Match(TokenType.DrawLine, "Se esperaba 'DrawLine'");

            var parameters = ParseParameters();

            if (parameters.Count != 3)
                Error.SetError("SYNTAX", "DrawLine requiere 3 parámetros", Current.line);

            var firstArg = parameters.ElementAtOrDefault(0) ?? new LiteralNode(0, Current.line);

            var secondArg = parameters.ElementAtOrDefault(1) ?? new LiteralNode(0, Current.line);

            var thirdArg = parameters.ElementAtOrDefault(2) ?? new LiteralNode(0, Current.line);

            return new DrawLineNode(firstArg, secondArg, thirdArg, Current.line);            
        }

        InstructionNode ParseDrawCircle()
        {
            Match(TokenType.DrawCircle, "Se esperaba 'DrawCircle'");

            var parameters = ParseParameters();

            if (parameters.Count != 3)
                Error.SetError("SYNTAX", "DrawCircle requiere 3 parámetros", Current.line);

            var firstArg = parameters.ElementAtOrDefault(0) ?? new LiteralNode(0, Current.line);

            var secondArg = parameters.ElementAtOrDefault(1) ?? new LiteralNode(0, Current.line);

            var thirdArg = parameters.ElementAtOrDefault(2) ?? new LiteralNode(0, Current.line);

            return new DrawCircleNode(firstArg, secondArg, thirdArg, Current.line);
        }

        InstructionNode ParseDrawRectangle()
        {
            Match(TokenType.DrawRectangle, "Se esperaba 'DrawRectangle'");

            var parameters = ParseParameters();

            if (parameters.Count != 5)
                Error.SetError("SYNTAX", "DrawRectangle solo requiere 5 parámetros", Current.line);

            var dirX  = parameters.ElementAtOrDefault(0) ?? new LiteralNode(0, Current.line);
            var dirY  = parameters.ElementAtOrDefault(1) ?? new LiteralNode(0, Current.line);
            var dist  = parameters.ElementAtOrDefault(2) ?? new LiteralNode(0, Current.line);
            var width = parameters.ElementAtOrDefault(3) ?? new LiteralNode(1, Current.line);
            var height= parameters.ElementAtOrDefault(4) ?? new LiteralNode(1, Current.line);
            
            return new DrawRectangleNode(dirX, dirY, dist, width, height, Current.line);
        }


        BinaryOperator MapToBinaryOperator(TokenType tokenType)
        {
            return tokenType switch
            {
                TokenType.Suma => BinaryOperator.Suma,

                TokenType.Resta => BinaryOperator.Resta,

                TokenType.Multiplicacion => BinaryOperator.Multiplicacion,

                TokenType.Division => BinaryOperator.Division,

                TokenType.Modulo => BinaryOperator.Modulo,

                TokenType.MenorQue => BinaryOperator.MenorQue,

                TokenType.MenorIgualQue => BinaryOperator.MenorIgualQue,

                TokenType.MayorQue => BinaryOperator.MayorQue,

                TokenType.MayorIgualQue => BinaryOperator.MayorIgualQue,

                TokenType.IgualQue => BinaryOperator.IgualQue,

                TokenType.And => BinaryOperator.And,

                TokenType.Or => BinaryOperator.Or,

                TokenType.Potencia => BinaryOperator.Potencia,

                _ => throw new InvalidOperationException($"Operador binario inesperado: {tokenType}")
            };
        }




    }
}