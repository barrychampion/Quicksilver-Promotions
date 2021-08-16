using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Shared.Pages;
using System.Collections.Generic;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels
{
    public class SinglePromotionPageViewModel : PageViewModel<SinglePromotionPage>
    {
        public SinglePromotionPage Current { get; set; }
        public PromotionViewModel Promotion { get; set; }
        public IEnumerable<Recommendation> Recommendations { get; set; }
    }
}