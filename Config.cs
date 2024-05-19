using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace TorchGodTweaks
{
	public class Config : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public static Config Instance => ModContent.GetInstance<Config>();

		[ReloadRequired]
		[DefaultValue(true)]
		public bool TorchGodsFavorRecipe;

		[DefaultValue(true)]
		public bool ReverseTorchSwap;

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ReverseTorchSwapForDemonTorch;

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ReverseTorchSwapForAetherTorch;

		[ReloadRequired]
		[DefaultValue(true)]
		public bool ReverseTorchSwapForBoneTorch;

		[DefaultValue(true)]
		public bool ConvertTorchesUponHardmode;

		[DefaultValue(true)]
		public bool PreventTorchGodSpawn;

		[DefaultValue(true)]
		public bool ConvertTorchesWhenClentaminating;

		[ReloadRequired]
		[DefaultValue(true)]
		public bool AffectCampfires;

		public static bool IsPlayerLocalServerOwner(int whoAmI)
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				return Netplay.Connection.Socket.GetRemoteAddress().IsLocalHost();
			}

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				RemoteClient client = Netplay.Clients[i];
				if (client.State == 10 && i == whoAmI && client.Socket.GetRemoteAddress().IsLocalHost())
				{
					return true;
				}
			}
			return false;
		}

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref NetworkText message)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return true;
			else if (!IsPlayerLocalServerOwner(whoAmI))
			{
				message = NetworkText.FromKey("tModLoader.ModConfigRejectChangesNotHost");
				return false;
			}
			return base.AcceptClientChanges(pendingConfig, whoAmI, ref message);
		}
	}
}
