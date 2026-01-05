using Polar.Core;
using Polar.HabboHotel.Catalog;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Users;
using Polar.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Polar.Communication.Packets.Outgoing.Catalog
{
    public class CatalogIndexComposer : ServerPacket
    {
        private readonly GameClient _session;
        private readonly HashSet<int> _processedPages;

        public CatalogIndexComposer(GameClient session, ICollection<CatalogPage> pages)
            : base(ServerPacketHeader.CatalogIndexMessageComposer)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _processedPages = new HashSet<int>();

            WriteCatalogIndex(pages);
        }

        private void WriteCatalogIndex(ICollection<CatalogPage> pages)
        {
            // Encabezado del catálogo
            WriteBoolean(true); // Catálogo activado

            // Créditos del usuario
            var habbo = _session.GetHabbo();
            WriteInteger(habbo?.Credits ?? 0);

            // Puntos seasonal (ajustar según tu sistema)
            WriteInteger(GetSeasonalPoints());

            // Nodo raíz
            WriteString("root");
            WriteString(string.Empty);
            WriteInteger(0); // Nivel mínimo requerido

            // Páginas principales
            WriteInteger(pages?.Count ?? 0);

            if (pages != null)
            {
                foreach (var page in pages)
                {
                    if (page != null)
                    {
                        AppendPage(page);
                    }
                }
            }

            // Footer
            WriteBoolean(HasNewItems());
            WriteString("NORMAL"); // Modo del catálogo
        }

        private void AppendPage(CatalogPage page)
        {
            if (page == null) return;

            // Prevenir ciclos infinitos
            if (_processedPages.Contains(page.Id))
            {
                Logging.WriteLine($"Warning: Circular reference detected in catalog page {page.Id}", ConsoleColor.Yellow);
                return;
            }

            _processedPages.Add(page.Id);

            // Obtener subpáginas
            ICollection<CatalogPage> subPages = null;
            try
            {
                subPages = PolarEnvironment.GetGame().GetCatalog().GetPages(_session, page.Id);
            }
            catch (Exception ex)
            {
                Logging.LogException("CatalogIndexComposer.AppendPage: " + ex.ToString());
            }

            // Datos de la página
            WriteBoolean(page.Visible);
            WriteInteger(page.Icon);
            WriteInteger(page.Enabled ? page.Id : -1);
            WriteString(page.PageLink ?? string.Empty);
            WriteString(page.Caption ?? string.Empty);

            // Items ofrecidos - MANERA CORRECTA
            if (page.ItemOffers != null)
            {
                WriteInteger(page.ItemOffers.Count);
                foreach (var key in page.ItemOffers.Keys)
                {
                    WriteInteger(key);
                }
            }
            else
            {
                WriteInteger(0);
            }

            // Subpáginas
            WriteInteger(subPages?.Count ?? 0);

            if (subPages != null)
            {
                foreach (var subPage in subPages)
                {
                    if (subPage != null)
                    {
                        AppendPage(subPage);
                    }
                }
            }
        }

        private int GetSeasonalPoints()
        {
            // Implementa según tu sistema
            // Ejemplo:
            // var habbo = _session.GetHabbo();
            // return habbo?.SeasonalPoints ?? -1;

            return -1; // -1 = no mostrar
        }

        private bool HasNewItems()
        {
            // Lógica para determinar si hay items nuevos
            // Podrías verificar última visita del usuario vs fecha de items
            return false;
        }
    }
}