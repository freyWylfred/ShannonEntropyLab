# ShannonEntropyLab

**シャノンエントロピー**を計算・可視化する Windows デスクトップアプリケーション

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](../LICENSE)
[![Build](https://github.com/freyWylfred/ShannonEntropyLab/actions/workflows/build.yml/badge.svg)](https://github.com/freyWylfred/ShannonEntropyLab/actions)

> **[English README](../README.md)**

---

## 概要

ShannonEntropyLab は、シャノンエントロピーを様々な方法で分析できるツールです：

- **文字列エントロピー計算** — テキストを入力して即座にエントロピー値・強度評価・文字頻度分布を表示
- **バイナリファイル解析** — ファイルのバイトレベルのエントロピーと頻度分布を分析
- **スライディングウィンドウ解析** — ヒートマップでファイル全体のエントロピー変化を可視化
- **高エントロピー文字列生成** — 暗号学的に安全なランダム文字列を生成し Excel にエクスポート
- **AI チャット** — Azure OpenAI 連携でエントロピーに関する質問が可能

---

## 必要条件

| 項目 | 要件 |
|------|------|
| **OS** | Windows 10 / 11 |
| **ランタイム** | [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **AI チャット (任意)** | Azure OpenAI Service のエンドポイント & API キー |

---

## ビルド & 実行

```bash
# リポジトリをクローン
git clone https://github.com/freyWylfred/ShannonEntropyLab.git
cd ShannonEntropyLab

# ビルド
dotnet build

# 実行
dotnet run --project ShannonEntropyLab
```

または **Visual Studio 2022 (17.14+)** でソリューションファイル `ShannonEntropyLab.slnx` を開いて `F5` で実行してください。

---

## 使い方

### 文字列エントロピー計算
1. メイン画面のテキストボックスに文字列を入力
2. **エントロピーを計算** をクリック（または `Ctrl+Enter`）
3. エントロピー値・強度評価・文字頻度分布が表示されます

### バイナリファイル解析
1. メニュー → **ファイル** → **ファイルを開く** (`Ctrl+O`)
2. 解析したいファイルを選択
3. バイトレベルのエントロピーと頻度分布（上位40種）が表示されます

### スライディングウィンドウ解析
1. メニュー → **ツール** → **スライディングウィンドウ解析** (`Ctrl+W`)
2. **ファイルを選択** でバイナリファイルを読み込み
3. 窓幅・ステップなどのパラメータを設定
4. **解析開始** をクリック
5. ヒートマップにマウスオーバーで詳細情報を確認

### 高エントロピー文字列生成
1. メニュー → **ツール** → **高エントロピー文字列を生成** (`Ctrl+G`)
2. 生成数・文字列長を設定して **生成**
3. **Excel出力** で .xlsx ファイルにエクスポート

### AI チャット (Azure OpenAI)
1. メニュー → **ツール** → **OpenAI 接続設定** でエンドポイント・API キーを設定
2. メニュー → **ツール** → **AI チャット** (`Ctrl+Shift+A`) で対話開始

---

## エントロピーの目安

| エントロピー | 評価 | 例 |
|:---:|:---:|:---|
| < 1.5 | 非常に低い | `aaaaaaa` |
| < 2.5 | 低い | `abcabc` |
| < 3.5 | 中程度 | 一般的な英文 |
| < 4.5 | 高い | パスワード |
| ≥ 4.5 | 非常に高い | 暗号学的乱数文字列 |

> **H = −Σ p(x) × log₂ p(x)**
>
> シャノンエントロピーが高いほど、データのランダム性が高いことを示します。

---

## 技術スタック

- **フレームワーク**: .NET 10 / Windows Forms
- **Excel 出力**: [ClosedXML](https://github.com/ClosedXML/ClosedXML) 0.105.0
- **AI 連携**: Azure OpenAI REST API (`System.Net.Http`)
- **乱数生成**: `System.Security.Cryptography.RandomNumberGenerator`
- **チャート描画**: GDI+ (`System.Drawing`)

---

## プロジェクト構成

```
ShannonEntropyLab/
├── .github/
│   └── workflows/
│       └── build.yml               # CI / 自動リリース (GitHub Actions)
├── ShannonEntropyLab/
│   ├── ShannonEntropyLab.csproj     # プロジェクトファイル (.NET 10)
│   ├── Program.cs                   # エントリポイント
│   ├── Form1.cs                     # メインフォーム & 全ダイアログロジック
│   ├── Form1.Designer.cs            # メインフォーム UI 定義
│   ├── Form1.resx                   # リソースファイル
│   └── OpenAiSettings.cs            # Azure OpenAI 設定モデル (JSON 永続化)
├── .editorconfig                    # コードスタイル設定
├── .gitignore
├── CONTRIBUTING.md                  # コントリビュートガイド
├── LICENSE                          # MIT License
├── README.md                        # English (デフォルト)
├── docs/
│   └── README.ja.md                 # 日本語 README
└── ShannonEntropyLab.slnx          # ソリューションファイル
```

---

## ライセンス

[MIT License](../LICENSE)

---

## コントリビュート

Issue や Pull Request を歓迎します。詳しくは [CONTRIBUTING.md](../CONTRIBUTING.md) をご覧ください。

---

## 配布用 exe の作成

```bash
dotnet publish ShannonEntropyLab/ShannonEntropyLab.csproj -c Release -r win-x64 -p:PublishSingleFile=true
```

`ShannonEntropyLab/bin/Release/net10.0-windows/win-x64/publish/ShannonEntropyLab.exe` に自己完結型の単一 exe が生成されます（.NET ランタイム不要）。

> **自動リリース**: `v*` タグをプッシュすると GitHub Actions が自動で exe をビルドし、Release を作成します。
>
> ```bash
> git tag v1.1.0
> git push origin v1.1.0
> ```
