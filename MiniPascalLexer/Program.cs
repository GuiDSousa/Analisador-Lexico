using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MiniPascalLexer.Lexer;
using MiniPascalLexer.UI;

namespace MiniPascalLexer
{
    internal static class Program
    {
        /// <summary>
        /// Ponto de entrada principal do aplicativo.
        /// Suporta dois modos:
        /// 1. Sem argumentos: Executa a Interface Gráfica Windows Forms amigável.
        /// 2. Com argumentos [arquivo_entrada.txt] [arquivo_saida.txt]: Executa em modo batch via console.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length >= 1)
            {
                RunCliMode(args);
                return;
            }

            // Modo Gráfico Windows Forms
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }

        private static void RunCliMode(string[] args)
        {
            string inputFile = args[0];
            string outputFile = args.Length >= 2 ? args[1] : "saida_tokens.txt";

            if (!File.Exists(inputFile))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERRO] Arquivo de entrada não encontrado: {inputFile}");
                Console.ResetColor();
                return;
            }

            try
            {
                string source = File.ReadAllText(inputFile, Encoding.UTF8);
                var lexer = new AfdLexer(source);
                var tokens = lexer.TokenizeAll();

                var sb = new StringBuilder();
                int errorCount = 0;

                foreach (var token in tokens)
                {
                    sb.AppendLine(token.ToOutputLine());
                    if (token.Tipo == TokenType.ErroLexico)
                        errorCount++;
                }

                File.WriteAllText(outputFile, sb.ToString(), Encoding.UTF8);

                Console.WriteLine($"=== ANALISADOR LÉXICO MINI_PASCAL ===");
                Console.WriteLine($"Entrada: {inputFile}");
                Console.WriteLine($"Saída:   {outputFile}");
                Console.WriteLine($"Total de Tokens: {tokens.Count}");
                Console.WriteLine($"Erros Léxicos:   {errorCount}");
                Console.WriteLine($"Status: OK");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FALHA] Erro durante a execução: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
