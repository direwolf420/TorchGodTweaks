using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TorchGodTweaks
{
	public class TGTClentaminatorSystem : ModSystem
	{
		public static Lazy<HashSet<int>> ConvertibleTorchPlaceStyles { get; private set; } = new Lazy<HashSet<int>>(() =>
		{
			var convertibleTorchItems = new HashSet<int>()
			{
				ItemID.Torch,
				ItemID.HallowedTorch,
				ItemID.CorruptTorch,
				ItemID.CrimsonTorch,
			};

			return TGTSystem.BiomeTorchItemToPlaceStyle.Where(pair => convertibleTorchItems.Contains(pair.Key)).Select(pair => pair.Value).ToHashSet();
		});

		public override void Load()
		{
			//Purification powder does not use this, so need to manually convert in its AI
			//Works for clentaminator and holy water
			On.Terraria.WorldGen.Convert += WorldGen_Convert;
		}

		public override void Unload()
		{
			ConvertibleTorchPlaceStyles = null;
		}

		public static void Convert(int x, int y, int newPlaceStyle)
		{
			Main.tile[x, y].TileFrameY = (short)(newPlaceStyle * TGTSystem.FrameY);
			WorldGen.SquareTileFrame(x, y);
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				NetMessage.SendTileSquare(-1, x, y);
			}
		}

		private void WorldGen_Convert(On.Terraria.WorldGen.orig_Convert orig, int i, int j, int conversionType, int size)
		{
			orig(i, j, conversionType, size);

			if (!Config.Instance.ConvertTorchesWhenClentaminating)
			{
				return;
			}

			for (int y = j - size; y <= j + size; y++)
			{
				for (int x = i - size; x <= i + size; x++)
				{
					if (!WorldGen.InWorld(x, y, 1) || Math.Abs(x - i) + Math.Abs(y - j) >= 6)
					{
						continue;
					}

					Tile tile = Main.tile[x, y];
					if (!tile.HasTile || tile.TileType != TileID.Torches)
					{
						continue;
					}

					int placeStyle = tile.TileFrameY / TGTSystem.FrameY;
					int intendedStyle = -1;

					/*
					/// Denotes the biome that you wish to convert to. The following biomes are supported:
					/// 0 => The Purity.
					/// 1 => The Corruption.
					/// 2 => The Hallow.
					/// 3 => Mushroom biome.
					/// 4 => The Crimson.
					*/
					switch (conversionType)
					{
						case 0:
							intendedStyle = TGTSystem.BiomeTorchItemToPlaceStyle[ItemID.Torch];
							break;
						case 1:
							intendedStyle = TGTSystem.BiomeTorchItemToPlaceStyle[ItemID.CorruptTorch];
							break;
						case 2:
							intendedStyle = TGTSystem.BiomeTorchItemToPlaceStyle[ItemID.HallowedTorch];
							break;
						case 3:
							break;
						case 4:
							intendedStyle = TGTSystem.BiomeTorchItemToPlaceStyle[ItemID.CrimsonTorch];
							break;
						default:
							break;
					}

					if (intendedStyle > -1)
					{
						if (ConvertibleTorchPlaceStyles.Value.Contains(placeStyle) && placeStyle != intendedStyle)
						{
							Convert(x, y, intendedStyle);
						}
					}
				}
			}
		}
	}

	public class TGTPurificationPowderProjectile : GlobalProjectile
	{
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return lateInstantiation && entity.type == ProjectileID.PurificationPowder;
		}

		public override void AI(Projectile projectile)
		{
			if (!Config.Instance.ConvertTorchesWhenClentaminating)
			{
				return;
			}

			if (Main.myPlayer != projectile.owner)
			{
				return;
			}

			int startX = Math.Clamp((int)(projectile.position.X / 16f) - 1, 0, Main.maxTilesY);
			int endXPlus1 = Math.Clamp((int)((projectile.position.X + (float)projectile.width) / 16f) + 2, 0, Main.maxTilesX);
			int startY = Math.Clamp((int)(projectile.position.Y / 16f) - 1, 0, Main.maxTilesY);
			int endYPlus1 = Math.Clamp((int)((projectile.position.Y + (float)projectile.height) / 16f) + 2, 0, Main.maxTilesY);

			Vector2 tilePos = default(Vector2);
			for (int y = startY; y < endYPlus1; y++)
			{
				for (int x = startX; x < endXPlus1; x++)
				{
					tilePos.X = x * 16;
					tilePos.Y = y * 16;
					if (!(projectile.position.X + (float)projectile.width > tilePos.X) || !(projectile.position.X < tilePos.X + 16f) || !(projectile.position.Y + (float)projectile.height > tilePos.Y) || !(projectile.position.Y < tilePos.Y + 16f))
					{
						continue;
					}

					Tile tile = Main.tile[x, y];
					if (!tile.HasTile || tile.TileType != TileID.Torches)
					{
						continue;
					}

					int placeStyle = tile.TileFrameY / TGTSystem.FrameY;
					if (placeStyle == TGTSystem.BiomeTorchItemToPlaceStyle[ItemID.HallowedTorch])
					{
						//Purification powder can't convert anything hallow
						continue;
					}

					int intendedStyle = TGTSystem.BiomeTorchItemToPlaceStyle[ItemID.Torch];
					if (TGTClentaminatorSystem.ConvertibleTorchPlaceStyles.Value.Contains(placeStyle) && placeStyle != intendedStyle)
					{
						TGTClentaminatorSystem.Convert(x, y, intendedStyle);
					}
				}
			}
		}
	}
}
