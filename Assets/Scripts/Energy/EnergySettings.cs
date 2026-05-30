using UnityEngine;

namespace Energy
{
    [CreateAssetMenu(
        fileName = "EnergySettings",
        menuName  = "Settings/EnergySettings")]
    public class EnergySettings : ScriptableObject
    {
        [field: SerializeField, Min(1),
                Tooltip("Максимальный запас энергии.")]
        public int MaxEnergy { get; private set; } = 100;

        [field: SerializeField, Min(0.1f),
                Tooltip("Секунд на восстановление одной единицы энергии.")]
        public float RegenSeconds { get; private set; } = 5f;

        [field: SerializeField, Min(1),
                Tooltip("Сколько энергии тратит кнопка «Потратить».")]
        public int SpendAmount { get; private set; } = 10;
    }
}
