# ShannonEntropyLab

A Windows desktop application for calculating and visualizing **Shannon entropy** — a fundamental measure of information randomness.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build](https://github.com/freyWylfred/ShannonEntropyLab/actions/workflows/build.yml/badge.svg)](https://github.com/freyWylfred/ShannonEntropyLab/actions)

> **[日本語版 README はこちら](docs/README.ja.md)**

---

## Overview

ShannonEntropyLab lets you explore Shannon entropy through multiple analysis modes:

- **String entropy** — type any text and instantly see its entropy value, strength rating, and character frequency distribution
- **Binary file analysis** — load a file and examine byte-level entropy with frequency charts
- **Sliding window analysis** — visualize entropy changes across a file with an interactive heatmap
- **High-entropy string generation** — produce cryptographically random strings and export them to Excel
- **AI Chat** — ask questions about entropy or anything else via Azure OpenAI integration

---

## Requirements

| Item | Details |
|------|---------|
| **OS** | Windows 10 / 11 |
| **Runtime** | [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) |
| **AI Chat (optional)** | Azure OpenAI Service endpoint & API key |

---

## Build & Run

```bash
# Clone the repository
git clone https://github.com/freyWylfred/ShannonEntropyLab.git
cd ShannonEntropyLab

# Build
dotnet build

# Run
dotnet run --project ShannonEntropyLab
```

Alternatively, open `ShannonEntropyLab.slnx` in **Visual Studio 2022 (17.14+)** and press `F5`.

---

## Usage

### String Entropy Calculation
1. Enter text in the main window text box
2. Click **Calculate Entropy** (or press `Ctrl+Enter`)
3. View the entropy value, strength rating, and character frequency distribution

### Binary File Analysis
1. Menu → **File** → **Open File** (`Ctrl+O`)
2. Select a file to analyze
3. View byte-level entropy and frequency distribution (top 40 byte values)

### Sliding Window Analysis
1. Menu → **Tools** → **Sliding Window Analysis** (`Ctrl+W`)
2. Click **Select File** to load a binary file
3. Configure window size and step size
4. Click **Start Analysis**
5. Hover over the heatmap for detailed information

### High-Entropy String Generation
1. Menu → **Tools** → **Generate High-Entropy Strings** (`Ctrl+G`)
2. Set the count and length, then click **Generate**
3. Click **Export to Excel** to save as `.xlsx`

### AI Chat (Azure OpenAI)
1. Menu → **Tools** → **OpenAI Settings** — configure your endpoint & API key
2. Menu → **Tools** → **AI Chat** (`Ctrl+Shift+A`) to start a conversation

---

## Entropy Reference

| Entropy | Rating | Example |
|:-------:|:------:|:--------|
| < 1.5 | Very Low | `aaaaaaa` |
| < 2.5 | Low | `abcabc` |
| < 3.5 | Moderate | Typical English text |
| < 4.5 | High | Passwords |
| ≥ 4.5 | Very High | Cryptographic random strings |

> **H = −Σ p(x) × log₂ p(x)**
>
> The higher the Shannon entropy, the greater the randomness of the data.

---

## Tech Stack

- **Framework**: .NET 10 / Windows Forms
- **Excel Export**: [ClosedXML](https://github.com/ClosedXML/ClosedXML) 0.105.0
- **AI Integration**: Azure OpenAI REST API (`System.Net.Http`)
- **Random Generation**: `System.Security.Cryptography.RandomNumberGenerator`
- **Chart Rendering**: GDI+ (`System.Drawing`)

---

## Project Structure

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
│   └── OpenAiSettings.cs            # Azure OpenAI settings (JSON persistence)
├── .editorconfig                    # Code style settings
├── .gitignore
├── CONTRIBUTING.md                  # Contribution guide
├── LICENSE                          # MIT License
├── README.md                        # English (default)
├── docs/
│   └── README.ja.md                 # Japanese README
└── ShannonEntropyLab.slnx          # Solution file
```

---

## License

[MIT License](LICENSE)

---

## Contributing

Issues and Pull Requests are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

---

## Creating a Distributable Executable

```bash
dotnet publish ShannonEntropyLab/ShannonEntropyLab.csproj -c Release -r win-x64 -p:PublishSingleFile=true
```

A self-contained single executable is generated at `ShannonEntropyLab/bin/Release/net10.0-windows/win-x64/publish/ShannonEntropyLab.exe` (no .NET runtime required).

> **Automated Releases**: Pushing a `v*` tag triggers GitHub Actions to build and create a Release automatically.
>
> ```bash
> git tag v1.1.0
> git push origin v1.1.0
> ```
