using System;
using System.Collections.Generic;

namespace MiniPascalLexer.Lexer
{
    /// <summary>
    /// Tabela de Palavras Reservadas e Símbolos do compilador Mini_Pascal.
    /// Implementada com Dictionary para garantir busca em tempo constante O(1).
    /// </summary>
    public class SymbolTable
    {
        private readonly Dictionary<string, TokenType> _keywords;
        private readonly HashSet<string> _declaredSymbols;

        public SymbolTable()
        {
            // Inicializa a tabela de palavras reservadas insensível a maiúsculas/minúsculas (case-insensitive)
            _keywords = new Dictionary<string, TokenType>(StringComparer.OrdinalIgnoreCase);
            _declaredSymbols = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            InitializeReservedWords();
        }

        private void InitializeReservedWords()
        {
            // Lista exata de 47 palavras reservadas constantes na especificação do trabalho:
            string[] reserved = new[]
            {
                "ABSOLUTE", "ARRAY", "BEGIN", "CASE", "CHAR", "CONST", "DIV", "DO",
                "DOWTO", "ELSE", "END", "EXTERNAL", "FILE", "FOR", "FORWARD", "FUNC",
                "FUNCTION", "GOTO", "IF", "IMPLEMENTATION", "INTEGER", "INTERFACE",
                "INTERRUPT", "LABEL", "MAIN", "NIL", "NIT", "OF", "PACKED", "PROC",
                "PROGRAM", "REAL", "RECORD", "REPEAT", "SET", "SHL", "SHR", "STRING",
                "THEN", "TO", "TYPE", "UNIT", "UNTIL", "USES", "VAR", "WHILE",
                "WITH", "XOR"
            };

            foreach (var word in reserved)
            {
                // Palavras especiais que atuam como operadores aritméticos ou lógicos
                if (string.Equals(word, "DIV", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(word, "MOD", StringComparison.OrdinalIgnoreCase))
                {
                    _keywords[word] = TokenType.OperadorAritmetico;
                }
                else if (string.Equals(word, "XOR", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(word, "SHL", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(word, "SHR", StringComparison.OrdinalIgnoreCase))
                {
                    _keywords[word] = TokenType.OperadorLogico;
                }
                else
                {
                    _keywords[word] = TokenType.PalavraReservada;
                }
            }

            // Operadores nomeados da especificação:
            _keywords["MOD"] = TokenType.OperadorAritmetico;
            _keywords["AND"] = TokenType.OperadorLogico;
            _keywords["OR"]  = TokenType.OperadorLogico;
            _keywords["NOT"] = TokenType.OperadorLogico;
        }

        /// <summary>
        /// Verifica se a cadeia alfanumérica é uma palavra reservada ou operador nomeado.
        /// Caso seja, retorna true e preenche o tipo correspondente.
        /// Caso contrário, retorna false.
        /// </summary>
        public bool TryGetKeywordType(string lexeme, out TokenType type)
        {
            return _keywords.TryGetValue(lexeme, out type);
        }

        /// <summary>
        /// Insere um novo identificador na tabela de símbolos durante a compilação.
        /// </summary>
        public bool RegisterSymbol(string identifier)
        {
            return _declaredSymbols.Add(identifier);
        }

        public IReadOnlyCollection<string> GetKeywords() => _keywords.Keys;
        public IReadOnlyCollection<string> GetSymbols() => _declaredSymbols;
    }
}
