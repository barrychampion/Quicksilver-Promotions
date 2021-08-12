
using EPiServer.Personalization.Commerce.Tracking;

using EPiServer.Reference.Commerce.Site.Features.Shared.Pages;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels
{
    public class PromotionsPageViewModel : PageViewModel<PromotionsPage>
    {
        public PromotionsPage PromotionsPage { get; set; }
        public IEnumerable<PromotionViewModel> Promotions { get; set; }
        public IEnumerable<Recommendation> Recommendations { get; set; }
    }
}