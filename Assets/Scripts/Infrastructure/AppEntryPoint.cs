using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Energy;
using VContainer.Unity;

namespace Infrastructure
{
    /// <summary>
    /// Точка входа приложения. VContainer вызывает StartAsync после сборки контейнера.
    ///
    /// Отвечает за:
    ///   • вызов InitializeAsync у всех сервисов в нужном порядке
    ///   • вызов ReleaseAsync при уничтожении scope (через IDisposable)
    ///
    /// async void запрещён — StartAsync возвращает UniTask.
    /// </summary>
    public sealed class AppEntryPoint : IAsyncStartable, IDisposable
    {
        private readonly IEnergyService _energyService;

        // CTS для ReleaseAsync — не пробрасываем внешний ct из Dispose(),
        // потому что Dispose() синхронный, а Release — async.
        // VContainer не ждёт async dispose, поэтому Release запускаем как Forget.
        private CancellationTokenSource _releaseCts;

        public AppEntryPoint(IEnergyService energyService)
        {
            _energyService = energyService;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _releaseCts = new CancellationTokenSource();
            await _energyService.InitializeAsync(cancellation);
        }

        public void Dispose()
        {
            if (_releaseCts == null) return;

            // ReleaseAsync — fire-and-forget через UniTaskVoid, т.к. Dispose() синхронный
            ReleaseAllAsync(_releaseCts.Token).Forget();
        }

        private async UniTaskVoid ReleaseAllAsync(CancellationToken ct)
        {
            await _energyService.ReleaseAsync(ct);
            _releaseCts?.Dispose();
            _releaseCts = null;
        }
    }
}
