using Core;
using Core.UI;

namespace Energy.UI
{
    /// <summary>
    /// ViewModel панели энергии.
    /// Чистый C#-класс — никакого MonoBehaviour, никакого Unity API.
    /// Инжектируется через VContainer, передаётся во View через Setup().
    ///
    /// View читает из VM только то, что нужно для отображения.
    /// Логика (TrySpend, форматирование текста) — здесь.
    /// </summary>
    public sealed class EnergyBarUIViewModel : IUIViewModel
    {
        private readonly IEnergyService  _energyService;
        private readonly EnergySettings  _settings;

        public IReadOnlyReactiveValue<int>   Current       => _energyService.Current;
        public IReadOnlyReactiveValue<float> SecondsToNext => _energyService.SecondsToNext;

        public int MaxEnergy   => _settings.MaxEnergy;
        public int SpendAmount => _settings.SpendAmount;

        /// <summary>Текст для лейбла кнопки, вычисляется один раз при создании VM.</summary>
        public string SpendButtonLabel => $"Потратить {_settings.SpendAmount}";

        public EnergyBarUIViewModel(IEnergyService energyService, EnergySettings settings)
        {
            _energyService = energyService;
            _settings      = settings;
        }

        /// <summary>Делегирует трату энергии в сервис.</summary>
        public bool TrySpend() => _energyService.TrySpend(_settings.SpendAmount);

        /// <summary>Форматированный текст счётчика, обновляемый во View через Subscribe.</summary>
        public string FormatCount(int current) => $"{current} / {MaxEnergy}";
    }
}
