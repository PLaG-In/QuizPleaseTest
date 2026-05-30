using System;
using System.Threading;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Energy
{
    /// <summary>
    /// Сервис энергии с фоновой UniTask-петлёй регенерации.
    ///
    /// ── Lifecycle ─────────────────────────────────────────────────────────────
    /// InitializeAsync: устанавливает начальное значение, стартует петлю.
    /// ReleaseAsync:    отменяет внутренний CTS → петля завершается через
    ///                  OperationCanceledException.
    ///
    /// ── Эффективный sleep при полной энергии ─────────────────────────────────
    /// Когда Current == Max, петля уходит в WaitUntil — не крутит Delay(0)
    /// в пустом while. Единственный триггер для пробуждения — TrySpend().
    ///
    /// ── Thread safety ────────────────────────────────────────────────────────
    /// Вся логика на главном потоке Unity (UniTask.Delay использует PlayerLoop).
    /// TrySpend() тоже вызывается с главного потока → гонок нет.
    /// </summary>
    public sealed class EnergyService : IEnergyService
    {
        // Интервал опроса прогресс-бара (мс). Достаточно для плавной анимации.
        private const int ProgressPollIntervalMs = 50;

        private readonly EnergySettings _settings;

        private readonly ReactiveValue<int>   _current     = new(0);
        private readonly ReactiveValue<float> _secondsToNext = new(0f);

        private CancellationTokenSource _regenCts;

        public IReadOnlyReactiveValue<int>   Current      => _current;
        public IReadOnlyReactiveValue<float> SecondsToNext => _secondsToNext;

        public EnergyService(EnergySettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        // ── IService ──────────────────────────────────────────────────────────

        public UniTask InitializeAsync(CancellationToken ct)
        {
            _current.Value       = _settings.MaxEnergy;
            _secondsToNext.Value = 0f;

            _regenCts = new CancellationTokenSource();
            // UniTaskVoid: fire-and-forget, ошибки пойманы внутри
            RunRegenLoopAsync(_regenCts.Token).Forget();

            return UniTask.CompletedTask;
        }

        public UniTask ReleaseAsync(CancellationToken ct)
        {
            _regenCts?.Cancel();
            _regenCts?.Dispose();
            _regenCts = null;
            return UniTask.CompletedTask;
        }

        // ── IEnergyService ────────────────────────────────────────────────────

        public bool TrySpend(int amount)
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Должно быть > 0.");

            if (_current.Value < amount) return false;

            _current.Value -= amount;
            return true;
        }

        // ── Regen loop ────────────────────────────────────────────────────────

        private async UniTaskVoid RunRegenLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    // ── Эффективный сон пока энергия полная ───────────────────
                    // WaitUntil не крутит цикл — подписывается на PlayerLoop
                    if (_current.Value >= _settings.MaxEnergy)
                    {
                        _secondsToNext.Value = 0f;

                        await UniTask.WaitUntil(
                            predicate: () => _current.Value < _settings.MaxEnergy,
                            cancellationToken: ct);
                    }

                    // ── Регенерация одной единицы ─────────────────────────────
                    var startedAt = DateTime.UtcNow;
                    var duration  = TimeSpan.FromSeconds(_settings.RegenSeconds);

                    while (DateTime.UtcNow - startedAt < duration)
                    {
                        ct.ThrowIfCancellationRequested();

                        float elapsed = (float)(DateTime.UtcNow - startedAt).TotalSeconds;
                        _secondsToNext.Value = Mathf.Clamp01(elapsed / _settings.RegenSeconds);

                        await UniTask.Delay(
                            TimeSpan.FromMilliseconds(ProgressPollIntervalMs),
                            cancellationToken: ct);
                    }

                    // Добавляем единицу (guard: не превышаем Max, если TrySpend
                    // вернул энергию к Max во время регена — маловероятно, но safe)
                    if (_current.Value < _settings.MaxEnergy)
                    {
                        _current.Value++;
                        _secondsToNext.Value = 0f;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Нормальное завершение через ReleaseAsync — ничего не делаем
            }
        }
    }
}
