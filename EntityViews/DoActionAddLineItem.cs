namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("DoActionAddLineItem")]
    public class DoActionAddLineItem : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;
        private readonly AddCartLineCommand _addCartLineCommand;

        public DoActionAddLineItem(
            CommerceCommander commerceCommander,
            GetCartCommand getCartCommand,
            AddCartLineCommand addCartLineCommand)
        {
            this._getCartCommand = getCartCommand;
            this._commerceCommander = commerceCommander;
            this._addCartLineCommand = addCartLineCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            if (entityView == null
                || !entityView.Action.Equals(context.GetPolicy<CartUiPolicy>().AddLineItemActionName, StringComparison.OrdinalIgnoreCase))
            {
                return entityView;
            }

            try
            {
                var cart = await _getCartCommand.Process(context.CommerceContext, entityView.EntityId, false);
                if (cart == null || cart.DateCreated == cart.DateUpdated)
                {
                    return entityView;
                }

                var catalog = entityView.Properties[1];
                var productId = entityView.Properties[2];
                var variantId = entityView.Properties[3];
                var quantity = entityView.Properties[4];

                cart = await _addCartLineCommand.Process(context.CommerceContext, entityView.EntityId, new CartLineComponent()
                {
                    ItemId = string.Join("|", catalog.Value, productId.Value, variantId.Value),
                    Quantity = int.Parse(quantity.Value)
                });
            }

            catch (Exception ex)
            {
                context.Logger.LogError($"{this.Name}.PathNotFound: Message={ex.Message}");
            }

            return entityView;
        }
    }
}
