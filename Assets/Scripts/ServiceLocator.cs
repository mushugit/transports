using LightInject;
using UnityEngine;

public static class ServiceLocator
{
    private static ServiceContainer _container = new ServiceContainer();

    public static void Register<TService, TImplementation>(TService implementation)
    {
        //_container.RegisterInstance<TImplementation>(implementation);
        _container.Register<TService>(factory => implementation, new PerContainerLifetime());
        Debug.Log($"Register {typeof(TImplementation)}");
    }

    public static TService GetInstance<TService>()
    {
        Debug.Log($"Getting instance of {typeof(TService)}");
        return _container.GetInstance<TService>();
    }

    public static void Init()
    {
        Debug.Log($"Register {typeof(Simulation)}");
        _container.Register<Simulation, Simulation>(new PerContainerLifetime());
    }
}

