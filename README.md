# Tesseract OCR Tester

A simple C# Blazor app to test Tesseract OCR quality on PDF documents with AI cleanup using Ollama.

## What It Does

- Upload PDF files
- Extract text using Tesseract OCR
- Clean up messy OCR output with local AI (Llama/Mistral)
- Compare raw OCR vs AI-cleaned results

## Prerequisites

1. **.NET 8.0 SDK** - https://dotnet.microsoft.com/download/dotnet/8.0
2. **Ollama** - https://ollama.com/download
3. **Tesseract language data** (see setup below)

## Setup

### 1. Clone the repo
```bash
git clone <your-repo-url>
cd TesseractOcrTest
```S

### 2. Download Tesseract language data
```bash
# Download eng.traineddata
# From: https://github.com/tesseract-ocr/tessdata/blob/main/eng.traineddata

# Put it here:
wwwroot/tessdata/eng.traineddata
```

Or use this direct link: https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata

### 3. Install Ollama and pull a model
```bash
# Install from https://ollama.com/download
ollama pull llama3.2
# or
ollama pull mistral
```

### 4. Restore packages and run
```bash
dotnet restore
dotnet run
```

Open browser to: `http://localhost:5000`

## Usage

1. Upload a PDF
2. Click "Process PDF" - see raw OCR output
3. Click "Clean with AI" - see AI-structured output
4. Compare results

## Configuration

Edit `appsettings.json` to change Ollama model:
```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "Model": "llama3.2:latest"  
  }
}
```

## Tech Stack

- .NET 8.0 Blazor Server
- Tesseract OCR (via Tesseract NuGet)
- Docnet.Core (PDF rendering)
- SkiaSharp (image processing)
- Ollama (local LLM for cleanup)

## License

MIT