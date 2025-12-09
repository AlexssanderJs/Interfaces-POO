namespace Fase4_ComInterfaces
{
    /// <summary>
    /// Implementação concreta: cobrança premium com desconto.
    /// Clientes premium recebem 5% de desconto.
    /// Mesmo inadimplentes premium recebem o desconto (estratégia de retenção).
    /// </summary>
    public sealed class PremiumChargingStrategy : IChargingStrategy
    {
        private const double DiscountRate = 0.05; // 5% desconto

        public double Calculate(double amount, bool isDefaulter)
        {
            // Cliente premium sempre tem desconto
            return amount * (1 - DiscountRate);
        }
    }
}
