namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Availability;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Coupons;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Sitecore.Commerce.Plugin.GiftCards;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("EntityViewCart")]
    public class EntityViewCart : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly GetCartCommand _getCartCommand;
        private readonly ViewCommander _viewCommander;

        public EntityViewCart(GetCartCommand getCartCommand, ViewCommander viewCommander)
        {
            this._getCartCommand = getCartCommand;
            this._viewCommander = viewCommander;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null"); ;

            if (string.IsNullOrEmpty(entityView.EntityId))
            {
                return entityView;
            }

            var cart = await _getCartCommand.Process(context.CommerceContext, entityView.EntityId, false);
            if (cart == null || cart.DateCreated == cart.DateUpdated)
            {
                return entityView;
            }

            if (!string.IsNullOrEmpty(entityView.Action))
            {
                return entityView;
            }

            var entityViewArgument = _viewCommander.CurrentEntityViewArgument(context.CommerceContext);

            var pluginPolicy = context.GetPolicy<PluginPolicy>();

            entityView.UiHint = "Flat";
            entityView.Icon = pluginPolicy.Icon;

            var name = entityView.EntityId;

            try
            {
                entityViewArgument.Entity = cart;

                this.AddTotalsView(entityView, context.CommerceContext, cart);
                this.AddAdjustmentsView(entityView, context.CommerceContext, cart);
                this.AddLineItemView(entityView, context.CommerceContext, cart);
                this.AddVoucherView(entityView, context.CommerceContext, cart);
                this.AddFulfillmentView(entityView, context.CommerceContext, cart);
                this.AddPaymentView(entityView, context.CommerceContext, cart);
                this.AddContactView(entityView, context.CommerceContext, cart);
            }
            catch (Exception ex)
            {
                context.Logger.LogError($"Content.SynchronizeContentPath.PathNotFound: Message={ex.Message}");
            }

            return entityView;
        }

        private void AddTotalsView(EntityView entityView, CommerceContext commerceContext, Cart cart)
        {
            if (cart == null)
            {
                return;
            }

            var entityViewCart = new EntityView
            {
                EntityId = string.Empty,
                ItemId = cart.Id,
                DisplayName = "Cart Totals",
                Name = "Cart - " + cart.Name
            };

            entityViewCart.Properties
                .Add(new ViewProperty { Name = "Subtotal", RawValue = cart.Totals.SubTotal });
            entityViewCart.Properties
                .Add(new ViewProperty { Name = "Adjustments", RawValue = cart.Totals.AdjustmentsTotal });
            entityViewCart.Properties
                .Add(new ViewProperty { Name = "GrandTotal", RawValue = cart.Totals.GrandTotal });
            entityViewCart.Properties
                .Add(new ViewProperty { Name = "PaymentsTotal", RawValue = cart.Totals.PaymentsTotal });
            entityView.ChildViews.Add(entityViewCart);
        }

        private void AddAdjustmentsView(EntityView entityView, CommerceContext commerceContext, Cart cart)
        {
            if (cart == null)
            {
                return;
            }

            var adjustments = new EntityView
            {
                EntityId = string.Empty,
                ItemId = string.Empty,
                DisplayName = "Adjustments",
                Name = "adjustments",
                UiHint = "Table"
            };
            entityView.ChildViews.Add(adjustments);

            foreach (var adjustment in cart.Adjustments)
            {
                var adjustmentView = new EntityView
                {
                    EntityId = entityView.EntityId,
                    ItemId = adjustment.Name,
                    Name = adjustment.Name,
                    DisplayName = adjustment.DisplayName,
                    UiHint = "Flat"
                };
                adjustments.ChildViews.Add(adjustmentView);

                adjustmentView.Properties.Add(new ViewProperty { Name = "Display Name", RawValue = adjustment.DisplayName });
                adjustmentView.Properties.Add(new ViewProperty { Name = "Name", RawValue = adjustment.Name });
                adjustmentView.Properties.Add(new ViewProperty { Name = "Is Taxable", RawValue = adjustment.IsTaxable });
                adjustmentView.Properties.Add(new ViewProperty { Name = "Include in GrandTotal", RawValue = adjustment.IncludeInGrandTotal });
                adjustmentView.Properties.Add(new ViewProperty { Name = "Awarding Block", RawValue = adjustment.AwardingBlock });
                adjustmentView.Properties.Add(new ViewProperty { Name = "Adjustment Type", RawValue = adjustment.AdjustmentType });
                adjustmentView.Properties.Add(new ViewProperty { Name = "Amount", RawValue = adjustment.Adjustment.Amount });
                adjustmentView.Properties.Add(new ViewProperty { Name = "Currency", RawValue = adjustment.Adjustment.CurrencyCode });
            }
        }

        private void AddLineItemView(EntityView entityView, CommerceContext commerceContext, Cart cart)
        {
            if (cart == null)
            {
                return;
            }

            var lineItems = new EntityView
            {
                EntityId = entityView.EntityId,
                ItemId = string.Empty,
                DisplayName = "Line Items",
                Name = "Line Items",
                UiHint = "Table"
            };
            entityView.ChildViews.Add(lineItems);

            var cartProductComponents = new EntityView
            {
                EntityId = string.Empty,
                ItemId = string.Empty,
                DisplayName = "CartProductComponents",
                Name = "CartProductComponent",
                UiHint = "Table"
            };
            entityView.ChildViews.Add(cartProductComponents);

            foreach (var line in cart.Lines)
            {
                var lineView = new EntityView
                {
                    EntityId = cart.Id,
                    ItemId = line.ItemId,
                    Name = line.Name,
                    DisplayName = line.Name,
                    UiHint = "Flat"
                };
                lineItems.ChildViews.Add(lineView);

                lineView.Properties.Add(new ViewProperty { Name = "Name", RawValue = line.Name });
                lineView.Properties.Add(new ViewProperty { Name = "ItemId", RawValue = line.ItemId });
                lineView.Properties.Add(new ViewProperty { Name = "Quantity", RawValue = line.Quantity });
                lineView.Properties.Add(new ViewProperty { Name = "Unit List Price", RawValue = line.UnitListPrice });
                lineView.Properties.Add(new ViewProperty { Name = "SubTotal", RawValue = line.Totals.SubTotal });
                lineView.Properties.Add(new ViewProperty { Name = "AdjustmentsTotal", RawValue = line.Totals.AdjustmentsTotal });
                lineView.Properties.Add(new ViewProperty { Name = "GrandTotal", RawValue = line.Totals.GrandTotal });

                CartProductComponent cartProductComponent = (CartProductComponent)line.CartLineComponents.FirstOrDefault(element => element is CartProductComponent);
                ItemAvailabilityComponent itemAvailabilityComponent = (ItemAvailabilityComponent)line.CartLineComponents.FirstOrDefault(element => element is ItemAvailabilityComponent);

                var cartProductEntityView = new EntityView
                {
                    EntityId = CommerceEntity.IdPrefix<SellableItem>() + cartProductComponent.Id,
                    ItemId = CommerceEntity.IdPrefix<SellableItem>() + cartProductComponent.Id,
                    Name = line.Id,
                    DisplayName = line.Name,
                    UiHint = "Flat"
                };
                cartProductComponents.ChildViews.Add(cartProductEntityView);

                cartProductEntityView.Properties.Add(new ViewProperty { Name = "Name", RawValue = cartProductComponent.Name, UiType = "EntityLink" });
                cartProductEntityView.Properties.Add(new ViewProperty { Name = "Id", RawValue = cartProductComponent.Id });
                cartProductEntityView.Properties.Add(new ViewProperty { Name = "Catalog", RawValue = cartProductComponent.Catalog });
                cartProductEntityView.Properties.Add(new ViewProperty { Name = "Product Url", RawValue = cartProductComponent.ProductUrl });
                cartProductEntityView.Properties.Add(new ViewProperty { Name = "Product Name", RawValue = cartProductComponent.ProductName });
                cartProductEntityView.Properties.Add(new ViewProperty { Name = "Product Image", RawValue = cartProductComponent.Image.SitecoreId, });
                cartProductEntityView.Properties.Add(new ViewProperty { Name = "Tags", RawValue = string.Join("|", cartProductComponent.Tags.Select(element => element.Name)) });
                cartProductEntityView.Properties.Add(new ViewProperty { Name = "Availability Quantity", RawValue = itemAvailabilityComponent.AvailableQuantity });
                cartProductEntityView.Properties.Add(new ViewProperty { Name = "Is Available", RawValue = itemAvailabilityComponent.IsAvailable });
            }
        }

        private void AddContactView(EntityView entityView, CommerceContext commerceContext, Cart cart)
        {
            var contactComponent = cart.GetComponent<ContactComponent>();
            var contactView = new EntityView
            {
                EntityId = contactComponent.CustomerId,
                ItemId = contactComponent.Id,
                Name = contactComponent.Id,
                DisplayName = contactComponent.Name,
                UiHint = "Flat"
            };

            entityView.ChildViews.Add(contactView);

            contactView.Properties.Add(new ViewProperty { Name = "Customer Id", RawValue = contactComponent.CustomerId });
            contactView.Properties.Add(new ViewProperty { Name = "Shopper Id", RawValue = contactComponent.ShopperId });
            contactView.Properties.Add(new ViewProperty { Name = "Is Registered", RawValue = contactComponent.IsRegistered });
            contactView.Properties.Add(new ViewProperty { Name = "Currency", RawValue = contactComponent.Currency });
            contactView.Properties.Add(new ViewProperty { Name = "Language", RawValue = contactComponent.Language });
        }

        private void AddVoucherView(EntityView entityView, CommerceContext commerceContext, Cart cart)
        {
            if (cart == null)
            {
                return;
            }

            var vouchers = new EntityView
            {
                EntityId = entityView.EntityId,
                ItemId = string.Empty,
                DisplayName = "Vouchers",
                Name = "Vouchers",
                UiHint = "Table"
            };
            entityView.ChildViews.Add(vouchers);

            var couponComponent = cart.GetComponent<CartCouponsComponent>();

            foreach (var coupon in couponComponent.List)
            {
                var couponsView = new EntityView
                {
                    EntityId = entityView.EntityId,
                    ItemId = coupon.Promotion.EntityTarget,
                    Name = coupon.CouponId,
                    DisplayName = coupon.CouponId,
                    UiHint = "Flat"
                };
                vouchers.ChildViews.Add(couponsView);

                couponsView.Properties.Add(new ViewProperty { Name = "Coupon Id", RawValue = coupon.CouponId });
                couponsView.Properties.Add(new ViewProperty { Name = "Added Date", RawValue = coupon.AddedDate });
                couponsView.Properties.Add(new ViewProperty { Name = "Promotion Name", RawValue = coupon.Promotion.Name });
                couponsView.Properties.Add(new ViewProperty { Name = "Promotion Entity Target", RawValue = coupon.Promotion.EntityTarget, UiType = "EntityLink" });
            }
        }

        private void AddFulfillmentView(EntityView entityView, CommerceContext commerceContext, Cart cart)
        {
            var physicalFulfillment = cart.GetComponent<PhysicalFulfillmentComponent>();

            var physicalFulfillmentView = new EntityView
            {
                EntityId = entityView.EntityId,
                ItemId = cart.Id,
                DisplayName = "Physical Fulfillment",
                Name = "Physical Fulfillment"
            };

            entityView.ChildViews.Add(physicalFulfillmentView);

            if (physicalFulfillment.ShippingParty == null)
            {
                return;
            }

            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "Method", RawValue = physicalFulfillment.FulfillmentMethod.Name });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "EntityTarget", RawValue = physicalFulfillment.FulfillmentMethod.EntityTarget });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty Name", RawValue = physicalFulfillment.ShippingParty.Name });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "Address Name", RawValue = physicalFulfillment.ShippingParty.AddressName });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty City", RawValue = physicalFulfillment.ShippingParty.City });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty Phonenumber", RawValue = physicalFulfillment.ShippingParty.PhoneNumber });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty Email", RawValue = physicalFulfillment.ShippingParty.Email });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty State", RawValue = physicalFulfillment.ShippingParty.State });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty State Code", RawValue = physicalFulfillment.ShippingParty.StateCode });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty First Name", RawValue = physicalFulfillment.ShippingParty.FirstName });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty Last Name", RawValue = physicalFulfillment.ShippingParty.LastName });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty Country", RawValue = physicalFulfillment.ShippingParty.Country });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty Country Code", RawValue = physicalFulfillment.ShippingParty.CountryCode });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty Address 1", RawValue = physicalFulfillment.ShippingParty.Address1 });
            physicalFulfillmentView.Properties
                .Add(new ViewProperty { Name = "ShippingParty Zip Postal Code", RawValue = physicalFulfillment.ShippingParty.ZipPostalCode });
        }

        private void AddPaymentView(EntityView entityView, CommerceContext commerceContext, Cart cart)
        {
            var paymentComponent = cart.GetComponent<GiftCardPaymentComponent>();
            var paymentEntityView = new EntityView
            {
                EntityId = entityView.EntityId,
                ItemId = paymentComponent.GiftCard.EntityTarget,
                DisplayName = "Gitft Card Payment",
                Name = "Gift Card Payment"
            };

            entityView.ChildViews.Add(paymentEntityView);

            if (string.IsNullOrEmpty(paymentComponent.GiftCardCode))
            {
                return;
            }

            paymentEntityView.Properties
                .Add(new ViewProperty { Name = "Code", RawValue = paymentComponent.GiftCardCode });
            paymentEntityView.Properties
                .Add(new ViewProperty { Name = "Amount", RawValue = paymentComponent.Amount });
            paymentEntityView.Properties
               .Add(new ViewProperty { Name = "Gift Card Name", RawValue = paymentComponent.GiftCard.Name });
            paymentEntityView.Properties
                .Add(new ViewProperty { Name = "Gift Card Entity", RawValue = paymentComponent.GiftCard.EntityTarget, UiType = "EntityLink" });
            paymentEntityView.Properties
                .Add(new ViewProperty { Name = "Paymentmethod Name", RawValue = paymentComponent.PaymentMethod.Name });
            paymentEntityView.Properties
                .Add(new ViewProperty { Name = "Paymentmethod Entity Target", RawValue = paymentComponent.PaymentMethod.EntityTarget });
        }
    }
}
