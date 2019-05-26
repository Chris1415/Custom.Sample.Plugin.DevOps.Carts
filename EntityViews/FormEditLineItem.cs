namespace Custom.Sameple.Plugin.DevOps.Carts
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Carts;
    using Sitecore.Framework.Conditions;
    using Sitecore.Framework.Pipelines;

    [PipelineDisplayName("FormEditLineItem")]
    public class FormEditLineItem : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        private readonly GetCartCommand _getCartCommand;

        public FormEditLineItem(CommerceCommander commerceCommander, GetCartCommand getCartCommand)
        {
            this._commerceCommander = commerceCommander;
            this._getCartCommand = getCartCommand;
        }

        public override async Task<EntityView> Run(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{this.Name}: The argument cannot be null");

            if (entityView.Name != context.GetPolicy<CartUiPolicy>().EditLineItemActionName)           
            {
                return entityView;
            }

            var entityViewArgument = this._commerceCommander.Command<ViewCommander>().CurrentEntityViewArgument(context.CommerceContext);
            var cart = await _getCartCommand.Process(context.CommerceContext, entityView.EntityId, false);
            var line = cart.Lines.FirstOrDefault(element => element.ItemId.Equals(entityView.ItemId));

            try
            {
                entityView.Properties.Add(
                   new ViewProperty
                   {
                       Name = "Quantity",
                       IsHidden = false,
                       IsRequired = true,
                       RawValue = line.Quantity,
                       OriginalType = "System.int"
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
