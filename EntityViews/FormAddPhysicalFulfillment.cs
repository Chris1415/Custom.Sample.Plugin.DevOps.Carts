namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Commerce.Plugin.Fulfillment;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("FormAddPhysicalFulfillment")]
    public class FormAddPhysicalFulfillment : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;

        public FormAddPhysicalFulfillment(CommerceCommander commerceCommander, GetCartCommand getCartCommand)
        {
            this._commerceCommander = commerceCommander;
            this._getCartCommand = getCartCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");

            if (entityView.Name != context.GetPolicy<CartUiPolicy>().AddPhysicalFulfillmentActionName)
            {
                return entityView;
            }

            var cart = await _getCartCommand.Process(context.CommerceContext, entityView.EntityId, false);
            var physicalFulfillment = cart.GetComponent<PhysicalFulfillmentComponent>();

            try
            {
                entityView.Properties.Add(
                    new ViewProperty
                    {
                        Name = "Address Name",
                        IsHidden = false,
                        IsRequired = true,
                        RawValue = physicalFulfillment.ShippingParty?.AddressName
                    });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "First Name",
                       IsHidden = false,
                       IsRequired = true,
                       RawValue = physicalFulfillment.ShippingParty?.FirstName
                   });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "Last Name",
                       IsHidden = false,
                       IsRequired = true,
                       RawValue = physicalFulfillment.ShippingParty?.LastName
                   });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "City",
                       IsHidden = false,
                       IsRequired = true,
                       RawValue = physicalFulfillment.ShippingParty?.City
                   });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "Street & House Number",
                       IsHidden = false,
                       IsRequired = true,
                       RawValue = physicalFulfillment.ShippingParty?.Address1
                   });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "Country Code",
                       IsHidden = false,
                       IsRequired = true,
                       RawValue = physicalFulfillment.ShippingParty?.Country
                   });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "Zip Code",
                       IsHidden = false,
                       IsRequired = true,
                       RawValue = physicalFulfillment.ShippingParty?.ZipPostalCode
                   });
                entityView.Properties.Add(
                  new ViewProperty
                  {
                      Name = "State Code",
                      IsHidden = false,
                      IsRequired = true,
                      RawValue = physicalFulfillment.ShippingParty?.StateCode
                  });
                entityView.Properties.Add(
                  new ViewProperty
                  {
                      Name = "Id",
                      IsHidden = true,
                      IsRequired = true,
                      RawValue = physicalFulfillment.Id
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
