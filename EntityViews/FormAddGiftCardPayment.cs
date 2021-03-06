﻿namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("FormAddGiftCardPayment")]
    public class FormAddGiftCardPayment : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;

        public FormAddGiftCardPayment(CommerceCommander commerceCommander, GetCartCommand getCartCommand)
        {
            this._commerceCommander = commerceCommander;
            this._getCartCommand = getCartCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");

            if (entityView.Name != context.GetPolicy<CartUiPolicy>().AddGiftCardPaymentActionName)
            {
                return entityView;
            }

            var cart = await _getCartCommand.Process(context.CommerceContext, entityView.EntityId, false);

            try
            {
                entityView.Properties.Add(
                    new ViewProperty
                    {
                        Name = "GiftCard Id",
                        IsHidden = false,
                        IsRequired = true,
                        RawValue = string.Empty
                    });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "Amount",
                       IsHidden = false,
                       IsRequired = false,
                       RawValue = string.Empty
                   });
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Content.SynchronizeContentPath.PathNotFound: Message={ex.Message}");
            }

            return entityView;
        }
    }
}
