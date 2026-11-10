namespace MiniPascalLexer.Lexer
{
    /// <summary>
    /// Enumeração contendo todas as categorias sintáticas de tokens
    /// requeridas pelo compilador da linguagem Mini_Pascal.
    /// </summary>
    public enum TokenType
    {
        PalavraReservada,
        Identificador,
        NumeroInteiro,
        NumeroReal,
        OperadorAritmetico,
        OperadorRelacional,
        OperadorLogico,
        SimboloEspecial,
        Atribuicao,
        Fim,
        ConstanteString,
        ConstanteChar,
        ErroLexico
    }

    public static class TokenTypeExtensions
    {
        /// <summary>
        /// Retorna a descrição amigável do token exatamente conforme
        /// exigido pelo padrão de saída do trabalho prático.
        /// </summary>
        public static string ToFriendlyString(this TokenType type)
        {
            return type switch
            {
                TokenType.PalavraReservada    => "Palavra reservada",
                TokenType.Identificador       => "Identificador",
                TokenType.NumeroInteiro       => "Número inteiro",
                TokenType.NumeroReal          => "Número real",
                TokenType.OperadorAritmetico  => "Operador aritmético",
                TokenType.OperadorRelacional  => "Operador relacional",
                TokenType.OperadorLogico      => "Operador lógico",
                TokenType.SimboloEspecial     => "Símbolo especial",
                TokenType.Atribuicao          => "Atribuição",
                TokenType.Fim                 => "Fim",
                TokenType.ConstanteString     => "Constante string",
                TokenType.ConstanteChar       => "Constante caractere",
                TokenType.ErroLexico          => "ERRO LÉXICO",
                _                             => type.ToString()
            };
        }
    }
}
