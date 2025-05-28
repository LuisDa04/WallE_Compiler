using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE
{
    public class Token
    {

        public TokenType token {get;}
        public int position {get;}
        public int line {get;}
        public string text {get;}
        public object value {get;}

        public Token(TokenType Token, int Line, int Position, string Text, object Value)
        {
            token = Token;
            position = Position;
            line = Line;
            text = Text;
            value = Value;
        }


        // public TokenType Type { get; set; }
        // public string Lexeme { get; set; }
        // public object Literal { get; set; }
        // public

        // public Token(TokenType type, string lexeme, object literal)
        // {
        //     Type = type;
        //     Lexeme = lexeme;
        //     Literal = literal;
        // }

        // public string toString()
        // {
        //     return Type + " " + Lexeme + " " + Literal;
        // }

    }
}
