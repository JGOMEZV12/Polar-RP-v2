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
    class GameEndsBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type { get { return WiredBoxType.TriggerGameEnds; } }
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public GameEndsBox(Room Instance, Item Item)
        {
            this.Item = Item;
            this.Instance = Instance;
            this.SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
        }

        public bool Execute(params object[] Params)
        {
            ICollection<IWiredItem> Effects = Instance.GetWired().GetEffects(this);
            ICollection<IWiredItem> Conditions = Instance.GetWired().GetConditions(this);

            foreach (IWiredItem Condition in Conditions)
            {
                if (!Condition.Execute(Condition.Item))
                    return false;

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
                    foreach (RoomUser User in Instance.GetRoomUserManager().GetRoomUsers().ToList())
                    {
                        if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                            continue;

                        Effect.Execute(User.GetClient().GetHabbo());
                    }

                    Instance.GetWired().OnEvent(Effect.Item);
                }
            }

            return true;
        }
    }
}
