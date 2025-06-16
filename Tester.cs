using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE_Compiler
{
class Program
{
    static void Main(string[] args)
    {
        var examples = new[]
        {
            "Hola mundo!",
            "123",
            "\"Hola mundo\"",
            "x + y * z",
            "if (x > y) then",
            "while (i < 10)",
            "function foo() {",
            "var x = 5;",
            "console.log(x);"
        };

        foreach (var example in examples)
        {
            Console.WriteLine($"Ejemplo: {example}");
            var lexer = new Lexer(example);
            Token? lastToken = null;

            while (true)
            {
                var token = lexer.NextToken();
                if (token.TokenType == Token.EOF)
                    break;

                Console.WriteLine($"{token.TokenType} {token.Value} en la línea {token.Line}, posición {token.Position}");

                if (lastToken.HasValue)
                {
                    if (!Regex.IsMatch(lastToken.Value + token.Value, @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
                        Console.WriteLine("ERROR: Combinación de tokens no válida");
                }

                lastToken = token;
            }

            Console.WriteLine("---");
        }
    }
}
}
