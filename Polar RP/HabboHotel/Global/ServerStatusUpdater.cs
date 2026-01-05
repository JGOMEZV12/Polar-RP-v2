using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using log4net;
using Polar.Database.Interfaces;


namespace Polar.HabboHotel.Global
{
    public class ServerStatusUpdater
    {
        private static ILog log = LogManager.GetLogger("Polar.HabboHotel.Global.ServerUpdater");

        private const int UPDATE_IN_SECS = 30;
        private static bool isExecuted;
        private static Stopwatch lowPriorityProcessWatch;
        private static int _userPeak;
        private static Timer? _timer;

        public static void Init()
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT userpeak FROM server_status");
                _userPeak = dbClient.getInteger();


            }

            _timer = new Timer(new TimerCallback(OnTick), null, TimeSpan.FromSeconds(UPDATE_IN_SECS), TimeSpan.FromSeconds(UPDATE_IN_SECS));

            Console.Title = "Polar Server RP [" + PolarEnvironment.GetConfig().data["hotel.name"] + "] » [0] ON » [0] SALAS » [0] DÍA(S) » [0] HORA(S)";

            //log.Info("Server Status Updater has been started.");
            //lowPriorityProcessWatch = new Stopwatch();
            //lowPriorityProcessWatch.Start();
            //StartProcessing();
            lowPriorityProcessWatch = new Stopwatch();
            lowPriorityProcessWatch.Start();
        }

        /*public static void StartProcessing()
        {
            _mTimer = new Timer(Process, null, 0, 10000);
        }*/

        public static void OnTick(object Obj)
        {
            UpdateOnlineUsers();
        }

        public static void Process()
        {
            if (lowPriorityProcessWatch.ElapsedMilliseconds >= 10000 || !isExecuted)
            {
                isExecuted = true;
                lowPriorityProcessWatch.Restart();
                TimeSpan Uptime = DateTime.Now - PolarEnvironment.ServerStarted;
                //var UsersOnline = PolarEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null && !x.GetHabbo().AppearOffline).ToList().Count;
                int UsersOnline = Convert.ToInt32(PolarEnvironment.GetGame().GetClientManager().Count);
                // int UsersOnline = PolarEnvironment.GetGame().GetClientManager().Count;
                // int UsersOnline = PolarEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null).ToList().Count;
                int RoomCount = PolarEnvironment.GetGame().GetRoomManager().Count;

                Console.Title = "Polar Server RP [" + PolarEnvironment.GetConfig().data["hotel.name"] + "] » [" + UsersOnline + "] ON » [" + RoomCount + "] SALAS » [" + Uptime.Days + "] DÍA(S) » [" + Uptime.Hours + "] HORA(S)";

                using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    //PolarEnvironment.GetGame().GetWebEventManager().BroadCastWebEvent("event_updateonlinecount", UsersOnline.ToString());
                    dbClient.SetQuery("UPDATE `server_status` SET `users_online` = @users, `loaded_rooms` = @loadedRooms LIMIT 1");
                    dbClient.AddParameter("users", UsersOnline);
                    dbClient.AddParameter("loadedRooms", RoomCount);
                    dbClient.AddParameter("upeak", _userPeak);
                    dbClient.RunQuery();
                }
            }
        }
        private static void UpdateOnlineUsers()
        {
            TimeSpan Uptime = DateTime.Now - PolarEnvironment.ServerStarted;

            if (PolarEnvironment.GetGame() == null)
                return;
            if (PolarEnvironment.GetGame().GetClientManager() == null)
                return;
            //var UsersOnline = PolarEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null && !x.GetHabbo().AppearOffline).ToList().Count;
            int UsersOnline = Convert.ToInt32(PolarEnvironment.GetGame().GetClientManager().Count);
            // int UsersOnline = PolarEnvironment.GetGame().GetClientManager().Count;
            //int UsersOnline = PolarEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null).ToList().Count;
            int RoomCount = PolarEnvironment.GetGame().GetRoomManager().Count;

            Console.Title = "Polar Server RP [" + PolarEnvironment.GetConfig().data["hotel.name"] + "] » [" + UsersOnline + "] ON » [" + RoomCount + "] SALAS » [" + Uptime.Days + "] DÍA(S) » [" + Uptime.Hours + "] HORA(S)";

            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                if (UsersOnline > _userPeak)
                    _userPeak = UsersOnline;


                PolarEnvironment.GetGame().GetWebEventManager().BroadCastWebEvent("event_updateonlinecount", UsersOnline.ToString());
                dbClient.SetQuery("UPDATE `server_status` SET `users_online` = @users, `loaded_rooms` = @loadedRooms, userpeak = @upeak LIMIT 1");
                dbClient.AddParameter("users", UsersOnline);
                dbClient.AddParameter("loadedRooms", RoomCount);
                dbClient.AddParameter("upeak", _userPeak);
                dbClient.RunQuery();
            }
        }


        public void Dispose()
        {
            using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `server_status` SET `users_online` = '0', `rooms_loaded` = '0', `environment_status` = '0'");
            }

            _timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
