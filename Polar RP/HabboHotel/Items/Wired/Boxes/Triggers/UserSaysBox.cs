using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Polar.Communication.Packets.Incoming;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Users;
using Polar.Communication.Packets.Outgoing.Rooms.Chat;

namespace Polar.HabboHotel.Items.Wired.Boxes.Triggers
{
    class UserSaysBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type { get { return WiredBoxType.TriggerUserSays; } }
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public UserSaysBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            this.StringData = "";
            this.SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            int Unknown = Packet.PopInt();
            int OwnerOnly = Packet.PopInt();
            string Message = Packet.PopString();

            this.BoolData = OwnerOnly == 1;
            this.StringData = Message;
        }

        public bool Execute(params object[] Params)
        {
            Habbo Player = (Habbo)Params[0];
            if (Player == null || Player.CurrentRoom == null || !Player.InRoom)
                return false;

            RoomUser User = Player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Player.Id);
            if (User == null)
                return false;

            string Message = Convert.ToString(Params[1]);

            // FIX: Eliminado "|| Player == null" redundante (Player ya fue validado arriba)
            if ((BoolData && Instance.OwnerId != Player.Id) || string.IsNullOrWhiteSpace(Message) || string.IsNullOrWhiteSpace(this.StringData))
                return false;

            if (Message.Contains(" " + this.StringData) || Message.Contains(this.StringData + " ") || Message == this.StringData)
            {
                Player.WiredInteraction = true;
                ICollection<IWiredItem> Effects = Instance.GetWired().GetEffects(this);
                ICollection<IWiredItem> Conditions = Instance.GetWired().GetConditions(this);

                foreach (IWiredItem Condition in Conditions.ToList())
                {
                    if (!Condition.Execute(Player))
                        return false;

                    Instance.GetWired().OnEvent(Condition.Item);
                }

                Player.GetClient().SendMessage(new WhisperComposer(User.VirtualId, Message, 0, 0));

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
                    foreach (IWiredItem Effect in Effects.ToList())
                    {
                        if (!Effect.Execute(Player))
                            return false;

                        Instance.GetWired().OnEvent(Effect.Item);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
