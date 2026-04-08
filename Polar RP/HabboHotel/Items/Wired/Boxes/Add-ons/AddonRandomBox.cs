using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Polar.Communication.Packets.Incoming;
using Polar.HabboHotel.Rooms;

namespace Polar.HabboHotel.Items.Wired.Boxes.Add_ons
{
    class AddonRandomBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.AddonRandom;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }


        public AddonRandomBox(Room instance, Item item)
        {
            this.Instance = instance;
            this.Item = item;
            this.SetItems = new();
            this.StringData = "";
        }

        public void HandleSave(ClientPacket Packet)
        {
            int unknown = Packet.PopInt();
            int chance = Packet.PopInt();
            this.StringData = chance.ToString();
        }

        public bool Execute(params object[] @params)
        {
            if (string.IsNullOrEmpty(StringData)) return true;
            if (int.TryParse(StringData, out int chance))
            {
                return Random.Shared.Next(1, 101) <= chance;
            }
            return true;
        }
    }
}