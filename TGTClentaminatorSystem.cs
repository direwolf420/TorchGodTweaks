using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TorchGodTweaks
{
	//Only works with vanilla!!
	public class TGTClentaminatorSystem : ModSystem
	{
		public static Lazy<HashSet<int>> ConvertibleTorchPlaceStyles { get; private set; } = new Lazy<HashSet<int>>(() =>
		{
			//Only torches for which solutions exist
			var convertibleTorchItems = new HashSet<int>()
			{
				ItemID.Torch,
				ItemID.HallowedTorch,
				ItemID.CorruptTorch,
				ItemID.CrimsonTorch,
				ItemID.IceTorch,
				ItemID.DesertTorch,
				ItemID.MushroomTorch,
			};

			return TGTSystem.VanillaBiomeTorchItemToPlaceStyle.Where(pair => convertibleTorchItems.Contains(pair.Key)).Select(pair => pair.Value).ToHashSet();
		});

		public override void Load()
		{
			//Purification powder does not use this, so need to manually convert in its AI
			//Works for clentaminator and thrown waters
			On_WorldGen.Convert += WorldGen_Convert;
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

		private void WorldGen_Convert(On_WorldGen.orig_Convert orig, int i, int j, int conversionType, int size)
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
						case BiomeConversionID.Purity:
							intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.Torch];
							break;
						case BiomeConversionID.Corruption:
							intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.CorruptTorch];
							break;
						case BiomeConversionID.Hallow:
							intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.HallowedTorch];
							break;
						case BiomeConversionID.GlowingMushroom:
							intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.MushroomTorch];
							break;
						case BiomeConversionID.Crimson:
							intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.CrimsonTorch];
							break;
						case BiomeConversionID.Sand:
							intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.DesertTorch];
							break;
						case BiomeConversionID.Snow:
							intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.IceTorch];
							break;
						case BiomeConversionID.Dirt:
							intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.Torch];
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

					int intendedStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.Torch];
					int corruptionStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.CorruptTorch];
					int crimsonStyle = TGTSystem.VanillaBiomeTorchItemToPlaceStyle[ItemID.CrimsonTorch];
					if ((placeStyle == corruptionStyle || placeStyle == crimsonStyle) && placeStyle != intendedStyle)
					{
						TGTClentaminatorSystem.Convert(x, y, intendedStyle);
					}
				}
			}
		}
	}
}
