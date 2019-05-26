namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("DevOpsCartsDashboard")]
    public class DevOpsCartsDashboard : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;

        public DevOpsCartsDashboard(CommerceCommander commerceCommander)
        {
            this._commerceCommander = commerceCommander;
        }
        
        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");

            if (entityView.Name != context.GetPolicy<CartUiPolicy>().DevOpsCartsViewName)
            {
                return entityView;
            }

            entityView.UiHint = "Flat";
            entityView.Icon = context.GetPolicy<CartUiPolicy>().Icon;
            entityView.DisplayName = "Custom Dev Ops";

            try
            {
                await this._commerceCommander.Command<ChildViewActiveCarts>().Process(context.CommerceContext, entityView);
            }
            catch (Exception ex)
            {
                context.Logger.LogError(ex, "DevOpsCarts.DashBoard.Exception");
            }

            return entityView;
        }
    }
}
