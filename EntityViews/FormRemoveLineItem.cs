namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("FormRemoveLineItem")]
    public class FormRemoveLineItem : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;

        public FormRemoveLineItem(CommerceCommander commerceCommander, GetCartCommand getCartCommand)
        {
            this._commerceCommander = commerceCommander;
            this._getCartCommand = getCartCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");

            if (entityView.Name != context.GetPolicy<CartUiPolicy>().RemoveLineItemActionName)
            {
                return entityView;
            }

            var entityViewArgument = this._commerceCommander.Command<ViewCommander>().CurrentEntityViewArgument(context.CommerceContext);
            var cart = await _getCartCommand.Process(context.CommerceContext, entityView.EntityId, false);
            string[] cartLineInformation = entityView.ItemId.Split('|');
            try
            {
                entityView.Properties.Add(
                    new ViewProperty
                    {
                        Name = "Cart",
                        IsHidden = true,
                        IsRequired = true,
                        RawValue = cart.Id
                    });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "Catalog",
                       IsHidden = true,
                       IsRequired = true,
                       RawValue = cartLineInformation[0]
                   });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "ProductID",
                       IsHidden = true,
                       IsRequired = true,
                       RawValue = cartLineInformation[1]
                   });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "VariantID",
                       IsHidden = true,
                       IsRequired = true,
                       RawValue = cartLineInformation[2]
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
