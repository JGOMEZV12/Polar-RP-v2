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
    class AddonUnseenBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.AddonUnseen;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }
        private int _lastIndex = -1;

        public AddonUnseenBox(Room instance, Item item)
        {
            this.Instance = instance;
            this.Item = item;
            this.SetItems = new();
            this.StringData = "";
        }

        public void HandleSave(ClientPacket Packet)
        {

        }

        public bool Execute(params object[] @params)
        {
            if (@params.Length == 0) return true;
            var effects = (List<IWiredItem>)@params[0];
            if (effects.Count == 0) return true;
            int nextIndex = _lastIndex;
            while (nextIndex == _lastIndex && effects.Count > 1)
                nextIndex = Random.Shared.Next(effects.Count);
            if (nextIndex == -1) nextIndex = 0;
            _lastIndex = nextIndex;
            return effects[nextIndex].Execute();
        }
    }
}