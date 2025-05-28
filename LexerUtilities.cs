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
        };

        public static TokenType GetKeywordToken(string text)
        {
            return Keywords.TryGetValue(text, out var token) ? token : TokenType.Identificador;
        }

    }
}



//         public static readonly Dictionary<char,Func<int, int, char, (Token, int)>> Operadores = new()
//         {
//             ['+'] = (pos,line, _) => (new Token(TokenType.Suma, line, pos,"+", null!), pos +1),
//             ['-'] = (pos,line, _) => (new Token(TokenType.Resta ,line, pos,"-", null!), pos +1),
//             ['*'] = ObtenerMultOPow,
//             ['/'] = (pos,line, _) => (new Token(TokenType.Division, line, pos,"/", null!), pos +1),
//             ['%'] = (pos,line, _) => (new Token(TokenType.Modulo, line, pos,"%", null!), pos +1),
//             ['('] = (pos,line, _) => (new Token(TokenType.ParentesisAbre, line, pos,"(", null!), pos +1),
//             [')'] = (pos,line, _) => (new Token(TokenType.ParentesisCierra, line, pos,")", null!), pos +1),
//             ['['] = (pos,line, _) => (new Token(TokenType.CorcheteAbre, line, pos,"[", null!), pos +1),
//             [']'] = (pos,line, _) => (new Token(TokenType.CorcheteCierra, line, pos,"]", null!), pos +1),
//             [','] = (pos,line, _) => (new Token(TokenType.Coma, line, pos,",", null!), pos +1),
//             ['>'] = ObtenerMayorOIgual,
//             ['<'] = ObtenerMenorOAssign,
//             ['='] = ObtenerIgual,
//             ['&'] = ObtenerAnd,
//             ['|'] = ObtenerOr
//         };


//         public static TokenType GetKeywords(string token)
//         {
//             if (Keywords.TryGetValue(token, out TokenType value))
//                 return value;

//             return TokenType.Identificador;
//         }
//         static(Token, int) ObtenerMultOPow(int pos, int line, char next)
//         {
//             if(next == '*')
//                 return (new Token(TokenType.Potencia, line, pos, "**" , null!), pos+2);
            
//             return (new Token(TokenType.Multiplicacion, line, pos, "*", null!), pos+1);
//         }

//         static (Token,int) ObtenerMayorOIgual(int pos, int line, char next)
//         {
//             if(next == '=')
//                 return (new Token(TokenType.MayorIgualQue, line,pos, ">=", null!), pos +2);

//             return (new Token(TokenType.MayorQue, line, pos , ">", null!), pos +1);
//         }

//         static (Token, int) ObtenerMenorOAssign(int pos , int line, char next)
//         {
//             if(next == '=')
//                 return (new Token(TokenType.MenorIgualQue, line, pos, "<=", null!), pos +2);
            
//             else if(next == '-')
//                 return (new Token(TokenType.Asignacion, line, pos, "<-", null!), pos +2);
            
//             return(new Token(TokenType.MenorQue, line, pos, "<", null!), pos +1);
//         }

//         static (Token, int) ObtenerIgual(int pos, int line, char next)
//         {
//             if(next == '=')
//                 return (new Token(TokenType.IgualQue, line, pos , "==", null!), pos+2);

//             return WrongNext(pos,line,next);
//         }
//         static(Token, int) ObtenerAnd(int pos, int line, char next)
//         {
//             if(next == '&')
//                 return (new Token(TokenType.And,line, pos, "&&", null!), pos +2);

//             return WrongNext(pos,line,next);
//         }
        
//         static (Token, int) ObtenerOr(int pos, int line, char next)
//         {
//             if(next == '|')
//                 return (new Token(TokenType.Or, line, pos, "||", null!), pos +2);
            
//             return WrongNext(pos,line,next);
//         }

//         static (Token, int) WrongNext(int pos, int line, char next)
//         {
//             Error.SetError("Lexical",$"Unexpected character at line {line}, position {pos}");

//             return (new Token(TokenType.Error,line,pos,next.ToString(),null!),pos+1);
//         }
//     }
// }
