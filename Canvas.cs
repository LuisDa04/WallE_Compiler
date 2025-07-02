// using System;

// namespace WallE
// {
//     public class Canvas
//     {
//         public int Size { get; }
//         public string[,] Pixels { get; }

//         public Canvas(int size)
//         {
//             Size = size;
//             Pixels = new string[size, size];
//             for (int y = 0; y < size; y++)
//                 for (int x = 0; x < size; x++)
//                     Pixels[y, x] = "White";
//         }

//         public void SetPixel(int x, int y, string color)
//         {
//             if (x >= 0 && x < Size && y >= 0 && y < Size)
//                 Pixels[y, x] = color;
//         }

//         public string GetPixel(int x, int y)
//         {
//             if (x >= 0 && x < Size && y >= 0 && y < Size)
//                 return Pixels[y, x];
//             return "White";
//         }
        
//         public void Print()
//         {
//             for (int y = 0; y < Size; y++)
//             {
//                 for (int x = 0; x < Size; x++)
//                 {
//                     Console.Write(Pixels[y, x].Substring(0, 1) + " ");
//                 }
//                 Console.WriteLine();
//             }
//         }
//     }
// }
