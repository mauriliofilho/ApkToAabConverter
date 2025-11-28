# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [1.0.0] - 2024-11-28

### Adicionado
- Projeto inicial .NET MAUI para macOS
- Conversão de APK para AAB usando bundletool
- Assinatura de AAB com jarsigner
- Interface de usuário nativa macOS
- Suporte para certificado padrão Android (testkey)
- Suporte para certificados personalizados (.jks, .keystore, .p12)
- Sistema de logs em tempo real
- Validação de ferramentas (Java, bundletool)
- Conversão e assinatura em operação única
- Barra de progresso visual
- Feedback de status em tempo real
- Documentação completa (README, INSTALLATION, LICENSE)
- Arquitetura MVVM
- Tratamento robusto de erros
- Diálogos de seleção de arquivos nativos

### Funcionalidades
- ✅ Conversão APK → AAB
- ✅ Assinatura de AAB
- ✅ Conversão + Assinatura combinada
- ✅ Certificado padrão Android
- ✅ Certificados personalizados
- ✅ Console de logs
- ✅ Validação de entrada
- ✅ Mensagens de erro amigáveis
- ✅ Interface responsiva
- ✅ Operações assíncronas

### Tecnologias
- .NET 9.0
- .NET MAUI
- C# 12
- bundletool (Google)
- Java JDK
- Git

### Estrutura do Projeto
```
ApkToAabConverter/
├── Models/              # Modelos de dados
├── Services/            # Lógica de negócio
├── ViewModels/          # ViewModels MVVM
├── Resources/           # Recursos do app
│   ├── Tools/          # bundletool
│   └── Certificates/   # Certificados
├── MainPage.xaml        # UI principal
└── README.md            # Documentação
```

### Configuração Git
- Repositório inicializado
- .gitignore configurado
- Commit semântico
- Branch principal: main
- Licença MIT

## [Futuro] - Planejado

### A Adicionar
- [ ] Conversão em lote
- [ ] Validação de APK
- [ ] Histórico de conversões
- [ ] Exportação de logs
- [ ] Temas claro/escuro
- [ ] Localização (PT-BR, EN, ES)
- [ ] Instalação via Homebrew
- [ ] Integração CI/CD
- [ ] Testes unitários
- [ ] Testes de integração
- [ ] Suporte para AAB → APK
- [ ] Informações do APK/AAB
- [ ] Comparação de arquivos
- [ ] Otimização de AAB

### Melhorias Planejadas
- [ ] Performance em arquivos grandes
- [ ] Cache de conversões
- [ ] Pré-visualização de arquivos
- [ ] Atalhos de teclado
- [ ] Drag & drop de arquivos
- [ ] Notificações do sistema
- [ ] Atualizações automáticas
- [ ] Telemetria (opcional)

---

**Formato de Versão**: MAJOR.MINOR.PATCH

- **MAJOR**: Mudanças incompatíveis na API
- **MINOR**: Novas funcionalidades compatíveis
- **PATCH**: Correções de bugs compatíveis
