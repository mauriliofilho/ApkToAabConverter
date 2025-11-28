#!/bin/bash

# Script para download automático do bundletool
# APK to AAB Converter for macOS

echo "======================================"
echo "  Bundletool Download Script"
echo "======================================"
echo ""

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

# Diretório de destino
TOOLS_DIR="Resources/Tools"
BUNDLETOOL_PATH="$TOOLS_DIR/bundletool.jar"

# Criar diretório se não existir
echo "Verificando diretório..."
if [ ! -d "$TOOLS_DIR" ]; then
    mkdir -p "$TOOLS_DIR"
    echo -e "${GREEN}✓${NC} Diretório criado: $TOOLS_DIR"
else
    echo -e "${GREEN}✓${NC} Diretório já existe: $TOOLS_DIR"
fi
echo ""

# Verificar se bundletool já existe
if [ -f "$BUNDLETOOL_PATH" ]; then
    echo -e "${YELLOW}⚠${NC} bundletool.jar já existe!"
    read -p "Deseja sobrescrever? (s/N): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Ss]$ ]]; then
        echo "Download cancelado."
        exit 0
    fi
    echo "Removendo versão antiga..."
    rm "$BUNDLETOOL_PATH"
fi

# Download do bundletool
echo "Baixando bundletool da última versão..."
echo ""

DOWNLOAD_URL="https://github.com/google/bundletool/releases/latest/download/bundletool-all.jar"

if command -v curl &> /dev/null; then
    echo "Usando curl para download..."
    if curl -L -o "$BUNDLETOOL_PATH" "$DOWNLOAD_URL" --progress-bar; then
        echo -e "${GREEN}✓${NC} Download concluído!"
    else
        echo -e "${RED}✗${NC} Erro no download!"
        exit 1
    fi
elif command -v wget &> /dev/null; then
    echo "Usando wget para download..."
    if wget -O "$BUNDLETOOL_PATH" "$DOWNLOAD_URL"; then
        echo -e "${GREEN}✓${NC} Download concluído!"
    else
        echo -e "${RED}✗${NC} Erro no download!"
        exit 1
    fi
else
    echo -e "${RED}✗${NC} curl ou wget não encontrado!"
    echo "Por favor, instale um deles:"
    echo "  brew install curl"
    echo "  brew install wget"
    exit 1
fi
echo ""

# Verificar download
echo "Verificando arquivo..."
if [ -f "$BUNDLETOOL_PATH" ]; then
    file_size=$(wc -c < "$BUNDLETOOL_PATH")
    echo -e "${GREEN}✓${NC} Arquivo criado: $BUNDLETOOL_PATH"
    echo "   Tamanho: $file_size bytes"
    
    # Verificar se é um JAR válido
    if file "$BUNDLETOOL_PATH" | grep -q "Java"; then
        echo -e "${GREEN}✓${NC} Arquivo JAR válido"
    else
        echo -e "${YELLOW}⚠${NC} Arquivo pode não ser um JAR válido"
    fi
else
    echo -e "${RED}✗${NC} Arquivo não foi criado!"
    exit 1
fi
echo ""

# Testar bundletool
echo "Testando bundletool..."
if java -jar "$BUNDLETOOL_PATH" version &> /dev/null; then
    bundletool_version=$(java -jar "$BUNDLETOOL_PATH" version 2>&1 | head -n 1)
    echo -e "${GREEN}✓${NC} bundletool funcionando!"
    echo "   $bundletool_version"
else
    echo -e "${RED}✗${NC} Erro ao executar bundletool!"
    echo "   Verifique se o Java está instalado:"
    echo "   java -version"
    exit 1
fi
echo ""

# Sucesso
echo "======================================"
echo -e "${GREEN}✓ Instalação concluída com sucesso!${NC}"
echo "======================================"
echo ""
echo "O bundletool está pronto para uso."
echo ""
echo "Próximos passos:"
echo "  1. Execute: ./check_installation.sh"
echo "  2. Build: dotnet build"
echo "  3. Run: dotnet run -f net9.0-maccatalyst"
echo ""

exit 0
