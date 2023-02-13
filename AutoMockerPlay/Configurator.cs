using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq.AutoMock;

namespace AutoMockerPlay
{
    public class Configurator : IServiceProvider, IDisposable
    {
        private readonly AutoMocker _autoMocker = new AutoMocker();
        private readonly IDictionary<Type, Mock> _registeredMocks = new Dictionary<Type, Mock>();
        private readonly IServiceCollection _services;

        private IContainer? _container;
        private IServiceScope? _scope;
        private bool _configurationIsFinished = false;

        public Configurator(IServiceCollection? services = null)
            : this(FillServices(services))
        {
        }

        public Configurator(Action<IServiceCollection> configuration)
        {
            _services = new ServiceCollection();

            configuration?.Invoke(_services);
        }

        private static Action<IServiceCollection> FillServices(IServiceCollection? services)
        {
            return internalServices =>
            {
                if (services != null)
                {
                    foreach (var description in services)
                    {
                        internalServices.Add(description);
                    }
                }
            };
        }

        public void Dispose()
        {
            _scope?.Dispose();

            _container?.Dispose();
        }

        /// <summary>
        /// Creates instance of <typeparamref name="T"/> type using dependency container
        /// to resolve constructor parameters.
        /// </summary>
        /// <typeparam name="T">Type of instence.</typeparam>
        /// <returns>Instance of <typeparamref name="T"/> type.</returns>
        public T CreateInstance<T>()
        {
            PrepareScope();

            return ActivatorUtilities.CreateInstance<T>(_scope!.ServiceProvider);
        }

        /// <summary>
        /// Returns service registered in the container.
        /// </summary>
        /// <param name="serviceType">Service type.</param>
        /// <returns>Instance of a service from the container.</returns>
        public object? GetService(Type serviceType)
        {
            PrepareScope();

            return _scope!.ServiceProvider.GetService(serviceType);
        }

        /// <summary>
        /// Replaces in the dependency container records of <typeparamref name="T"/> type
        /// with a singleton mock and returns the mock.
        /// </summary>
        /// <typeparam name="T">Type of service.</typeparam>
        /// <returns>Mock for the <typeparamref name="T"/> type.</returns>
        /// <exception cref="InvalidOperationException">This method can't be called after
        /// any service is resolved from the container.</exception>
        public Mock<T> GetMock<T>()
            where T : class
        {
            if (_registeredMocks.ContainsKey(typeof(T)))
            {
                return (Mock<T>)_registeredMocks[typeof(T)];
            }

            if (!_configurationIsFinished)
            {
                var mock = new Mock<T>();

                _registeredMocks.Add(typeof(T), mock);

                _services.RemoveAll<T>();
                _services.AddSingleton(mock.Object);

                return mock;
            }
            else
            {
                throw new InvalidOperationException($"You can not create new mock after any service is already resolved (after call of {nameof(CreateInstance)} or {nameof(GetService)})");
            }
        }

        private void PrepareScope()
        {
            if (!_configurationIsFinished)
            {
                _configurationIsFinished = true;

                _container = CreateContainer();

                _scope = _container.BuildServiceProvider().CreateScope();
            }
        }

        private IContainer CreateContainer()
        {
            Rules.DynamicRegistrationProvider dynamicRegistration = (serviceType, serviceKey) =>
            new[]
            {
                new DynamicRegistration(DelegateFactory.Of(_ =>
                {
                    if(_registeredMocks.ContainsKey(serviceType))
                    {
                        return _registeredMocks[serviceType].Object;
                    }

                    var mock = _autoMocker.GetMock(serviceType);

                    _registeredMocks[serviceType] = mock;

                    return mock.Object;
                }))
            };

            var rules = Rules.Default.WithDynamicRegistration(
                dynamicRegistration,
                DynamicRegistrationFlags.Service | DynamicRegistrationFlags.AsFallback);

            var container = new Container(rules);

            container.Populate(_services);
            
            return DryIocAdapter.WithDependencyInjectionAdapter(container);
        }
    }
}
