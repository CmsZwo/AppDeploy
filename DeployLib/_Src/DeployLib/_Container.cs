using Unity;
using Unity.RegistrationByConvention;

namespace DeployLib
{
	public interface IContainer
	{
		T Get<T>();
		void Inject<T>(T instance);
	}

	public class Container : IContainer
	{
		public static Container Shared { get; }
			= new Container();

		private readonly IUnityContainer _IUnityContainer;

		public Container()
		{
			_IUnityContainer = new UnityContainer();

			_IUnityContainer
				.RegisterTypes(
					AllClasses.FromLoadedAssemblies(),
					WithMappings.FromMatchingInterface,
					WithName.Default,
					WithLifetime.ContainerControlled
				);
		}

		public T Get<T>()
			=> _IUnityContainer.Resolve<T>();

		public void Inject<T>(T instance)
			=> _IUnityContainer.BuildUp(instance);
	}
}
