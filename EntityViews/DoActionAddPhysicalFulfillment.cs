namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("DoActionAddPhysicalFulfillment")]
    public class DoActionAddPhysicalFulfillment : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;
        private readonly SetCartFulfillmentCommand _setCartFulfillmentCommand;

        public DoActionAddPhysicalFulfillment(
            CommerceCommander commerceCommander,
            GetCartCommand getCartCommand,
            SetCartFulfillmentCommand setCartFulfillmentCommand)
        {
            this._getCartCommand = getCartCommand;
            this._commerceCommander = commerceCommander;
            this._setCartFulfillmentCommand = setCartFulfillmentCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            if (entityView == null
                || !entityView.Action.Equals(context.GetPolicy<CartUiPolicy>().AddPhysicalFulfillmentActionName, StringComparison.OrdinalIgnoreCase))
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

                var addressName = entityView.Properties[1];
                var firstName = entityView.Properties[2];
                var lastName = entityView.Properties[3];
                var city = entityView.Properties[4];
                var address1 = entityView.Properties[5];
                var country = entityView.Properties[6];
                var zipCode = entityView.Properties[7];
                var stateCode = entityView.Properties[8];
                var id = entityView.Properties[9];

                var physicalFulfillmentComponent = new PhysicalFulfillmentComponent()
                {
                    Name = addressName.Value,
                    Id = string.IsNullOrEmpty(id.Value) ? Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) : id.Value,
                    ShippingParty = new Party()
                    {
                        AddressName = addressName.Value,
                        FirstName = firstName.Value,
                        LastName = lastName.Value,
                        Address1 = address1.Value,
                        City = city.Value,
                        Country = country.Value,
                        ZipPostalCode = zipCode.Value,
                        StateCode = stateCode.Value
                    },
                    FulfillmentMethod = new EntityReference()
                    {
                        EntityTarget = "B146622D-DC86-48A3-B72A-05EE8FFD187A",
                        Name = "Ground"
                    }
                };

                await _setCartFulfillmentCommand.Process(context.CommerceContext, entityView.EntityId, physicalFulfillmentComponent);
            }

            catch (Exception ex)
            {
                context.Logger.LogError($"{this.Name}.PathNotFound: Message={ex.Message}");
            }

            return entityView;
        }
    }
}
