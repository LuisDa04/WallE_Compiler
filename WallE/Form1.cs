using WallE;
using System.Linq;

namespace WallE;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        this.runButton.Click += (sender, e) => RunButton_Click(sender, e);

        void RenderCanvas(Interpreter interpreter, int canvasSize)
        {
            int pixelSize = canvasPanel.Width / canvasSize;
            using Bitmap bmp = new Bitmap(canvasPanel.Width, canvasPanel.Height);
            using Graphics g = Graphics.FromImage(bmp);

            for (int y = 0; y < canvasSize; y++)
            {
                for (int x = 0; x < canvasSize; x++)
                {
                    string colorName = interpreter.GetCanvasColor(x, y);
                    using Brush brush = new SolidBrush(Color.FromName(colorName));
                    g.FillRectangle(brush, x * pixelSize, y * pixelSize, pixelSize, pixelSize);
                }
            }

            canvasPanel.BackgroundImage = bmp.Clone() as Image;
        }

        void RunButton_Click(object sender, EventArgs e)
        {
            string code = editorTextBox.Text;

            try
            {
                // 1. Análisis léxico
                var lexer = new Lexer(code);
                var tokens = lexer.LexAll().ToList();

                // 2. Análisis sintáctico (parser)
                var parser = new Parser(tokens);
                var program = parser.ParseProgram();

                // 3. Validación semántica
                var semanticContext = new SemanticContext();
                program.Validate(semanticContext);

                if (semanticContext.Errors.Count > 0)
                {
                    MessageBox.Show("Errores semánticos:\n" + string.Join("\n", semanticContext.Errors), "Wall‑E dice...");
                    return;
                }

                // 4. Ejecutar el programa con el intérprete
                int canvasSize = 64; // o lee desde un NumericUpDown
                var interpreter = new Interpreter(program, canvasSize);
                interpreter.Execute();

                // 5. Dibujar el canvas resultante
                RenderCanvas(interpreter, canvasSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al ejecutar el programa:\n" + ex.Message, "Wall‑E tiene un problema");
            }
        }

    }
}
