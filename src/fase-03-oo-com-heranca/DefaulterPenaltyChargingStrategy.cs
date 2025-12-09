namespace Fase3_OO_ComHeranca
{
    /// <summary>
    /// Implementação concreta: cobrança com penalidade para inadimplentes.
    /// Aplica uma taxa adicional (p.ex., 10%) apenas se o cliente está inadimplente.
    /// </summary>
    public sealed class DefaulterPenaltyChargingStrategy : ChargingStrategyBase
    {
        private const double DefaulterTaxRate = 0.10; // 10% de penalidade

        protected override double Apply(double amount, bool isDefaulter)
        {
            if (isDefaulter)
            {
                // Aplica acréscimo para inadimplente
                return amount * (1 + DefaulterTaxRate);
            }

            // Sem penalidade
            return amount;
        }
    }
}
