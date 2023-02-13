using Microsoft.Extensions.DependencyInjection;

namespace AutoMockerPlay
{
    [TestClass]
    public class SystemTests
    {
        [TestMethod]
        public void NoRegisteredServices_MocksAreUsed()
        {
            using var configurator = new Configurator();

            var dependencyMock = configurator.GetMock<IDependency>();
            dependencyMock.Setup(d => d.GetName()).Returns("Ivan");

            var featureToggleMock = configurator.GetMock<IFeatureToggle>();
            featureToggleMock.Setup(f => f.SupportGreeting()).Returns(true);

            var _sut = configurator.CreateInstance<System>();

            var greeting = _sut.GetGreeting();

            greeting.Should().Be("Hello, Ivan");

            featureToggleMock.Verify(ft => ft.SupportGreeting(), Times.Once);
        }

        [TestMethod]
        public void PartiallyRegisteredServices_MocksAreUsedForTheRest()
        {
            using var configurator = new Configurator(services => {
                services.AddScoped<IFeatureToggle, FeatureToggle>();
            });

            var dependencyMock = configurator.GetMock<IDependency>();
            dependencyMock.Setup(d => d.GetName()).Returns("Ivan");

            var _sut = configurator.CreateInstance<System>();

            var greeting = _sut.GetGreeting();

            greeting.Should().Be("Hello, Ivan");

            dependencyMock.Verify(d => d.GetName(), Times.Once);
        }

        [TestMethod]
        public void PartiallyRegisteredServices_CanMockRegisteredService()
        {
            using var configurator = new Configurator(services => {
                services.AddScoped<IFeatureToggle, FeatureToggle>();
            });

            var dependencyMock = configurator.GetMock<IDependency>();
            dependencyMock.Setup(d => d.GetName()).Returns("Ivan");

            var featureToggleMock = configurator.GetMock<IFeatureToggle>();
            featureToggleMock.Setup(f => f.SupportGreeting()).Returns(false);

            var _sut = configurator.CreateInstance<System>();

            var greeting = _sut.GetGreeting();

            greeting.Should().BeEmpty();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PartiallyRegisteredServices_CantMockUsedService()
        {
            using var configurator = new Configurator(services => {
                services.AddScoped<IFeatureToggle, FeatureToggle>();
            });

            _ = configurator.CreateInstance<System>();

            _ = configurator.GetMock<IFeatureToggle>();
        }

        [TestMethod]
        public void PartiallyRegisteredServices_CanGetAutomaticallyMockedService()
        {
            using var configurator = new Configurator(services => {
                services.AddScoped<IFeatureToggle, FeatureToggle>();
            });

            _ = configurator.CreateInstance<System>();

            var urlProvider = configurator.GetMock<IUrlProvider>();

            urlProvider.Should().NotBeNull();
        }

    }
}