using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

		
// P_user.c

namespace HereticXNA
{
	public static class p_user
	{
		// Macros

		public static int MAXBOB = 0x100000; // 16 pixels of bob

		// Data
		public static bool onground;
		public static int newtorch; // used in the torch flicker effect.
		public static int newtorchdelta;

		public static bool[] WeaponInShareware = new bool[]
{
	true,           // Staff
	true,           // Gold wand
	true,           // Crossbow
	true,           // Blaster
	false,          // Skull rod
	false,          // Phoenix rod
	false,          // Mace
	true,           // Gauntlets
	true            // Beak
};


		/*
==================
=
= P_Thrust
=
= moves the given origin along a given angle
=
==================
*/

		public static void P_Thrust(DoomDef.player_t player, uint angle, int move)
		{
			angle = (uint)(angle >> (int)DoomDef.ANGLETOFINESHIFT);
			if (player.powers[(int)DoomDef.powertype_t.pw_flight] != 0 && !(player.mo.z <= player.mo.floorz))
			{
				player.mo.momx += DoomDef.FixedMul(move, r_main.finecosine(angle));
				player.mo.momy += DoomDef.FixedMul(move, tables.finesine[angle]);
			}
			else if (player.mo.subsector.sector.special == 15) // Friction_Low
			{
				player.mo.momx += DoomDef.FixedMul(move >> 2, r_main.finecosine(angle));
				player.mo.momy += DoomDef.FixedMul(move >> 2, tables.finesine[angle]);
			}
			else
			{
				player.mo.momx += DoomDef.FixedMul(move, r_main.finecosine(angle));
				player.mo.momy += DoomDef.FixedMul(move, tables.finesine[angle]);
			}
		}

		/*
		==================
		=
		= P_CalcHeight
		=
		=Calculate the walking / running height adjustment
		=
		==================
		*/

		public static void P_CalcHeight(DoomDef.player_t player)
		{
			int angle;
			int bob;

			//
			// regular movement bobbing (needs to be calculated for gun swing even
			// if not on ground)
			// OPTIMIZE: tablify angle

			player.bob =
				DoomDef.FixedMul(player.mo.momx, player.mo.momx) +
				DoomDef.FixedMul(player.mo.momy, player.mo.momy);
			player.bob >>= 2;
			if (player.bob > MAXBOB)
				player.bob = MAXBOB;
			if ((player.mo.flags2 & DoomDef.MF2_FLY) != 0 && !onground)
			{
				player.bob = DoomDef.FRACUNIT / 2;
			}

			if ((player.cheats & DoomDef.CF_NOMOMENTUM) != 0)
			{
				player.viewz = player.mo.z + p_local.VIEWHEIGHT;
				if (player.viewz > player.mo.ceilingz - 4 * DoomDef.FRACUNIT)
					player.viewz = player.mo.ceilingz - 4 * DoomDef.FRACUNIT;
				player.viewz = player.mo.z + player.viewheight;
				return;
			}

			angle = (int)((DoomDef.FINEANGLES / 20 * p_tick.leveltime) & DoomDef.FINEMASK);
			bob = DoomDef.FixedMul(player.bob / 2, tables.finesine[angle]);

			//
			// move viewheight
			//
			if (player.playerstate == DoomDef.playerstate_t.PST_LIVE)
			{
				player.viewheight += player.deltaviewheight;
				if (player.viewheight > p_local.VIEWHEIGHT)
				{
					player.viewheight = p_local.VIEWHEIGHT;
					player.deltaviewheight = 0;
				}
				if (player.viewheight < p_local.VIEWHEIGHT / 2)
				{
					player.viewheight = p_local.VIEWHEIGHT / 2;
					if (player.deltaviewheight <= 0)
						player.deltaviewheight = 1;
				}

				if (player.deltaviewheight != 0)
				{
					player.deltaviewheight += DoomDef.FRACUNIT / 4;
					if (player.deltaviewheight == 0)
						player.deltaviewheight = 1;
				}
			}

			if (player.chickenTics != 0)
			{
				player.viewz = player.mo.z + player.viewheight - (20 * DoomDef.FRACUNIT);
			}
			else
			{
				player.viewz = player.mo.z + player.viewheight + bob;
			}
			if ((player.mo.flags2 & DoomDef.MF2_FEETARECLIPPED) != 0
				&& player.playerstate != DoomDef.playerstate_t.PST_DEAD
				&& player.mo.z <= player.mo.floorz)
			{
				player.viewz -= p_local.FOOTCLIPSIZE;
			}
			if (player.viewz > player.mo.ceilingz - 4 * DoomDef.FRACUNIT)
			{
				player.viewz = player.mo.ceilingz - 4 * DoomDef.FRACUNIT;
			}
			if (player.viewz < player.mo.floorz + 4 * DoomDef.FRACUNIT)
			{
				player.viewz = player.mo.floorz + 4 * DoomDef.FRACUNIT;
			}
		}

		/*
		=================
		=
		= P_MovePlayer
		=
		=================
		*/

		public static void P_MovePlayer(DoomDef.player_t player)
		{
			int look;
			int fly;
			DoomDef.ticcmd_t cmd;

			cmd = player.cmd;
			player.mo.angle += (uint)(cmd.angleturn << 16);
			player.mo.xangle += (uint)(cmd.angleupdown << 16);
			if (player.mo.xangle > DoomDef.ANG180)
			{
				if (player.mo.xangle < DoomDef.ANG270 + DoomDef.ANG45 / 4)
					player.mo.xangle = DoomDef.ANG270 + DoomDef.ANG45 / 4;
			}
			else if (player.mo.xangle > DoomDef.ANG45 + DoomDef.ANG45 / 4)
				player.mo.xangle = DoomDef.ANG45 + DoomDef.ANG45 / 4;

			onground = (player.mo.z <= player.mo.floorz
				|| (player.mo.flags2 & DoomDef.MF2_ONMOBJ) != 0);

			if (player.chickenTics != 0)
			{ // Chicken speed
				if (cmd.forwardmove != 0 && (onground || (player.mo.flags2 & DoomDef.MF2_FLY) != 0))
					P_Thrust(player, player.mo.angle, cmd.forwardmove * 2500);
				if (cmd.sidemove != 0 && (onground || (player.mo.flags2 & DoomDef.MF2_FLY) != 0))
					P_Thrust(player, player.mo.angle - DoomDef.ANG90, cmd.sidemove * 2500);
			}
			else
			{ // Normal speed
				if (cmd.forwardmove != 0 && (onground || (player.mo.flags2 & DoomDef.MF2_FLY) != 0))
					P_Thrust(player, player.mo.angle, cmd.forwardmove * 2048);
				if (cmd.sidemove != 0 && (onground || (player.mo.flags2 & DoomDef.MF2_FLY) != 0))
					P_Thrust(player, player.mo.angle - DoomDef.ANG90, cmd.sidemove * 2048);
			}

			if (cmd.forwardmove != 0 || cmd.sidemove != 0)
			{
				if (player.chickenTics != 0)
				{
					if (player.mo.state == info.states[(int)info.statenum_t.S_CHICPLAY])
					{
						p_mobj.P_SetMobjState(player.mo, info.statenum_t.S_CHICPLAY_RUN1);
					}
				}
				else
				{
					if (player.mo.state == info.states[(int)info.statenum_t.S_PLAY])
					{
						p_mobj.P_SetMobjState(player.mo, info.statenum_t.S_PLAY_RUN1);
					}
				}
			}

			look = cmd.lookfly & 15;
			if (look > 7)
			{
				look -= 16;
			}
			if (look != 0)
			{
				if (look == p_local.TOCENTER)
				{
					player.centering = true;
				}
				else
				{
					player.lookdir += 5 * look;
					if (player.lookdir > 90 || player.lookdir < -110)
					{
						player.lookdir -= 5 * look;
					}
				}
			}
			if (player.centering)
			{
				if (player.lookdir > 0)
				{
					player.lookdir -= 8;
				}
				else if (player.lookdir < 0)
				{
					player.lookdir += 8;
				}
				if (Math.Abs(player.lookdir) < 8)
				{
					player.lookdir = 0;
					player.centering = false;
				}
			}
			fly = cmd.lookfly >> 4;
			if (fly > 7)
			{
				fly -= 16;
			}
			if (fly != 0 && player.powers[(int)DoomDef.powertype_t.pw_flight] != 0)
			{
				if (fly != p_local.TOCENTER)
				{
					player.flyheight = fly * 2;
					if ((player.mo.flags2 & DoomDef.MF2_FLY) == 0)
					{
						player.mo.flags2 |= DoomDef.MF2_FLY;
						player.mo.flags |= DoomDef.MF_NOGRAVITY;
					}
				}
				else
				{
					player.mo.flags2 &= ~DoomDef.MF2_FLY;
					player.mo.flags &= ~DoomDef.MF_NOGRAVITY;
				}
			}
			else if (fly > 0)
			{
				P_PlayerUseArtifact(player, DoomDef.artitype_t.arti_fly);
			}
			if ((player.mo.flags2 & DoomDef.MF2_FLY) != 0)
			{
				player.mo.momz = player.flyheight * DoomDef.FRACUNIT;
				if (player.flyheight != 0)
				{
					player.flyheight /= 2;
				}
			}
		}

		/*
		=================
		=
		= P_DeathThink
		=
		=================
		*/

		public const uint ANG5 = (DoomDef.ANG90 / 18);

		public static void P_DeathThink(DoomDef.player_t player)
		{
			uint angle;
			uint delta;
			int lookDelta;

			p_pspr.P_MovePsprites(player);

			onground = (player.mo.z <= player.mo.floorz);
			if (player.mo.type == info.mobjtype_t.MT_BLOODYSKULL)
			{ // Flying bloody skull
				player.viewheight = 6 * DoomDef.FRACUNIT;
				player.deltaviewheight = 0;
				//player.damagecount = 20;
				if (onground)
				{
					if (player.lookdir < 60)
					{
						lookDelta = (60 - player.lookdir) / 8;
						if (lookDelta < 1 && (p_tick.leveltime & 1) != 0)
						{
							lookDelta = 1;
						}
						else if (lookDelta > 6)
						{
							lookDelta = 6;
						}
						player.lookdir += lookDelta;
					}
				}
			}
			else
			{ // Fall to ground
				player.deltaviewheight = 0;
				if (player.viewheight > 6 * DoomDef.FRACUNIT)
					player.viewheight -= DoomDef.FRACUNIT;
				if (player.viewheight < 6 * DoomDef.FRACUNIT)
					player.viewheight = 6 * DoomDef.FRACUNIT;
				if (player.lookdir > 0)
				{
					player.lookdir -= 6;
				}
				else if (player.lookdir < 0)
				{
					player.lookdir += 6;
				}
				if (Math.Abs(player.lookdir) < 6)
				{
					player.lookdir = 0;
				}
			}
			P_CalcHeight(player);

			if (player.attacker != null && player.attacker != player.mo)
			{
				angle = r_main.R_PointToAngle2(player.mo.x, player.mo.y,
					player.attacker.x, player.attacker.y);
				delta = angle - player.mo.angle;
				int negAng = (int)-ANG5;
				if (delta < ANG5 || delta > (uint)negAng)
				{ // Looking at killer, so fade damage flash down
					player.mo.angle = angle;
					if (player.damagecount != 0)
					{
						player.damagecount--;
					}
				}
				else if (delta < DoomDef.ANG180)
					player.mo.angle += ANG5;
				else
					player.mo.angle -= ANG5;
			}
			else if (player.damagecount != 0)
			{
				player.damagecount--;
			}

			if ((player.cmd.buttons & DoomDef.BT_USE) != 0)
			{
				if (player == g_game.players[g_game.consoleplayer])
				{
					//I_SetPalette((byte *)W_CacheLumpName("PLAYPAL", PU_CACHE));
					sb_bar.inv_ptr = 0;
					sb_bar.curpos = 0;
					newtorch = 0;
					newtorchdelta = 0;
				}
				player.playerstate = DoomDef.playerstate_t.PST_REBORN;
				// Let the mobj know the player has entered the reborn state.  Some
				// mobjs need to know when it's ok to remove themselves.
				player.mo.special2 = 666;
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC P_ChickenPlayerThink
		//
		//----------------------------------------------------------------------------

		public static void P_ChickenPlayerThink(DoomDef.player_t player)
		{
			DoomDef.mobj_t pmo;

			if (player.health > 0)
			{ // Handle beak movement
				p_pspr.P_UpdateBeak(player, player.psprites[(int)DoomDef.psprnum_t.ps_weapon]);
			}
			if ((player.chickenTics & 15) != 0)
			{
				return;
			}
			pmo = player.mo;
			if ((pmo.momx + pmo.momy) == 0 && m_misc.P_Random() < 160)
			{ // Twitch view angle
				pmo.angle += (uint)((m_misc.P_Random() - m_misc.P_Random()) << 19);
			}
			if ((pmo.z <= pmo.floorz) && (m_misc.P_Random() < 32))
			{ // Jump and noise
				pmo.momz += DoomDef.FRACUNIT;
				p_mobj.P_SetMobjState(pmo, info.statenum_t.S_CHICPLAY_PAIN);
				return;
			}
			if (m_misc.P_Random() < 48)
			{ // Just noise
				i_ibm.S_StartSound(pmo, (int)sounds.sfxenum_t.sfx_chicact);
			}
		}

		//----------------------------------------------------------------------------
		//
		// FUNC P_GetPlayerNum
		//
		//----------------------------------------------------------------------------

		public static int P_GetPlayerNum(DoomDef.player_t player)
		{
			int i;

			for (i = 0; i < DoomDef.MAXPLAYERS; i++)
			{
				if (player == g_game.players[i])
				{
					return (i);
				}
			}
			return (0);
		}


		//----------------------------------------------------------------------------
		//
		// FUNC P_UndoPlayerChicken
		//
		//----------------------------------------------------------------------------

		public static bool P_UndoPlayerChicken(DoomDef.player_t player)
		{
			DoomDef.mobj_t fog;
			DoomDef.mobj_t mo;
			DoomDef.mobj_t pmo;
			int x;
			int y;
			int z;
			uint angle;
			int playerNum;
			DoomDef.weapontype_t weapon;
			int oldFlags;
			int oldFlags2;

			pmo = player.mo;
			x = pmo.x;
			y = pmo.y;
			z = pmo.z;
			angle = pmo.angle;
			weapon = (DoomDef.weapontype_t)pmo.special1;
			oldFlags = pmo.flags;
			oldFlags2 = pmo.flags2;
			p_mobj.P_SetMobjState(pmo, info.statenum_t.S_FREETARGMOBJ);
			mo = p_mobj.P_SpawnMobj(x, y, z, info.mobjtype_t.MT_PLAYER);
			if (p_map.P_TestMobjLocation(mo) == false)
			{ // Didn't fit
				p_mobj.P_RemoveMobj(mo);
				mo = p_mobj.P_SpawnMobj(x, y, z, info.mobjtype_t.MT_CHICPLAYER);
				mo.angle = angle;
				mo.health = player.health;
				mo.special1 = (int)weapon;
				mo.player = player;
				mo.flags = oldFlags;
				mo.flags2 = oldFlags2;
				player.mo = mo;
				player.chickenTics = 2 * 35;
				return (false);
			}
			playerNum = P_GetPlayerNum(player);
			if (playerNum != 0)
			{ // Set color translation
				mo.flags |= playerNum << DoomDef.MF_TRANSSHIFT;
			}
			mo.angle = angle;
			mo.player = player;
			mo.reactiontime = 18;
			if ((oldFlags2 & DoomDef.MF2_FLY) != 0)
			{
				mo.flags2 |= DoomDef.MF2_FLY;
				mo.flags |= DoomDef.MF_NOGRAVITY;
			}
			player.chickenTics = 0;
			player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] = 0;
			player.health = mo.health = p_local.MAXHEALTH;
			player.mo = mo;
			angle >>= (int)DoomDef.ANGLETOFINESHIFT;
			fog = p_mobj.P_SpawnMobj(x + 20 * r_main.finecosine(angle),
				y + 20 * tables.finesine[angle], z + DoomDef.TELEFOGHEIGHT, info.mobjtype_t.MT_TFOG);
			i_ibm.S_StartSound(fog, (int)sounds.sfxenum_t.sfx_telept);
			p_pspr.P_PostChickenWeapon(player, weapon);
			return (true);
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_PlayerThink
		//
		//----------------------------------------------------------------------------

		static public void P_PlayerThink(DoomDef.player_t player)
		{
			DoomDef.ticcmd_t cmd;
			DoomDef.weapontype_t newweapon;

			// No-clip cheat
			if ((player.cheats & DoomDef.CF_NOCLIP) != 0)
			{
				player.mo.flags |= DoomDef.MF_NOCLIP;
			}
			else
			{
				player.mo.flags &= ~DoomDef.MF_NOCLIP;
			}
			cmd = player.cmd;
			if ((player.mo.flags & DoomDef.MF_JUSTATTACKED) != 0)
			{ // Gauntlets attack auto forward motion
				cmd.angleturn = 0;
				cmd.angleupdown = 0;
				cmd.forwardmove = 0xc800 / 512;
				cmd.sidemove = 0;
				player.mo.flags &= ~DoomDef.MF_JUSTATTACKED;
			}
			// messageTics is above the rest of the counters so that messages will
			//              go away, even in death.
			player.messageTics--; // Can go negative
			if (player.messageTics == 0)
			{ // Refresh the screen when a message goes away
				p_inter.ultimatemsg = false; // clear out any chat messages.
				r_draw.BorderTopRefresh = true;
			}
			if (player.playerstate == DoomDef.playerstate_t.PST_DEAD)
			{
				P_DeathThink(player);
				return;
			}
			if (player.chickenTics != 0)
			{
				P_ChickenPlayerThink(player);
			}
			// Handle movement
			if (player.mo.reactiontime != 0)
			{ // Player is frozen
				player.mo.reactiontime--;
			}
			else
			{
				P_MovePlayer(player);
			}
			P_CalcHeight(player);
			if (player.mo.subsector.sector.special != 0)
			{
				p_spec.P_PlayerInSpecialSector(player);
			}
			if (cmd.arti != 0)
			{ // Use an artifact
				if (cmd.arti == 0xff)
				{
					P_PlayerNextArtifact(player);
				}
				else
				{
					P_PlayerUseArtifact(player, (DoomDef.artitype_t)cmd.arti);
				}
			}
			// Check for weapon change
			if ((cmd.buttons & DoomDef.BT_SPECIAL) != 0)
			{ // A special event has no other buttons
				cmd.buttons = 0;
			}
			if ((cmd.buttons & DoomDef.BT_CHANGE) != 0)
			{
				// The actual changing of the weapon is done when the weapon
				// psprite can do it (A_WeaponReady), so it doesn't happen in
				// the middle of an attack.
				newweapon = (DoomDef.weapontype_t)((cmd.buttons & DoomDef.BT_WEAPONMASK) >> DoomDef.BT_WEAPONSHIFT);
				if (newweapon == DoomDef.weapontype_t.wp_staff && player.weaponowned[(int)DoomDef.weapontype_t.wp_gauntlets]
					&& !(player.readyweapon == DoomDef.weapontype_t.wp_gauntlets))
				{
					newweapon = DoomDef.weapontype_t.wp_gauntlets;
				}
				if (player.weaponowned[(int)newweapon]
					&& newweapon != player.readyweapon)
				{
					if (WeaponInShareware[(int)newweapon] || !d_main.shareware)
					{
						player.pendingweapon = newweapon;
					}
				}
			}
			// Check for use
			if ((cmd.buttons & DoomDef.BT_USE) != 0)
			{
				if (player.usedown == 0)
				{
					p_map.P_UseLines(player);
					player.usedown = 1;
				}
			}
			else
			{
				player.usedown = 0;
			}
			// Chicken counter
			if (player.chickenTics != 0)
			{
				if (player.chickenPeck != 0)
				{ // Chicken attack counter
					player.chickenPeck -= 3;
				}
				if ((--player.chickenTics) == 0)
				{ // Attempt to undo the chicken
					P_UndoPlayerChicken(player);
				}
			}
			// Cycle psprites
			p_pspr.P_MovePsprites(player);
			// Other Counters
			if (player.powers[(int)DoomDef.powertype_t.pw_invulnerability] != 0)
			{
				player.powers[(int)DoomDef.powertype_t.pw_invulnerability]--;
			}
			if (player.powers[(int)DoomDef.powertype_t.pw_invisibility] != 0)
			{
				if ((--player.powers[(int)DoomDef.powertype_t.pw_invisibility]) == 0)
				{
					player.mo.flags &= ~DoomDef.MF_SHADOW;
				}
			}
			if (player.powers[(int)DoomDef.powertype_t.pw_infrared] != 0)
			{
				player.powers[(int)DoomDef.powertype_t.pw_infrared]--;
			}
			if (player.powers[(int)DoomDef.powertype_t.pw_flight] != 0)
			{
				if ((--player.powers[(int)DoomDef.powertype_t.pw_flight]) == 0)
				{
					if (player.mo.z != player.mo.floorz)
					{
						player.centering = true;
					}

					player.mo.flags2 &= ~DoomDef.MF2_FLY;
					player.mo.flags &= ~DoomDef.MF_NOGRAVITY;
					//BorderTopRefresh = true; //make sure the sprite's cleared out
				}
			}
			if (player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] != 0)
			{
				if ((--player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2]) == 0)
				{
					if ((player.readyweapon == DoomDef.weapontype_t.wp_phoenixrod)
						&& (player.psprites[(int)DoomDef.psprnum_t.ps_weapon].state
						!= info.states[(int)info.statenum_t.S_PHOENIXREADY])
						&& (player.psprites[(int)DoomDef.psprnum_t.ps_weapon].state
						!= info.states[(int)info.statenum_t.S_PHOENIXUP]))
					{
						p_pspr.P_SetPsprite(player, (int)DoomDef.psprnum_t.ps_weapon, info.statenum_t.S_PHOENIXREADY);
						player.ammo[(int)DoomDef.ammotype_t.am_phoenixrod] -= p_local.USE_PHRD_AMMO_2;
						player.refire = 0;
					}
					else if ((player.readyweapon == DoomDef.weapontype_t.wp_gauntlets)
						|| (player.readyweapon == DoomDef.weapontype_t.wp_staff))
					{
						player.pendingweapon = player.readyweapon;
					}
					//BorderTopRefresh = true;
				}
			}
			if (player.damagecount != 0)
			{
				player.damagecount--;
			}
			if (player.bonuscount != 0)
			{
				player.bonuscount--;
			}
			// Colormaps
			if (player.powers[(int)DoomDef.powertype_t.pw_invulnerability] != 0)
			{
				if (player.powers[(int)DoomDef.powertype_t.pw_invulnerability] > DoomDef.BLINKTHRESHOLD
					|| (player.powers[(int)DoomDef.powertype_t.pw_invulnerability] & 8) != 0)
				{
					player.fixedcolormap = r_local.INVERSECOLORMAP;
				}
				else
				{
					player.fixedcolormap = 0;
				}
			}
			else if (player.powers[(int)DoomDef.powertype_t.pw_infrared] != 0)
			{
				if (player.powers[(int)DoomDef.powertype_t.pw_infrared] <= DoomDef.BLINKTHRESHOLD)
				{
					if ((player.powers[(int)DoomDef.powertype_t.pw_infrared] & 8) != 0)
					{
						player.fixedcolormap = 0;
					}
					else
					{
						player.fixedcolormap = 1;
					}
				}
				else if ((p_tick.leveltime & 16) == 0 && player == g_game.players[g_game.consoleplayer])
				{
					if (newtorch != 0)
					{
						if (player.fixedcolormap + newtorchdelta > 7
							|| player.fixedcolormap + newtorchdelta < 1
							|| newtorch == player.fixedcolormap)
						{
							newtorch = 0;
						}
						else
						{
							player.fixedcolormap += newtorchdelta;
						}
					}
					else
					{
						newtorch = (m_misc.M_Random() & 7) + 1;
						newtorchdelta = (newtorch == player.fixedcolormap) ?
								0 : ((newtorch > player.fixedcolormap) ? 1 : -1);
					}
				}
			}
			else
			{
				player.fixedcolormap = 0;
			}
		}

#if DOS

//----------------------------------------------------------------------------
//
// PROC P_ArtiTele
//
//----------------------------------------------------------------------------

void P_ArtiTele(player_t *player)
{
	int i;
	int selections;
	int destX;
	int destY;
	uint destAngle;

	if(deathmatch)
	{
		selections = deathmatch_p-deathmatchstarts;
		i = P_Random()%selections;
		destX = deathmatchstarts[i].x<<FRACBITS;
		destY = deathmatchstarts[i].y<<FRACBITS;
		destAngle = ANG45*(deathmatchstarts[i].angle/45);
	}
	else
	{
		destX = playerstarts[0].x<<FRACBITS;
		destY = playerstarts[0].y<<FRACBITS;
		destAngle = ANG45*(playerstarts[0].angle/45);
	}
	P_Teleport(player.mo, destX, destY, destAngle);
	S_StartSound(NULL, sfx_wpnup); // Full volume laugh
}
#endif

		//----------------------------------------------------------------------------
		//
		// PROC P_PlayerNextArtifact
		//
		//----------------------------------------------------------------------------

		public static void P_PlayerNextArtifact(DoomDef.player_t player)
		{
			if (player == g_game.players[g_game.consoleplayer])
			{
				sb_bar.inv_ptr--;
				if (sb_bar.inv_ptr < 6)
				{
					sb_bar.curpos--;
					if (sb_bar.curpos < 0)
					{
						sb_bar.curpos = 0;
					}
				}
				if (sb_bar.inv_ptr < 0)
				{
					sb_bar.inv_ptr = player.inventorySlotNum - 1;
					if (sb_bar.inv_ptr < 6)
					{
						sb_bar.curpos = sb_bar.inv_ptr;
					}
					else
					{
						sb_bar.curpos = 6;
					}
				}
				player.readyArtifact = (DoomDef.artitype_t)
					player.inventory[sb_bar.inv_ptr].type;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_PlayerRemoveArtifact
		//
		//----------------------------------------------------------------------------

		public static void P_PlayerRemoveArtifact(DoomDef.player_t player, int slot)
		{
			int i;

			player.artifactCount--;
			if ((--player.inventory[slot].count) == 0)
			{ // Used last of a type - compact the artifact list
				player.readyArtifact = DoomDef.artitype_t.arti_none;
				player.inventory[slot].type = (int)DoomDef.artitype_t.arti_none;
				for (i = slot + 1; i < player.inventorySlotNum; i++)
				{
					player.inventory[i - 1] = player.inventory[i];
				}
				player.inventorySlotNum--;
				if (player == g_game.players[g_game.consoleplayer])
				{ // Set position markers and get next readyArtifact
					sb_bar.inv_ptr--;
					if (sb_bar.inv_ptr < 6)
					{
						sb_bar.curpos--;
						if (sb_bar.curpos < 0)
						{
							sb_bar.curpos = 0;
						}
					}
					if (sb_bar.inv_ptr >= player.inventorySlotNum)
					{
						sb_bar.inv_ptr = player.inventorySlotNum - 1;
					}
					if (sb_bar.inv_ptr < 0)
					{
						sb_bar.inv_ptr = 0;
					}
					player.readyArtifact = (DoomDef.artitype_t)
						player.inventory[sb_bar.inv_ptr].type;
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_PlayerUseArtifact
		//
		//----------------------------------------------------------------------------

		public static void P_PlayerUseArtifact(DoomDef.player_t player, DoomDef.artitype_t arti)
		{
			int i;

			for (i = 0; i < player.inventorySlotNum; i++)
			{
				if (player.inventory[i].type == (int)arti)
				{ // Found match - try to use
					if (P_UseArtifact(player, arti))
					{ // Artifact was used - remove it from inventory
						P_PlayerRemoveArtifact(player, i);
						if (player == g_game.players[g_game.consoleplayer])
						{
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_artiuse);
							sb_bar.ArtifactFlash = 4;
						}
					}
					else
					{ // Unable to use artifact, advance pointer
						P_PlayerNextArtifact(player);
					}
					break;
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// FUNC P_UseArtifact
		//
		// Returns true if artifact was used.
		//
		//----------------------------------------------------------------------------

		public static bool P_UseArtifact(DoomDef.player_t player, DoomDef.artitype_t arti)
		{
#if DOS
			mobj_t* mo;
			uint angle;

			switch (arti)
			{
				case arti_invulnerability:
					if (!P_GivePower(player, pw_invulnerability))
					{
						return (false);
					}
					break;
				case arti_invisibility:
					if (!P_GivePower(player, pw_invisibility))
					{
						return (false);
					}
					break;
				case arti_health:
					if (!P_GiveBody(player, 25))
					{
						return (false);
					}
					break;
				case arti_superhealth:
					if (!P_GiveBody(player, 100))
					{
						return (false);
					}
					break;
				case arti_tomeofpower:
					if (player.chickenTics)
					{ // Attempt to undo chicken
						if (P_UndoPlayerChicken(player) == false)
						{ // Failed
							P_DamageMobj(player.mo, NULL, NULL, 10000);
						}
						else
						{ // Succeeded
							player.chickenTics = 0;
							S_StartSound(player.mo, sfx_wpnup);
						}
					}
					else
					{
						if (!P_GivePower(player, pw_weaponlevel2))
						{
							return (false);
						}
						if (player.readyweapon == wp_staff)
						{
							P_SetPsprite(player, ps_weapon, S_STAFFREADY2_1);
						}
						else if (player.readyweapon == wp_gauntlets)
						{
							P_SetPsprite(player, ps_weapon, S_GAUNTLETREADY2_1);
						}
					}
					break;
				case arti_torch:
					if (!P_GivePower(player, pw_infrared))
					{
						return (false);
					}
					break;
				case arti_firebomb:
					angle = player.mo.angle >> ANGLETOFINESHIFT;
					mo = P_SpawnMobj(player.mo.x + 24 * finecosine[angle],
						player.mo.y + 24 * finesine[angle], player.mo.z - 15 * FRACUNIT *
						(player.mo.flags2 & MF2_FEETARECLIPPED != 0), MT_FIREBOMB);
					mo.target = player.mo;
					break;
				case arti_egg:
					mo = player.mo;
					P_SpawnPlayerMissile(mo, MT_EGGFX);
					P_SPMAngle(mo, MT_EGGFX, mo.angle - (ANG45 / 6));
					P_SPMAngle(mo, MT_EGGFX, mo.angle + (ANG45 / 6));
					P_SPMAngle(mo, MT_EGGFX, mo.angle - (ANG45 / 3));
					P_SPMAngle(mo, MT_EGGFX, mo.angle + (ANG45 / 3));
					break;
				case arti_fly:
					if (!P_GivePower(player, pw_flight))
					{
						return (false);
					}
					break;
				case arti_teleport:
					P_ArtiTele(player);
					break;
				default:
					return (false);
			}
#endif
			return (true);
		}
	}
}
