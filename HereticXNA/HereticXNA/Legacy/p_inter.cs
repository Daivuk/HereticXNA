using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// P_inter.c

namespace HereticXNA
{
	public static class p_inter
	{
		public const int BONUSADD = 6;


		public static int[] WeaponValue = new int[]
{
	1,		// staff
	3,		// goldwand
	4,		// crossbow
	5,		// blaster
	6,		// skullrod
	7,		// phoenixrod
	8,		// mace
	2,		// gauntlets
	0		// beak
};

		public static int[] maxammo = new int[(int)DoomDef.ammotype_t.NUMAMMO]
{
	100,	// gold wand
	50,		// crossbow
	200,	// blaster
	200,	// skull rod
	20,		// phoenix rod
	150		// mace
};

		public static int[] GetWeaponAmmo = new int[(int)DoomDef.weapontype_t.NUMWEAPONS]
{
	0,		// staff
	25,		// gold wand
	10,		// crossbow
	30,		// blaster
	50,		// skull rod
	2,		// phoenix rod
	50,		// mace
	0,		// gauntlets
	0		// beak
};

		public static DoomDef.weapontype_t[] GetAmmoChange = new DoomDef.weapontype_t[]
{
	DoomDef.weapontype_t.wp_goldwand,
	DoomDef.weapontype_t.wp_crossbow,
	DoomDef.weapontype_t.wp_blaster,
	DoomDef.weapontype_t.wp_skullrod,
	DoomDef.weapontype_t.wp_phoenixrod,
	DoomDef.weapontype_t.wp_mace
};

		//--------------------------------------------------------------------------
		//
		// PROC P_SetMessage
		//
		//--------------------------------------------------------------------------

		public static bool ultimatemsg;

		public static void P_SetMessage(DoomDef.player_t player, string message, bool ultmsg)
		{
			if ((ultimatemsg || !mn_menu.messageson) && !ultmsg)
			{
				return;
			}
			player.message = message;
			player.messageTics = DoomDef.MESSAGETICS;
			r_draw.BorderTopRefresh = true;
			if (ultmsg)
			{
				ultimatemsg = true;
			}
		}

		//--------------------------------------------------------------------------
		//
		// FUNC P_GiveAmmo
		//
		// Returns true if the player accepted the ammo, false if it was
		// refused (player has maxammo[ammo]).
		//
		//--------------------------------------------------------------------------

		public static bool P_GiveAmmo(DoomDef.player_t player, DoomDef.ammotype_t ammo, int count)
		{
			int prevAmmo;

			if (ammo == DoomDef.ammotype_t.am_noammo)
			{
				return (false);
			}
			if (ammo < 0 || ammo > DoomDef.ammotype_t.NUMAMMO)
			{
				i_ibm.I_Error("P_GiveAmmo: bad type " + (int)ammo);
			}
			if (player.ammo[(int)ammo] == player.maxammo[(int)ammo])
			{
				return (false);
			}
			if (g_game.gameskill == DoomDef.skill_t.sk_baby || g_game.gameskill == DoomDef.skill_t.sk_nightmare)
			{ // extra ammo in baby mode and nightmare mode
				count += count >> 1;
			}
			prevAmmo = player.ammo[(int)ammo];

			player.ammo[(int)ammo] += count;
			if (player.ammo[(int)ammo] > player.maxammo[(int)ammo])
			{
				player.ammo[(int)ammo] = player.maxammo[(int)ammo];
			}
			if (prevAmmo != 0)
			{
				// Don't attempt to change weapons if the player already had
				// ammo of the type just given
				return (true);
			}
			if (player.readyweapon == DoomDef.weapontype_t.wp_staff
				|| player.readyweapon == DoomDef.weapontype_t.wp_gauntlets)
			{
				if (player.weaponowned[(int)GetAmmoChange[(int)ammo]])
				{
					player.pendingweapon = GetAmmoChange[(int)ammo];
				}
			}

			return (true);
		}

		//--------------------------------------------------------------------------
		//
		// FUNC P_GiveWeapon
		//
		// Returns true if the weapon or its ammo was accepted.
		//
		//--------------------------------------------------------------------------

		public static bool P_GiveWeapon(DoomDef.player_t player, DoomDef.weapontype_t weapon)
		{
			bool gaveAmmo;
			bool gaveWeapon;

			if (g_game.netgame && !g_game.deathmatch)
			{ // Cooperative net-game
				if (player.weaponowned[(int)weapon])
				{
					return (false);
				}
				player.bonuscount += BONUSADD;
				player.weaponowned[(int)weapon] = true;
				P_GiveAmmo(player, p_pspr.wpnlev1info[(int)weapon].ammo,
					GetWeaponAmmo[(int)weapon]);
				player.pendingweapon = weapon;
				if (player == g_game.players[g_game.consoleplayer])
				{
					i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_wpnup);
				}
				return (false);
			}
			gaveAmmo = P_GiveAmmo(player, p_pspr.wpnlev1info[(int)weapon].ammo,
				GetWeaponAmmo[(int)weapon]);
			if (player.weaponowned[(int)weapon])
			{
				gaveWeapon = false;
			}
			else
			{
				gaveWeapon = true;
				player.weaponowned[(int)weapon] = true;
				if (WeaponValue[(int)weapon] > WeaponValue[(int)player.readyweapon])
				{ // Only switch to more powerful weapons
					player.pendingweapon = weapon;
				}
			}
			return (gaveWeapon || gaveAmmo);
		}


		//---------------------------------------------------------------------------
		//
		// FUNC P_GiveBody
		//
		// Returns false if the body isn't needed at all.
		//
		//---------------------------------------------------------------------------

		public static bool P_GiveBody(DoomDef.player_t player, int num)
		{
			int max;

			max = p_local.MAXHEALTH;
			if (player.chickenTics != 0)
			{
				max = p_local.MAXCHICKENHEALTH;
			}
			if (player.health >= max)
			{
				return (false);
			}
			player.health += num;
			if (player.health > max)
			{
				player.health = max;
			}
			player.mo.health = player.health;
			return (true);
		}

		//---------------------------------------------------------------------------
		//
		// FUNC P_GiveArmor
		//
		// Returns false if the armor is worse than the current armor.
		//
		//---------------------------------------------------------------------------

		public static bool P_GiveArmor(DoomDef.player_t player, int armortype)
		{
			int hits;

			hits = armortype * 100;
			if (player.armorpoints >= hits)
			{
				return (false);
			}
			player.armortype = armortype;
			player.armorpoints = hits;
			return (true);
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_GiveKey
		//
		//---------------------------------------------------------------------------

		public static void P_GiveKey(DoomDef.player_t player, DoomDef.keytype_t key)
		{
			if (player.keys[(int)key])
			{
				return;
			}
			if (player == g_game.players[g_game.consoleplayer])
			{
				sb_bar.playerkeys |= 1 << (int)key;
				am_map.KeyPoints[(int)key].x = 0;
				am_map.KeyPoints[(int)key].y = 0;
			}
			player.bonuscount = BONUSADD;
			player.keys[(int)key] = true;
		}

#if DOS
//---------------------------------------------------------------------------
//
// FUNC P_GivePower
//
// Returns true if power accepted.
//
//---------------------------------------------------------------------------

boolean P_GivePower(player_t *player, powertype_t power)
{
	if(power == pw_invulnerability)
	{
		if(player.powers[power] > BLINKTHRESHOLD)
		{ // Already have it
			return(false);
		}
		player.powers[power] = INVULNTICS;
		return(true);
	}
	if(power == pw_weaponlevel2)
	{
		if(player.powers[power] > BLINKTHRESHOLD)
		{ // Already have it
			return(false);
		}
		player.powers[power] = WPNLEV2TICS;
		return(true);
	}
	if(power == pw_invisibility)
	{
		if(player.powers[power] > BLINKTHRESHOLD)
		{ // Already have it
			return(false);
		}
		player.powers[power] = INVISTICS;
		player.mo.flags |= MF_SHADOW;
		return(true);
	}
	if(power == pw_flight)
	{
		if(player.powers[power] > BLINKTHRESHOLD)
		{ // Already have it
			return(false);
		}
		player.powers[power] = FLIGHTTICS;
		player.mo.flags2 |= MF2_FLY;
		player.mo.flags |= MF_NOGRAVITY;
		if(player.mo.z <= player.mo.floorz)
		{
			player.flyheight = 10; // thrust the player in the air a bit
		}
		return(true);
	}
	if(power == pw_infrared)
	{
		if(player.powers[power] > BLINKTHRESHOLD)
		{ // Already have it
			return(false);
		}
		player.powers[power] = INFRATICS;
		return(true);
	}

	if(player.powers[power])
	{
		return(false); // already got it
	}
	player.powers[power] = 1;
	return(true);
}

//---------------------------------------------------------------------------
//
// FUNC P_GiveArtifact
//
// Returns true if artifact accepted.
//
//---------------------------------------------------------------------------

boolean P_GiveArtifact(player_t *player, artitype_t arti, mobj_t *mo)
{
	int i;

	i = 0;
	while(player.inventory[i].type != arti && i < player.inventorySlotNum)
	{
		i++;
	}
	if(i == player.inventorySlotNum)
	{
		player.inventory[i].count = 1;
		player.inventory[i].type = arti;
		player.inventorySlotNum++;
	}
	else
	{
		if(player.inventory[i].count >= 16)
		{ // Player already has 16 of this item
			return(false);
		}
		player.inventory[i].count++;
	}
	if(player.artifactCount == 0)
	{
		player.readyArtifact = arti;
	}
	player.artifactCount++;
	if(mo && (mo.flags&MF_COUNTITEM))
	{
		player.itemcount++;
	}
	return(true);
}

//---------------------------------------------------------------------------
//
// PROC P_SetDormantArtifact
//
// Removes the MF_SPECIAL flag, and initiates the artifact pickup
// animation.
//
//---------------------------------------------------------------------------

void P_SetDormantArtifact(mobj_t *arti)
{
	arti.flags &= ~MF_SPECIAL;
	if(deathmatch && (arti.type != MT_ARTIINVULNERABILITY)
		&& (arti.type != MT_ARTIINVISIBILITY))
	{
		P_SetMobjState(arti, S_DORMANTARTI1);
	}
	else
	{ // Don't respawn
		P_SetMobjState(arti, S_DEADARTI1);
	}
	S_StartSound(arti, sfx_artiup);
}
#endif
		//---------------------------------------------------------------------------
		//
		// PROC A_RestoreArtifact
		//
		//---------------------------------------------------------------------------
		public class A_RestoreArtifact : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				//arti.flags |= MF_SPECIAL;
				//P_SetMobjState(arti, arti.info.spawnstate);
				//S_StartSound(arti, sfx_respawn);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_HideSpecialThing
		//
		//----------------------------------------------------------------------------

		public static void P_HideSpecialThing(DoomDef.mobj_t thing)
		{
			thing.flags &= ~DoomDef.MF_SPECIAL;
			thing.flags2 |= DoomDef.MF2_DONTDRAW;
			p_mobj.P_SetMobjState(thing, info.statenum_t.S_HIDESPECIAL1);
		}

		//---------------------------------------------------------------------------
		//
		// PROC A_RestoreSpecialThing1
		//
		// Make a special thing visible again.
		//
		//---------------------------------------------------------------------------
		public class A_RestoreSpecialThing1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				//if (thing.type == MT_WMACE)
				//{ // Do random mace placement
				//    P_RepositionMace(thing);
				//}
				//thing.flags2 &= ~MF2_DONTDRAW;
				//S_StartSound(thing, sfx_respawn);
			}
		}


		//---------------------------------------------------------------------------
		//
		// PROC A_RestoreSpecialThing2
		//
		//---------------------------------------------------------------------------
		public class A_RestoreSpecialThing2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				//thing.flags |= MF_SPECIAL;
				//P_SetMobjState(thing, thing.info.spawnstate);
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_TouchSpecialThing
		//
		//---------------------------------------------------------------------------

		public static void P_TouchSpecialThing(DoomDef.mobj_t special, DoomDef.mobj_t toucher)
		{
			int i;
			DoomDef.player_t player;
			int delta;
			int sound;
			bool respawn;

			delta = special.z - toucher.z;
			if (delta > toucher.height || delta < -32 * DoomDef.FRACUNIT)
			{ // Out of reach
				return;
			}
			if (toucher.health <= 0)
			{ // Toucher is dead
				return;
			}
			sound = (int)sounds.sfxenum_t.sfx_itemup;
			player = toucher.player;
			respawn = true;
			switch (special.sprite)
			{
				// Items
				case info.spritenum_t.SPR_PTN1: // Item_HealingPotion
					if (!P_GiveBody(player, 10))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_ITEMHEALTH, false);
					break;
				case info.spritenum_t.SPR_SHLD: // Item_Shield1
					if (!P_GiveArmor(player, 1))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_ITEMSHIELD1, false);
					break;
#if DOS
		case info.spritenum_t.SPR_SHD2: // Item_Shield2
			if(!P_GiveArmor(player, 2))
			{
				return;
			}
			P_SetMessage(player, TXT_ITEMSHIELD2, false);
			break;
		case info.spritenum_t.SPR_BAGH: // Item_BagOfHolding
			if(!player.backpack)
			{
				for(i = 0; i < NUMAMMO; i++)
				{
					player.maxammo[i] *= 2;
				}
				player.backpack = true;
			}
			P_GiveAmmo(player, am_goldwand, AMMO_GWND_WIMPY);
			P_GiveAmmo(player, am_blaster, AMMO_BLSR_WIMPY);
			P_GiveAmmo(player, am_crossbow, AMMO_CBOW_WIMPY);
			P_GiveAmmo(player, am_skullrod, AMMO_SKRD_WIMPY);
			P_GiveAmmo(player, am_phoenixrod, AMMO_PHRD_WIMPY);
			P_SetMessage(player, TXT_ITEMBAGOFHOLDING, false);
			break;
		case info.spritenum_t.SPR_SPMP: // Item_SuperMap
			if(!P_GivePower(player, pw_allmap))
			{
				return;
			}
			P_SetMessage(player, TXT_ITEMSUPERMAP, false);
			break;
#endif
				// Keys
				case info.spritenum_t.SPR_BKYY: // Key_Blue
					if (!player.keys[(int)DoomDef.keytype_t.key_blue])
					{
						P_SetMessage(player, dstring.TXT_GOTBLUEKEY, false);
					}
					P_GiveKey(player, DoomDef.keytype_t.key_blue);
					sound = (int)sounds.sfxenum_t.sfx_keyup;
					if (!g_game.netgame)
					{
						break;
					}
					return;
				case info.spritenum_t.SPR_CKYY: // Key_Yellow
					if (!player.keys[(int)DoomDef.keytype_t.key_yellow])
					{
						P_SetMessage(player, dstring.TXT_GOTYELLOWKEY, false);
					}
					sound = (int)sounds.sfxenum_t.sfx_keyup;
					P_GiveKey(player, DoomDef.keytype_t.key_yellow);
					if (!g_game.netgame)
					{
						break;
					}
					return;
				case info.spritenum_t.SPR_AKYY: // Key_Green
					if (!player.keys[(int)DoomDef.keytype_t.key_green])
					{
						P_SetMessage(player, dstring.TXT_GOTGREENKEY, false);
					}
					sound = (int)sounds.sfxenum_t.sfx_keyup;
					P_GiveKey(player, DoomDef.keytype_t.key_green);
					if (!g_game.netgame)
					{
						break;
					}
					return;
#if DOS
		// Artifacts
		case info.spritenum_t.SPR_PTN2: // Arti_HealingPotion
			if(P_GiveArtifact(player, arti_health, special))
			{
				P_SetMessage(player, TXT_ARTIHEALTH, false);
				P_SetDormantArtifact(special);
			}
			return;
		case info.spritenum_t.SPR_SOAR: // Arti_Fly
			if(P_GiveArtifact(player, arti_fly, special))
			{
				P_SetMessage(player, TXT_ARTIFLY, false);
				P_SetDormantArtifact(special);
			}
			return;
		case info.spritenum_t.SPR_INVU: // Arti_Invulnerability
			if(P_GiveArtifact(player, arti_invulnerability, special))
			{
				P_SetMessage(player, TXT_ARTIINVULNERABILITY, false);
				P_SetDormantArtifact(special);
			}
			return;
		case info.spritenum_t.SPR_PWBK: // Arti_TomeOfPower
			if(P_GiveArtifact(player, arti_tomeofpower, special))
			{
				P_SetMessage(player, TXT_ARTITOMEOFPOWER, false);
				P_SetDormantArtifact(special);
			}
			return;
		case info.spritenum_t.SPR_INVS: // Arti_Invisibility
			if(P_GiveArtifact(player, arti_invisibility, special))
			{
				P_SetMessage(player, TXT_ARTIINVISIBILITY, false);
				P_SetDormantArtifact(special);
			}
			return;
		case info.spritenum_t.SPR_EGGC: // Arti_Egg
			if(P_GiveArtifact(player, arti_egg, special))
			{
				P_SetMessage(player, TXT_ARTIEGG, false);
				P_SetDormantArtifact(special);
			}
			return;
		case info.spritenum_t.SPR_SPHL: // Arti_SuperHealth
			if(P_GiveArtifact(player, arti_superhealth, special))
			{
				P_SetMessage(player, TXT_ARTISUPERHEALTH, false);
				P_SetDormantArtifact(special);
			}
			return;
		case info.spritenum_t.SPR_TRCH: // Arti_Torch
			if(P_GiveArtifact(player, arti_torch, special))
			{
				P_SetMessage(player, TXT_ARTITORCH, false);
				P_SetDormantArtifact(special);
			}
			return;
		case info.spritenum_t.SPR_FBMB: // Arti_FireBomb
			if(P_GiveArtifact(player, arti_firebomb, special))
			{
				P_SetMessage(player, TXT_ARTIFIREBOMB, false);
				P_SetDormantArtifact(special);
			}
			return;
		case info.spritenum_t.SPR_ATLP: // Arti_Teleport
			if(P_GiveArtifact(player, arti_teleport, special))
			{
				P_SetMessage(player, TXT_ARTITELEPORT, false);
				P_SetDormantArtifact(special);
			}
			return;
#endif
				// Ammo
				case info.spritenum_t.SPR_AMG1: // Ammo_GoldWandWimpy
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_goldwand, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOGOLDWAND1, false);
					break;
				case info.spritenum_t.SPR_AMG2: // Ammo_GoldWandHefty
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_goldwand, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOGOLDWAND2, false);
					break;
				case info.spritenum_t.SPR_AMM1: // Ammo_MaceWimpy
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_mace, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOMACE1, false);
					break;
				case info.spritenum_t.SPR_AMM2: // Ammo_MaceHefty
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_mace, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOMACE2, false);
					break;
				case info.spritenum_t.SPR_AMC1: // Ammo_CrossbowWimpy
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_crossbow, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOCROSSBOW1, false);
					break;
				case info.spritenum_t.SPR_AMC2: // Ammo_CrossbowHefty
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_crossbow, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOCROSSBOW2, false);
					break;
				case info.spritenum_t.SPR_AMB1: // Ammo_BlasterWimpy
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_blaster, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOBLASTER1, false);
					break;
				case info.spritenum_t.SPR_AMB2: // Ammo_BlasterHefty
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_blaster, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOBLASTER2, false);
					break;
				case info.spritenum_t.SPR_AMS1: // Ammo_SkullRodWimpy
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_skullrod, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOSKULLROD1, false);
					break;
				case info.spritenum_t.SPR_AMS2: // Ammo_SkullRodHefty
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_skullrod, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOSKULLROD2, false);
					break;
				case info.spritenum_t.SPR_AMP1: // Ammo_PhoenixRodWimpy
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_phoenixrod, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOPHOENIXROD1, false);
					break;
				case info.spritenum_t.SPR_AMP2: // Ammo_PhoenixRodHefty
					if (!P_GiveAmmo(player, DoomDef.ammotype_t.am_phoenixrod, special.health))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_AMMOPHOENIXROD2, false);
					break;

				// Weapons
				case info.spritenum_t.SPR_WMCE: // Weapon_Mace
					if (!P_GiveWeapon(player, DoomDef.weapontype_t.wp_mace))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_WPNMACE, false);
					sound = (int)sounds.sfxenum_t.sfx_wpnup;
					break;
				case info.spritenum_t.SPR_WBOW: // Weapon_Crossbow
					if (!P_GiveWeapon(player, DoomDef.weapontype_t.wp_crossbow))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_WPNCROSSBOW, false);
					sound = (int)sounds.sfxenum_t.sfx_wpnup;
					break;
				case info.spritenum_t.SPR_WBLS: // Weapon_Blaster
					if (!P_GiveWeapon(player, DoomDef.weapontype_t.wp_blaster))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_WPNBLASTER, false);
					sound = (int)sounds.sfxenum_t.sfx_wpnup;
					break;
				case info.spritenum_t.SPR_WSKL: // Weapon_SkullRod
					if (!P_GiveWeapon(player, DoomDef.weapontype_t.wp_skullrod))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_WPNSKULLROD, false);
					sound = (int)sounds.sfxenum_t.sfx_wpnup;
					break;
				case info.spritenum_t.SPR_WPHX: // Weapon_PhoenixRod
					if (!P_GiveWeapon(player, DoomDef.weapontype_t.wp_phoenixrod))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_WPNPHOENIXROD, false);
					sound = (int)sounds.sfxenum_t.sfx_wpnup;
					break;
				case info.spritenum_t.SPR_WGNT: // Weapon_Gauntlets
					if (!P_GiveWeapon(player, DoomDef.weapontype_t.wp_gauntlets))
					{
						return;
					}
					P_SetMessage(player, dstring.TXT_WPNGAUNTLETS, false);
					sound = (int)sounds.sfxenum_t.sfx_wpnup;
					break;
				default:
					i_ibm.I_Error("P_SpecialThing: Unknown gettable thing");
					break;
			}
			if ((special.flags & DoomDef.MF_COUNTITEM) != 0)
			{
				player.itemcount++;
			}
			if (g_game.deathmatch && respawn && (special.flags & DoomDef.MF_DROPPED) == 0)
			{
				p_inter.P_HideSpecialThing(special);
			}
			else
			{
				p_mobj.P_RemoveMobj(special);
			}
			player.bonuscount += BONUSADD;
			if (player == g_game.players[g_game.consoleplayer])
			{
				i_ibm.S_StartSound(null, sound);
				sb_bar.SB_PaletteFlash();
			}
		}


		//---------------------------------------------------------------------------
		//
		// PROC P_KillMobj
		//
		//---------------------------------------------------------------------------

		public static void P_KillMobj(DoomDef.mobj_t source, DoomDef.mobj_t target)
		{
			target.flags &= ~(DoomDef.MF_SHOOTABLE | DoomDef.MF_FLOAT | DoomDef.MF_SKULLFLY | DoomDef.MF_NOGRAVITY);
			target.flags |= DoomDef.MF_CORPSE | DoomDef.MF_DROPOFF;
			target.flags2 &= ~DoomDef.MF2_PASSMOBJ;
			target.height >>= 2;
			if (source != null && source.player != null)
			{
				if ((target.flags & DoomDef.MF_COUNTKILL) != 0)
				{ // Count for intermission
					source.player.killcount++;
				}
				if (target.player != null)
				{ // Frag stuff
					if (target == source)
					{ // Self-frag
						target.player.frags[Array.IndexOf(g_game.players, target.player)]--;
					}
					else
					{
						source.player.frags[Array.IndexOf(g_game.players, target.player)]++;
						if (source.player == g_game.players[g_game.consoleplayer])
						{
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_gfrag);
						}
						if (source.player.chickenTics != 0)
						{ // Make a super chicken
							//p_inter.P_GivePower(source.player, DoomDef.powertype_t.pw_weaponlevel2);
						}
					}
				}
			}
			else if (!g_game.netgame && (target.flags & DoomDef.MF_COUNTKILL) != 0)
			{ // Count all monster deaths
				g_game.players[0].killcount++;
			}
			if (target.player != null)
			{
				if (source == null)
				{ // Self-frag
					target.player.frags[Array.IndexOf(g_game.players, target.player)]--;
				}
				target.flags &= ~DoomDef.MF_SOLID;
				target.flags2 &= ~DoomDef.MF2_FLY;
				target.player.powers[(int)DoomDef.powertype_t.pw_flight] = 0;
				target.player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] = 0;
				target.player.playerstate = DoomDef.playerstate_t.PST_DEAD;
				p_pspr.P_DropWeapon(target.player);
				if ((target.flags2 & DoomDef.MF2_FIREDAMAGE) != 0)
				{ // Player flame death
					p_mobj.P_SetMobjState(target, info.statenum_t.S_PLAY_FDTH1);
					return;
				}
			}
			if (target.health < -(target.infol.spawnhealth >> 1)
				&& target.infol.xdeathstate != 0)
			{ // Extreme death
				p_mobj.P_SetMobjState(target, (info.statenum_t)target.infol.xdeathstate);
			}
			else
			{ // Normal death
				p_mobj.P_SetMobjState(target, (info.statenum_t)target.infol.deathstate);
			}
			target.tics -= m_misc.P_Random() & 3;
		}
#if DOS

//---------------------------------------------------------------------------
//
// FUNC P_MinotaurSlam
//
//---------------------------------------------------------------------------

void P_MinotaurSlam(mobj_t *source, mobj_t *target)
{
	uint angle;
	int thrust;

	angle = R_PointToAngle2(source.x, source.y, target.x, target.y);
	angle >>= ANGLETOFINESHIFT;
	thrust = 16*FRACUNIT+(P_Random()<<10);
	target.momx += FixedMul(thrust, finecosine[angle]);
	target.momy += FixedMul(thrust, finesine[angle]);
	P_DamageMobj(target, NULL, NULL, HITDICE(6));
	if(target.player)
	{
		target.reactiontime = 14+(P_Random()&7);
	}
}

//---------------------------------------------------------------------------
//
// FUNC P_TouchWhirlwind
//
//---------------------------------------------------------------------------

void P_TouchWhirlwind(mobj_t *target)
{
	int randVal;

	target.angle += (P_Random()-P_Random())<<20;
	target.momx += (P_Random()-P_Random())<<10;
	target.momy += (P_Random()-P_Random())<<10;
	if(leveltime&16 && !(target.flags2&MF2_BOSS))
	{
		randVal = P_Random();
		if(randVal > 160)
		{
			randVal = 160;
		}
		target.momz += randVal<<10;
		if(target.momz > 12*FRACUNIT)
		{
			target.momz = 12*FRACUNIT;
		}
	}
	if(!(leveltime&7))
	{
		P_DamageMobj(target, NULL, NULL, 3);
	}
}

//---------------------------------------------------------------------------
//
// FUNC P_ChickenMorphPlayer
//
// Returns true if the player gets turned into a chicken.
//
//---------------------------------------------------------------------------

boolean P_ChickenMorphPlayer(player_t *player)
{
	mobj_t *pmo;
	mobj_t *fog;
	mobj_t *chicken;
	int x;
	int y;
	int z;
	uint angle;
	int oldFlags2;

	if(player.chickenTics)
	{
		if((player.chickenTics < CHICKENTICS-TICSPERSEC)
			&& !player.powers[pw_weaponlevel2])
		{ // Make a super chicken
			P_GivePower(player, pw_weaponlevel2);
		}
		return(false);
	}
	if(player.powers[pw_invulnerability])
	{ // Immune when invulnerable
		return(false);
	}
	pmo = player.mo;
	x = pmo.x;
	y = pmo.y;
	z = pmo.z;
	angle = pmo.angle;
	oldFlags2 = pmo.flags2;
	P_SetMobjState(pmo, S_FREETARGMOBJ);
	fog = P_SpawnMobj(x, y, z+TELEFOGHEIGHT, MT_TFOG);
	S_StartSound(fog, sfx_telept);
	chicken = P_SpawnMobj(x, y, z, MT_CHICPLAYER);
	chicken.special1 = player.readyweapon;
	chicken.angle = angle;
	chicken.player = player;
	player.health = chicken.health = MAXCHICKENHEALTH;
	player.mo = chicken;
	player.armorpoints = player.armortype = 0;
	player.powers[pw_invisibility] = 0;
	player.powers[pw_weaponlevel2] = 0;
	if(oldFlags2&MF2_FLY)
	{
		chicken.flags2 |= MF2_FLY;
	}
	player.chickenTics = CHICKENTICS;
	P_ActivateBeak(player);
	return(true);
}

//---------------------------------------------------------------------------
//
// FUNC P_ChickenMorph
//
//---------------------------------------------------------------------------

boolean P_ChickenMorph(mobj_t *actor)
{
	mobj_t *fog;
	mobj_t *chicken;
	mobj_t *target;
	mobjtype_t moType;
	int x;
	int y;
	int z;
	uint angle;
	int ghost;

	if(actor.player)
	{
		return(false);
	}
	moType = actor.type;
	switch(moType)
	{
		case MT_POD:
		case MT_CHICKEN:
		case MT_HEAD:
		case MT_MINOTAUR:
		case MT_SORCERER1:
		case MT_SORCERER2:
			return(false);
		default:
			break;
	}
	x = actor.x;
	y = actor.y;
	z = actor.z;
	angle = actor.angle;
	ghost = actor.flags&MF_SHADOW;
	target = actor.target;
	P_SetMobjState(actor, S_FREETARGMOBJ);
	fog = P_SpawnMobj(x, y, z+TELEFOGHEIGHT, MT_TFOG);
	S_StartSound(fog, sfx_telept);
	chicken = P_SpawnMobj(x, y, z, MT_CHICKEN);
	chicken.special2 = moType;
	chicken.special1 = CHICKENTICS+P_Random();
	chicken.flags |= ghost;
	chicken.target = target;
	chicken.angle = angle;
	return(true);
}

//---------------------------------------------------------------------------
//
// FUNC P_AutoUseChaosDevice
//
//---------------------------------------------------------------------------

boolean P_AutoUseChaosDevice(player_t *player)
{
	int i;

	for(i = 0; i < player.inventorySlotNum; i++)
	{
		if(player.inventory[i].type == arti_teleport)
		{
			P_PlayerUseArtifact(player, arti_teleport);
			player.health = player.mo.health = (player.health+1)/2;
			return(true);
		}
	}
	return(false);
}
#endif

		//---------------------------------------------------------------------------
		//
		// PROC P_AutoUseHealth
		//
		//---------------------------------------------------------------------------

		public static void P_AutoUseHealth(DoomDef.player_t player, int saveHealth)
		{
			int i;
			int count;
			int normalCount;
			int normalSlot;
			int superCount;
			int superSlot;

#if DOS
	normalCount = superCount = 0;
	for(i = 0; i < player.inventorySlotNum; i++)
	{
		if(player.inventory[i].type == arti_health)
		{
			normalSlot = i;
			normalCount = player.inventory[i].count;
		}
		else if(player.inventory[i].type == arti_superhealth)
		{
			superSlot = i;
			superCount = player.inventory[i].count;
		}
	}
	if((gameskill == sk_baby) && (normalCount*25 >= saveHealth))
	{ // Use quartz flasks
		count = (saveHealth+24)/25;
		for(i = 0; i < count; i++)
		{
			player.health += 25;
			P_PlayerRemoveArtifact(player, normalSlot);
		}
	}
	else if(superCount*100 >= saveHealth)
	{ // Use mystic urns
		count = (saveHealth+99)/100;
		for(i = 0; i < count; i++)
		{
			player.health += 100;
			P_PlayerRemoveArtifact(player, superSlot);
		}
	}
	else if((gameskill == sk_baby)
		&& (superCount*100+normalCount*25 >= saveHealth))
	{ // Use mystic urns and quartz flasks
		count = (saveHealth+24)/25;
		saveHealth -= count*25;
		for(i = 0; i < count; i++)
		{
			player.health += 25;
			P_PlayerRemoveArtifact(player, normalSlot);
		}
		count = (saveHealth+99)/100;
		for(i = 0; i < count; i++)
		{
			player.health += 100;
			P_PlayerRemoveArtifact(player, normalSlot);
		}
	}
	player.mo.health = player.health;
#endif
		}


		/*
		=================
		=
		= P_DamageMobj
		=
		= Damages both enemies and players
		= inflictor is the thing that caused the damage
		= 		creature or missile, can be NULL (slime, etc)
		= source is the thing to target after taking damage
		=		creature or NULL
		= Source and inflictor are the same for melee attacks
		= source can be null for barrel explosions and other environmental stuff
		==================
		*/

		public static void P_DamageMobj
		(
			DoomDef.mobj_t target,
			DoomDef.mobj_t inflictor,
			DoomDef.mobj_t source,
			int damage
		)
		{
			uint ang;
			int saved;
			DoomDef.player_t player;
			int thrust;
			int temp;

			if ((target.flags & DoomDef.MF_SHOOTABLE) == 0)
			{
				// Shouldn't happen
				return;
			}
			if (target.health <= 0)
			{
				return;
			}
			if ((target.flags & DoomDef.MF_SKULLFLY) != 0)
			{
				if (target.type == info.mobjtype_t.MT_MINOTAUR)
				{ // Minotaur is invulnerable during charge attack
					return;
				}
				target.momx = target.momy = target.momz = 0;
			}
			player = target.player;
			if (player != null && g_game.gameskill == DoomDef.skill_t.sk_baby)
			{
				// Take half damage in trainer mode
				damage >>= 1;
			}
			// Special damage types
			if (inflictor != null)
			{
				switch (inflictor.type)
				{
					case info.mobjtype_t.MT_EGGFX:
						if (player != null)
						{
							//p_inter.P_ChickenMorphPlayer(player);
						}
						else
						{
							//p_inter.P_ChickenMorph(target);
						}
						return; // Always return
					case info.mobjtype_t.MT_WHIRLWIND:
						//P_TouchWhirlwind(target);
						return;
					case info.mobjtype_t.MT_MINOTAUR:
						if ((inflictor.flags & DoomDef.MF_SKULLFLY) != 0)
						{ // Slam only when in charge mode
							//P_MinotaurSlam(inflictor, target);
							return;
						}
						break;
					case info.mobjtype_t.MT_MACEFX4: // Death ball
						if ((target.flags2 & DoomDef.MF2_BOSS) != 0 || target.type == info.mobjtype_t.MT_HEAD)
						{ // Don't allow cheap boss kills
							break;
						}
						else if (target.player != null)
						{ // Player specific checks
							if (target.player.powers[(int)DoomDef.powertype_t.pw_invulnerability] != 0)
							{ // Can't hurt invulnerable players
								break;
							}
							//if(P_AutoUseChaosDevice(target.player))
							{ // Player was saved using chaos device
								return;
							}
						}
						damage = 10000; // Something's gonna die
						break;
					case info.mobjtype_t.MT_PHOENIXFX2: // Flame thrower
						if (target.player != null && m_misc.P_Random() < 128)
						{ // Freeze player for a bit
							target.reactiontime += 4;
						}
						break;
					case info.mobjtype_t.MT_RAINPLR1: // Rain missiles
					case info.mobjtype_t.MT_RAINPLR2:
					case info.mobjtype_t.MT_RAINPLR3:
					case info.mobjtype_t.MT_RAINPLR4:
						if ((target.flags2 & DoomDef.MF2_BOSS) != 0)
						{ // Decrease damage for bosses
							damage = (m_misc.P_Random() & 7) + 1;
						}
						break;
					case info.mobjtype_t.MT_HORNRODFX2:
					case info.mobjtype_t.MT_PHOENIXFX1:
						if (target.type == info.mobjtype_t.MT_SORCERER2 && m_misc.P_Random() < 96)
						{ // D'Sparil teleports away
							//p_enemy.P_DSparilTeleport(target);
							return;
						}
						break;
					case info.mobjtype_t.MT_BLASTERFX1:
					case info.mobjtype_t.MT_RIPPER:
						if (target.type == info.mobjtype_t.MT_HEAD)
						{ // Less damage to Ironlich bosses
							damage = m_misc.P_Random() & 1;
							if (damage == 0)
							{
								return;
							}
						}
						break;
					default:
						break;
				}
			}

			// Push the target unless source is using the gauntlets
			if (inflictor != null && (source == null || source.player == null
				|| source.player.readyweapon != DoomDef.weapontype_t.wp_gauntlets)
				&& (inflictor.flags2 & DoomDef.MF2_NODMGTHRUST) == 0)
			{
				ang = r_main.R_PointToAngle2(inflictor.x, inflictor.y,
					target.x, target.y);
				thrust = damage * (DoomDef.FRACUNIT >> 3) * 150 / target.infol.mass;
				// make fall forwards sometimes
				if ((damage < 40) && (damage > target.health)
					&& (target.z - inflictor.z > 64 * DoomDef.FRACUNIT) && (m_misc.P_Random() & 1) != 0)
				{
					ang += DoomDef.ANG180;
					thrust *= 4;
				}
				ang >>= (int)DoomDef.ANGLETOFINESHIFT;
				if (source != null && source.player != null && (source == inflictor)
					&& (source.player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2]) != 0
					&& source.player.readyweapon == DoomDef.weapontype_t.wp_staff)
				{
					// Staff power level 2
					target.momx += DoomDef.FixedMul(10 * DoomDef.FRACUNIT, r_main.finecosine(ang));
					target.momy += DoomDef.FixedMul(10 * DoomDef.FRACUNIT, tables.finesine[ang]);
					if ((target.flags & DoomDef.MF_NOGRAVITY) == 0)
					{
						target.momz += 5 * DoomDef.FRACUNIT;
					}
				}
				else
				{
					target.momx += DoomDef.FixedMul(thrust, r_main.finecosine(ang));
					target.momy += DoomDef.FixedMul(thrust, tables.finesine[ang]);
				}
			}

			//
			// player specific
			//
			if (player != null)
			{
				if (damage < 1000 && ((player.cheats & DoomDef.CF_GODMODE) != 0
					|| player.powers[(int)DoomDef.powertype_t.pw_invulnerability] != 0))
				{
					return;
				}
				if (player.armortype != 0)
				{
					if (player.armortype == 1)
					{
						saved = damage >> 1;
					}
					else
					{
						saved = (damage >> 1) + (damage >> 2);
					}
					if (player.armorpoints <= saved)
					{
						// armor is used up
						saved = player.armorpoints;
						player.armortype = 0;
					}
					player.armorpoints -= saved;
					damage -= saved;
				}
				if (damage >= player.health
					&& ((g_game.gameskill == DoomDef.skill_t.sk_baby) || g_game.deathmatch)
					&& player.chickenTics == 0)
				{ // Try to use some inventory health
					P_AutoUseHealth(player, damage - player.health + 1);
				}
				player.health -= damage; // mirror mobj health here for Dave
				if (player.health < 0)
				{
					player.health = 0;
				}
				player.attacker = source;
				player.damagecount += damage; // add damage after armor / invuln
				if (player.damagecount > 100)
				{
					player.damagecount = 100; // teleport stomp does 10k points...
				}
				temp = damage < 100 ? damage : 100;
				if (player == g_game.players[g_game.consoleplayer])
				{
					//	i_cyber.I_Tactile(40, 10, 40+temp*2);
					sb_bar.SB_PaletteFlash();
				}
			}

			//
			// do the damage
			//
			target.health -= damage;
			if (target.health <= 0)
			{ // Death
				target.special1 = damage;
				if (target.type == info.mobjtype_t.MT_POD && source != null && source.type != info.mobjtype_t.MT_POD)
				{ // Make sure players get frags for chain-reaction kills
					target.target = source;
				}
				if (player != null && inflictor != null && player.chickenTics == 0)
				{ // Check for flame death
					if ((inflictor.flags2 & DoomDef.MF2_FIREDAMAGE) != 0
						|| ((inflictor.type == info.mobjtype_t.MT_PHOENIXFX1)
						&& (target.health > -50) && (damage > 25)))
					{
						target.flags2 |= DoomDef.MF2_FIREDAMAGE;
					}
				}
				p_inter.P_KillMobj(source, target);
				return;
			}
			if ((m_misc.P_Random() < target.infol.painchance)
				&& (target.flags & DoomDef.MF_SKULLFLY) == 0)
			{
				target.flags |= DoomDef.MF_JUSTHIT; // fight back!
				p_mobj.P_SetMobjState(target, (info.statenum_t)target.infol.painstate);
			}
			target.reactiontime = 0; // we're awake now...
			if (target.threshold == 0 && source != null && (source.flags2 & DoomDef.MF2_BOSS) == 0
				&& !(target.type == info.mobjtype_t.MT_SORCERER2 && source.type == info.mobjtype_t.MT_WIZARD))
			{
				// Target actor is not intent on another actor,
				// so make him chase after source
				target.target = source;
				target.threshold = p_local.BASETHRESHOLD;
				if (target.state == info.states[target.infol.spawnstate]
					&& target.infol.seestate != (int)info.statenum_t.S_NULL)
				{
					p_mobj.P_SetMobjState(target, (info.statenum_t)target.infol.seestate);
				}
			}
		}
	}
}
