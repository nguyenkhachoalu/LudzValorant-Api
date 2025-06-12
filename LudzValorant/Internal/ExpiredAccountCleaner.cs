using LudzValorant.DataContexts;
using LudzValorant.Services.InterfaceServices;

namespace LudzValorant.Internal
{
    public class ExpiredAccountCleaner : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        public ExpiredAccountCleaner(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(DoCleanup, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private void DoCleanup(object state)
        {
            using var scope = _scopeFactory.CreateScope();
            var accountService = scope.ServiceProvider.GetRequiredService<IAccountService>();
            accountService.CleanupExpiredAccounts(); // gọi service để xử lý
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public void Dispose() => _timer?.Dispose();
    }

}
