namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Core.Commands;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;

    public class ChildViewActiveCarts : CommerceCommand
    {
        private readonly CommerceCommander _commerceCommander;

        public ChildViewActiveCarts(
            IServiceProvider serviceProvider,
            CommerceCommander commerceCommander) : base(serviceProvider)
        {
            this._commerceCommander = commerceCommander;
        }

        public async Task<EntityView> Process(CommerceContext commerceContext, EntityView entityView)
        {
            using (CommandActivity.Start(commerceContext, this))
            {
                try
                {
                    var activeCartsView = new EntityView
                    {
                        EntityId = string.Empty,
                        ItemId = string.Empty,
                        DisplayName = "Active Carts List View",
                        Name = "CustomDevOps-ListView-ActiveCarts",
                        UiHint = "Table",
                        Icon = commerceContext.GetPolicy<CartUiPolicy>().Icon
                    };
                    entityView.ChildViews.Add(activeCartsView);

                    CommerceList<Cart> carts = await _commerceCommander.Command<FindEntitiesInListCommand>().Process<Cart>(commerceContext, CommerceEntity.ListName<Cart>(), 0, int.MaxValue);

                    foreach (Cart cart in carts.Items)
                    {

                        var cartView = new EntityView
                        {
                            EntityId = cart.Id,
                            ItemId = cart.Id,
                            DisplayName = cart.DisplayName,
                            Name = cart.Name,
                            Icon = commerceContext.GetPolicy<CartUiPolicy>().Icon
                        };

                        cartView.Properties.Add(new ViewProperty { Name = "Name", RawValue = cart.Name, UiType = "EntityLink" });
                        cartView.Properties.Add(new ViewProperty { Name = "ShopName", RawValue = cart.ShopName });
                        cartView.Properties.Add(new ViewProperty { Name = "Last Changed", RawValue = cart.DateUpdated });
                        cartView.Properties.Add(new ViewProperty { Name = "Lines", RawValue = cart.Lines.Count });
                        cartView.Properties.Add(new ViewProperty { Name = "Subtotal", RawValue = cart.Totals.SubTotal });
                        cartView.Properties.Add(new ViewProperty { Name = "Adjustments", RawValue = cart.Totals.AdjustmentsTotal });
                        cartView.Properties.Add(new ViewProperty { Name = "Grandtotal", RawValue = cart.Totals.GrandTotal });

                        activeCartsView.ChildViews.Add(cartView);
                    }
                }
                catch (Exception ex)
            {
                commerceContext.Logger.LogError($"ChildViewRunningMinions.Exception: Message={ex.Message}");
            }
            return entityView;
        }
    }
}
}