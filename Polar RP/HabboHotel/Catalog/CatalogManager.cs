using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net;

using Polar.HabboHotel.Items;
using Polar.HabboHotel.Catalog.Pets;
using Polar.HabboHotel.Catalog.Vouchers;
using Polar.HabboHotel.Catalog.Marketplace;
using Polar.HabboHotel.Catalog.Clothing;
using Polar.Database.Interfaces;
using Polar.HabboHotel.GameClients;

namespace Polar.HabboHotel.Catalog
{
    public class CatalogManager
    {
        private static readonly ILog log = LogManager.GetLogger("Polar.HabboHotel.Catalog.CatalogManager");

        private readonly MarketplaceManager _marketplace;
        private readonly PetRaceManager _petRaceManager;
        private readonly VoucherManager _voucherManager;
        private readonly ClothingManager _clothingManager;

        private Dictionary<int, int> _itemOffers; // OfferId -> PageId
        private readonly Dictionary<int, CatalogPage> _pages;
        private readonly Dictionary<int, CatalogBot> _botPresets;
        private readonly Dictionary<int, Dictionary<int, CatalogItem>> _items; // PageId -> Dictionary<ItemId, CatalogItem>
        
        // Nuevo: Diccionario para mapear OfferId -> CatalogItem directamente
        private readonly Dictionary<int, CatalogItem> _offerItems;
        
        private readonly Dictionary<int, int> _itemsPage;

        public CatalogManager()
        {
            this._marketplace = new MarketplaceManager();
            this._petRaceManager = new PetRaceManager();
            this._voucherManager = new VoucherManager();
            this._clothingManager = new ClothingManager();

            this._itemOffers = new Dictionary<int, int>();
            this._itemsPage = new Dictionary<int, int>();
            this._pages = new Dictionary<int, CatalogPage>();
            this._botPresets = new Dictionary<int, CatalogBot>();
            this._items = new Dictionary<int, Dictionary<int, CatalogItem>>();
            this._offerItems = new Dictionary<int, CatalogItem>(); // Nuevo
        }

        public async Task InitAsync(ItemDataManager ItemDataManager)
        {
            if (_pages.Count > 0)
                _pages.Clear();
            if (_botPresets.Count > 0)
                _botPresets.Clear();
            if (_items.Count > 0)
                _items.Clear();
            if (this._itemsPage.Count > 0) this._itemsPage.Clear();
            if (this._offerItems.Count > 0) this._offerItems.Clear(); // Limpiar ofertas

            await Task.Run(() =>
            {
                using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id`,`item_id`,`catalog_name`,`cost_credits`,`cost_pixels`,`cost_diamonds`,`amount`,`page_id`,`limited_sells`,`limited_stack`,`offer_active`,`extradata`,`badge`,`offer_id` FROM `catalog_items` WHERE `offer_active` = '1' ORDER BY `id`");
                    DataTable CatalogueItems = dbClient.getTable();

                    if (CatalogueItems != null)
                    {
                        foreach (DataRow Row in CatalogueItems.Rows)
                        {
                            try
                            {
                                if (Convert.ToInt32(Row["amount"]) <= 0)
                                    continue;

                                int ItemId = Convert.ToInt32(Row["id"]);
                                int PageId = Convert.ToInt32(Row["page_id"]);
                                int BaseId = Convert.ToInt32(Row["item_id"]);
                                int OfferId = Convert.ToInt32(Row["offer_id"]);

                                // Solo procesar si tiene OfferId válido
                                if (OfferId <= 0)
                                    continue;

                                ItemData Data = null;
                                if (!ItemDataManager.GetItem(BaseId, out Data))
                                {
                                    log.Warn($"No se pudo cargar el artículo {ItemId} del catálogo, no se encontró ningún registro de muebles.");
                                    continue;
                                }

                                // Crear el CatalogItem
                                var catalogItem = new CatalogItem(
    Convert.ToInt32(Row["id"]),
    Convert.ToInt32(Row["item_id"]),
    Data,
    Convert.ToString(Row["catalog_name"]),
    Convert.ToInt32(Row["page_id"]),
    Convert.ToInt32(Row["cost_credits"]),
    Convert.ToInt32(Row["cost_pixels"]),
    Convert.ToInt32(Row["cost_diamonds"]),
    Convert.ToInt32(Row["amount"]),
    Convert.ToInt32(Row["limited_sells"]),
    Convert.ToInt32(Row["limited_stack"]),
    Convert.ToString(Row["offer_active"]).ToLower() == "1",
    Convert.ToString(Row["extradata"]),
    Convert.ToString(Row["badge"]),
    Convert.ToInt32(Row["offer_id"])
);

                                // Agregar a la página
                                if (!_items.ContainsKey(PageId))
                                    _items[PageId] = new Dictionary<int, CatalogItem>();

                                _items[PageId][ItemId] = catalogItem;
                                _itemsPage[ItemId] = PageId;

                                // Agregar a las ofertas
                                if (!_itemOffers.ContainsKey(OfferId))
                                    _itemOffers.Add(OfferId, PageId);
                                
                                // Agregar al diccionario de ofertas directo
                                if (!_offerItems.ContainsKey(OfferId))
                                    _offerItems.Add(OfferId, catalogItem);
                                else
                                    log.Warn($"OfferId duplicado {OfferId} para item {ItemId}");
                            }
                            catch (Exception ex)
                            {
                                log.Error($"Error al cargar item del catálogo: {ex}");
                            }
                        }
                    }

                    // Cargar páginas
                    dbClient.SetQuery("SELECT `id`,`parent_id`,`caption`,`page_link`,`visible`,`enabled`,`min_rank`,`min_vip`,`icon_image`,`page_layout`,`page_strings_1`,`page_strings_2` FROM `catalog_pages` ORDER BY `order_num`");
                    DataTable CatalogPages = dbClient.getTable();

                    if (CatalogPages != null)
                    {
                        foreach (DataRow Row in CatalogPages.Rows)
                        {
                            try
                            {
                                int pageId = Convert.ToInt32(Row["id"]);
                                
                                // Obtener items para esta página
                                var pageItems = _items.ContainsKey(pageId) 
                                    ? _items[pageId] 
                                    : new Dictionary<int, CatalogItem>();
                                
                                // Filtrar solo items con oferta activa para ItemOffers de la página
                                var pageItemOffers = pageItems
                                    .Where(x => x.Value.OfferId > 0 && x.Value.OfferActive)
                                    .ToDictionary(x => x.Value.OfferId, x => x.Value);
                                
                                // Crear la página
                                var page = new CatalogPage(
                                    pageId,
                                    Convert.ToInt32(Row["parent_id"]),
                                    Row["enabled"].ToString(),
                                    Convert.ToString(Row["caption"]),
                                    Convert.ToString(Row["page_link"]),
                                    Convert.ToInt32(Row["icon_image"]),
                                    Convert.ToInt32(Row["min_rank"]),
                                    Convert.ToInt32(Row["min_vip"]),
                                    Row["visible"].ToString(),
                                    Convert.ToString(Row["page_layout"]),
                                    Convert.ToString(Row["page_strings_1"]),
                                    Convert.ToString(Row["page_strings_2"]),
                                    pageItems,
                                    pageItemOffers
                                );
                                
                                _pages[pageId] = page;
                            }
                            catch (Exception ex)
                            {
                                log.Error($"Error al cargar página del catálogo: {ex}");
                            }
                        }
                    }

                    // Cargar bots
                    dbClient.SetQuery("SELECT `id`,`name`,`figure`,`motto`,`gender`,`ai_type` FROM `catalog_bot_presets`");
                    DataTable bots = dbClient.getTable();

                    if (bots != null)
                    {
                        foreach (DataRow Row in bots.Rows)
                        {
                            try
                            {
                                _botPresets.Add(
                                    Convert.ToInt32(Row[0]),
                                    new CatalogBot(
                                        Convert.ToInt32(Row[0]),
                                        Convert.ToString(Row[1]),
                                        Convert.ToString(Row[2]),
                                        Convert.ToString(Row[3]),
                                        Convert.ToString(Row[4]),
                                        Convert.ToString(Row[5])
                                    )
                                );
                            }
                            catch (Exception ex)
                            {
                                log.Error($"Error al cargar bot preset: {ex}");
                            }
                        }
                    }

                    _petRaceManager.Init();
                    _clothingManager.Init();

                    log.Info($"Catálogo cargado: {_pages.Count} páginas, {_items.Sum(x => x.Value.Count)} items, {_offerItems.Count} ofertas activas");
                }
            });
        }

        // Nuevo método: Obtener CatalogItem directamente por OfferId
        public bool TryGetOffer(int offerId, out CatalogItem item)
        {
            return _offerItems.TryGetValue(offerId, out item);
        }

        // Nuevo método: Obtener PageId por OfferId
        public bool TryGetPageByOffer(int offerId, out int pageId)
        {
            return _itemOffers.TryGetValue(offerId, out pageId);
        }

        // Método mejorado para GetCatalogOfferEvent
        public bool TryGetCatalogItemByOffer(int offerId, out CatalogItem item, out CatalogPage page)
        {
            item = null;
            page = null;

            // Primero intentar obtener el item directamente
            if (TryGetOffer(offerId, out item))
            {
                // Luego obtener la página
                if (item != null && TryGetPage(item.PageId, out page))
                {
                    return true;
                }
            }

            // Fallback: método antiguo
            if (TryGetPageByOffer(offerId, out int pageId) && TryGetPage(pageId, out page))
            {
                // Buscar el item en la página por OfferId
                item = page.ItemOffers.Values.FirstOrDefault(x => x.OfferId == offerId);
                return item != null;
            }

            return false;
        }

        public bool TryGetBot(int ItemId, out CatalogBot Bot)
        {
            return _botPresets.TryGetValue(ItemId, out Bot);
        }

        public Dictionary<int, int> ItemOffers => _itemOffers;
        public Dictionary<int, CatalogItem> OfferItems => _offerItems; // Nuevo

        public bool TryGetPage(int pageId, out CatalogPage page)
        {
            return _pages.TryGetValue(pageId, out page);
        }

        public ICollection<CatalogPage> GetPages()
        {
            return _pages.Values;
        }

        public MarketplaceManager GetMarketplace()
        {
            return _marketplace;
        }

        public PetRaceManager GetPetRaceManager()
        {
            return _petRaceManager;
        }

        public VoucherManager GetVoucherManager()
        {
            return _voucherManager;
        }

        public ClothingManager GetClothingManager()
        {
            return _clothingManager;
        }

        public List<CatalogPage> GetSubPages(int PageId)
        {
            return _pages.Values.Where(x => x.ParentId == PageId).ToList();
        }

        public CatalogPage GetSectionPage(int PageId)
        {
            return _pages.Values.FirstOrDefault(x => x.ParentId == -1 && GetSubPages(x.ParentId).Select(y => y.Id).ToList().Contains(PageId));
        }

        public CatalogPage TryGetPageByTemplate(string template)
        {
            return _pages.Values.FirstOrDefault(current => current.Template == template);
        }

        public List<CatalogPage> GetChildPages(int PageId)
        {
            return _pages.Values.Where(x => x.ParentId == PageId).ToList();
        }

        public ICollection<CatalogPage> GetPages(GameClient session, int pageId)
        {
            List<CatalogPage> pages = new List<CatalogPage>();
            foreach (CatalogPage page in this._pages.Values)
            {
                if (page.ParentId != pageId || page.MinimumRank > session.GetHabbo().Rank)
                {
                    continue;
                }
                pages.Add(page);
            }
            return pages;
        }
    }
}