namespace ShannonEntropyLab
{
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using ClosedXML.Excel;

    public partial class Form1 : Form
    {
        private static readonly HttpClient s_http = new() { Timeout = TimeSpan.FromSeconds(120) };
        private OpenAiSettings _aiSettings = OpenAiSettings.Load();

        public Form1()
        {
            InitializeComponent();
            btnCalc.Click += BtnCalc_Click;
            txtInput.KeyDown += TxtInput_KeyDown;
            menuFileOpen.Click += (_, _) => OpenBinaryFile();
            menuFileExit.Click += (_, _) => Close();
            menuEditClear.Click += MenuEditClear_Click;
            menuToolsGenerate.Click += (_, _) => ShowGeneratorDialog();
            menuToolsSlidingWindow.Click += (_, _) => ShowSlidingWindowEntropyDialog();
            menuToolsOpenAiSettings.Click += (_, _) => ShowOpenAiSettingsDialog();
            menuToolsAiChat.Click += (_, _) => ShowAiChatDialog();
            menuHelpUsage.Click += (_, _) => ShowUsageDialog();
            menuHelpAbout.Click += (_, _) => ShowAboutDialog();
        }

        private void TxtInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e is { Control: true, KeyCode: Keys.Enter })
            {
                e.SuppressKeyPress = true;
                Calculate();
            }
        }

        private void BtnCalc_Click(object? sender, EventArgs e) => Calculate();

        private void Calculate()
        {
            var input = txtInput.Text;

            if (string.IsNullOrEmpty(input))
            {
                lblEntropy.Text = "---";
                lblStrength.Text = "";
                lblStats.Text = "文字数: 0　　ユニーク文字数: 0　　最大エントロピー: 0.000";
                lblFreqDetails.Text = "";
                lblEntropyBarFill.Width = 0;
                return;
            }

            try
            {
                int length = input.Length;
                var freq = new Dictionary<char, int>();
                foreach (char c in input)
                {
                    freq.TryGetValue(c, out int count);
                    freq[c] = count + 1;
                }

                int uniqueCount = freq.Count;
                double maxEntropy = uniqueCount > 1 ? Math.Log2(uniqueCount) : 0;

                double entropy = 0.0;
                foreach (var kvp in freq)
                {
                    double p = (double)kvp.Value / length;
                    if (p > 0)
                        entropy -= p * Math.Log2(p);
                }

                lblEntropy.Text = entropy.ToString("F4");

                // Strength rating
                var (label, barColor) = entropy switch
                {
                    < 1.5 => ("⚠ 非常に低い", Color.FromArgb(239, 68, 68)),
                    < 2.5 => ("△ 低い", Color.FromArgb(251, 146, 60)),
                    < 3.5 => ("○ 中程度", Color.FromArgb(250, 204, 21)),
                    < 4.5 => ("◎ 高い", Color.FromArgb(74, 222, 128)),
                    _     => ("★ 非常に高い", Color.FromArgb(34, 211, 238))
                };
                lblStrength.Text = label;
                lblStrength.ForeColor = barColor;

                // Entropy bar (max ~8 bits for display)
                double ratio = Math.Clamp(entropy / 8.0, 0, 1);
                lblEntropyBarFill.Width = (int)(lblEntropyBar.Width * ratio);
                lblEntropyBarFill.BackColor = barColor;

                // Stats
                lblStats.Text = $"文字数: {length}　　ユニーク文字数: {uniqueCount}　　最大エントロピー: {maxEntropy:F3}";

                // Frequency details (sorted desc)
                var sorted = freq.OrderByDescending(kvp => kvp.Value);
                var lines = new StringBuilder();
                foreach (var kvp in sorted)
                {
                    double p = (double)kvp.Value / length * 100;
                    string display = kvp.Key switch
                    {
                        ' '  => "[SP]",
                        '\t' => "[TAB]",
                        '\r' => "[CR]",
                        '\n' => "[LF]",
                        _    => kvp.Key.ToString()
                    };
                    int bar = (int)(p / 2);
                    lines.AppendLine($"{display,-5} {kvp.Value,4}回  {p,6:F1}%  {"".PadLeft(bar, '█')}");
                }
                lblFreqDetails.Text = lines.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                lblEntropy.Text = "---";
                lblStrength.Text = "";
                MessageBox.Show(this,
                    $"エントロピー計算中にエラーが発生しました。\n\n{ex.Message}",
                    "計算エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MenuEditClear_Click(object? sender, EventArgs e)
        {
            txtInput.Clear();
            txtInput.Focus();
            lblEntropy.Text = "---";
            lblStrength.Text = "";
            lblStats.Text = "文字数: 0　　ユニーク文字数: 0　　最大エントロピー: 0.000";
            lblFreqDetails.Text = "";
            lblEntropyBarFill.Width = 0;
        }

        private void OpenBinaryFile()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "バイナリファイルを開く — Shannon Entropy Lab",
                Filter = "すべてのファイル (*.*)|*.*|" +
                         "実行ファイル (*.exe;*.dll)|*.exe;*.dll|" +
                         "画像ファイル (*.png;*.jpg;*.bmp;*.gif)|*.png;*.jpg;*.bmp;*.gif|" +
                         "アーカイブ (*.zip;*.7z;*.tar;*.gz)|*.zip;*.7z;*.tar;*.gz",
                FilterIndex = 1
            };

            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            byte[] data;
            try
            {
                var fi = new FileInfo(ofd.FileName);
                if (fi.Length > 500_000_000)
                {
                    var result = MessageBox.Show(this,
                        $"ファイルサイズが {fi.Length / 1_048_576.0:F1} MB と非常に大きいため、\n" +
                        "メモリ不足になる可能性があります。続行しますか？",
                        "大きなファイル", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result != DialogResult.Yes) return;
                }
                data = File.ReadAllBytes(ofd.FileName);
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show(this,
                    "ファイルが大きすぎてメモリに読み込めませんでした。\n" +
                    "スライディングウィンドウ解析をお試しください。",
                    "メモリ不足", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(this,
                    "ファイルへのアクセスが拒否されました。\n読み取り権限を確認してください。",
                    "アクセス拒否", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (IOException ex)
            {
                MessageBox.Show(this,
                    $"ファイルの読み込みに失敗しました (I/O エラー)。\n\n{ex.Message}",
                    "I/O エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"ファイルの読み込みに失敗しました。\n\n{ex.GetType().Name}: {ex.Message}",
                    "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (data.Length == 0)
            {
                MessageBox.Show(this,
                    "ファイルが空です (0 バイト)。",
                    "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Show file path in the text box
            string fileName = Path.GetFileName(ofd.FileName);
            string sizeText = data.Length switch
            {
                >= 1_073_741_824 => $"{data.Length / 1_073_741_824.0:F2} GB",
                >= 1_048_576     => $"{data.Length / 1_048_576.0:F2} MB",
                >= 1_024         => $"{data.Length / 1_024.0:F2} KB",
                _                => $"{data.Length} bytes"
            };
            txtInput.Text = $"[バイナリファイル] {ofd.FileName}\r\nサイズ: {sizeText}";

            try
            {
                // Byte-level frequency count (0-255)
                var freq = new int[256];
                foreach (byte b in data)
                    freq[b]++;

                int uniqueCount = 0;
                foreach (int f in freq)
                    if (f > 0) uniqueCount++;

                double maxEntropy = uniqueCount > 1 ? Math.Log2(uniqueCount) : 0;

                // Shannon entropy on bytes
                double entropy = 0.0;
                double len = data.Length;
                for (int i = 0; i < 256; i++)
                {
                    if (freq[i] == 0) continue;
                    double p = freq[i] / len;
                    entropy -= p * Math.Log2(p);
                }

                lblEntropy.Text = entropy.ToString("F4");

                // Strength rating (byte entropy max = 8.0)
                var (label, barColor) = entropy switch
                {
                    < 3.0 => ("⚠ 非常に低い", Color.FromArgb(239, 68, 68)),
                    < 5.0 => ("△ 低い",       Color.FromArgb(251, 146, 60)),
                    < 6.5 => ("○ 中程度",     Color.FromArgb(250, 204, 21)),
                    < 7.5 => ("◎ 高い",       Color.FromArgb(74, 222, 128)),
                    _     => ("★ 非常に高い", Color.FromArgb(34, 211, 238))
                };
                lblStrength.Text = label;
                lblStrength.ForeColor = barColor;

                // Entropy bar (max 8 bits for bytes)
                double ratio = Math.Clamp(entropy / 8.0, 0, 1);
                lblEntropyBarFill.Width = (int)(lblEntropyBar.Width * ratio);
                lblEntropyBarFill.BackColor = barColor;

                // Stats
                lblStats.Text = $"ファイル: {fileName}　　サイズ: {sizeText}　　" +
                                $"ユニークバイト数: {uniqueCount}/256　　最大エントロピー: {maxEntropy:F3}";

                // Byte frequency details (top 40, sorted desc)
                var sorted = Enumerable.Range(0, 256)
                    .Where(i => freq[i] > 0)
                    .OrderByDescending(i => freq[i])
                    .Take(40);

                var sb = new StringBuilder();
                foreach (int b in sorted)
                {
                    double pct = freq[b] / len * 100;
                    string hex = $"0x{b:X2}";
                    string ch = b is >= 0x20 and <= 0x7E ? $"'{(char)b}'" : "   ";
                    int bar = (int)(pct / 2);
                    sb.AppendLine($"{hex} {ch}  {freq[b],8}回  {pct,6:F1}%  {"".PadLeft(bar, '█')}");
                }
                if (uniqueCount > 40)
                    sb.AppendLine($"\n… 他 {uniqueCount - 40} 種のバイト値");

                lblFreqDetails.Text = sb.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $"バイナリ解析中にエラーが発生しました。\n\n{ex.Message}",
                    "解析エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowUsageDialog()
        {
            var bgColor = Color.FromArgb(30, 30, 42);
            var textColor = Color.FromArgb(230, 230, 240);
            var accentColor = Color.FromArgb(99, 102, 241);

            using var dlg = new Form
            {
                Text = "使い方 — Shannon Entropy Lab",
                ClientSize = new Size(620, 640),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = bgColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                AutoScaleMode = AutoScaleMode.Font
            };

            var lbl = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Padding = new Padding(32, 28, 32, 28),
                Font = new Font("Segoe UI", 10F),
                ForeColor = textColor,
                BackColor = bgColor,
                Text =
                    "■ シャノンエントロピーとは\n" +
                    "━━━━━━━━━━━━━━━━━━━━\n" +
                    "情報理論の父 クロード・シャノンが定義した、\n" +
                    "データに含まれる \"情報量\" の尺度です。\n" +
                    "単位は bits/char で表されます。\n\n" +
                    "  H = − Σ p(x) × log₂ p(x)\n\n" +
                    "■ エントロピーが高い = ランダム性が高い\n" +
                    "━━━━━━━━━━━━━━━━━━━━\n" +
                    "エントロピーが高いほど、次の文字を予測しにくく、\n" +
                    "文字の出現が均等に分布していることを意味します。\n\n" +
                    "  ⚠ < 1.5  … 非常に低い (例: 'aaaa')\n" +
                    "  △ < 2.5  … 低い\n" +
                    "  ○ < 3.5  … 中程度\n" +
                    "  ◎ < 4.5  … 高い\n" +
                    "  ★ ≥ 4.5  … 非常に高い (ランダム文字列)\n\n" +
                    "■ セキュリティ観点での活用\n" +
                    "━━━━━━━━━━━━━━━━━━━━\n" +
                    "パスワードやトークンの強度評価、暗号化データの\n" +
                    "ランダム性検証、難読化コードの検出などに\n" +
                    "エントロピー計測は広く利用されています。\n\n" +
                    "■ 使い方\n" +
                    "━━━━━━━━━━━━━━━━━━━━\n" +
                    "1. テキストボックスに文字列を入力\n" +
                    "2. [⚡ エントロピーを算出] または Ctrl+Enter\n" +
                    "3. エントロピー・強度・文字頻度が表示されます"
            };

            var btnOk = new Button
            {
                Text = "OK",
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(0, 48),
                FlatStyle = FlatStyle.Flat,
                BackColor = accentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;

            dlg.Controls.Add(lbl);
            dlg.Controls.Add(btnOk);
            dlg.AcceptButton = btnOk;
            dlg.ShowDialog(this);
        }

        private void ShowAboutDialog()
        {
            var bgColor = Color.FromArgb(30, 30, 42);
            var textColor = Color.FromArgb(230, 230, 240);
            var accentColor = Color.FromArgb(99, 102, 241);
            var dimColor = Color.FromArgb(140, 144, 164);

            using var dlg = new Form
            {
                Text = "About — Shannon Entropy Lab",
                ClientSize = new Size(540, 420),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = bgColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                AutoScaleMode = AutoScaleMode.Font
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = bgColor,
                Padding = new Padding(36, 28, 36, 16)
            };

            var lblIcon = new Label
            {
                Text = "🔐  Shannon Entropy Lab",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = textColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 12),
                BackColor = Color.Transparent
            };

            var lblVer = new Label
            {
                Text = "Version 1.0.0　　.NET 10",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = dimColor,
                AutoSize = true,
                Margin = new Padding(2, 0, 0, 24),
                BackColor = Color.Transparent
            };

            var lblDesc = new Label
            {
                Text =
                    "入力文字列のシャノンエントロピーを算出し、\n" +
                    "文字の出現頻度とランダム性の度合いを\n" +
                    "視覚的に表示する LLM セキュリティ向けツールです。",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = textColor,
                AutoSize = true,
                Margin = new Padding(2, 0, 0, 32),
                BackColor = Color.Transparent
            };

            var lblCopy = new Label
            {
                Text = $"© {DateTime.Now.Year} Shannon Entropy Lab",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = dimColor,
                AutoSize = true,
                Margin = new Padding(2, 0, 0, 0),
                BackColor = Color.Transparent
            };

            flow.Controls.AddRange([lblIcon, lblVer, lblDesc, lblCopy]);

            var btnOk = new Button
            {
                Text = "OK",
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(0, 48),
                FlatStyle = FlatStyle.Flat,
                BackColor = accentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                DialogResult = DialogResult.OK
            };
            btnOk.FlatAppearance.BorderSize = 0;

            dlg.Controls.Add(flow);
            dlg.Controls.Add(btnOk);
            dlg.AcceptButton = btnOk;
            dlg.ShowDialog(this);
        }

        // =====================================================
        // 高エントロピー文字列生成 & Excel 出力
        // =====================================================

        private static double CalcEntropy(string s)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            var freq = new Dictionary<char, int>();
            foreach (char c in s)
            {
                freq.TryGetValue(c, out int n);
                freq[c] = n + 1;
            }
            double h = 0;
            foreach (var kvp in freq)
            {
                double p = (double)kvp.Value / s.Length;
                if (p > 0) h -= p * Math.Log2(p);
            }
            return h;
        }

        private static string GetStrengthLabel(double entropy) => entropy switch
        {
            < 1.5 => "⚠ 非常に低い",
            < 2.5 => "△ 低い",
            < 3.5 => "○ 中程度",
            < 4.5 => "◎ 高い",
            _     => "★ 非常に高い"
        };

        private const string CharPool =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" +
            "!@#$%^&*()-_=+[]{}|;:',.<>?/`~";

        private static string GenerateHighEntropyString(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "文字列長は1以上を指定してください。");

            var buf = new char[length];
            // stackalloc は小さいサイズのみ使用（スタックオーバーフロー防止）
            byte[] rndArray = length <= 1024 ? null! : new byte[length];
            Span<byte> rnd = length <= 1024 ? stackalloc byte[length] : rndArray;
            RandomNumberGenerator.Fill(rnd);
            for (int i = 0; i < length; i++)
                buf[i] = CharPool[rnd[i] % CharPool.Length];
            return new string(buf);
        }

        private void ShowGeneratorDialog()
        {
            var bgColor = Color.FromArgb(24, 24, 32);
            var panelColor = Color.FromArgb(32, 34, 46);
            var textColor = Color.FromArgb(230, 230, 240);
            var dimColor = Color.FromArgb(140, 144, 164);
            var accentColor = Color.FromArgb(99, 102, 241);
            var inputBgColor = Color.FromArgb(40, 42, 56);
            var greenColor = Color.FromArgb(34, 197, 94);

            using var dlg = new Form
            {
                Text = "高エントロピー文字列を生成 — Shannon Entropy Lab",
                ClientSize = new Size(800, 760),
                FormBorderStyle = FormBorderStyle.Sizable,
                StartPosition = FormStartPosition.CenterParent,
                MinimumSize = new Size(650, 500),
                BackColor = bgColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                AutoScaleMode = AutoScaleMode.Font
            };

            // -- Header (Dock.Top, AutoSize) --
            var panelHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                BackColor = bgColor,
                Padding = new Padding(28, 18, 28, 12)
            };
            panelHeader.Controls.Add(new Label
            {
                Text = "🎲 高エントロピー文字列ジェネレーター",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = textColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent
            });
            panelHeader.Controls.Add(new Label
            {
                Text = "暗号学的に安全な乱数で高エントロピー文字列を一括生成し、Excel に出力します",
                Font = new Font("Segoe UI", 9F),
                ForeColor = dimColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0),
                BackColor = Color.Transparent
            });

            // -- Config (Dock.Top, AutoSize) --
            var panelConfig = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(0, 60),
                BackColor = panelColor,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(20, 14, 20, 14)
            };

            var lblCount = new Label
            {
                Text = "生成数:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = dimColor,
                AutoSize = true,
                Margin = new Padding(0, 8, 6, 4),
                BackColor = Color.Transparent
            };
            var nudCount = new NumericUpDown
            {
                Minimum = 1, Maximum = 30, Value = 10,
                Size = new Size(72, 32),
                BackColor = inputBgColor, ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 4, 24, 4)
            };
            var lblLen = new Label
            {
                Text = "文字列長:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = dimColor,
                AutoSize = true,
                Margin = new Padding(0, 8, 6, 4),
                BackColor = Color.Transparent
            };
            var nudLength = new NumericUpDown
            {
                Minimum = 8, Maximum = 256, Value = 48, Increment = 8,
                Size = new Size(80, 32),
                BackColor = inputBgColor, ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 4, 32, 4)
            };
            var btnGenerate = new Button
            {
                Text = "⚡ 生成",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(100, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = accentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 12, 4)
            };
            btnGenerate.FlatAppearance.BorderSize = 0;

            var btnExport = new Button
            {
                Text = "📊 Excel出力",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(130, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = greenColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false,
                Margin = new Padding(0, 2, 0, 4)
            };
            btnExport.FlatAppearance.BorderSize = 0;

            panelConfig.Controls.AddRange([lblCount, nudCount, lblLen, nudLength, btnGenerate, btnExport]);

            // -- Status (Dock.Bottom) --
            var lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9F),
                ForeColor = dimColor,
                Dock = DockStyle.Bottom,
                AutoSize = false,
                Height = 40,
                Padding = new Padding(28, 10, 0, 4),
                BackColor = bgColor
            };

            // -- DataGridView (Dock.Fill) --
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = panelColor,
                GridColor = Color.FromArgb(50, 54, 72),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = panelColor,
                    ForeColor = textColor,
                    SelectionBackColor = accentColor,
                    SelectionForeColor = Color.White,
                    Font = new Font("Consolas", 9.5F)
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(40, 42, 56),
                    ForeColor = textColor,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ScrollBars = ScrollBars.Both
            };

            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "No.", FillWeight = 8 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "生成文字列", FillWeight = 55 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "文字数", FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "エントロピー", FillWeight = 14 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "強度", FillWeight = 16 });

            // -- Generate handler --
            btnGenerate.Click += (_, _) =>
            {
                try
                {
                    dgv.Rows.Clear();
                    int count = (int)nudCount.Value;
                    int len = (int)nudLength.Value;

                    for (int i = 0; i < count; i++)
                    {
                        string s = GenerateHighEntropyString(len);
                        double h = CalcEntropy(s);
                        dgv.Rows.Add(i + 1, s, len, h.ToString("F4"), GetStrengthLabel(h));
                    }

                    btnExport.Enabled = dgv.Rows.Count > 0;
                    lblStatus.Text = $"{count} 個の高エントロピー文字列を生成しました";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(dlg,
                        $"文字列の生成中にエラーが発生しました。\n\n{ex.Message}",
                        "生成エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            // -- Excel export handler --
            btnExport.Click += (_, _) =>
            {
                using var sfd = new SaveFileDialog
                {
                    Title = "Excel ファイルの保存先を選択",
                    Filter = "Excel ファイル (*.xlsx)|*.xlsx",
                    FileName = $"HighEntropyStrings_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (sfd.ShowDialog(dlg) != DialogResult.OK) return;

                try
                {
                    using var wb = new XLWorkbook();
                    var ws = wb.AddWorksheet("HighEntropyStrings");

                    // Title
                    ws.Cell(1, 1).Value = "Shannon Entropy Lab — 高エントロピー文字列レポート";
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 14;
                    ws.Range(1, 1, 1, 5).Merge();

                    ws.Cell(2, 1).Value = $"生成日時: {DateTime.Now:yyyy/MM/dd HH:mm:ss}　　文字列長: {(int)nudLength.Value}　　生成数: {dgv.Rows.Count}";
                    ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
                    ws.Range(2, 1, 2, 5).Merge();

                    // Header
                    int headerRow = 4;
                    string[] headers = ["No.", "生成文字列", "文字数", "エントロピー (bits/char)", "強度"];
                    for (int c = 0; c < headers.Length; c++)
                    {
                        var cell = ws.Cell(headerRow, c + 1);
                        cell.Value = headers[c];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.FromArgb(99, 102, 241);
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    }

                    // Data rows
                    double totalEntropy = 0;
                    for (int r = 0; r < dgv.Rows.Count; r++)
                    {
                        int row = headerRow + 1 + r;
                        var dgvRow = dgv.Rows[r];
                        ws.Cell(row, 1).Value = (int)dgvRow.Cells[0].Value;
                        ws.Cell(row, 2).Value = (string)dgvRow.Cells[1].Value;
                        ws.Cell(row, 3).Value = (int)dgvRow.Cells[2].Value;

                        double h = double.Parse((string)dgvRow.Cells[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                        ws.Cell(row, 4).Value = h;
                        ws.Cell(row, 4).Style.NumberFormat.Format = "0.0000";
                        totalEntropy += h;

                        ws.Cell(row, 5).Value = (string)dgvRow.Cells[4].Value;

                        // Alternate row color
                        if (r % 2 == 1)
                        {
                            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(245, 245, 250);
                        }

                        // Center align columns 1,3,4
                        ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    // Summary
                    int sumRow = headerRow + dgv.Rows.Count + 2;
                    ws.Cell(sumRow, 1).Value = "サマリー";
                    ws.Cell(sumRow, 1).Style.Font.Bold = true;
                    ws.Range(sumRow, 1, sumRow, 2).Merge();

                    ws.Cell(sumRow + 1, 1).Value = "平均エントロピー:";
                    ws.Cell(sumRow + 1, 1).Style.Font.Bold = true;
                    ws.Range(sumRow + 1, 1, sumRow + 1, 2).Merge();
                    ws.Cell(sumRow + 1, 3).Value = dgv.Rows.Count > 0 ? totalEntropy / dgv.Rows.Count : 0;
                    ws.Cell(sumRow + 1, 3).Style.NumberFormat.Format = "0.0000";
                    ws.Cell(sumRow + 1, 3).Style.Font.Bold = true;
                    ws.Cell(sumRow + 1, 3).Style.Font.FontColor = XLColor.FromArgb(99, 102, 241);

                    ws.Cell(sumRow + 2, 1).Value = "使用文字プール:";
                    ws.Range(sumRow + 2, 1, sumRow + 2, 2).Merge();
                    ws.Cell(sumRow + 2, 3).Value = $"{CharPool.Length} 種類 (英大小文字+数字+記号)";
                    ws.Range(sumRow + 2, 3, sumRow + 2, 5).Merge();

                    ws.Cell(sumRow + 3, 1).Value = "理論最大エントロピー:";
                    ws.Range(sumRow + 3, 1, sumRow + 3, 2).Merge();
                    ws.Cell(sumRow + 3, 3).Value = Math.Log2(CharPool.Length);
                    ws.Cell(sumRow + 3, 3).Style.NumberFormat.Format = "0.0000";

                    // Column widths
                    ws.Column(1).Width = 8;
                    ws.Column(2).Width = 58;
                    ws.Column(3).Width = 10;
                    ws.Column(4).Width = 22;
                    ws.Column(5).Width = 18;

                    // Borders for data area
                    var dataRange = ws.Range(headerRow, 1, headerRow + dgv.Rows.Count, 5);
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorderColor = XLColor.FromArgb(220, 220, 230);
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(180, 180, 200);

                    wb.SaveAs(sfd.FileName);

                    lblStatus.Text = $"✅ Excel に保存しました: {Path.GetFileName(sfd.FileName)}";

                    if (MessageBox.Show(dlg,
                            $"Excel ファイルを保存しました。\n{sfd.FileName}\n\nファイルを開きますか？",
                            "保存完了",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = sfd.FileName,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception exOpen)
                        {
                            MessageBox.Show(dlg,
                                $"ファイルを開けませんでした。\n\n{exOpen.Message}",
                                "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(dlg,
                        $"Excel の保存に失敗しました。\n\n{ex.Message}",
                        "エラー",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };

            dlg.Controls.Add(dgv);
            dlg.Controls.Add(panelConfig);
            dlg.Controls.Add(panelHeader);
            dlg.Controls.Add(lblStatus);
            dlg.ShowDialog(this);
        }

        // =====================================================
        // OpenAI 接続設定ダイアログ
        // =====================================================

        private void ShowOpenAiSettingsDialog()
        {
            var bgColor = Color.FromArgb(24, 24, 32);
            var panelColor = Color.FromArgb(32, 34, 46);
            var textColor = Color.FromArgb(230, 230, 240);
            var dimColor = Color.FromArgb(140, 144, 164);
            var accentColor = Color.FromArgb(99, 102, 241);
            var inputBgColor = Color.FromArgb(40, 42, 56);

            using var dlg = new Form
            {
                Text = "OpenAI 接続設定 — Shannon Entropy Lab",
                ClientSize = new Size(680, 580),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = bgColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                AutoScaleMode = AutoScaleMode.Font
            };

            // -- Header (Dock.Top, AutoSize) --
            var panelHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                BackColor = bgColor,
                Padding = new Padding(28, 18, 28, 14)
            };
            panelHeader.Controls.Add(new Label
            {
                Text = "🤖 Azure OpenAI 接続設定",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = textColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent
            });
            panelHeader.Controls.Add(new Label
            {
                Text = "Azure OpenAI Service のエンドポイント情報を設定します",
                Font = new Font("Segoe UI", 9F),
                ForeColor = dimColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0),
                BackColor = Color.Transparent
            });

            // -- Form fields (TableLayoutPanel, Dock.Fill) --
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                BackColor = bgColor,
                Padding = new Padding(24, 16, 24, 16),
                AutoScroll = true
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Endpoint
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // hint
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // API Key
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // API Version
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Deployment

            Label MakeLabel(string text) => new()
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = dimColor,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(4, 14, 16, 14),
                BackColor = Color.Transparent
            };

            TextBox MakeInput(string val, bool password = false) => new()
            {
                Text = val,
                Dock = DockStyle.Fill,
                BackColor = inputBgColor,
                ForeColor = textColor,
                Font = new Font("Consolas", 10F),
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = password,
                Margin = new Padding(0, 12, 8, 12)
            };

            var txtEndpoint = MakeInput(_aiSettings.Endpoint);
            var txtApiKey = MakeInput(_aiSettings.ApiKey, password: true);
            var txtApiVer = MakeInput(_aiSettings.ApiVersion);
            var txtDeploy = MakeInput(_aiSettings.DeploymentName);

            table.Controls.Add(MakeLabel("エンドポイント:"), 0, 0);
            table.Controls.Add(txtEndpoint, 1, 0);

            var lblHint = new Label
            {
                Text = "例: https://your-resource.openai.azure.com/",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(100, 104, 124),
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 2, 0, 8),
                BackColor = Color.Transparent
            };
            table.Controls.Add(new Label { AutoSize = true }, 0, 1); // spacer
            table.Controls.Add(lblHint, 1, 1);

            table.Controls.Add(MakeLabel("API キー:"), 0, 2);
            table.Controls.Add(txtApiKey, 1, 2);
            table.Controls.Add(MakeLabel("API バージョン:"), 0, 3);
            table.Controls.Add(txtApiVer, 1, 3);
            table.Controls.Add(MakeLabel("デプロイ名:"), 0, 4);
            table.Controls.Add(txtDeploy, 1, 4);

            // -- Buttons (Dock.Bottom, AutoSize) --
            var panelButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(0, 64),
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = panelColor,
                Padding = new Padding(16, 12, 16, 12)
            };

            var btnCancel = new Button
            {
                Text = "キャンセル",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(120, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 58, 74),
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            var btnSave = new Button
            {
                Text = "💾 保存",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(120, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = accentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
            };
            btnSave.FlatAppearance.BorderSize = 0;

            var btnTest = new Button
            {
                Text = "🔌 接続テスト",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(150, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(34, 197, 94),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTest.FlatAppearance.BorderSize = 0;

            panelButtons.Controls.AddRange([btnCancel, btnSave, btnTest]);

            btnTest.Click += async (_, _) =>
            {
                btnTest.Enabled = false;
                btnTest.Text = "接続中...";
                try
                {
                    var settings = new OpenAiSettings
                    {
                        Endpoint = txtEndpoint.Text.Trim(),
                        ApiKey = txtApiKey.Text.Trim(),
                        ApiVersion = txtApiVer.Text.Trim(),
                        DeploymentName = txtDeploy.Text.Trim()
                    };
                    var reply = await CallAzureOpenAiAsync(settings, "Say OK");
                    MessageBox.Show(dlg, $"✅ 接続成功!\n\n応答: {reply}", "接続テスト", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(dlg, $"❌ 接続に失敗しました。\n\n{ex.Message}", "接続テスト", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnTest.Enabled = true;
                    btnTest.Text = "🔌 接続テスト";
                }
            };

            btnSave.Click += (_, _) =>
            {
                var endpoint = txtEndpoint.Text.Trim();
                var apiKey = txtApiKey.Text.Trim();
                var apiVer = txtApiVer.Text.Trim();
                var deploy = txtDeploy.Text.Trim();

                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    MessageBox.Show(dlg, "エンドポイントを入力してください。",
                        "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtEndpoint.Focus();
                    return;
                }
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    MessageBox.Show(dlg, "API キーを入力してください。",
                        "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtApiKey.Focus();
                    return;
                }

                try
                {
                    _aiSettings.Endpoint = endpoint;
                    _aiSettings.ApiKey = apiKey;
                    _aiSettings.ApiVersion = apiVer;
                    _aiSettings.DeploymentName = deploy;
                    _aiSettings.Save();
                    MessageBox.Show(dlg, "設定を保存しました。", "保存完了", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlg.DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(dlg,
                        $"設定の保存に失敗しました。\n\n{ex.Message}",
                        "保存エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            dlg.Controls.Add(table);
            dlg.Controls.Add(panelHeader);
            dlg.Controls.Add(panelButtons);
            dlg.CancelButton = btnCancel;
            dlg.ShowDialog(this);
        }

        // =====================================================
        // AI チャットダイアログ
        // =====================================================

        private void ShowAiChatDialog()
        {
            if (!_aiSettings.IsConfigured)
            {
                MessageBox.Show(this,
                    "OpenAI の接続設定が完了していません。\n\nツール → OpenAI 接続設定 から設定してください。",
                    "未設定", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var bgColor = Color.FromArgb(24, 24, 32);
            var panelColor = Color.FromArgb(32, 34, 46);
            var textColor = Color.FromArgb(230, 230, 240);
            var dimColor = Color.FromArgb(140, 144, 164);
            var accentColor = Color.FromArgb(99, 102, 241);
            var inputBgColor = Color.FromArgb(40, 42, 56);
            var userBubble = Color.FromArgb(55, 58, 94);
            var aiBubble = Color.FromArgb(38, 42, 58);

            var dlg = new Form
            {
                Text = $"AI チャット ({_aiSettings.DeploymentName}) — Shannon Entropy Lab",
                ClientSize = new Size(760, 680),
                FormBorderStyle = FormBorderStyle.Sizable,
                StartPosition = FormStartPosition.CenterParent,
                MinimumSize = new Size(540, 460),
                BackColor = bgColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                AutoScaleMode = AutoScaleMode.Font
            };

            var lblHeader = new Label
            {
                Text = $"🤖 AI チャット — {_aiSettings.DeploymentName}",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = textColor,
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 52,
                Padding = new Padding(24, 14, 0, 0),
                BackColor = panelColor
            };

            var chatLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = bgColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10.5F),
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };

            var panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 90,
                BackColor = panelColor,
                Padding = new Padding(20, 14, 20, 14)
            };

            var txtPrompt = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                BackColor = inputBgColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Vertical
            };

            var btnSend = new Button
            {
                Text = "送信 ▶",
                Dock = DockStyle.Right,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(110, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = accentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnSend.FlatAppearance.BorderSize = 0;

            panelBottom.Controls.Add(txtPrompt);
            panelBottom.Controls.Add(btnSend);

            // -- Chat history --
            var messages = new List<(string role, string content)>();

            // System message for entropy context
            messages.Add(("system",
                "あなたは情報セキュリティとシャノンエントロピーの専門家です。" +
                "ユーザーの質問に正確かつ分かりやすく回答してください。" +
                "エントロピーの計算やセキュリティ分析に関する質問に特に詳しく答えてください。"));

            void AppendChat(string role, string text)
            {
                try
                {
                    if (chatLog.IsDisposed) return;

                    if (chatLog.TextLength > 0)
                        chatLog.AppendText("\n\n");

                    int start = chatLog.TextLength;
                    string prefix = role == "user" ? "👤 You" : "🤖 AI";
                    chatLog.AppendText($"  {prefix}\n");
                    chatLog.Select(start, chatLog.TextLength - start);
                    chatLog.SelectionColor = role == "user" ? Color.FromArgb(129, 140, 248) : Color.FromArgb(52, 211, 153);
                    chatLog.SelectionFont = new Font("Segoe UI", 9F, FontStyle.Bold);

                    int bodyStart = chatLog.TextLength;
                    chatLog.AppendText($"  {text}");
                    chatLog.Select(bodyStart, chatLog.TextLength - bodyStart);
                    chatLog.SelectionColor = textColor;
                    chatLog.SelectionFont = new Font("Segoe UI", 10.5F);

                    chatLog.SelectionStart = chatLog.TextLength;
                    chatLog.ScrollToCaret();
                }
                catch (ObjectDisposedException) { }
            }

            async Task SendAsync()
            {
                var prompt = txtPrompt.Text.Trim();
                if (string.IsNullOrEmpty(prompt)) return;

                txtPrompt.Clear();
                btnSend.Enabled = false;
                btnSend.Text = "⏳";

                messages.Add(("user", prompt));
                AppendChat("user", prompt);

                try
                {
                    var reply = await CallAzureOpenAiAsync(_aiSettings, messages);
                    messages.Add(("assistant", reply));
                    AppendChat("assistant", reply);
                }
                catch (Exception ex)
                {
                    if (messages.Count > 0 && messages[^1].role == "user")
                        messages.RemoveAt(messages.Count - 1);
                    AppendChat("assistant", $"⚠ エラー: {ex.Message}");
                }
                finally
                {
                    btnSend.Enabled = true;
                    btnSend.Text = "送信 ▶";
                    txtPrompt.Focus();
                }
            }

            btnSend.Click += async (_, _) => await SendAsync();

            txtPrompt.KeyDown += async (_, e) =>
            {
                if (e is { Control: true, KeyCode: Keys.Enter })
                {
                    e.SuppressKeyPress = true;
                    await SendAsync();
                }
            };

            dlg.Controls.Add(chatLog);
            dlg.Controls.Add(lblHeader);
            dlg.Controls.Add(panelBottom);

            AppendChat("assistant",
                "こんにちは！シャノンエントロピーやセキュリティに関する質問をどうぞ。\n" +
                "  Ctrl+Enter で送信できます。");

            dlg.Show(this);
        }

        // =====================================================
        // スライディングウィンドウエントロピー解析
        // =====================================================

        private void ShowSlidingWindowEntropyDialog()
        {
            var bgColor = Color.FromArgb(24, 24, 32);
            var panelColor = Color.FromArgb(32, 34, 46);
            var accentColor = Color.FromArgb(99, 102, 241);
            var textColor = Color.FromArgb(230, 230, 240);
            var dimColor = Color.FromArgb(140, 144, 164);
            var inputBgColor = Color.FromArgb(40, 42, 56);
            var greenColor = Color.FromArgb(34, 197, 94);

            byte[]? fileData = null;
            List<(long offset, double entropy)>? analysisResults = null;
            double maxPossibleH = 8.0;

            var dlg = new Form
            {
                Text = "スライディングウィンドウエントロピー解析 — Shannon Entropy Lab",
                ClientSize = new Size(960, 720),
                FormBorderStyle = FormBorderStyle.Sizable,
                StartPosition = FormStartPosition.CenterParent,
                MinimumSize = new Size(720, 520),
                BackColor = bgColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10F),
                AutoScaleMode = AutoScaleMode.Font
            };

            // -- Header --
            var panelHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                BackColor = bgColor,
                Padding = new Padding(28, 16, 28, 8)
            };
            panelHeader.Controls.Add(new Label
            {
                Text = "📊 スライディングウィンドウエントロピー解析",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = textColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6),
                BackColor = Color.Transparent
            });
            panelHeader.Controls.Add(new Label
            {
                Text = "ファイルの局所的なエントロピー変化を可視化し、暗号化・圧縮・平文領域を検出します",
                Font = new Font("Segoe UI", 9F),
                ForeColor = dimColor,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 0),
                BackColor = Color.Transparent
            });

            // -- Config --
            var panelConfig = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                BackColor = panelColor,
                Padding = new Padding(20, 14, 20, 14)
            };

            // File selection row
            var fileRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 10)
            };
            var btnOpenFile = new Button
            {
                Text = "📂 ファイルを選択",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(160, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = accentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 12, 0)
            };
            btnOpenFile.FlatAppearance.BorderSize = 0;
            var lblFilePath = new Label
            {
                Text = "(ファイル未選択)",
                Font = new Font("Segoe UI", 9F),
                ForeColor = dimColor,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 0),
                BackColor = Color.Transparent
            };
            fileRow.Controls.AddRange([btnOpenFile, lblFilePath]);

            // Parameter row
            var paramRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 0, 0)
            };

            Label MakeParamLabel(string text) => new()
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = dimColor,
                AutoSize = true,
                Margin = new Padding(0, 10, 4, 4),
                BackColor = Color.Transparent
            };

            var nudWindow = new NumericUpDown
            {
                Minimum = 16, Maximum = 65536, Value = 256, Increment = 16,
                Size = new Size(80, 30),
                BackColor = inputBgColor, ForeColor = textColor,
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 6, 16, 4)
            };
            var nudStep = new NumericUpDown
            {
                Minimum = 1, Maximum = 65536, Value = 256, Increment = 1,
                Size = new Size(80, 30),
                BackColor = inputBgColor, ForeColor = textColor,
                Font = new Font("Segoe UI", 9.5F),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 6, 16, 4)
            };
            var cmbSmoothing = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(160, 30),
                BackColor = inputBgColor, ForeColor = textColor,
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 6, 16, 4)
            };
            cmbSmoothing.Items.AddRange(new object[] { "MLE (最尤推定)", "Laplace (ラプラス)" });
            cmbSmoothing.SelectedIndex = 0;

            var cmbUnit = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new Size(180, 30),
                BackColor = inputBgColor, ForeColor = textColor,
                Font = new Font("Segoe UI", 9F),
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 6, 16, 4)
            };
            cmbUnit.Items.AddRange(new object[] { "バイト (256値)", "ビット (0/1)", "バイグラム (2-gram)", "トライグラム (3-gram)" });
            cmbUnit.SelectedIndex = 0;

            var btnAnalyze = new Button
            {
                Text = "▶ 解析開始",
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(140, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = greenColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = false,
                Margin = new Padding(0, 4, 0, 4)
            };
            btnAnalyze.FlatAppearance.BorderSize = 0;

            paramRow.Controls.AddRange([
                MakeParamLabel("窓幅:"), nudWindow,
                MakeParamLabel("ステップ:"), nudStep,
                MakeParamLabel("平滑化:"), cmbSmoothing,
                MakeParamLabel("単位:"), cmbUnit
            ]);

            // Action row (解析開始ボタンを独立した行に配置)
            var actionRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 6, 0, 0)
            };
            actionRow.Controls.Add(btnAnalyze);

            panelConfig.Controls.AddRange([fileRow, paramRow, actionRow]);

            // -- Progress --
            var progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 6,
                Style = ProgressBarStyle.Continuous,
                Minimum = 0, Maximum = 100, Value = 0
            };

            // -- Chart --
            var chartBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = bgColor,
                SizeMode = PictureBoxSizeMode.Normal
            };

            // -- Status --
            var lblStatus = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9F),
                ForeColor = dimColor,
                Dock = DockStyle.Bottom,
                AutoSize = false,
                Height = 44,
                Padding = new Padding(28, 10, 28, 4),
                BackColor = panelColor
            };

            var tip = new ToolTip();
            int lastTipIdx = -1;

            // -- Local: render chart --
            void RenderChart()
            {
                if (analysisResults == null || analysisResults.Count == 0) return;
                int w = chartBox.ClientSize.Width;
                int h = chartBox.ClientSize.Height;
                if (w < 20 || h < 20) return;

                const int mL = 56, mR = 16, mT = 12, mB = 32;
                int cW = w - mL - mR;
                int cH = h - mT - mB;
                if (cW < 10 || cH < 10) return;

                try
                {
                    var bmp = new Bitmap(w, h);
                    using var g = Graphics.FromImage(bmp);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(bgColor);

                    using (var br = new SolidBrush(Color.FromArgb(18, 20, 28)))
                        g.FillRectangle(br, mL, mT, cW, cH);

                    using var gridPen = new Pen(Color.FromArgb(35, 255, 255, 255));
                    gridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    using var axFont = new Font("Segoe UI", 7.5f);
                    using var axBrush = new SolidBrush(dimColor);

                    int gridN = maxPossibleH <= 1 ? 4 : Math.Min((int)Math.Ceiling(maxPossibleH), 8);
                    for (int i = 0; i <= gridN; i++)
                    {
                        double val = maxPossibleH * i / gridN;
                        int y = mT + cH - (int)((double)cH * i / gridN);
                        g.DrawLine(gridPen, mL, y, mL + cW, y);
                        var vs = val.ToString(maxPossibleH <= 1 ? "F2" : "F1");
                        var sz = g.MeasureString(vs, axFont);
                        g.DrawString(vs, axFont, axBrush, mL - sz.Width - 4, y - sz.Height / 2);
                    }

                    var fillPens = new Pen[101];
                    for (int i = 0; i <= 100; i++)
                        fillPens[i] = new Pen(Color.FromArgb(110, GetEntropyHeatColor(i / 100.0)));

                    try
                    {
                        var data = analysisResults;
                        for (int px = 0; px < cW; px++)
                        {
                            int idx = (int)((long)px * data.Count / cW);
                            idx = Math.Clamp(idx, 0, data.Count - 1);
                            double norm = Math.Clamp(data[idx].entropy / maxPossibleH, 0, 1);
                            int barH = Math.Max(1, (int)(norm * cH));
                            int ci = Math.Clamp((int)(norm * 100), 0, 100);
                            g.DrawLine(fillPens[ci], mL + px, mT + cH, mL + px, mT + cH - barH);
                        }

                        if (cW >= 2)
                        {
                            var pts = new PointF[cW];
                            for (int px = 0; px < cW; px++)
                            {
                                int idx = (int)((long)px * data.Count / cW);
                                idx = Math.Clamp(idx, 0, data.Count - 1);
                                double norm = Math.Clamp(data[idx].entropy / maxPossibleH, 0, 1);
                                pts[px] = new PointF(mL + px, mT + cH - (float)(norm * cH));
                            }
                            using var linePen = new Pen(Color.FromArgb(200, 230, 232, 240), 1.2f);
                            g.DrawLines(linePen, pts);
                        }
                    }
                    finally
                    {
                        foreach (var p in fillPens) p.Dispose();
                    }

                    // X-axis labels
                    if (analysisResults.Count > 0)
                    {
                        long off0 = analysisResults[0].offset;
                        long offN = analysisResults[^1].offset;
                        for (int i = 0; i <= 4; i++)
                        {
                            long off = off0 + (offN - off0) * i / 4;
                            int x = mL + cW * i / 4;
                            string lbl = FormatOffset(off);
                            var sz = g.MeasureString(lbl, axFont);
                            g.DrawString(lbl, axFont, axBrush, x - sz.Width / 2, mT + cH + 6);
                        }
                    }

                    // Unit label
                    string unitStr = cmbUnit.SelectedIndex switch
                    {
                        1 => "bits (ビット単位)",
                        2 => "bits/bigram",
                        3 => "bits/trigram",
                        _ => "bits/byte (バイト単位)"
                    };
                    g.DrawString($"Entropy: {unitStr}", axFont, axBrush, mL + 4, mT + 2);

                    chartBox.Image?.Dispose();
                    chartBox.Image = bmp;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"RenderChart error: {ex.Message}");
                }
            }

            static string FormatOffset(long off) => off switch
            {
                >= 1_073_741_824 => $"{off / 1_073_741_824.0:F1}G",
                >= 1_048_576 => $"{off / 1_048_576.0:F1}M",
                >= 1_024 => $"{off / 1_024.0:F1}K",
                _ => $"0x{off:X}"
            };

            // -- Events --
            btnOpenFile.Click += (_, _) =>
            {
                using var ofd = new OpenFileDialog
                {
                    Title = "バイナリファイルを選択",
                    Filter = "すべてのファイル (*.*)|*.*|" +
                             "実行ファイル (*.exe;*.dll)|*.exe;*.dll|" +
                             "画像 (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp|" +
                             "アーカイブ (*.zip;*.7z;*.tar;*.gz)|*.zip;*.7z;*.tar;*.gz"
                };
                if (ofd.ShowDialog(dlg) != DialogResult.OK) return;

                try
                {
                    var fi = new FileInfo(ofd.FileName);
                    if (fi.Length == 0)
                    {
                        MessageBox.Show(dlg, "ファイルが空です (0 バイト)。",
                            "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    if (fi.Length > 200_000_000)
                    {
                        if (MessageBox.Show(dlg,
                            $"ファイルサイズが {fi.Length / 1_048_576.0:F1} MB あります。\n続行しますか？",
                            "大きなファイル", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                            return;
                    }

                    fileData = File.ReadAllBytes(ofd.FileName);
                    string sz = fileData.Length switch
                    {
                        >= 1_048_576 => $"{fileData.Length / 1_048_576.0:F2} MB",
                        >= 1_024 => $"{fileData.Length / 1_024.0:F2} KB",
                        _ => $"{fileData.Length} bytes"
                    };
                    lblFilePath.Text = $"{Path.GetFileName(ofd.FileName)}  ({sz})";
                    lblFilePath.ForeColor = textColor;
                    btnAnalyze.Enabled = true;
                    analysisResults = null;
                    chartBox.Image?.Dispose();
                    chartBox.Image = null;
                    lblStatus.Text = "";
                    progressBar.Value = 0;
                }
                catch (OutOfMemoryException)
                {
                    MessageBox.Show(dlg,
                        "ファイルが大きすぎてメモリに読み込めませんでした。\n窓幅を小さくするか、より小さいファイルをお試しください。",
                        "メモリ不足", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show(dlg,
                        "ファイルへのアクセスが拒否されました。\n読み取り権限を確認してください。",
                        "アクセス拒否", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(dlg, $"ファイル読み込みエラー (I/O):\n{ex.Message}",
                        "I/O エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(dlg, $"ファイル読み込みエラー:\n{ex.GetType().Name}: {ex.Message}",
                        "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            btnAnalyze.Click += async (_, _) =>
            {
                if (fileData == null || fileData.Length == 0) return;

                int windowSize = (int)nudWindow.Value;
                int stepSize = (int)nudStep.Value;
                int smoothingIdx = cmbSmoothing.SelectedIndex;
                int unitIdx = cmbUnit.SelectedIndex;

                if (fileData.Length < windowSize)
                {
                    MessageBox.Show(dlg,
                        $"ファイルサイズ ({fileData.Length} bytes) が窓幅 ({windowSize}) より小さいです。\n窓幅を小さくしてください。",
                        "パラメータエラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                maxPossibleH = unitIdx switch
                {
                    1 => 1.0,
                    2 => 16.0,
                    3 => 24.0,
                    _ => 8.0
                };

                btnAnalyze.Enabled = false;
                btnOpenFile.Enabled = false;
                btnAnalyze.Text = "⏳ 解析中...";
                progressBar.Value = 0;

                var data = fileData;
                var sw = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    analysisResults = await Task.Run(() =>
                        CalculateSlidingEntropy(data, windowSize, stepSize,
                            smoothingIdx, unitIdx,
                            pct => { try { dlg.BeginInvoke(() => progressBar.Value = pct); } catch { } }));

                    sw.Stop();
                    progressBar.Value = 100;

                    if (analysisResults.Count > 0)
                    {
                        double minE = analysisResults.Min(r => r.entropy);
                        double avgE = analysisResults.Average(r => r.entropy);
                        double maxE = analysisResults.Max(r => r.entropy);
                        lblStatus.Text =
                            $"Min: {minE:F4}   Avg: {avgE:F4}   Max: {maxE:F4}   " +
                            $"サンプル数: {analysisResults.Count:#,0}   解析時間: {sw.ElapsedMilliseconds:#,0} ms";
                    }

                    RenderChart();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(dlg, $"解析エラー:\n{ex.Message}",
                        "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnAnalyze.Enabled = true;
                    btnOpenFile.Enabled = true;
                    btnAnalyze.Text = "▶ 解析開始";
                }
            };

            chartBox.Resize += (_, _) => RenderChart();

            chartBox.MouseMove += (_, e) =>
            {
                if (analysisResults == null || analysisResults.Count == 0) return;
                int w = chartBox.ClientSize.Width;
                const int mL = 56, mR = 16;
                int cW = w - mL - mR;
                if (cW <= 0) return;

                int px = e.X - mL;
                if (px < 0 || px >= cW)
                {
                    if (lastTipIdx != -1) { tip.SetToolTip(chartBox, ""); lastTipIdx = -1; }
                    return;
                }

                int idx = (int)((long)px * analysisResults.Count / cW);
                idx = Math.Clamp(idx, 0, analysisResults.Count - 1);
                if (idx == lastTipIdx) return;
                lastTipIdx = idx;
                var (offset, entropy) = analysisResults[idx];
                tip.SetToolTip(chartBox, $"Offset: 0x{offset:X} ({offset:#,0})  Entropy: {entropy:F4}");
            };

            // -- Assembly (Dock順: Bottom→Top→Fill の逆順で追加) --
            dlg.Controls.Add(lblStatus);      // Dock.Bottom — 最初に追加
            dlg.Controls.Add(chartBox);       // Dock.Fill  — 残り領域
            dlg.Controls.Add(progressBar);    // Dock.Top
            dlg.Controls.Add(panelConfig);    // Dock.Top
            dlg.Controls.Add(panelHeader);    // Dock.Top   — 最後に追加→最上部

            dlg.Show(this);
        }

        // ---- Heatmap color (blue→green→yellow→orange→red) ----
        private static Color GetEntropyHeatColor(double normalized) => normalized switch
        {
            double.NaN => Color.FromArgb(80, 80, 80),
            < 0.0   => Color.FromArgb(41, 128, 185),
            < 0.125 => Color.FromArgb(41, 128, 185),
            < 0.25  => Color.FromArgb(22, 160, 133),
            < 0.375 => Color.FromArgb(39, 174, 96),
            < 0.50  => Color.FromArgb(46, 204, 113),
            < 0.625 => Color.FromArgb(241, 196, 15),
            < 0.75  => Color.FromArgb(243, 156, 18),
            < 0.875 => Color.FromArgb(230, 126, 34),
            _       => Color.FromArgb(231, 76, 60)
        };

        // ---- Sliding window entropy calculation ----
        private static List<(long offset, double entropy)> CalculateSlidingEntropy(
            byte[] data, int windowSize, int stepSize,
            int smoothingIdx, int unitIdx, Action<int>? onProgress)
        {
            if (data == null || data.Length == 0)
                return [];
            if (windowSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(windowSize), "窓幅は1以上を指定してください。");
            if (stepSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(stepSize), "ステップは1以上を指定してください。");

            int total = (data.Length - windowSize) / stepSize + 1;
            if (total <= 0) return [];
            var results = new List<(long offset, double entropy)>(total);

            for (int pos = 0; pos <= data.Length - windowSize; pos += stepSize)
            {
                var window = new ReadOnlySpan<byte>(data, pos, windowSize);
                double h = unitIdx switch
                {
                    1 => CalcBitWindowEntropy(window, smoothingIdx == 1),
                    2 => CalcNgramWindowEntropy(window, 2, smoothingIdx == 1),
                    3 => CalcNgramWindowEntropy(window, 3, smoothingIdx == 1),
                    _ => CalcByteWindowEntropy(window, smoothingIdx == 1)
                };
                results.Add((pos, h));

                if (results.Count % 2000 == 0)
                    onProgress?.Invoke(Math.Min(99, (int)(100.0 * results.Count / total)));
            }

            onProgress?.Invoke(100);
            return results;
        }

        private static double CalcByteWindowEntropy(ReadOnlySpan<byte> window, bool laplace)
        {
            Span<int> freq = stackalloc int[256];
            freq.Clear();
            foreach (byte b in window) freq[b]++;

            double total = laplace ? window.Length + 256.0 : window.Length;
            double add = laplace ? 1.0 : 0.0;
            double h = 0;
            for (int i = 0; i < 256; i++)
            {
                double c = freq[i] + add;
                if (c > 0) { double p = c / total; h -= p * Math.Log2(p); }
            }
            return h;
        }

        private static double CalcBitWindowEntropy(ReadOnlySpan<byte> window, bool laplace)
        {
            int ones = 0;
            foreach (byte b in window)
            {
                for (int v = b; v != 0; v >>= 1)
                    ones += v & 1;
            }
            int totalBits = window.Length * 8;
            int zeros = totalBits - ones;

            double total = laplace ? totalBits + 2.0 : totalBits;
            double add = laplace ? 1.0 : 0.0;
            double h = 0;
            double p0 = (zeros + add) / total;
            double p1 = (ones + add) / total;
            if (p0 > 0) h -= p0 * Math.Log2(p0);
            if (p1 > 0) h -= p1 * Math.Log2(p1);
            return h;
        }

        private static double CalcNgramWindowEntropy(ReadOnlySpan<byte> window, int n, bool laplace)
        {
            if (n <= 0 || n > 4) return 0;
            if (window.Length < n) return 0;
            var freq = new Dictionary<int, int>();
            int count = window.Length - n + 1;

            for (int i = 0; i <= window.Length - n; i++)
            {
                int key = 0;
                for (int j = 0; j < n; j++)
                    key = (key << 8) | window[i + j];
                freq.TryGetValue(key, out int c);
                freq[key] = c + 1;
            }

            long alphaSize = 1L << (8 * n);
            double total = laplace ? count + (double)alphaSize : count;
            double add = laplace ? 1.0 : 0.0;
            double h = 0;

            if (laplace && alphaSize - freq.Count > 0)
            {
                long unobserved = alphaSize - freq.Count;
                double pZero = add / total;
                h -= unobserved * pZero * Math.Log2(pZero);
            }

            foreach (var kvp in freq)
            {
                double p = (kvp.Value + add) / total;
                if (p > 0) h -= p * Math.Log2(p);
            }
            return h;
        }

        // =====================================================
        // Azure OpenAI REST API 呼び出し
        // =====================================================

        private static async Task<string> CallAzureOpenAiAsync(OpenAiSettings settings, string singlePrompt)
        {
            return await CallAzureOpenAiAsync(settings, [("user", singlePrompt)]);
        }

        private static async Task<string> CallAzureOpenAiAsync(
            OpenAiSettings settings,
            IEnumerable<(string role, string content)> messages)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings), "設定が null です。");
            if (string.IsNullOrWhiteSpace(settings.Endpoint))
                throw new InvalidOperationException("エンドポイントが設定されていません。");
            if (string.IsNullOrWhiteSpace(settings.ApiKey))
                throw new InvalidOperationException("API キーが設定されていません。");
            if (string.IsNullOrWhiteSpace(settings.DeploymentName))
                throw new InvalidOperationException("デプロイメント名が設定されていません。");

            var endpoint = settings.Endpoint.TrimEnd('/');
            var url = $"{endpoint}/openai/deployments/{settings.DeploymentName}" +
                      $"/chat/completions?api-version={settings.ApiVersion}";

            var msgArray = messages.Select(m => new { role = m.role, content = m.content }).ToArray();
            if (msgArray.Length == 0)
                throw new ArgumentException("メッセージが空です。", nameof(messages));

            var body = JsonSerializer.Serialize(new
            {
                messages = msgArray,
                temperature = 0.7,
                max_tokens = 2048
            });

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Add("api-key", settings.ApiKey);
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

            HttpResponseMessage res;
            try
            {
                res = await s_http.SendAsync(req);
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("API リクエストがタイムアウトしました。\nネットワーク接続とエンドポイントを確認してください。");
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException(
                    $"ネットワークエラーが発生しました。\n\n{ex.Message}", ex);
            }

            using (res)
            {
                var json = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"API エラー ({(int)res.StatusCode} {res.ReasonPhrase})\n{json}");
                }

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var content = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    return content?.Trim() ?? "(空の応答)";
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException(
                        $"API 応答の解析に失敗しました。\n\n{ex.Message}\n\n応答: {json[..Math.Min(json.Length, 500)]}", ex);
                }
                catch (KeyNotFoundException)
                {
                    throw new InvalidOperationException(
                        $"API 応答の形式が予期と異なります。\n\n応答: {json[..Math.Min(json.Length, 500)]}");
                }
            }
        }
    }
}
