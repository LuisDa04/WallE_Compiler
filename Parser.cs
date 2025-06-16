using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            Error.SetError("SYNTAX", $"Line {Current.line}: Error en el analisis de la cadena");

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
                Error.SetError("SYNTAX", $"Line {Current.line}: El programa debe comenzar con Spawn");
            
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
            return new ProgramNode(instructions, Current.Line);
        }
        
        InstructionNode ParseInstruction()
        {

            if (Current.token == TokenType.Identificador && LookAhead(1).token == TokenType.NewLine)
            {
                var labelToken = NextToken();

                NextToken();

                if (!labelsTable.TryAdd(labelToken.Text, labelToken.Line))
                    Error.SetError("SYNTAX", $"Line {labelToken.line}: La etiqueta '{labelToken.Text}' ya esta definida");
                
                return new LabelNode(labelToken.Text, labelToken.Line);
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
                        Error.SetError("SYNTAX", $"Line {Current.line}: Instruccion desconocida: {Current.Text}");

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

            if (!labelsTable.ContainsKey(labelToken.Text))
                Error.SetError("SEMANTIC", $"Line {Current.Line}: La etiqueta {labelToken.Text} no existe en el contexto actual");
            

            Match(TokenType.CorcheteCierra, "Se esperaba ']'");

            Match(TokenType.ParentesisAbre, "Se esperaba '(' tras el label");

            var condition = ParseExpression();

            Match(TokenType.ParentesisCierra, "Se esperaba ')'");

            return new GoToNode(labelToken.Text, condition, labelToken.Line);

        }


        InstructionNode ParseFill()
        {
            Match(TokenType.Fill, "Se esperaba 'Fill' ");

            if (Current.token == TokenType.ParentesisAbre)
            {
                var args = ParseParameters();
                if (args.Count > 0)
                    Error.SetError("SYNTAX", $"Line {Current.Line}: Fill no recibe parámetros");
            }

            else
                Error.SetError("SYNTAX", $"Line {Current.Line}: Se esperaba () despues de Fill");

            return new FillNode(Current.Line);
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
                    Error.SetError("SYNTAX", $"Line {opToken.Line}: Operador binario inesperado: '{opToken.Text}'");

                    return new InvalidExpressionNode($"Operador inesperado: '{opToken.Text}'", opToken.Line);
                }

                var right = ParseBinaryExpression(precedence + 1);

                var op = MapToBinaryOperator(opToken.Kind);

                left = new BinaryExpressionNode(left, op, right, opToken.Line);
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
                var opKind = (Current.token == TokenType.MinusToken)? UnaryOperator.Minus: UnaryOperator.Not;

                NextToken();

                var operand = ParseUnaryOrPrimary();

                if (operand is InvalidExpressionNode)
                    return new InvalidExpressionNode("Error en operador unario", opToken.Line);

                return new UnaryExpressionNode(opKind, operand, opToken.Line);
            }

            return ParsePrimary();
        }

        private ExpressionNode ParsePrimary()
        {
            if (Current.token == TokenType.Numero)
            {
                var text = Current.Text;

                var token = Current;

                NextToken();

                if (int.TryParse(text, out int value))
                    return new LiteralNode(value, Current.Line);

                Error.SetError("SYNTAX", $"Line {Current.Line}: Número inválido '{text}'");

                return new InvalidExpressionNode($"Número inválido '{text}'", Current.Line);
            }

            if (Current.token == TokenType.String)
            {
                var raw = Current.Text;

                var token = Current;

                NextToken();

                var str = raw[1..^1];

                return new LiteralNode(str, Current.Line);
            }

            if (Current.token == TokenType.TrueTok || Current.token == TokenType.FalseTok)
            {
                bool val = Current.token == TokenType.TrueTok;

                var token = Current;

                NextToken();

                return new LiteralNode(val, Current.Line);
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
                var name = Current.Text;

                var token = Current;

                NextToken();

                if (Current.token == TokenType.ParentesisAbre)
                {
                    var args = ParseParameters();  

                    if (Enum.TryParse<FunctionKind>(name, out var kind))
                        return new BuiltInFunctionNode(kind, args, token.Line);

                    Error.SetError("SYNTAX", $"Line {token.Line}: Función desconocida '{name}'");
                    return new InvalidExpressionNode($"Función desconocida '{name}'", token.Line);
                }

                return new VariableNode(name, Current.Line);
            }

            if (Current.Kind == TokenType.ParentesisAbre)
            {
                var token = Current;

                NextToken();

                var expr = ParseExpression();

                Match(TokenType.ParentesisCierra, "Se esperaba ')'");

                return expr;
            }

            var wrong = Current;

            Error.SetError("SYNTAX", $"Line {wrong.Line}: Se esperaba expresión primaria, encontró '{wrong.Text}'");

            NextToken();

            return new InvalidExpressionNode($"Token inesperado '{wrong.Text}'", wrong.Line);
        }

        InstructionNode ParseAssignment()
        {
            var idToken = Match(TokenType.Identificador, "Se esperaba identifier");

            Match(TokenType.Asignacion, "Se esperaba '<-'");

            var expr = ParseExpression();

            return new AssignmentNode(idToken.Text, expr, idToken.Line);
        }

        InstructionNode ParseSpawn()
        {
            Match(TokenType.Spawn, "Se esperaba un 'Spawn' ");

            var parameters = ParseParameters();

            if (parameters.Count != 2)
                Error.SetError("SYNTAX", $"Line {Current.Line}: Spawn requiere solo 2 parametros");


            var xExpr = parameters.ElementAtOrDefault(0) ?? new LiteralNode(0, Current.Line);

            var yExpr = parameters.ElementAtOrDefault(1) ?? new LiteralNode(0, Current.Line);

            return new SpawnNode(xExpr, yExpr, Current.Line);
        }

        InstructionNode ParseColor()
        {
            Match(TokenType.Color, "Se esperaba 'Color' ");

            var parameters = ParseParameters();

            var expression = parameters.FirstOrDefault() ?? new LiteralNode("Transparent",Current.Line);

            return new ColorNode(expression, expression.Line);
        }

        InstructionNode ParseSize()
        {
            Match(TokenType.SizeKeyword, "Se esperaba 'Size' ");

            var parameters = ParseParameters();

            if (parameters.Count != 1)
                ErrorsCollecter.Add("SYNTAX", "Size solo requiere un parametro", Current.Line);

            var sizeArg = parameters.ElementAtOrDefault(0) ?? new LiteralNode(1, Current.Line);

            return new SizeNode(sizeArg, Current.Line);
        }

        InstructionNode ParseDrawLine()
        {
            Match(TokenType.DrawLineKeyword, "Se esperaba 'DrawLine'");

            var parameters = ParseParameters();

            if (parameters.Count != 3)
                ErrorsCollecter.Add("SYNTAX", "DrawLine requiere 3 parámetros", Current.Line);

            var firstArg = parameters.ElementAtOrDefault(0) ?? new LiteralNode(0, Current.Line);

            var secondArg = parameters.ElementAtOrDefault(1) ?? new LiteralNode(0, Current.Line);

            var thirdArg = parameters.ElementAtOrDefault(2) ?? new LiteralNode(0, Current.Line);

            return new DrawLineNode(firstArg, secondArg, thirdArg, Current.Line);            
        }

        InstructionNode ParseDrawCircle()
        {
            Match(TokenType.DrawCircleKeyword, "Se esperaba 'DrawCircle'");

            var parameters = ParseParameters();

            if (parameters.Count != 3)
                ErrorsCollecter.Add("SYNTAX", "DrawCircle requiere 3 parámetros", Current.Line);

            var firstArg = parameters.ElementAtOrDefault(0) ?? new LiteralNode(0, Current.Line);

            var secondArg = parameters.ElementAtOrDefault(1) ?? new LiteralNode(0, Current.Line);

            var thirdArg = parameters.ElementAtOrDefault(2) ?? new LiteralNode(0, Current.Line);

            return new DrawCircleNode(firstArg, secondArg, thirdArg, Current.Line);
        }

        InstructionNode ParseDrawRectangle()
        {
            Match(TokenType.DrawRectangleKeyword, "Se esperaba 'DrawRectangle'");

            var parameters = ParseParameters();

            if (parameters.Count != 5)
                ErrorsCollecter.Add("SYNTAX", "DrawRectangle solo requiere 5 parámetros", Current.Line);

            var dirX  = parameters.ElementAtOrDefault(0) ?? new LiteralNode(0, Current.Line);
            var dirY  = parameters.ElementAtOrDefault(1) ?? new LiteralNode(0, Current.Line);
            var dist  = parameters.ElementAtOrDefault(2) ?? new LiteralNode(0, Current.Line);
            var width = parameters.ElementAtOrDefault(3) ?? new LiteralNode(1, Current.Line);
            var height= parameters.ElementAtOrDefault(4) ?? new LiteralNode(1, Current.Line);
            
            return new DrawRectangleNode(dirX, dirY, dist, width, height, Current.Line);
        }


        BinaryOperator MapToBinaryOperator(TokenType TokenType)
        {
            return TokenType switch
            {
                TokenType.PlusToken => BinaryOperator.Plus,

                TokenType.MinusToken => BinaryOperator.Minus,

                TokenType.MultToken => BinaryOperator.Mult,

                TokenType.SlashToken => BinaryOperator.Slash,

                TokenType.ModToken => BinaryOperator.Mod,

                TokenType.LessToken => BinaryOperator.LessThan,

                TokenType.LessOrEqualToken => BinaryOperator.LessThanOrEqual,

                TokenType.GreaterToken => BinaryOperator.GreaterThan,

                TokenType.GreaterOrEqualToken => BinaryOperator.GreaterThanOrEqual,

                TokenType.EqualToken => BinaryOperator.Equal,

                TokenType.AndAndToken => BinaryOperator.AndAnd,

                TokenType.OrOrToken => BinaryOperator.OrOr,

                TokenType.PowToken => BinaryOperator.Pow,

                _ => throw new InvalidOperationException($"Operador binario inesperado: {TokenType}")
            };
        }




    }
}