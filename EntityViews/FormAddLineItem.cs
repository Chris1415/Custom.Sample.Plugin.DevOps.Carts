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

    [PipelineDisplayName("FormAddLineItem")]
    public class FormAddLineItem : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;

        public FormAddLineItem(CommerceCommander commerceCommander, GetCartCommand getCartCommand)
        {
            this._commerceCommander = commerceCommander;
            this._getCartCommand = getCartCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");

            if (entityView.Name != context.GetPolicy<CartUiPolicy>().AddLineItemActionName)
            {
                return entityView;
            }

            var entityViewArgument = this._commerceCommander.Command<ViewCommander>().CurrentEntityViewArgument(context.CommerceContext);
            var cart = await _getCartCommand.Process(context.CommerceContext, entityView.EntityId, false);

            try
            {
                entityView.Properties.Add(
                    new ViewProperty
                    {
                        Name = "Catalog",
                        IsHidden = false,
                        IsRequired = true,
                        RawValue = string.Empty
                    });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "Product ID",
                       IsHidden = false,
                       IsRequired = true,
                       RawValue = string.Empty
                   });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "Variant ID",
                       IsHidden = false,
                       IsRequired = true,
                       RawValue = string.Empty
                   });
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "Quantity",
                       IsHidden = false,
                       IsRequired = true,
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
