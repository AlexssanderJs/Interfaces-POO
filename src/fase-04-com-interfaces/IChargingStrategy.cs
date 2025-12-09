namespace Fase4_ComInterfaces
{
    /// <summary>
    /// Contrato explícito para estratégias de cobrança.
    /// Define O QUÊ a estratégia deve fazer, não o COMO.
    /// Permite que o cliente dependa apenas do contrato, não dos concretos.
    /// </summary>
    public interface IChargingStrategy
    {
        /// <summary>
        /// Calcula o valor da cobrança baseado no valor original e status de inadimplência.
        /// </summary>
        /// <param name="amount">Valor original da cobrança</param>
        /// <param name="isDefaulter">Indica se o cliente está inadimplente</param>
        /// <returns>Valor final da cobrança após aplicar a estratégia</returns>
        double Calculate(double amount, bool isDefaulter);
    }
}
