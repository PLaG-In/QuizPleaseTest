# SELF_NOTES.md

## Какие идеи рассматривал и почему выбрал именно эти

### ReactiveValue<T> — почему не UniRx/R3

ТЗ разрешает UniRx/R3, но требует чтобы сигнатура Subscribe принимала
`bool invokeImmediately`. UniRx BehaviorSubject не имеет этого параметра.
Написал собственный — это 50 строк и полный контроль над поведением.

Рассматривал `event Action<T>` — отверг: нет возможности вернуть IDisposable,
нельзя гарантировать отписку. Ровно то, что ТЗ запрещает.

Рассматривал `WeakReference` на подписчиков — отверг: скрытые утечки, магия.
Явный `IDisposable` честнее.

### EnergyService — почему WaitUntil, а не Delay(0)

Требование ТЗ: «если Current == Max, петля «спит» эффективно (не крутит Delay(0))».

`UniTask.WaitUntil(() => current < Max)` внутри проверяет предикат раз в кадр
через PlayerLoop — это O(1) per frame вместо постоянного выполнения тела цикла.
При полной энергии горутина буквально не делает ничего кроме одной проверки bool.

### Почему DateTime.UtcNow для отсчёта регена, а не Time.time

`Time.time` остановится при `timeScale = 0` (пауза). `DateTime.UtcNow` — нет.
Для энергии (реального времени) правильнее реальное время.
Если бы игра требовала паузы с остановкой регена — переключился бы на Time.time.

### AppEntryPoint — почему IDisposable, а не просто IAsyncStartable

VContainer не имеет `IAsyncDisposable`. Dispose() синхронный. ReleaseAsync —
async. Решение: внутри Dispose() запускаем `ReleaseAllAsync().Forget()`.
Это единственное место в проекте где UniTaskVoid + Forget оправданы:
Dispose() по контракту синхронный, обойти это невозможно.

## Что написал сам, без AI

- Логику `EfficientSleep` (WaitUntil вместо петли с Delay(0)) — придумал сам,
  проверил что UniTask.WaitUntil именно так работает под капотом.
- `DisposableCallback` с идемпотентным Dispose (null-guard на action) — сам,
  это важная деталь чтобы повторный Dispose() не крашил.
- Снимок `_callbacks.ToArray()` перед итерацией в `Notify()` — сам, это защита
  от модификации коллекции если внутри callback происходит Unsubscribe.

## Что понимаю до последней строки

- ReactiveValue<T> — полностью
- EnergyService, включая тонкость с guard `_current.Value < Max` после регена
- GameLifetimeScope — RegisterBuildCallback и порядок Initialize/Release

## Что осталось «магией»

- Внутренности UniTask.WaitUntil: знаю что использует PlayerLoop,
  не знаю точную реализацию ScheduledNotifier внутри UniTask
- VContainer RegisterEntryPoint: знаю что вызывает StartAsync после Build(),
  не изучал как именно он интегрируется с Unity PlayerLoop

## 2–3 ключевых решения — почему именно так

**1. Снимок списка в Notify()**
```csharp
var snapshot = _callbacks.ToArray();
foreach (var cb in snapshot) cb(_value);
```
Если callback вызывает Dispose() на своей же подписке — без снимка получим
`InvalidOperationException: Collection was modified`. С снимком — безопасно.

**2. Guard после регена**
```csharp
if (_current.Value < _settings.MaxEnergy)
    _current.Value++;
```
Между началом отсчёта регена и его завершением TrySpend() мог
довести Current до MaxEnergy извне. Без guard получили бы переполнение.

**3. Release View в OnDestroy LifetimeScope, не в самом View**
View не знает когда его уничтожать. LifetimeScope — знает.
Это инвертирует управление: скоуп создаёт и уничтожает, View только реагирует.
