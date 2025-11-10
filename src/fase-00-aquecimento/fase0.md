### Exemplo 1:

## Envio de notificação de aplicativo de banco: 
contrato = “transação do usuário”; 
implementações = Atualização de informações cadastradas / Transações financeiras; 
política = “se usuário Atualização→ Protocolo API: REST;
 se não → Protocolo API: SOAP;”.

 *O protocolo REST é mais fácil de implementar mas menos seguro, já o protocolo SOAP é mais seguro, recomendado para transações financeiras.

### Exemplo 2:
#### Cobrança de pagamento serviço streaming
contrato = “pagamento serviço streaming”
implementações: Cobrança com taxa / Cobrança normal 
política: “Se usuário está inadimplente → Cobrança com taxa;
Se não → Cobrança normal;”.
