using System;
using System.Collections.Generic;

namespace Fase4_ComInterfaces
{
    /// <summary>
    /// Ponto único de composição (Factory/Catálogo).
    /// Responsável por converter uma política (mode) em uma implementação concreta.
    /// Centraliza a decisão de instanciação - cliente não conhece as classes concretas.
    /// </summary>
    public static class ChargingStrategyFactory
    {
        // Catálogo interno: mapeia políticas para funções de criação
        private static readonly Dictionary<string, Func<IChargingStrategy>> _catalog = new()
        {
            ["standard"] = () => new StandardChargingStrategy(),
            ["penalty"] = () => new DefaulterPenaltyChargingStrategy(),
            ["progressive"] = () => new ProgressiveChargingStrategy(),
            ["premium"] = () => new PremiumChargingStrategy()
        };

        /// <summary>
        /// Resolve a estratégia baseada na política fornecida.
        /// </summary>
        /// <param name="mode">Identificador da política (ex: "standard", "penalty")</param>
        /// <returns>Instância da estratégia correspondente</returns>
        /// <exception cref="ArgumentException">Quando a política não é reconhecida</exception>
        public static IChargingStrategy Resolve(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                return new StandardChargingStrategy(); // fallback padrão
            }

            var normalizedMode = mode.ToLowerInvariant();

            if (_catalog.TryGetValue(normalizedMode, out var factory))
            {
                return factory();
            }

            throw new ArgumentException($"Política de cobrança '{mode}' não reconhecida.", nameof(mode));
        }

        /// <summary>
        /// Obtém todas as políticas disponíveis no catálogo.
        /// </summary>
        public static IEnumerable<string> GetAvailablePolicies()
        {
            return _catalog.Keys;
        }
    }
}
