namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("EnsureNavigationView")]
    public class EnsureNavigationView : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;

        public EnsureNavigationView(CommerceCommander commerceCommander)
        {
            this._commerceCommander = commerceCommander;
        }

        public override Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");

            if (entityView.Name != "ToolsNavigation")
            {
                return Task.FromResult(entityView);
            }

            var cartsView = new EntityView
            {
                Name = context.GetPolicy<CartUiPolicy>().DevOpsCartsViewName,
                DisplayName = "Carts",
                UiHint = "extension",
                Icon = context.GetPolicy<CartUiPolicy>().Icon,
                ItemId = context.GetPolicy<CartUiPolicy>().DevOpsCartsViewName
            };

            entityView.ChildViews.Add(cartsView);

            return Task.FromResult(entityView);
        }
    }
}
