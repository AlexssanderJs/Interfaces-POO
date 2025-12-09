namespace Fase4_ComInterfaces
{
    /// <summary>
    /// Implementação concreta: cobrança padrão sem acréscimo.
    /// Estratégia mais simples - retorna o valor nominal.
    /// </summary>
    public sealed class StandardChargingStrategy : IChargingStrategy
    {
        public double Calculate(double amount, bool isDefaulter)
        {
            // Cobrança padrão: sem alteração no valor
            return amount;
        }
    }
}
