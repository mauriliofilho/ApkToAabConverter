# Instruções para Download do Bundletool

O bundletool é necessário para converter APK em AAB. 

## Opção 1: Download Manual

1. Visite: https://github.com/google/bundletool/releases
2. Baixe a versão mais recente: `bundletool-all-X.X.X.jar`
3. Renomeie para `bundletool.jar`
4. Mova para: `Resources/Tools/bundletool.jar`

## Opção 2: Via Script (Recomendado)

Execute o script de download incluído:

```bash
./download_bundletool.sh
```

## Opção 3: Via Terminal

```bash
# Criar diretório se não existir
mkdir -p Resources/Tools

# Baixar última versão
curl -L -o Resources/Tools/bundletool.jar \
  https://github.com/google/bundletool/releases/latest/download/bundletool-all.jar

# Verificar
java -jar Resources/Tools/bundletool.jar version
```

## Verificação

Após o download, execute:

```bash
./check_installation.sh
```

Isso verificará se o bundletool está instalado corretamente.

## Solução de Problemas

Se o bundletool não funcionar:

1. **Verificar Java**:
   ```bash
   java -version
   ```
   Deve ser Java 11 ou superior.

2. **Testar bundletool manualmente**:
   ```bash
   java -jar Resources/Tools/bundletool.jar version
   ```

3. **Verificar permissões**:
   ```bash
   chmod 644 Resources/Tools/bundletool.jar
   ```

## Informações do Bundletool

- **Repositório**: https://github.com/google/bundletool
- **Documentação**: https://developer.android.com/studio/command-line/bundletool
- **Licença**: Apache 2.0
- **Versão Mínima**: 1.0.0
- **Versão Recomendada**: Última disponível

## Notas

- O bundletool é uma ferramenta oficial do Google
- É necessário para todas as operações de conversão APK → AAB
- Atualizado regularmente com novas funcionalidades
- Compatível com todas as versões do Android App Bundle

---

**Importante**: Este arquivo não está incluído no Git devido ao seu tamanho. Você deve baixá-lo separadamente.
