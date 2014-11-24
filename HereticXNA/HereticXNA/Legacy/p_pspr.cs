using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// P_pspr.c

namespace HereticXNA
{
	class p_pspr
	{

		// Macros

		public const int LOWERSPEED = DoomDef.FRACUNIT * 6;
		public const int RAISESPEED = DoomDef.FRACUNIT * 6;

		public const int WEAPONBOTTOM = 128 * DoomDef.FRACUNIT;
		public const int WEAPONTOP = 32 * DoomDef.FRACUNIT;
		public const int FLAME_THROWER_TICS = 10 * 35;
		public const int MAGIC_JUNK = 1234;
		public const int MAX_MACE_SPOTS = 8;
		static int MaceSpotCount;
		class sMaceSpots
		{
			public int x;
			public int y;
		}
		static sMaceSpots[] MaceSpots = new sMaceSpots[MAX_MACE_SPOTS]{
	new sMaceSpots(),
	new sMaceSpots(),
	new sMaceSpots(),
	new sMaceSpots(),
	new sMaceSpots(),
	new sMaceSpots(),
	new sMaceSpots(),
	new sMaceSpots()
};

		public static int bulletslope;

		static int[] WeaponAmmoUsePL1 = new int[(int)DoomDef.weapontype_t.NUMWEAPONS] {
	0,					// staff
	p_local.USE_GWND_AMMO_1,	// gold wand
	p_local.USE_CBOW_AMMO_1,	// crossbow
	p_local.USE_BLSR_AMMO_1,	// blaster
	p_local.USE_SKRD_AMMO_1,	// skull rod
	p_local.USE_PHRD_AMMO_1,	// phoenix rod
	p_local.USE_MACE_AMMO_1,	// mace
	0,					// gauntlets
	0					// beak
};

		static int[] WeaponAmmoUsePL2 = new int[(int)DoomDef.weapontype_t.NUMWEAPONS] {
	0,					// staff
	p_local.USE_GWND_AMMO_2,	// gold wand
	p_local.USE_CBOW_AMMO_2,	// crossbow
	p_local.USE_BLSR_AMMO_2,	// blaster
	p_local.USE_SKRD_AMMO_2,	// skull rod
	p_local.USE_PHRD_AMMO_2,	// phoenix rod
	p_local.USE_MACE_AMMO_2,	// mace
	0,					// gauntlets
	0					// beak
};

		public static DoomDef.weaponinfo_t[] wpnlev1info = new DoomDef.weaponinfo_t[(int)DoomDef.weapontype_t.NUMWEAPONS]
{
	new DoomDef.weaponinfo_t{ // Staff
		ammo = DoomDef.ammotype_t.am_noammo,			// ammo
		upstate=(int)info.statenum_t.S_STAFFUP,			// upstate
		downstate=(int)info.statenum_t.S_STAFFDOWN,		// downstate
		readystate=(int)info.statenum_t.S_STAFFREADY,		// readystate
		atkstate=(int)info.statenum_t.S_STAFFATK1_1,		// atkstate
		holdatkstate=(int)info.statenum_t.S_STAFFATK1_1,		// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Gold wand
		ammo = DoomDef.ammotype_t.am_goldwand,		// ammo
		upstate=(int)info.statenum_t.S_GOLDWANDUP,		// upstate
		downstate=(int)info.statenum_t.S_GOLDWANDDOWN,		// downstate
		readystate=(int)info.statenum_t.S_GOLDWANDREADY,	// readystate
		atkstate=(int)info.statenum_t.S_GOLDWANDATK1_1,	// atkstate
		holdatkstate=(int)info.statenum_t.S_GOLDWANDATK1_1,	// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Crossbow
		ammo = DoomDef.ammotype_t.am_crossbow,		// ammo
		upstate=(int)info.statenum_t.S_CRBOWUP,			// upstate
		downstate=(int)info.statenum_t.S_CRBOWDOWN,		// downstate
		readystate=(int)info.statenum_t.S_CRBOW1,			// readystate
		atkstate=(int)info.statenum_t.S_CRBOWATK1_1,		// atkstate
		holdatkstate=(int)info.statenum_t.S_CRBOWATK1_1,		// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Blaster
		ammo = DoomDef.ammotype_t.am_blaster,			// ammo
		upstate=(int)info.statenum_t.S_BLASTERUP,		// upstate
		downstate=(int)info.statenum_t.S_BLASTERDOWN,		// downstate
		readystate=(int)info.statenum_t.S_BLASTERREADY,		// readystate
		atkstate=(int)info.statenum_t.S_BLASTERATK1_1,	// atkstate
		holdatkstate=(int)info.statenum_t.S_BLASTERATK1_3,	// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Skull rod
		ammo = DoomDef.ammotype_t.am_skullrod,		// ammo
		upstate=(int)info.statenum_t.S_HORNRODUP,		// upstate
		downstate=(int)info.statenum_t.S_HORNRODDOWN,		// downstate
		readystate=(int)info.statenum_t.S_HORNRODREADY,		// readystae
		atkstate=(int)info.statenum_t.S_HORNRODATK1_1,	// atkstate
		holdatkstate=(int)info.statenum_t.S_HORNRODATK1_1,	// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Phoenix rod
		ammo = DoomDef.ammotype_t.am_phoenixrod,		// ammo
		upstate=(int)info.statenum_t.S_PHOENIXUP,		// upstate
		downstate=(int)info.statenum_t.S_PHOENIXDOWN,		// downstate
		readystate=(int)info.statenum_t.S_PHOENIXREADY,		// readystate
		atkstate=(int)info.statenum_t.S_PHOENIXATK1_1,	// atkstate
		holdatkstate=(int)info.statenum_t.S_PHOENIXATK1_1,	// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Mace
		ammo = DoomDef.ammotype_t.am_mace,			// ammo
		upstate=(int)info.statenum_t.S_MACEUP,			// upstate
		downstate=(int)info.statenum_t.S_MACEDOWN,			// downstate
		readystate=(int)info.statenum_t.S_MACEREADY,		// readystate
		atkstate=(int)info.statenum_t.S_MACEATK1_1,		// atkstate
		holdatkstate=(int)info.statenum_t.S_MACEATK1_2,		// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Gauntlets
		ammo = DoomDef.ammotype_t.am_noammo,			// ammo
		upstate=(int)info.statenum_t.S_GAUNTLETUP,		// upstate
		downstate=(int)info.statenum_t.S_GAUNTLETDOWN,		// downstate
		readystate=(int)info.statenum_t.S_GAUNTLETREADY,	// readystate
		atkstate=(int)info.statenum_t.S_GAUNTLETATK1_1,	// atkstate
		holdatkstate=(int)info.statenum_t.S_GAUNTLETATK1_3,	// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Beak
		ammo = DoomDef.ammotype_t.am_noammo,			// ammo
		upstate=(int)info.statenum_t.S_BEAKUP,			// upstate
		downstate=(int)info.statenum_t.S_BEAKDOWN,			// downstate
		readystate=(int)info.statenum_t.S_BEAKREADY,		// readystate
		atkstate=(int)info.statenum_t.S_BEAKATK1_1,		// atkstate
		holdatkstate=(int)info.statenum_t.S_BEAKATK1_1,		// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	}
};

		public static DoomDef.weaponinfo_t[] wpnlev2info = new DoomDef.weaponinfo_t[(int)DoomDef.weapontype_t.NUMWEAPONS]
{
	new DoomDef.weaponinfo_t{ // Staff
		ammo = DoomDef.ammotype_t.am_noammo,			// ammo
		upstate=(int)info.statenum_t.S_STAFFUP2,			// upstate
		downstate=(int)info.statenum_t.S_STAFFDOWN2,		// downstate
		readystate=(int)info.statenum_t.S_STAFFREADY2_1,	// readystate
		atkstate=(int)info.statenum_t.S_STAFFATK2_1,		// atkstate
		holdatkstate=(int)info.statenum_t.S_STAFFATK2_1,		// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Gold wand
		ammo = DoomDef.ammotype_t.am_goldwand,		// ammo
		upstate=(int)info.statenum_t.S_GOLDWANDUP,		// upstate
		downstate=(int)info.statenum_t.S_GOLDWANDDOWN,		// downstate
		readystate=(int)info.statenum_t.S_GOLDWANDREADY,	// readystate
		atkstate=(int)info.statenum_t.S_GOLDWANDATK2_1,	// atkstate
		holdatkstate=(int)info.statenum_t.S_GOLDWANDATK2_1,	// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Crossbow
		ammo = DoomDef.ammotype_t.am_crossbow,		// ammo
		upstate=(int)info.statenum_t.S_CRBOWUP,			// upstate
		downstate=(int)info.statenum_t.S_CRBOWDOWN,		// downstate
		readystate=(int)info.statenum_t.S_CRBOW1,			// readystate
		atkstate=(int)info.statenum_t.S_CRBOWATK2_1,		// atkstate
		holdatkstate=(int)info.statenum_t.S_CRBOWATK2_1,		// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Blaster
		ammo = DoomDef.ammotype_t.am_blaster,			// ammo
		upstate=(int)info.statenum_t.S_BLASTERUP,		// upstate
		downstate=(int)info.statenum_t.S_BLASTERDOWN,		// downstate
		readystate=(int)info.statenum_t.S_BLASTERREADY,		// readystate
		atkstate=(int)info.statenum_t.S_BLASTERATK2_1,	// atkstate
		holdatkstate=(int)info.statenum_t.S_BLASTERATK2_3,	// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Skull rod
		ammo = DoomDef.ammotype_t.am_skullrod,		// ammo
		upstate=(int)info.statenum_t.S_HORNRODUP,		// upstate
		downstate=(int)info.statenum_t.S_HORNRODDOWN,		// downstate
		readystate=(int)info.statenum_t.S_HORNRODREADY,		// readystae
		atkstate=(int)info.statenum_t.S_HORNRODATK2_1,	// atkstate
		holdatkstate=(int)info.statenum_t.S_HORNRODATK2_1,	// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Phoenix rod
		ammo = DoomDef.ammotype_t.am_phoenixrod,		// ammo
		upstate=(int)info.statenum_t.S_PHOENIXUP,		// upstate
		downstate=(int)info.statenum_t.S_PHOENIXDOWN,		// downstate
		readystate=(int)info.statenum_t.S_PHOENIXREADY,		// readystate
		atkstate=(int)info.statenum_t.S_PHOENIXATK2_1,	// atkstate
		holdatkstate=(int)info.statenum_t.S_PHOENIXATK2_2,	// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Mace
		ammo = DoomDef.ammotype_t.am_mace,			// ammo
		upstate=(int)info.statenum_t.S_MACEUP,			// upstate
		downstate=(int)info.statenum_t.S_MACEDOWN,			// downstate
		readystate=(int)info.statenum_t.S_MACEREADY,		// readystate
		atkstate=(int)info.statenum_t.S_MACEATK2_1,		// atkstate
		holdatkstate=(int)info.statenum_t.S_MACEATK2_1,		// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Gauntlets
		ammo = DoomDef.ammotype_t.am_noammo,			// ammo
		upstate=(int)info.statenum_t.S_GAUNTLETUP2,		// upstate
		downstate=(int)info.statenum_t.S_GAUNTLETDOWN2,	// downstate
		readystate=(int)info.statenum_t.S_GAUNTLETREADY2_1,	// readystate
		atkstate=(int)info.statenum_t.S_GAUNTLETATK2_1,	// atkstate
		holdatkstate=(int)info.statenum_t.S_GAUNTLETATK2_3,	// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	},
	new DoomDef.weaponinfo_t{ // Beak
		ammo = DoomDef.ammotype_t.am_noammo,			// ammo
		upstate=(int)info.statenum_t.S_BEAKUP,			// upstate
		downstate=(int)info.statenum_t.S_BEAKDOWN,			// downstate
		readystate=(int)info.statenum_t.S_BEAKREADY,		// readystate
		atkstate=(int)info.statenum_t.S_BEAKATK2_1,		// atkstate
		holdatkstate=(int)info.statenum_t.S_BEAKATK2_1,		// holdatkstate
		flashstate=(int)info.statenum_t.S_NULL				// flashstate
	}
};

		//---------------------------------------------------------------------------
		//
		// PROC P_OpenWeapons
		//
		// Called at level load before things are loaded.
		//
		//---------------------------------------------------------------------------

		public static void P_OpenWeapons()
		{
			MaceSpotCount = 0;
		}
		//---------------------------------------------------------------------------
		//
		// PROC P_AddMaceSpot
		//
		//---------------------------------------------------------------------------

		public static void P_AddMaceSpot(DoomData.mapthing_t mthing)
		{
			if (MaceSpotCount == MAX_MACE_SPOTS)
			{
				i_ibm.I_Error("Too many mace spots.");
			}
			MaceSpots[MaceSpotCount].x = mthing.x << DoomDef.FRACBITS;
			MaceSpots[MaceSpotCount].y = mthing.y << DoomDef.FRACBITS;
			MaceSpotCount++;
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_RepositionMace
		//
		// Chooses the next spot to place the mace.
		//
		//---------------------------------------------------------------------------

		public static void P_RepositionMace(DoomDef.mobj_t mo)
		{
			int spot;
			r_local.subsector_t ss;

			p_maputl.P_UnsetThingPosition(mo);
			spot = m_misc.P_Random() % MaceSpotCount;
			mo.x = MaceSpots[spot].x;
			mo.y = MaceSpots[spot].y;
			ss = r_main.R_PointInSubsector(mo.x, mo.y);
			mo.z = mo.floorz = ss.sector.floorheight;
			mo.ceilingz = ss.sector.ceilingheight;
			p_maputl.P_SetThingPosition(mo);
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_CloseWeapons
		//
		// Called at level load after things are loaded.
		//
		//---------------------------------------------------------------------------

		public static void P_CloseWeapons()
		{
			int spot;

			if (MaceSpotCount == 0)
			{ // No maces placed
				return;
			}
			if (!g_game.deathmatch && m_misc.P_Random() < 64)
			{ // Sometimes doesn't show up if not in deathmatch
				return;
			}
			spot = m_misc.P_Random() % MaceSpotCount;
			p_mobj.P_SpawnMobj(MaceSpots[spot].x, MaceSpots[spot].y, p_local.ONFLOORZ, info.mobjtype_t.MT_WMACE);
		}
		//---------------------------------------------------------------------------
		//
		// PROC P_SetPsprite
		//
		//---------------------------------------------------------------------------

		public static void P_SetPsprite(DoomDef.player_t player, int position, info.statenum_t stnum)
		{
			DoomDef.pspdef_t psp;
			info.state_t state;

			psp = player.psprites[position];
			do
			{
				if (stnum == info.statenum_t.S_NULL)
				{ // Object removed itself.
					psp.state = null;
					break;
				}
				state = info.states[(int)stnum];
				psp.state = state;
				psp.tics = (int)state.tics; // could be 0
				if (state.misc1 != 0)
				{ // Set coordinates.
					psp.sx = (int)(state.misc1 << DoomDef.FRACBITS);
					psp.sy = (int)(state.misc2 << DoomDef.FRACBITS);
				}
				if (state.action != null)
				{ // Call action routine.
					state.action.action(player, psp);
					if (psp.state == null)
					{
						break;
					}
				}
				stnum = psp.state.nextstate;
			} while (psp.tics == 0); // An initial state of 0 could cycle through.
		}

		/*
		=================
		=
		= P_CalcSwing
		=
		=================
		*/


		//---------------------------------------------------------------------------
		//
		// PROC P_ActivateBeak
		//
		//---------------------------------------------------------------------------

		public static void P_ActivateBeak(DoomDef.player_t player)
		{
			player.pendingweapon = DoomDef.weapontype_t.wp_nochange;
			player.readyweapon = DoomDef.weapontype_t.wp_beak;
			player.psprites[(int)DoomDef.psprnum_t.ps_weapon].sy = WEAPONTOP;
			P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon, info.statenum_t.S_BEAKREADY);
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_PostChickenWeapon
		//
		//---------------------------------------------------------------------------

		public static void P_PostChickenWeapon(DoomDef.player_t player, DoomDef.weapontype_t weapon)
		{
			if (weapon == DoomDef.weapontype_t.wp_beak)
			{ // Should never happen
				weapon = DoomDef.weapontype_t.wp_staff;
			}
			player.pendingweapon = DoomDef.weapontype_t.wp_nochange;
			player.readyweapon = weapon;
			player.psprites[(int)DoomDef.psprnum_t.ps_weapon].sy = WEAPONBOTTOM;
			P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon, (info.statenum_t)wpnlev1info[(int)weapon].upstate);
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_BringUpWeapon
		//
		// Starts bringing the pending weapon up from the bottom of the screen.
		//
		//---------------------------------------------------------------------------

		public static void P_BringUpWeapon(DoomDef.player_t player)
		{
			info.statenum_t newW;

			if (player.pendingweapon == DoomDef.weapontype_t.wp_nochange)
			{
				player.pendingweapon = player.readyweapon;
			}
			if (player.pendingweapon == DoomDef.weapontype_t.wp_gauntlets)
			{
				i_ibm.S_StartSound(player.mo, (int)sounds.sfxenum_t.sfx_gntact);
			}
			if (player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0)
			{
				newW = (info.statenum_t)wpnlev2info[(int)player.pendingweapon].upstate;
			}
			else
			{
				newW = (info.statenum_t)wpnlev1info[(int)player.pendingweapon].upstate;
			}
			player.pendingweapon = DoomDef.weapontype_t.wp_nochange;
			player.psprites[(int)DoomDef.psprnum_t.ps_weapon].sy = WEAPONBOTTOM;
			P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon, newW);
		}

		//---------------------------------------------------------------------------
		//
		// FUNC P_CheckAmmo
		//
		// Returns true if there is enough ammo to shoot.  If not, selects the
		// next weapon to use.
		//
		//---------------------------------------------------------------------------

		public static bool P_CheckAmmo(DoomDef.player_t player)
		{
			DoomDef.ammotype_t ammo;
			int[] ammoUse;
			int count;

			ammo = wpnlev1info[(int)player.readyweapon].ammo;
			if (player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0 && !g_game.deathmatch)
			{
				ammoUse = WeaponAmmoUsePL2;
			}
			else
			{
				ammoUse = WeaponAmmoUsePL1;
			}
			count = ammoUse[(int)player.readyweapon];
			if (ammo == DoomDef.ammotype_t.am_noammo || player.ammo[(int)ammo] >= count)
			{
				return (true);
			}
			// out of ammo, pick a weapon to change to
			do
			{
				if (player.weaponowned[(int)DoomDef.weapontype_t.wp_skullrod]
					&& player.ammo[(int)DoomDef.ammotype_t.am_skullrod] > ammoUse[(int)DoomDef.weapontype_t.wp_skullrod])
				{
					player.pendingweapon = DoomDef.weapontype_t.wp_skullrod;
				}
				else if (player.weaponowned[(int)DoomDef.weapontype_t.wp_blaster]
					&& player.ammo[(int)DoomDef.ammotype_t.am_blaster] > ammoUse[(int)DoomDef.weapontype_t.wp_blaster])
				{
					player.pendingweapon = DoomDef.weapontype_t.wp_blaster;
				}
				else if (player.weaponowned[(int)DoomDef.weapontype_t.wp_crossbow]
					&& player.ammo[(int)DoomDef.ammotype_t.am_crossbow] > ammoUse[(int)DoomDef.weapontype_t.wp_crossbow])
				{
					player.pendingweapon = DoomDef.weapontype_t.wp_crossbow;
				}
				else if (player.weaponowned[(int)DoomDef.weapontype_t.wp_mace]
					&& player.ammo[(int)DoomDef.ammotype_t.am_mace] > ammoUse[(int)DoomDef.weapontype_t.wp_mace])
				{
					player.pendingweapon = DoomDef.weapontype_t.wp_mace;
				}
				else if (player.ammo[(int)DoomDef.ammotype_t.am_goldwand] > ammoUse[(int)DoomDef.weapontype_t.wp_goldwand])
				{
					player.pendingweapon = DoomDef.weapontype_t.wp_goldwand;
				}
				else if (player.weaponowned[(int)DoomDef.weapontype_t.wp_gauntlets])
				{
					player.pendingweapon = DoomDef.weapontype_t.wp_gauntlets;
				}
				else if (player.weaponowned[(int)DoomDef.weapontype_t.wp_phoenixrod]
					&& player.ammo[(int)DoomDef.ammotype_t.am_phoenixrod] > ammoUse[(int)DoomDef.weapontype_t.wp_phoenixrod])
				{
					player.pendingweapon = DoomDef.weapontype_t.wp_phoenixrod;
				}
				else
				{
					player.pendingweapon = DoomDef.weapontype_t.wp_staff;
				}
			} while (player.pendingweapon == DoomDef.weapontype_t.wp_nochange);
			if (player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0)
			{
				P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon,
					(info.statenum_t)wpnlev2info[(int)player.readyweapon].downstate);
			}
			else
			{
				P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon,
					(info.statenum_t)wpnlev1info[(int)player.readyweapon].downstate);
			}
			return (false);
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_FireWeapon
		//
		//---------------------------------------------------------------------------

		public static void P_FireWeapon(DoomDef.player_t player)
		{
			DoomDef.weaponinfo_t wpinfo;
			DoomDef.weaponinfo_t[] wpinfot;
			info.statenum_t attackState;

			if (!P_CheckAmmo(player))
			{
				return;
			}
			p_mobj.P_SetMobjState(player.mo, info.statenum_t.S_PLAY_ATK2);
			wpinfot = player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0 ?
				wpnlev2info :
				wpnlev1info;
			wpinfo = player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0 ?
				wpnlev2info[0] :
				wpnlev1info[0];
			attackState = (info.statenum_t)(player.refire != 0 ?
				wpinfot[(int)player.readyweapon].holdatkstate :
				wpinfot[(int)player.readyweapon].atkstate);
			P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon, attackState);
			p_enemy.P_NoiseAlert(player.mo, player.mo);
			if (player.readyweapon == DoomDef.weapontype_t.wp_gauntlets && player.refire == 0)
			{ // Play the sound for the initial gauntlet attack
				i_ibm.S_StartSound(player.mo, (int)sounds.sfxenum_t.sfx_gntuse);
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_DropWeapon
		//
		// The player died, so put the weapon away.
		//
		//---------------------------------------------------------------------------

		public static void P_DropWeapon(DoomDef.player_t player)
		{
			if (player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0)
			{
				P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon,
					(info.statenum_t)wpnlev2info[(int)player.readyweapon].downstate);
			}
			else
			{
				P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon,
					(info.statenum_t)wpnlev1info[(int)player.readyweapon].downstate);
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC A_WeaponReady
		//
		// The player can fire the weapon or change to another weapon at this time.
		//
		//---------------------------------------------------------------------------
		public class A_WeaponReady : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				int angle;

				if (player.chickenTics != 0)
				{ // Change to the chicken beak
					P_ActivateBeak(player);
					return;
				}
				//// Change player from attack state
				if (player.mo.state == info.states[(int)info.statenum_t.S_PLAY_ATK1]
					|| player.mo.state == info.states[(int)info.statenum_t.S_PLAY_ATK2])
				{
					p_mobj.P_SetMobjState(player.mo, info.statenum_t.S_PLAY);
				}
				//// Check for staff PL2 active sound
				if ((player.readyweapon == DoomDef.weapontype_t.wp_staff)
					&& (psp.state == info.states[(int)info.statenum_t.S_STAFFREADY2_1])
					&& m_misc.P_Random() < 128)
				{
					i_ibm.S_StartSound(player.mo, (int)(sounds.sfxenum_t.sfx_stfcrk));
				}
				//// Put the weapon away if the player has a pending weapon or has
				//// died.
				if (player.pendingweapon != DoomDef.weapontype_t.wp_nochange || player.health == 0)
				{
					if (player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0)
					{
						P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon,
							(info.statenum_t)wpnlev2info[(int)player.readyweapon].downstate);
					}
					else
					{
						P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon,
							(info.statenum_t)wpnlev1info[(int)player.readyweapon].downstate);
					}
					return;
				}

				//// Check for fire.  The phoenix rod does not auto fire.
				if ((player.cmd.buttons & DoomDef.BT_ATTACK) != 0)
				{
					if (player.attackdown == 0 || (player.readyweapon != DoomDef.weapontype_t.wp_phoenixrod))
					{
						player.attackdown = 1;
						P_FireWeapon(player);
						return;
					}
				}
				else
				{
					player.attackdown = 0;
				}

				// Bob the weapon based on movement speed.
				angle = (int)((128 * p_tick.leveltime) & DoomDef.FINEMASK);
				psp.sx = DoomDef.FRACUNIT + DoomDef.FixedMul(player.bob, r_main.finecosine((uint)angle));
				angle &= (int)(DoomDef.FINEANGLES / 2 - 1);
				psp.sy = WEAPONTOP + DoomDef.FixedMul(player.bob, tables.finesine[angle]);
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_UpdateBeak
		//
		//---------------------------------------------------------------------------

		public static void P_UpdateBeak(DoomDef.player_t player, DoomDef.pspdef_t psp)
		{
			psp.sy = WEAPONTOP + (player.chickenPeck << (DoomDef.FRACBITS - 1));
		}

		//---------------------------------------------------------------------------
		//
		// PROC A_BeakReady
		//
		//---------------------------------------------------------------------------
		public class A_BeakReady : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				if ((player.cmd.buttons & DoomDef.BT_ATTACK) != 0)
				{ // Chicken beak attack
					player.attackdown = 1;
					p_mobj.P_SetMobjState(player.mo, info.statenum_t.S_CHICPLAY_ATK1);
					if (player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0)
					{
						P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon, info.statenum_t.S_BEAKATK2_1);
					}
					else
					{
						P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon, info.statenum_t.S_BEAKATK1_1);
					}
					p_enemy.P_NoiseAlert(player.mo, player.mo);
				}
				else
				{
					if (player.mo.state == info.states[(int)info.statenum_t.S_CHICPLAY_ATK1])
					{ // Take out of attack state
						p_mobj.P_SetMobjState(player.mo, info.statenum_t.S_CHICPLAY);
					}
					player.attackdown = 0;
				}
			}
		}


		//---------------------------------------------------------------------------
		//
		// PROC A_ReFire
		//
		// The player can re fire the weapon without lowering it entirely.
		//
		//---------------------------------------------------------------------------

		public class A_ReFire : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				if ((player.cmd.buttons & DoomDef.BT_ATTACK) != 0
					&& player.pendingweapon == DoomDef.weapontype_t.wp_nochange && player.health != 0)
				{
					player.refire++;
					P_FireWeapon(player);
				}
				else
				{
					player.refire = 0;
					P_CheckAmmo(player);
				}
			}
		}


		//---------------------------------------------------------------------------
		//
		// PROC A_Lower
		//
		//---------------------------------------------------------------------------
		public class A_Lower : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				if (player.chickenTics != 0)
				{
					psp.sy = WEAPONBOTTOM;
				}
				else
				{
					psp.sy += LOWERSPEED;
				}
				if (psp.sy < WEAPONBOTTOM)
				{ // Not lowered all the way yet
					return;
				}
				if (player.playerstate == DoomDef.playerstate_t.PST_DEAD)
				{ // Player is dead, so don't bring up a pending weapon
					psp.sy = WEAPONBOTTOM;
					return;
				}
				if (player.health == 0)
				{ // Player is dead, so keep the weapon off screen
					P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon, info.statenum_t.S_NULL);
					return;
				}
				player.readyweapon = player.pendingweapon;
				P_BringUpWeapon(player);
			}
		}



		//---------------------------------------------------------------------------
		//
		// PROC A_BeakRaise
		//
		//---------------------------------------------------------------------------

		public class A_BeakRaise : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				psp.sy = WEAPONTOP;
				P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon,
					(info.statenum_t)wpnlev1info[(int)player.readyweapon].readystate);
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC A_Raise
		//
		//---------------------------------------------------------------------------
		public class A_Raise : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				psp.sy -= RAISESPEED;
				if (psp.sy > WEAPONTOP)
				{ // Not raised all the way yet
					return;
				}
				psp.sy = WEAPONTOP;
				if (player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0)
				{
					P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon,
						(info.statenum_t)wpnlev2info[(int)player.readyweapon].readystate);
				}
				else
				{
					P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon,
						(info.statenum_t)wpnlev1info[(int)player.readyweapon].readystate);
				}
			}
		}



		/*
		===============
		=
		= P_BulletSlope
		=
		= Sets a slope so a near miss is at aproximately the height of the
		= intended target
		=
		===============
		*/

		public static void P_BulletSlope(DoomDef.mobj_t mo)
		{
			uint an;

			//
			// see which target is to be aimed at
			//
			an = mo.angle;
			bulletslope = p_map.P_AimLineAttack(mo, an, 16 * 64 * DoomDef.FRACUNIT);
			if (p_map.linetarget == null)
			{
				an += 1 << 26;
				bulletslope = p_map.P_AimLineAttack(mo, an, 16 * 64 * DoomDef.FRACUNIT);
				if (p_map.linetarget == null)
				{
					an -= 2 << 26;
					bulletslope = p_map.P_AimLineAttack(mo, an, 16 * 64 * DoomDef.FRACUNIT);
				}
				if (p_map.linetarget == null)
				{
					an += 2 << 26;
					bulletslope = (mo.player.lookdir << DoomDef.FRACBITS) / 173;
				}
			}
		}

		//****************************************************************************
		//
		// WEAPON ATTACKS
		//
		//****************************************************************************

		//----------------------------------------------------------------------------
		//
		// PROC A_BeakAttackPL1
		//
		//----------------------------------------------------------------------------
		public class A_BeakAttackPL1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				//uint angle;
				//int damage;
				//int slope;

				//damage = 1 + (P_Random() & 3);
				//angle = player.mo.angle;
				//slope = P_AimLineAttack(player.mo, angle, MELEERANGE);
				//PuffType = MT_BEAKPUFF;
				//P_LineAttack(player.mo, angle, MELEERANGE, slope, damage);
				//if (linetarget)
				//{
				//    player.mo.angle = R_PointToAngle2(player.mo.x,
				//        player.mo.y, linetarget.x, linetarget.y);
				//}
				//S_StartSound(player.mo, sfx_chicpk1 + (P_Random() % 3));
				//player.chickenPeck = 12;
				//psp.tics -= P_Random() & 7;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_BeakAttackPL2
		//
		//----------------------------------------------------------------------------
		public class A_BeakAttackPL2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				//uint angle;
				//int damage;
				//int slope;

				//damage = HITDICE(4);
				//angle = player.mo.angle;
				//slope = P_AimLineAttack(player.mo, angle, MELEERANGE);
				//PuffType = MT_BEAKPUFF;
				//P_LineAttack(player.mo, angle, MELEERANGE, slope, damage);
				//if (linetarget)
				//{
				//    player.mo.angle = R_PointToAngle2(player.mo.x,
				//        player.mo.y, linetarget.x, linetarget.y);
				//}
				//S_StartSound(player.mo, sfx_chicpk1 + (P_Random() % 3));
				//player.chickenPeck = 12;
				//psp.tics -= P_Random() & 3;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_StaffAttackPL1
		//
		//----------------------------------------------------------------------------

		public class A_StaffAttackPL1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				uint angle;
				int damage;
				int slope;

				damage = 5 + (m_misc.P_Random() & 15);
				angle = player.mo.angle;
				angle += (uint)((m_misc.P_Random() - m_misc.P_Random()) << 18);
				slope = p_map.P_AimLineAttack(player.mo, angle, p_local.MELEERANGE);
				p_mobj.PuffType = info.mobjtype_t.MT_STAFFPUFF;
				p_map.P_LineAttack(player.mo, angle, p_local.MELEERANGE, slope, damage);
				if (p_map.linetarget != null)
				{
					//S_StartSound(player.mo, sfx_stfhit);
					// turn to face target
					player.mo.angle = r_main.R_PointToAngle2(player.mo.x,
						player.mo.y, p_map.linetarget.x, p_map.linetarget.y);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_StaffAttackPL2
		//
		//----------------------------------------------------------------------------
		public class A_StaffAttackPL2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				//uint angle;
				//int damage;
				//int slope;

				//// P_inter.c:P_DamageMobj() handles target momentums
				//damage = 18 + (P_Random() & 63);
				//angle = player.mo.angle;
				//angle += (P_Random() - P_Random()) << 18;
				//slope = P_AimLineAttack(player.mo, angle, MELEERANGE);
				//PuffType = MT_STAFFPUFF2;
				//P_LineAttack(player.mo, angle, MELEERANGE, slope, damage);
				//if (linetarget)
				//{
				//    //S_StartSound(player.mo, sfx_stfpow);
				//    // turn to face target
				//    player.mo.angle = R_PointToAngle2(player.mo.x,
				//        player.mo.y, linetarget.x, linetarget.y);
				//}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FireBlasterPL1
		//
		//----------------------------------------------------------------------------
		public class A_FireBlasterPL1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				DoomDef.mobj_t mo;
				uint angle;
				int damage;

				mo = player.mo;
				i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_gldhit);
				player.ammo[(int)DoomDef.ammotype_t.am_blaster] -= p_local.USE_BLSR_AMMO_1;
				P_BulletSlope(mo);
				damage = DoomDef.HITDICE(4);
				angle = mo.angle;
				if (player.refire != 0)
				{
					angle += (uint)((m_misc.P_Random() - m_misc.P_Random()) << 18);
				}
				p_mobj.PuffType = info.mobjtype_t.MT_BLASTERPUFF1;
				p_map.P_LineAttack(mo, angle, p_local.MISSILERANGE, bulletslope, damage);
				i_ibm.S_StartSound(player.mo, (int)sounds.sfxenum_t.sfx_blssht);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FireBlasterPL2
		//
		//----------------------------------------------------------------------------
		public class A_FireBlasterPL2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				//mobj_t* mo;

				//player.ammo[am_blaster] -=
				//    deathmatch ? USE_BLSR_AMMO_1 : USE_BLSR_AMMO_2;
				//mo = P_SpawnPlayerMissile(player.mo, MT_BLASTERFX1);
				//if (mo)
				//{
				//    mo.thinker.function = P_BlasterMobjThinker;
				//}
				//S_StartSound(player.mo, sfx_blssht);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FireGoldWandPL1
		//
		//----------------------------------------------------------------------------
		public class A_FireGoldWandPL1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				DoomDef.mobj_t mo;
				uint angle;
				int damage;

				mo = player.mo;
				player.ammo[(int)DoomDef.ammotype_t.am_goldwand] -= p_local.USE_GWND_AMMO_1;
				P_BulletSlope(mo);
				damage = 7 + (m_misc.P_Random() & 7);
				angle = mo.angle;
				if (player.refire != 0)
				{
					angle += (uint)((m_misc.P_Random() - m_misc.P_Random()) << 18);
				}
				p_mobj.PuffType = info.mobjtype_t.MT_GOLDWANDPUFF1;
				p_map.P_LineAttack(mo, angle, p_local.MISSILERANGE, bulletslope, damage);
				i_ibm.S_StartSound(player.mo, (int)sounds.sfxenum_t.sfx_gldhit);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FireGoldWandPL2
		//
		//----------------------------------------------------------------------------
		public class A_FireGoldWandPL2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				//int i;
				//mobj_t* mo;
				//uint angle;
				//int damage;
				//int momz;

				//mo = player.mo;
				//player.ammo[am_goldwand] -=
				//    deathmatch ? USE_GWND_AMMO_1 : USE_GWND_AMMO_2;
				//PuffType = MT_GOLDWANDPUFF2;
				//P_BulletSlope(mo);
				//momz = FixedMul(mobjinfo[MT_GOLDWANDFX2].speed, bulletslope);
				//P_SpawnMissileAngle(mo, MT_GOLDWANDFX2, mo.angle - (ANG45 / 8), momz);
				//P_SpawnMissileAngle(mo, MT_GOLDWANDFX2, mo.angle + (ANG45 / 8), momz);
				//angle = mo.angle - (ANG45 / 8);
				//for (i = 0; i < 5; i++)
				//{
				//    damage = 1 + (P_Random() & 7);
				//    P_LineAttack(mo, angle, MISSILERANGE, bulletslope, damage);
				//    angle += ((ANG45 / 8) * 2) / 4;
				//}
				//S_StartSound(player.mo, sfx_gldhit);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FireMacePL1B
		//
		//----------------------------------------------------------------------------

		public static void A_FireMacePL1B(DoomDef.player_t player, DoomDef.pspdef_t psp)
		{
			DoomDef.mobj_t pmo;
			DoomDef.mobj_t ball;
			uint angle;

			if (player.ammo[(int)DoomDef.ammotype_t.am_mace] < p_local.USE_MACE_AMMO_1)
			{
				return;
			}
			player.ammo[(int)DoomDef.ammotype_t.am_mace] -= p_local.USE_MACE_AMMO_1;
			pmo = player.mo;
			ball = p_mobj.P_SpawnMobj(pmo.x, pmo.y, pmo.z + 28 * DoomDef.FRACUNIT
				- p_local.FOOTCLIPSIZE * (((pmo.flags2 & DoomDef.MF2_FEETARECLIPPED) != 0) ? 1 : 0), info.mobjtype_t.MT_MACEFX2);
			ball.momz = 2 * DoomDef.FRACUNIT + ((player.lookdir) << (DoomDef.FRACBITS - 5));
			angle = pmo.angle;
			ball.target = pmo;
			ball.angle = angle;
			ball.z += (player.lookdir) << (DoomDef.FRACBITS - 4);
			angle >>= (int)DoomDef.ANGLETOFINESHIFT;
			ball.momx = (pmo.momx >> 1)
				+ DoomDef.FixedMul(ball.infol.speed, r_main.finecosine(angle));
			ball.momy = (pmo.momy >> 1)
				+ DoomDef.FixedMul(ball.infol.speed, tables.finesine[angle]);
			i_ibm.S_StartSound(ball, (int)sounds.sfxenum_t.sfx_lobsht);
			p_mobj.P_CheckMissileSpawn(ball);
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FireMacePL1
		//
		//----------------------------------------------------------------------------
		public class A_FireMacePL1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				DoomDef.mobj_t ball;

				if (m_misc.P_Random() < 28)
				{
					A_FireMacePL1B(player, psp);
					return;
				}
				if (player.ammo[(int)DoomDef.ammotype_t.am_mace] < p_local.USE_MACE_AMMO_1)
				{
					return;
				}
				player.ammo[(int)DoomDef.ammotype_t.am_mace] -= p_local.USE_MACE_AMMO_1;
				psp.sx = ((m_misc.P_Random() & 3) - 2) * DoomDef.FRACUNIT;
				psp.sy = WEAPONTOP + (m_misc.P_Random() & 3) * DoomDef.FRACUNIT;
				ball = p_mobj.P_SPMAngle(player.mo, info.mobjtype_t.MT_MACEFX1, (uint)(player.mo.angle
					+ (((m_misc.P_Random() & 7) - 4) << 24)));
				if (ball != null)
				{
					ball.special1 = 16; // tics till dropoff
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_MacePL1Check
		//
		//----------------------------------------------------------------------------
		public class A_MacePL1Check : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t ball)
			{
				uint angle;

				if (ball.special1 == 0)
				{
					return;
				}
				ball.special1 -= 4;
				if (ball.special1 > 0)
				{
					return;
				}
				ball.special1 = 0;
				ball.flags2 |= DoomDef.MF2_LOGRAV;
				angle = ball.angle >> (int)DoomDef.ANGLETOFINESHIFT;
				ball.momx = DoomDef.FixedMul(7 * DoomDef.FRACUNIT, r_main.finecosine(angle));
				ball.momy = DoomDef.FixedMul(7 * DoomDef.FRACUNIT, tables.finesine[angle]);
				ball.momz -= ball.momz >> 1;
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_MaceBallImpact
		//
		//----------------------------------------------------------------------------
		public class A_MaceBallImpact : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t ball)
			{
				if ((ball.z <= ball.floorz) && (p_mobj.P_HitFloor(ball) != p_local.FLOOR_SOLID))
				{ // Landed in some sort of liquid
					p_mobj.P_RemoveMobj(ball);
					return;
				}
				if ((ball.health != MAGIC_JUNK) && (ball.z <= ball.floorz)
					&& ball.momz != 0)
				{ // Bounce
					ball.health = MAGIC_JUNK;
					ball.momz = (ball.momz * 192) >> 8;
					ball.flags2 &= ~DoomDef.MF2_FLOORBOUNCE;
					p_mobj.P_SetMobjState(ball, (info.statenum_t)ball.infol.spawnstate);
					i_ibm.S_StartSound(ball, (int)sounds.sfxenum_t.sfx_bounce);
				}
				else
				{ // Explode
					ball.flags |= DoomDef.MF_NOGRAVITY;
					ball.flags2 &= ~DoomDef.MF2_LOGRAV;
					i_ibm.S_StartSound(ball, (int)sounds.sfxenum_t.sfx_lobhit);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_MaceBallImpact2
		//
		//----------------------------------------------------------------------------
		public class A_MaceBallImpact2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t ball)
			{
				DoomDef.mobj_t tiny;
				uint angle;

				if ((ball.z <= ball.floorz) && (p_mobj.P_HitFloor(ball) != p_local.FLOOR_SOLID))
				{ // Landed in some sort of liquid
					p_mobj.P_RemoveMobj(ball);
					return;
				}
				if ((ball.z != ball.floorz) || (ball.momz < 2 * DoomDef.FRACUNIT))
				{ // Explode
					ball.momx = ball.momy = ball.momz = 0;
					ball.flags |= DoomDef.MF_NOGRAVITY;
					ball.flags2 &= ~(DoomDef.MF2_LOGRAV | DoomDef.MF2_FLOORBOUNCE);
				}
				else
				{ // Bounce
					ball.momz = (ball.momz * 192) >> 8;
					p_mobj.P_SetMobjState(ball, (info.statenum_t)ball.infol.spawnstate);

					tiny = p_mobj.P_SpawnMobj(ball.x, ball.y, ball.z, info.mobjtype_t.MT_MACEFX3);
					angle = ball.angle + DoomDef.ANG90;
					tiny.target = ball.target;
					tiny.angle = angle;
					angle >>= (int)DoomDef.ANGLETOFINESHIFT;
					tiny.momx = (ball.momx >> 1) + DoomDef.FixedMul(ball.momz - DoomDef.FRACUNIT,
						r_main.finecosine(angle));
					tiny.momy = (ball.momy >> 1) + DoomDef.FixedMul(ball.momz - DoomDef.FRACUNIT,
						tables.finesine[angle]);
					tiny.momz = ball.momz;
					p_mobj.P_CheckMissileSpawn(tiny);

					tiny = p_mobj.P_SpawnMobj(ball.x, ball.y, ball.z, info.mobjtype_t.MT_MACEFX3);
					angle = ball.angle - DoomDef.ANG90;
					tiny.target = ball.target;
					tiny.angle = angle;
					angle >>= (int)DoomDef.ANGLETOFINESHIFT;
					tiny.momx = (ball.momx >> 1) + DoomDef.FixedMul(ball.momz - DoomDef.FRACUNIT,
						r_main.finecosine(angle));
					tiny.momy = (ball.momy >> 1) + DoomDef.FixedMul(ball.momz - DoomDef.FRACUNIT,
						tables.finesine[angle]);
					tiny.momz = ball.momz;
					p_mobj.P_CheckMissileSpawn(tiny);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FireMacePL2
		//
		//----------------------------------------------------------------------------

		public class A_FireMacePL2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				//mobj_t *mo;

				//player.ammo[am_mace] -=
				//    deathmatch ? USE_MACE_AMMO_1 : USE_MACE_AMMO_2;
				//mo = P_SpawnPlayerMissile(player.mo, MT_MACEFX4);
				//if(mo)
				//{
				//    mo.momx += player.mo.momx;
				//    mo.momy += player.mo.momy;
				//    mo.momz = 2*FRACUNIT+((player.lookdir)<<(FRACBITS-5));
				//    if(linetarget)
				//    {
				//        mo.special1 = (int)linetarget;
				//    }
				//}
				//S_StartSound(player.mo, sfx_lobsht);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_DeathBallImpact
		//
		//----------------------------------------------------------------------------
		public class A_DeathBallImpact : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				//int i;
				//mobj_t *target;
				//uint angle;
				//boolean newAngle;

				//if((ball.z <= ball.floorz) && (P_HitFloor(ball) != FLOOR_SOLID))
				//{ // Landed in some sort of liquid
				//    P_RemoveMobj(ball);
				//    return;
				//}
				//if((ball.z <= ball.floorz) && ball.momz)
				//{ // Bounce
				//    newAngle = false;
				//    target = (mobj_t *)ball.special1;
				//    if(target)
				//    {
				//        if(!(target.flags&MF_SHOOTABLE))
				//        { // Target died
				//            ball.special1 = 0;
				//        }
				//        else
				//        { // Seek
				//            angle = R_PointToAngle2(ball.x, ball.y,
				//                target.x, target.y);
				//            newAngle = true;
				//        }
				//    }
				//    else
				//    { // Find new target
				//        angle = 0;
				//        for(i = 0; i < 16; i++)
				//        {
				//            P_AimLineAttack(ball, angle, 10*64*FRACUNIT);
				//            if(linetarget && ball.target != linetarget)
				//            {
				//                ball.special1 = (int)linetarget;
				//                angle = R_PointToAngle2(ball.x, ball.y,
				//                    linetarget.x, linetarget.y);
				//                newAngle = true;
				//                break;
				//            }
				//            angle += ANGLE_45/2;
				//        }
				//    }
				//    if(newAngle)
				//    {
				//        ball.angle = angle;
				//        angle >>= ANGLETOFINESHIFT;
				//        ball.momx = FixedMul(ball.info.speed, finecosine[angle]);
				//        ball.momy = FixedMul(ball.info.speed, finesine[angle]);
				//    }
				//    P_SetMobjState(ball, ball.info.spawnstate);
				//    S_StartSound(ball, sfx_pstop);
				//}
				//else
				//{ // Explode
				//    ball.flags |= MF_NOGRAVITY;
				//    ball.flags2 &= ~MF2_LOGRAV;
				//    S_StartSound(ball, sfx_phohit);
				//}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_SpawnRippers
		//
		//----------------------------------------------------------------------------
		public class A_SpawnRippers : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				//int i;
				//uint angle;
				//mobj_t* ripper;

				//for (i = 0; i < 8; i++)
				//{
				//    ripper = P_SpawnMobj(actor.x, actor.y, actor.z, MT_RIPPER);
				//    angle = i * ANG45;
				//    ripper.target = actor.target;
				//    ripper.angle = angle;
				//    angle >>= ANGLETOFINESHIFT;
				//    ripper.momx = FixedMul(ripper.info.speed, finecosine[angle]);
				//    ripper.momy = FixedMul(ripper.info.speed, finesine[angle]);
				//    P_CheckMissileSpawn(ripper);
				//}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FireCrossbowPL1
		//
		//----------------------------------------------------------------------------
		public class A_FireCrossbowPL1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				DoomDef.mobj_t pmo;

				pmo = player.mo;
				player.ammo[(int)DoomDef.ammotype_t.am_crossbow] -= p_local.USE_CBOW_AMMO_1;
				p_mobj.P_SpawnPlayerMissile(pmo, info.mobjtype_t.MT_CRBOWFX1);
				p_mobj.P_SPMAngle(pmo, info.mobjtype_t.MT_CRBOWFX3, pmo.angle - (DoomDef.ANG45 / 10));
				p_mobj.P_SPMAngle(pmo, info.mobjtype_t.MT_CRBOWFX3, pmo.angle + (DoomDef.ANG45 / 10));
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FireCrossbowPL2
		//
		//----------------------------------------------------------------------------
		public class A_FireCrossbowPL2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				DoomDef.mobj_t pmo;

				pmo = player.mo;
				player.ammo[(int)DoomDef.ammotype_t.am_crossbow] -=
					g_game.deathmatch ? p_local.USE_CBOW_AMMO_1 : p_local.USE_CBOW_AMMO_2;
				p_mobj.P_SpawnPlayerMissile(pmo, info.mobjtype_t.MT_CRBOWFX2);
				p_mobj.P_SPMAngle(pmo, info.mobjtype_t.MT_CRBOWFX2, pmo.angle - (DoomDef.ANG45 / 10));
				p_mobj.P_SPMAngle(pmo, info.mobjtype_t.MT_CRBOWFX2, pmo.angle + (DoomDef.ANG45 / 10));
				p_mobj.P_SPMAngle(pmo, info.mobjtype_t.MT_CRBOWFX3, pmo.angle - (DoomDef.ANG45 / 5));
				p_mobj.P_SPMAngle(pmo, info.mobjtype_t.MT_CRBOWFX3, pmo.angle + (DoomDef.ANG45 / 5));
			}
		}



		//----------------------------------------------------------------------------
		//
		// PROC A_BoltSpark
		//
		//----------------------------------------------------------------------------
		public class A_BoltSpark : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t bolt)
			{
				DoomDef.mobj_t spark;

				if (m_misc.P_Random() > 50)
				{
					spark = p_mobj.P_SpawnMobj(bolt.x, bolt.y, bolt.z, info.mobjtype_t.MT_CRBOWFX4);
					spark.x += (m_misc.P_Random() - m_misc.P_Random()) << 10;
					spark.y += (m_misc.P_Random() - m_misc.P_Random()) << 10;
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FireSkullRodPL1
		//
		//----------------------------------------------------------------------------
		public class A_FireSkullRodPL1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				DoomDef.mobj_t mo;

				if (player.ammo[(int)DoomDef.ammotype_t.am_skullrod] < p_local.USE_SKRD_AMMO_1)
				{
					return;
				}
				player.ammo[(int)DoomDef.ammotype_t.am_skullrod] -= p_local.USE_SKRD_AMMO_1;
				mo = p_mobj.P_SpawnPlayerMissile(player.mo, info.mobjtype_t.MT_HORNRODFX1);
				// Randomize the first frame
				if (mo != null && m_misc.P_Random() > 128)
				{
					p_mobj.P_SetMobjState(mo, info.statenum_t.S_HRODFX1_2);
				}
			}
		}
		//----------------------------------------------------------------------------
		//
		// PROC A_FireSkullRodPL2
		//
		// The special2 field holds the player number that shot the rain missile.
		// The special1 field is used for the seeking routines, then as a counter
		// for the sound looping.
		//
		//----------------------------------------------------------------------------
		public class A_FireSkullRodPL2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				//player.ammo[am_skullrod] -=
				//    deathmatch ? USE_SKRD_AMMO_1 : USE_SKRD_AMMO_2;
				//P_SpawnPlayerMissile(player.mo, MT_HORNRODFX2);
				//// Use MissileMobj instead of the return value from
				//// P_SpawnPlayerMissile because we need to give info to the mobj
				//// even if it exploded immediately.
				//if(netgame)
				//{ // Multi-player game
				//    MissileMobj.special2 = P_GetPlayerNum(player);
				//}
				//else
				//{ // Always use red missiles in single player games
				//    MissileMobj.special2 = 2;
				//}
				//if(linetarget)
				//{
				//    MissileMobj.special1 = (int)linetarget;
				//}
				//S_StartSound(MissileMobj, sfx_hrnpow);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_SkullRodPL2Seek
		//
		//----------------------------------------------------------------------------
		public class A_SkullRodPL2Seek : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				p_mobj.P_SeekerMissile(actor, DoomDef.ANGLE_1 * 10, DoomDef.ANGLE_1 * 30);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_AddPlayerRain
		//
		//----------------------------------------------------------------------------
		public class A_AddPlayerRain : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				//int playerNum;
				//player_t* player;

				//playerNum = netgame ? actor.special2 : 0;
				//if (!playeringame[playerNum])
				//{ // Player left the game
				//    return;
				//}
				//player = &players[playerNum];
				//if (player.health <= 0)
				//{ // Player is dead
				//    return;
				//}
				//if (player.rain1 && player.rain2)
				//{ // Terminate an active rain
				//    if (player.rain1.health < player.rain2.health)
				//    {
				//        if (player.rain1.health > 16)
				//        {
				//            player.rain1.health = 16;
				//        }
				//        player.rain1 = NULL;
				//    }
				//    else
				//    {
				//        if (player.rain2.health > 16)
				//        {
				//            player.rain2.health = 16;
				//        }
				//        player.rain2 = NULL;
				//    }
				//}
				//// Add rain mobj to list
				//if (player.rain1)
				//{
				//    player.rain2 = actor;
				//}
				//else
				//{
				//    player.rain1 = actor;
				//}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_SkullRodStorm
		//
		//----------------------------------------------------------------------------
		public class A_SkullRodStorm : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				//int x;
				//int y;
				//mobj_t* mo;
				//int playerNum;
				//player_t* player;

				//if (actor.health-- == 0)
				//{
				//    P_SetMobjState(actor, S_NULL);
				//    playerNum = netgame ? actor.special2 : 0;
				//    if (!playeringame[playerNum])
				//    { // Player left the game
				//        return;
				//    }
				//    player = &players[playerNum];
				//    if (player.health <= 0)
				//    { // Player is dead
				//        return;
				//    }
				//    if (player.rain1 == actor)
				//    {
				//        player.rain1 = NULL;
				//    }
				//    else if (player.rain2 == actor)
				//    {
				//        player.rain2 = NULL;
				//    }
				//    return;
				//}
				//if (P_Random() < 25)
				//{ // Fudge rain frequency
				//    return;
				//}
				//x = actor.x + ((P_Random() & 127) - 64) * FRACUNIT;
				//y = actor.y + ((P_Random() & 127) - 64) * FRACUNIT;
				//mo = P_SpawnMobj(x, y, ONCEILINGZ, MT_RAINPLR1 + actor.special2);
				//mo.target = actor.target;
				//mo.momx = 1; // Force collision detection
				//mo.momz = -mo.info.speed;
				//mo.special2 = actor.special2; // Transfer player number
				//P_CheckMissileSpawn(mo);
				//if (!(actor.special1 & 31))
				//{
				//    S_StartSound(actor, sfx_ramrain);
				//}
				//actor.special1++;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_RainImpact
		//
		//----------------------------------------------------------------------------
		public class A_RainImpact : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.z > actor.floorz)
				{
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_RAINAIRXPLR1_1 + actor.special2);
				}
				else if (m_misc.P_Random() < 40)
				{
					p_mobj.P_HitFloor(actor);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_HideInCeiling
		//
		//----------------------------------------------------------------------------
		public class A_HideInCeiling : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.z = actor.ceilingz + 4 * DoomDef.FRACUNIT;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FirePhoenixPL1
		//
		//----------------------------------------------------------------------------
		public class A_FirePhoenixPL1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				uint angle;

				player.ammo[(int)DoomDef.ammotype_t.am_phoenixrod] -= p_local.USE_PHRD_AMMO_1;
				p_mobj.P_SpawnPlayerMissile(player.mo, info.mobjtype_t.MT_PHOENIXFX1);
				angle = player.mo.angle + DoomDef.ANG180;
				angle >>= (int)DoomDef.ANGLETOFINESHIFT;
				player.mo.momx += DoomDef.FixedMul(4 * DoomDef.FRACUNIT, r_main.finecosine(angle));
				player.mo.momy += DoomDef.FixedMul(4 * DoomDef.FRACUNIT, tables.finesine[angle]);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_PhoenixPuff
		//
		//----------------------------------------------------------------------------
		public class A_PhoenixPuff : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t puff;
				uint angle;

				p_mobj.P_SeekerMissile(actor, DoomDef.ANGLE_1 * 5, DoomDef.ANGLE_1 * 10);
				puff = p_mobj.P_SpawnMobj(actor.x, actor.y, actor.z, info.mobjtype_t.MT_PHOENIXPUFF);
				angle = actor.angle + DoomDef.ANG90;
				angle >>= (int)DoomDef.ANGLETOFINESHIFT;
				puff.momx = DoomDef.FixedMul((int)((double)DoomDef.FRACUNIT * 1.3), r_main.finecosine(angle));
				puff.momy = DoomDef.FixedMul((int)((double)DoomDef.FRACUNIT * 1.3), tables.finesine[angle]);
				puff.momz = 0;
				puff = p_mobj.P_SpawnMobj(actor.x, actor.y, actor.z, info.mobjtype_t.MT_PHOENIXPUFF);
				angle = actor.angle - DoomDef.ANG90;
				angle >>= (int)DoomDef.ANGLETOFINESHIFT;
				puff.momx = DoomDef.FixedMul((int)((double)DoomDef.FRACUNIT * 1.3), r_main.finecosine(angle));
				puff.momy = DoomDef.FixedMul((int)((double)DoomDef.FRACUNIT * 1.3), tables.finesine[angle]);
				puff.momz = 0;
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_InitPhoenixPL2
		//
		//----------------------------------------------------------------------------
		public class A_InitPhoenixPL2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				player.flamecount = FLAME_THROWER_TICS;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FirePhoenixPL2
		//
		// Flame thrower effect.
		//
		//----------------------------------------------------------------------------
		public class A_FirePhoenixPL2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				//mobj_t* mo;
				//mobj_t* pmo;
				//uint angle;
				//int x, y, z;
				//int slope;

				//if (--player.flamecount == 0)
				//{ // Out of flame
				//    P_SetPsprite(player, ps_weapon, S_PHOENIXATK2_4);
				//    player.refire = 0;
				//    return;
				//}
				//pmo = player.mo;
				//angle = pmo.angle;
				//x = pmo.x + ((P_Random() - P_Random()) << 9);
				//y = pmo.y + ((P_Random() - P_Random()) << 9);
				//z = pmo.z + 26 * FRACUNIT + ((player.lookdir) << FRACBITS) / 173;
				//if (pmo.flags2 & MF2_FEETARECLIPPED)
				//{
				//    z -= FOOTCLIPSIZE;
				//}
				//slope = ((player.lookdir) << FRACBITS) / 173 + (FRACUNIT / 10);
				//mo = P_SpawnMobj(x, y, z, MT_PHOENIXFX2);
				//mo.target = pmo;
				//mo.angle = angle;
				//mo.momx = pmo.momx + FixedMul(mo.info.speed,
				//    finecosine[angle >> ANGLETOFINESHIFT]);
				//mo.momy = pmo.momy + FixedMul(mo.info.speed,
				//    finesine[angle >> ANGLETOFINESHIFT]);
				//mo.momz = FixedMul(mo.info.speed, slope);
				//if (!player.refire || !(leveltime % 38))
				//{
				//    S_StartSound(player.mo, sfx_phopow);
				//}
				//P_CheckMissileSpawn(mo);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_ShutdownPhoenixPL2
		//
		//----------------------------------------------------------------------------
		public class A_ShutdownPhoenixPL2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				player.ammo[(int)DoomDef.ammotype_t.am_phoenixrod] -= p_local.USE_PHRD_AMMO_2;
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_FlameEnd
		//
		//----------------------------------------------------------------------------
		public class A_FlameEnd : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.momz += (int)(1.5 * (double)DoomDef.FRACUNIT);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_FloatPuff
		//
		//----------------------------------------------------------------------------
		public class A_FloatPuff : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t puff)
			{
				puff.momz += (int)(1.8 * (double)DoomDef.FRACUNIT);
			}
		}


		//---------------------------------------------------------------------------
		//
		// PROC A_GauntletAttack
		//
		//---------------------------------------------------------------------------
		public class A_GauntletAttack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				uint angle;
				int damage;
				int slope;
				int randVal;
				int dist;

				psp.sx = ((m_misc.P_Random() & 3) - 2) * DoomDef.FRACUNIT;
				psp.sy = WEAPONTOP + (m_misc.P_Random() & 3) * DoomDef.FRACUNIT;
				angle = player.mo.angle;
				if (player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0)
				{
					damage = DoomDef.HITDICE(2);
					dist = 4 * p_local.MELEERANGE;
					angle += (uint)((m_misc.P_Random() - m_misc.P_Random()) << 17);
					p_mobj.PuffType = info.mobjtype_t.MT_GAUNTLETPUFF2;
				}
				else
				{
					damage = DoomDef.HITDICE(2);
					dist = p_local.MELEERANGE + 1;
					angle += (uint)((m_misc.P_Random() - m_misc.P_Random()) << 18);
					p_mobj.PuffType = info.mobjtype_t.MT_GAUNTLETPUFF1;
				}
				slope = p_map.P_AimLineAttack(player.mo, angle, dist);
				p_map.P_LineAttack(player.mo, angle, dist, slope, damage);
				if (p_map.linetarget == null)
				{
					if (m_misc.P_Random() > 64)
					{
						player.extralight = (player.extralight != 0) ? 0 : 1;
					}
					i_ibm.S_StartSound(player.mo, (int)sounds.sfxenum_t.sfx_gntful);
					return;
				}
				randVal = m_misc.P_Random();
				if (randVal < 64)
				{
					player.extralight = 0;
				}
				else if (randVal < 160)
				{
					player.extralight = 1;
				}
				else
				{
					player.extralight = 2;
				}
				if (player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0)
				{
					p_inter.P_GiveBody(player, damage >> 1);
					i_ibm.S_StartSound(player.mo, (int)sounds.sfxenum_t.sfx_gntpow);
				}
				else
				{
					i_ibm.S_StartSound(player.mo, (int)sounds.sfxenum_t.sfx_gnthit);
				}
				// turn to face target
				angle = r_main.R_PointToAngle2(player.mo.x, player.mo.y,
					p_map.linetarget.x, p_map.linetarget.y);
				if (angle - player.mo.angle > DoomDef.ANG180)
				{
					if ((int)(angle - player.mo.angle) < -(int)(DoomDef.ANG90 / 20))
						player.mo.angle = angle + DoomDef.ANG90 / 21;
					else
						player.mo.angle -= DoomDef.ANG90 / 20;
				}
				else
				{
					if (angle - player.mo.angle > DoomDef.ANG90 / 20)
						player.mo.angle = angle - DoomDef.ANG90 / 21;
					else
						player.mo.angle += DoomDef.ANG90 / 20;
				}
				player.mo.flags |= DoomDef.MF_JUSTATTACKED;
			}
		}
		public class A_Light0 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.player_t player, DoomDef.pspdef_t psp)
			{
				player.extralight = 0;
			}
		}
#if DOS
void A_Light1(player_t *player, pspdef_t *psp)
{
	player.extralight = 1;
}

void A_Light2(player_t *player, pspdef_t *psp)
{
	player.extralight = 2;
}
#endif
		//------------------------------------------------------------------------
		//
		// PROC P_SetupPsprites
		//
		// Called at start of level for each player
		//
		//------------------------------------------------------------------------

		public static void P_SetupPsprites(DoomDef.player_t player)
		{
			int i;

			// Remove all psprites
			for (i = 0; i < (int)DoomDef.psprnum_t.NUMPSPRITES; i++)
			{
				player.psprites[i].state = null;
			}
			// Spawn the ready weapon
			player.pendingweapon = player.readyweapon;
			P_BringUpWeapon(player);
		}
		//------------------------------------------------------------------------
		//
		// PROC P_MovePsprites
		//
		// Called every tic by player thinking routine
		//
		//------------------------------------------------------------------------

		public static void P_MovePsprites(DoomDef.player_t player)
		{
			int i;
			DoomDef.pspdef_t psp;
			info.state_t state;

			for (i = 0; i < (int)DoomDef.psprnum_t.NUMPSPRITES; i++)
			{
				psp = player.psprites[i];
				state = psp.state;
				if (state != null) // a null state means not active
				{
					// drop tic count and possibly change state
					if (psp.tics != -1)	// a -1 tic count never changes
					{
						psp.tics--;
						if (psp.tics == 0)
						{
							P_SetPsprite(player, i, psp.state.nextstate);
						}
					}
				}
			}
			player.psprites[(int)DoomDef.psprnum_t.ps_flash].sx = player.psprites[(int)DoomDef.psprnum_t.ps_weapon].sx;
			player.psprites[(int)DoomDef.psprnum_t.ps_flash].sy = player.psprites[(int)DoomDef.psprnum_t.ps_weapon].sy;
		}
	}
}
