namespace MiniPascalLexer.Lexer
{
    /// <summary>
    /// Representa um token reconhecido pelo analisador léxico,
    /// contendo seu lexema original, sua classificação, posição no código e eventual mensagem de erro.
    /// </summary>
    public class Token
    {
        public string Lexema { get; set; }
        public TokenType Tipo { get; set; }
        public int Linha { get; set; }
        public int Coluna { get; set; }
        public string? MensagemErro { get; set; }

        public Token(string lexema, TokenType tipo, int linha, int coluna, string? mensagemErro = null)
        {
            Lexema = lexema;
            Tipo = tipo;
            Linha = linha;
            Coluna = coluna;
            MensagemErro = mensagemErro;
        }

        /// <summary>
        /// Formato exato pedido no trabalho prático: "<lexema, token>" por linha.
        /// Exemplo: "program Palavra reservada" ou "x Identificador"
        /// </summary>
        public string ToOutputLine()
        {
            if (Tipo == TokenType.ErroLexico)
            {
                return $"{Lexema} {Tipo.ToFriendlyString()} (Linha {Linha}, Coluna {Coluna}: {MensagemErro})";
            }
            return $"{Lexema} {Tipo.ToFriendlyString()}";
        }

        public override string ToString()
        {
            return $"<{Lexema}, {Tipo.ToFriendlyString()}> [L:{Linha}, C:{Coluna}]";
        }
    }
}
