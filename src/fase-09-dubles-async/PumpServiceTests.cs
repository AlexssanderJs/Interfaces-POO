using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fase9_DublesAsync
{
    /// <summary>
    /// Testes unitários/integração com dublês para PumpService.
    /// </summary>
    public static class PumpServiceTests
    {
        public static async Task RunAllAsync()
        {
            Console.WriteLine("=== Executando Testes Fase 9 (Dublês Async) ===\n");

            await Test_Success_Simple();
            await Test_Retry_WithBackoff();
            await Test_Cancel_Midway();
            await Test_Empty_Stream_ShouldReturnZero();
            await Test_Reader_Error_MidStream();

            Console.WriteLine("\n✅ Todos os testes de dublês assíncronos passaram!\n");
        }

        private static async Task Test_Success_Simple()
        {
            var clock = new FakeClock();
            var delay = new FakeDelay(clock);
            var backoff = new ExponentialBackoffPolicy(TimeSpan.FromMilliseconds(10));
            var reader = new FakeReader<int>(new[] { 1, 2, 3 });
            var writer = new FakeWriter<int>();

            var svc = new PumpService<int>(reader, writer, clock, delay, backoff);
            var total = await svc.RunAsync();

            Assert(total == 3, "Deveria processar 3 itens");
            Assert(writer.Written.Count == 3, "Writer deveria receber 3 itens");
            Console.WriteLine("✓ Test_Success_Simple");
        }

        private static async Task Test_Retry_WithBackoff()
        {
            var clock = new FakeClock(start: DateTimeOffset.UnixEpoch, tick: TimeSpan.FromMilliseconds(1));
            var delay = new FakeDelay(clock);
            var backoff = new LinearBackoffPolicy(TimeSpan.FromMilliseconds(5));
            var reader = new FakeReader<string>(new[] { "a", "b", "c" });
            var writer = new FakeWriter<string>(failCount: 2); // falha 2 vezes e depois funciona

            var svc = new PumpService<string>(reader, writer, clock, delay, backoff, maxRetries: 3);
            var total = await svc.RunAsync();

            Assert(total == 3, "Deveria processar 3 itens mesmo com retentativa");
            Assert(writer.Written.Count == 3, "Writer deveria ter 3 itens escritos");
            Assert(writer.Attempts >= 5, "Deveria ter havido tentativas extras");
            Console.WriteLine("✓ Test_Retry_WithBackoff");
        }

        private static async Task Test_Cancel_Midway()
        {
            var clock = new FakeClock();
            var delay = new FakeDelay(clock);
            var backoff = new ExponentialBackoffPolicy(TimeSpan.FromMilliseconds(1));
            var cts = new CancellationTokenSource();

            var reader = new FakeReader<int>(new[] { 10, 20, 30 });
            var writer = new FakeWriter<int>(onWrite: () =>
            {
                // Cancela após a primeira escrita bem-sucedida
                if (!cts.IsCancellationRequested)
                    cts.Cancel();
            }, invokeAfterSuccess: true);

            var svc = new PumpService<int>(reader, writer, clock, delay, backoff);
            try
            {
                await svc.RunAsync(cts.Token);
                throw new Exception("Cancelamento esperado não ocorreu");
            }
            catch (OperationCanceledException)
            {
                Assert(writer.Written.Count == 1, "Cancelamento deveria parar após 1 item");
                Console.WriteLine("✓ Test_Cancel_Midway");
            }
        }

        private static async Task Test_Empty_Stream_ShouldReturnZero()
        {
            var clock = new FakeClock();
            var delay = new FakeDelay(clock);
            var backoff = new LinearBackoffPolicy(TimeSpan.FromMilliseconds(1));
            var reader = new FakeReader<int>(Array.Empty<int>());
            var writer = new FakeWriter<int>();

            var svc = new PumpService<int>(reader, writer, clock, delay, backoff);
            var total = await svc.RunAsync();

            Assert(total == 0, "Stream vazio deve retornar 0");
            Assert(writer.Written.Count == 0, "Writer não deve receber itens");
            Console.WriteLine("✓ Test_Empty_Stream_ShouldReturnZero");
        }

        private static async Task Test_Reader_Error_MidStream()
        {
            var clock = new FakeClock();
            var delay = new FakeDelay(clock);
            var backoff = new LinearBackoffPolicy(TimeSpan.FromMilliseconds(1));
            var reader = new FakeReader<int>(new[] { 1, 2, 3 }, failAtIndex: 1); // falha no 2º item
            var writer = new FakeWriter<int>();

            var svc = new PumpService<int>(reader, writer, clock, delay, backoff);
            try
            {
                await svc.RunAsync();
                throw new Exception("Exceção esperada do reader não ocorreu");
            }
            catch (InvalidOperationException)
            {
                Assert(writer.Written.Count == 1, "Deveria ter escrito apenas o primeiro item antes da falha");
                Console.WriteLine("✓ Test_Reader_Error_MidStream");
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new Exception($"❌ Teste falhou: {message}");
        }
    }
}
