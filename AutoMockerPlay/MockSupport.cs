using Moq.AutoMock.Resolvers;

namespace AutoMockerPlay
{
    internal class ServicesMockResolver : IMockResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ServicesMockResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool IsDisabled { get; set; }

        public void Resolve(MockResolutionContext context)
        {
            if (IsDisabled) return;

            var service = _serviceProvider.GetService(context.RequestType);

            if (service != null)
            {
                context.Value = service;
            }
        }
    }
}
