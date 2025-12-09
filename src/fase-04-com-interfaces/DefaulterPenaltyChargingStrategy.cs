namespace Fase4_ComInterfaces
{
    /// <summary>
    /// Implementação concreta: aplica penalidade para inadimplentes.
    /// Taxa fixa de 10% adicional para clientes inadimplentes.
    /// </summary>
    public sealed class DefaulterPenaltyChargingStrategy : IChargingStrategy
    {
        private const double PenaltyRate = 0.10; // 10%

        public double Calculate(double amount, bool isDefaulter)
        {
            if (isDefaulter)
            {
                return amount * (1 + PenaltyRate);
            }

            return amount;
        }
    }
}
