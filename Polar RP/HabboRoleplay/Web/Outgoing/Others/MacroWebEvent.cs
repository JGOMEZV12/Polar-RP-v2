using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Polar.HabboHotel.GameClients;
using System.IO;
using Polar.HabboRoleplay.Misc;
using Polar.Database.Interfaces;
using System.Data;
using Polar.Utilities;

namespace Polar.HabboHotel.Roleplay.Web.Outgoing.Misc
{
	/// <summary>
	/// ATMWebEvent class.
	/// </summary>
	class MacroWebEvent : IWebEvent
	{
		/// <summary>
		/// Executes socket data.
		/// </summary>
		/// <param name="Client"></param>
		/// <param name="Data"></param>
		/// <param name="Socket"></param>
		public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
		{

			if (!PolarEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PolarEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
				return;

			string Action = (Data.Contains(',') ? Data.Split(',')[0] : Data);

			switch (Action)
			{


				#region Enviar Macros
				case "st":
					{
					string[] ReceivedData = Data.Split(',');

						Socket.Send("limparmacro");
						DataTable Cargos = null;
						using (var dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
						{
							dbClient.SetQuery("SELECT `id` FROM `users` where `username` = '" + Client.GetHabbo().Username + "' LIMIT 1");
							var UserRow = dbClient.getRow();

							int Id = Convert.ToInt32(UserRow["id"]);

							dbClient.SetQuery("SELECT * FROM `user_macros` WHERE `user_id` = '" + Id + "'");
							Cargos = dbClient.getTable();

							if (Cargos == null)
							{
								return;
							}
							else
							{
								foreach (DataRow Row in Cargos.Rows)
								{

									int id = Convert.ToInt32(Row["id"]);
									string key = Convert.ToString(Row["macro_key"]);
									string val = Convert.ToString(Row["macro_val"]);
									string tecla = Convert.ToString(Row["macro_tecla"]);

									PolarEnvironment.GetGame().GetWebEventManager().SendDataDirect(Client, "macro|" + key + "|" + val + "|" + tecla);

								}
							}
						}

					}
					break;
				#endregion

				#region Remover Macros
				case "rv":
					{
						string[] ReceivedData = Data.Split(',');


						string Key = Convert.ToString(ReceivedData[1]);

						using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
						{
							
								dbClient.SetQuery("DELETE FROM `user_macros` WHERE `user_id` = @user AND `macro_tecla` = @tecla");
								dbClient.AddParameter("user", Client.GetHabbo().Id);
								dbClient.AddParameter("tecla", Key);
								dbClient.RunQuery();
							
}

					}
					break;
				#endregion

				#region Adicionar Macros
				case "ad":
					{
						string[] ReceivedData = Data.Split(',');

						
						string Key = StringCharFilter.Escape(ReceivedData[1]);
						string Val = StringCharFilter.Escape(ReceivedData[2]);
						string Tecla = StringCharFilter.Escape(ReceivedData[3]);
						if (Tecla.Length < 1)
							return;

						if (Key.Length > 10)
							return;
						if (Tecla.Length > 10)
							return;
						if (Key.Length < 1)
							return;
						if (Val.Length < 1)
							return;
						if (Val.Length > 150)
							return;
						if (Val.Length > 150)
							Val = Val.Substring(0, 150);

						if (Client.GetHabbo().Frase(Tecla) == Val)
						{

							Client.SendWhisper("Esta frase/clave ya ha sido agregada, la acción no se pudo completar.", 1);
							return;
						}
						if (Client.GetHabbo().Tecla(Tecla) == Tecla)
						{
							Client.SendWhisper("Esta frase/clave ya ha sido agregada, la acción no se pudo completar.", 1);
							return;
						}

						Socket.Send("macro|" + Key + "|" + Val + "|" + Tecla);
						Client.GetHabbo().macro(Key, Val, Tecla);

					}
					break;
					#endregion
			}
		}
	}
}
