using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE
{
    public class Lexer
    {
        private readonly string text;
        private int position = 0;
        private int line = 1;
        private char current => Peek(0);
        private char next => Peek(1);

        public Lexer(string Text)
        {
            text = Text;
        }

        public Token NextToken()
        {
            if(position >= text.Length)
                return new Token(Token.EOF, line, position, string.Empty, null);

            if(current == '"')
                return ObtenerString();

            if(current == '<' && next == '-')
                return ObtenerAssign();

            if(IsOperBin())
                return ObtenerOperBin();

            if(OperUn.TryGetValue(current, out var singleToken))
            {
                var test = new Token(singleToken, line, position, current.ToString(), null!);
                position++;
                return test;
            }

            if(char.IsWhiteSpace(current))
                return ObtenerEspacio();

            if(char.IsDigit(current))
                return ObtenerNumber();

            if(char.IsLetter(current))
                return ObtenerIdentificador();

            string test = text.Substring(position, 1);
            var error = new Token(TokenType.Error, line, position, test, null!);
            position++;
            return error;
        }
            
        private char Peek(int place)
        {
            var id = position + place;
            return id >=text.Length ? '\0' : text[id];
        }

        Token ObtenerString()
        {
            var start = position;
            position++;
            bool final = false;

            while(current != '\0' && (current != '"' || final))
            {
                if(current == '\n')
                {
                    Error.SetError("SYNTAX", $"Line {line}: Saltos de linea no validos");
                    return new Token(Token.Error, line, position, Text.Substring(start), null!);
                }
            }

            if(current == '\0')
            {
                Error.SetError("SYNTAX", $"Line {line}: No es una cadena valida");
                return new Token(TokenType.ErrorToken, line, start, Text.Substring(start, position - start), null!);
            }            
            position++;
            var stock = text.Substring(start, position - start);

            var center = stock.Substring(1, stock.Length - 2);

            string clean = value.Replace("\\\"", "\"").Replace("\\\\", "\\");

            return new Token(TokenType.String, line, start, stock, clean)
        }

        Token ObtenerNumber()
        {
            var start = position;
            while(char.IsDigit(current))
            {
                position++;
            }

            var test = text.Substring(start, position - start);
            if(!int.TryParse(test, out var value))
            {
                Error.SetError("LEXICAL", $"Line {line}: Numero no valido '{text}'");
                return new Token(TokenType.Error, line, start, text, null!);
            }

            return new Token(TokenType.Numero, line, start, test, value)
        }

        Token ObtenerEspacio()
        {
            var start = position;

            if (current == '\n')
            {
                position++;
                line++;
                return new Token(TokenType.NewLine, line, start, "\n", null!);
            }

            while (char.IsWhiteSpace(current) && current != '\n')
            {
                position++;
            }

            var test = text.Substring(start, position - start);

            return new Token(TokenType.Whitespace, line, start, test, null!);
        }

        bool IsOperBin()
        {
            if ((Current == '=' && NextChar == '=') ||
                    (Current == '<' && NextChar == '=') ||
                    (Current == '>' && NextChar == '=') ||
                    (Current == '&' && NextChar == '&') ||
                    (Current == '|' && NextChar == '|') ||
                    (Current == '*' && NextChar == '*'))
            {
                return true;
            }

            return false;
        }

        Token ObtenerAssign()
        {
            var start = position;
            position += 2; 
            return new Token(TokenType.Asignacion, line, start, "<-", null!);
        }

        Token ObtenerOperBin()
        {
            int start = position;

            string test = text.Substring(position, 2);

            Token token = test switch
            {
                "==" => TokenType.IgualQue,
                ">=" => TokenType.MayorIgualQue,
                "<=" => TokenType.MenorIgualQue,
                "&&" => TokenType.And,
                "||" => TokenType.Or,
                "**" => TokenType.Potencia,
                _ => TokenType.Error
            };

            position += 2;

            return token == TokenType.Error ? TheresError(test, start) : new SyntaxToken(token, line, start, test, null!);
        }

        private Token TheresError(string test, int start)
        {
            Error.SetError("LEXICAL", $"Line {line}: Token inválido '{test}'");
            return new Token(TokenType.Error, line, start, test, null!);
        }

        static readonly Dictionary<char, TokenType> singleToken = new()
        {
            ['('] = TokenType.ParentesisAbre,
            [')'] = TokenType.ParentesisCierra,
            ['['] = TokenType.CorcheteAbre,
            [']'] = TokenType.CorcheteCierra,
            [','] = TokenType.Coma,
            ['+'] = TokenType.Suma,
            ['-'] = TokenType.Resta,
            ['*'] = TokenType.Multiplicacion,
            ['/'] = TokenType.Division,
            ['%'] = TokenType.Modulo,
            ['<'] = TokenType.MenorQue,
            ['>'] = TokenType.MayorQue,
        };

        Token ObtenerIdentificador()
        {
            var start = position;

            if (char.IsDigit(current) || current == '-')
            {
                Error.SetError("LEXICAL", $"Line{line} : Identificador Invalido :{current}");
                position++;
                return new Token(TokenType.Error, line, start, text.Substring(start, 1), null!);
            }

            position++;

            while (char.IsLetterOrDigit(current) || current == '_' || current == '-')
                position++;

            int identLenght = position - start;
            string identText = text.Substring(start, identLenght);

            if (current == '\n' || current == '\0')
            {
                return new Token(TokenType.Label, line, start, identText, identText);
            }

            TokenType token = LexerUtilities.GetKeywordToken(identText);

            return new Token(token, line, start, identText, null!);
        }






    }
}