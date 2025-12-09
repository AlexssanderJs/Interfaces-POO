namespace Fase3_OO_ComHeranca
{
    /// <summary>
    /// Implementação concreta: cobrança padrão sem acréscimo.
    /// Cada cliente paga o valor nominal.
    /// </summary>
    public sealed class StandardChargingStrategy : ChargingStrategyBase
    {
        protected override double Apply(double amount, bool isDefaulter)
        {
            // Cobrança padrão: retorna o valor como está
            return amount;
        }
    }
}
