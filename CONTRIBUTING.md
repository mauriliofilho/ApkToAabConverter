# Contribuindo para APK to AAB Converter

Obrigado por considerar contribuir para o projeto! 🎉

## Como Posso Contribuir?

### Reportando Bugs

Se você encontrou um bug, por favor crie uma issue incluindo:

- Descrição clara do problema
- Passos para reproduzir
- Comportamento esperado vs. comportamento atual
- Versão do sistema operacional
- Versão do .NET
- Logs relevantes

### Sugerindo Melhorias

Sugestões são bem-vindas! Crie uma issue com:

- Descrição da melhoria
- Por que ela seria útil
- Exemplos de uso (se aplicável)

### Pull Requests

1. **Fork o Projeto**
   ```bash
   git clone https://github.com/seu-usuario/ApkToAabConverter.git
   cd ApkToAabConverter
   ```

2. **Crie uma Branch**
   ```bash
   git checkout -b feature/minha-feature
   # ou
   git checkout -b fix/meu-bug
   ```

3. **Faça suas Mudanças**
   - Siga os padrões de código
   - Adicione comentários quando necessário
   - Atualize a documentação

4. **Commit suas Mudanças**
   
   Use commits semânticos:
   ```bash
   git commit -m "feat: adiciona nova funcionalidade X"
   git commit -m "fix: corrige bug Y"
   git commit -m "docs: atualiza README"
   ```

   Tipos de commit:
   - `feat`: Nova funcionalidade
   - `fix`: Correção de bug
   - `docs`: Documentação
   - `style`: Formatação
   - `refactor`: Refatoração
   - `test`: Testes
   - `chore`: Tarefas de manutenção

5. **Push para o GitHub**
   ```bash
   git push origin feature/minha-feature
   ```

6. **Abra um Pull Request**

## Padrões de Código

### C#

- Use PascalCase para classes, métodos e propriedades
- Use camelCase para variáveis locais
- Use _ para campos privados
- Adicione comentários XML para APIs públicas
- Siga as convenções do C# 12

Exemplo:
```csharp
/// <summary>
/// Converte APK para AAB
/// </summary>
/// <param name="apkPath">Caminho do APK</param>
/// <returns>Resultado da conversão</returns>
public async Task<ConversionResult> ConvertAsync(string apkPath)
{
    var result = new ConversionResult();
    // ...
    return result;
}
```

### XAML

- Use indentação de 4 espaços
- Mantenha atributos em ordem lógica
- Use bindings quando apropriado

### Git

- Commits pequenos e focados
- Mensagens claras e descritivas
- Use commits semânticos

## Estrutura do Projeto

```
ApkToAabConverter/
├── Models/              # Modelos de dados
├── Services/            # Serviços e lógica
├── ViewModels/          # ViewModels MVVM
├── Resources/           # Recursos (imagens, etc)
├── MainPage.xaml        # UI principal
└── Tests/              # Testes (futuro)
```

## Testando

Antes de submeter um PR:

1. Teste todas as funcionalidades
2. Verifique se não há erros de compilação
3. Execute o app em diferentes cenários
4. Teste com diferentes tamanhos de APK

## Código de Conduta

### Nossos Valores

- Seja respeitoso e inclusivo
- Aceite críticas construtivas
- Foque no que é melhor para a comunidade
- Mostre empatia com outros membros

### Comportamentos Inaceitáveis

- Linguagem ofensiva ou discriminatória
- Assédio de qualquer tipo
- Trolling ou comentários depreciativos
- Publicação de informações privadas

## Licença

Ao contribuir, você concorda que suas contribuições serão licenciadas sob a Licença MIT.

## Dúvidas?

Sinta-se à vontade para abrir uma issue com suas dúvidas!

## Reconhecimento

Todos os contribuidores serão listados no README.md! 🌟

---

**Obrigado por contribuir!** ❤️
