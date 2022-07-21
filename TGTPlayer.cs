using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace TorchGodTweaks
{
	public class TGTPlayer : ModPlayer
	{
		public const int CooldownThreshold = 120;

		//Player: private int torchGodCooldown;
		public FieldInfo torchGodCooldownInfo;

		private bool reflectionFailed = false;

		//Runs after vanilla decrements the timer, never letting it compare to 0
		public override void PostUpdateEquips()
		{
			//The vanilla code of that runs clientside
			if (Main.myPlayer != Player.whoAmI)
			{
				return;
			}

			if (!Config.Instance.PreventTorchGodSpawn)
			{
				return;
			}

			if (reflectionFailed)
			{
				return;
			}

			if (torchGodCooldownInfo == null)
			{
				torchGodCooldownInfo = typeof(Player).GetField("torchGodCooldown", BindingFlags.Instance | BindingFlags.NonPublic);

				if (torchGodCooldownInfo == null)
				{
					reflectionFailed = true;
					Mod.Logger.Info("Failed to reflect 'torchGodCooldown', 'Prevent Torch God Spawn' config setting will not work");
				}
			}

			if (torchGodCooldownInfo != null)
			{
				//Prevent torch god from ever appearing by not letting its timer reach 0
				var value = (int)torchGodCooldownInfo.GetValue(Player);
				if (value < CooldownThreshold)
				{
					torchGodCooldownInfo.SetValue(Player, CooldownThreshold);
				}
			}
		}
	}
}
