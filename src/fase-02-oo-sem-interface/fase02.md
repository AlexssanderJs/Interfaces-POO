## Exemplo básico de implementação em código
Sistema de cobrança

### cobranca.cs
```
    cobranca(double valor, bool ehInadimplente, float taxa) {
        if (ehInadimplente)
            valor = valor * taxa;

        return valor
    }
```

## Imagine o seguinte
    Caso as regras de negócio mudem e a taxa seja diferente, aonde o admnistrador irá mudar este valor? A implementação de uma taxa sem a implementação de um arquivo de configurações será errado, é essencial que sejam feitas configurações e testes para garantir a integridade deste funcionalidade tão importante.

### Testes de Qualidade
    É essencial que seja testada a taxa sendo -1%, 0%, 0,1%, 100%, 101%, etc. Para assim, garantir os testes de fronteira.

### Testes de escalabilidade
    