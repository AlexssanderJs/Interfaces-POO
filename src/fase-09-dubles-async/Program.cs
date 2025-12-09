using System;
using System.Threading.Tasks;

namespace Fase9_DublesAsync
{
    public class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("=== Fase 9: Dublês avançados e testes assíncronos ===\n");

            await Demo();
            await PumpServiceTests.RunAllAsync();
        }

        private static async Task Demo()
        {
            Console.WriteLine("--- Demo: PumpService com retentativa e backoff controlado ---");

            var clock = new FakeClock();
            var delay = new FakeDelay(clock);
            var backoff = new ExponentialBackoffPolicy(TimeSpan.FromMilliseconds(20));
            var reader = new FakeReader<string>(new[] { "item-1", "item-2", "item-3" });
            var writer = new FakeWriter<string>(failCount: 1); // falha a primeira escrita, depois sucesso

            var svc = new PumpService<string>(reader, writer, clock, delay, backoff, maxRetries: 3);
            var total = await svc.RunAsync();

            Console.WriteLine($"✓ Processados: {total} itens");
            Console.WriteLine($"✓ Tentativas de escrita: {writer.Attempts}");
            Console.WriteLine($"✓ Clock final: {clock.Now}");
        }
    }
}
