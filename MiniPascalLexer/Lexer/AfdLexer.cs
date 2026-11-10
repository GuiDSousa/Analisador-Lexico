using System;
using System.Collections.Generic;
using System.Text;

namespace MiniPascalLexer.Lexer
{
    /// <summary>
    /// Motor do Analisador Léxico baseado integralmente no
    /// Autômato Finito Determinístico (AFD) formalizado para o Mini_Pascal.
    /// Percorre o fluxo de caracteres com controle estrito de linha e coluna,
    /// tratando comentários de bloco, descarte de caracteres brancos e recuperação de erros.
    /// </summary>
    public class AfdLexer
    {
        private readonly string _source;
        private readonly int _length;
        private int _index;
        private int _line;
        private int _column;
        private readonly SymbolTable _symbolTable;

        public AfdLexer(string sourceCode, SymbolTable? symbolTable = null)
        {
            _source = sourceCode ?? string.Empty;
            _length = _source.Length;
            _index = 0;
            _line = 1;
            _column = 1;
            _symbolTable = symbolTable ?? new SymbolTable();
        }

        /// <summary>
        /// Processa todo o código fonte e retorna a lista ordenada de todos os tokens reconhecidos.
        /// </summary>
        public List<Token> TokenizeAll()
        {
            var tokens = new List<Token>();
            while (true)
            {
                Token? token = NextToken();
                if (token == null)
                    break;
                tokens.Add(token);
            }
            return tokens;
        }

        /// <summary>
        /// Função principal de leitura: retorna o próximo par <cadeia, token>
        /// conforme a especificação do trabalho.
        /// </summary>
        public Token? NextToken()
        {
            while (!IsAtEnd())
            {
                char c = Peek();

                // 1. Descarte de espaços em branco e controle de linha/coluna
                if (c == ' ' || c == '\t' || c == '\r')
                {
                    Advance();
                    continue;
                }
                if (c == '\n')
                {
                    AdvanceLine();
                    continue;
                }

                // 2. Comentários de bloco '/* ... */' ou operador de divisão '/'
                if (c == '/')
                {
                    int startLine = _line;
                    int startCol = _column;
                    Advance(); // consome '/'

                    if (!IsAtEnd() && Peek() == '*')
                    {
                        // Entrou no estado q2 (corpo de comentário de bloco)
                        Advance(); // consome '*'
                        bool closed = ProcessBlockComment(startLine, startCol, out string? errorMsg);
                        if (!closed)
                        {
                            return new Token("/*", TokenType.ErroLexico, startLine, startCol, errorMsg);
                        }
                        // Comentário fechado com sucesso: continua varredura em q0 sem emitir token
                        continue;
                    }
                    else
                    {
                        // Transição q1 -> q_op_div (Operador aritmético)
                        return new Token("/", TokenType.OperadorAritmetico, startLine, startCol);
                    }
                }

                int tokenLine = _line;
                int tokenCol = _column;

                // 3. Identificadores e Palavras Reservadas: [a-zA-Z_][a-zA-Z0-9_]*
                if (char.IsLetter(c) || c == '_')
                {
                    return RecognizeIdentifierOrKeyword(tokenLine, tokenCol);
                }

                // 4. Números Inteiros e Números Reais (com decimal e notação científica)
                if (char.IsDigit(c))
                {
                    return RecognizeNumber(tokenLine, tokenCol);
                }

                // 5. Constantes Strings delimitadas por aspas duplas: "..."
                if (c == '"')
                {
                    return RecognizeString(tokenLine, tokenCol);
                }

                // 6. Constantes Caracteres delimitadas por aspas simples: '...'
                if (c == '\'')
                {
                    return RecognizeChar(tokenLine, tokenCol);
                }

                // 7. Atribuição ':=' ou delimitador ':'
                if (c == ':')
                {
                    Advance();
                    if (!IsAtEnd() && Peek() == '=')
                    {
                        Advance();
                        return new Token(":=", TokenType.Atribuicao, tokenLine, tokenCol);
                    }
                    return new Token(":", TokenType.SimboloEspecial, tokenLine, tokenCol);
                }

                // 8. Operadores Relacionais: '<', '<=', '<>', '>', '>='
                if (c == '<')
                {
                    Advance();
                    if (!IsAtEnd() && Peek() == '=')
                    {
                        Advance();
                        return new Token("<=", TokenType.OperadorRelacional, tokenLine, tokenCol);
                    }
                    if (!IsAtEnd() && Peek() == '>')
                    {
                        Advance();
                        return new Token("<>", TokenType.OperadorRelacional, tokenLine, tokenCol);
                    }
                    return new Token("<", TokenType.OperadorRelacional, tokenLine, tokenCol);
                }

                if (c == '>')
                {
                    Advance();
                    if (!IsAtEnd() && Peek() == '=')
                    {
                        Advance();
                        return new Token(">=", TokenType.OperadorRelacional, tokenLine, tokenCol);
                    }
                    return new Token(">", TokenType.OperadorRelacional, tokenLine, tokenCol);
                }

                if (c == '=')
                {
                    Advance();
                    // '=' pode atuar como operador relacional ou símbolo especial; categorizado como relacional
                    return new Token("=", TokenType.OperadorRelacional, tokenLine, tokenCol);
                }

                // 9. Operadores Aritméticos simples
                if (c == '+')
                {
                    Advance();
                    return new Token("+", TokenType.OperadorAritmetico, tokenLine, tokenCol);
                }
                if (c == '-')
                {
                    Advance();
                    return new Token("-", TokenType.OperadorAritmetico, tokenLine, tokenCol);
                }
                if (c == '*')
                {
                    Advance();
                    return new Token("*", TokenType.OperadorAritmetico, tokenLine, tokenCol);
                }

                // 10. Delimitadores e Símbolos Especiais
                if (c == ';')
                {
                    Advance();
                    return new Token(";", TokenType.SimboloEspecial, tokenLine, tokenCol);
                }
                if (c == ',')
                {
                    Advance();
                    return new Token(",", TokenType.SimboloEspecial, tokenLine, tokenCol);
                }
                if (c == '(')
                {
                    Advance();
                    return new Token("(", TokenType.SimboloEspecial, tokenLine, tokenCol);
                }
                if (c == ')')
                {
                    Advance();
                    return new Token(")", TokenType.SimboloEspecial, tokenLine, tokenCol);
                }
                if (c == '.')
                {
                    Advance();
                    // O ponto final geralmente encerra o programa (end.)
                    return new Token(".", TokenType.Fim, tokenLine, tokenCol);
                }

                // 11. Caractere desconhecido / Erro Léxico
                char invalidChar = Advance();
                return new Token(invalidChar.ToString(), TokenType.ErroLexico, tokenLine, tokenCol,
                    $"Caractere inválido ou desconhecido: '{invalidChar}'");
            }

            return null; // Fim do arquivo
        }

        private bool ProcessBlockComment(int startLine, int startCol, out string? errorMsg)
        {
            errorMsg = null;
            while (!IsAtEnd())
            {
                char c = Peek();
                if (c == '\n')
                {
                    AdvanceLine();
                    continue;
                }
                if (c == '*')
                {
                    Advance();
                    if (!IsAtEnd() && Peek() == '/')
                    {
                        Advance(); // Comentário fechado '*/'
                        return true;
                    }
                    continue;
                }
                Advance();
            }
            errorMsg = $"Comentário de bloco iniciado na linha {startLine}, coluna {startCol} não foi fechado.";
            return false;
        }

        private Token RecognizeIdentifierOrKeyword(int startLine, int startCol)
        {
            var sb = new StringBuilder();
            while (!IsAtEnd())
            {
                char c = Peek();
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(Advance());
                }
                else
                {
                    break;
                }
            }

            string lexeme = sb.ToString();

            // Consulta na Tabela de Símbolos / Palavras Reservadas
            if (_symbolTable.TryGetKeywordType(lexeme, out TokenType kwType))
            {
                return new Token(lexeme, kwType, startLine, startCol);
            }

            _symbolTable.RegisterSymbol(lexeme);
            return new Token(lexeme, TokenType.Identificador, startLine, startCol);
        }

        private Token RecognizeNumber(int startLine, int startCol)
        {
            var sb = new StringBuilder();
            bool isReal = false;

            // Dígitos inteiros
            while (!IsAtEnd() && char.IsDigit(Peek()))
            {
                sb.Append(Advance());
            }

            // Ponto decimal
            if (!IsAtEnd() && Peek() == '.')
            {
                // Lookahead: se for ponto seguido de dígito, é número real.
                // Se for ponto seguido de algo que não é dígito (ex: ".."), não consome o ponto aqui.
                if (_index + 1 < _length && char.IsDigit(_source[_index + 1]))
                {
                    isReal = true;
                    sb.Append(Advance()); // consome '.'

                    while (!IsAtEnd() && char.IsDigit(Peek()))
                    {
                        sb.Append(Advance());
                    }
                }
            }

            // Notação científica 'e' ou 'E' (ex.: 24.40e-04)
            if (!IsAtEnd() && (Peek() == 'e' || Peek() == 'E'))
            {
                int saveIndex = _index;
                char eChar = Advance();
                var expSb = new StringBuilder();
                expSb.Append(eChar);

                if (!IsAtEnd() && (Peek() == '+' || Peek() == '-'))
                {
                    expSb.Append(Advance());
                }

                if (!IsAtEnd() && char.IsDigit(Peek()))
                {
                    isReal = true;
                    while (!IsAtEnd() && char.IsDigit(Peek()))
                    {
                        expSb.Append(Advance());
                    }
                    sb.Append(expSb.ToString());
                }
                else
                {
                    // Erro léxico no expoente
                    return new Token(sb.ToString() + expSb.ToString(), TokenType.ErroLexico, startLine, startCol,
                        "Número real malformado: expoente científico sem dígitos.");
                }
            }

            string lexeme = sb.ToString();
            return isReal
                ? new Token(lexeme, TokenType.NumeroReal, startLine, startCol)
                : new Token(lexeme, TokenType.NumeroInteiro, startLine, startCol);
        }

        private Token RecognizeString(int startLine, int startCol)
        {
            var sb = new StringBuilder();
            Advance(); // Consome aspa dupla inicial '"'

            while (!IsAtEnd())
            {
                char c = Peek();
                if (c == '\n')
                {
                    return new Token(sb.ToString(), TokenType.ErroLexico, startLine, startCol,
                        "Constante string não fechada antes do término da linha.");
                }
                if (c == '"')
                {
                    Advance(); // Consome aspa dupla final
                    return new Token($"\"{sb.ToString()}\"", TokenType.ConstanteString, startLine, startCol);
                }
                sb.Append(Advance());
            }

            return new Token(sb.ToString(), TokenType.ErroLexico, startLine, startCol,
                "Constante string não fechada: fim de arquivo inesperado.");
        }

        private Token RecognizeChar(int startLine, int startCol)
        {
            Advance(); // Consome aspa simples inicial '''

            if (IsAtEnd() || Peek() == '\n')
            {
                return new Token("'", TokenType.ErroLexico, startLine, startCol,
                    "Constante caractere malformada.");
            }

            char c = Advance();

            if (!IsAtEnd() && Peek() == '\'')
            {
                Advance(); // Consome aspa simples final
                return new Token($"'{c}'", TokenType.ConstanteChar, startLine, startCol);
            }

            // Consome até fechar ou quebrar linha para reportar erro
            var errSb = new StringBuilder();
            errSb.Append(c);
            while (!IsAtEnd() && Peek() != '\'' && Peek() != '\n')
            {
                errSb.Append(Advance());
            }
            if (!IsAtEnd() && Peek() == '\'') Advance();

            return new Token($"'{errSb}'", TokenType.ErroLexico, startLine, startCol,
                "Constante caractere inválida: deve conter exatamente um caractere.");
        }

        private bool IsAtEnd() => _index >= _length;

        private char Peek() => IsAtEnd() ? '\0' : _source[_index];

        private char Advance()
        {
            char c = _source[_index++];
            _column++;
            return c;
        }

        private void AdvanceLine()
        {
            _index++;
            _line++;
            _column = 1;
        }
    }
}
