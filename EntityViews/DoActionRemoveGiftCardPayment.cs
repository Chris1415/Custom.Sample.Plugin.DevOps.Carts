namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.GiftCards;
    using Sitecore.Commerce.Plugin.Payments;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("DoActionRemoveGiftCardPayment")]
    public class DoActionRemoveGiftCardPayment : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;
        private readonly RemovePaymentsCommand _removePaymentsCommand;

        public DoActionRemoveGiftCardPayment(
            CommerceCommander commerceCommander,
            GetCartCommand getCartCommand,
            RemovePaymentsCommand removePaymentsCommand)
        {
            this._getCartCommand = getCartCommand;
            this._commerceCommander = commerceCommander;
            this._removePaymentsCommand = removePaymentsCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            if (entityView == null
                || !entityView.Action.Equals(context.GetPolicy<CartUiPolicy>().RemoveGiftCardPaymentActionName, StringComparison.OrdinalIgnoreCase))
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

                var giftCardCodeProperty = entityView.Properties[1];
                var giftCardCodeFormValue = giftCardCodeProperty.Value;
                var paymentComponent = cart.GetComponent<GiftCardPaymentComponent>();
                if (!paymentComponent.GiftCardCode.Equals(giftCardCodeFormValue))
                {
                    return entityView;
                }

                await _removePaymentsCommand.Process(context.CommerceContext, entityView.EntityId, new List<PaymentComponent>()
                {
                    paymentComponent
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
