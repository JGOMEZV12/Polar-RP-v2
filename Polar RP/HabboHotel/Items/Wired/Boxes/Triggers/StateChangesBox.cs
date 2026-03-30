using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Polar.Communication.Packets.Incoming;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Users;

namespace Polar.HabboHotel.Items.Wired.Boxes.Triggers
{
    class StateChangesBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type { get { return WiredBoxType.TriggerStateChanges; } }
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public StateChangesBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            this.SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            int Unknown = Packet.PopInt();
            string Unknown2 = Packet.PopString();

            if (this.SetItems.Count > 0)
                this.SetItems.Clear();

            int FurniCount = Packet.PopInt();
            for (int i = 0; i < FurniCount; i++)
            {
                Item SelectedItem = Instance.GetRoomItemHandler().GetItem(Packet.PopInt());
                if (SelectedItem != null)
                    SetItems.TryAdd(SelectedItem.Id, SelectedItem);
            }
        }

        public bool Execute(params object[] Params)
        {
            Habbo Player = (Habbo)Params[0];
            if (Player == null)
                return false;

            Item Item = (Item)Params[1];
            if (Item == null)
                return false;

            if (!this.SetItems.ContainsKey(Item.Id))
                return false;

            ICollection<IWiredItem> Effects = Instance.GetWired().GetEffects(this);
            ICollection<IWiredItem> Conditions = Instance.GetWired().GetConditions(this);

            foreach (IWiredItem Condition in Conditions)
            {
                if (!Condition.Execute(Player))
                    return false;

                if (Instance != null)
                    Instance.GetWired().OnEvent(Condition.Item);
            }

            // FIX: Any() en lugar de .Where().ToList().Count() > 0
            bool HasRandomEffectAddon = Effects.Any(x => x.Type == WiredBoxType.AddonRandomEffect);
            if (HasRandomEffectAddon)
            {
                // FIX: null-check en RandomBox antes de ejecutar
                IWiredItem RandomBox = Effects.FirstOrDefault(x => x.Type == WiredBoxType.AddonRandomEffect);
                if (RandomBox == null || !RandomBox.Execute())
                    return false;

                IWiredItem SelectedBox = Instance.GetWired().GetRandomEffect(Effects.ToList());
                if (SelectedBox == null || !SelectedBox.Execute())
                    return false;

                if (Instance != null)
                {
                    Instance.GetWired().OnEvent(RandomBox.Item);
                    Instance.GetWired().OnEvent(SelectedBox.Item);
                }
            }
            else
            {
                foreach (IWiredItem Effect in Effects)
                {
                    if (!Effect.Execute(Player))
                        return false;

                    if (Instance != null)
                        Instance.GetWired().OnEvent(Effect.Item);
                }
            }

            return true;
        }
    }
}
