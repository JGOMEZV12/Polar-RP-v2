using System;
using System.Linq;
using System.Collections.Concurrent;
using Polar.Communication.Packets.Incoming;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Users;
using Polar.Communication.Packets.Outgoing.Rooms.Engine;

namespace Polar.HabboHotel.Items.Wired.Boxes.Effects
{
    class SetMottoBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type { get { return WiredBoxType.EffectSetMotto; } }
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public SetMottoBox(Room instance, Item item)
        {
            this.Instance = instance;
            this.Item = item;
            this.SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            int Unknown = Packet.PopInt();
            string Motto = Packet.PopString();
            this.StringData = Motto;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            Habbo Player = (Habbo)Params[0];
            if (Player == null || Player.GetClient() == null || Player.GetClient().GetRoomUser() == null)
                return false;

            if (string.IsNullOrEmpty(StringData))
                return false;

            Player.Motto = StringData;
            Instance.SendMessage(new UserChangeComposer(Player.GetClient().GetRoomUser(), false));

            return true;
        }
    }
}
