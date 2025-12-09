namespace Fase4_ComInterfaces
{
    /// <summary>
    /// Implementação concreta: cobrança com taxa progressiva.
    /// A taxa varia conforme o valor da cobrança:
    /// - Até R$ 100: sem taxa
    /// - R$ 100 a R$ 500: 5%
    /// - Acima de R$ 500: 8%
    /// Inadimplentes pagam 3% adicional sobre o total.
    /// </summary>
    public sealed class ProgressiveChargingStrategy : IChargingStrategy
    {
        public double Calculate(double amount, bool isDefaulter)
        {
            // Determina a taxa baseada no valor
            double rate = amount switch
            {
                <= 100 => 0.00,
                <= 500 => 0.05,
                _ => 0.08
            };

            double charged = amount * (1 + rate);

            // Acréscimo extra para inadimplentes
            if (isDefaulter)
            {
                charged *= 1.03;
            }

            return charged;
        }
    }
}
