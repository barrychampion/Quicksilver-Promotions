using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Pages
{
    [ContentType(
        DisplayName = "Single Promotion page",
        GUID = "b67a1ae8-3280-4f85-a7d6-6cd176df5a13", 
        Description = "Disdplay a single discount",
        AvailableInEditMode = true)]
    [ImageUrl("~/styles/images/page_type.png")]
    public class SinglePromotionPage : PageData
    {
        [Display(
                Name = "Discount",
                Description = "",
                Order = 0)]
        [AllowedTypes(typeof(EPiServer.Commerce.Marketing.PromotionData))]
        public virtual ContentReference PromotionContent { get; set; }

        [CultureSpecific]
        [Display(
                Name = "Title",
                Description = "Title for the start page",
                GroupName = SystemTabNames.Content,
                Order = 1)]
        public virtual string Title { get; set; }

        [CultureSpecific]
        [Display(
                Name = "Main body",
                Description = "",
                GroupName = SystemTabNames.Content,
                Order = 2)]
        public virtual XhtmlString MainBody { get; set; }
    }
}