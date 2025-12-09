using System;

namespace Fase4_ComInterfaces
{
    /// <summary>
    /// Cliente que depende APENAS do contrato (interface).
    /// Não conhece implementações concretas - recebe a estratégia via construtor (DI).
    /// Princípio de Inversão de Dependência: depende de abstração, não de concreto.
    /// </summary>
    public class BillingService
    {
        private readonly IChargingStrategy _strategy;

        /// <summary>
        /// Construtor com injeção de dependência.
        /// Recebe a estratégia via contrato - não sabe qual implementação concreta é.
        /// </summary>
        public BillingService(IChargingStrategy strategy)
        {
            _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        }

        /// <summary>
        /// Processa a cobrança usando a estratégia injetada.
        /// Lógica do cliente é linear - sem ramificações, sem conhecimento de concretos.
        /// </summary>
        public double ProcessBilling(double amount, bool isDefaulter)
        {
            // Validação de negócio
            if (amount < 0)
            {
                throw new ArgumentException("Valor não pode ser negativo.", nameof(amount));
            }

            // Delegação para a estratégia (polimorfismo via interface)
            return _strategy.Calculate(amount, isDefaulter);
        }

        /// <summary>
        /// Gera relatório formatado da cobrança.
        /// Demonstra que o cliente pode ter lógica adicional sem conhecer a estratégia.
        /// </summary>
        public string GenerateReport(double amount, bool isDefaulter)
        {
            var finalAmount = ProcessBilling(amount, isDefaulter);
            var status = isDefaulter ? "Inadimplente" : "Adimplente";
            var difference = finalAmount - amount;

            return $"Status: {status}\n" +
                   $"Valor original: R$ {amount:F2}\n" +
                   $"Ajuste: R$ {difference:F2}\n" +
                   $"Valor final: R$ {finalAmount:F2}";
        }
    }
}
