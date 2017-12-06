[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Reporting_application.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Reporting_application.App_Start.NinjectWebCommon), "Stop")]

namespace Reporting_application.App_Start
{
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Models;
    using Ninject;
    using Ninject.Web.Common;
    using ReportingModels;
    using Repository;
    using Services.Performance;
    using Services.SuppliersAnalysis;
    using System;
    using System.Web;

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {

            kernel.Bind<IThirdpartyDBrepository>().To<ThirdpartyDBrepository>();
            kernel.Bind<ICompanyDBRepository>().To<CompanyDBRepository>();
            kernel.Bind<IPerformance>().To<Performance>();
            kernel.Bind<IThirdpartyDBContext>().To<ThirdpartyDBContext>();
            kernel.Bind<IBookingsStagesAnalysis>().To<BookingsStagesAnalysis>();
            kernel.Bind<ISuppliersAnalysis>().To<SuppliersAnalysis>();


        }
    }
}
