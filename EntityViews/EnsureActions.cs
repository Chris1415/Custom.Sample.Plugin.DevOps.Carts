namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System.Linq;
    using System.Threading.Tasks;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("EnsureActions")]
    public class EnsureActions : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        public override Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{Name}: The argument cannot be null");
            var cartUiPolicy = context.GetPolicy<CartUiPolicy>();
            var mainActionsPolicy = entityView.GetPolicy<ActionsPolicy>();

            // Overall Main View Actions

            // Active Carts List View Actions
            var lineItemsView = entityView.ChildViews.FirstOrDefault(p => p.Name == cartUiPolicy.LineItemsView);
            if (lineItemsView != null)
            {
                var tableEntityViewActionsPolicy = lineItemsView.GetPolicy<ActionsPolicy>();


                tableEntityViewActionsPolicy.Actions.Add(new EntityActionView
                {
                    Name = cartUiPolicy.AddLineItemActionName,
                    DisplayName = "Add line item",
                    Description = string.Empty,
                    IsEnabled = true,
                    RequiresConfirmation = true,
                    EntityView = cartUiPolicy.AddLineItemActionName,
                    UiHint = string.Empty,
                    Icon = "navigate_plus"
                });

                tableEntityViewActionsPolicy.Actions.Add(new EntityActionView
                {
                    Name = cartUiPolicy.EditLineItemActionName,
                    DisplayName = "Edit Line Item",
                    Description = string.Empty,
                    IsEnabled = true,
                    RequiresConfirmation = false,
                    EntityView = cartUiPolicy.EditLineItemActionName,
                    UiHint = string.Empty,
                    Icon = "edit"
                });

                tableEntityViewActionsPolicy.Actions.Add(new EntityActionView
                {
                    Name = cartUiPolicy.RemoveLineItemActionName,
                    DisplayName = "Remove Line Item",
                    Description = string.Empty,
                    IsEnabled = true,
                    RequiresConfirmation = true,
                    EntityView = cartUiPolicy.RemoveLineItemActionName,
                    UiHint = string.Empty,
                    Icon = "minus"
                });
            }

            // Voucher
            var vouchersListView = entityView.ChildViews.FirstOrDefault(p => p.Name == cartUiPolicy.VouchersListView);
            if (vouchersListView != null)
            {
                var tableEntityViewActionsPolicy = vouchersListView.GetPolicy<ActionsPolicy>();

                tableEntityViewActionsPolicy.Actions.Add(new EntityActionView
                {
                    Name = cartUiPolicy.AddVoucherActionName,
                    DisplayName = "Add Voucher",
                    Description = string.Empty,
                    IsEnabled = true,
                    RequiresConfirmation = true,
                    EntityView = cartUiPolicy.AddVoucherActionName,
                    UiHint = string.Empty,
                    Icon = "navigate_plus"
                });

                tableEntityViewActionsPolicy.Actions.Add(new EntityActionView
                {
                    Name = cartUiPolicy.RemoveVoucherActionName,
                    DisplayName = "Remove Voucher",
                    Description = string.Empty,
                    IsEnabled = true,
                    RequiresConfirmation = true,
                    EntityView = cartUiPolicy.RemoveVoucherActionName,
                    UiHint = string.Empty,
                    Icon = "minus"
                });
            }

            // Gift Card
            var giftCardPaymentView = entityView.ChildViews.FirstOrDefault(p => p.Name == cartUiPolicy.GiftCardPaymentsView);
            if (giftCardPaymentView != null)
            {
                var tableEntityViewActionsPolicy = giftCardPaymentView.GetPolicy<ActionsPolicy>();

                tableEntityViewActionsPolicy.Actions.Add(new EntityActionView
                {
                    Name = cartUiPolicy.AddGiftCardPaymentActionName,
                    DisplayName = "Add GiftCard Payment",
                    Description = string.Empty,
                    IsEnabled = true,
                    RequiresConfirmation = true,
                    EntityView = cartUiPolicy.AddGiftCardPaymentActionName,
                    UiHint = string.Empty,
                    Icon = "navigate_plus"
                });

                tableEntityViewActionsPolicy.Actions.Add(new EntityActionView
                {
                    Name = cartUiPolicy.RemoveGiftCardPaymentActionName,
                    DisplayName = "Remove GiftCard Payment",
                    Description = string.Empty,
                    IsEnabled = true,
                    RequiresConfirmation = true,
                    EntityView = cartUiPolicy.RemoveGiftCardPaymentActionName,
                    UiHint = string.Empty,
                    Icon = "minus"
                });
            }

            // Physical Fulfillment 
            var physicalFulfillmentView = entityView.ChildViews.FirstOrDefault(p => p.Name == cartUiPolicy.PhysicalFulfillmentListView);
            if (physicalFulfillmentView != null)
            {
                var tableEntityViewActionsPolicy = physicalFulfillmentView.GetPolicy<ActionsPolicy>();

                tableEntityViewActionsPolicy.Actions.Add(new EntityActionView
                {
                    Name = cartUiPolicy.AddPhysicalFulfillmentActionName,
                    DisplayName = "Add physical Fulfillment",
                    Description = string.Empty,
                    IsEnabled = true,
                    RequiresConfirmation = true,
                    EntityView = cartUiPolicy.AddPhysicalFulfillmentActionName,
                    UiHint = string.Empty,
                    Icon = "navigate_plus"
                });
            }

            return Task.FromResult(entityView);
        }
    }
}
