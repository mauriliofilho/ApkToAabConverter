# APK to AAB Converter for macOS

Um aplicativo nativo macOS desenvolvido em C# usando .NET MAUI para converter arquivos APK Android para o formato AAB (Android App Bundle) e assiná-los com certificados.

![Platform](https://img.shields.io/badge/platform-macOS-lightgrey)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)

## 📋 Sobre o Projeto

Este projeto foi criado como uma alternativa nativa para macOS ao aplicativo Android original "APK to AAB Converter". Ele permite que desenvolvedores Android convertam facilmente seus arquivos APK para o formato AAB exigido pela Google Play Store, além de oferecer funcionalidades de assinatura com certificados personalizados ou o certificado padrão do Android.

### ✨ Funcionalidades Principais

- **Conversão APK → AAB**: Converte arquivos APK para o formato Android App Bundle
- **Assinatura de AAB**: Assina arquivos AAB com certificados digitais
- **Certificado Padrão**: Inclui o certificado padrão do Android (testkey) para testes
- **Certificados Personalizados**: Suporte para keystores personalizados (.jks, .keystore, .p12)
- **Interface Nativa**: UI nativa macOS com design moderno e intuitivo
- **Logs em Tempo Real**: Visualização de console com progresso detalhado
- **Operação Combinada**: Converta e assine em uma única operação

## 🚀 Tecnologias Utilizadas

- **.NET 9.0** - Framework multiplataforma da Microsoft
- **.NET MAUI** - Multi-platform App UI para macOS
- **C#** - Linguagem de programação principal
- **bundletool** - Ferramenta oficial do Google para manipulação de AAB
- **Java JDK** - Necessário para bundletool e jarsigner

## 📦 Pré-requisitos

Antes de executar o projeto, certifique-se de ter instalado:

1. **.NET 9.0 SDK** ou superior
   ```bash
   brew install --cask dotnet-sdk
   ```

2. **Java JDK** (versão 11 ou superior)
   ```bash
   brew install openjdk@17
   ```

3. **Visual Studio 2022 para Mac** ou **Visual Studio Code** com extensão C#
   ```bash
   brew install --cask visual-studio-code
   ```

4. **Workload .NET MAUI**
   ```bash
   dotnet workload install maui
   ```

## 🛠️ Instalação e Configuração

### 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/ApkToAabConverter.git
cd ApkToAabConverter
```

### 2. Instale as dependências

O projeto já inclui os certificados padrão. Você precisará baixar o bundletool:

```bash
# Criar diretório de ferramentas
mkdir -p Resources/Tools

# Baixar bundletool (versão mais recente)
curl -L -o Resources/Tools/bundletool.jar \
  https://github.com/google/bundletool/releases/latest/download/bundletool-all.jar
```

### 3. Restaure os pacotes NuGet

```bash
dotnet restore
```

### 4. Build do projeto

```bash
dotnet build -f net9.0-maccatalyst
```

### 5. Execute o aplicativo

```bash
dotnet run -f net9.0-maccatalyst
```

Ou abra o projeto no Visual Studio e pressione F5.

## 📖 Como Usar

### Conversão Simples de APK para AAB

1. **Selecione o APK**: Clique em "Select APK File" e escolha seu arquivo .apk
2. **Defina a Saída**: O caminho de saída será gerado automaticamente no mesmo diretório
3. **Converta**: Clique em "Convert APK to AAB"
4. **Pronto!**: O arquivo .aab será criado no local especificado

### Conversão e Assinatura com Certificado Padrão

1. **Selecione o APK**: Escolha o arquivo .apk desejado
2. **Use Certificado Padrão**: Deixe marcada a opção "Use Default Android Certificate"
3. **Converta e Assine**: Clique em "Convert and Sign AAB"
4. **Resultado**: AAB convertido e assinado com o certificado de debug do Android

### Usar Certificado Personalizado

1. **Desmarque**: "Use Default Android Certificate"
2. **Selecione Keystore**: Clique em "Select Custom Certificate" e escolha seu arquivo .jks/.keystore
3. **Forneça Credenciais**: Insira:
   - Password do Keystore
   - Alias da Chave
   - Password da Chave
4. **Converta e Assine**: Clique em "Convert and Sign AAB"

## 🏗️ Estrutura do Projeto

```
ApkToAabConverter/
├── Models/
│   ├── ConversionResult.cs      # Resultado de operações
│   └── CertificateInfo.cs       # Informações de certificado
├── Services/
│   ├── IApkToAabService.cs      # Interface do serviço
│   └── ApkToAabService.cs       # Implementação do serviço
├── ViewModels/
│   └── MainViewModel.cs         # ViewModel principal (MVVM)
├── Resources/
│   ├── Tools/
│   │   └── bundletool.jar       # Bundletool do Google
│   └── Certificates/
│       ├── testkey.pk8           # Chave privada padrão
│       └── testkey.x509.pem     # Certificado padrão
├── MainPage.xaml                # UI principal
├── MainPage.xaml.cs             # Code-behind
└── ApkToAabConverter.csproj     # Arquivo do projeto
```

## 🔧 Arquitetura

O projeto segue o padrão **MVVM (Model-View-ViewModel)**:

- **Models**: Representam dados e lógica de negócio
- **Views**: Interface do usuário (XAML)
- **ViewModels**: Lógica de apresentação e binding de dados
- **Services**: Serviços para conversão e assinatura

### Fluxo de Conversão

```
APK File → bundletool → AAB File → jarsigner → Signed AAB
```

1. **Extração**: O APK é processado pelo bundletool
2. **Conversão**: Geração do arquivo AAB
3. **Assinatura** (opcional): AAB é assinado com jarsigner
4. **Verificação**: Validação da assinatura digital

## 🔐 Certificados e Segurança

### Certificado Padrão (testkey)

O projeto inclui o certificado padrão do Android (`testkey`) que é usado **apenas para desenvolvimento e testes**. 

⚠️ **IMPORTANTE**: Nunca use este certificado para publicar aplicativos em produção!

### Certificados Personalizados

Para produção, sempre use seu próprio certificado:

```bash
# Criar um novo keystore
keytool -genkey -v -keystore my-release-key.jks \
  -keyalg RSA -keysize 2048 -validity 10000 \
  -alias my-alias
```

## 🐛 Solução de Problemas

### "Java não encontrado"

```bash
# Verificar instalação do Java
java -version

# Definir JAVA_HOME
export JAVA_HOME=$(/usr/libexec/java_home)
```

### "Bundletool não encontrado"

Certifique-se de que o bundletool.jar está em `Resources/Tools/`:

```bash
ls -la Resources/Tools/bundletool.jar
```

### Erro de Permissão

Se encontrar erros de permissão no macOS:

```bash
# Dar permissão de execução
chmod +x Resources/Tools/*
```

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para:

1. Fork o projeto
2. Criar uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abrir um Pull Request

## 📝 Melhores Práticas Implementadas

### Código
- ✅ Arquitetura MVVM
- ✅ Injeção de dependências
- ✅ Async/Await para operações I/O
- ✅ Tratamento de exceções robusto
- ✅ Logging detalhado

### Interface
- ✅ UI responsiva e moderna
- ✅ Feedback visual em tempo real
- ✅ Validação de entrada
- ✅ Mensagens de erro claras

### Git
- ✅ .gitignore configurado
- ✅ Commits semânticos
- ✅ README detalhado
- ✅ Estrutura organizada

## 📜 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 👨‍💻 Autor

Desenvolvido com ❤️ para a comunidade macOS e Android.

## 🙏 Agradecimentos

- Projeto original: **APK to AAB Converter** (Android)
- Google: **bundletool** e ferramentas Android
- Microsoft: **.NET MAUI** framework
- Comunidade: **Open Source**

## 📚 Recursos Adicionais

- [Documentação oficial do Android App Bundle](https://developer.android.com/guide/app-bundle)
- [bundletool no GitHub](https://github.com/google/bundletool)
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui)
- [Android Signing](https://developer.android.com/studio/publish/app-signing)

## 🔄 Roadmap

- [ ] Suporte para conversão em lote
- [ ] Validação de APK antes da conversão
- [ ] Histórico de conversões
- [ ] Exportação de logs
- [ ] Temas claro/escuro
- [ ] Localização (PT-BR, EN, ES)
- [ ] Instalação via Homebrew
- [ ] Integração com CI/CD

---

**Nota**: Este é um projeto independente e não é afiliado ao Google ou Android.
