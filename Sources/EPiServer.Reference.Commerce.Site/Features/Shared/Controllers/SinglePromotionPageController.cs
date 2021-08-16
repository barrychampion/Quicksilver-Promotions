using EPiServer.Core;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Pages;
using EPiServer.Reference.Commerce.Site.Features.Shared.ViewModels;
using EPiServer.Web.Mvc;
using Mediachase.Commerce;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Web.Routing;
using System;
using System.Web.Mvc;

namespace EPiServer.Reference.Commerce.Site.Features.Shared.Controllers
{
    public class SinglePromotionPageController : PageController<SinglePromotionPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IContentRepository _contentRepo;
        //private readonly ISiteDefinitionRepository _siteDefinitionRepo;
        private readonly ICurrentMarket _currentMarket;
        private readonly MarketContentLoader _marketContentFilter;
        private readonly IPromotionEngine _promo;

        public SinglePromotionPageController(
            IContentLoader contentLoader,
            IContentRepository contentRepo,
            //ISiteDefinitionRepository siteDefinitionRepo,
            ICurrentMarket currentMarket,
            MarketContentLoader marketContentFilter,
            IPromotionEngine promotionEngine
            )
        {
            _contentLoader = contentLoader;
            _contentRepo = contentRepo;
            //_siteDefinitionRepo = siteDefinitionRepo;
            _currentMarket = currentMarket;
            _marketContentFilter = marketContentFilter;
            _promo = promotionEngine;
        }

        [HttpGet]
        public ViewResult Index(SinglePromotionPage currentPage, int page = 1, int pageSize = 12)
        {
            #region Input sanitize
            pageSize = pageSize > 50 ? 50 : pageSize;
            page = page < 1 ? 1 : page;
            int skip = (page - 1) * pageSize;
            #endregion
            var viewModel = new SinglePromotionPageViewModel()
            {
                Current = currentPage
            };

            PromotionData promo;
            if (!_contentRepo.TryGet<PromotionData>(currentPage.PromotionContent, out promo))
            {
                Response.StatusCode = 404;
                return null;
            }
            PromotionItems promoItems = _marketContentFilter.GetPromotionItemsForMarket(_currentMarket.GetCurrentMarket())
            .Where(x => promo.ContentLink.ID == x.Promotion.ContentLink.ID).FirstOrDefault();
            if (promoItems == null)
            {
                Response.StatusCode = 404;
                return null;
            }

            viewModel.Promotion = new PromotionViewModel()
            {
                Name = promo.Name,
                BannerImage = promo.Banner,
                SelectionType = promoItems.Condition.Type,
                Items = GetProductsForPromotion(itemsOnPromotion: promoItems)
            };

            return View(viewModel);
        }

        private IEnumerable<CatalogContentBase> GetProductsForPromotion(PromotionItems itemsOnPromotion)
        {
            var conditionProducts = new List<CatalogContentBase>();

            foreach (var conditionItemReference in itemsOnPromotion.Condition.Items)
            {
                CatalogContentBase conditionItem;
                if (_contentLoader.TryGet(conditionItemReference, out conditionItem))
                {
                    AddIfProduct(conditionItem, conditionProducts);
                    var nodeContent = conditionItem as NodeContentBase;
                    if (nodeContent != null)
                    {
                        AddItemsRecursive(nodeContent, itemsOnPromotion, conditionProducts);
                    }
                }
            }

            return conditionProducts;
        }

        private void AddItemsRecursive(NodeContentBase nodeContent, PromotionItems itemsOnPromotion, List<CatalogContentBase> conditionProducts)
        {
            foreach (var child in _contentLoader.GetChildren<CatalogContentBase>(nodeContent.ContentLink))
            {
                AddIfProduct(child, conditionProducts);

                var childNode = child as NodeContentBase;
                if (childNode != null && itemsOnPromotion.Condition.IncludesSubcategories)
                {
                    AddItemsRecursive(childNode, itemsOnPromotion, conditionProducts);
                }
            }
        }

        private static void AddIfProduct(CatalogContentBase content, List<CatalogContentBase> productsInPromotion)
        {
            if (content is ProductContent)
            {
                productsInPromotion.Add(content);
            }
        }
    }
}