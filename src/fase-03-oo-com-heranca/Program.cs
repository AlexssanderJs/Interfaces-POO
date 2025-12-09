using System;

namespace Fase3_OO_ComHeranca
{
    /// <summary>
    /// Cliente que conhece as estratégias concretas e escolhe qual usar.
    /// Demonstra o padrão Strategy com herança polimórfica.
    /// 
    /// Observação: O switch abaixo permanece apenas para COMPOSIÇÃO da estratégia inicial.
    /// Não há mais ramificações DENTRO do fluxo de cálculo (removidas da classe base).
    /// Na Fase 4, essa composição será extraída para um ponto único (Factory ou Container).
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("=== Fase 3: OO com Herança e Polimorfismo ===\n");

            // Dados de teste
            var testCases = new[]
            {
                new { Amount = 50.0, IsDefaulter = false, Mode = "standard" },
                new { Amount = 150.0, IsDefaulter = true, Mode = "penalty" },
                new { Amount = 300.0, IsDefaulter = false, Mode = "progressive" },
                new { Amount = 600.0, IsDefaulter = true, Mode = "progressive" }
            };

            foreach (var testCase in testCases)
            {
                var result = Render(testCase.Amount, testCase.IsDefaulter, testCase.Mode);
                Console.WriteLine($"Valor: R$ {testCase.Amount:F2}");
                Console.WriteLine($"Inadimplente: {testCase.IsDefaulter}");
                Console.WriteLine($"Estratégia: {testCase.Mode}");
                Console.WriteLine($"Resultado: R$ {result:F2}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Método cliente que compõe a estratégia concreta.
        /// Este é o ponto onde o switch permanece (apenas para instanciação).
        /// O fluxo de cálculo é totalmente linear (via polimorfismo).
        /// </summary>
        public static double Render(double amount, bool isDefaulter, string mode)
        {
            // Switch para COMPOSIÇÃO (instanciação da estratégia concreta)
            // Este switch foi removido do fluxo de cálculo e ficou apenas aqui.
            ChargingStrategyBase strategy = mode switch
            {
                "standard" => new StandardChargingStrategy(),
                "penalty" => new DefaulterPenaltyChargingStrategy(),
                "progressive" => new ProgressiveChargingStrategy(),
                _ => new StandardChargingStrategy() // padrão
            };

            // Fluxo linear: nenhuma ramificação aqui.
            // Toda a lógica variável é delegada à subclasse via polimorfismo.
            return strategy.Charge(amount, isDefaulter);
        }
    }
}
