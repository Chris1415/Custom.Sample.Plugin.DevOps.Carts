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

    [PipelineDisplayName("DoActionRemoveLineItem")]
    public class DoActionRemoveLineItem : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;
        private readonly RemoveCartLineCommand _removeCartLineCommand;

        public DoActionRemoveLineItem(
            CommerceCommander commerceCommander,
            GetCartCommand getCartCommand,
            RemoveCartLineCommand RemoveCartLineCommand)
        {
            this._getCartCommand = getCartCommand;
            this._commerceCommander = commerceCommander;
            this._removeCartLineCommand = RemoveCartLineCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            if (entityView == null
                || !entityView.Action.Equals(context.GetPolicy<CartUiPolicy>().RemoveLineItemActionName, StringComparison.OrdinalIgnoreCase))
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

                var line = cart.Lines.FirstOrDefault(element => element.ItemId.Equals(entityView.ItemId));

                cart = await _removeCartLineCommand.Process(context.CommerceContext, entityView.EntityId, line);
            }

            catch (Exception ex)
            {
                context.Logger.LogError($"{this.Name}.PathNotFound: Message={ex.Message}");
            }

            return entityView;
        }
    }
}
