# AI_LOG.md

Задание выполнялось с использованием AI (Claude).

## Что делал AI

- Сгенерировал структуру файлов и boilerplate (интерфейсы, классы)
- Написал реализацию EnergyService, ReactiveValue, UIView
- Написал документацию (README, SELF_NOTES, этот файл)

## Где AI ошибся / что было скорректировано

- Первый вариант AppEntryPoint использовал `async void Dispose()` — это нарушение
  требования ТЗ. Исправлено: Dispose() синхронный, ReleaseAllAsync запускается через Forget()
- Первый вариант ReactiveValue не делал снимок _callbacks перед итерацией —
  потенциальный краш при unsubscribe внутри callback. Исправлено вручную.
- В EnergyService не было guard `_current.Value < Max` после завершения регенерации —
  возможное переполнение если TrySpend вызывался во время регена. Добавлено.

## Промпты (краткий лог)

1. "Реализуй Вариант B из ТЗ: EnergyService + ReactiveValue + VContainer"
2. "Добавь guard после регена и снимок коллекции в Notify"
3. "Исправь AppEntryPoint: Dispose() должен быть синхронным"
4. "Напиши SELF_NOTES объясняя ключевые решения"
