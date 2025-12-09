using System;

namespace Fase4_ComInterfaces
{
    /// <summary>
    /// Ponto de entrada da aplicação.
    /// Demonstra o uso do padrão com interface + factory + injeção de dependência.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== Fase 4: Interface Plugável e Testável ===\n");

            // Demonstra políticas disponíveis
            Console.WriteLine("Políticas disponíveis:");
            foreach (var policy in ChargingStrategyFactory.GetAvailablePolicies())
            {
                Console.WriteLine($"  - {policy}");
            }
            Console.WriteLine();

            // Casos de teste
            var testCases = new[]
            {
                new { Amount = 50.0, IsDefaulter = false, Mode = "standard" },
                new { Amount = 150.0, IsDefaulter = true, Mode = "penalty" },
                new { Amount = 300.0, IsDefaulter = false, Mode = "progressive" },
                new { Amount = 600.0, IsDefaulter = true, Mode = "progressive" },
                new { Amount = 200.0, IsDefaulter = false, Mode = "premium" }
            };

            foreach (var testCase in testCases)
            {
                ProcessAndDisplay(testCase.Amount, testCase.IsDefaulter, testCase.Mode);
                Console.WriteLine(new string('-', 50));
            }

            // Executa testes com dublês
            BillingServiceTests.RunAllTests();
        }

        /// <summary>
        /// Processa uma cobrança usando o padrão completo:
        /// 1. Factory resolve a estratégia (composição centralizada)
        /// 2. Service recebe a estratégia via DI (cliente depende do contrato)
        /// 3. Service processa sem conhecer a implementação concreta
        /// </summary>
        private static void ProcessAndDisplay(double amount, bool isDefaulter, string mode)
        {
            // 1. Composição: Factory resolve a política → implementação
            IChargingStrategy strategy = ChargingStrategyFactory.Resolve(mode);

            // 2. Injeção de dependência: Cliente recebe o contrato
            var billingService = new BillingService(strategy);

            // 3. Uso: Cliente processa sem conhecer a concreta
            Console.WriteLine($"Política: {mode}");
            Console.WriteLine(billingService.GenerateReport(amount, isDefaulter));
        }
    }
}
