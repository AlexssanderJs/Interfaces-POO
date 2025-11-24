# Repository-JSON

Este é um exemplo de implementação do padrão **Repository** em C# utilizando persistência em arquivo JSON.

## Objetivo
Demonstrar o desacoplamento entre a aplicação e a camada de acesso a dados através de um contrato único (Interface), com dados persistidos em JSON.

## Estrutura
- **Transaction.cs**: Entidade de transação bancária.
- **IRepository.cs**: Interface genérica que define o contrato.
- **TransactionRepository.cs**: Implementação original do repositório usando `List` em memória (mantida para referência).
- **JsonTransactionRepository.cs**: Implementação do repositório com persistência em JSON.
- **Program.cs**: Aplicação console que testa o fluxo usando o repositório JSON.
- **transactions.json**: Arquivo JSON que armazena as transações persistidas.

## Funcionalidades
- ✅ **Add**: Adiciona nova transação e persiste em JSON
- ✅ **GetAll**: Lista todas as transações carregadas do arquivo
- ✅ **GetById**: Busca transação por ID
- ✅ **Update**: Atualiza transação existente e salva em JSON
- ✅ **Delete**: Remove transação e persiste mudanças em JSON

## Como Executar
Certifique-se de ter o .NET SDK 9.0 ou superior instalado e execute:

```bash
dotnet run
```

Os dados serão automaticamente salvos em `transactions.json` no diretório do projeto e carregados na próxima execução.
