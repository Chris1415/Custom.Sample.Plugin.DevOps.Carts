namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("DoActionEditLineItem")]
    public class DoActionEditLineItem : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;
        private readonly UpdateCartLineCommand _updateCartLineCommand;
        private readonly RemoveCartLineCommand _removeCartLineCommand;

        public DoActionEditLineItem(
            CommerceCommander commerceCommander,
            GetCartCommand getCartCommand,
            UpdateCartLineCommand updateCartLineCommand,
            RemoveCartLineCommand removeCartLineCommand)
        {
            this._getCartCommand = getCartCommand;
            this._commerceCommander = commerceCommander;
            this._updateCartLineCommand = updateCartLineCommand;
            this._removeCartLineCommand = removeCartLineCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            if (entityView == null
                || !entityView.Action.Equals(context.GetPolicy<CartUiPolicy>().EditLineItemActionName, StringComparison.OrdinalIgnoreCase))
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

                var quantityProperty = entityView.Properties[1];
                int quantity = int.Parse(quantityProperty.Value);
                var line = cart.Lines.FirstOrDefault(element => element.ItemId.Equals(entityView.ItemId));

                if (quantity == 0)
                {
                    await _removeCartLineCommand.Process(context.CommerceContext, entityView.EntityId, line);
                }
                else if (quantity > 0)
                {
                    line.Quantity = quantity;
                    await _updateCartLineCommand.Process(context.CommerceContext, entityView.EntityId, line);
                }
            }

            catch (Exception ex)
            {
                context.Logger.LogError($"{this.Name}.PathNotFound: Message={ex.Message}");
            }

            return entityView;
        }
    }
}
