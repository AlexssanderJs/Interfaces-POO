namespace Fase4_ComInterfaces
{
    /// <summary>
    /// Dublê de teste (Fake/Stub) para a interface IChargingStrategy.
    /// Usado em testes para evitar I/O, dependências externas e tornar testes determinísticos.
    /// Implementa o mesmo contrato, mas com comportamento controlado para teste.
    /// </summary>
    public sealed class FakeChargingStrategy : IChargingStrategy
    {
        private readonly double _fixedResult;

        /// <summary>
        /// Cria um fake que sempre retorna um valor fixo.
        /// Útil para testar a lógica do cliente sem depender da lógica real da estratégia.
        /// </summary>
        public FakeChargingStrategy(double fixedResult)
        {
            _fixedResult = fixedResult;
        }

        public double Calculate(double amount, bool isDefaulter)
        {
            // Retorna valor fixo configurado - comportamento previsível para teste
            return _fixedResult;
        }
    }

    /// <summary>
    /// Outro dublê de teste - Spy que registra as chamadas.
    /// Permite verificar se o cliente chamou a estratégia corretamente.
    /// </summary>
    public sealed class SpyChargingStrategy : IChargingStrategy
    {
        public double LastAmount { get; private set; }
        public bool LastIsDefaulter { get; private set; }
        public int CallCount { get; private set; }

        public double Calculate(double amount, bool isDefaulter)
        {
            // Registra a chamada
            LastAmount = amount;
            LastIsDefaulter = isDefaulter;
            CallCount++;

            // Retorna valor simples para não interferir no teste
            return amount;
        }
    }
}
