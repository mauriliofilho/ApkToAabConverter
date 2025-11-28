# Notas de Instalação - APK to AAB Converter

## Requisitos do Sistema

- **Sistema Operacional**: macOS 12.0 (Monterey) ou superior
- **Arquitetura**: x64 ou ARM64 (M1/M2/M3)
- **RAM**: Mínimo 4GB, recomendado 8GB
- **Espaço em Disco**: 500MB livres
- **.NET Runtime**: 9.0 ou superior
- **Java JDK**: 11, 17 ou 21

## Instalação Passo a Passo

### 1. Instalar Dependências

#### Instalar Homebrew (se não tiver)
```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

#### Instalar .NET SDK
```bash
brew install --cask dotnet-sdk
dotnet --version  # Verificar instalação
```

#### Instalar Java JDK
```bash
brew install openjdk@17
sudo ln -sfn /opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk /Library/Java/JavaVirtualMachines/openjdk-17.jdk
java -version  # Verificar instalação
```

### 2. Configurar Variáveis de Ambiente

Adicione ao seu `~/.zshrc` ou `~/.bash_profile`:

```bash
# Java
export JAVA_HOME=$(/usr/libexec/java_home)
export PATH=$JAVA_HOME/bin:$PATH

# .NET
export DOTNET_ROOT=/opt/homebrew/opt/dotnet/libexec
export PATH=$DOTNET_ROOT:$PATH
```

Aplique as mudanças:
```bash
source ~/.zshrc  # ou source ~/.bash_profile
```

### 3. Clonar e Configurar o Projeto

```bash
# Clonar repositório
git clone https://github.com/seu-usuario/ApkToAabConverter.git
cd ApkToAabConverter

# Baixar bundletool
mkdir -p Resources/Tools
curl -L -o Resources/Tools/bundletool.jar \
  https://github.com/google/bundletool/releases/latest/download/bundletool-all.jar

# Verificar bundletool
java -jar Resources/Tools/bundletool.jar version

# Restaurar dependências
dotnet restore

# Instalar workload MAUI (se necessário)
dotnet workload install maui
```

### 4. Build e Execução

#### Build
```bash
dotnet build -f net9.0-maccatalyst -c Release
```

#### Executar
```bash
dotnet run -f net9.0-maccatalyst
```

#### Ou via Visual Studio
1. Abra `ApkToAabConverter.sln`
2. Selecione "macOS" como target
3. Pressione F5 ou clique em "Run"

### 5. Criar Pacote de Distribuição (Opcional)

```bash
dotnet publish -f net9.0-maccatalyst -c Release -p:CreatePackage=true
```

O aplicativo será criado em: `bin/Release/net9.0-maccatalyst/ApkToAabConverter.app`

## Solução de Problemas Comuns

### Erro: "Java not found"
```bash
# Verificar instalação
which java
java -version

# Se não encontrado, reinstalar
brew reinstall openjdk@17
```

### Erro: "dotnet command not found"
```bash
# Verificar instalação
which dotnet

# Adicionar ao PATH
export PATH="/opt/homebrew/bin:$PATH"
```

### Erro: "MAUI workload not found"
```bash
# Instalar workload
dotnet workload install maui

# Verificar workloads instalados
dotnet workload list
```

### Erro: "bundletool.jar not found"
```bash
# Baixar novamente
curl -L -o Resources/Tools/bundletool.jar \
  https://github.com/google/bundletool/releases/latest/download/bundletool-all.jar

# Verificar permissões
chmod 644 Resources/Tools/bundletool.jar
```

### Erro de Permissões no macOS
```bash
# Dar permissões necessárias
sudo xattr -r -d com.apple.quarantine ApkToAabConverter.app
```

## Verificação de Instalação

Execute o seguinte script para verificar todas as dependências:

```bash
#!/bin/bash

echo "Verificando instalação..."
echo ""

# Verificar .NET
echo "✓ Verificando .NET..."
if command -v dotnet &> /dev/null; then
    echo "  .NET version: $(dotnet --version)"
else
    echo "  ✗ .NET não encontrado!"
fi

# Verificar Java
echo ""
echo "✓ Verificando Java..."
if command -v java &> /dev/null; then
    java -version 2>&1 | head -n 1
else
    echo "  ✗ Java não encontrado!"
fi

# Verificar bundletool
echo ""
echo "✓ Verificando bundletool..."
if [ -f "Resources/Tools/bundletool.jar" ]; then
    java -jar Resources/Tools/bundletool.jar version 2>&1 | head -n 1
else
    echo "  ✗ bundletool.jar não encontrado!"
fi

# Verificar certificados
echo ""
echo "✓ Verificando certificados..."
if [ -f "Resources/Certificates/testkey.pk8" ]; then
    echo "  testkey.pk8 encontrado"
else
    echo "  ✗ testkey.pk8 não encontrado!"
fi

echo ""
echo "Verificação concluída!"
```

Salve como `check_installation.sh` e execute:
```bash
chmod +x check_installation.sh
./check_installation.sh
```

## Desinstalação

Para remover completamente o aplicativo:

```bash
# Remover aplicativo
rm -rf bin/
rm -rf obj/

# Remover dependências (opcional)
brew uninstall dotnet-sdk
brew uninstall openjdk@17

# Remover repositório
cd ..
rm -rf ApkToAabConverter
```

## Atualizações

Para atualizar o projeto:

```bash
# Atualizar código
git pull origin main

# Atualizar dependências
dotnet restore
dotnet workload update

# Atualizar bundletool
curl -L -o Resources/Tools/bundletool.jar \
  https://github.com/google/bundletool/releases/latest/download/bundletool-all.jar

# Rebuild
dotnet build -f net9.0-maccatalyst -c Release
```

## Suporte

Se encontrar problemas durante a instalação:

1. Verifique os logs em: `~/Library/Logs/ApkToAabConverter/`
2. Consulte as Issues no GitHub
3. Execute o script de verificação acima
4. Entre em contato através do repositório

## Notas de Segurança

- ⚠️ Nunca commite keystores de produção no Git
- ⚠️ Use o certificado testkey apenas para desenvolvimento
- ⚠️ Mantenha suas senhas seguras e não as compartilhe
- ✅ Use certificados próprios para aplicações em produção

## Performance

Para melhor performance:

- Certifique-se de ter pelo menos 8GB de RAM
- Use SSD para melhor I/O
- Feche outras aplicações pesadas durante conversões grandes
- Considere aumentar a heap do Java para arquivos grandes:
  ```bash
  export _JAVA_OPTIONS="-Xmx2G"
  ```

## Próximos Passos

Após a instalação bem-sucedida:

1. Leia o README.md principal
2. Teste com um APK pequeno primeiro
3. Configure seus certificados personalizados
4. Explore as opções de conversão

---

**Versão**: 1.0.0  
**Data**: Novembro 2024  
**Plataforma**: macOS (Catalyst)
