using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Pages
{
    [ContentType(
        DisplayName = "Promotions page", 
        GUID = "58dcf4dd-c22d-4ee5-9879-3d9440b0daff",
        Description = "The promotions page.", 
        AvailableInEditMode = true)]
    [ImageUrl("~/styles/images/page_type.png")]
    public class PromotionsPage : PageData
    {
        [Display(
            Name = "Campaign",
            Description = "",
            Order = 0)]
        [AllowedTypes(typeof(EPiServer.Commerce.Marketing.SalesCampaign))]
        public virtual ContentReference CampaignContent { get; set; }

        [CultureSpecific]
        [Display(
               Name = "Title",
               Description = "Title for the page",
               GroupName = SystemTabNames.Content,
               Order = 1)]
        public virtual string Title { get; set; }

        [CultureSpecific]
        [Display(
               Name = "Main body",
               Description = "Main body",
               GroupName = SystemTabNames.Content,
               Order = 2)]
        public virtual XhtmlString MainBody { get; set; }
        public ContentReference OptinConfirmSuccessPage { get; internal set; }
    }
}