using System.Threading;
using Cysharp.Threading.Tasks;

namespace Core
{
    /// <summary>
    /// Контракт сервиса с асинхронным жизненным циклом.
    ///
    /// InitializeAsync — стартует фоновые петли, загружает данные.
    /// ReleaseAsync    — отменяет CTS, ждёт завершения фоновых задач.
    ///
    /// CancellationToken пробрасывается насквозь: внутренние await
    /// реально прерываются при отмене — никаких «подвисших» задач.
    /// </summary>
    public interface IService
    {
        UniTask InitializeAsync(CancellationToken ct);
        UniTask ReleaseAsync(CancellationToken ct);
    }
}
