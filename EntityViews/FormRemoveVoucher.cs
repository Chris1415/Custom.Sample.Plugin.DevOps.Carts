namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Coupons;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("FormRemoveVoucher")]
    public class FormRemoveVoucher : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;

        public FormRemoveVoucher(CommerceCommander commerceCommander, GetCartCommand getCartCommand)
        {
            this._commerceCommander = commerceCommander;
            this._getCartCommand = getCartCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");

            if (entityView.Name != context.GetPolicy<CartUiPolicy>().RemoveVoucherActionName)
            {
                return entityView;
            }

            var entityViewArgument = this._commerceCommander.Command<ViewCommander>().CurrentEntityViewArgument(context.CommerceContext);
            var cart = await _getCartCommand.Process(context.CommerceContext, entityView.EntityId, false);

            var couponComponent = cart.GetComponent<CartCouponsComponent>();
            if (couponComponent == null)
            {
                return entityView;
            }

            var foundcoupon = couponComponent.List.FirstOrDefault(element => element.Promotion.EntityTarget.Equals(entityView.ItemId));
                   if(foundcoupon == null)
            {
                return entityView;
            }

            try
            {
                entityView.Properties.Add(
                    new ViewProperty
                    {
                        Name = "Voucher ID",
                        IsHidden = true,
                        IsRequired = true,
                        RawValue = foundcoupon.CouponId
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
