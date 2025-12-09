using System;

namespace Fase4_ComInterfaces
{
    /// <summary>
    /// Testes unitários do BillingService usando dublês (fakes/stubs).
    /// Demonstra como testar o cliente SEM:
    /// - I/O real
    /// - Dependências externas
    /// - Lógica complexa das estratégias concretas
    /// 
    /// Benefícios: testes rápidos, determinísticos e focados no cliente.
    /// </summary>
    public static class BillingServiceTests
    {
        public static void RunAllTests()
        {
            Console.WriteLine("\n=== Executando Testes com Dublês ===\n");

            Test_ProcessBilling_WithFake_ReturnsExpectedValue();
            Test_ProcessBilling_WithNegativeAmount_ThrowsException();
            Test_ProcessBilling_WithSpy_CallsStrategyOnce();
            Test_GenerateReport_FormatsCorrectly();

            Console.WriteLine("\n✅ Todos os testes passaram!\n");
        }

        /// <summary>
        /// Teste 1: Verifica que o service usa o resultado da estratégia.
        /// Usa FAKE para controlar o retorno sem lógica complexa.
        /// </summary>
        private static void Test_ProcessBilling_WithFake_ReturnsExpectedValue()
        {
            // Arrange: Fake sempre retorna 123.45
            var fakeStrategy = new FakeChargingStrategy(123.45);
            var service = new BillingService(fakeStrategy);

            // Act: Processa qualquer valor
            var result = service.ProcessBilling(100, false);

            // Assert: Deve retornar o valor fixo do fake
            if (Math.Abs(result - 123.45) > 0.001)
            {
                throw new Exception($"Teste falhou: esperado 123.45, obtido {result}");
            }

            Console.WriteLine("✓ Test_ProcessBilling_WithFake_ReturnsExpectedValue");
        }

        /// <summary>
        /// Teste 2: Verifica validação de valor negativo.
        /// Não depende da estratégia - testa lógica do próprio service.
        /// </summary>
        private static void Test_ProcessBilling_WithNegativeAmount_ThrowsException()
        {
            // Arrange: Fake simples
            var fakeStrategy = new FakeChargingStrategy(0);
            var service = new BillingService(fakeStrategy);

            // Act & Assert: Deve lançar exceção
            try
            {
                service.ProcessBilling(-10, false);
                throw new Exception("Teste falhou: deveria ter lançado ArgumentException");
            }
            catch (ArgumentException)
            {
                // Esperado
            }

            Console.WriteLine("✓ Test_ProcessBilling_WithNegativeAmount_ThrowsException");
        }

        /// <summary>
        /// Teste 3: Verifica que o service chama a estratégia corretamente.
        /// Usa SPY para verificar parâmetros e contagem de chamadas.
        /// </summary>
        private static void Test_ProcessBilling_WithSpy_CallsStrategyOnce()
        {
            // Arrange: Spy registra as chamadas
            var spyStrategy = new SpyChargingStrategy();
            var service = new BillingService(spyStrategy);

            // Act: Processa uma cobrança
            service.ProcessBilling(250.0, true);

            // Assert: Verifica que chamou com os parâmetros corretos
            if (spyStrategy.CallCount != 1)
            {
                throw new Exception($"Teste falhou: esperado 1 chamada, obtido {spyStrategy.CallCount}");
            }
            if (Math.Abs(spyStrategy.LastAmount - 250.0) > 0.001)
            {
                throw new Exception($"Teste falhou: esperado amount=250, obtido {spyStrategy.LastAmount}");
            }
            if (!spyStrategy.LastIsDefaulter)
            {
                throw new Exception("Teste falhou: esperado isDefaulter=true");
            }

            Console.WriteLine("✓ Test_ProcessBilling_WithSpy_CallsStrategyOnce");
        }

        /// <summary>
        /// Teste 4: Verifica formatação do relatório.
        /// Usa FAKE para controlar o valor final sem lógica de cálculo.
        /// </summary>
        private static void Test_GenerateReport_FormatsCorrectly()
        {
            // Arrange: Fake retorna valor conhecido
            var fakeStrategy = new FakeChargingStrategy(110.0);
            var service = new BillingService(fakeStrategy);

            // Act: Gera relatório
            var report = service.GenerateReport(100.0, false);

            // Assert: Verifica que contém informações esperadas
            if (!report.Contains("R$ 100,00") || !report.Contains("R$ 110,00"))
            {
                throw new Exception($"Teste falhou: formatação incorreta\n{report}");
            }

            Console.WriteLine("✓ Test_GenerateReport_FormatsCorrectly");
        }
    }
}
