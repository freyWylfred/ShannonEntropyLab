[English](README.md) | [日本語](docs/README.ja.md)

# 🔐 Shannon Entropy Lab

**Shannon Entropy Lab** is a Windows desktop application for measuring and visualizing the **Shannon entropy** of text strings and binary files.  
It can be used for security analysis, encryption verification, data randomness evaluation, and more.

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Windows Forms](https://img.shields.io/badge/UI-WinForms-blue)
![License](https://img.shields.io/badge/License-MIT-green)
![Build](https://github.com/freyWylfred/ShannonEntropyLab/actions/workflows/build.yml/badge.svg)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| **String Entropy Calculation** | Computes Shannon entropy (bits/char) of input text in real time |
| **Binary File Analysis** | Analyzes any file at the byte level, displaying entropy and frequency distribution |
| **Sliding Window Analysis** | Visualizes local entropy changes within a file using a heatmap chart |
| **High-Entropy String Generator** | Batch-generates cryptographically secure random strings & exports to Excel |
| **AI Chat** | Integrates with Azure OpenAI for AI-powered conversations about entropy and security |
| **Dark Theme UI** | Eye-friendly dark color scheme |

---

## 📸 Overview

### Main Window
- Enter a string and click **⚡ Calculate Entropy** (or press `Ctrl+Enter`) to instantly display the entropy value, strength rating, and character frequency distribution
- The entropy bar provides an intuitive visual indicator of strength

### Sliding Window Analysis
- Visualizes **local entropy** of binary files as a time-series chart
- Customizable window size, step size, smoothing method, and analysis unit (byte / bit / bigram / trigram)
- Heatmap coloring lets you identify encrypted, compressed, and plaintext regions at a glance

### High-Entropy String Generator
- Secure random generation powered by `System.Security.Cryptography.RandomNumberGenerator`
- Export results to Excel (.xlsx)

---

## 🚀 Requirements

| Item | Requirement |
|------|-------------|
| **OS** | Windows 10 / 11 |
| **Runtime** | [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **AI Chat (optional)** | Azure OpenAI Service endpoint & API key |

---

## 🏗️ Build & Run

```bash
# Clone the repository
git clone https://github.com/freyWylfred/ShannonEntropyLab.git
cd ShannonEntropyLab

# Build
dotnet build

# Run
dotnet run --project ShannonEntropyLab
```

Alternatively, open the solution file `ShannonEntropyLab.slnx` in **Visual Studio 2022 (17.14+)** and press `F5` to run.

---

## 📖 Usage

### String Entropy Calculation
1. Enter a string in the text box on the main window
2. Click **⚡ Calculate Entropy** (or press `Ctrl+Enter`)
3. The entropy value, strength rating, and character frequency distribution are displayed

### Binary File Analysis
1. Menu → **File** → **Open File** (`Ctrl+O`)
2. Select the file to analyze
3. Byte-level entropy and frequency distribution (top 40 byte values) are displayed

### Sliding Window Analysis
1. Menu → **Tools** → **Sliding Window Analysis** (`Ctrl+W`)
2. Click **📂 Select File** to load a binary file
3. Configure parameters such as window size and step size
4. Click **▶ Start Analysis**
5. Hover over the heatmap chart to see detailed information

### High-Entropy String Generation
1. Menu → **Tools** → **Generate High-Entropy Strings** (`Ctrl+G`)
2. Set the count and string length, then click **⚡ Generate**
3. Click **📊 Export to Excel** to save as an .xlsx file

### AI Chat (Azure OpenAI)
1. Menu → **Tools** → **OpenAI Settings** — configure your endpoint & API key
2. Menu → **Tools** → **AI Chat** (`Ctrl+Shift+A`) to start a conversation

---

## 📐 Entropy Reference

| Entropy | Rating | Example |
|:-------:|:------:|:--------|
| < 1.5 | ⚠ Very Low | `aaaaaaa` |
| < 2.5 | △ Low | `abcabc` |
| < 3.5 | ○ Moderate | Typical English text |
| < 4.5 | ◎ High | Passwords |
| ≥ 4.5 | ★ Very High | Cryptographic random strings |

> **H = − Σ p(x) × log₂ p(x)**  
> The higher the Shannon entropy, the greater the randomness of the data.

---

## 🛠️ Tech Stack

- **Framework**: .NET 10 / Windows Forms
- **Excel Export**: [ClosedXML](https://github.com/ClosedXML/ClosedXML) 0.105.0
- **AI Integration**: Azure OpenAI REST API (`System.Net.Http`)
- **Random Generation**: `System.Security.Cryptography.RandomNumberGenerator`
- **Chart Rendering**: GDI+ (`System.Drawing`)

---

## 📁 Project Structure

```
ShannonEntropyLab/
├── .github/
│   └── workflows/
│       └── build.yml               # CI / auto-release (GitHub Actions)
├── ShannonEntropyLab/
│   ├── ShannonEntropyLab.csproj     # Project file (.NET 10)
│   ├── Program.cs                   # Entry point
│   ├── Form1.cs                     # Main form & all dialog logic
│   ├── Form1.Designer.cs            # Main form UI definitions
│   ├── Form1.resx                   # Resource file
│   └── OpenAiSettings.cs            # Azure OpenAI settings model (JSON persistence)
├── .editorconfig                    # Code style settings
├── .gitignore
├── CONTRIBUTING.md                  # Contribution guide
├── LICENSE                          # MIT License
├── README.md                        # English (default)
├── docs/
│   └── README.ja.md                 # 日本語 README
└── ShannonEntropyLab.slnx          # Solution file
```

---

## 📜 License

[MIT License](LICENSE)

---

## 🤝 Contributing

Issues and Pull Requests are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

---

## 📦 Creating a Distributable Executable

```bash
dotnet publish ShannonEntropyLab/ShannonEntropyLab.csproj -c Release -r win-x64 -p:PublishSingleFile=true
```

A self-contained single executable is generated at `ShannonEntropyLab/bin/Release/net10.0-windows/win-x64/publish/ShannonEntropyLab.exe` (no .NET runtime required).

> **Automated Releases**: Pushing a `v*` tag triggers GitHub Actions to automatically build the executable and create a Release.
>
> ```bash
> git tag v1.1.0
> git push origin v1.1.0
> ```
