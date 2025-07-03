using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WallE;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Runtime.Versioning;
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
[assembly: SupportedOSPlatform("windows")]



namespace WallE
{
    public class Interpreter
    {
        public event Action<int, int,Color>? PixelDrawn;
        public bool shouldStop{ get; private set; }
        public string LastErrorMessage { get; private set; } = "";
        
        int walleX = 0;
        int walleY = 0;
        public string currentColor = "Transparent";
        int brushSize = 1;

        
        readonly Dictionary<string, object> VariablesTable = new Dictionary<string, object>();

        readonly Dictionary<string, int> LabelsTable = new Dictionary<string, int>();

        
        readonly Color[,] canvas;

        readonly ProgramNode programNode;

        public Interpreter(ProgramNode programNode, int canvasSize)
        {
            this.programNode = programNode;

            canvas = new Color[canvasSize, canvasSize];

            Createcanvas();
        }

        void Createcanvas()
        {
            for (int i = 0; i < canvas.GetLength(0); i++)
            {
                for (int j = 0; j < canvas.GetLength(1); j++)
                {
                    canvas[i, j] = Color.White;
                }
            }
        }

        public void Start()
        {
            LabelesTableCollecter();

            int flag = 0; 

            while (flag < programNode.Instructions.Count)
            {
                if (programNode.Instructions[flag] is GoToNode goToNode)
                {
                    if (EvaluateBool(goToNode.Condition) && LabelsTable.ContainsKey(goToNode.Label))
                    {
                        flag = LabelsTable[goToNode.Label];
                        continue;
                    }
                }

                DoInstruccion(programNode.Instructions[flag]);
                flag++;
            }

            ShowCanvas();
        }

        void LabelesTableCollecter()
        {
            for (int i = 0; i < programNode.Instructions.Count; i++)
            {
                if (programNode.Instructions[i] is LabelNode labelNode)
                {
                    if (LabelsTable.ContainsKey(labelNode.LabelName))
                        throw new Exception($"Label duplicado '{labelNode.LabelName}' en la línea {labelNode.line}");

                    LabelsTable[labelNode.LabelName] = i + 1;

                }
            }
        }


        void DoInstruccion(InstructionNode instructionNode)
        {
            if (instructionNode is SpawnNode spawnNode)
                DoSpawn(spawnNode);

            else if (instructionNode is ColorNode colorNode)
                DoColor(colorNode);

            else if (instructionNode is SizeNode sizeNode)
                DoSize(sizeNode);

            else if (instructionNode is DrawLineNode drawLineNode)
                DoDrawLine(drawLineNode);

            else if (instructionNode is DrawCircleNode drawCircleNode)
                DoDrawCircle(drawCircleNode);

            else if (instructionNode is DrawRectangleNode drawRectangleNode)
                DoDrawRectangle(drawRectangleNode);

            else if (instructionNode is FillNode fillNode)
                DoFill(fillNode);

            else if (instructionNode is AssignmentNode assignmentNode)
                DoAssignment(assignmentNode);

            else if (instructionNode is LabelNode)
            { } 

            else if (instructionNode is GoToNode)
            { }

            else
                throw new Exception($"Instrucción no reconocida: {instructionNode.GetType().Name} en la línea {instructionNode.line}");
        }

        private void DoAssignment(AssignmentNode assignmentNode)
        {
            VariablesTable[assignmentNode.Identifier] = Evaluate(assignmentNode.Expression);
        }

        private void DoFill(FillNode fillNode)
        {
            int size = canvas.GetLength(0);
            Color targetColor = canvas[walleX, walleY];
            Color newColor = ColorFromName(currentColor);

           
            if (targetColor == newColor) return;
            if (newColor == ColorFromName("Transparent")) return;


            Queue<(int x, int y)> queue = new Queue<(int, int)>();
            queue.Enqueue((walleX, walleY));

            bool[,] visited = new bool[size, size];
            visited[walleX, walleY] = true;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                canvas[x, y] = newColor;
                PixelDrawn?.Invoke(x, y,newColor);

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (nx >= 0 && nx < size && ny >= 0 && ny < size &&
                        !visited[nx, ny] && canvas[nx, ny] == targetColor)
                    {
                        //si es un pixel valido y no se ha visitado agg y visitalo
                        visited[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }

        private void DoDrawRectangle(DrawRectangleNode drawRectangleNode)
        {
            int dx = EvaluateInt(drawRectangleNode.DirXExpression);

            int dy = EvaluateInt(drawRectangleNode.DirYExpression);

            int dist = EvaluateInt(drawRectangleNode.DistanceExpression);

            int w = EvaluateInt(drawRectangleNode.WidthExpression);

            int h = EvaluateInt(drawRectangleNode.HeightExpression);

            int cx = walleX + dx * dist;

            int cy = walleY + dy * dist;

            int halfW = w / 2, halfH = h / 2;

            // Esquinas
            int x1 = cx - halfW, x2 = cx + halfW;

            int y1 = cy - halfH, y2 = cy + halfH;

            // Dibujar bordes
            for (int xi = x1; xi <= x2; xi++) { Draw(xi, y1); Draw(xi, y2); }

            for (int yi = y1; yi <= y2; yi++) { Draw(x1, yi); Draw(x2, yi); }

            walleX = cx; walleY = cy;
        }

        private void DoDrawCircle(DrawCircleNode drawCircleNode)
        {
            int dx = EvaluateInt(drawCircleNode.DirXExpression);

            int dy = EvaluateInt(drawCircleNode.DirYExpression);

            int r = EvaluateInt(drawCircleNode.RadiusExpression);

            int cCenterX = walleX + dx * r;

            int cCenterY = walleY + dy * r;

            for (int i = -r; i <= r; i++)
            {
                int y0 = (int)Math.Round(Math.Sqrt(r * r - i * i));

                PlotCirclePoints(cCenterX, cCenterY, i, y0); 

                PlotCirclePoints(cCenterX, cCenterY, i, -y0); 
            }

            walleX = cCenterX;
            walleY = cCenterY;
        }

        private void DoDrawLine(DrawLineNode drawLineNode)
        {
            try
            {
                int dirX = EvaluateInt(drawLineNode.DirXExpression);

                int dirY = EvaluateInt(drawLineNode.DirYExpression);

                int dist = EvaluateInt(drawLineNode.DistanceExpression);

                if (dirX < -1 || dirX > 1 || dirY < -1 || dirY > 1)
                    throw new Exception($"DrawLine: dirección inválida ({dirX},{dirY}) en línea {drawLineNode.line}. " + "Sólo se permiten -1, 0 o 1.");

                for (int i = 0; i <= dist; i++)
                {
                    int paintX = walleX + (dirX * i);

                    int paintY = walleY + (dirY * i);

                    Draw(paintX, paintY);
                }

                walleX += dirX * dist;

                walleY += dirY * dist;
            }
            catch (Exception ex)
            {
                Stop($"Error en DrawLine:{ex.Message}");
                Error.SetError("RUNTIME", $"{ex.Message}", drawLineNode.line);
            }
        }

        private void DoSize(SizeNode sizeNode)
        {
            int tempSize = EvaluateInt(sizeNode.SizeExpression);

            if (tempSize % 2 == 0)
                brushSize = tempSize - 1;

            else
                brushSize = tempSize;

            if (brushSize < 1)
                brushSize = 1;
        }

        private void DoColor(ColorNode colorNode)
        {
            var color = Evaluate(colorNode.expression);

            currentColor = (string)color;
        }

        private void DoSpawn(SpawnNode spawnNode)
        {
            walleX = EvaluateInt(spawnNode.XExpression);

            walleY= EvaluateInt(spawnNode.YExpression);

            if (walleX < 0 || walleY < 0 || walleX > canvas.GetLength(0) || walleY > canvas.GetLength(1))
                throw new Exception($"Sapwneo no valido, Wall-E fuera de rango");
        }



        public void Stop(string errorMessage)
        {
            shouldStop = true;
            LastErrorMessage = errorMessage; 
        }
        private void PlotCirclePoints(int cx, int cy, int dx, int dy)
        {
            Draw(cx + dx, cy + dy);

            Draw(cx - dx, cy + dy);

            Draw(cx + dx, cy - dy);

            Draw(cx - dx, cy - dy);

            Draw(cx + dy, cy + dx);

            Draw(cx - dy, cy + dx);

            Draw(cx + dy, cy - dx);

            Draw(cx - dy, cy - dx);
        }

        private void Draw(int paintX, int paintY)
        {
            int size = canvas.GetLength(0);

            int r = brushSize / 2;

            for (int ox = -r; ox <= r; ox++)
            {
                for (int oy = -r; oy <= r; oy++)
                {
                    int tx = paintX + ox, ty = paintY + oy;
                    if (tx < 0 || tx >= size || ty < 0 || ty >= size)
                        continue;

                    canvas[tx, ty] = ColorFromName(currentColor);
                        
                    PixelDrawn?.Invoke(tx, ty,canvas[tx,ty]);
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        private Color ColorFromName(string name) =>
            name switch
            {
                "Red" => Color.Red,
                "Blue" => Color.Blue,
                "Green" => Color.Green,
                "Yellow" => Color.Yellow,
                "Orange" => Color.Orange,
                "Purple" => Color.Purple,
                "Black" => Color.Black,
                "White" => Color.White,
                "Transparent" => Color.Transparent,
                _ => Color.Black
            };


        object Evaluate(ExpressionNode expressionNode)
        {
            if (expressionNode is LiteralNode literalNode)
                return literalNode.Value;

            else if (expressionNode is VariableNode variableNode)
                return VariablesTable[variableNode.Name];

            else if (expressionNode is UnaryExpressionNode unaryExpressionNode)
            {
                var val = Evaluate(unaryExpressionNode.MiddleExpression);

                return unaryExpressionNode.Operator == UnaryOperator.Resta ? -(int)val : !(bool)val;
            }

            if (expressionNode is BinaryExpressionNode binaryExpressionNode)
            {
                var left = Evaluate(binaryExpressionNode.LeftExpressionNode);

                var right = Evaluate(binaryExpressionNode.RightExpressionNode);

                if (binaryExpressionNode.Operator == BinaryOperator.Suma)

                    return (int)left + (int)right;

                else if (binaryExpressionNode.Operator == BinaryOperator.Resta)
                    return (int)left - (int)right;

                else if (binaryExpressionNode.Operator == BinaryOperator.Multiplicacion)
                    return (int)left * (int)right;

                else if (binaryExpressionNode.Operator == BinaryOperator.Potencia)
                    return (int)Math.Pow((int)left, (int)right);

                else if (binaryExpressionNode.Operator == BinaryOperator.Division)
                    return (int)left / (int)right;

                else if (binaryExpressionNode.Operator == BinaryOperator.Modulo)
                    return (int)left % (int)right;

                else if (binaryExpressionNode.Operator == BinaryOperator.MenorQue)
                    return (int)left < (int)right;

                else if (binaryExpressionNode.Operator == BinaryOperator.MayorQue)
                    return (int)left > (int)right;

                else if (binaryExpressionNode.Operator == BinaryOperator.MenorIgualQue)
                    return (int)left <= (int)right;

                else if (binaryExpressionNode.Operator == BinaryOperator.MayorIgualQue)
                    return (int)left >= (int)right;

                else if (binaryExpressionNode.Operator == BinaryOperator.IgualQue)
                    return left.Equals(right);

                else if (binaryExpressionNode.Operator == BinaryOperator.NoIgual)
                    return !left.Equals(right);

                else if (binaryExpressionNode.Operator == BinaryOperator.And)
                    return (bool)left && (bool)right;

                else if (binaryExpressionNode.Operator == BinaryOperator.Or)
                    return (bool)left || (bool)right;
            }

            if (expressionNode is BuiltInFunctionNode builtInFunctionNode)
            {
                if (builtInFunctionNode.FunctionKind is FunctionKind.GetActualX)
                    return walleX;

                else if (builtInFunctionNode.FunctionKind is FunctionKind.GetActualY)
                    return walleY;

                else if (builtInFunctionNode.FunctionKind is FunctionKind.GetCanvasSize)
                    return canvas.GetLength(0);

                else if (builtInFunctionNode.FunctionKind is FunctionKind.IsBrushColor)
                {
                    var colorName = (string)Evaluate(builtInFunctionNode.Arguments[0]);

                    bool match = currentColor == colorName;

                    return match ? 1 : 0;
                }

                else if (builtInFunctionNode.FunctionKind is FunctionKind.IsBrushSize)
                {
                    bool boolValue = brushSize == (int)Evaluate(builtInFunctionNode.Arguments[0]);

                    return boolValue ? 1 : 0;
                }

                else if (builtInFunctionNode.FunctionKind is FunctionKind.GetColorCount)
                {
                    var colorName = (string)Evaluate(builtInFunctionNode.Arguments[0]);

                    int x1 = (int)Evaluate(builtInFunctionNode.Arguments[1]);

                    int y1 = (int)Evaluate(builtInFunctionNode.Arguments[2]);

                    int x2 = (int)Evaluate(builtInFunctionNode.Arguments[3]);

                    int y2 = (int)Evaluate(builtInFunctionNode.Arguments[4]);


                    int size = canvas.GetLength(0);

                    if (x1 < 0 || x1 >= size || y1 < 0 || y1 >= size ||
                        x2 < 0 || x2 >= size || y2 < 0 || y2 >= size)
                        return 0;

                    int minX = Math.Min(x1, x2);

                    int maxX = Math.Max(x1, x2);

                    int minY = Math.Min(y1, y2);

                    int maxY = Math.Max(y1, y2);

                    int count = 0;

                    for (int ix = minX; ix <= maxX; ix++)
                        for (int iy = minY; iy <= maxY; iy++)
                            if (canvas[ix, iy] == ColorFromName(colorName))
                                count++;

                    return count;
                }

                else if (builtInFunctionNode.FunctionKind is FunctionKind.IsCanvasColor)
                {
                    var colorName = (string)Evaluate(builtInFunctionNode.Arguments[0]);

                    int vertical = (int)Evaluate(builtInFunctionNode.Arguments[1]);

                    int horizontal = (int)Evaluate(builtInFunctionNode.Arguments[2]);


                    int tx = walleX + horizontal;

                    int ty = walleY + vertical;

                    int size = canvas.GetLength(0);

                    bool inside = tx >= 0 && tx < size && ty >= 0 && ty < size;

                    bool match = inside && canvas[tx, ty].ToString() == colorName;

                    return match ? 1 : 0;
                }
            }

            throw new InvalidOperationException($"No se puede evaluar una expresion de tipo '{expressionNode.GetType().Name}' en la linea {expressionNode.line}");
        }

        int EvaluateInt(ExpressionNode expressionNode)
        {
            return (int)Evaluate(expressionNode);
        }
        bool EvaluateBool(ExpressionNode expressionNode)
        {
            return (bool)Evaluate(expressionNode);
        }

        public Bitmap ShowCanvas()
        {
            int size = canvas.GetLength(0);

            var bmp = new Bitmap(size, size);

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    bmp.SetPixel(x, y, canvas[x, y]);

            using var g = Graphics.FromImage(bmp);

            using var pen = new Pen(Color.FromArgb(50, Color.Gray)); 
                                                                     
            for (int x = 0; x <= size; x++)
                g.DrawLine(pen, x, 0, x, size);
       
            for (int y = 0; y <= size; y++)
                g.DrawLine(pen, 0, y, size, y);

            return bmp;
        }

        public Bitmap ShowCanvasGrid(int cellSize = 16)
        {
            var bmp = ShowCanvas();        
            int w = bmp.Width, h = bmp.Height;
            var big = new Bitmap(w * cellSize, h * cellSize);

            using (var g = Graphics.FromImage(big))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bmp,
                    new Rectangle(0, 0, w * cellSize, h * cellSize),
                    new Rectangle(0, 0, w, h),
                    GraphicsUnit.Pixel);

                using (var pen = new Pen(Color.FromArgb(100, Color.Gray)))
                {
                    pen.DashStyle = DashStyle.Dot;
                    for (int x = 0; x <= w; x++)
                        g.DrawLine(pen, x * cellSize, 0, x * cellSize, h * cellSize);
                    for (int y = 0; y <= h; y++)
                        g.DrawLine(pen, 0, y * cellSize, w * cellSize, y * cellSize);
                }
            }

            return big;
        }
    }
}