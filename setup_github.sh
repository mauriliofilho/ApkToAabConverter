#!/bin/bash

# Script para configurar repositório GitHub
# APK to AAB Converter for macOS

echo "======================================"
echo "  GitHub Repository Setup"
echo "======================================"
echo ""

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}Este script ajudará a conectar seu repositório local ao GitHub${NC}"
echo ""

# Solicitar informações
echo "Por favor, forneça as seguintes informações:"
echo ""
read -p "Seu usuário do GitHub: " github_user
read -p "Nome do repositório (ex: ApkToAabConverter): " repo_name

echo ""
echo -e "${YELLOW}Agora, siga estes passos:${NC}"
echo ""
echo "1. Acesse: https://github.com/new"
echo "2. Repository name: $repo_name"
echo "3. Description: APK to AAB Converter - Native macOS app for converting Android APK to AAB"
echo "4. Public ou Private (sua escolha)"
echo "5. NÃO inicialize com README, .gitignore ou LICENSE (já temos esses arquivos!)"
echo "6. Clique em 'Create repository'"
echo ""

read -p "Pressione Enter quando criar o repositório no GitHub..."

echo ""
echo -e "${GREEN}Configurando remote...${NC}"

# Construir URL do repositório
repo_url="https://github.com/$github_user/$repo_name.git"

# Adicionar remote
git remote add origin "$repo_url"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Remote 'origin' adicionado com sucesso!${NC}"
else
    echo -e "${RED}✗ Erro ao adicionar remote${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}Verificando configuração...${NC}"
git remote -v

echo ""
echo -e "${BLUE}Push inicial para GitHub...${NC}"

# Fazer push
git push -u origin main

if [ $? -eq 0 ]; then
    echo ""
    echo "======================================"
    echo -e "${GREEN}✓ Sucesso!${NC}"
    echo "======================================"
    echo ""
    echo "Seu repositório está no ar!"
    echo "URL: https://github.com/$github_user/$repo_name"
    echo ""
    echo -e "${BLUE}Próximos passos sugeridos:${NC}"
    echo "  1. Configure os Topics no GitHub:"
    echo "     - csharp, dotnet, maui, macos, android"
    echo "     - apk, aab, bundletool, converter"
    echo ""
    echo "  2. Adicione uma imagem/screenshot ao README"
    echo ""
    echo "  3. Configure GitHub Pages (opcional)"
    echo ""
    echo "  4. Adicione badges ao README (opcional):"
    echo "     - Build status"
    echo "     - License"
    echo "     - Platform"
    echo ""
else
    echo ""
    echo -e "${RED}✗ Erro no push${NC}"
    echo ""
    echo "Possíveis soluções:"
    echo "  1. Verifique suas credenciais do GitHub"
    echo "  2. Configure um Personal Access Token:"
    echo "     https://github.com/settings/tokens"
    echo "  3. Configure SSH keys:"
    echo "     https://docs.github.com/authentication/connecting-to-github-with-ssh"
    echo ""
    echo "Para push manual, execute:"
    echo "  git push -u origin main"
    exit 1
fi
