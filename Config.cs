using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace TorchGodTweaks
{
	public class Config : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public static Config Instance => ModContent.GetInstance<Config>();

		[Label("[i:5043]: Torch God's Favor Recipe")]
		[Tooltip("Toggle if Torch God's Favor will be craftable (recipe in the mod description)")]
		[DefaultValue(true)]
		[ReloadRequired]
		public bool TorchGodsFavorRecipe;

		[Label("[i:8]: Reverse Torch Swap")]
		[Tooltip("Toggle if biome torches should turn into regular torches when picked up (only if 'Biome torch swap' is enabled)")]
		[DefaultValue(true)]
		public bool ReverseTorchSwap;

		[Label("[i:8]->[i:4385]: Convert Torches Upon Hardmode")]
		[Tooltip("Toggle if any torch that is near evil blocks will get converted to the corresponding evil torch when hardmode is first entered")]
		[DefaultValue(true)]
		public bool ConvertTorchesUponHardmode;

		[Label("Prevent Torch God Spawn")]
		[Tooltip("Toggle if Torch God should never spawn (its regular condition is 'more than 100 torches nearby while underground')")]
		[DefaultValue(true)]
		public bool PreventTorchGodSpawn;

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

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) return true;
			else if (!IsPlayerLocalServerOwner(whoAmI))
			{
				message = "You are not the server owner so you can not change this config";
				return false;
			}
			return base.AcceptClientChanges(pendingConfig, whoAmI, ref message);
		}
	}
}
