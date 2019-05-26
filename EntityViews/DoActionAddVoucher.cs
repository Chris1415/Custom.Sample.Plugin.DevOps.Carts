namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Coupons;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("DoActionAddVoucher")]
    public class DoActionAddVoucher : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;
        private readonly AddCouponCommand _addCouponCommand;

        public DoActionAddVoucher(
            CommerceCommander commerceCommander,
            GetCartCommand getCartCommand,
            AddCouponCommand addCouponCommand)
        {
            this._getCartCommand = getCartCommand;
            this._commerceCommander = commerceCommander;
            this._addCouponCommand = addCouponCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            if (entityView == null
                || !entityView.Action.Equals(context.GetPolicy<CartUiPolicy>().AddVoucherActionName, StringComparison.OrdinalIgnoreCase))
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

                var voucherIdProperty = entityView.Properties[1];

                await _addCouponCommand.Process(context.CommerceContext, entityView.EntityId, voucherIdProperty.Value);
            }

            catch (Exception ex)
            {
                context.Logger.LogError($"{this.Name}.PathNotFound: Message={ex.Message}");
            }

            return entityView;
        }
    }
}
