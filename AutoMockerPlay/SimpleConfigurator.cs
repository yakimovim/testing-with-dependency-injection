using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AutoMockerPlay
{
    public class SimpleConfigurator : IServiceProvider, IDisposable
    {
        private readonly IDictionary<Type, Mock> _registeredMocks = new Dictionary<Type, Mock>();
        private readonly IServiceCollection _services;

        private IServiceProvider _serviceProvider;
        private IServiceScope? _scope;
        private bool _configurationIsFinished = false;

        public SimpleConfigurator(IServiceCollection services)
        {
            _services = services;
        }


        public void Dispose()
        {
            _scope?.Dispose();
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

                _serviceProvider = _services.BuildServiceProvider();

                _scope = _serviceProvider.CreateScope();
            }
        }
    }
}
