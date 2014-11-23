using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


//----------------------------------------------------------------------------
// Port - Done
//----------------------------------------------------------------------------

namespace HereticXNA
{
	class p_enemy
	{

		// P_enemy.c


		// Macros

		public const int MAX_BOSS_SPOTS = 8;

		// Types

		public class BossSpot_t
		{
			public int x;
			public int y;
			public uint angle;
		} ;

		// Private Data

		public static BossSpot_t[] BossSpots = new BossSpot_t[MAX_BOSS_SPOTS] {
			new BossSpot_t(),
			new BossSpot_t(),
			new BossSpot_t(),
			new BossSpot_t(),
			new BossSpot_t(),
			new BossSpot_t(),
			new BossSpot_t(),
			new BossSpot_t()
		};
		private static int BossSpotCount;

		//----------------------------------------------------------------------------
		//
		// PROC P_InitMonsters
		//
		// Called at level load.
		//
		//----------------------------------------------------------------------------

		public static void P_InitMonsters()
		{
			BossSpotCount = 0;
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_AddBossSpot
		//
		//----------------------------------------------------------------------------

		public static void P_AddBossSpot(int x, int y, uint angle)
		{
			if (BossSpotCount == MAX_BOSS_SPOTS)
			{
				i_ibm.I_Error("Too many boss spots.");
			}
			BossSpots[BossSpotCount].x = x;
			BossSpots[BossSpotCount].y = y;
			BossSpots[BossSpotCount].angle = angle;
			BossSpotCount++;
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_RecursiveSound
		//
		//----------------------------------------------------------------------------

		public static DoomDef.mobj_t soundtarget;

		public static void P_RecursiveSound(r_local.sector_t sec, int soundblocks)
		{
			int i;
			r_local.line_t check;
			r_local.sector_t other;

			// Wake up all monsters in this sector
			if (sec.validcount == r_main.validcount && sec.soundtraversed <= soundblocks + 1)
			{ // Already flooded
				return;
			}
			sec.validcount = r_main.validcount;
			sec.soundtraversed = soundblocks + 1;
			sec.soundtarget = soundtarget;
			for (i = 0; i < sec.linecount; i++)
			{
				check = p_setup.linebuffer[sec.linesi + i];
				if ((check.flags & DoomData.ML_TWOSIDED) == 0)
				{
					continue;
				}
				p_maputl.P_LineOpening(check);
				if (p_maputl.openrange <= 0)
				{ // Closed door
					continue;
				}
				if (p_setup.sides[check.sidenum[0]].sector == sec)
				{
					other = p_setup.sides[check.sidenum[1]].sector;
				}
				else
				{
					other = p_setup.sides[check.sidenum[0]].sector;
				}
				if ((check.flags & DoomData.ML_SOUNDBLOCK) != 0)
				{
					if (soundblocks == 0)
					{
						P_RecursiveSound(other, 1);
					}
				}
				else
				{
					P_RecursiveSound(other, soundblocks);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_NoiseAlert
		//
		// If a monster yells at a player, it will alert other monsters to the
		// player.
		//
		//----------------------------------------------------------------------------

		public static void P_NoiseAlert(DoomDef.mobj_t target, DoomDef.mobj_t emmiter)
		{
			soundtarget = target;
			r_main.validcount++;
			P_RecursiveSound(emmiter.subsector.sector, 0);
		}

		//----------------------------------------------------------------------------
		//
		// FUNC P_CheckMeleeRange
		//
		//----------------------------------------------------------------------------

		public static bool P_CheckMeleeRange(DoomDef.mobj_t actor)
		{
			DoomDef.mobj_t mo;
			int dist;

			if (actor.target == null)
			{
				return (false);
			}
			mo = actor.target;
			dist = p_maputl.P_AproxDistance(mo.x - actor.x, mo.y - actor.y);
			if (dist >= p_local.MELEERANGE)
			{
				return (false);
			}
			if (!p_sight.P_CheckSight(actor, mo))
			{
				return (false);
			}
			if (mo.z > actor.z + actor.height)
			{ // Target is higher than the attacker
				return (false);
			}
			else if (actor.z > mo.z + mo.height)
			{ // Attacker is higher
				return (false);
			}
			return (true);
		}

		//----------------------------------------------------------------------------
		//
		// FUNC P_CheckMissileRange
		//
		//----------------------------------------------------------------------------

		public static bool P_CheckMissileRange(DoomDef.mobj_t actor)
		{
			int dist;

			if (!p_sight.P_CheckSight(actor, actor.target))
			{
				return (false);
			}
			if ((actor.flags & DoomDef.MF_JUSTHIT) != 0)
			{ // The target just hit the enemy, so fight back!
				actor.flags &= ~DoomDef.MF_JUSTHIT;
				return (true);
			}
			if (actor.reactiontime != 0)
			{ // Don't attack yet
				return (false);
			}
			dist = (p_maputl.P_AproxDistance(actor.x - actor.target.x,
				actor.y - actor.target.y) >> DoomDef.FRACBITS) - 64;
			if (actor.infol.meleestate == 0)
			{ // No melee attack, so fire more frequently
				dist -= 128;
			}
			if (actor.type == info.mobjtype_t.MT_IMP)
			{ // Imp's fly attack from far away
				dist >>= 1;
			}
			if (dist > 200)
			{
				dist = 200;
			}
			if (m_misc.P_Random() < dist)
			{
				return (false);
			}
			return (true);
		}

		/*
		================
		=
		= P_Move
		=
		= Move in the current direction
		= returns false if the move is blocked
		================
		*/

		static public int[] xspeed = new int[8] { DoomDef.FRACUNIT, 47000, 0, -47000, -DoomDef.FRACUNIT, -47000, 0, 47000 };
		static public int[] yspeed = new int[8] { 0, 47000, DoomDef.FRACUNIT, 47000, 0, -47000, -DoomDef.FRACUNIT, -47000 };

		public const int MAXSPECIALCROSS = 8;

		public static bool P_Move(DoomDef.mobj_t actor)
		{
			int tryx, tryy;
			r_local.line_t ld;
			bool good;

			if (actor.movedir == (int)p_local.dirtype_t.DI_NODIR)
			{
				return (false);
			}
			tryx = actor.x + actor.infol.speed * xspeed[actor.movedir];
			tryy = actor.y + actor.infol.speed * yspeed[actor.movedir];
			if (!p_map.P_TryMove(actor, tryx, tryy))
			{ // open any specials
				if ((actor.flags & DoomDef.MF_FLOAT) != 0 && p_map.floatok)
				{ // must adjust height
					if (actor.z < p_map.tmfloorz)
					{
						actor.z += p_local.FLOATSPEED;
					}
					else
					{
						actor.z -= p_local.FLOATSPEED;
					}
					actor.flags |= DoomDef.MF_INFLOAT;
					return (true);
				}
				if (p_map.numspechit == 0)
				{
					return false;
				}
				actor.movedir = (int)p_local.dirtype_t.DI_NODIR;
				good = false;
				while ((p_map.numspechit--) != 0)
				{
					ld = p_map.spechit[p_map.numspechit];
					// if the special isn't a door that can be opened, return false
					if (p_switch.P_UseSpecialLine(actor, ld))
					{
						good = true;
					}
				}
				return (good);
			}
			else
			{
				actor.flags &= ~DoomDef.MF_INFLOAT;
			}
			if ((actor.flags & DoomDef.MF_FLOAT) == 0)
			{
				if (actor.z > actor.floorz)
				{
					p_mobj.P_HitFloor(actor);
				}
				actor.z = actor.floorz;
			}
			return (true);
		}

		//----------------------------------------------------------------------------
		//
		// FUNC P_TryWalk
		//
		// Attempts to move actor in its current (ob.moveangle) direction.
		// If blocked by either a wall or an actor returns FALSE.
		// If move is either clear of block only by a door, returns TRUE and sets.
		// If a door is in the way, an OpenDoor call is made to start it opening.
		//
		//----------------------------------------------------------------------------

		public static bool P_TryWalk(DoomDef.mobj_t actor)
		{
			if (!P_Move(actor))
			{
				return (false);
			}
			actor.movecount = m_misc.P_Random() & 15;
			return (true);
		}

		/*
		================
		=
		= P_NewChaseDir
		=
		================
		*/

		public static p_local.dirtype_t[] opposite = new p_local.dirtype_t[]
{p_local.dirtype_t.DI_WEST, p_local.dirtype_t.DI_SOUTHWEST, p_local.dirtype_t.DI_SOUTH, p_local.dirtype_t.DI_SOUTHEAST, p_local.dirtype_t.DI_EAST, p_local.dirtype_t.DI_NORTHEAST,
p_local.dirtype_t.DI_NORTH, p_local.dirtype_t.DI_NORTHWEST, p_local.dirtype_t.DI_NODIR};

		public static p_local.dirtype_t[] diags = new p_local.dirtype_t[4]{
	p_local.dirtype_t.DI_NORTHWEST,
	p_local.dirtype_t.DI_NORTHEAST,
	p_local.dirtype_t.DI_SOUTHWEST,
	p_local.dirtype_t.DI_SOUTHEAST
};

		public static void P_NewChaseDir(DoomDef.mobj_t actor)
		{
			int deltax, deltay;
			p_local.dirtype_t[] d = new p_local.dirtype_t[3];
			p_local.dirtype_t tdir, olddir, turnaround;

			if (actor.target == null)
				i_ibm.I_Error("P_NewChaseDir: called with no target");

			olddir = (p_local.dirtype_t)actor.movedir;
			turnaround = opposite[(int)olddir];

			deltax = actor.target.x - actor.x;
			deltay = actor.target.y - actor.y;
			if (deltax > 10 * DoomDef.FRACUNIT)
				d[1] = p_local.dirtype_t.DI_EAST;
			else if (deltax < -10 * DoomDef.FRACUNIT)
				d[1] = p_local.dirtype_t.DI_WEST;
			else
				d[1] = p_local.dirtype_t.DI_NODIR;
			if (deltay < -10 * DoomDef.FRACUNIT)
				d[2] = p_local.dirtype_t.DI_SOUTH;
			else if (deltay > 10 * DoomDef.FRACUNIT)
				d[2] = p_local.dirtype_t.DI_NORTH;
			else
				d[2] = p_local.dirtype_t.DI_NODIR;

			// try direct route
			if (d[1] != p_local.dirtype_t.DI_NODIR && d[2] != p_local.dirtype_t.DI_NODIR)
			{
				actor.movedir = (int)diags[(((deltay < 0) ? 1 : 0) << 1) + (deltax > 0 ? 1 : 0)];
				if (actor.movedir != (int)turnaround && P_TryWalk(actor))
					return;
			}

			// try other directions
			if (m_misc.P_Random() > 200 || Math.Abs(deltay) > Math.Abs(deltax))
			{
				tdir = d[1];
				d[1] = d[2];
				d[2] = tdir;
			}

			if (d[1] == turnaround)
				d[1] = p_local.dirtype_t.DI_NODIR;
			if (d[2] == turnaround)
				d[2] = p_local.dirtype_t.DI_NODIR;

			if (d[1] != p_local.dirtype_t.DI_NODIR)
			{
				actor.movedir = (int)d[1];
				if (P_TryWalk(actor))
					return;     /*either moved forward or attacked*/
			}

			if (d[2] != p_local.dirtype_t.DI_NODIR)
			{
				actor.movedir = (int)d[2];
				if (P_TryWalk(actor))
					return;
			}

			/* there is no direct path to the player, so pick another direction */

			if (olddir != p_local.dirtype_t.DI_NODIR)
			{
				actor.movedir = (int)olddir;
				if (P_TryWalk(actor))
					return;
			}

			if ((m_misc.P_Random() & 1) != 0) 	/*randomly determine direction of search*/
			{
				for (tdir = p_local.dirtype_t.DI_EAST; tdir <= p_local.dirtype_t.DI_SOUTHEAST; tdir++)
				{
					if (tdir != turnaround)
					{
						actor.movedir = (int)tdir;
						if (P_TryWalk(actor))
							return;
					}
				}
			}
			else
			{
				for (tdir = p_local.dirtype_t.DI_SOUTHEAST; tdir >= p_local.dirtype_t.DI_EAST; tdir--)
				{
					if (tdir != turnaround)
					{
						actor.movedir = (int)tdir;
						if (P_TryWalk(actor))
							return;
					}
				}
			}

			if (turnaround != p_local.dirtype_t.DI_NODIR)
			{
				actor.movedir = (int)turnaround;
				if (P_TryWalk(actor))
					return;
			}

			actor.movedir = (int)p_local.dirtype_t.DI_NODIR;		// can't move
		}

		//---------------------------------------------------------------------------
		//
		// FUNC P_LookForMonsters
		//
		//---------------------------------------------------------------------------

		public const int MONS_LOOK_RANGE = (20 * 64 * DoomDef.FRACUNIT);
		public const int MONS_LOOK_LIMIT = 64;

		public static bool P_LookForMonsters(DoomDef.mobj_t actor)
		{
			int count;
			DoomDef.mobj_t mo;
			DoomDef.thinker_t think;

			if (!p_sight.P_CheckSight(g_game.players[0].mo, actor))
			{ // Player can't see monster
				return (false);
			}
			count = 0;
			for (think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
			{
				if (!(think.function is p_mobj.P_MobjThinker))
				{ // Not a mobj thinker
					continue;
				}
				mo = think.function.obj as DoomDef.mobj_t;
				if ((mo.flags & DoomDef.MF_COUNTKILL) == 0 || (mo == actor) || (mo.health <= 0))
				{ // Not a valid monster
					continue;
				}
				if (p_maputl.P_AproxDistance(actor.x - mo.x, actor.y - mo.y)
					> MONS_LOOK_RANGE)
				{ // Out of range
					continue;
				}
				if (m_misc.P_Random() < 16)
				{ // Skip
					continue;
				}
				if (count++ > MONS_LOOK_LIMIT)
				{ // Stop searching
					return (false);
				}
				if (!p_sight.P_CheckSight(actor, mo))
				{ // Out of sight
					continue;
				}
				// Found a target monster
				actor.target = mo;
				return (true);
			}
			return (false);
		}


		/*
		================
		=
		= P_LookForPlayers
		=
		= If allaround is false, only look 180 degrees in front
		= returns true if a player is targeted
		================
		*/

		public static bool P_LookForPlayers(DoomDef.mobj_t actor, bool allaround)
		{
			int c;
			int stop;
			DoomDef.player_t player;
			r_local.sector_t sector;
			uint an;
			int dist;

			if (!g_game.netgame && g_game.players[0].health <= 0)
			{ // Single player game and player is dead, look for monsters
				return (P_LookForMonsters(actor));
			}
			sector = actor.subsector.sector;
			c = 0;
			stop = (actor.lastlook - 1) & 3;
			for (; ; actor.lastlook = (actor.lastlook + 1) & 3)
			{
				if (!g_game.playeringame[actor.lastlook])
					continue;

				if (c++ == 2 || actor.lastlook == stop)
					return false;		// done looking

				player = g_game.players[actor.lastlook];
				if (player.health <= 0)
					continue;		// dead
				if (!p_sight.P_CheckSight(actor, player.mo))
					continue;		// out of sight

				if (!allaround)
				{
					an = r_main.R_PointToAngle2(actor.x, actor.y,
					player.mo.x, player.mo.y) - actor.angle;
					if (an > DoomDef.ANG90 && an < DoomDef.ANG270)
					{
						dist = p_maputl.P_AproxDistance(player.mo.x - actor.x,
							player.mo.y - actor.y);
						// if real close, react anyway
						if (dist > p_local.MELEERANGE)
							continue;		// behind back
					}
				}
				if ((player.mo.flags & DoomDef.MF_SHADOW) != 0)
				{ // Player is invisible
					if ((p_maputl.P_AproxDistance(player.mo.x - actor.x,
						player.mo.y - actor.y) > 2 * p_local.MELEERANGE)
						&& p_maputl.P_AproxDistance(player.mo.momx, player.mo.momy)
						< 5 * DoomDef.FRACUNIT)
					{ // Player is sneaking - can't detect
						return (false);
					}
					if (m_misc.P_Random() < 225)
					{ // Player isn't sneaking, but still didn't detect
						return (false);
					}
				}
				actor.target = player.mo;
				return (true);
			}
			//return (false); // [dsl] Can not reach that
		}

		/*
		===============================================================================

								ACTION ROUTINES

		===============================================================================
		*/

		/*
		==============
		=
		= A_Look
		=
		= Stay in state until a player is sighted
		=
		==============
		*/
		public class A_Look : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t targ;

				actor.threshold = 0;		// any shot will wake up
				targ = actor.subsector.sector.soundtarget;
				if (targ != null && (targ.flags & DoomDef.MF_SHOOTABLE) != 0)
				{
					actor.target = targ;
					if ((actor.flags & DoomDef.MF_AMBUSH) != 0)
					{
						if (p_sight.P_CheckSight(actor, actor.target))
							goto seeyou;
					}
					else
						goto seeyou;
				}


				if (!P_LookForPlayers(actor, false))
					return;

				// go into chase state
			seeyou:
				if (actor.infol.seesound != 0)
				{
					int sound;
					sound = actor.infol.seesound;
					if ((actor.flags2 & DoomDef.MF2_BOSS) != 0)
					{ // Full volume
						i_ibm.S_StartSound(null, sound);
					}
					else
					{
						i_ibm.S_StartSound(actor, sound);
					}
				}
				p_mobj.P_SetMobjState(actor, (info.statenum_t)actor.infol.seestate);
			}
		}

		/*
		==============
		=
		= A_Chase
		=
		= Actor has a melee attack, so it tries to close as fast as possible
		=
		==============
		*/
		public class A_Chase : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				int delta;

				if (actor.reactiontime != 0)
				{
					actor.reactiontime--;
				}

				// Modify target threshold
				if (actor.threshold != 0)
				{
					actor.threshold--;
				}

				if (g_game.gameskill == DoomDef.skill_t.sk_nightmare)
				{ // Monsters move faster in nightmare mode
					actor.tics -= actor.tics / 2;
					if (actor.tics < 3)
					{
						actor.tics = 3;
					}
				}

				//
				// turn towards movement direction if not there yet
				//
				if (actor.movedir < 8)
				{
					actor.angle = (uint)(actor.angle & (7 << 29));
					delta = (int)(actor.angle - (actor.movedir << 29));
					if (delta > 0)
					{
						actor.angle -= DoomDef.ANG90 / 2;
					}
					else if (delta < 0)
					{
						actor.angle += DoomDef.ANG90 / 2;
					}
				}

				if (actor.target == null || (actor.target.flags & DoomDef.MF_SHOOTABLE) == 0)
				{ // look for a new target
					if (P_LookForPlayers(actor, true))
					{ // got a new target
						return;
					}
					p_mobj.P_SetMobjState(actor, (info.statenum_t)actor.infol.spawnstate);
					return;
				}

				//
				// don't attack twice in a row
				//
				if ((actor.flags & DoomDef.MF_JUSTATTACKED) != 0)
				{
					actor.flags &= ~DoomDef.MF_JUSTATTACKED;
					if (g_game.gameskill != DoomDef.skill_t.sk_nightmare)
						P_NewChaseDir(actor);
					return;
				}

				//
				// check for melee attack
				//	
				if (actor.infol.meleestate != 0 && P_CheckMeleeRange(actor))
				{
					if (actor.infol.attacksound != 0)
						i_ibm.S_StartSound(actor, actor.infol.attacksound);
					p_mobj.P_SetMobjState(actor, (info.statenum_t)actor.infol.meleestate);
					return;
				}

				//
				// check for missile attack
				//
				if (actor.infol.missilestate != 0)
				{
					if (g_game.gameskill < DoomDef.skill_t.sk_nightmare && actor.movecount != 0)
						goto nomissile;
					if (!P_CheckMissileRange(actor))
						goto nomissile;
					p_mobj.P_SetMobjState(actor, (info.statenum_t)actor.infol.missilestate);
					actor.flags |= DoomDef.MF_JUSTATTACKED;
					return;
				}
			nomissile:

				//
				// possibly choose another target
				//
				if (g_game.netgame && actor.threshold == 0 && !p_sight.P_CheckSight(actor, actor.target))
				{
					if (P_LookForPlayers(actor, true))
						return;		// got a new target
				}

				//
				// chase towards player
				//
				if (--actor.movecount < 0 || !P_Move(actor))
				{
					P_NewChaseDir(actor);
				}

				//
				// make active sound
				//
				if (actor.infol.activesound != 0 && m_misc.P_Random() < 3)
				{
					if (actor.type == info.mobjtype_t.MT_WIZARD && m_misc.P_Random() < 128)
					{
						i_ibm.S_StartSound(actor, actor.infol.seesound);
					}
					else if (actor.type == info.mobjtype_t.MT_SORCERER2)
					{
						i_ibm.S_StartSound(null, actor.infol.activesound);
					}
					else
					{
						i_ibm.S_StartSound(actor, actor.infol.activesound);
					}
				}
			}
		}
		//----------------------------------------------------------------------------
		//
		// PROC A_FaceTarget
		//
		//----------------------------------------------------------------------------
		public class A_FaceTarget : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.target == null)
				{
					return;
				}
				actor.flags &= ~DoomDef.MF_AMBUSH;
				actor.angle = r_main.R_PointToAngle2(actor.x, actor.y, actor.target.x,
					actor.target.y);
				if ((actor.target.flags & DoomDef.MF_SHADOW) != 0)
				{ // Target is a ghost
					actor.angle += (uint)((m_misc.P_Random() - m_misc.P_Random()) << 21);
				}
			}
		}

		public static A_FaceTarget s_A_FaceTarget = new A_FaceTarget();


		//----------------------------------------------------------------------------
		//
		// PROC A_Pain
		//
		//----------------------------------------------------------------------------
		public class A_Pain : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.infol.painsound != 0)
				{
					i_ibm.S_StartSound(actor, actor.infol.painsound);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_DripBlood
		//
		//----------------------------------------------------------------------------
		public class A_DripBlood : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;

				mo = p_mobj.P_SpawnMobj(actor.x + ((m_misc.P_Random() - m_misc.P_Random()) << 11),
					actor.y + ((m_misc.P_Random() - m_misc.P_Random()) << 11), actor.z, info.mobjtype_t.MT_BLOOD);
				mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 10;
				mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 10;
				mo.flags2 |= DoomDef.MF2_LOGRAV;
			}
		}
		//----------------------------------------------------------------------------
		//
		// PROC A_KnightAttack
		//
		//----------------------------------------------------------------------------
		public class A_KnightAttack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.target == null)
				{
					return;
				}
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, DoomDef.HITDICE(3));
					i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_kgtat2);
					return;
				}
				// Throw axe
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				if (actor.type == info.mobjtype_t.MT_KNIGHTGHOST || m_misc.P_Random() < 40)
				{ // Red axe
					p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_REDAXE);
					return;
				}
				// Green axe
				p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_KNIGHTAXE);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_ImpExplode
		//
		//----------------------------------------------------------------------------

		public class A_ImpExplode : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;

				mo = p_mobj.P_SpawnMobj(actor.x, actor.y, actor.z, info.mobjtype_t.MT_IMPCHUNK1);
				mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 10;
				mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 10;
				mo.momz = 9 * DoomDef.FRACUNIT;
				mo = p_mobj.P_SpawnMobj(actor.x, actor.y, actor.z, info.mobjtype_t.MT_IMPCHUNK2);
				mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 10;
				mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 10;
				mo.momz = 9 * DoomDef.FRACUNIT;
				if (actor.special1 == 666)
				{ // Extreme death crash
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_IMP_XCRASH1);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_BeastPuff
		//
		//----------------------------------------------------------------------------

		public class A_BeastPuff : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (m_misc.P_Random() > 64)
				{
					p_mobj.P_SpawnMobj(actor.x + ((m_misc.P_Random() - m_misc.P_Random()) << 10),
						actor.y + ((m_misc.P_Random() - m_misc.P_Random()) << 10),
						actor.z + ((m_misc.P_Random() - m_misc.P_Random()) << 10), info.mobjtype_t.MT_PUFFY);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_ImpMeAttack
		//
		//----------------------------------------------------------------------------
		public class A_ImpMeAttack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.target == null)
				{
					return;
				}
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, 5 + (m_misc.P_Random() & 7));
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_ImpMsAttack
		//
		//----------------------------------------------------------------------------
		public class A_ImpMsAttack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t dest;
				uint an;
				int dist;

				if (actor.target == null || m_misc.P_Random() > 64)
				{
					p_mobj.P_SetMobjState(actor, (info.statenum_t)actor.infol.seestate);
					return;
				}
				dest = actor.target;
				actor.flags |= DoomDef.MF_SKULLFLY;
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				s_A_FaceTarget.action(actor);
				an = actor.angle >> (int)DoomDef.ANGLETOFINESHIFT;
				actor.momx = DoomDef.FixedMul(12 * DoomDef.FRACUNIT, r_main.finecosine(an));
				actor.momy = DoomDef.FixedMul(12 * DoomDef.FRACUNIT, tables.finesine[an]);
				dist = p_maputl.P_AproxDistance(dest.x - actor.x, dest.y - actor.y);
				dist = dist / (12 * DoomDef.FRACUNIT);
				if (dist < 1)
				{
					dist = 1;
				}
				actor.momz = (dest.z + (dest.height >> 1) - actor.z) / dist;
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_ImpMsAttack2
		//
		// Fireball attack of the imp leader.
		//
		//----------------------------------------------------------------------------
		public class A_ImpMsAttack2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.target == null)
				{
					return;
				}
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, 5 + (m_misc.P_Random() & 7));
					return;
				}
				p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_IMPBALL);
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_ImpDeath
		//
		//----------------------------------------------------------------------------
		public class A_ImpDeath : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.flags &= ~DoomDef.MF_SOLID;
				actor.flags2 |= DoomDef.MF2_FOOTCLIP;
				if (actor.z <= actor.floorz)
				{
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_IMP_CRASH1);
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_ImpXDeath1
		//
		//----------------------------------------------------------------------------
		public class A_ImpXDeath1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.flags &= ~DoomDef.MF_SOLID;
				actor.flags |= DoomDef.MF_NOGRAVITY;
				actor.flags2 |= DoomDef.MF2_FOOTCLIP;
				actor.special1 = 666; // Flag the crash routine
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_ImpXDeath2
		//
		//----------------------------------------------------------------------------
		public class A_ImpXDeath2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.flags &= ~DoomDef.MF_NOGRAVITY;
				if (actor.z <= actor.floorz)
				{
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_IMP_CRASH1);
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// FUNC P_UpdateChicken
		//
		// Returns true if the chicken morphs.
		//
		//----------------------------------------------------------------------------

		public static bool P_UpdateChicken(DoomDef.mobj_t actor, int tics)
		{
			DoomDef.mobj_t fog;
			int x;
			int y;
			int z;
			info.mobjtype_t moType;
			DoomDef.mobj_t mo;
			DoomDef.mobj_t oldChicken = new DoomDef.mobj_t();

			actor.special1 -= tics;
			if (actor.special1 > 0)
			{
				return (false);
			}
			moType = (info.mobjtype_t)actor.special2;
			x = actor.x;
			y = actor.y;
			z = actor.z;
			oldChicken.set(actor);
			p_mobj.P_SetMobjState(actor, info.statenum_t.S_FREETARGMOBJ);
			mo = p_mobj.P_SpawnMobj(x, y, z, moType);
			if (p_map.P_TestMobjLocation(mo) == false)
			{ // Didn't fit
				p_mobj.P_RemoveMobj(mo);
				mo = p_mobj.P_SpawnMobj(x, y, z, info.mobjtype_t.MT_CHICKEN);
				mo.angle = oldChicken.angle;
				mo.flags = oldChicken.flags;
				mo.health = oldChicken.health;
				mo.target = oldChicken.target;
				mo.special1 = 5 * 35; // Next try in 5 seconds
				mo.special2 = (int)moType;
				return (false);
			}
			mo.angle = oldChicken.angle;
			mo.target = oldChicken.target;
			fog = p_mobj.P_SpawnMobj(x, y, z + DoomDef.TELEFOGHEIGHT, info.mobjtype_t.MT_TFOG);
			i_ibm.S_StartSound(fog, (int)sounds.sfxenum_t.sfx_telept);
			return (true);
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_ChicAttack
		//
		//----------------------------------------------------------------------------
		public class A_ChicAttack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (P_UpdateChicken(actor, 18))
				{
					return;
				}
				if (actor.target == null)
				{
					return;
				}
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, 1 + (m_misc.P_Random() & 1));
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_ChicLook
		//
		//----------------------------------------------------------------------------
		public class A_ChicLook : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (P_UpdateChicken(actor, 10))
				{
					return;
				}
				(new A_Look()).action(actor);
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_ChicChase
		//
		//----------------------------------------------------------------------------
		public class A_ChicChase : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (P_UpdateChicken(actor, 3))
				{
					return;
				}
				(new A_Chase()).action(actor);
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_ChicPain
		//
		//----------------------------------------------------------------------------
		public class A_ChicPain : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (P_UpdateChicken(actor, 10))
				{
					return;
				}
				i_ibm.S_StartSound(actor, actor.infol.painsound);
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_Feathers
		//
		//----------------------------------------------------------------------------
		public class A_Feathers : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				int i;
				int count;
				DoomDef.mobj_t mo;

				if (actor.health > 0)
				{ // Pain
					count = m_misc.P_Random() < 32 ? 2 : 1;
				}
				else
				{ // Death
					count = 5 + (m_misc.P_Random() & 3);
				}
				for (i = 0; i < count; i++)
				{
					mo = p_mobj.P_SpawnMobj(actor.x, actor.y, actor.z + 20 * DoomDef.FRACUNIT,
						info.mobjtype_t.MT_FEATHER);
					mo.target = actor;
					mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 8;
					mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 8;
					mo.momz = DoomDef.FRACUNIT + (m_misc.P_Random() << 9);
					p_mobj.P_SetMobjState(mo, info.statenum_t.S_FEATHER1 + (m_misc.P_Random() & 7));
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_MummyAttack
		//
		//----------------------------------------------------------------------------
		public class A_MummyAttack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.target == null)
				{
					return;
				}
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, DoomDef.HITDICE(2));
					i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_mumat2);
					return;
				}
				i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_mumat1);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_MummyAttack2
		//
		// Mummy leader missile attack.
		//
		//----------------------------------------------------------------------------

		public class A_MummyAttack2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;

				if (actor.target == null)
				{
					return;
				}
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, DoomDef.HITDICE(2));
					return;
				}
				mo = p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_MUMMYFX1);
				if (mo != null)
				{
					mo.special1AsTarget = actor.target;
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_MummyFX1Seek
		//
		//----------------------------------------------------------------------------
		public class A_MummyFX1Seek : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				p_mobj.P_SeekerMissile(actor, DoomDef.ANGLE_1 * 10, DoomDef.ANGLE_1 * 20);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_MummySoul
		//
		//----------------------------------------------------------------------------
		public class A_MummySoul : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t mummy)
			{
				DoomDef.mobj_t mo;

				mo = p_mobj.P_SpawnMobj(mummy.x, mummy.y, mummy.z + 10 * DoomDef.FRACUNIT, info.mobjtype_t.MT_MUMMYSOUL);
				mo.momz = DoomDef.FRACUNIT;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_Sor1Pain
		//
		//----------------------------------------------------------------------------
		public class A_Sor1Pain : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.special1 = 20; // Number of steps to walk fast
				(new A_Pain()).action(actor);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_Sor1Chase
		//
		//----------------------------------------------------------------------------
		public class A_Sor1Chase : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.special1 != 0)
				{
					actor.special1--;
					actor.tics -= 3;
				}
				(new A_Chase()).action(actor);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_Srcr1Attack
		//
		// Sorcerer demon attack.
		//
		//----------------------------------------------------------------------------
		public class A_Srcr1Attack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;
				int momz;
				uint angle;

				if (actor.target == null)
				{
					return;
				}
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, DoomDef.HITDICE(8));
					return;
				}
				if (actor.health > (actor.infol.spawnhealth / 3) * 2)
				{ // Spit one fireball
					p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_SRCRFX1);
				}
				else
				{ // Spit three fireballs
					mo = p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_SRCRFX1);
					if (mo != null)
					{
						momz = mo.momz;
						angle = mo.angle;
						p_mobj.P_SpawnMissileAngle(actor, info.mobjtype_t.MT_SRCRFX1, angle - DoomDef.ANGLE_1 * 3, momz);
						p_mobj.P_SpawnMissileAngle(actor, info.mobjtype_t.MT_SRCRFX1, angle + DoomDef.ANGLE_1 * 3, momz);
					}
					if (actor.health < actor.infol.spawnhealth / 3)
					{ // Maybe attack again
						if (actor.special1 != 0)
						{ // Just attacked, so don't attack again
							actor.special1 = 0;
						}
						else
						{ // Set state to attack again
							actor.special1 = 1;
							p_mobj.P_SetMobjState(actor, info.statenum_t.S_SRCR1_ATK4);
						}
					}
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_SorcererRise
		//
		//----------------------------------------------------------------------------
		public class A_SorcererRise : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;

				actor.flags &= ~DoomDef.MF_SOLID;
				mo = p_mobj.P_SpawnMobj(actor.x, actor.y, actor.z, info.mobjtype_t.MT_SORCERER2);
				p_mobj.P_SetMobjState(mo, info.statenum_t.S_SOR2_RISE1);
				mo.angle = actor.angle;
				mo.target = actor.target;
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC P_DSparilTeleport
		//
		//----------------------------------------------------------------------------

		public static void P_DSparilTeleport(DoomDef.mobj_t actor)
		{
			int i;
			int x;
			int y;
			int prevX;
			int prevY;
			int prevZ;
			DoomDef.mobj_t mo;

			if (BossSpotCount == 0)
			{ // No spots
				return;
			}
			i = m_misc.P_Random();
			do
			{
				i++;
				x = BossSpots[i % BossSpotCount].x;
				y = BossSpots[i % BossSpotCount].y;
			} while (p_maputl.P_AproxDistance(actor.x - x, actor.y - y) < 128 * DoomDef.FRACUNIT);
			prevX = actor.x;
			prevY = actor.y;
			prevZ = actor.z;
			if (p_map.P_TeleportMove(actor, x, y))
			{
				mo = p_mobj.P_SpawnMobj(prevX, prevY, prevZ, info.mobjtype_t.MT_SOR2TELEFADE);
				i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_telept);
				p_mobj.P_SetMobjState(actor, info.statenum_t.S_SOR2_TELE1);
				i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_telept);
				actor.z = actor.floorz;
				actor.angle = BossSpots[i % BossSpotCount].angle;
				actor.momx = actor.momy = actor.momz = 0;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_Srcr2Decide
		//
		//----------------------------------------------------------------------------
		static int[] chance = new int[]
		{
			192, 120, 120, 120, 64, 64, 32, 16, 0
		};
		public class A_Srcr2Decide : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (BossSpotCount == 0)
				{ // No spots
					return;
				}
				if (m_misc.P_Random() < chance[actor.health / (actor.infol.spawnhealth / 8)])
				{
					P_DSparilTeleport(actor);
				}
			}
		}
		//----------------------------------------------------------------------------
		//
		// PROC A_Srcr2Attack
		//
		//----------------------------------------------------------------------------
		public class A_Srcr2Attack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				int chance;

				if (actor.target == null)
				{
					return;
				}
				i_ibm.S_StartSound(null, actor.infol.attacksound);
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, DoomDef.HITDICE(20));
					return;
				}
				chance = actor.health < actor.infol.spawnhealth / 2 ? 96 : 48;
				if (m_misc.P_Random() < chance)
				{ // Wizard spawners
					p_mobj.P_SpawnMissileAngle(actor, info.mobjtype_t.MT_SOR2FX2,
						actor.angle - DoomDef.ANG45, DoomDef.FRACUNIT / 2);
					p_mobj.P_SpawnMissileAngle(actor, info.mobjtype_t.MT_SOR2FX2,
						actor.angle + DoomDef.ANG45, DoomDef.FRACUNIT / 2);
				}
				else
				{ // Blue bolt
					p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_SOR2FX1);
				}
			}
		}
		//----------------------------------------------------------------------------
		//
		// PROC A_BlueSpark
		//
		//----------------------------------------------------------------------------
		public class A_BlueSpark : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				int i;
				DoomDef.mobj_t mo;

				for (i = 0; i < 2; i++)
				{
					mo = p_mobj.P_SpawnMobj(actor.x, actor.y, actor.z, info.mobjtype_t.MT_SOR2FXSPARK);
					mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 9;
					mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 9;
					mo.momz = DoomDef.FRACUNIT + (m_misc.P_Random() << 8);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_GenWizard
		//
		//----------------------------------------------------------------------------
		public class A_GenWizard : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;
				DoomDef.mobj_t fog;

				mo = p_mobj.P_SpawnMobj(actor.x, actor.y,
					actor.z - info.mobjinfo[(int)info.mobjtype_t.MT_WIZARD].height / 2, info.mobjtype_t.MT_WIZARD);
				if (p_map.P_TestMobjLocation(mo) == false)
				{ // Didn't fit
					p_mobj.P_RemoveMobj(mo);
					return;
				}
				actor.momx = actor.momy = actor.momz = 0;
				p_mobj.P_SetMobjState(actor, (info.statenum_t)info.mobjinfo[(int)actor.type].deathstate);
				actor.flags &= ~DoomDef.MF_MISSILE;
				fog = p_mobj.P_SpawnMobj(actor.x, actor.y, actor.z, info.mobjtype_t.MT_TFOG);
				i_ibm.S_StartSound(fog, (int)sounds.sfxenum_t.sfx_telept);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_Sor2DthInit
		//
		//----------------------------------------------------------------------------
		public class A_Sor2DthInit : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.special1 = 7; // Animation loop counter
				P_Massacre(); // Kill monsters early
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_Sor2DthLoop
		//
		//----------------------------------------------------------------------------
		public class A_Sor2DthLoop : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if ((--actor.special1) != 0)
				{ // Need to loop
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_SOR2_DIE4);
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// D'Sparil Sound Routines
		//
		//----------------------------------------------------------------------------
		public class A_SorZap : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_sorzap);
			}
		}
		public class A_SorRise : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_sorrise);
			}
		}
		public class A_SorDSph : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_sordsph);
			}
		}
		public class A_SorDExp : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_sordexp);
			}
		}
		public class A_SorDBon : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_sordbon);
			}
		}
		public class A_SorSightSnd : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_sorsit);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_MinotaurAtk1
		//
		// Melee attack.
		//
		//----------------------------------------------------------------------------
		public class A_MinotaurAtk1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.player_t player;

				if (actor.target == null)
				{
					return;
				}
				i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_stfpow);
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, DoomDef.HITDICE(4));
					if ((player = actor.target.player) != null)
					{ // Squish the player
						player.deltaviewheight = -16 * DoomDef.FRACUNIT;
					}
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_MinotaurDecide
		//
		// Choose a missile attack.
		//
		//----------------------------------------------------------------------------

		public const int MNTR_CHARGE_SPEED = (13 * DoomDef.FRACUNIT);
		public class A_MinotaurDecide : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				uint angle;
				DoomDef.mobj_t target;
				int dist;

				target = actor.target;
				if (target == null)
				{
					return;
				}
				i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_minsit);
				dist = p_maputl.P_AproxDistance(actor.x - target.x, actor.y - target.y);
				if (target.z + target.height > actor.z
					&& target.z + target.height < actor.z + actor.height
					&& dist < 8 * 64 * DoomDef.FRACUNIT
					&& dist > 1 * 64 * DoomDef.FRACUNIT
					&& m_misc.P_Random() < 150)
				{ // Charge attack
					// Don't call the state function right away
					p_mobj.P_SetMobjStateNF(actor, info.statenum_t.S_MNTR_ATK4_1);
					actor.flags |= DoomDef.MF_SKULLFLY;
					(new A_FaceTarget()).action(actor);
					angle = actor.angle >> (int)DoomDef.ANGLETOFINESHIFT;
					actor.momx = DoomDef.FixedMul(MNTR_CHARGE_SPEED, r_main.finecosine(angle));
					actor.momy = DoomDef.FixedMul(MNTR_CHARGE_SPEED, tables.finesine[angle]);
					actor.special1 = 35 / 2; // Charge duration
				}
				else if (target.z == target.floorz
					&& dist < 9 * 64 * DoomDef.FRACUNIT
					&& m_misc.P_Random() < 220)
				{ // Floor fire attack
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_MNTR_ATK3_1);
					actor.special2 = 0;
				}
				else
				{ // Swing attack
					(new A_FaceTarget()).action(actor);
					// Don't need to call P_SetMobjState because the current state
					// falls through to the swing attack
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_MinotaurCharge
		//
		//----------------------------------------------------------------------------
		public class A_MinotaurCharge : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t puff;

				if (actor.special1 != 0)
				{
					puff = p_mobj.P_SpawnMobj(actor.x, actor.y, actor.z, info.mobjtype_t.MT_PHOENIXPUFF);
					puff.momz = 2 * DoomDef.FRACUNIT;
					actor.special1--;
				}
				else
				{
					actor.flags &= ~DoomDef.MF_SKULLFLY;
					p_mobj.P_SetMobjState(actor, (info.statenum_t)actor.infol.seestate);
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_MinotaurAtk2
		//
		// Swing attack.
		//
		//----------------------------------------------------------------------------
		public class A_MinotaurAtk2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;
				uint angle;
				int momz;

				if (actor.target == null)
				{
					return;
				}
				i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_minat2);
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, DoomDef.HITDICE(5));
					return;
				}
				mo = p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_MNTRFX1);
				if (mo != null)
				{
					i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_minat2);
					momz = mo.momz;
					angle = mo.angle;
					p_mobj.P_SpawnMissileAngle(actor, info.mobjtype_t.MT_MNTRFX1, angle - (DoomDef.ANG45 / 8), momz);
					p_mobj.P_SpawnMissileAngle(actor, info.mobjtype_t.MT_MNTRFX1, angle + (DoomDef.ANG45 / 8), momz);
					p_mobj.P_SpawnMissileAngle(actor, info.mobjtype_t.MT_MNTRFX1, angle - (DoomDef.ANG45 / 16), momz);
					p_mobj.P_SpawnMissileAngle(actor, info.mobjtype_t.MT_MNTRFX1, angle + (DoomDef.ANG45 / 16), momz);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_MinotaurAtk3
		//
		// Floor fire attack.
		//
		//----------------------------------------------------------------------------
		public class A_MinotaurAtk3 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;
				DoomDef.player_t player;

				if (actor.target == null)
				{
					return;
				}
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, DoomDef.HITDICE(5));
					if ((player = actor.target.player) != null)
					{ // Squish the player
						player.deltaviewheight = -16 * DoomDef.FRACUNIT;
					}
				}
				else
				{
					mo = p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_MNTRFX2);
					if (mo != null)
					{
						i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_minat1);
					}
				}
				if (m_misc.P_Random() < 192 && actor.special2 == 0)
				{
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_MNTR_ATK3_4);
					actor.special2 = 1;
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_MntrFloorFire
		//
		//----------------------------------------------------------------------------
		public class A_MntrFloorFire : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;

				actor.z = actor.floorz;
				mo = p_mobj.P_SpawnMobj(actor.x + ((m_misc.P_Random() - m_misc.P_Random()) << 10),
					actor.y + ((m_misc.P_Random() - m_misc.P_Random()) << 10), p_local.ONFLOORZ, info.mobjtype_t.MT_MNTRFX3);
				mo.target = actor.target;
				mo.momx = 1; // Force block checking
				p_mobj.P_CheckMissileSpawn(mo);
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_BeastAttack
		//
		//----------------------------------------------------------------------------
		public class A_BeastAttack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.target == null)
				{
					return;
				}
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, DoomDef.HITDICE(3));
					return;
				}
				p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_BEASTBALL);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_HeadAttack
		//
		//----------------------------------------------------------------------------
		static int[] atkResolve1 = new int[] { 50, 150 };
		static int[] atkResolve2 = new int[] { 150, 200 };
		public class A_HeadAttack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				int i;
				DoomDef.mobj_t fire;
				DoomDef.mobj_t baseFire;
				DoomDef.mobj_t mo;
				DoomDef.mobj_t target;
				int randAttack;
				int dist;

				// Ice ball		(close 20% : far 60%)
				// Fire column	(close 40% : far 20%)
				// Whirlwind	(close 40% : far 20%)
				// Distance threshold = 8 cells

				target = actor.target;
				if (target == null)
				{
					return;
				}
				(new A_FaceTarget()).action(actor);
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(target, actor, actor, DoomDef.HITDICE(6));
					return;
				}
				dist = (p_maputl.P_AproxDistance(actor.x - target.x, actor.y - target.y)
					> 8 * 64 * DoomDef.FRACUNIT) ? 1 : 0;
				randAttack = m_misc.P_Random();
				if (randAttack < atkResolve1[dist])
				{ // Ice ball
					p_mobj.P_SpawnMissile(actor, target, info.mobjtype_t.MT_HEADFX1);
					i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_hedat2);
				}
				else if (randAttack < atkResolve2[dist])
				{ // Fire column
					baseFire = p_mobj.P_SpawnMissile(actor, target, info.mobjtype_t.MT_HEADFX3);
					if (baseFire != null)
					{
						p_mobj.P_SetMobjState(baseFire, info.statenum_t.S_HEADFX3_4); // Don't grow
						for (i = 0; i < 5; i++)
						{
							fire = p_mobj.P_SpawnMobj(baseFire.x, baseFire.y,
								baseFire.z, info.mobjtype_t.MT_HEADFX3);
							if (i == 0)
							{
								i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_hedat1);
							}
							fire.target = baseFire.target;
							fire.angle = baseFire.angle;
							fire.momx = baseFire.momx;
							fire.momy = baseFire.momy;
							fire.momz = baseFire.momz;
							fire.damage = 0;
							fire.health = (i + 1) * 2;
							p_mobj.P_CheckMissileSpawn(fire);
						}
					}
				}
				else
				{ // Whirlwind
					mo = p_mobj.P_SpawnMissile(actor, target, info.mobjtype_t.MT_WHIRLWIND);
					if (mo != null)
					{
						mo.z -= 32 * DoomDef.FRACUNIT;
						mo.special1AsTarget = target;
						mo.special2 = 50; // Timer for active sound
						mo.health = 20 * DoomDef.TICSPERSEC; // Duration
						i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_hedat3);
					}
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_WhirlwindSeek
		//
		//----------------------------------------------------------------------------
		public class A_WhirlwindSeek : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.health -= 3;
				if (actor.health < 0)
				{
					actor.momx = actor.momy = actor.momz = 0;
					p_mobj.P_SetMobjState(actor, (info.statenum_t)info.mobjinfo[(int)actor.type].deathstate);
					actor.flags &= ~DoomDef.MF_MISSILE;
					return;
				}
				if ((actor.special2 -= 3) < 0)
				{
					actor.special2 = 58 + (m_misc.P_Random() & 31);
					i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_hedat3);
				}
				if (actor.special1 != 0
					&& (actor.special1AsTarget.flags & DoomDef.MF_SHADOW) != 0)
				{
					return;
				}
				p_mobj.P_SeekerMissile(actor, DoomDef.ANGLE_1 * 10, DoomDef.ANGLE_1 * 30);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_HeadIceImpact
		//
		//----------------------------------------------------------------------------
		public class A_HeadIceImpact : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t ice)
			{
				int i;
				uint angle;
				DoomDef.mobj_t shard;

				for (i = 0; i < 8; i++)
				{
					shard = p_mobj.P_SpawnMobj(ice.x, ice.y, ice.z, info.mobjtype_t.MT_HEADFX2);
					angle = (uint)(i * DoomDef.ANG45);
					shard.target = ice.target;
					shard.angle = angle;
					angle >>= (int)DoomDef.ANGLETOFINESHIFT;
					shard.momx = DoomDef.FixedMul(shard.infol.speed, r_main.finecosine(angle));
					shard.momy = DoomDef.FixedMul(shard.infol.speed, tables.finesine[angle]);
					shard.momz = (int)(-.6 * (double)DoomDef.FRACUNIT);
					p_mobj.P_CheckMissileSpawn(shard);
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_HeadFireGrow
		//
		//----------------------------------------------------------------------------
		public class A_HeadFireGrow : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t fire)
			{
				fire.health--;
				fire.z += 9 * DoomDef.FRACUNIT;
				if (fire.health == 0)
				{
					fire.damage = fire.infol.damage;
					p_mobj.P_SetMobjState(fire, info.statenum_t.S_HEADFX3_4);
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_SnakeAttack
		//
		//----------------------------------------------------------------------------
		public class A_SnakeAttack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.target == null)
				{
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_SNAKE_WALK1);
					return;
				}
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				(new A_FaceTarget()).action(actor);
				p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_SNAKEPRO_A);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_SnakeAttack2
		//
		//----------------------------------------------------------------------------
		public class A_SnakeAttack2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.target == null)
				{
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_SNAKE_WALK1);
					return;
				}
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				(new A_FaceTarget()).action(actor);
				p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_SNAKEPRO_B);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_ClinkAttack
		//
		//----------------------------------------------------------------------------
		public class A_ClinkAttack : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				int damage;

				if (actor.target == null)
				{
					return;
				}
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				if (P_CheckMeleeRange(actor))
				{
					damage = ((m_misc.P_Random() % 7) + 3);
					p_inter.P_DamageMobj(actor.target, actor, actor, damage);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_GhostOff
		//
		//----------------------------------------------------------------------------
		public class A_GhostOff : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.flags &= ~DoomDef.MF_SHADOW;
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_WizAtk1
		//
		//----------------------------------------------------------------------------
		public class A_WizAtk1 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				s_A_FaceTarget.action(actor);
				actor.flags &= ~DoomDef.MF_SHADOW;
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_WizAtk2
		//
		//----------------------------------------------------------------------------
		public class A_WizAtk2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				s_A_FaceTarget.action(actor);
				actor.flags |= DoomDef.MF_SHADOW;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_WizAtk3
		//
		//----------------------------------------------------------------------------
		public class A_WizAtk3 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;
				uint angle;
				int momz;

				actor.flags &= ~DoomDef.MF_SHADOW;
				if (actor.target == null)
				{
					return;
				}
				i_ibm.S_StartSound(actor, actor.infol.attacksound);
				if (P_CheckMeleeRange(actor))
				{
					p_inter.P_DamageMobj(actor.target, actor, actor, DoomDef.HITDICE(4));
					return;
				}
				mo = p_mobj.P_SpawnMissile(actor, actor.target, info.mobjtype_t.MT_WIZFX1);
				if (mo != null)
				{
					momz = mo.momz;
					angle = mo.angle;
					p_mobj.P_SpawnMissileAngle(actor, info.mobjtype_t.MT_WIZFX1, angle - (DoomDef.ANG45 / 8), momz);
					p_mobj.P_SpawnMissileAngle(actor, info.mobjtype_t.MT_WIZFX1, angle + (DoomDef.ANG45 / 8), momz);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_Scream
		//
		//----------------------------------------------------------------------------
		public class A_Scream : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				switch (actor.type)
				{
					case info.mobjtype_t.MT_CHICPLAYER:
					case info.mobjtype_t.MT_SORCERER1:
					case info.mobjtype_t.MT_MINOTAUR:
						// Make boss death sounds full volume
						i_ibm.S_StartSound(null, actor.infol.deathsound);
						break;
					case info.mobjtype_t.MT_PLAYER:
						// Handle the different player death screams
						if (actor.special1 < 10)
						{ // Wimpy death sound
							i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_plrwdth);
						}
						else if (actor.health > -50)
						{ // Normal death sound
							i_ibm.S_StartSound(actor, actor.infol.deathsound);
						}
						else if (actor.health > -100)
						{ // Crazy death sound
							i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_plrcdth);
						}
						else
						{ // Extreme death sound
							i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_gibdth);
						}
						break;
					default:
						i_ibm.S_StartSound(actor, actor.infol.deathsound);
						break;
				}
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_DropItem
		//
		//---------------------------------------------------------------------------

		public static void P_DropItem(DoomDef.mobj_t source, info.mobjtype_t type, int special, int chance)
		{
			DoomDef.mobj_t mo;

			if (m_misc.P_Random() > chance)
			{
				return;
			}
			mo = p_mobj.P_SpawnMobj(source.x, source.y,
				source.z + (source.height >> 1), type);
			mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 8;
			mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 8;
			mo.momz = DoomDef.FRACUNIT * 5 + (m_misc.P_Random() << 10);
			mo.flags |= DoomDef.MF_DROPPED;
			mo.health = special;
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_NoBlocking
		//
		//----------------------------------------------------------------------------

		public class A_NoBlocking : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.flags &= ~DoomDef.MF_SOLID;
				// Check for monsters dropping things
				switch (actor.type)
				{
					case info.mobjtype_t.MT_MUMMY:
					case info.mobjtype_t.MT_MUMMYLEADER:
					case info.mobjtype_t.MT_MUMMYGHOST:
					case info.mobjtype_t.MT_MUMMYLEADERGHOST:
						P_DropItem(actor, info.mobjtype_t.MT_AMGWNDWIMPY, 3, 84);
						break;
					case info.mobjtype_t.MT_KNIGHT:
					case info.mobjtype_t.MT_KNIGHTGHOST:
						P_DropItem(actor, info.mobjtype_t.MT_AMCBOWWIMPY, 5, 84);
						break;
					case info.mobjtype_t.MT_WIZARD:
						P_DropItem(actor, info.mobjtype_t.MT_AMBLSRWIMPY, 10, 84);
						P_DropItem(actor, info.mobjtype_t.MT_ARTITOMEOFPOWER, 0, 4);
						break;
					case info.mobjtype_t.MT_HEAD:
						P_DropItem(actor, info.mobjtype_t.MT_AMBLSRWIMPY, 10, 84);
						P_DropItem(actor, info.mobjtype_t.MT_ARTIEGG, 0, 51);
						break;
					case info.mobjtype_t.MT_BEAST:
						P_DropItem(actor, info.mobjtype_t.MT_AMCBOWWIMPY, 10, 84);
						break;
					case info.mobjtype_t.MT_CLINK:
						P_DropItem(actor, info.mobjtype_t.MT_AMSKRDWIMPY, 20, 84);
						break;
					case info.mobjtype_t.MT_SNAKE:
						P_DropItem(actor, info.mobjtype_t.MT_AMPHRDWIMPY, 5, 84);
						break;
					case info.mobjtype_t.MT_MINOTAUR:
						P_DropItem(actor, info.mobjtype_t.MT_ARTISUPERHEAL, 0, 51);
						P_DropItem(actor, info.mobjtype_t.MT_AMPHRDWIMPY, 10, 84);
						break;
					default:
						break;
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_Explode
		//
		// Handles a bunch of exploding things.
		//
		//----------------------------------------------------------------------------
		public class A_Explode : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				int damage;

				damage = 128;
				switch (actor.type)
				{
					case info.mobjtype_t.MT_FIREBOMB: // Time Bombs
						actor.z += 32 * DoomDef.FRACUNIT;
						actor.flags &= ~DoomDef.MF_SHADOW;
						break;
					case info.mobjtype_t.MT_MNTRFX2: // Minotaur floor fire
						damage = 24;
						break;
					case info.mobjtype_t.MT_SOR2FX1: // D'Sparil missile
						damage = 80 + (m_misc.P_Random() & 31);
						break;
					default:
						break;
				}
				p_map.P_RadiusAttack(actor, actor.target, damage);
				p_mobj.P_HitFloor(actor);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_PodPain
		//
		//----------------------------------------------------------------------------
		public class A_PodPain : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				int i;
				int count;
				int chance;
				DoomDef.mobj_t goo;

				chance = m_misc.P_Random();
				if (chance < 128)
				{
					return;
				}
				count = chance > 240 ? 2 : 1;
				for (i = 0; i < count; i++)
				{
					goo = p_mobj.P_SpawnMobj(actor.x, actor.y,
						actor.z + 48 * DoomDef.FRACUNIT, info.mobjtype_t.MT_PODGOO);
					goo.target = actor;
					goo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 9;
					goo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 9;
					goo.momz = DoomDef.FRACUNIT / 2 + (m_misc.P_Random() << 9);
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_RemovePod
		//
		//----------------------------------------------------------------------------
		public class A_RemovePod : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;

				if (actor.special2 != 0)
				{
					mo = actor.special2AsTarget;
					if (mo.special1 > 0)
					{
						mo.special1--;
					}
				}
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_MakePod
		//
		//----------------------------------------------------------------------------

		public const int MAX_GEN_PODS = 16;

		public class A_MakePod : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;
				int x;
				int y;
				int z;

				if (actor.special1 == MAX_GEN_PODS)
				{ // Too many generated pods
					return;
				}
				x = actor.x;
				y = actor.y;
				z = actor.z;
				mo = p_mobj.P_SpawnMobj(x, y, p_local.ONFLOORZ, info.mobjtype_t.MT_POD);
				if (p_map.P_CheckPosition(mo, x, y) == false)
				{ // Didn't fit
					p_mobj.P_RemoveMobj(mo);
					return;
				}
				p_mobj.P_SetMobjState(mo, info.statenum_t.S_POD_GROW1);
				p_mobj.P_ThrustMobj(mo, (uint)(m_misc.P_Random() << 24), (int)(4.5 * DoomDef.FRACUNIT));
				i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_newpod);
				actor.special1++; // Increment generated pod count
				mo.special2AsTarget = actor; // Link the generator to the pod
				return;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_Massacre
		//
		// Kills all monsters.
		//
		//----------------------------------------------------------------------------

		public static void P_Massacre()
		{
			DoomDef.mobj_t mo;
			DoomDef.thinker_t think;

			for (think = p_tick.thinkercap.next; think != p_tick.thinkercap;
				think = think.next)
			{
				if (!(think.function is p_mobj.P_MobjThinker))
				{ // Not a mobj thinker
					continue;
				}
				mo = think.function.obj as DoomDef.mobj_t;
				if ((mo.flags & DoomDef.MF_COUNTKILL) != 0 && (mo.health > 0))
				{
					p_inter.P_DamageMobj(mo, null, null, 10000);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_BossDeath
		//
		// Trigger special effects if all bosses are dead.
		//
		//----------------------------------------------------------------------------
		static info.mobjtype_t[] bossType = new info.mobjtype_t[5]
		{
			info.mobjtype_t.MT_HEAD,
			info.mobjtype_t.MT_MINOTAUR,
			info.mobjtype_t.MT_SORCERER2,
			info.mobjtype_t.MT_HEAD,
			info.mobjtype_t.MT_MINOTAUR
		};
		public class A_BossDeath : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;
				DoomDef.thinker_t think;
				r_local.line_t dummyLine = new r_local.line_t();

				if (g_game.gamemap != 8)
				{ // Not a boss level
					return;
				}
				if (g_game.gameepisode - 1 == 5) return;
				if (actor.type != bossType[g_game.gameepisode - 1])
				{ // Not considered a boss in this episode
					return;
				}
				// Make sure all bosses are dead
				for (think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
				{
					if (!(think.function is p_mobj.P_MobjThinker))
					{ // Not a mobj thinker
						continue;
					}
					mo = think.function.obj as DoomDef.mobj_t;
					if ((mo != actor) && (mo.type == actor.type) && (mo.health > 0))
					{ // Found a living boss
						return;
					}
				}
				if (g_game.gameepisode > 1)
				{ // Kill any remaining monsters
					P_Massacre();
				}
				dummyLine.tag = 666;
				p_floor.EV_DoFloor(dummyLine, p_spec.floor_e.lowerFloor);
			}
		}
		//----------------------------------------------------------------------------
		//
		// PROC A_ESound
		//
		//----------------------------------------------------------------------------
		public class A_ESound : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t mo)
			{
				int sound = 0;

				switch (mo.type)
				{
					case info.mobjtype_t.MT_SOUNDWATERFALL:
						sound = (int)sounds.sfxenum_t.sfx_waterfl;
						break;
					case info.mobjtype_t.MT_SOUNDWIND:
						sound = (int)sounds.sfxenum_t.sfx_wind;
						break;
					default:
						break;
				}
				i_ibm.S_StartSound(mo, sound);
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_SpawnTeleGlitter
		//
		//----------------------------------------------------------------------------
		public class A_SpawnTeleGlitter : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;

				mo = p_mobj.P_SpawnMobj(actor.x + ((m_misc.P_Random() & 31) - 16) * DoomDef.FRACUNIT,
					actor.y + ((m_misc.P_Random() & 31) - 16) * DoomDef.FRACUNIT,
					actor.subsector.sector.floorheight, info.mobjtype_t.MT_TELEGLITTER);
				mo.momz = DoomDef.FRACUNIT / 4;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_SpawnTeleGlitter2
		//
		//----------------------------------------------------------------------------
		public class A_SpawnTeleGlitter2 : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;

				mo = p_mobj.P_SpawnMobj(actor.x + ((m_misc.P_Random() & 31) - 16) * DoomDef.FRACUNIT,
					actor.y + ((m_misc.P_Random() & 31) - 16) * DoomDef.FRACUNIT,
					actor.subsector.sector.floorheight, info.mobjtype_t.MT_TELEGLITTER2);
				mo.momz = DoomDef.FRACUNIT / 4;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_AccTeleGlitter
		//
		//----------------------------------------------------------------------------
		public class A_AccTeleGlitter : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (++actor.health > 35)
				{
					actor.momz += actor.momz / 2;
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_InitKeyGizmo
		//
		//----------------------------------------------------------------------------
		public class A_InitKeyGizmo : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t gizmo)
			{
				DoomDef.mobj_t mo;
				info.statenum_t state = info.statenum_t.S_NULL;

				switch (gizmo.type)
				{
					case info.mobjtype_t.MT_KEYGIZMOBLUE:
						state = info.statenum_t.S_KGZ_BLUEFLOAT1;
						break;
					case info.mobjtype_t.MT_KEYGIZMOGREEN:
						state = info.statenum_t.S_KGZ_GREENFLOAT1;
						break;
					case info.mobjtype_t.MT_KEYGIZMOYELLOW:
						state = info.statenum_t.S_KGZ_YELLOWFLOAT1;
						break;
					default:
						break;
				}
				mo = p_mobj.P_SpawnMobj(gizmo.x, gizmo.y, gizmo.z + 60 * DoomDef.FRACUNIT,
					info.mobjtype_t.MT_KEYGIZMOFLOAT);
				p_mobj.P_SetMobjState(mo, state);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_VolcanoSet
		//
		//----------------------------------------------------------------------------
		public class A_VolcanoSet : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t volcano)
			{
				volcano.tics = 105 + (m_misc.P_Random() & 127);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_VolcanoBlast
		//
		//----------------------------------------------------------------------------
		public class A_VolcanoBlast : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t volcano)
			{
				int i;
				int count;
				DoomDef.mobj_t blast;
				uint angle;

				count = 1 + (m_misc.P_Random() % 3);
				for (i = 0; i < count; i++)
				{
					blast = p_mobj.P_SpawnMobj(volcano.x, volcano.y,
						volcano.z + 44 * DoomDef.FRACUNIT, info.mobjtype_t.MT_VOLCANOBLAST); // MT_VOLCANOBLAST
					blast.target = volcano;
					angle = (uint)(m_misc.P_Random() << 24);
					blast.angle = angle;
					angle >>= (int)DoomDef.ANGLETOFINESHIFT;
					blast.momx = DoomDef.FixedMul(1 * DoomDef.FRACUNIT, r_main.finecosine(angle));
					blast.momy = DoomDef.FixedMul(1 * DoomDef.FRACUNIT, tables.finesine[angle]);
					blast.momz = (int)(2.5 * (double)DoomDef.FRACUNIT) + (m_misc.P_Random() << 10);
					i_ibm.S_StartSound(blast, (int)sounds.sfxenum_t.sfx_volsht);
					p_mobj.P_CheckMissileSpawn(blast);
				}
			}
		}



		//----------------------------------------------------------------------------
		//
		// PROC A_VolcBallImpact
		//
		//----------------------------------------------------------------------------
		public class A_VolcBallImpact : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t ball)
			{
				int i;
				DoomDef.mobj_t tiny;
				uint angle;

				if (ball.z <= ball.floorz)
				{
					ball.flags |= DoomDef.MF_NOGRAVITY;
					ball.flags2 &= ~DoomDef.MF2_LOGRAV;
					ball.z += 28 * DoomDef.FRACUNIT;
					//ball.momz = 3*FRACUNIT;
				}
				p_map.P_RadiusAttack(ball, ball.target, 25);
				for (i = 0; i < 4; i++)
				{
					tiny = p_mobj.P_SpawnMobj(ball.x, ball.y, ball.z, info.mobjtype_t.MT_VOLCANOTBLAST);
					tiny.target = ball;
					angle = (uint)(i * DoomDef.ANG90);
					tiny.angle = angle;
					angle >>= (int)DoomDef.ANGLETOFINESHIFT;
					tiny.momx = DoomDef.FixedMul((int)((double)DoomDef.FRACUNIT * .7), r_main.finecosine(angle));
					tiny.momy = DoomDef.FixedMul((int)((double)DoomDef.FRACUNIT * .7), tables.finesine[angle]);
					tiny.momz = DoomDef.FRACUNIT + (m_misc.P_Random() << 9);
					p_mobj.P_CheckMissileSpawn(tiny);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_SkullPop
		//
		//----------------------------------------------------------------------------
		public class A_SkullPop : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				DoomDef.mobj_t mo;
				DoomDef.player_t player;

				actor.flags &= ~DoomDef.MF_SOLID;
				mo = p_mobj.P_SpawnMobj(actor.x, actor.y, actor.z + 48 * DoomDef.FRACUNIT,
					info.mobjtype_t.MT_BLOODYSKULL);
				//mo.target = actor;
				mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 9;
				mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 9;
				mo.momz = DoomDef.FRACUNIT * 2 + (m_misc.P_Random() << 6);
				// Attach player mobj to bloody skull
				player = actor.player;
				actor.player = null;
				mo.player = player;
				mo.health = actor.health;
				mo.angle = actor.angle;
				player.mo = mo;
				player.lookdir = 0;
				player.damagecount = 32;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_CheckSkullFloor
		//
		//----------------------------------------------------------------------------
		public class A_CheckSkullFloor : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.z <= actor.floorz)
				{
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_BLOODYSKULLX1);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_CheckSkullDone
		//
		//----------------------------------------------------------------------------

		public class A_CheckSkullDone : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.special2 == 666)
				{
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_BLOODYSKULLX2);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_CheckBurnGone
		//
		//----------------------------------------------------------------------------
		public class A_CheckBurnGone : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (actor.special2 == 666)
				{
					p_mobj.P_SetMobjState(actor, info.statenum_t.S_PLAY_FDTH20);
				}
			}
		}
		//----------------------------------------------------------------------------
		//
		// PROC A_FreeTargMobj
		//
		//----------------------------------------------------------------------------

		public class A_FreeTargMobj : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t mo)
			{
				mo.momx = mo.momy = mo.momz = 0;
				mo.z = mo.ceilingz + 4 * DoomDef.FRACUNIT;
				mo.flags &= ~(DoomDef.MF_SHOOTABLE | DoomDef.MF_FLOAT | DoomDef.MF_SKULLFLY | DoomDef.MF_SOLID);
				mo.flags |= DoomDef.MF_CORPSE | DoomDef.MF_DROPOFF | DoomDef.MF_NOGRAVITY;
				mo.flags2 &= ~(DoomDef.MF2_PASSMOBJ | DoomDef.MF2_LOGRAV);
				mo.player = null;
			}
		}
		//----------------------------------------------------------------------------
		//
		// PROC A_AddPlayerCorpse
		//
		//----------------------------------------------------------------------------

		public const int BODYQUESIZE = 32;
		public static DoomDef.mobj_t[] bodyque = new DoomDef.mobj_t[BODYQUESIZE];
		public static int bodyqueslot;
		public class A_AddPlayerCorpse : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				if (bodyqueslot >= BODYQUESIZE)
				{ // Too many player corpses - remove an old one
					p_mobj.P_RemoveMobj(bodyque[bodyqueslot % BODYQUESIZE]);
				}
				bodyque[bodyqueslot % BODYQUESIZE] = actor;
				bodyqueslot++;
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC A_FlameSnd
		//
		//----------------------------------------------------------------------------
		public class A_FlameSnd : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				i_ibm.S_StartSound(actor, (int)sounds.sfxenum_t.sfx_hedat1); // Burn sound
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_HideThing
		//
		//----------------------------------------------------------------------------
		public class A_HideThing : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.flags2 |= DoomDef.MF2_DONTDRAW;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC A_UnHideThing
		//
		//----------------------------------------------------------------------------

		public class A_UnHideThing : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t actor)
			{
				actor.flags2 &= ~DoomDef.MF2_DONTDRAW;
			}
		}
	}
}
