using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE
{
    public enum TokenType
    {
        //Keywords
        Spawn, Color, Size, DrawLine, DrawCircle, DrawRectangle, Fill, 
        GetActualX, GetActualY, GetCanvasSize, GetColorCount, IsBrushColor, IsBrushSize, IsCanvassColor,
        GoTo,

        //Anothers
        Asignacion,
        Identificador, Numero, ColorLiteral, String,
        ParentesisAbre, ParentesisCierra, CorcheteAbre, CorcheteCierra, Coma,
        Error,
        WhiteEspace, EOF, Label, NewLine,

        //Operadores
        Suma, Resta, Multiplicacion, Division, Potencia, Modulo,
        IgualQue, MayorQue, MenorQue, MayorIgualQue, MenorIgualQue,
        And, Or
    }
}
