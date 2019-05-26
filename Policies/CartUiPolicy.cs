namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using Sitecore.Commerce.Core;
    
    public class CartUiPolicy : Policy
    {
        public string AddLineItemActionName { get; set; }
        public string EditLineItemActionName { get; set; }
        public string RemoveLineItemActionName { get; set; }
        public string AddVoucherActionName { get; set; }
        public string RemoveVoucherActionName { get; set; }
        public string AddGiftCardPaymentActionName { get; set; }
        public string RemoveGiftCardPaymentActionName { get; set; }
        public string AddPhysicalFulfillmentActionName { get; set; }
        public string PhysicalFulfillmentListView { get; set; }
        public string GiftCardPaymentsView { get; set; }
        public string VouchersListView { get; set; }
        public string LineItemsView { get; set; }
        public string DevOpsCartsViewName { get; set; }
        public string Icon { get; set; }

        public CartUiPolicy()
        {
            AddLineItemActionName = "CustomDevOps-AddLineItem";
            EditLineItemActionName = "CustomDevOps-EditLineItem";
            RemoveLineItemActionName = "CustomDevOps-RemoveLineItem";
            AddVoucherActionName = "CustomDevOps - AddVoucher";
            RemoveVoucherActionName = "CustomDevOps-RemoveVoucher";
            AddGiftCardPaymentActionName = "CustomDevOps-AddGiftCardPayment";
            RemoveGiftCardPaymentActionName = "CustomDevOps-RemoveGiftCardPayment";
            AddPhysicalFulfillmentActionName = "CustomDevOps-AddPhysicalFullfillment";
            PhysicalFulfillmentListView = "Physical Fulfillment";
            GiftCardPaymentsView = "Gift Card Payment";
            VouchersListView = "Vouchers";
            LineItemsView = "Line Items";
            DevOpsCartsViewName = "DevOps-Carts";
            Icon = "shopping_cart_full";
        }
    }
}
