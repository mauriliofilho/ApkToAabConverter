# 🚀 Guia de Publicação no GitHub

## Método 1: Script Automático (Recomendado)

### Passo 1: Execute o script
```bash
./setup_github.sh
```

O script irá:
1. Solicitar seu usuário e nome do repositório
2. Guiá-lo na criação do repo no GitHub
3. Configurar o remote automaticamente
4. Fazer o push inicial

---

## Método 2: Manual

### Passo 1: Criar Repositório no GitHub

1. Acesse: **https://github.com/new**

2. Preencha:
   - **Repository name**: `ApkToAabConverter`
   - **Description**: `APK to AAB Converter - Native macOS app for converting Android APK to AAB format and signing with certificates`
   - **Visibility**: Public ou Private
   - ⚠️ **NÃO marque**: "Initialize this repository with a README"

3. Clique em **"Create repository"**

### Passo 2: Conectar Repositório Local

No terminal, execute:

```bash
cd /Users/mauriliofilho/dev/ApkToAabConverter

# Adicionar remote (substitua SEU_USUARIO pelo seu usuário do GitHub)
git remote add origin https://github.com/SEU_USUARIO/ApkToAabConverter.git

# Verificar
git remote -v

# Push inicial
git push -u origin main
```

### Passo 3: Verificar

Acesse: `https://github.com/SEU_USUARIO/ApkToAabConverter`

Você deve ver todos os arquivos do projeto!

---

## Método 3: Com SSH (Mais Seguro)

### Se você tem SSH configurado:

```bash
# Adicionar remote com SSH
git remote add origin git@github.com:SEU_USUARIO/ApkToAabConverter.git

# Push
git push -u origin main
```

### Se NÃO tem SSH configurado:

1. **Gerar chave SSH**:
   ```bash
   ssh-keygen -t ed25519 -C "seu-email@exemplo.com"
   ```

2. **Copiar chave pública**:
   ```bash
   cat ~/.ssh/id_ed25519.pub | pbcopy
   ```

3. **Adicionar no GitHub**:
   - Acesse: https://github.com/settings/keys
   - Clique em "New SSH key"
   - Cole a chave e salve

4. **Testar**:
   ```bash
   ssh -T git@github.com
   ```

---

## Método 4: Via GitHub CLI (Se instalado)

### Instalar GitHub CLI:
```bash
brew install gh
```

### Autenticar:
```bash
gh auth login
```

### Criar e publicar:
```bash
cd /Users/mauriliofilho/dev/ApkToAabConverter

# Criar repositório e fazer push
gh repo create ApkToAabConverter --public --source=. --push

# Ou privado:
# gh repo create ApkToAabConverter --private --source=. --push
```

---

## 🔐 Autenticação

### Opção A: Personal Access Token (HTTPS)

1. **Criar Token**:
   - Acesse: https://github.com/settings/tokens
   - "Generate new token" → "Generate new token (classic)"
   - Nome: "ApkToAabConverter"
   - Scopes: Marque `repo`
   - "Generate token"
   - **COPIE O TOKEN** (você não verá novamente!)

2. **Usar no push**:
   ```bash
   git push -u origin main
   ```
   - Username: seu usuário do GitHub
   - Password: **COLE O TOKEN** (não sua senha!)

3. **Salvar credenciais** (opcional):
   ```bash
   git config --global credential.helper osxkeychain
   ```

### Opção B: SSH (Recomendado)

Ver "Método 3" acima.

---

## 📝 Após Publicar

### 1. Adicionar Topics/Tags

No GitHub, vá em "Settings" → "General" e adicione topics:

```
csharp
dotnet
maui
macos
android
apk
aab
bundletool
converter
tool
```

### 2. Configurar About

Edite a seção "About" com:
- ✅ Website (se tiver)
- ✅ Topics (acima)
- ✅ Marque "Packages" e "Releases" se aplicável

### 3. Adicionar Badges ao README

Edite o `README.md` e adicione no topo:

```markdown
![Platform](https://img.shields.io/badge/platform-macOS-lightgrey)
![.NET](https://img.shields.io/badge/.NET-9.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![GitHub last commit](https://img.shields.io/github/last-commit/SEU_USUARIO/ApkToAabConverter)
![GitHub stars](https://img.shields.io/github/stars/SEU_USUARIO/ApkToAabConverter)
```

### 4. Criar Release (Opcional)

```bash
# Via CLI
gh release create v1.0.0 --title "v1.0.0 - Initial Release" --notes "First stable release"

# Ou manualmente no GitHub:
# https://github.com/SEU_USUARIO/ApkToAabConverter/releases/new
```

### 5. Adicionar Screenshot (Recomendado)

1. Tire um screenshot do app rodando
2. Adicione à pasta `docs/screenshots/`
3. Inclua no README:
   ```markdown
   ## Screenshots
   
   ![App Screenshot](docs/screenshots/main-screen.png)
   ```

---

## 🔄 Workflow Git Futuro

### Para mudanças futuras:

```bash
# Fazer alterações no código
# ...

# Ver status
git status

# Adicionar arquivos
git add .

# Commit
git commit -m "feat: adiciona nova funcionalidade"

# Push
git push
```

### Tipos de commit semânticos:

- `feat:` - Nova funcionalidade
- `fix:` - Correção de bug
- `docs:` - Documentação
- `style:` - Formatação
- `refactor:` - Refatoração
- `test:` - Testes
- `chore:` - Manutenção

---

## 🆘 Solução de Problemas

### Erro: "failed to push some refs"

```bash
# Pull primeiro
git pull origin main --rebase

# Depois push
git push -u origin main
```

### Erro: "Authentication failed"

- Use Personal Access Token (não sua senha)
- Ou configure SSH

### Erro: "remote origin already exists"

```bash
# Remover e adicionar novamente
git remote remove origin
git remote add origin https://github.com/SEU_USUARIO/ApkToAabConverter.git
```

### Ver logs de push

```bash
GIT_CURL_VERBOSE=1 git push -u origin main
```

---

## ✅ Checklist Final

Após publicar, verifique:

- [ ] Todos os arquivos estão no GitHub
- [ ] README exibe corretamente
- [ ] Links no README funcionam
- [ ] Licença está visível
- [ ] Topics/tags configurados
- [ ] About section preenchido
- [ ] .gitignore funcionando (sem keystores!)

---

## 🎯 Próximos Passos

1. **Star** seu próprio repositório (para testar)
2. Compartilhe com a comunidade
3. Considere:
   - GitHub Actions para CI/CD
   - GitHub Pages para documentação
   - GitHub Discussions para comunidade
   - Issue templates
   - Pull request templates

---

## 📞 Precisa de Ajuda?

- [GitHub Docs](https://docs.github.com)
- [Git Basics](https://git-scm.com/book/en/v2/Getting-Started-Git-Basics)
- [SSH Keys](https://docs.github.com/authentication/connecting-to-github-with-ssh)

---

**Boa sorte com seu projeto! 🚀**
