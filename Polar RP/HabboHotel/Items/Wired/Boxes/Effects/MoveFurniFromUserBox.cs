using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Polar.Communication.Packets.Incoming;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Users;
using System.Drawing;
using Polar.Communication.Packets.Outgoing.Rooms.Engine;
using Polar.Utilities;

namespace Polar.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class MoveFurniFromUserBox : IWiredItem, IWiredCycle
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectMoveFurniFromNearestUser;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }

        private int _delay = 0;
        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value + 1;
            }
        }

        public int TickCount { get; set; }
        public string ItemsData { get; set; }
        private bool Requested;
        private long _next = 0;

        public MoveFurniFromUserBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new();
            TickCount = Delay;
            Requested = false;
        }

        public void HandleSave(ClientPacket packet)
        {
            // Variables desconocidas que no se usan, se podría revisar si son necesarias
            int unknown = packet.PopInt();
            string unknown2 = packet.PopString();

            // Limpiar los elementos previos
            SetItems.Clear();

            int furniCount = packet.PopInt();
            for (int i = 0; i < furniCount; i++)
            {
                Item selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
                if (selectedItem != null)
                {
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
                }
            }

            int delay = packet.PopInt();
            Delay = delay;
        }

        public bool Execute(params object[] Params)
        {
            if (SetItems.Count == 0)
                return false;

            // Verificar si el ciclo debe ejecutarse
            if (_next == 0 || _next < DateTime.UtcNow.Ticks)
                _next = DateTime.UtcNow.Ticks + Delay;

            if (!Requested)
            {
                TickCount = Delay;
                Requested = true;
            }

            return true;
        }

        public bool OnCycle()
        {
            if (Instance == null || !Requested || _next == 0)
                return false;

            var now = DateTime.UtcNow.Ticks;
            if (_next < now)
            {
                // Recorrer los elementos a mover
                foreach (Item item in SetItems.Values.ToList())
                {
                    if (item == null)
                        continue;

                    // Comprobar si el item aún está en el suelo
                    if (!Instance.GetRoomItemHandler().GetFloor.Contains(item))
                        continue;

                    // Si el elemento ya no debe estar en la lista, eliminarlo
                    if (Instance.GetWired().OtherBoxHasItem(this, item.Id))
                    {
                        SetItems.TryRemove(item.Id, out _);
                    }

                    Point point = Instance.GetGameMap().GetChaseMovement(item);
                    Instance.GetWired().OnUserFurniCollision(Instance, item);

                    // Verificar si el ítem puede moverse a la nueva ubicación
                    if (!Instance.GetGameMap().ItemCanMove(item, point))
                        continue;

                    // Verificar si el ítem puede colocarse en el punto
                    if (Instance.GetGameMap().CanRollItemHere(point.X, point.Y) && !Instance.GetGameMap().SquareHasUsers(point.X, point.Y))
                    {
                        double newZ = item.GetZ;
                        bool canBePlaced = true;

                        // Verificar otros items en las coordenadas
                        List<Item> items = Instance.GetGameMap().GetCoordinatedItems(point);
                        foreach (Item iItem in items.ToList())
                        {
                            if (iItem == null || iItem.Id == item.Id)
                                continue;

                            if (!iItem.GetBaseItem().Walkable)
                            {
                                _next = 0; // Resetear el tiempo de espera si no se puede colocar
                                canBePlaced = false;
                                break;
                            }

                            if (iItem.TotalHeight > newZ)
                                newZ = iItem.TotalHeight;

                            if (canBePlaced && !iItem.GetBaseItem().Stackable)
                                canBePlaced = false;
                        }

                        // Colocar el ítem si es posible
                        if (canBePlaced && point != item.Coordinate)
                        {
                            Instance.SendMessage(new SlideObjectBundleComposer(item.GetX, item.GetY, item.GetZ, point.X,
                                point.Y, newZ, 0, 0, item.Id));
                            Instance.GetRoomItemHandler().SetFloorItem(item, point.X, point.Y, newZ);
                        }
                    }
                }

                _next = 0;
                return true;
            }

            return false;
        }
    }
}
