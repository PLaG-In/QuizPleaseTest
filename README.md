# Energy & Regen — Unity Middle Test (Variant B)

## Как запустить

1. Открыть в Unity 6+
2. Package Manager → установить:
   - **VContainer** (via OpenUPM: `jp.hadashikick.vcontainer`)
   - **UniTask** (via OpenUPM: `com.cysharp.unitask`)
3. `Assets → Create → Settings → EnergySettings` → создать ассет, настроить параметры
4. На сцене создать `LifetimeScope`-объект, добавить компонент `GameLifetimeScope`
5. В инспекторе `GameLifetimeScope` назначить ассет `EnergySettings` и `EnergyBarUIView`
6. Настроить UI-иерархию (см. ниже), запустить Play

## UI-иерархия на сцене

```
Canvas
└── EnergyPanel
    ├── EnergyCountLabel   (TextMeshProUGUI)  ← "current / max"
    ├── RegenBar           (Image, FillMethod = Horizontal, fillAmount = 0..1)
    └── SpendButton        (Button)
        └── SpendButtonLabel (TextMeshProUGUI)
```

Компонент `EnergyBarUIView` вешается на `EnergyPanel`.
В инспекторе `EnergyBarUIView` назначить все дочерние ссылки.

## Что бы я доделал за ещё 2 часа

- **Персистентность**: сохранять `Current` и timestamp последнего выхода в `PlayerPrefs`,
  при старте досчитывать накопленную регенерацию за оффлайн-время
- **Нотификации**: показывать тост «Энергия восстановлена!» при полном восстановлении
- **Тесты**: unit-тесты для `ReactiveValue<T>` (Subscribe/Dispose/DistinctUntilChanged)
  и для `EnergyService` (TrySpend границы, overflow guard при Max)
- **Анимации**: DOTween-пульс на лейбле счётчика при изменении значения
- **Variant A Boot Flow**: надстроить стейт-машину поверх этой же архитектуры
