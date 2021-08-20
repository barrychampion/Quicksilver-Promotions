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
    public class PromotionsPageController : PageController<PromotionsPage>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IContentRepository _contentRepo;
        //private readonly ISiteDefinitionRepository _siteDefinitionRepo;
        private readonly ICurrentMarket _currentMarket;
        private readonly MarketContentLoader _marketContentFilter;
        private readonly IPromotionEngine _promo;

        public PromotionsPageController(
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
        public ActionResult Index(PromotionsPage currentPage)
        {
            PromotionsPageViewModel model = new PromotionsPageViewModel
            {
                CurrentPage = currentPage,
                Promotions = GetActivePromotions(currentPage.CampaignContent)
            };
            if (model.Promotions == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(model);
        }

        private IEnumerable<PromotionViewModel> GetActivePromotions(ContentReference campaignRef)
        {
            if (ContentReference.IsNullOrEmpty(campaignRef)) return null;
            SalesCampaign campaign;
            try
            {
                campaign = _contentRepo.Get<SalesCampaign>(campaignRef);
            }
            catch (System.Exception)
            {
                return null;
            }
            if (campaign == null) return null;

            var promotionIds = _contentRepo.GetChildren<PromotionData>(campaign.ContentLink).Select(x => x.ContentLink.ID);

            var promotions = new List<PromotionViewModel>();
            var promotionItemGroups = _marketContentFilter.GetPromotionItemsForMarket(_currentMarket.GetCurrentMarket())
                .Where(x => promotionIds.Contains(x.Promotion.ContentLink.ID))
                .GroupBy(x => x.Promotion);

            foreach (var promotionGroup in promotionItemGroups)
            {
                var promotionItems = promotionGroup.First();
                promotions.Add(new PromotionViewModel()
                {
                    Name = promotionGroup.Key.Name,
                    Description = promotionGroup.Key.Description,
                    BannerImage = promotionGroup.Key.Banner,
                    SelectionType = promotionItems.Condition.Type,
                    Items = GetProductsForPromotion(promotionItems)
                });
            }

            return promotions;
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