using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE
{
    public class LexerUtilities
    {
        public static readonly Dictionary<string, TokenType> Keywords = new()
        {
            //Comands
            ["Spawn"] = TokenType.Spawn,
            ["Color"] = TokenType.Color,
            ["Size"] = TokenType.Size,
            ["DrawLine"] = TokenType.DrawLine,
            ["DrawCircle"] = TokenType.DrawCircle,
            ["DrawRectangle"] = TokenType.DrawRectangle,
            ["Fill"] = TokenType.Fill,
            ["GoTo"] = TokenType.GoTo,

            //Functions
            ["GetActualX"] = TokenType.GetActualX,
            ["GetActualY"] = TokenType.GetActualY,
            ["GetCanvasSize"] = TokenType.GetCanvasSize,
            ["GetColorCount"] = TokenType.GetColorCount,
            ["IsBrushColor"] = TokenType.IsBrushColor,
            ["IsBrushSize"] = TokenType.IsBrushSize,
            ["IsCanvasColor"] = TokenType.IsCanvasColor,

            ["true"] = TokenType.TrueTok,
            ["false"] = TokenType.FalseTok,
        };

        public static TokenType GetKeywordToken(string text)
        {
            return Keywords.TryGetValue(text, out var token) ? token : TokenType.Identificador;
        }

    }
}



