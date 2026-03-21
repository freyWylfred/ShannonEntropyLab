namespace ShannonEntropyLab
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // --- Colors ---
            var bgColor = Color.FromArgb(24, 24, 32);
            var panelColor = Color.FromArgb(32, 34, 46);
            var accentColor = Color.FromArgb(99, 102, 241);
            var textColor = Color.FromArgb(230, 230, 240);
            var subtextColor = Color.FromArgb(140, 144, 164);
            var inputBgColor = Color.FromArgb(40, 42, 56);
            var btnHoverColor = Color.FromArgb(79, 82, 220);

            const int panelW = 680;
            const int innerW = panelW - 40;

            // --- Form ---
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(730, 920);
            BackColor = bgColor;
            ForeColor = textColor;
            Text = "Shannon Entropy Lab";
            Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            StartPosition = FormStartPosition.CenterScreen;
            AutoScroll = true;

            // ============================
            // MenuStrip
            // ============================
            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(28, 28, 38),
                ForeColor = textColor,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                Padding = new Padding(8, 3, 0, 3),
                Renderer = new DarkMenuRenderer(panelColor, inputBgColor, accentColor, textColor, subtextColor)
            };

            // -- File --
            menuFile = new ToolStripMenuItem("ファイル(&F)");
            menuFileOpen = new ToolStripMenuItem("ファイルを開く(&O)...", null, null, Keys.Control | Keys.O);
            menuFileExit = new ToolStripMenuItem("終了(&X)", null, null, Keys.Alt | Keys.F4);
            menuFile.DropDownItems.AddRange(new ToolStripItem[]
            {
                menuFileOpen,
                new ToolStripSeparator(),
                menuFileExit
            });

            // -- Edit --
            menuEdit = new ToolStripMenuItem("編集(&E)");
            menuEditClear = new ToolStripMenuItem("入力をクリア(&C)", null, null, Keys.Control | Keys.Delete);
            menuEdit.DropDownItems.Add(menuEditClear);

            // -- Tools --
            menuTools = new ToolStripMenuItem("ツール(&T)");
            menuToolsGenerate = new ToolStripMenuItem("高エントロピー文字列を生成(&G)...", null, null, Keys.Control | Keys.G);
            menuToolsSlidingWindow = new ToolStripMenuItem("スライディングウィンドウ解析(&W)...", null, null, Keys.Control | Keys.W);
            menuToolsAiChat = new ToolStripMenuItem("AI チャット(&A)...", null, null, Keys.Control | Keys.Shift | Keys.A);
            menuToolsOpenAiSettings = new ToolStripMenuItem("OpenAI 接続設定(&O)...");
            menuTools.DropDownItems.AddRange(new ToolStripItem[]
            {
                menuToolsGenerate,
                menuToolsSlidingWindow,
                new ToolStripSeparator(),
                menuToolsAiChat,
                menuToolsOpenAiSettings
            });

            // -- Help --
            menuHelp = new ToolStripMenuItem("ヘルプ(&H)");
            menuHelpUsage = new ToolStripMenuItem("使い方(&U)", null, null, Keys.F1);
            menuHelpAbout = new ToolStripMenuItem("Shannon Entropy Lab について(&A)");
            menuHelp.DropDownItems.AddRange(new ToolStripItem[] { menuHelpUsage, new ToolStripSeparator(), menuHelpAbout });

            menuStrip.Items.AddRange(new ToolStripItem[] { menuFile, menuEdit, menuTools, menuHelp });
            MainMenuStrip = menuStrip;

            // ============================
            // FlowLayoutPanel (メインコンテナ)
            // ============================
            flowMain = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = bgColor,
                Padding = new Padding(20, 16, 20, 16)
            };

            // --- Title ---
            lblTitle = new Label
            {
                Text = "🔐 Shannon Entropy Lab",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = textColor,
                AutoSize = true,
                Margin = new Padding(4, 0, 0, 0),
                BackColor = Color.Transparent
            };

            // --- Subtitle ---
            lblSubtitle = new Label
            {
                Text = "文字列の情報エントロピーを計測します",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = subtextColor,
                AutoSize = true,
                Margin = new Padding(6, 2, 0, 12),
                BackColor = Color.Transparent
            };

            // ============================
            // Input Panel (入力セクション)
            // ============================
            panelInput = new Panel
            {
                Size = new Size(panelW, 240),
                BackColor = panelColor,
                Padding = new Padding(20, 16, 20, 16),
                Margin = new Padding(0, 0, 0, 12)
            };

            lblInputCaption = new Label
            {
                Text = "入力文字列",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = subtextColor,
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent
            };

            txtInput = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 120,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = inputBgColor,
                ForeColor = textColor,
                Font = new Font("Consolas", 11F),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnCalc = new Button
            {
                Text = "⚡ エントロピーを算出",
                Dock = DockStyle.Bottom,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(200, 42),
                FlatStyle = FlatStyle.Flat,
                BackColor = accentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnCalc.FlatAppearance.BorderSize = 0;
            btnCalc.FlatAppearance.MouseOverBackColor = btnHoverColor;

            panelInput.Controls.Add(txtInput);
            panelInput.Controls.Add(lblInputCaption);
            panelInput.Controls.Add(btnCalc);

            // ============================
            // Result Panel (結果セクション)
            // ============================
            panelResult = new Panel
            {
                Size = new Size(panelW, 210),
                BackColor = panelColor,
                Margin = new Padding(0, 0, 0, 12)
            };

            lblEntropyCaption = new Label
            {
                Text = "エントロピー (bits/char)",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = subtextColor,
                Location = new Point(20, 16),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblEntropy = new Label
            {
                Text = "---",
                Font = new Font("Segoe UI", 34F, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(16, 46),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblStrength = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = subtextColor,
                Location = new Point(20, 120),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblEntropyBar = new Label
            {
                Location = new Point(20, 155),
                Size = new Size(innerW, 28),
                BackColor = inputBgColor,
                Text = "",
            };

            lblEntropyBarFill = new Label
            {
                Location = new Point(20, 155),
                Size = new Size(0, 28),
                BackColor = accentColor,
                Text = "",
            };

            panelResult.Controls.Add(lblEntropyBarFill);
            panelResult.Controls.Add(lblEntropyBar);
            panelResult.Controls.Add(lblEntropyCaption);
            panelResult.Controls.Add(lblEntropy);
            panelResult.Controls.Add(lblStrength);

            // ============================
            // Stats Panel (統計セクション)
            // ============================
            panelStats = new Panel
            {
                Size = new Size(panelW, 60),
                BackColor = panelColor,
                Margin = new Padding(0, 0, 0, 12)
            };

            lblStats = new Label
            {
                Text = "文字数: 0　　ユニーク文字数: 0　　最大エントロピー: 0.000",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = subtextColor,
                Location = new Point(20, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            panelStats.Controls.Add(lblStats);

            // ============================
            // Frequency Panel (頻度分布)
            // ============================
            panelFreq = new Panel
            {
                Size = new Size(panelW, 200),
                BackColor = panelColor,
                AutoScroll = true,
                Margin = new Padding(0, 0, 0, 0)
            };

            lblFreqCaption = new Label
            {
                Text = "文字頻度分布",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = subtextColor,
                Location = new Point(20, 14),
                AutoSize = true,
                BackColor = Color.Transparent
            };

            lblFreqDetails = new Label
            {
                Text = "",
                Font = new Font("Consolas", 9.5F, FontStyle.Regular),
                ForeColor = textColor,
                Location = new Point(20, 44),
                AutoSize = true,
                BackColor = Color.Transparent,
                MaximumSize = new Size(innerW, 0)
            };

            panelFreq.Controls.Add(lblFreqCaption);
            panelFreq.Controls.Add(lblFreqDetails);

            // ============================
            // Assemble
            // ============================
            flowMain.Controls.Add(lblTitle);
            flowMain.Controls.Add(lblSubtitle);
            flowMain.Controls.Add(panelInput);
            flowMain.Controls.Add(panelResult);
            flowMain.Controls.Add(panelStats);
            flowMain.Controls.Add(panelFreq);
            Controls.Add(flowMain);
            Controls.Add(menuStrip);
        }

        #endregion

        private FlowLayoutPanel flowMain;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel panelInput;
        private Label lblInputCaption;
        private TextBox txtInput;
        private Button btnCalc;
        private Panel panelResult;
        private Label lblEntropyCaption;
        private Label lblEntropy;
        private Label lblStrength;
        private Label lblEntropyBar;
        private Label lblEntropyBarFill;
        private Panel panelStats;
        private Label lblStats;
        private Panel panelFreq;
        private Label lblFreqCaption;
        private Label lblFreqDetails;
        private MenuStrip menuStrip;
        private ToolStripMenuItem menuFile;
        private ToolStripMenuItem menuFileOpen;
        private ToolStripMenuItem menuFileExit;
        private ToolStripMenuItem menuEdit;
        private ToolStripMenuItem menuEditClear;
        private ToolStripMenuItem menuTools;
        private ToolStripMenuItem menuToolsGenerate;
        private ToolStripMenuItem menuToolsSlidingWindow;
        private ToolStripMenuItem menuToolsAiChat;
        private ToolStripMenuItem menuToolsOpenAiSettings;
        private ToolStripMenuItem menuHelp;
        private ToolStripMenuItem menuHelpUsage;
        private ToolStripMenuItem menuHelpAbout;
    }

    /// <summary>
    /// Dark-themed renderer for MenuStrip / ToolStripDropDown.
    /// </summary>
    internal sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly Color _highlight;
        private readonly Color _text;
        private readonly Color _dimText;

        public DarkMenuRenderer(Color menuBg, Color dropBg, Color highlight, Color text, Color dimText)
            : base(new DarkColorTable(menuBg, dropBg, highlight))
        {
            _highlight = highlight;
            _text = text;
            _dimText = dimText;
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var rc = new Rectangle(Point.Empty, e.Item.Size);
            var color = e.Item.Selected ? _highlight : Color.Transparent;
            using var brush = new SolidBrush(color);
            e.Graphics.FillRectangle(brush, rc);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? _text : _dimText;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            int y = e.Item.Height / 2;
            using var pen = new Pen(_dimText, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
            e.Graphics.DrawLine(pen, 4, y, e.Item.Width - 4, y);
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(
                e.ToolStrip is ToolStripDropDown
                    ? ((DarkColorTable)ColorTable).DropBg
                    : ((DarkColorTable)ColorTable).MenuBg);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (e.ToolStrip is ToolStripDropDown)
            {
                using var pen = new Pen(_highlight);
                var rc = new Rectangle(0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
                e.Graphics.DrawRectangle(pen, rc);
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) { }
    }

    internal sealed class DarkColorTable : ProfessionalColorTable
    {
        public Color MenuBg { get; }
        public Color DropBg { get; }
        private readonly Color _highlight;

        public DarkColorTable(Color menuBg, Color dropBg, Color highlight)
        {
            MenuBg = menuBg;
            DropBg = dropBg;
            _highlight = highlight;
        }

        public override Color MenuBorder => _highlight;
        public override Color MenuItemBorder => Color.Transparent;
        public override Color MenuItemSelected => _highlight;
        public override Color MenuItemSelectedGradientBegin => _highlight;
        public override Color MenuItemSelectedGradientEnd => _highlight;
        public override Color MenuItemPressedGradientBegin => _highlight;
        public override Color MenuItemPressedGradientEnd => _highlight;
        public override Color MenuStripGradientBegin => MenuBg;
        public override Color MenuStripGradientEnd => MenuBg;
        public override Color ToolStripDropDownBackground => DropBg;
        public override Color ImageMarginGradientBegin => DropBg;
        public override Color ImageMarginGradientMiddle => DropBg;
        public override Color ImageMarginGradientEnd => DropBg;
        public override Color SeparatorDark => _highlight;
        public override Color SeparatorLight => _highlight;
    }
}
