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

    [PipelineDisplayName("DoActionAddGiftCardPayment")]
    public class DoActionAddGiftCardPayment : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;
        private readonly AddPaymentsCommand _addPaymentsCommand;

        public DoActionAddGiftCardPayment(
            CommerceCommander commerceCommander,
            GetCartCommand getCartCommand,
            AddPaymentsCommand addPaymentsCommand)
        {
            this._getCartCommand = getCartCommand;
            this._commerceCommander = commerceCommander;
            this._addPaymentsCommand = addPaymentsCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            if (entityView == null
                || !entityView.Action.Equals(context.GetPolicy<CartUiPolicy>().AddGiftCardPaymentActionName, StringComparison.OrdinalIgnoreCase))
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
                var amountProperty = entityView.Properties[2];
                decimal amount = !string.IsNullOrEmpty(amountProperty.Value) ? decimal.Parse(amountProperty.Value) : cart.Totals.GrandTotal.Amount;

                var paymentComponent = new GiftCardPaymentComponent()
                {
                    PaymentMethod = new EntityReference()
                    {
                        Name = "GiftCard",
                        EntityTarget = "B5E5464E-C851-4C3C-8086-A4A874DD2DB0"
                    },
                    GiftCardCode = giftCardCodeProperty.Value,
                    Amount = new Money(amount)
                };

                await _addPaymentsCommand.Process(context.CommerceContext, entityView.EntityId, new List<PaymentComponent>()
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
