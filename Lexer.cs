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
            text = Text ?? string.Empty;
        }

        public Token NextToken()
        {
            if(position >= text.Length)
                return new Token(TokenType.EOF, line, position, string.Empty, null);


            while (char.IsWhiteSpace(current) && current != '\n')
                position++;

            if (current == '\n')
                return ObtenerNuevaLinea();

            if(current == '"')
                return ObtenerString();

            if(current == '<' && next == '-')
                return ObtenerAssign();

            if(IsOperBin())
                return ObtenerOperBin();

            if(singleToken.TryGetValue(current, out var singTok))
            {
                var proove = new Token(singTok, line, position, current.ToString(), null!);
                position++;
                return proove;
            }

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

        Token ObtenerNuevaLinea()
        {
            var start = position;
            position++;;
            line++;
            return new Token(TokenType.NewLine, line, start, "\n", null!);
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
                    Error.SetError("SYNTAX", "Saltos de linea no validos", line);
                    return new Token(TokenType.Error, line, position, text.Substring(start), null!);
                }

                if (current == '\\' && !final)
                    final = true;

                else
                    final = false;

                position++;
            }

            if(current == '\0')
            {
                Error.SetError("SYNTAX", "No es una cadena valida", line);
                return new Token(TokenType.Error, line, start, text.Substring(start, position - start), null!);
            }            
            position++;
            var stock = text.Substring(start, position - start);

            if (stock.Length < 2)
            {
                Error.SetError("SYNTAX", "Cadena demasiado corta o incompleta", line);
                return new Token(TokenType.Error, line, start, stock, null!);
            }

            var center = stock.Substring(1, stock.Length - 2);
            string clean = center.Replace("\\\"", "\"").Replace("\\\\", "\\");

            return new Token(TokenType.String, line, start, stock, clean);
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
                Error.SetError("LEXICAL", $"Numero no valido '{text}'", line);
                return new Token(TokenType.Error, line, start, text, null!);
            }

            return new Token(TokenType.Numero, line, start, test, value);
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

            return new Token(TokenType.WhiteEspace, line, start, test, null!);
        }

        bool IsOperBin()
        {
            if ((current == '=' && next == '=') ||
                    (current == '<' && next == '=') ||
                    (current == '>' && next == '=') ||
                    (current == '&' && next == '&') ||
                    (current == '|' && next == '|') ||
                    (current == '*' && next == '*'))
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

            TokenType tok = test switch
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

            return tok == TokenType.Error ? TheresError(test, start) : new Token(tok, line, start, test, null!);
        }

        private Token TheresError(string test, int start)
        {
            Error.SetError("LEXICAL", $"Token inválido '{test}'", line);
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
                Error.SetError("LEXICAL", $"Identificador Invalido :{current}", line);
                position++;
                return new Token(TokenType.Error, line, start, text.Substring(start, 1), null!);
            }

            position++;

            while (char.IsLetterOrDigit(current) || current == '_')
                position++;

            int identLenght = position - start;
            string identText = text.Substring(start, identLenght);


            TokenType token = LexerUtilities.GetKeywordToken(identText);

            return new Token(token, line, start, identText, null!);
        }

        public IEnumerable<Token> LexAll()
        {
            Token tok;

            do
            {
                tok = NextToken();

                if (tok.token == TokenType.Error)
                    Error.SetError("LEXICAL", $"Token no valido: {current}", line);

                yield return tok;

            }
            while (tok.token != TokenType.EOF);
        }

    }
}