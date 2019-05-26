namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.BusinessUsers;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config
                .ConfigurePipeline<IGetEntityViewPipeline>(d =>
                {
                    d.Add<DevOpsCartsDashboard>().Before<IFormatEntityViewPipeline>();
                    d.Add<EntityViewCart>().Before<IFormatEntityViewPipeline>();
                    d.Add<FormAddLineItem>().Before<IFormatEntityViewPipeline>();
                    d.Add<FormRemoveLineItem>().Before<IFormatEntityViewPipeline>();
                    d.Add<FormEditLineItem>().Before<IFormatEntityViewPipeline>();
                    d.Add<FormRemoveVoucher>().Before<IFormatEntityViewPipeline>();
                    d.Add<FormAddVoucher>().Before<IFormatEntityViewPipeline>();
                    d.Add<FormAddGiftCardPayment>().Before<IFormatEntityViewPipeline>();
                    d.Add<FormRemoveGiftCardPayment>().Before<IFormatEntityViewPipeline>();
                    d.Add<FormAddPhysicalFulfillment>().Before<IFormatEntityViewPipeline>();
                })
               .ConfigurePipeline<IBizFxNavigationPipeline>(d =>
               {
                   d.Add<EnsureNavigationView>();
               })
               .ConfigurePipeline<IFormatEntityViewPipeline>(d =>
               {
                   d.Add<EnsureActions>().After<PopulateEntityViewActionsBlock>();
               })
               .ConfigurePipeline<IDoActionPipeline>(
               c =>
               {
                   c.Add<DoActionRemoveLineItem>().After<ValidateEntityVersionBlock>();
                   c.Add<DoActionAddLineItem>().After<ValidateEntityVersionBlock>();
                   c.Add<DoActionEditLineItem>().After<ValidateEntityVersionBlock>();
                   c.Add<DoActionRemoveVoucher>().After<ValidateEntityVersionBlock>();
                   c.Add<DoActionAddVoucher>().After<ValidateEntityVersionBlock>();
                   c.Add<DoActionAddGiftCardPayment>().After<ValidateEntityVersionBlock>();
                   c.Add<DoActionRemoveGiftCardPayment>().After<ValidateEntityVersionBlock>();
                   c.Add<DoActionAddPhysicalFulfillment>().After<ValidateEntityVersionBlock>();
               })
               .ConfigurePipeline<IConfigureServiceApiPipeline>(configure => configure.Add<ConfigureServiceApiBlock>()));

            services.RegisterAllCommands(assembly);
        }
    }
}