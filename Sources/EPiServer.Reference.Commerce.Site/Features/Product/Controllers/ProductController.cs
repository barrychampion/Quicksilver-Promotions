using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Product.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Infrastructure.Facades;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc;
using Mediachase.Commerce;
using System.Linq;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Product.Controllers
{
    public class ProductController : ContentController<FashionProduct>
    {
        private readonly bool _isInEditMode;
        private readonly CatalogEntryViewModelFactory _viewModelFactory;
        private Injected<IPromotionEngine> _promoEngine;
        private Injected<ICurrentMarket> _currentMarket;
        private Injected<IContentRepository> _repo;

        public ProductController(IsInEditModeAccessor isInEditModeAccessor, CatalogEntryViewModelFactory viewModelFactory)
        {
            _isInEditMode = isInEditModeAccessor();
            _viewModelFactory = viewModelFactory;
        }

        [HttpGet]
        public ActionResult Index(FashionProduct currentContent, string entryCode = "", bool useQuickview = false, bool skipTracking = false)
        {
            var viewModel = _viewModelFactory.Create(currentContent, entryCode);
            viewModel.SkipTracking = skipTracking;

            if (_isInEditMode && viewModel.Variant == null)
            {
                var emptyViewName = "ProductWithoutEntries";
                return Request.IsAjaxRequest() ? PartialView(emptyViewName, viewModel) : (ActionResult)View(emptyViewName, viewModel);
            }

            if (viewModel.Variant == null)
            {
                return HttpNotFound();
            }

            var market = _currentMarket.Service.GetCurrentMarket();
            viewModel.Promos = _promoEngine.Service.Evaluate(currentContent.GetVariants(), market, market.DefaultCurrency, RequestFulfillmentStatus.All)
                .Where(x => x.Promotion != null && x.Status == FulfillmentStatus.Fulfilled && (x.SavedAmount > 0 || x.UnitDiscount > 0 || x.Percentage > 0))
                .GroupBy(x => x.Promotion.ContentLink.ID)
                .Select(x => x.First())
                .Take(10);

            viewModel.Campaigns = viewModel.Promos.Select(x => _repo.Service.Get<SalesCampaign>(x.Promotion.ParentLink)).Distinct();

            if (useQuickview)
            {
                return PartialView("_Quickview", viewModel);
            }
            return Request.IsAjaxRequest() ? PartialView(viewModel) : (ActionResult)View(viewModel);
        }

        [HttpPost]
        public ActionResult SelectVariant(FashionProduct currentContent, string color, string size, bool useQuickview = false)
        {
            var variant = _viewModelFactory.SelectVariant(currentContent, color, size);
            if (variant != null)
            {
                return RedirectToAction("Index", new { entryCode = variant.Code, useQuickview, skipTracking = true });
            }

            return HttpNotFound();
        }
    }
}