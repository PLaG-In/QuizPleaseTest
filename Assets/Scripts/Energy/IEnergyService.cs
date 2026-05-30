using Core;

namespace Energy
{
    public interface IEnergyService : IService
    {
        /// <summary>Текущий запас энергии.</summary>
        IReadOnlyReactiveValue<int> Current { get; }

        /// <summary>
        /// Прогресс регенерации следующей единицы, 0..1.
        /// 0 = только что начался отсчёт (или энергия полная).
        /// 1 = единица вот-вот добавится.
        /// </summary>
        IReadOnlyReactiveValue<float> SecondsToNext { get; }

        /// <summary>
        /// Потратить <paramref name="amount"/> единиц энергии.
        /// Возвращает false, если энергии недостаточно — значение не меняется.
        /// </summary>
        bool TrySpend(int amount);
    }
}
