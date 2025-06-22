using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE
{
    public class Interprete
    {
        private Dictionary<string, int> labels = new Dictionary<string, int>();
        private int programCounter = 0;
        private Dictionary<string, int> variables = new Dictionary<string, int>();
        private List<string> output = new List<string>();
        private Bitmap canvas = new Bitmap(800, 600);
        private SolidBrush brush = new SolidBrush(Color.Red);
        private int brushSize = 2;

        public void Interpret(string code)
        {
            string[] lines = code.Split('\n');
            foreach (string line in lines)
            {
                line = line.Trim();
                if (line.StartsWith("Label:"))
                {
                    string label = line.Substring(7).Trim();
                    labels[label] = programCounter;
                }
                else if (line.StartsWith("GoTo:"))
                {
                    string label = line.Substring(5).Trim();
                    programCounter = labels[label];
                }
                else if (line.StartsWith("Spawn:"))
                {
                    string[] parts = line.Substring(6).Split(',');
                    int x = int.Parse(parts[0].Trim());
                    int y = int.Parse(parts[1].Trim());
                    DrawCircle(x, y, 10);
                }
                else if (line.StartsWith("Color:"))
                {
                    string color = line.Substring(6).Trim();
                    SetBrushColor(color);
                }
                else if (line.StartsWith("Size:"))
                {
                    int size = int.Parse(line.Substring(5).Trim());
                    SetBrushSize(size);
                }
                else if (line.StartsWith("DrawLine:"))
                {
                    string[] parts = line.Substring(10).Split(',');
                    int x1 = int.Parse(parts[0].Trim());
                    int y1 = int.Parse(parts[1].Trim());
                    int x2 = int.Parse(parts[2].Trim());
                    int y2 = int.Parse(parts[3].Trim());
                    DrawLine(x1, y1, x2, y2);
                }
                else if (line.StartsWith("DrawCircle:"))
                {
                    string[] parts = line.Substring(11).Split(',');
                    int x = int.Parse(parts[0].Trim());
                    int y = int.Parse(parts[1].Trim());
                    int radius = int.Parse(parts[2].Trim());
                    DrawCircle(x, y, radius);
                }
                else if (line.StartsWith("DrawRectangle:"))
                {
                    string[] parts = line.Substring(14).Split(',');
                    int x = int.Parse(parts[0].Trim());
                    int y = int.Parse(parts[1].Trim());
                    int width = int.Parse(parts[2].Trim());
                    int height = int.Parse(parts[3].Trim());
                    DrawRectangle(x, y, width, height);
                }
                else if (line.StartsWith("Fill:"))
                {
                    string[] parts = line.Substring(5).Split(',');
                    int x = int.Parse(parts[0].Trim());
                    int y = int.Parse(parts[1].Trim());
                    int width = int.Parse(parts[2].Trim());
                    int height = int.Parse(parts[3].Trim());
                    FillRectangle(x, y, width, height);
                }
                else if (line.StartsWith("Assignment:"))
                {
                    string[] parts = line.Substring(12).Split(',');
                    string variable = parts[0].Trim();
                    int value = int.Parse(parts[1].Trim());
                    variables[variable] = value;
                }
                else
                {
                    output.Add($"Linea {programCounter + 1}: {line}");
                }
                programCounter++;
            }
        }

        private void DrawCircle(int x, int y, int radius)
        {
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
            }
        }

        private void DrawLine(int x1, int y1, int x2, int y2)
        {
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.DrawLine(Pens.Black, x1, y1, x2, y2);
            }
        }

        private void DrawRectangle(int x, int y, int width, int height)
        {
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.DrawRectangle(Pens.Black, x, y, width, height);
            }
        }

        private void FillRectangle(int x, int y, int width, int height)
        {
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.FillRectangle(brush, x, y, width, height);
            }
        }

        private void SetBrushColor(string color)
        {
            switch (color.ToLower())
            {
                case "red":
                    brush.Color = Color.Red;
                    break;
                case "green":
                    brush.Color = Color.Green;
                    break;
                case "blue":
                    brush.Color = Color.Blue;
                    break;
                case "yellow":
                    brush.Color = Color.Yellow;
                    break;
                case "black":
                    brush.Color = Color.Black;
                    break;
                case "white":
                    brush.Color = Color.White;
                    break;
                case "orange":
                    brush.Color = Color.Orange;
                    break;
                case "purple":
                    brush.Color = Color.Purple;
                    break;
                case "transparent":
                    brush.Color = Color.Transparent;
                    break;
                default:
                    output.Add($"Color desconocido: {color}");
                    break;
            }
        }

        private void SetBrushSize(int size)
        {
            brushSize = size;
        }

        public void Run()
        {
            string code = richTextBox1.Text;
            Interpret(code);
            pictureBox1.Image = canvas;
            foreach (string line in output)
            {
                richTextBox1.AppendText(line + Environment.NewLine);
            }
        }
    }
}