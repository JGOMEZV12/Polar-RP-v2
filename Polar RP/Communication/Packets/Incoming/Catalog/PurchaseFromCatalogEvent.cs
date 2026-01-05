using Polar.Communication.Packets.Incoming;
using Polar.Communication.Packets.Outgoing.Catalog;
using Polar.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Polar.Communication.Packets.Outgoing.Inventory.Bots;
using Polar.Communication.Packets.Outgoing.Inventory.Furni;
using Polar.Communication.Packets.Outgoing.Inventory.Pets;
using Polar.Communication.Packets.Outgoing.Inventory.Purse;
using Polar.Communication.Packets.Outgoing.Rooms.Notifications;
using Polar.Communication.Packets.Outgoing.Users;
using Polar.Core;
using Polar.Database.Interfaces;
using Polar.HabboHotel.Catalog;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Items.Utilities;
using Polar.HabboHotel.Rooms.AI;
using Polar.HabboHotel.Users.Effects;
using System;
using System.Collections.Generic;

namespace Polar.Communication.Packets.Incoming.Catalog
{
    internal sealed class PurchaseFromCatalogEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            try
            {
                //Console.WriteLine($"=== PurchaseFromCatalogEvent INICIO ===");

                // Validar catálogo habilitado
                if (PolarEnvironment.GetDBConfig().DBData["catalogue_enabled"] != "1")
                {
                    session.SendNotification("Staff han inhabilitado el catálogo");
                    //Console.WriteLine("Catálogo deshabilitado");
                    return;
                }

                // Leer datos del paquete
                int pageId = packet.PopInt();
                int offerId = packet.PopInt();
                string extraData = packet.PopString();
                int amount = packet.PopInt();

                //Console.WriteLine($"Datos recibidos: PageId={pageId}, OfferId={offerId}, ExtraData='{extraData}', Amount={amount}");

                // Validar cantidad
                amount = Math.Clamp(amount, 1, 100);
                //Console.WriteLine($"Cantidad ajustada: {amount}");

                // Obtener el item del catálogo
                CatalogItem catalogItem = null;
                CatalogPage page = null;

                // CASO 1: PageId = -1 (compra directa por OfferId)
                if (pageId == -1)
                {
                    //Console.WriteLine($"Compra directa por OfferId: {offerId}");

                    // Buscar el item directamente por OfferId
                    var catalog = PolarEnvironment.GetGame().GetCatalog();

                    // Método 1: Buscar en OfferItems
                    if (catalog.OfferItems.TryGetValue(offerId, out catalogItem))
                    {
                        //Console.WriteLine($"Item encontrado en OfferItems: {catalogItem.Id} - {catalogItem.Name}");

                        // Obtener la página del item
                        if (catalog.TryGetPage(catalogItem.PageId, out page))
                        {
                            //Console.WriteLine($"Página encontrada: {page.Id} - {page.Caption}");
                        }
                        else
                        {
                            //Console.WriteLine($"ERROR: No se encontró la página {catalogItem.PageId} para el item");
                            session.SendNotification("Error: Página no encontrada");
                            return;
                        }
                    }
                    // Método 2: Buscar en todas las páginas
                    else
                    {
                        //Console.WriteLine($"Buscando item en todas las páginas...");
                        foreach (var catalogPage in catalog.GetPages())
                        {
                            if (catalogPage.ItemOffers.TryGetValue(offerId, out catalogItem))
                            {
                                page = catalogPage;
                                //Console.WriteLine($"Item encontrado en página {page.Id}: {catalogItem.Id} - {catalogItem.Name}");
                                break;
                            }
                        }
                    }

                    if (catalogItem == null)
                    {
                        //Console.WriteLine($"ERROR: No se encontró item para OfferId {offerId}");
                        session.SendNotification("El artículo no está disponible");
                        return;
                    }
                }
                // CASO 2: Compra normal por página
                else
                {
                    // Obtener página del catálogo
                    if (!PolarEnvironment.GetGame().GetCatalog().TryGetPage(pageId, out page))
                    {
                        //Console.WriteLine($"Página {pageId} no encontrada");
                        session.SendNotification("La página no existe");
                        return;
                    }

                    //Console.WriteLine($"Página encontrada: {page.Id} - {page.Caption}");

                    if (!page.Enabled || !page.Visible)
                    {
                        //Console.WriteLine($"Página no habilitada o visible");
                        session.SendNotification("La página no está disponible");
                        return;
                    }

                    // Validar rango del usuario
                    if (page.MinimumRank > session.GetHabbo().Rank ||
                        (page.MinimumVIP > session.GetHabbo().VIPRank && session.GetHabbo().Rank == 1))
                    {
                        //Console.WriteLine($"Usuario no tiene permisos: Rank={session.GetHabbo().Rank}, VIP={session.GetHabbo().VIPRank}, Requerido: MinRank={page.MinimumRank}, MinVIP={page.MinimumVIP}");
                        session.SendNotification("No tienes permisos para acceder a esta página");
                        return;
                    }

                    // Obtener item del catálogo
                    catalogItem = GetCatalogItem(page, offerId);
                    if (catalogItem == null)
                    {
                        //Console.WriteLine($"Item no encontrado para OfferId {offerId} en página {pageId}");
                        session.SendNotification("El artículo no existe");
                        return;
                    }
                }

                //Console.WriteLine($"Item encontrado: {catalogItem.Id} - {catalogItem.Name}");
                //Console.WriteLine($"Tipo: {catalogItem.Data.Type}, Costo: {catalogItem.CostCredits}c/{catalogItem.CostPixels}d/{catalogItem.CostDiamonds}dm");

                // Procesar la compra
                ProcessPurchase(session, catalogItem, extraData, amount);
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"ERROR en PurchaseFromCatalogEvent: {ex.Message}");
                //Console.WriteLine($"Stack trace: {ex.StackTrace}");
                session.SendNotification("Error al procesar la compra");
            }
        }

        private CatalogItem GetCatalogItem(CatalogPage page, int identifier)
        {
            // Primero buscar por OfferId en ItemOffers
            if (page.ItemOffers.TryGetValue(identifier, out CatalogItem item))
            {
                //Console.WriteLine($"Item encontrado por OfferId: {identifier}");
                return item;
            }

            // Luego buscar por ItemId en Items
            if (page.Items.TryGetValue(identifier, out item))
            {
                //Console.WriteLine($"Item encontrado por ItemId: {identifier}");
                return item;
            }

            //Console.WriteLine($"Item NO encontrado: {identifier}");
            return null;
        }

        private void ProcessPurchase(GameClient session, CatalogItem item, string extraData, int amount)
        {
            //Console.WriteLine($"=== Procesando compra ===");
            //Console.WriteLine($"Item: {item.Name}, Tipo: {item.Data.Type}, Cantidad: {amount}");

            // Calcular costos totales
            int totalCredits = item.CostCredits * amount;
            int totalPixels = item.CostPixels * amount;
            int totalDiamonds = item.CostDiamonds * amount;

            //Console.WriteLine($"Costos: Credits={totalCredits}, Pixels={totalPixels}, Diamonds={totalDiamonds}");
            //Console.WriteLine($"Fondos usuario: Credits={session.GetHabbo().Credits}, Pixels={session.GetHabbo().Duckets}, Diamonds={session.GetHabbo().Diamonds}");

            // Validar fondos
            if (session.GetHabbo().Credits < totalCredits ||
                session.GetHabbo().Duckets < totalPixels ||
                session.GetHabbo().Diamonds < totalDiamonds)
            {
                //Console.WriteLine("Fondos insuficientes");
                session.SendNotification("No tienes suficientes fondos");
                return;
            }

            // Procesar item limitado
            if (item.IsLimited)
            {
                //Console.WriteLine($"Item limitado: Stack={item.LimitedEditionStack}, Sells={item.LimitedEditionSells}");
                if (!ProcessLimitedItem(item, session))
                    return;
            }

            // Deduct currency
            //Console.WriteLine("Deductiendo monedas...");
            DeductCurrency(session, totalCredits, totalPixels, totalDiamonds);

            // Crear y entregar item
            //Console.WriteLine("Entregando item...");
            DeliverItem(session, item, extraData, amount);

            // Enviar confirmación
            //Console.WriteLine("Enviando confirmación de compra...");
            session.SendMessage(new PurchaseOKComposer(item, item.Data));

            // Notificación
            SendPurchaseNotification(session, item, amount);

            // Actualizar inventario
            session.SendMessage(new FurniListUpdateComposer());

            //Console.WriteLine($"=== Compra completada exitosamente ===");
            //return;
        }

        private bool ProcessLimitedItem(CatalogItem item, GameClient session)
        {
            if (item.LimitedEditionStack <= item.LimitedEditionSells)
            {
                //Console.WriteLine($"Item limitado agotado: Stack={item.LimitedEditionStack}, Sells={item.LimitedEditionSells}");
                session.SendNotification("¡Este artículo se ha agotado!");
                session.SendMessage(new CatalogUpdatedComposer());
                session.SendMessage(new PurchaseOKComposer());
                return false;
            }

            item.LimitedEditionSells++;
            //Console.WriteLine($"Actualizando item limitado: Nuevo sells={item.LimitedEditionSells}");

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `catalog_items` SET `limited_sells` = @sells WHERE `id` = @id");
                dbClient.AddParameter("sells", item.LimitedEditionSells);
                dbClient.AddParameter("id", item.Id);
                dbClient.RunQuery();
            }

            return true;
        }

        private void DeductCurrency(GameClient session, int credits, int pixels, int diamonds)
        {
            if (credits > 0)
            {
                session.GetHabbo().Credits -= credits;
                session.SendMessage(new CreditBalanceComposer(session.GetHabbo().Credits));
                //Console.WriteLine($"Créditos deducidos: {credits}, Nuevo saldo: {session.GetHabbo().Credits}");
            }

            if (pixels > 0)
            {
                session.GetHabbo().Duckets -= pixels;
                session.SendMessage(new HabboActivityPointNotificationComposer(
                    session.GetHabbo().Duckets, session.GetHabbo().Duckets));
                //Console.WriteLine($"Duckets deducidos: {pixels}, Nuevo saldo: {session.GetHabbo().Duckets}");
            }

            if (diamonds > 0)
            {
                session.GetHabbo().Diamonds -= diamonds;
                session.SendMessage(new HabboActivityPointNotificationComposer(
                    session.GetHabbo().Diamonds, 0, 5));
                //Console.WriteLine($"Diamantes deducidos: {diamonds}, Nuevo saldo: {session.GetHabbo().Diamonds}");
            }
        }

        private void DeliverItem(GameClient session, CatalogItem catalogItem, string extraData, int amount)
        {
            //Console.WriteLine($"Entregando item tipo: {catalogItem.Data.Type}, Interaction: {catalogItem.Data.InteractionType}");

            switch (catalogItem.Data.Type.ToString().ToLower())
            {
                case "s": // Furniture
                    DeliverFurniture(session, catalogItem, extraData, amount);
                    break;

                case "e": // Effect
                    DeliverEffect(session, catalogItem);
                    break;

                case "r": // Bot
                    DeliverBot(session, catalogItem);
                    break;

                case "b": // Badge
                    DeliverBadge(session, catalogItem);
                    break;

                case "p": // Pet
                    DeliverPet(session, catalogItem, extraData);
                    break;

                default:
                    //Console.WriteLine($"Tipo desconocido: {catalogItem.Data.Type}, tratando como furniture");
                    DeliverFurniture(session, catalogItem, extraData, amount);
                    break;
            }

            // Badge adicional si existe
            if (!string.IsNullOrEmpty(catalogItem.Badge))
            {
                //Console.WriteLine($"Entregando badge adicional: {catalogItem.Badge}");
                session.GetHabbo().GetBadgeComponent().GiveBadge(catalogItem.Badge, true, session);
            }
        }

        private void DeliverFurniture(GameClient session, CatalogItem item, string extraData, int amount)
        {
            //Console.WriteLine($"Creando furniture: Amount={amount}");

            List<Item> items = new List<Item>();

            if (amount > 1)
            {
                items = ItemFactory.CreateMultipleItems(item.Data, session.GetHabbo(), extraData, amount);
                //Console.WriteLine($"Creados {items?.Count ?? 0} items múltiples");
            }
            else
            {
                var newItem = ItemFactory.CreateSingleItemNullable(item.Data, session.GetHabbo(), extraData, extraData);
                if (newItem != null)
                {
                    items.Add(newItem);
                    //Console.WriteLine($"Creado 1 item: {newItem.Id}");
                }
            }

            if (items != null)
            {
                foreach (var furniItem in items)
                {
                    if (session.GetHabbo().GetInventoryComponent().TryAddItem(furniItem))
                    {
                        session.SendMessage(new FurniListNotificationComposer(furniItem.Id, 1));
                        //Console.WriteLine($"Item agregado al inventario: {furniItem.Id}");
                    }
                    else
                    {
                        //Console.WriteLine($"No se pudo agregar item al inventario: {furniItem.Id}");
                    }
                }
            }
        }

        private void DeliverEffect(GameClient session, CatalogItem item)
        {
            //Console.WriteLine($"Entregando effect: {item.Data.SpriteId}");

            var effect = session.GetHabbo().Effects().GetEffectNullable(item.Data.SpriteId);

            if (effect != null)
            {
                effect.AddToQuantity();
                //Console.WriteLine($"Effect existente incrementado");
            }
            else
            {
                var newEffect = AvatarEffectFactory.CreateNullable(session.GetHabbo(), item.Data.SpriteId, 3600);
                if (newEffect != null)
                {
                    session.SendMessage(new AvatarEffectAddedComposer(item.Data.SpriteId, 3600));
                    //Console.WriteLine($"Nuevo effect creado");
                }
            }
        }

        private void DeliverBot(GameClient session, CatalogItem item)
        {
            //Console.WriteLine($"Entregando bot");

            var bot = BotUtility.CreateBot(item.Data, session.GetHabbo().Id);
            if (bot != null)
            {
                session.GetHabbo().GetInventoryComponent().TryAddBot(bot);
                session.SendMessage(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
                session.SendMessage(new FurniListNotificationComposer(bot.Id, 5));
                //Console.WriteLine($"Bot creado: {bot.Id}");
            }
        }

        private void DeliverBadge(GameClient session, CatalogItem item)
        {
            //Console.WriteLine($"Entregando badge: {item.Data.ItemName}");

            session.GetHabbo().GetBadgeComponent().GiveBadge(item.Data.ItemName, true, session);
            session.SendMessage(new FurniListNotificationComposer(0, 4));
        }

        private void DeliverPet(GameClient session, CatalogItem item, string extraData)
        {
            //Console.WriteLine($"Entregando pet, extraData: {extraData}");

            try
            {
                string[] petData = extraData.Split('\n');
                if (petData.Length >= 3)
                {
                    string petName = petData[0];
                    string race = petData[1];
                    string color = petData[2];

                    //Console.WriteLine($"Pet data: Name='{petName}', Race='{race}', Color='{color}'");

                    if (int.TryParse(race, out int petType))
                    {
                        var pet = PetUtility.CreatePet(session.GetHabbo().Id, petName, petType, race, color);
                        if (pet != null)
                        {
                            session.GetHabbo().GetInventoryComponent().TryAddPet(pet);
                            session.SendMessage(new FurniListNotificationComposer(0, 3));
                            session.SendMessage(new PetInventoryComposer(session.GetHabbo().GetInventoryComponent().GetPets()));
                            //Console.WriteLine($"Pet creado: {pet.PetId}, Type: {pet.Type}");
                        }
                    }
                }
                else
                {
                    //Console.WriteLine($"Pet data inválida, longitud: {petData.Length}");
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error al crear mascota: {ex.Message}");
            }
        }

        private void SendPurchaseNotification(GameClient session, CatalogItem item, int amount)
        {
            string itemName = item.Data.ItemName.Replace("*", "_");
            string displayName = string.IsNullOrEmpty(item.Data.PublicName)
                ? item.Data.ItemName
                : item.Data.PublicName;

            string message = amount > 1
                ? $"Acabas de comprar unos/unas {displayName}"
                : $"Acabas de comprar un/una {displayName}";

            //Console.WriteLine($"Enviando notificación: {message}");

            session.SendMessage(new RoomBubbleNotificationComposer($"icon/{itemName}_icon", message));
        }
    }
}