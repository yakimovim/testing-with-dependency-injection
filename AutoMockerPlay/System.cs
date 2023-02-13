namespace AutoMockerPlay
{
    public interface IDependency
    {
        string GetName();
    }

    public interface IUrlProvider
    { }

    public interface IFeatureToggle
    {
        bool SupportGreeting();
    }

    internal sealed class FeatureToggle : IFeatureToggle
    {
        private readonly IUrlProvider _urlProvider;

        public FeatureToggle(IUrlProvider urlProvider)
        {
            _urlProvider = urlProvider;
        }

        public bool SupportGreeting()
        {
            return true;
        }
    }

    internal sealed class System
    {
        private readonly IDependency _dependency;
        private readonly IFeatureToggle _featureToggle;

        public System(IDependency dependency, IFeatureToggle featureToggle)
        {
            _dependency = dependency;
            _featureToggle = featureToggle;
        }

        public string GetGreeting()
        {
            if(_featureToggle.SupportGreeting())
            {
                return $"Hello, {_dependency.GetName()}";
            }

            return string.Empty;
        }
    }
}
