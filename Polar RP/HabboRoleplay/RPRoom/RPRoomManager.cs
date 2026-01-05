using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using log4net;
using Polar.Database.Interfaces;
using System.Data;
using Polar.HabboHotel.Users;
using MoreLinq;

namespace Polar.HabboRoleplay.RPRoom
{
    public class RPRoomManager
    {
        public readonly ILog log = LogManager.GetLogger("Polar.HabboRoleplay.RPRoom");
        public readonly Dictionary<string, RPRoom> HospitalRooms = new Dictionary<string, RPRoom>();
        public readonly Dictionary<string, RPRoom> JailRooms = new Dictionary<string, RPRoom>();
        public readonly Dictionary<string, RPRoom> CourtRooms = new Dictionary<string, RPRoom>();
        public readonly Dictionary<string, RPRoom> JailBack = new Dictionary<string, RPRoom>();

        // Jobs Zones
        public readonly Dictionary<string, RPRoom> PolStationRooms = new Dictionary<string, RPRoom>();
        public readonly Dictionary<string, RPRoom> CamionerosRooms = new Dictionary<string, RPRoom>();
        public readonly Dictionary<string, RPRoom> BasurerosRooms = new Dictionary<string, RPRoom>();
        public readonly Dictionary<string, RPRoom> ArmerosRooms = new Dictionary<string, RPRoom>();

        public RPRoomManager()
        {

        }
        public void Init()
        {
            this.HospitalRooms.Clear();
            this.JailRooms.Clear();
            this.JailBack.Clear();
            this.PolStationRooms.Clear();
            this.CamionerosRooms.Clear();
            this.BasurerosRooms.Clear();
            this.ArmerosRooms.Clear();

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `is_hospital` = '1'");
                DataTable GetRPRooms = dbClient.getTable();

                if (GetRPRooms != null)
                {
                    foreach (DataRow Row in GetRPRooms.Rows)//ROOm.IsHospital;
                    {
                        this.HospitalRooms.Add(Convert.ToString(Row["city"]), new RPRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PolarEnvironment.EnumToBool(Row["is_court"].ToString()), PolarEnvironment.EnumToBool(Row["is_hospital"].ToString()), PolarEnvironment.EnumToBool(Row["is_prison"].ToString()), PolarEnvironment.EnumToBool(Row["is_prisonback"].ToString()), PolarEnvironment.EnumToBool(Row["is_camionero"].ToString()), PolarEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PolarEnvironment.EnumToBool(Row["is_basurero"].ToString()), PolarEnvironment.EnumToBool(Row["is_armero"].ToString()), PolarEnvironment.EnumToBool(Row["is_polstation"].ToString())));
                    }
                }
            }

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `is_prison` = '1'");
                DataTable GetRPRooms = dbClient.getTable();

                if (GetRPRooms != null)
                {
                    foreach (DataRow Row in GetRPRooms.Rows)//ROOm.IsHospital;
                    {
                        this.JailRooms.Add(Convert.ToString(Row["city"]), new RPRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PolarEnvironment.EnumToBool(Row["is_court"].ToString()), PolarEnvironment.EnumToBool(Row["is_hospital"].ToString()), PolarEnvironment.EnumToBool(Row["is_prison"].ToString()), PolarEnvironment.EnumToBool(Row["is_prisonback"].ToString()), PolarEnvironment.EnumToBool(Row["is_camionero"].ToString()), PolarEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PolarEnvironment.EnumToBool(Row["is_basurero"].ToString()), PolarEnvironment.EnumToBool(Row["is_armero"].ToString()), PolarEnvironment.EnumToBool(Row["is_polstation"].ToString())));
                    }
                }
            }

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `is_prisonback` = '1'");
                DataTable GetRPRooms = dbClient.getTable();

                if (GetRPRooms != null)
                {
                    foreach (DataRow Row in GetRPRooms.Rows)//ROOm.IsHospital;
                    {
                        this.JailBack.Add(Convert.ToString(Row["city"]), new RPRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PolarEnvironment.EnumToBool(Row["is_court"].ToString()), PolarEnvironment.EnumToBool(Row["is_hospital"].ToString()), PolarEnvironment.EnumToBool(Row["is_prison"].ToString()), PolarEnvironment.EnumToBool(Row["is_prisonback"].ToString()), PolarEnvironment.EnumToBool(Row["is_camionero"].ToString()), PolarEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PolarEnvironment.EnumToBool(Row["is_basurero"].ToString()), PolarEnvironment.EnumToBool(Row["is_armero"].ToString()), PolarEnvironment.EnumToBool(Row["is_polstation"].ToString())));
                    }
                }
            }

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `is_court` = '1'");
                DataTable GetRPRooms = dbClient.getTable();

                if (GetRPRooms != null)
                {
                    foreach (DataRow Row in GetRPRooms.Rows)//ROOm.IsHospital;
                    {
                        this.CourtRooms.Add(Convert.ToString(Row["city"]), new RPRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PolarEnvironment.EnumToBool(Row["is_court"].ToString()), PolarEnvironment.EnumToBool(Row["is_hospital"].ToString()), PolarEnvironment.EnumToBool(Row["is_prison"].ToString()), PolarEnvironment.EnumToBool(Row["is_prisonback"].ToString()), PolarEnvironment.EnumToBool(Row["is_camionero"].ToString()), PolarEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PolarEnvironment.EnumToBool(Row["is_basurero"].ToString()), PolarEnvironment.EnumToBool(Row["is_armero"].ToString()), PolarEnvironment.EnumToBool(Row["is_polstation"].ToString())));
                    }
                }
            }


            #region Jobs Zones
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `is_camionero` = '1'");
                DataTable GetRPRooms = dbClient.getTable();

                if (GetRPRooms != null)
                {
                    foreach (DataRow Row in GetRPRooms.Rows)//ROOm.IsHospital;
                    {
                        this.CamionerosRooms.Add(Convert.ToString(Row["city"]), new RPRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PolarEnvironment.EnumToBool(Row["is_court"].ToString()), PolarEnvironment.EnumToBool(Row["is_hospital"].ToString()), PolarEnvironment.EnumToBool(Row["is_prison"].ToString()), PolarEnvironment.EnumToBool(Row["is_prisonback"].ToString()), PolarEnvironment.EnumToBool(Row["is_camionero"].ToString()), PolarEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PolarEnvironment.EnumToBool(Row["is_basurero"].ToString()), PolarEnvironment.EnumToBool(Row["is_armero"].ToString()), PolarEnvironment.EnumToBool(Row["is_polstation"].ToString())));
                    }
                }
            }

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `is_basurero` = '1'");
                DataTable GetRPRooms = dbClient.getTable();

                if (GetRPRooms != null)
                {
                    foreach (DataRow Row in GetRPRooms.Rows)//ROOm.IsHospital;
                    {
                        this.BasurerosRooms.Add(Convert.ToString(Row["city"]), new RPRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PolarEnvironment.EnumToBool(Row["is_court"].ToString()), PolarEnvironment.EnumToBool(Row["is_hospital"].ToString()), PolarEnvironment.EnumToBool(Row["is_prison"].ToString()), PolarEnvironment.EnumToBool(Row["is_prisonback"].ToString()), PolarEnvironment.EnumToBool(Row["is_camionero"].ToString()), PolarEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PolarEnvironment.EnumToBool(Row["is_basurero"].ToString()), PolarEnvironment.EnumToBool(Row["is_armero"].ToString()), PolarEnvironment.EnumToBool(Row["is_polstation"].ToString())));
                    }
                }
            }

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `is_polstation` = '1'");
                DataTable GetRPRooms = dbClient.getTable();

                if (GetRPRooms != null)
                {
                    foreach (DataRow Row in GetRPRooms.Rows)//ROOm.IsHospital;
                    {
                        this.PolStationRooms.Add(Convert.ToString(Row["city"]), new RPRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PolarEnvironment.EnumToBool(Row["is_court"].ToString()), PolarEnvironment.EnumToBool(Row["is_hospital"].ToString()), PolarEnvironment.EnumToBool(Row["is_prison"].ToString()), PolarEnvironment.EnumToBool(Row["is_prisonback"].ToString()), PolarEnvironment.EnumToBool(Row["is_camionero"].ToString()), PolarEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PolarEnvironment.EnumToBool(Row["is_basurero"].ToString()), PolarEnvironment.EnumToBool(Row["is_armero"].ToString()), PolarEnvironment.EnumToBool(Row["is_polstation"].ToString())));
                    }
                }
            }

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `rp_rooms` WHERE `is_armero` = '1'");
                DataTable GetPlayRooms = dbClient.getTable();

                if (GetPlayRooms != null)
                {
                    foreach (DataRow Row in GetPlayRooms.Rows)//ROOm.IsHospital;
                    {
                        this.ArmerosRooms.Add(Convert.ToString(Row["city"]), new RPRoom(Convert.ToInt32(Row["id"]), Convert.ToString(Row["city"]), PolarEnvironment.EnumToBool(Row["is_court"].ToString()), PolarEnvironment.EnumToBool(Row["is_hospital"].ToString()), PolarEnvironment.EnumToBool(Row["is_prison"].ToString()), PolarEnvironment.EnumToBool(Row["is_prisonback"].ToString()), PolarEnvironment.EnumToBool(Row["is_camionero"].ToString()), PolarEnvironment.EnumToBool(Row["is_mecanico"].ToString()), PolarEnvironment.EnumToBool(Row["is_basurero"].ToString()), PolarEnvironment.EnumToBool(Row["is_armero"].ToString()), PolarEnvironment.EnumToBool(Row["is_polstation"].ToString())));
                    }
                }
            }
            #endregion

            Out.WriteLine(this.HospitalRooms.Count + " Hospital(es) -> CARGADO", "Polar.HabboRoleplay.RPRoom", ConsoleColor.DarkGray);
            Out.WriteLine(this.JailRooms.Count + " Prision(es) -> CARGADO", "Polar.HabboRoleplay.RPRoom", ConsoleColor.DarkGray);
            Out.WriteLine(this.JailBack.Count + " Prisonback(es) -> CARGADO", "Polar.HabboRoleplay.RPRoom", ConsoleColor.DarkGray);
            Out.WriteLine(this.PolStationRooms.Count + " Police Station(es) -> CARGADO", "Polar.HabboRoleplay.RPRoom", ConsoleColor.DarkGray);
            Out.WriteLine(this.CamionerosRooms.Count + " Camioneros Zone(es) -> CARGADO", "Polar.HabboRoleplay.RPRoom", ConsoleColor.DarkGray);
            Out.WriteLine(this.BasurerosRooms.Count + " Basureros Zone(es) -> CARGADO", "Polar.HabboRoleplay.RPRoom", ConsoleColor.DarkGray);



        }

        public int TryToGetCourt(string city, out RPRoom Room)
        {

            if (this.CourtRooms.TryGetValue(city, out Room))
            {
                return this.CourtRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetHospital(string city, out RPRoom Room)
        {
            
            if(this.HospitalRooms.TryGetValue(city, out Room))
            {
                return this.HospitalRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetJail(string city, out RPRoom Room)
        {

            if (this.JailRooms.TryGetValue(city, out Room))
            {
                return this.JailRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetJailBack(string city, out RPRoom Room)
            {

                if (this.JailBack.TryGetValue(city, out Room))
                {
                    return this.JailBack[city].Id;
                }
                else
                {
                    return 0;
                }
            }

        public int TryToGetCamioneros(string city, out RPRoom Room)
        {

            if (this.CamionerosRooms.TryGetValue(city, out Room))
            {
                return this.CamionerosRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
        public int TryToGetPolStation(string city, out RPRoom Room)
        {

            if (this.PolStationRooms.TryGetValue(city, out Room))
            {
                return this.PolStationRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }

        public int TryToGetBasureros(string city, out RPRoom Room)
        {

            if (this.BasurerosRooms.TryGetValue(city, out Room))
            {
                return this.BasurerosRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }

        public int TryToGetArmeros(string city, out RPRoom Room)
        {

            if (this.ArmerosRooms.TryGetValue(city, out Room))
            {
                return this.ArmerosRooms[city].Id;
            }
            else
            {
                return 0;
            }
        }
    }
}