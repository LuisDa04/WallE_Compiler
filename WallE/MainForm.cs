using System.Drawing;
using System.Windows.Forms;

namespace WallE
{
    public class MainForm : Form
    {
        private TextBox editor;
        private Panel canvas;
        private Button runButton;

        public MainForm()
        {
            Text = "Wall-E Pixel Compiler";
            Width = 1000;
            Height = 600;

            editor = new TextBox {
                Multiline = true,
                Font = new Font("Consolas", 12),
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Left,
                Width = 400
            };

            canvas = new Panel {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

            runButton = new Button {
                Text = "Ejecutar",
                Dock = DockStyle.Top,
                Height = 40
            };

            runButton.Click += (sender, e) => {
                MessageBox.Show("Aquí irá la ejecución del compilador");
            };

            Controls.Add(canvas);
            Controls.Add(editor);
            Controls.Add(runButton);
        }
    }
}
