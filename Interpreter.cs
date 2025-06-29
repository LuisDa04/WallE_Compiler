using System;
using System.Collections.Generic;
using System.Linq;
using AST = WallE.AST;
using InstructionNode = WallE.AST.InstructionNode;
using ExpressionNode = WallE.AST.ExpressionNode;
using ProgramNode = WallE.AST.ProgramNode;
using InvalidExpressionNode = WallE.AST.InvalidExpressionNode;
using UnaryExpressionNode = WallE.AST.UnaryExpressionNode;
using BuiltInFunctionNode = WallE.AST.BuiltInFunctionNode;
using VariableNode = WallE.AST.VariableNode;
using BinaryOperator = WallE.AST.BinaryOperator;
using UnaryOperator = WallE.AST.UnaryOperator;
using AssignmentNode = WallE.AST.AssignmentNode;
using LabelNode = WallE.AST.LabelNode;
using GoToNode = WallE.AST.GoToNode;
using SpawnNode = WallE.AST.SpawnNode;
using ColorNode = WallE.AST.ColorNode;
using SizeNode = WallE.AST.SizeNode;
using DrawLineNode = WallE.AST.DrawLineNode;
using DrawCircleNode = WallE.AST.DrawCircleNode;
using DrawRectangleNode = WallE.AST.DrawRectangleNode;
using FillNode = WallE.AST.FillNode;
using FunctionKind = WallE.AST.FunctionKind;
using LiteralNode = WallE.AST.LiteralNode;
using BinaryExpressionNode = WallE.AST.BinaryExpressionNode;


namespace WallE
{
    public class Interpreter
    {
        private ProgramNode program;
        private Dictionary<string, int> labelsMapping = new Dictionary<string, int>();
        private Dictionary<string, object> environment = new Dictionary<string, object>();
        private Canvas canvas;
        

        private int currentX;
        private int currentY;
        private string brushColor = "Transparent";
        private int brushSize = 1;
        private Action<string> outputCallback;

        public Interpreter(ProgramNode program, int canvasSize, Action<string> outputCallback = null)
        {
            this.program = program;
            canvas = new Canvas(canvasSize);
            this.outputCallback = outputCallback;

            for (int i = 0; i < program.Instructions.Count; i++)
            {
                if (program.Instructions[i] is LabelNode labelNode)
                    labelsMapping[labelNode.LabelName] = i;
            }
        }

        public void Execute()
        {
            
            int pc = 0;
            while (pc < program.Instructions.Count)
            {
                InstructionNode instruction = program.Instructions[pc];
                int oldPc = pc;
                
                pc = ExecuteInstruction(instruction, pc);
                
                if (pc == oldPc)
                    pc++;
            }
        }

        private int ExecuteInstruction(InstructionNode instr, int pc)
        {
            if (instr is SpawnNode spawn)
            {
                int x = (int)Evaluate(spawn.XExpression);
                int y = (int)Evaluate(spawn.YExpression);
                
                if (x < 0 || x >= canvas.Size || y < 0 || y >= canvas.Size)
                {
                    Console.WriteLine($"Error de ejecución: Posición ({x},{y}) fuera del canvas.");
                    return pc + 1;
                }
                currentX = x;
                currentY = y;
                Console.WriteLine($"Wall-E posicionado en ({x},{y})");
            }
            else if (instr is ColorNode colorInstr)
            {
                var col = Evaluate(colorInstr.expression);
                if (col is string s)
                {
                    brushColor = s;
                    Console.WriteLine($"Cambio de color a {s}");
                }
            }
            else if (instr is SizeNode sizeInstr)
            {
                int size = (int)Evaluate(sizeInstr.SizeExpression);
                
                if (size % 2 == 0)
                    size = Math.Max(1, size - 1);
                brushSize = size;
                Console.WriteLine($"Cambio de tamaño a {brushSize}");
            }
            else if (instr is DrawLineNode drawLine)
            {
                int dirX = (int)Evaluate(drawLine.DirXExpression);
                int dirY = (int)Evaluate(drawLine.DirYExpression);
                int distance = (int)Evaluate(drawLine.DistanceExpression);
                int endX = currentX + dirX * distance;
                int endY = currentY + dirY * distance;
                DrawLine(currentX, currentY, endX, endY);
                
                currentX = endX;
                currentY = endY;
                Console.WriteLine($"Dibujó línea hasta ({endX},{endY})");
            }
            else if (instr is DrawCircleNode drawCircle)
            {
                int dirX = (int)Evaluate(drawCircle.DirXExpression);
                int dirY = (int)Evaluate(drawCircle.DirYExpression);
                int radius = (int)Evaluate(drawCircle.RadiusExpression);
                DrawCircle(currentX + dirX, currentY + dirY, radius);
                
                Console.WriteLine($"Dibujó círculo con centro en ({currentX + dirX},{currentY + dirY}) y radio {radius}");
            }
            else if (instr is DrawRectangleNode drawRect)
            {
                int dirX = (int)Evaluate(drawRect.DirXExpression);
                int dirY = (int)Evaluate(drawRect.DirYExpression);
                int distance = (int)Evaluate(drawRect.DistanceExpression);
                int width = (int)Evaluate(drawRect.WidthExpression);
                int height = (int)Evaluate(drawRect.HeightExpression);
                int centerX = currentX + dirX * distance;
                int centerY = currentY + dirY * distance;
                DrawRectangle(centerX, centerY, width, height);
                Console.WriteLine($"Dibujó rectángulo centrado en ({centerX},{centerY}), tamaño {width}x{height}");
            }
            else if (instr is FillNode)
            {
                Fill(currentX, currentY, brushColor);
                Console.WriteLine($"Realizó fill en ({currentX},{currentY}) con color {brushColor}");
            }
            else if (instr is AssignmentNode assign)
            {
                var value = Evaluate(assign.Expression);
                environment[assign.Identifier] = value;
                Console.WriteLine($"Asignó {assign.Identifier} = {value}");
            }
            else if (instr is GoToNode goTo)
            {
                var conditionVal = Evaluate(goTo.Condition);
                if (conditionVal is bool cond && cond)
                {
                    if (labelsMapping.TryGetValue(goTo.Label, out int target))
                    {
                        Console.WriteLine($"Saltando a la etiqueta {goTo.Label}");
                        return target;
                    }
                    else
                    {
                        Console.WriteLine($"Error de ejecución: La etiqueta {goTo.Label} no existe.");
                    }
                }
            }
            return pc;
        }

        
        private object Evaluate(ExpressionNode expr)
        {
            if (expr is LiteralNode literal)
                return literal.Value;
            else if (expr is VariableNode varNode)
            {
                if (environment.TryGetValue(varNode.Name, out object value))
                    return value;
                Console.WriteLine($"Error de ejecución: La variable {varNode.Name} no está definida.");
                return 0;
            }
            else if (expr is BinaryExpressionNode binExpr)
            {
                object left = Evaluate(binExpr.LeftExpressionNode);
                object right = Evaluate(binExpr.RightExpressionNode);
                switch (binExpr.Operator)
                {
                    case BinaryOperator.Suma:
                        return (int)left + (int)right;
                    case BinaryOperator.Resta:
                        return (int)left - (int)right;
                    case BinaryOperator.Multiplicacion:
                        return (int)left * (int)right;
                    case BinaryOperator.Division:
                        return (int)left / (int)right;
                    case BinaryOperator.Modulo:
                        return (int)left % (int)right;
                    case BinaryOperator.Potencia:
                        return (int)Math.Pow((int)left, (int)right);
                    case BinaryOperator.IgualQue:
                        return left.Equals(right);
                    case BinaryOperator.MenorQue:
                        return (int)left < (int)right;
                    case BinaryOperator.MayorQue:
                        return (int)left > (int)right;
                    case BinaryOperator.MenorIgualQue:
                        return (int)left <= (int)right;
                    case BinaryOperator.MayorIgualQue:
                        return (int)left >= (int)right;
                    case BinaryOperator.And:
                        return (bool)left && (bool)right;
                    case BinaryOperator.Or:
                        return (bool)left || (bool)right;
                    default:
                        Console.WriteLine($"Operador binario no soportado: {binExpr.Operator}");
                        return null;
                }
            }
            else if (expr is UnaryExpressionNode unExpr)
            {
                object operand = Evaluate(unExpr.MiddleExpression);
                switch (unExpr.Operator)
                {
                    case UnaryOperator.Resta:
                        return -(int)operand;
                    case UnaryOperator.Not:
                        return !(bool)operand;
                    default:
                        Console.WriteLine($"Operador unario no soportado: {unExpr.Operator}");
                        return null;
                }
            }
            else if (expr is BuiltInFunctionNode funcNode)
            {
                List<object> argValues = funcNode.Arguments.Select(Evaluate).ToList();
                switch (funcNode.FunctionKind)
                {
                    case FunctionKind.GetActualX:
                        return currentX;
                    case FunctionKind.GetActualY:
                        return currentY;
                    case FunctionKind.GetCanvasSize:
                        return canvas.Size;
                    case FunctionKind.GetColorCount:
                        
                        if (argValues.Count == 5)
                            return GetColorCount((string)argValues[0], (int)argValues[1], (int)argValues[2], (int)argValues[3], (int)argValues[4]);
                        break;
                    case FunctionKind.IsBrushColor:
                        if (argValues.Count == 1)
                            return brushColor == (string)argValues[0] ? 1 : 0;
                        break;
                    case FunctionKind.IsBrushSize:
                        if (argValues.Count == 1)
                            return brushSize == (int)argValues[0] ? 1 : 0;
                        break;
                    case FunctionKind.IsCanvasColor:
                        if (argValues.Count == 3)
                        {
                            int offsetX = (int)argValues[1];
                            int offsetY = (int)argValues[2];
                            int posX = currentX + offsetX;
                            int posY = currentY + offsetY;
                            return canvas.GetPixel(posX, posY) == (string)argValues[0] ? 1 : 0;
                        }
                        break;
                }
                Console.WriteLine($"Función integrada {funcNode.FunctionKind} no implementada o con argumentos incorrectos.");
                return 0;
            }
            return null;
        }

        private int GetColorCount(string color, int x1, int y1, int x2, int y2)
        {
            int count = 0;
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
                for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
                    if (canvas.GetPixel(x, y) == color)
                        count++;
            return count;
        }

        
        private void DrawLine(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;
            while (true)
            {
                DrawPixel(x0, y0);
                if (x0 == x1 && y0 == y1)
                    break;
                e2 = 2 * err;
                if (e2 >= dy)
                {
                    err += dy;
                    x0 += sx;
                }
                if (e2 <= dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        private void Fill(int x, int y, string color)
        {
            string targetColor = canvas.GetPixel(x, y);
            if (targetColor == null || targetColor == color || color == "Transparent")
                return;

            Queue<(int, int)> queue = new Queue<(int, int)>();
            queue.Enqueue((x, y));

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                string current = canvas.GetPixel(cx, cy);
                if (current != targetColor)
                    continue;

                canvas.SetPixel(cx, cy, color);

                foreach (var (dx, dy) in new[] { (0,1), (1,0), (0,-1), (-1,0) })
                {
                    int nx = cx + dx, ny = cy + dy;
                    if (canvas.GetPixel(nx, ny) == targetColor)
                        queue.Enqueue((nx, ny));
                }
            }
        }

        private void DrawCircle(int centerX, int centerY, int radius)
        {
            int x = radius, y = 0;
            int decisionOver2 = 1 - x;
            while (y <= x)
            {
                DrawPixel(centerX + x, centerY + y);
                DrawPixel(centerX + y, centerY + x);
                DrawPixel(centerX - x, centerY + y);
                DrawPixel(centerX - y, centerY + x);
                DrawPixel(centerX - x, centerY - y);
                DrawPixel(centerX - y, centerY - x);
                DrawPixel(centerX + x, centerY - y);
                DrawPixel(centerX + y, centerY - x);
                y++;
                if (decisionOver2 <= 0)
                    decisionOver2 += 2 * y + 1;
                else
                {
                    x--;
                    decisionOver2 += 2 * (y - x) + 1;
                }
            }
        }

        private void DrawRectangle(int centerX, int centerY, int width, int height)
        {
            int startX = centerX - width / 2;
            int startY = centerY - height / 2;
            int endX = startX + width - 1;
            int endY = startY + height - 1;
            DrawLine(startX, startY, endX, startY);
            DrawLine(endX, startY, endX, endY);
            DrawLine(endX, endY, startX, endY);
            DrawLine(startX, endY, startX, startY);
        }
        
        private void DrawPixel(int x, int y)
        {
            int half = brushSize / 2;
            for (int i = -half; i <= half; i++)
                for (int j = -half; j <= half; j++)
                    canvas.SetPixel(x + i, y + j, brushColor);
        }
        
        public void ShowCanvas()
        {
            canvas.Print();
        }

        public string GetCanvasColor(int x, int y)
        {
            if (canvas != null && x >= 0 && x < canvas.Size && y >= 0 && y < canvas.Size)
                return canvas.GetPixel(x, y);
            return "White"; // o "Transparent" según tu lógica
        }

    }
}
