using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace MovieProject.Services
{
    public class TestRunnerHostedService : IHostedService
    {
        private readonly ILogger<TestRunnerHostedService> _logger;
        private readonly IWebHostEnvironment _env;

        public TestRunnerHostedService(
            ILogger<TestRunnerHostedService> logger,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_env.IsDevelopment())
            {
                _logger.LogInformation("Test runner servis başlatılıyor…");

                // 3 sn bekleyip testleri çalıştır
                _ = Task.Delay(3000, cancellationToken)
                         .ContinueWith(async _ => await RunTestsAsync(), cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Test runner servis durduruluyor.");
            return Task.CompletedTask;
        }

        private async Task RunTestsAsync()
        {
            try
            {
                _logger.LogInformation("Unit test’ler çalıştırılıyor…");

                var psi = new ProcessStartInfo("dotnet")
                {
                    Arguments = "test MovieProject.Tests/MovieProject.Tests.csproj --logger console --verbosity normal --no-build --no-restore --filter Category=Unit",
                    WorkingDirectory = GetSolutionDirectory(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var proc = Process.Start(psi)!;
                var outp = await proc.StandardOutput.ReadToEndAsync();
                var err = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();

                if (proc.ExitCode == 0)
                    _logger.LogInformation("Tüm unit test’ler başarılı:\n{Out}", outp);
                else
                {
                    _logger.LogError("Bazı test’ler başarısız oldu:\n{Err}", err);
                    if (!string.IsNullOrWhiteSpace(outp))
                        _logger.LogInformation("Test çıktısı:\n{Out}", outp);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test çalıştırırken hata oluştu");
            }
        }

        private string GetSolutionDirectory()
        {
            var dir = Directory.GetCurrentDirectory();
            if (Directory.GetFiles(dir, "*.sln").Any()) return dir;
            var parent = Directory.GetParent(dir);
            return parent is not null && Directory.GetFiles(parent.FullName, "*.sln").Any()
                ? parent.FullName
                : dir;
        }
    }
}
