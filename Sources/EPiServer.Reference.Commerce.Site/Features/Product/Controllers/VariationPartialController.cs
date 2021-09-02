using System.Web.Mvc;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using EPiServer.Reference.Commerce.Site.Features.Product.Services;
using EPiServer.Commerce.Marketing;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using System.Linq;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Controllers
{
    [TemplateDescriptor(Inherited = true)]
    public class VariationPartialController : PartialContentController<VariationContent>
    {
        private readonly IProductService _productService;
        private Injected<IPromotionEngine> _promoEngine;
        private Injected<ICurrentMarket> _currentMarket;

        public VariationPartialController(IProductService productService)
        {
            _productService = productService;
        }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public override ActionResult Index(VariationContent currentContent)
        {
            var market = _currentMarket.Service.GetCurrentMarket();

            var promos = _promoEngine.Service.Evaluate(currentContent.ContentLink, market, market.DefaultCurrency, RequestFulfillmentStatus.All).ToList();
            var filteredPromos = promos
                .Where(x => x.Promotion != null && x.Status == FulfillmentStatus.Fulfilled && (x.SavedAmount > 0 || x.UnitDiscount > 0 || x.Percentage > 0))
                .GroupBy(x => x.Promotion.ContentLink.ID)
                .Select(x => x.First())
                .Take(10);

            return PartialView("_Product", _productService.GetProductTileViewModel(currentContent));
        }
    }
}