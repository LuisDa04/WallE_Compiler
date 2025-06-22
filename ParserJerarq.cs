using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WallE
{
    public static class ParserJerarq
    {
        private static readonly Dictionary<TokenType, int> binaryOperatorPrecedence = new() 
        {       
                [TokenType.Or]                 = 10,
            
                [TokenType.And]                = 20,
            
                [TokenType.IgualQue]           = 30, 
            
                [TokenType.MenorQue]           = 40, 
            
                [TokenType.MayorQue]           = 40,
            
                [TokenType.MenorIgualQue]      = 40,  
            
                [TokenType.MayorIgualQue]      = 40, 
            
                [TokenType.Suma]               = 50,  
            
                [TokenType.Resta]              = 50,  
            
                [TokenType.Multiplicacion]     = 60, 
            
                [TokenType.Division]           = 60,
            
                [TokenType.Modulo]             = 60,  
        
                [TokenType.Potencia]           = 70
        };

        private static readonly Dictionary<TokenType, int> unaryOperatorPrecedence = new()
        {
            [TokenType.Suma]  = 80,

            [TokenType.Resta] = 80
        };

        public static int GetBinaryOperatorPrecedence(this TokenType tok)
        {
            return binaryOperatorPrecedence.TryGetValue(tok, out int value) ? value : 0 ;
        }

        public static int GetUnaryOperatorPrecedence(this TokenType tok)
        {
            return unaryOperatorPrecedence.TryGetValue(tok, out int value) ? value : 0 ;
        }
    }
}