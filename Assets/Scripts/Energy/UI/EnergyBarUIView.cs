using System;
using System.Collections.Generic;
using Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Energy.UI
{
    /// <summary>
    /// View панели энергии.
    ///
    /// ── Что здесь ──────────────────────────────────────────────────────────────
    /// · Unity-ссылки (SerializeField)
    /// · Initialize(): Subscribe на VM + AddListener на кнопку
    /// · Release():    Dispose всех подписок + RemoveListener
    ///
    /// ── Чего здесь НЕТ ────────────────────────────────────────────────────────
    /// · Никакого Update() для опроса состояния
    /// · Никакой логики (только биндинги к VM)
    /// · Никаких прямых обращений к IEnergyService
    /// </summary>
    public sealed class EnergyBarUIView : UIView<EnergyBarUIViewModel>
    {
        [Header("Счётчик энергии")]
        [SerializeField] private TextMeshProUGUI energyCountLabel;

        [Header("Прогресс регенерации")]
        [SerializeField] private Image regenFillImage;   // Image.fillAmount = SecondsToNext

        [Header("Кнопка траты")]
        [SerializeField] private Button          spendButton;
        [SerializeField] private TextMeshProUGUI spendButtonLabel;

        // Все подписки в одном месте — Release() чистит одним проходом
        private readonly List<IDisposable> _subscriptions = new();

        // ── UIView lifecycle ──────────────────────────────────────────────────

        public override void Initialize()
        {
            // Текст кнопки — статичный, устанавливаем один раз
            if (spendButtonLabel != null)
                spendButtonLabel.text = ViewModel.SpendButtonLabel;

            // Current → счётчик + interactable кнопки
            _subscriptions.Add(
                ViewModel.Current.Subscribe(current =>
                {
                    if (energyCountLabel != null)
                        energyCountLabel.text = ViewModel.FormatCount(current);

                    // Кнопка недоступна если не хватает энергии
                    if (spendButton != null)
                        spendButton.interactable = current >= ViewModel.SpendAmount;
                }));

            // SecondsToNext → fillAmount прогресс-бара
            _subscriptions.Add(
                ViewModel.SecondsToNext.Subscribe(progress =>
                {
                    if (regenFillImage != null)
                        regenFillImage.fillAmount = progress;
                }));

            spendButton.onClick.AddListener(OnSpendClicked);
        }

        public override void Release()
        {
            // Диспозим все подписки — никаких NRE при повторном входе
            foreach (var sub in _subscriptions)
                sub.Dispose();
            _subscriptions.Clear();

            spendButton.onClick.RemoveListener(OnSpendClicked);
        }

        // ── Event handlers ────────────────────────────────────────────────────

        private void OnSpendClicked()
        {
            // Результат TrySpend VM уже учтёт в reactive-потоке
            ViewModel.TrySpend();
            Debug.Log("Spend");
        }
    }
}
