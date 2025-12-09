namespace Fase3_OO_ComHeranca
{
    /// <summary>
    /// Implementação concreta: cobrança progressiva.
    /// Aplica taxa diferente conforme o valor da cobrança.
    /// - Até R$ 100: sem taxa
    /// - De R$ 100 a R$ 500: 5% de taxa
    /// - Acima de R$ 500: 8% de taxa
    /// Se está inadimplente, adiciona 3% extra sobre a taxa.
    /// </summary>
    public sealed class ProgressiveChargingStrategy : ChargingStrategyBase
    {
        protected override double Apply(double amount, bool isDefaulter)
        {
            double rate = amount switch
            {
                <= 100 => 0.00,
                <= 500 => 0.05,
                _ => 0.08
            };

            double charged = amount * (1 + rate);

            // Acréscimo extra se inadimplente
            if (isDefaulter)
            {
                charged *= 1.03; // +3% extra para inadimplentes
            }

            return charged;
        }
    }
}
