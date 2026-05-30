using System;
using System.Collections.Generic;

namespace Core
{
    public interface IReadOnlyReactiveValue<out T>
    {
        T Value { get; }

        /// <param name="invokeImmediately">Вызвать callback сразу с текущим значением.</param>
        IDisposable Subscribe(Action<T> callback, bool invokeImmediately = true);
    }

    /// <summary>
    /// Реактивное значение. Уведомляет всех подписчиков при изменении Value.
    /// Не уведомляет при записи того же значения (DistinctUntilChanged).
    ///
    /// Subscribe возвращает IDisposable — единственный способ отписаться.
    /// Нет голых event Action: нельзя «потерять» подписку.
    /// </summary>
    public sealed class ReactiveValue<T> : IReadOnlyReactiveValue<T>
    {
        private T _value;
        private readonly List<Action<T>> _callbacks = new();

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;
                _value = value;
                Notify();
            }
        }

        public ReactiveValue(T initial = default) => _value = initial;

        public IDisposable Subscribe(Action<T> callback, bool invokeImmediately = true)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            _callbacks.Add(callback);

            if (invokeImmediately)
                callback(_value);

            return new DisposableCallback(() => _callbacks.Remove(callback));
        }

        // Снимок списка перед итерацией — отписка внутри callback не ломает цикл
        private void Notify()
        {
            var snapshot = _callbacks.ToArray();
            foreach (var cb in snapshot)
                cb(_value);
        }
    }

    /// <summary>Одноразовый IDisposable, вызывающий делегат при Dispose().</summary>
    internal sealed class DisposableCallback : IDisposable
    {
        private Action _action;

        public DisposableCallback(Action action) => _action = action;

        public void Dispose()
        {
            _action?.Invoke();
            _action = null; // идемпотентность: повторный Dispose — no-op
        }
    }
}
