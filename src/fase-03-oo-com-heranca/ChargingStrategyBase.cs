using System;

namespace Fase3_OO_ComHeranca
{
    /// <summary>
    /// Classe base abstrata que orquestra o ritual de cobrança.
    /// Define o fluxo comum (contrato) e delega o "como calcular" para as subclasses.
    /// </summary>
    public abstract class ChargingStrategyBase
    {
        /// <summary>
        /// Ritual público de cobrança.
        /// O fluxo é linear: não há ramificações aqui, apenas delegação.
        /// </summary>
        public double Charge(double amount, bool isDefaulter)
        {
            // Passo comum: validação
            if (amount < 0)
                throw new ArgumentException("Valor não pode ser negativo.", nameof(amount));

            // Passo variável: delegado à subclasse
            return Apply(amount, isDefaulter);
        }

        /// <summary>
        /// Gancho abstrato: cada subclasse implementa sua lógica de cálculo.
        /// </summary>
        protected abstract double Apply(double amount, bool isDefaulter);
    }
}
