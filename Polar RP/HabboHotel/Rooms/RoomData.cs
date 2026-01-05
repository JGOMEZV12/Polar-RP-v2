using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Polar.HabboHotel.Groups;
using Polar.Database.Interfaces;
using System.Security.Cryptography;

namespace Polar.HabboHotel.Rooms
{
    public class RoomData
    {
        public int Id;
        public bool AllowPets;
        public bool AllowPetsEating;
        public bool RoomBlockingEnabled;
        public int Category;
        public string Description;
        public string Floor;
        public int FloorThickness;
        public Group Group;
        public bool Hidewall;
        public string Landscape;
        public string ModelName;
        public string Name;
        public string OwnerName;
        public int OwnerId;
        public string Password;
        public int Score;
        public int State;
        public List<string> Tags;
        public string Type;
        public int UsersMax;
        public int UsersNow;
        public int WallThickness;
        public string Wallpaper;
        public int WhoCanBan;
        public int WhoCanKick;
        public int WhoCanMute;
        private RoomModel mModel;
        public int chatMode;
        public int chatSpeed;
        public int chatSize;
        public int extraFlood;
        public int chatDistance;

        public int TradeSettings;//Default = 2;

        public RoomPromotion _promotion;

        public bool PushEnabled;
        public bool PullEnabled;
        public bool SPushEnabled;
        public bool SPullEnabled;
        public bool EnablesEnabled;
        public bool RespectNotificationsEnabled;
        public bool PetMorphsAllowed;
        public bool HideWired;

        public string City;
        public int DoorOrientation;
        public int DoorX;
        public int DoorY;
        public double DoorZ;
        public bool BankEnabled;
        /* Old public int BankBalance; */
        public bool ShootEnabled;
        public bool HitEnabled;
        public bool SafeZoneEnabled;
        public bool SexCommandsEnabled;
        public bool TurfEnabled;
        public bool TurfCapturing = false;
        public bool BankCapturing = false;
        public int TurfUserAtackerId = 0;
        public bool RobEnabled;
        public bool GymEnabled;
        public bool DeliveryEnabled;
        public bool TutorialEnabled;
        public bool DriveEnabled;
        public bool TaxiToEnabled;
        public bool BusToEnabled;
        public bool TaxiFromEnabled;
        public string EnterRoomMessage;
        public bool MallEnabled;
        public bool SupermarketEnabled;
        public bool BuyCarEnabled;
        public bool IsHospital;
        public bool IsPrison;
        public bool IsPrison2;
        public bool IsPolStation;
        public bool IsCamionero;
        public bool IsCourt;
        public bool IsMecanico;
        public bool IsBasurero;
        public bool WardrobeEnabled;
        public bool PhoneStoreEnabled;
        public int GroupId;

        public Dictionary<int, KeyValuePair<int, string>> WiredScoreBordDay;
        public Dictionary<int, KeyValuePair<int, string>> WiredCasinoApuestas;
        public Dictionary<int, KeyValuePair<int, string>> WiredScoreBordWeek;
        public Dictionary<int, KeyValuePair<int, string>> WiredScoreBordMonth;
        public List<int> WiredScoreFirstBordInformation = new List<int>();

        public void Fill(DataRow Row)
        {
            Fill(Row, string.Empty);
        }

        public void Fill(DataRow Row, string ownerName)
        {
            Id = Convert.ToInt32(Row["id"]);
            Name = Convert.ToString(Row["caption"]);
            Description = Convert.ToString(Row["description"]);
            Type = Convert.ToString(Row["roomtype"]);
            OwnerId = Convert.ToInt32(Row["owner"]);

            if (string.IsNullOrEmpty(ownerName))
            {
                using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `username` FROM `users` WHERE `id` = @owner LIMIT 1");
                    dbClient.AddParameter("owner", OwnerId);
                    OwnerName = dbClient.getString();
                }
            }
            else
                OwnerName = ownerName;

            switch (Row["state"].ToString().ToLower())
            {
                case "open":
                    this.State = 0;
                    break;
                case "password":
                    this.State = 2;
                    break;
                case "hide":
                    this.State = 3;
                    break;
                default:
                    this.State = 1;
                    break;
            }

            Category = Convert.ToInt32(Row["category"]);
            if (!string.IsNullOrEmpty(Row["users_now"].ToString()))
                UsersNow = Convert.ToInt32(Row["users_now"]);
            else
                UsersNow = 0;
            UsersMax = Convert.ToInt32(Row["users_max"]);
            ModelName = Convert.ToString(Row["model_name"]);
            Score = Convert.ToInt32(Row["score"]);
            Tags = new List<string>();
            AllowPets = PolarEnvironment.EnumToBool(Row["allow_pets"].ToString());
            AllowPetsEating = PolarEnvironment.EnumToBool(Row["allow_pets_eat"].ToString());
            RoomBlockingEnabled = PolarEnvironment.EnumToBool(Row["room_blocking_disabled"].ToString());
            Hidewall = PolarEnvironment.EnumToBool(Row["allow_hidewall"].ToString());
            Password = Convert.ToString(Row["password"]);
            Wallpaper = Convert.ToString(Row["wallpaper"]);
            Floor = Convert.ToString(Row["floor"]);
            Landscape = Convert.ToString(Row["landscape"]);
            FloorThickness = Convert.ToInt32(Row["floorthick"]);
            WallThickness = Convert.ToInt32(Row["wallthick"]);
            WhoCanMute = Convert.ToInt32(Row["mute_settings"]);
            WhoCanKick = Convert.ToInt32(Row["kick_settings"]);
            WhoCanBan = Convert.ToInt32(Row["ban_settings"]);
            chatMode = Convert.ToInt32(Row["chat_mode"]);
            chatSpeed = Convert.ToInt32(Row["chat_speed"]);
            chatSize = Convert.ToInt32(Row["chat_size"]);
            TradeSettings = Convert.ToInt32(Row["trade_settings"]);
            GroupId = Convert.ToInt32(Row["group_id"]);
            Group G = null;

            if (GroupId < 1000)
                G = GroupManager.GetJob(GroupId);
            else
                G = GroupManager.GetGang(GroupId);

            if (G != null)
                Group = G;
            else
                Group = null;

            foreach (string Tag in Row["tags"].ToString().Split(','))
            {
                Tags.Add(Tag);
            }

            mModel = PolarEnvironment.GetGame().GetRoomManager().GetModel(ModelName, this.Id);

            this.PushEnabled = PolarEnvironment.EnumToBool(Row["push_enabled"].ToString());
            this.PullEnabled = PolarEnvironment.EnumToBool(Row["pull_enabled"].ToString());
            this.SPushEnabled = PolarEnvironment.EnumToBool(Row["spush_enabled"].ToString());
            this.SPullEnabled = PolarEnvironment.EnumToBool(Row["spull_enabled"].ToString());
            this.EnablesEnabled = PolarEnvironment.EnumToBool(Row["enables_enabled"].ToString());
            this.RespectNotificationsEnabled = PolarEnvironment.EnumToBool(Row["respect_notifications_enabled"].ToString());
            this.PetMorphsAllowed = PolarEnvironment.EnumToBool(Row["pet_morphs_allowed"].ToString());
            this.HideWired = PolarEnvironment.EnumToBool(Row["hide_wired"].ToString());

            WiredScoreBordDay = new Dictionary<int, KeyValuePair<int, string>>();
            WiredScoreBordWeek = new Dictionary<int, KeyValuePair<int, string>>();
            WiredScoreBordMonth = new Dictionary<int, KeyValuePair<int, string>>();

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                List<bool> SuperCheck = new List<bool>()
                {
                    false,
                    false,
                    false
                };

                DateTime now = DateTime.Now;
                int getdaytoday = Convert.ToInt32(now.ToString("MMddyyyy"));
                int getmonthtoday = Convert.ToInt32(now.ToString("MM"));
                int getweektoday = CultureInfo.GetCultureInfo("Nl-nl").Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                this.WiredScoreFirstBordInformation = new List<int>()
                {
                    getdaytoday,
                    getmonthtoday,
                    getweektoday
                };

                dbClient.SetQuery("SELECT * FROM wired_scorebord WHERE roomid = @id ORDER BY `punten` DESC ");
                dbClient.AddParameter("id", this.Id);
                foreach (DataRow row in dbClient.getTable().Rows)
                {
                    int userid = Convert.ToInt32(row["userid"]);
                    string username = Convert.ToString(row["username"]);
                    int Punten = Convert.ToInt32(row["punten"]);
                    string soort = Convert.ToString(row["soort"]);
                    int timestamp = Convert.ToInt32(row["timestamp"]);
                    if ((!(soort == "day") || this.WiredScoreBordDay.ContainsKey(userid) ? false : !SuperCheck[0]))
                    {
                        if (timestamp != getdaytoday)
                        {
                            SuperCheck[0] = false;
                        }
                        if (!SuperCheck[0])
                        {
                            this.WiredScoreBordDay.Add(userid, new KeyValuePair<int, string>(Punten, username));
                        }
                    }
                    if ((!(soort == "month") || this.WiredScoreBordMonth.ContainsKey(userid) ? false : !SuperCheck[1]))
                    {
                        if (timestamp != getmonthtoday)
                        {
                            SuperCheck[1] = false;
                        }
                        this.WiredScoreBordMonth.Add(userid, new KeyValuePair<int, string>(Punten, username));
                    }
                    if ((!(soort == "week") || this.WiredScoreBordWeek.ContainsKey(userid) ? false : !SuperCheck[2]))
                    {
                        if (timestamp != getweektoday)
                        {
                            SuperCheck[2] = false;
                        }
                        this.WiredScoreBordWeek.Add(userid, new KeyValuePair<int, string>(Punten, username));
                    }
                }
                if (SuperCheck[0])
                {
                    dbClient.RunQuery(string.Concat("DELETE FROM `wired_scorebord` WHERE `roomid`='", this.Id, "' AND `soort`='day'"));
                    this.WiredScoreBordDay.Clear();
                }
                if (SuperCheck[1])
                {
                    dbClient.RunQuery(string.Concat("DELETE FROM `wired_scorebord` WHERE `roomid`='", this.Id, "' AND `soort`='month'"));
                    this.WiredScoreBordMonth.Clear();
                }
                if (SuperCheck[2])
                {
                    dbClient.RunQuery(string.Concat("DELETE FROM `wired_scorebord` WHERE `roomid`='", this.Id, "' AND `soort`='week'"));
                    this.WiredScoreBordDay.Clear();
                }
            }
        }

        public void FillRP(DataRow Row)
        {
            this.City = Row["city"].ToString();
            this.BankEnabled = PolarEnvironment.EnumToBool(Row["bank_enabled"].ToString());
            /* Old this.BankBalance = Convert.ToInt32(Row["bank_balance"]);*/
            this.ShootEnabled = PolarEnvironment.EnumToBool(Row["shoot_enabled"].ToString());
            this.HitEnabled = PolarEnvironment.EnumToBool(Row["hit_enabled"].ToString());
            this.SafeZoneEnabled = PolarEnvironment.EnumToBool(Row["safezone_enabled"].ToString());
            this.SexCommandsEnabled = PolarEnvironment.EnumToBool(Row["sexcommands_enabled"].ToString());
            this.TurfEnabled = PolarEnvironment.EnumToBool(Row["turf_enabled"].ToString());
            this.RobEnabled = PolarEnvironment.EnumToBool(Row["rob_enabled"].ToString());
            this.GymEnabled = PolarEnvironment.EnumToBool(Row["gym_enabled"].ToString());
            this.DeliveryEnabled = PolarEnvironment.EnumToBool(Row["delivery_enabled"].ToString());
            this.TutorialEnabled = PolarEnvironment.EnumToBool(Row["tutorial_enabled"].ToString());
            this.DriveEnabled = PolarEnvironment.EnumToBool(Row["drive_enabled"].ToString());
            this.TaxiFromEnabled = PolarEnvironment.EnumToBool(Row["taxi_from_enabled"].ToString());
            this.TaxiToEnabled = PolarEnvironment.EnumToBool(Row["taxi_to_enabled"].ToString());
            this.BusToEnabled = PolarEnvironment.EnumToBool(Row["bus_to_enabled"].ToString());
            this.EnterRoomMessage = Row["enter_message"].ToString();
            this.IsHospital = PolarEnvironment.EnumToBool(Row["is_hospital"].ToString());
            this.IsPrison = PolarEnvironment.EnumToBool(Row["is_prison"].ToString());
            this.IsPrison2 = PolarEnvironment.EnumToBool(Row["is_prisonback"].ToString());
            this.IsCourt = PolarEnvironment.EnumToBool(Row["is_court"].ToString());
            this.IsCamionero = PolarEnvironment.EnumToBool(Row["is_camionero"].ToString());
            this.IsBasurero = PolarEnvironment.EnumToBool(Row["is_basurero"].ToString());
            this.MallEnabled = PolarEnvironment.EnumToBool(Row["mall_enabled"].ToString());
            this.BuyCarEnabled = PolarEnvironment.EnumToBool(Row["buycar_enabled"].ToString());
            this.WardrobeEnabled = PolarEnvironment.EnumToBool(Row["wardrobe_enabled"].ToString());
            this.PhoneStoreEnabled = PolarEnvironment.EnumToBool(Row["phonestore_enabled"].ToString());
            this.SupermarketEnabled = PolarEnvironment.EnumToBool(Row["supermarket_enabled"].ToString());
        }

        public RoomPromotion Promotion
        {
            get { return this._promotion; }
            set { this._promotion = value; }
        }

        public bool HasActivePromotion
        {
            get { return this.Promotion != null; }
        }

        public void EndPromotion()
        {
            if (!this.HasActivePromotion)
                return;

            this.Promotion = null;
        }

        public RoomModel Model
        {
            get
            {
                if (mModel == null)
                    mModel = PolarEnvironment.GetGame().GetRoomManager().GetModel(ModelName, Id);
                return mModel;
            }
        }
    }
}