using System;
using UnityEngine;

namespace Core.UI
{
    /// <summary>Маркер для всех ViewModel.</summary>
    public interface IUIViewModel { }

    /// <summary>
    /// Базовый View — MonoBehaviour с явным lifecycle.
    /// Initialize() вешает биндинги и слушателей.
    /// Release()     диспозит все подписки; после Release() объект можно уничтожить
    ///               без NRE или висящих callbacks.
    /// </summary>
    public abstract class UIView : MonoBehaviour
    {
        public abstract void Initialize();
        public abstract void Release();
    }

    /// <summary>
    /// View с типизированной ViewModel.
    /// Логика — в VM (чистый C#-класс).
    /// View держит только Unity-ссылки и биндинги.
    /// </summary>
    public abstract class UIView<TVm> : UIView where TVm : class, IUIViewModel
    {
        private TVm _viewModel;

        protected TVm ViewModel =>
            _viewModel ?? throw new InvalidOperationException(
                $"{GetType().Name}: ViewModel не задана. Вызови Setup() до Initialize().");

        /// <summary>Вызывается из LifetimeScope до Initialize().</summary>
        public void Setup(TVm viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        }
    }
}
