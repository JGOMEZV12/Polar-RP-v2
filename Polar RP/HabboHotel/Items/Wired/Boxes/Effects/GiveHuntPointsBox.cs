using System;
using System.Linq;
using System.Collections.Concurrent;
using Polar.Communication.Packets.Incoming;
using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Users;

namespace Polar.HabboHotel.Items.Wired.Boxes.Effects
{
    class GiveHuntPointsBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type { get { return WiredBoxType.EffectGiveHuntPoints; } }
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public GiveHuntPointsBox(Room instance, Item item)
        {
            this.Instance = instance;
            this.Item = item;
            this.SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            int Unknown = Packet.PopInt();
            string Amount = Packet.PopString();
            this.StringData = Amount;
        }

        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            Habbo Player = (Habbo)Params[0];
            if (Player == null || Player.GetClient() == null || Player.GetClient().GetRoleplay() == null)
                return false;

            if (string.IsNullOrEmpty(StringData))
                return false;

            if (!int.TryParse(StringData, out int points))
                return false;

            Player.GetClient().GetRoleplay().HuntPoints += points;
            Player.GetClient().SendWhisper("¡Has recibido " + points + " puntos de caza vía Wired!", 1);
            Player.GetClient().GetRoleplay().RefreshStatDialogue();

            return true;
        }
    }
}
