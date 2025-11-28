#!/bin/bash

# Script de verificação de instalação
# APK to AAB Converter for macOS

echo "======================================"
echo "  APK to AAB Converter"
echo "  Verificação de Instalação"
echo "======================================"
echo ""

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Contador de erros
errors=0

# Função para verificar comando
check_command() {
    if command -v $1 &> /dev/null; then
        echo -e "${GREEN}✓${NC} $2 encontrado"
        return 0
    else
        echo -e "${RED}✗${NC} $2 não encontrado!"
        ((errors++))
        return 1
    fi
}

# Verificar .NET
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "1. Verificando .NET SDK..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if check_command dotnet ".NET CLI"; then
    dotnet_version=$(dotnet --version)
    echo "   Versão: $dotnet_version"
    
    # Verificar workload MAUI
    if dotnet workload list | grep -q "maui"; then
        echo -e "   ${GREEN}✓${NC} MAUI workload instalado"
    else
        echo -e "   ${YELLOW}⚠${NC} MAUI workload não encontrado"
        echo "   Execute: dotnet workload install maui"
        ((errors++))
    fi
fi
echo ""

# Verificar Java
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "2. Verificando Java JDK..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if check_command java "Java Runtime"; then
    java_version=$(java -version 2>&1 | head -n 1)
    echo "   $java_version"
    
    # Verificar JAVA_HOME
    if [ -n "$JAVA_HOME" ]; then
        echo -e "   ${GREEN}✓${NC} JAVA_HOME: $JAVA_HOME"
    else
        echo -e "   ${YELLOW}⚠${NC} JAVA_HOME não configurado"
    fi
    
    # Verificar keytool
    if check_command keytool "keytool"; then
        echo -e "   ${GREEN}✓${NC} keytool disponível"
    fi
    
    # Verificar jarsigner
    if check_command jarsigner "jarsigner"; then
        echo -e "   ${GREEN}✓${NC} jarsigner disponível"
    fi
fi
echo ""

# Verificar bundletool
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "3. Verificando bundletool..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
bundletool_path="Resources/Tools/bundletool.jar"
if [ -f "$bundletool_path" ]; then
    echo -e "${GREEN}✓${NC} bundletool.jar encontrado"
    if java -jar "$bundletool_path" version &> /dev/null; then
        bundletool_version=$(java -jar "$bundletool_path" version 2>&1 | head -n 1)
        echo "   $bundletool_version"
    else
        echo -e "   ${RED}✗${NC} bundletool.jar corrompido"
        ((errors++))
    fi
else
    echo -e "${RED}✗${NC} bundletool.jar não encontrado!"
    echo "   Baixe de: https://github.com/google/bundletool/releases"
    ((errors++))
fi
echo ""

# Verificar certificados
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "4. Verificando certificados..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
certs_path="Resources/Certificates"

if [ -f "$certs_path/testkey.pk8" ]; then
    echo -e "${GREEN}✓${NC} testkey.pk8 encontrado"
    size=$(wc -c < "$certs_path/testkey.pk8")
    echo "   Tamanho: $size bytes"
else
    echo -e "${RED}✗${NC} testkey.pk8 não encontrado!"
    ((errors++))
fi

if [ -f "$certs_path/testkey.x509.pem" ]; then
    echo -e "${GREEN}✓${NC} testkey.x509.pem encontrado"
else
    echo -e "${RED}✗${NC} testkey.x509.pem não encontrado!"
    ((errors++))
fi
echo ""

# Verificar estrutura do projeto
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "5. Verificando estrutura do projeto..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

check_dir() {
    if [ -d "$1" ]; then
        echo -e "${GREEN}✓${NC} $1/"
    else
        echo -e "${YELLOW}⚠${NC} $1/ não encontrado"
    fi
}

check_file() {
    if [ -f "$1" ]; then
        echo -e "${GREEN}✓${NC} $1"
    else
        echo -e "${RED}✗${NC} $1 não encontrado!"
        ((errors++))
    fi
}

check_dir "Models"
check_dir "Services"
check_dir "ViewModels"
check_dir "Resources"
check_file "ApkToAabConverter.csproj"
check_file "MainPage.xaml"
check_file "README.md"
echo ""

# Verificar Git
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "6. Verificando Git..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if check_command git "Git"; then
    git_version=$(git --version)
    echo "   $git_version"
    
    if [ -d ".git" ]; then
        echo -e "   ${GREEN}✓${NC} Repositório Git inicializado"
        branch=$(git branch --show-current 2>/dev/null)
        if [ -n "$branch" ]; then
            echo "   Branch atual: $branch"
        fi
        commits=$(git rev-list --count HEAD 2>/dev/null)
        if [ -n "$commits" ]; then
            echo "   Commits: $commits"
        fi
    else
        echo -e "   ${YELLOW}⚠${NC} Repositório Git não inicializado"
    fi
fi
echo ""

# Resumo
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "Resumo da Verificação"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [ $errors -eq 0 ]; then
    echo -e "${GREEN}✓ Todas as verificações passaram!${NC}"
    echo ""
    echo "O projeto está pronto para uso."
    echo "Execute: dotnet run -f net9.0-maccatalyst"
    exit 0
else
    echo -e "${RED}✗ $errors erro(s) encontrado(s)${NC}"
    echo ""
    echo "Por favor, corrija os problemas acima antes de continuar."
    echo "Consulte INSTALLATION.md para instruções detalhadas."
    exit 1
fi
