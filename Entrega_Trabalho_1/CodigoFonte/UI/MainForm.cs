using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MiniPascalLexer.Lexer;

namespace MiniPascalLexer.UI
{
    /// <summary>
    /// Interface Gráfica moderna do Analisador Léxico Mini_Pascal.
    /// Permite carregar arquivos fonte (.txt), editar código, executar a análise léxica baseada no AFD,
    /// visualizar a listagem de pares <lexema, token>, inspecionar erros e exportar o arquivo de saída.
    /// </summary>
    public class MainForm : Form
    {
        private TextBox _txtSource = null!;
        private DataGridView _gridTokens = null!;
        private ListBox _lstErrors = null!;
        private StatusStrip _statusStrip = null!;
        private ToolStripStatusLabel _lblStatus = null!;
        private ToolStripStatusLabel _lblCounts = null!;
        private OpenFileDialog _openFileDialog = null!;
        private SaveFileDialog _saveFileDialog = null!;
        private List<Token> _currentTokens = new List<Token>();

        public MainForm()
        {
            InitializeComponentsCustom();
        }

        private void InitializeComponentsCustom()
        {
            Text = "Compiladores 2026.2 - Analisador Léxico (Mini_Pascal) | Discente: Guilherme Dias Sousa";
            Width = 1100;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            Icon = SystemIcons.Application;

            // Menu Superior
            var menuStrip = new MenuStrip();
            var menuArquivo = new ToolStripMenuItem("Arquivo");
            var menuTestes = new ToolStripMenuItem("Exemplos de Teste");
            var menuAjuda = new ToolStripMenuItem("Ajuda");

            var itemAbrir = new ToolStripMenuItem("Abrir Arquivo Fonte (.txt)...", null, OnOpenFileClicked, Keys.Control | Keys.O);
            var itemSalvar = new ToolStripMenuItem("Salvar Tokens (.txt)...", null, OnSaveOutputClicked, Keys.Control | Keys.S);
            var itemSair = new ToolStripMenuItem("Sair", null, (s, e) => Close());
            menuArquivo.DropDownItems.AddRange(new ToolStripItem[] { itemAbrir, itemSalvar, new ToolStripSeparator(), itemSair });

            // Menus rápidos de testes
            var itemT1 = new ToolStripMenuItem("Exemplo 1 do PDF (var, const, if/else, write)", null, (s, e) => LoadSample(1));
            var itemT2 = new ToolStripMenuItem("Exemplo 2 do PDF (Program Piloto, laço for, médias)", null, (s, e) => LoadSample(2));
            var itemT3 = new ToolStripMenuItem("Exemplo 3 (while, repeat, char, lógicos)", null, (s, e) => LoadSample(3));
            var itemT4 = new ToolStripMenuItem("Exemplo 4 (operadores relacionais e aritméticos)", null, (s, e) => LoadSample(4));
            var itemT5 = new ToolStripMenuItem("Exemplo 5 (tratamento de erros léxicos)", null, (s, e) => LoadSample(5));
            menuTestes.DropDownItems.AddRange(new ToolStripItem[] { itemT1, itemT2, itemT3, itemT4, itemT5 });

            var itemSobre = new ToolStripMenuItem("Sobre o Compilador...", null, OnAboutClicked);
            menuAjuda.DropDownItems.Add(itemSobre);

            menuStrip.Items.AddRange(new ToolStripItem[] { menuArquivo, menuTestes, menuAjuda });
            MainMenuStrip = menuStrip;
            Controls.Add(menuStrip);

            // Barra de Ferramentas com Ações Rápidas
            var toolStrip = new ToolStrip { ImageList = null, GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(8, 4, 8, 4) };
            
            var btnAbrir = new ToolStripButton("📁 Abrir Código (.txt)", null, OnOpenFileClicked);
            var btnAnalisar = new ToolStripButton("▶ Executar Análise Léxica", null, OnAnalyzeClicked)
            {
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            var btnSalvar = new ToolStripButton("💾 Salvar Tokens (.txt)", null, OnSaveOutputClicked);
            var btnLimpar = new ToolStripButton("🗑 Limpar", null, OnClearClicked);

            toolStrip.Items.AddRange(new ToolStripItem[] { btnAbrir, new ToolStripSeparator(), btnAnalisar, new ToolStripSeparator(), btnSalvar, new ToolStripSeparator(), btnLimpar });
            Controls.Add(toolStrip);

            // Divisão Principal (Esquerda: Código Fonte | Direita: Tokens e Erros)
            var splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 500,
                SplitterWidth = 6
            };

            // Painel Esquerdo: Editor / Visualizador do Código
            var pnlLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            var lblCodeTitle = new Label
            {
                Text = "Código-Fonte Mini_Pascal (Entrada):",
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            _txtSource = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 10.5f, FontStyle.Regular),
                WordWrap = false,
                BackColor = Color.FromArgb(250, 250, 252)
            };
            pnlLeft.Controls.Add(_txtSource);
            pnlLeft.Controls.Add(lblCodeTitle);
            splitMain.Panel1.Controls.Add(pnlLeft);

            // Painel Direito: Split Horizontal (Tokens em Cima | Erros Embaixo)
            var splitRight = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 420,
                SplitterWidth = 6
            };

            // Tabela de Tokens (DataGridView)
            var pnlTokens = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 8, 8, 0) };
            var lblTokensTitle = new Label
            {
                Text = "Tokens Reconhecidos (Saída: <Lexema, Token>):",
                Dock = DockStyle.Top,
                Height = 26,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            _gridTokens = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.Fixed3D
            };
            var colIndex = new DataGridViewTextBoxColumn { Name = "colIndex", HeaderText = "#", Width = 45, FillWeight = 15 };
            var colLinha = new DataGridViewTextBoxColumn { Name = "colLinha", HeaderText = "Linha", Width = 55, FillWeight = 18 };
            var colColuna = new DataGridViewTextBoxColumn { Name = "colColuna", HeaderText = "Col", Width = 45, FillWeight = 15 };
            var colLexema = new DataGridViewTextBoxColumn { Name = "colLexema", HeaderText = "Lexema", Width = 140, FillWeight = 40 };
            var colToken = new DataGridViewTextBoxColumn { Name = "colToken", HeaderText = "Token Reconhecido", Width = 180, FillWeight = 50 };
            _gridTokens.Columns.AddRange(new DataGridViewColumn[] { colIndex, colLinha, colColuna, colLexema, colToken });

            pnlTokens.Controls.Add(_gridTokens);
            pnlTokens.Controls.Add(lblTokensTitle);
            splitRight.Panel1.Controls.Add(pnlTokens);

            // Painel de Erros Léxicos
            var pnlErrors = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 8, 8) };
            var lblErrorsTitle = new Label
            {
                Text = "Diagnósticos e Erros Léxicos:",
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };
            _lstErrors = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9.0f, FontStyle.Regular),
                ForeColor = Color.DarkRed,
                BackColor = Color.FromArgb(255, 248, 248)
            };
            pnlErrors.Controls.Add(_lstErrors);
            pnlErrors.Controls.Add(lblErrorsTitle);
            splitRight.Panel2.Controls.Add(pnlErrors);

            splitMain.Panel2.Controls.Add(splitRight);
            Controls.Add(splitMain);

            // Barra de Status Inferior
            _statusStrip = new StatusStrip();
            _lblStatus = new ToolStripStatusLabel("Pronto. Digite o código ou abra um arquivo .txt.") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            _lblCounts = new ToolStripStatusLabel("Tokens: 0 | Erros: 0");
            _statusStrip.Items.AddRange(new ToolStripItem[] { _lblStatus, _lblCounts });
            Controls.Add(_statusStrip);

            // Dialogs
            _openFileDialog = new OpenFileDialog
            {
                Filter = "Arquivos de Texto (*.txt)|*.txt|Todos os Arquivos (*.*)|*.*",
                Title = "Selecione o arquivo de código-fonte Mini_Pascal"
            };

            _saveFileDialog = new SaveFileDialog
            {
                Filter = "Arquivo de Texto (*.txt)|*.txt",
                Title = "Salvar arquivo de saída de tokens",
                FileName = "saida_tokens.txt"
            };

            // Carrega o exemplo padrão 1 na inicialização para facilidade do usuário
            LoadSample(1);
        }

        private void OnOpenFileClicked(object? sender, EventArgs e)
        {
            if (_openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _txtSource.Text = File.ReadAllText(_openFileDialog.FileName, Encoding.UTF8);
                    _lblStatus.Text = $"Arquivo carregado: {Path.GetFileName(_openFileDialog.FileName)}";
                    ExecuteLexicalAnalysis();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao abrir arquivo:\n{ex.Message}", "Erro de Leitura", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnSaveOutputClicked(object? sender, EventArgs e)
        {
            if (_currentTokens.Count == 0)
            {
                MessageBox.Show("Não há tokens para salvar. Execute a análise léxica primeiro.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sb = new StringBuilder();
                    foreach (var token in _currentTokens)
                    {
                        sb.AppendLine(token.ToOutputLine());
                    }
                    File.WriteAllText(_saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Arquivo salvo com sucesso em:\n{_saveFileDialog.FileName}", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _lblStatus.Text = $"Saída salva em: {Path.GetFileName(_saveFileDialog.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao salvar arquivo:\n{ex.Message}", "Erro de Escrita", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OnAnalyzeClicked(object? sender, EventArgs e)
        {
            ExecuteLexicalAnalysis();
        }

        private void OnClearClicked(object? sender, EventArgs e)
        {
            _txtSource.Clear();
            _gridTokens.Rows.Clear();
            _lstErrors.Items.Clear();
            _currentTokens.Clear();
            _lblStatus.Text = "Campos limpos.";
            _lblCounts.Text = "Tokens: 0 | Erros: 0";
        }

        private void ExecuteLexicalAnalysis()
        {
            string code = _txtSource.Text;
            _gridTokens.Rows.Clear();
            _lstErrors.Items.Clear();
            _currentTokens.Clear();

            if (string.IsNullOrWhiteSpace(code))
            {
                _lblStatus.Text = "Código-fonte vazio.";
                _lblCounts.Text = "Tokens: 0 | Erros: 0";
                return;
            }

            try
            {
                var lexer = new AfdLexer(code);
                _currentTokens = lexer.TokenizeAll();

                int tokenCount = 0;
                int errorCount = 0;

                for (int i = 0; i < _currentTokens.Count; i++)
                {
                    var t = _currentTokens[i];
                    if (t.Tipo == TokenType.ErroLexico)
                    {
                        errorCount++;
                        _lstErrors.Items.Add($"[Linha {t.Linha}, Coluna {t.Coluna}] {t.MensagemErro}");
                    }
                    else
                    {
                        tokenCount++;
                    }

                    int rowIndex = _gridTokens.Rows.Add(i + 1, t.Linha, t.Coluna, t.Lexema, t.Tipo.ToFriendlyString());

                    if (t.Tipo == TokenType.ErroLexico)
                    {
                        _gridTokens.Rows[rowIndex].DefaultCellStyle.BackColor = Color.MistyRose;
                        _gridTokens.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                    else if (t.Tipo == TokenType.PalavraReservada)
                    {
                        _gridTokens.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkBlue;
                    }
                }

                if (errorCount == 0)
                {
                    _lblStatus.Text = "Análise concluída com sucesso! Nenhum erro léxico encontrado.";
                    _lstErrors.Items.Add("✓ Nenhum erro léxico detectado no código.");
                }
                else
                {
                    _lblStatus.Text = $"Análise concluída com {errorCount} erro(s) léxico(s).";
                }

                _lblCounts.Text = $"Tokens Válidos: {tokenCount} | Erros: {errorCount}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro durante a execução do autômato:\n{ex.Message}", "Falha de Execução", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSample(int index)
        {
            switch (index)
            {
                case 1:
                    _txtSource.Text = @"program teste;
var x,y: integer;
const pi := 3.1416;
/* inicio do programa */
begin
read(x);
if (x > y) then
y := x ;
else
y := -x;
writeln(x);
write(y);
end.";
                    break;

                case 2:
                    _txtSource.Text = @"/* Outro exemplo de programa */
Program Piloto;
 /* declarações de variáveis e constantes globais */
 var cont, total: integer ;
Nota1, Nota2, Media_das_medias, med: real;
/* Inicio do Programa */
begin
media_das_medias := 0;
writeln(""******** ENTRADA DE DADOS ***************"");
writeln(""Digite o total de alunos"");
read(total);
for cont=1 to total do
begin
writeln(""Digite os valores da primeira nota do aluno "", cont);
read(Nota1);
writeln(""Digite os valores da segunda nota do aluno "", cont);
read(Nota2);
med := (Nota1+Nota2)/2.0;
media_das_medias := media_das_medias + med;
write(""Media = "",med);
end;
write(""Media Geral = "", Media_das_medias/total);
end.";
                    break;

                case 3:
                    _txtSource.Text = @"program Estruturas;
var a, b: integer;
c: char;
s: string;
begin
c := 'k';
s := ""Mensagem de teste"";
while (a < 10) do
begin
a := a + 1;
end;
repeat
b := b - 1;
until (b = 0 or not a);
end.";
                    break;

                case 4:
                    _txtSource.Text = @"program Operadores;
var res, x, y: real;
cond: integer;
begin
res := 24.40e-04;
if (x <= y and x <> 0) then
res := (x * y) / 2.5 + (x mod 3);
end.";
                    break;

                case 5:
                    _txtSource.Text = @"program Erros;
var x: integer;
/* comentario de bloco nao fechado
begin
x := 10 @ 20;
s := ""string nao fechada
c := 'mais_de_um_char';
end.";
                    break;
            }
            ExecuteLexicalAnalysis();
        }

        private void OnAboutClicked(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Analisador Léxico para a linguagem Mini_Pascal\n" +
                "Disciplina: Compiladores (2026.2)\n" +
                "Professor: José Carlos Martins Oliveira\n" +
                "Discente: Guilherme Dias Sousa\n" +
                "Universidade Estadual do Sudoeste da Bahia - UESB\n\n" +
                "Implementação baseada em Autômato Finito Determinístico (AFD).\n" +
                "Desenvolvido em C# (.NET 10 Windows Forms).",
                "Sobre o Compilador",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
