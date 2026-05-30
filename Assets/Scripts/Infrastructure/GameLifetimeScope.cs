using Energy;
using Energy.UI;
using Infrastructure;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Корневой VContainer-скоуп сцены.
///
/// Все сервисы регистрируются через интерфейсы:
///   builder.Register{Impl}(Lifetime.Singleton).As{IInterface}()
///
/// Никаких FindObjectOfType, Singleton.Instance, static состояния.
/// </summary>
public sealed class GameLifetimeScope : LifetimeScope
{
    [Header("Settings (ScriptableObject ассеты)")]
    [SerializeField] private EnergySettings _energySettings;

    [Header("Views (MonoBehaviour на сцене)")]
    [SerializeField] private EnergyBarUIView _energyBarView;

    protected override void Configure(IContainerBuilder builder)
    {
        // ── Settings ─────────────────────────────────────────────────────────
        // RegisterInstance: уже созданный объект, VContainer его не создаёт
        builder.RegisterInstance(_energySettings);

        // ── Services ──────────────────────────────────────────────────────────
        builder.Register<EnergyService>(Lifetime.Singleton)
               .As<IEnergyService>();

        // ── ViewModels ────────────────────────────────────────────────────────
        builder.Register<EnergyBarUIViewModel>(Lifetime.Singleton);

        // ── Entry point ───────────────────────────────────────────────────────
        // VContainer вызовет StartAsync после сборки контейнера
        builder.RegisterEntryPoint<AppEntryPoint>(Lifetime.Singleton);

        // ── Views ─────────────────────────────────────────────────────────────
        // После сборки контейнера: резолвим VM, передаём во View, инициализируем
        builder.RegisterBuildCallback(container =>
        {
            var vm = container.Resolve<EnergyBarUIViewModel>();
            _energyBarView.Setup(vm);
            _energyBarView.Initialize();
        });
    }

    protected override void OnDestroy()
    {
        // Release View до уничтожения scope — чистим подписки
        _energyBarView.Release();
        base.OnDestroy();
    }
}
