using System.Linq;
using Microsoft.Xna.Framework;

// P_mobj.c


namespace HereticXNA
{
	public static class p_mobj
	{

		public static info.mobjtype_t PuffType = new info.mobjtype_t();
		public static DoomDef.mobj_t MissileMobj;

		public static int[] FloatBobOffsets = new int[64]
{
	0, 51389, 102283, 152192,
	200636, 247147, 291278, 332604,
	370727, 405280, 435929, 462380,
	484378, 501712, 514213, 521763,
	524287, 521763, 514213, 501712,
	484378, 462380, 435929, 405280,
	370727, 332604, 291278, 247147,
	200636, 152192, 102283, 51389,
	-1, -51390, -102284, -152193,
	-200637, -247148, -291279, -332605,
	-370728, -405281, -435930, -462381,
	-484380, -501713, -514215, -521764,
	-524288, -521764, -514214, -501713,
	-484379, -462381, -435930, -405280,
	-370728, -332605, -291279, -247148,
	-200637, -152193, -102284, -51389
};
		//----------------------------------------------------------------------------
		//
		// FUNC P_SetMobjState
		//
		// Returns true if the mobj is still present.
		//
		//----------------------------------------------------------------------------

		public static bool P_SetMobjState(DoomDef.mobj_t mobj, info.statenum_t state)
		{
			info.state_t st;

			if (state == info.statenum_t.S_NULL)
			{ // Remove mobj
				mobj.state = null;
				P_RemoveMobj(mobj);
				return (false);
			}
			st = info.states[(int)state];
			mobj.state = st;
			mobj.tics = (int)st.tics;
			mobj.sprite = st.sprite;
			mobj.frame = (int)st.frame;
			if (st.action != null)
			{ // Call action function
				st.action.action(mobj);
			}
			return (true);
		}

		//----------------------------------------------------------------------------
		//
		// FUNC P_SetMobjStateNF
		//
		// Same as P_SetMobjState, but does not call the state function.
		//
		//----------------------------------------------------------------------------

		public static bool P_SetMobjStateNF(DoomDef.mobj_t mobj, info.statenum_t state)
		{
			info.state_t st;

			if (state == info.statenum_t.S_NULL)
			{ // Remove mobj
				mobj.state = null;
				P_RemoveMobj(mobj);
				return (false);
			}
			st = info.states[(int)state];
			mobj.state = st;
			mobj.tics = (int)st.tics;
			mobj.sprite = st.sprite;
			mobj.frame = (int)st.frame;
			return (true);
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_ExplodeMissile
		//
		//----------------------------------------------------------------------------

		public static void P_ExplodeMissile(DoomDef.mobj_t mo)
		{
			if (mo.type == info.mobjtype_t.MT_WHIRLWIND)
			{
				if (++mo.special2 < 60)
				{
					return;
				}
			}
			mo.momx = mo.momy = mo.momz = 0;
			P_SetMobjState(mo, (info.statenum_t)info.mobjinfo[(int)mo.type].deathstate);
			//mo.tics -= P_Random()&3;
			mo.flags &= ~DoomDef.MF_MISSILE;
			if (mo.infol.deathsound != 0)
			{
				i_ibm.S_StartSound(mo, mo.infol.deathsound);
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_FloorBounceMissile
		//
		//----------------------------------------------------------------------------

		public static void P_FloorBounceMissile(DoomDef.mobj_t mo)
		{
			mo.momz = -mo.momz;
			P_SetMobjState(mo, (info.statenum_t)info.mobjinfo[(int)mo.type].deathstate);
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_ThrustMobj
		//
		//----------------------------------------------------------------------------

		public static void P_ThrustMobj(DoomDef.mobj_t mo, uint angle, int move)
		{
			angle = (uint)(angle >> (int)DoomDef.ANGLETOFINESHIFT);
			mo.momx += DoomDef.FixedMul(move, r_main.finecosine(angle));
			mo.momy += DoomDef.FixedMul(move, tables.finesine[angle]);
		}

		//----------------------------------------------------------------------------
		//
		// FUNC P_FaceMobj
		//
		// Returns 1 if 'source' needs to turn clockwise, or 0 if 'source' needs
		// to turn counter clockwise.  'delta' is set to the amount 'source'
		// needs to turn.
		//
		//----------------------------------------------------------------------------

		public static int P_FaceMobj(DoomDef.mobj_t source, DoomDef.mobj_t target, ref uint delta)
		{
			uint diff;
			uint angle1;
			uint angle2;

			angle1 = source.angle;
			angle2 = r_main.R_PointToAngle2(source.x, source.y, target.x, target.y);
			if (angle2 > angle1)
			{
				diff = angle2 - angle1;
				if (diff > DoomDef.ANGLE_180)
				{
					delta = DoomDef.ANGLE_MAX - diff;
					return (0);
				}
				else
				{
					delta = diff;
					return (1);
				}
			}
			else
			{
				diff = angle1 - angle2;
				if (diff > DoomDef.ANGLE_180)
				{
					delta = DoomDef.ANGLE_MAX - diff;
					return (1);
				}
				else
				{
					delta = diff;
					return (0);
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// FUNC P_SeekerMissile
		//
		// The missile special1 field must be mobj_t *target.  Returns true if
		// target was tracked, false if not.
		//
		//----------------------------------------------------------------------------

		public static bool P_SeekerMissile(DoomDef.mobj_t actor, uint thresh, uint turnMax)
		{
			int dir;
			int dist;
			uint delta = 0;
			uint angle;
			DoomDef.mobj_t target;

			target = actor.special1AsTarget;
			if (target == null)
			{
				return (false);
			}
			if ((target.flags & DoomDef.MF_SHOOTABLE) == 0)
			{ // Target died
				actor.special1AsTarget = null;
				return (false);
			}
			dir = p_mobj.P_FaceMobj(actor, target, ref delta);
			if (delta > thresh)
			{
				delta >>= 1;
				if (delta > turnMax)
				{
					delta = turnMax;
				}
			}
			if (dir != 0)
			{ // Turn clockwise
				actor.angle += delta;
			}
			else
			{ // Turn counter clockwise
				actor.angle -= delta;
			}
			angle = actor.angle >> (int)DoomDef.ANGLETOFINESHIFT;
			actor.momx = DoomDef.FixedMul(actor.infol.speed, r_main.finecosine(angle));
			actor.momy = DoomDef.FixedMul(actor.infol.speed, tables.finesine[angle]);
			if (actor.z + actor.height < target.z ||
				target.z + target.height < actor.z)
			{ // Need to seek vertically
				dist = p_maputl.P_AproxDistance(target.x - actor.x, target.y - actor.y);
				dist = dist / actor.infol.speed;
				if (dist < 1)
				{
					dist = 1;
				}
				actor.momz = (target.z - actor.z) / dist;
			}
			return (true);
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_XYMovement
		//
		//----------------------------------------------------------------------------

		public const int STOPSPEED = 0x1000;
		public const int FRICTION_NORMAL = 0xe800;
		public const int FRICTION_LOW = 0xf900;
		public const int FRICTION_FLY = 0xeb00;

		static int[] windTab = new int[3] { 2048 * 5, 2048 * 10, 2048 * 25 };
		public static void P_XYMovement(DoomDef.mobj_t mo)
		{
			int ptryx, ptryy;
			DoomDef.player_t player;
			int xmove, ymove;
			int special;

			if (mo.momx == 0 && mo.momy == 0)
			{
				if ((mo.flags & DoomDef.MF_SKULLFLY) != 0)
				{ // A flying mobj slammed into something
					mo.flags &= ~DoomDef.MF_SKULLFLY;
					mo.momx = mo.momy = mo.momz = 0;
					p_mobj.P_SetMobjState(mo, (info.statenum_t)mo.infol.seestate);
				}
				return;
			}
			special = mo.subsector.sector.special;
			if ((mo.flags2 & DoomDef.MF2_WINDTHRUST) != 0)
			{
				switch (special)
				{
					case 40:
					case 41:
					case 42: // Wind_East
						p_mobj.P_ThrustMobj(mo, 0, windTab[special - 40]);
						break;
					case 43:
					case 44:
					case 45: // Wind_North
						p_mobj.P_ThrustMobj(mo, DoomDef.ANG90, windTab[special - 43]);
						break;
					case 46:
					case 47:
					case 48: // Wind_South
						p_mobj.P_ThrustMobj(mo, DoomDef.ANG270, windTab[special - 46]);
						break;
					case 49:
					case 50:
					case 51: // Wind_West
						p_mobj.P_ThrustMobj(mo, DoomDef.ANG180, windTab[special - 49]);
						break;
				}
			}
			player = mo.player;
			if (mo.momx > p_local.MAXMOVE)
			{
				mo.momx = p_local.MAXMOVE;
			}
			else if (mo.momx < -p_local.MAXMOVE)
			{
				mo.momx = -p_local.MAXMOVE;
			}
			if (mo.momy > p_local.MAXMOVE)
			{
				mo.momy = p_local.MAXMOVE;
			}
			else if (mo.momy < -p_local.MAXMOVE)
			{
				mo.momy = -p_local.MAXMOVE;
			}
			xmove = mo.momx;
			ymove = mo.momy;
			do
			{
				if (xmove > p_local.MAXMOVE / 2 || ymove > p_local.MAXMOVE / 2)
				{
					ptryx = mo.x + xmove / 2;
					ptryy = mo.y + ymove / 2;
					xmove >>= 1;
					ymove >>= 1;
				}
				else
				{
					ptryx = mo.x + xmove;
					ptryy = mo.y + ymove;
					xmove = ymove = 0;
				}
				if (!p_map.P_TryMove(mo, ptryx, ptryy))
				{ // Blocked move
					if ((mo.flags2 & DoomDef.MF2_SLIDE) != 0)
					{ // Try to slide along it
						p_map.P_SlideMove(mo);
					}
					else if ((mo.flags & DoomDef.MF_MISSILE) != 0)
					{ // Explode a missile
						if (p_map.ceilingline != null && p_map.ceilingline.backsector != null
							&& p_map.ceilingline.backsector.ceilingpic == r_plane.skyflatnum)
						{ // Hack to prevent missiles exploding against the sky
							if (mo.type == info.mobjtype_t.MT_BLOODYSKULL)
							{
								mo.momx = mo.momy = 0;
								mo.momz = -DoomDef.FRACUNIT;
							}
							else
							{
								P_RemoveMobj(mo);
							}
							return;
						}
						p_mobj.P_ExplodeMissile(mo);
					}
					else
					{
						mo.momx = mo.momy = 0;
					}
				}
			} while (xmove != 0 || ymove != 0);

			// Friction

			if (player != null && (player.cheats & DoomDef.CF_NOMOMENTUM) != 0)
			{ // Debug option for no sliding at all
				mo.momx = mo.momy = 0;
				return;
			}
			if ((mo.flags & (DoomDef.MF_MISSILE | DoomDef.MF_SKULLFLY)) != 0)
			{ // No friction for missiles
				return;
			}
			if (mo.z > mo.floorz && (mo.flags2 & DoomDef.MF2_FLY) == 0 && (mo.flags2 & DoomDef.MF2_ONMOBJ) == 0)
			{ // No friction when falling
				return;
			}
			if ((mo.flags & DoomDef.MF_CORPSE) != 0)
			{ // Don't stop sliding if halfway off a step with some momentum
				if (mo.momx > DoomDef.FRACUNIT / 4 || mo.momx < -DoomDef.FRACUNIT / 4
					|| mo.momy > DoomDef.FRACUNIT / 4 || mo.momy < -DoomDef.FRACUNIT / 4)
				{
					if (mo.floorz != mo.subsector.sector.floorheight)
					{
						return;
					}
				}
			}
			if (mo.momx > -STOPSPEED && mo.momx < STOPSPEED
				&& mo.momy > -STOPSPEED && mo.momy < STOPSPEED
				&& (player == null || (player.cmd.forwardmove == 0
				&& player.cmd.sidemove == 0)))
			{ // If in a walking frame, stop moving
				if (player != null)
				{
					if (player.chickenTics != 0)
					{
						int indefOf = info.states.ToList().IndexOf(player.mo.state);
						if (indefOf - (int)info.statenum_t.S_CHICPLAY_RUN1 < 4)
						{
							P_SetMobjState(player.mo, info.statenum_t.S_CHICPLAY);
						}
					}
					else
					{
						int indefOf = info.states.ToList().IndexOf(player.mo.state);
						if (indefOf - (int)info.statenum_t.S_PLAY_RUN1 < 4)
						{
							P_SetMobjState(player.mo, info.statenum_t.S_PLAY);
						}
					}
				}
				mo.momx = 0;
				mo.momy = 0;
			}
			else
			{
				if ((mo.flags2 & DoomDef.MF2_FLY) != 0 && !(mo.z <= mo.floorz)
					&& (mo.flags2 & DoomDef.MF2_ONMOBJ) == 0)
				{
					mo.momx = DoomDef.FixedMul(mo.momx, FRICTION_FLY);
					mo.momy = DoomDef.FixedMul(mo.momy, FRICTION_FLY);
				}
				else if (special == 15) // Friction_Low
				{
					mo.momx = DoomDef.FixedMul(mo.momx, FRICTION_LOW);
					mo.momy = DoomDef.FixedMul(mo.momy, FRICTION_LOW);
				}
				else
				{
					mo.momx = DoomDef.FixedMul(mo.momx, FRICTION_NORMAL);
					mo.momy = DoomDef.FixedMul(mo.momy, FRICTION_NORMAL);
				}
			}
		}

		/*
		===============
		=
		= P_ZMovement
		=
		===============
		*/

		public static void P_ZMovement(DoomDef.mobj_t mo)
		{
			int dist;
			int delta;
			//
			// check for smooth step up
			//
			if (mo.player != null && mo.z < mo.floorz)
			{
				mo.player.viewheight -= mo.floorz - mo.z;
				mo.player.deltaviewheight = (p_local.VIEWHEIGHT - mo.player.viewheight) >> 3;
			}
			//
			// adjust height
			//
			mo.z += mo.momz;
			if ((mo.flags & DoomDef.MF_FLOAT) != 0 && mo.target != null)
			{       // float down towards target if too close
				if ((mo.flags & DoomDef.MF_SKULLFLY) == 0 && (mo.flags & DoomDef.MF_INFLOAT) == 0)
				{
					dist = p_maputl.P_AproxDistance(mo.x - mo.target.x, mo.y - mo.target.y);
					delta = (mo.target.z + (mo.height >> 1)) - mo.z;
					if (delta < 0 && dist < -(delta * 3))
						mo.z -= p_local.FLOATSPEED;
					else if (delta > 0 && dist < (delta * 3))
						mo.z += -p_local.FLOATSPEED;
				}
			}
			if (mo.player != null && (mo.flags2 & DoomDef.MF2_FLY) != 0 && !(mo.z <= mo.floorz)
				&& (p_tick.leveltime & 2) != 0)
			{
				mo.z += tables.finesine[(DoomDef.FINEANGLES / 20 * p_tick.leveltime >> 2) & DoomDef.FINEMASK];
			}

			//
			// clip movement
			//
			if (mo.z <= mo.floorz)
			{ // Hit the floor
				if ((mo.flags & DoomDef.MF_MISSILE) != 0)
				{
					mo.z = mo.floorz;
					if ((mo.flags2 & DoomDef.MF2_FLOORBOUNCE) != 0)
					{
						p_mobj.P_FloorBounceMissile(mo);
						return;
					}
					else if (mo.type == info.mobjtype_t.MT_MNTRFX2)
					{ // Minotaur floor fire can go up steps
						return;
					}
					else
					{
						P_ExplodeMissile(mo);
						return;
					}
				}
				if (mo.z - mo.momz > mo.floorz)
				{ // Spawn splashes, etc.
					P_HitFloor(mo);
				}
				mo.z = mo.floorz;
				if (mo.momz < 0)
				{
					if (mo.player != null && mo.momz < -p_local.GRAVITY * 8
						&& (mo.flags2 & DoomDef.MF2_FLY) == 0)       // squat down
					{
						mo.player.deltaviewheight = mo.momz >> 3;
						i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_plroof);
						mo.player.centering = true;
					}
					mo.momz = 0;
				}
				if ((mo.flags & DoomDef.MF_SKULLFLY) != 0)
				{ // The skull slammed into something
					mo.momz = -mo.momz;
				}
				if (mo.infol.crashstate != 0 && (mo.flags & DoomDef.MF_CORPSE) != 0)
				{
					P_SetMobjState(mo, (info.statenum_t)mo.infol.crashstate);
					return;
				}
			}
			else if ((mo.flags2 & DoomDef.MF2_LOGRAV) != 0)
			{
				if (mo.momz == 0)
					mo.momz = -(p_local.GRAVITY >> 3) * 2;
				else
					mo.momz -= p_local.GRAVITY >> 3;
			}
			else if ((mo.flags & DoomDef.MF_NOGRAVITY) == 0)
			{
				if (mo.momz == 0)
					mo.momz = -p_local.GRAVITY * 2;
				else
					mo.momz -= p_local.GRAVITY;
			}

			if (mo.z + mo.height > mo.ceilingz)
			{       // hit the ceiling
				if (mo.momz > 0)
					mo.momz = 0;
				mo.z = mo.ceilingz - mo.height;
				if ((mo.flags & DoomDef.MF_SKULLFLY) != 0)
				{       // the skull slammed into something
					mo.momz = -mo.momz;
				}
				if ((mo.flags & DoomDef.MF_MISSILE) != 0)
				{
					if (mo.subsector.sector.ceilingpic == r_plane.skyflatnum)
					{
						if (mo.type == info.mobjtype_t.MT_BLOODYSKULL)
						{
							mo.momx = mo.momy = 0;
							mo.momz = -DoomDef.FRACUNIT;
						}
						else
						{
							P_RemoveMobj(mo);
						}
						return;
					}
					P_ExplodeMissile(mo);
					return;
				}
			}
		}

		/*
		================
		=
		= P_NightmareRespawn
		=
		================
		*/

		public static void P_NightmareRespawn(DoomDef.mobj_t mobj)
		{
			int x, y, z;
			r_local.subsector_t ss;
			DoomDef.mobj_t mo;
			DoomData.mapthing_t mthing;

			x = mobj.spawnpoint.x << DoomDef.FRACBITS;
			y = mobj.spawnpoint.y << DoomDef.FRACBITS;

			if (!p_map.P_CheckPosition(mobj, x, y))
				return; // somthing is occupying it's position


			// spawn a teleport fog at old spot

			mo = P_SpawnMobj(mobj.x, mobj.y,
				mobj.subsector.sector.floorheight + DoomDef.TELEFOGHEIGHT, info.mobjtype_t.MT_TFOG);
			i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_telept);

			// spawn a teleport fog at the new spot
			ss = r_main.R_PointInSubsector(x, y);
			mo = P_SpawnMobj(x, y, ss.sector.floorheight + DoomDef.TELEFOGHEIGHT, info.mobjtype_t.MT_TFOG);
			i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_telept);

			// spawn the new monster
			mthing = mobj.spawnpoint;

			// spawn it
			if ((mobj.infol.flags & DoomDef.MF_SPAWNCEILING) != 0)
				z = p_local.ONCEILINGZ;
			else
				z = p_local.ONFLOORZ;
			mo = P_SpawnMobj(x, y, z, mobj.type);
			mo.spawnpoint = mobj.spawnpoint;
			mo.angle = (uint)(DoomDef.ANG45 * (mthing.angle / 45));
			if ((mthing.options & DoomData.MTF_AMBUSH) != 0)
				mo.flags |= DoomDef.MF_AMBUSH;

			mo.reactiontime = 18;

			// remove the old monster
			P_RemoveMobj(mobj);
		}
#if DOS

//----------------------------------------------------------------------------
//
// PROC P_BlasterMobjThinker
//
// Thinker for the ultra-fast blaster PL2 ripper-spawning missile.
//
//----------------------------------------------------------------------------

void P_BlasterMobjThinker(mobj_t *mobj)
{
	int i;
	int xfrac;
	int yfrac;
	int zfrac;
	int z;
	boolean changexy;

	// Handle movement
	if(mobj.momx || mobj.momy ||
		(mobj.z != mobj.floorz) || mobj.momz)
	{
		xfrac = mobj.momx>>3;
		yfrac = mobj.momy>>3;
		zfrac = mobj.momz>>3;
		changexy = xfrac || yfrac;
		for(i = 0; i < 8; i++)
		{
			if(changexy)
			{
				if(!P_TryMove(mobj, mobj.x+xfrac, mobj.y+yfrac))
				{ // Blocked move
					P_ExplodeMissile(mobj);
					return;
				}
			}
			mobj.z += zfrac;
			if(mobj.z <= mobj.floorz)
			{ // Hit the floor
				mobj.z = mobj.floorz;
				P_HitFloor(mobj);
				P_ExplodeMissile(mobj);
				return;
			}
			if(mobj.z+mobj.height > mobj.ceilingz)
			{ // Hit the ceiling
				mobj.z = mobj.ceilingz-mobj.height;
				P_ExplodeMissile(mobj);
				return;
			}
			if(changexy && (P_Random() < 64))
			{
				z = mobj.z-8*FRACUNIT;
				if(z < mobj.floorz)
				{
					z = mobj.floorz;
				}
				P_SpawnMobj(mobj.x, mobj.y, z, MT_BLASTERSMOKE);
			}
		}
	}
	// Advance the state
	if(mobj.tics != -1)
	{
		mobj.tics--;
		while(!mobj.tics)
		{
			if(!P_SetMobjState(mobj, mobj.state.nextstate))
			{ // mobj was removed
				return;
			}
		}
	}
}
#endif
		//----------------------------------------------------------------------------
		//
		// PROC P_MobjThinker
		//
		//----------------------------------------------------------------------------
		public class P_MobjThinker : DoomDef.think_t_delegate
		{
			public P_MobjThinker(object in_obj) : base(in_obj) { }
			public override void function(object obj)
			{
				DoomDef.mobj_t mobj = obj as DoomDef.mobj_t;
				DoomDef.mobj_t onmo;

				// Handle X and Y momentums
				if (mobj.momx != 0 || mobj.momy != 0 || (mobj.flags & DoomDef.MF_SKULLFLY) != 0)
				{
					P_XYMovement(mobj);
					if (mobj.thinker.function == null)
					{ // mobj was removed
						return;
					}
				}
				if ((mobj.flags2 & DoomDef.MF2_FLOATBOB) != 0)
				{ // Floating item bobbing motion
					mobj.z = mobj.floorz + FloatBobOffsets[(mobj.health++) & 63];
				}
				else if ((mobj.z != mobj.floorz) || mobj.momz != 0)
				{ // Handle Z momentum and gravity
					if ((mobj.flags2 & DoomDef.MF2_PASSMOBJ) != 0)
					{
						onmo = p_map.P_CheckOnmobj(mobj);
						if (onmo == null)
						{
							P_ZMovement(mobj);
						}
						else
						{
							if (mobj.player != null && mobj.momz < 0)
							{
								mobj.flags2 |= DoomDef.MF2_ONMOBJ;
								mobj.momz = 0;
							}
							if (mobj.player != null && (onmo.player != null || onmo.type == info.mobjtype_t.MT_POD))
							{
								mobj.momx = onmo.momx;
								mobj.momy = onmo.momy;
								if (onmo.z < onmo.floorz)
								{
									mobj.z += onmo.floorz - onmo.z;
									if (onmo.player != null)
									{
										onmo.player.viewheight -= onmo.floorz - onmo.z;
										onmo.player.deltaviewheight = (p_local.VIEWHEIGHT -
											onmo.player.viewheight) >> 3;
									}
									onmo.z = onmo.floorz;
								}
							}
						}
					}
					else
					{
						P_ZMovement(mobj);
					}
					if (mobj.thinker.function == null)
					{ // mobj was removed
						return;
					}
				}

				//
				// cycle through states, calling action functions at transitions
				//
				if (mobj.tics != -1)
				{
					mobj.tics--;
					// you can cycle through multiple states in a tic
					while (mobj.tics == 0)
					{
						if (!P_SetMobjState(mobj, mobj.state.nextstate))
						{ // mobj was removed
							return;
						}
					}
				}
				else
				{ // Check for monster respawn
					if ((mobj.flags & DoomDef.MF_COUNTKILL) == 0)
					{
						return;
					}
					if (!g_game.respawnmonsters)
					{
						return;
					}
					mobj.movecount++;
					if (mobj.movecount < 12 * 35)
					{
						return;
					}
					if ((p_tick.leveltime & 31) != 0)
					{
						return;
					}
					if (m_misc.P_Random() > 4)
					{
						return;
					}
					P_NightmareRespawn(mobj);
				}
			}
		}


		/*
		===============
		=
		= P_SpawnMobj
		=
		===============
		*/

		public static DoomDef.mobj_t P_SpawnMobj(int x, int y, int z, info.mobjtype_t type)
		{
			DoomDef.mobj_t mobj;
			info.state_t st;
			info.mobjinfo_t infol;
			int space;

			mobj = new DoomDef.mobj_t();
			infol = info.mobjinfo[(int)type];
			mobj.type = type;
			mobj.infol = infol;
			mobj.x = x;
			mobj.y = y;
			mobj.radius = infol.radius;
			mobj.height = infol.height;
			mobj.flags = infol.flags;
			mobj.flags2 = infol.flags2;
			mobj.damage = infol.damage;
			mobj.health = infol.spawnhealth;
			if (g_game.gameskill != DoomDef.skill_t.sk_nightmare)
			{
				mobj.reactiontime = infol.reactiontime;
			}
			mobj.lastlook = m_misc.P_Random() % DoomDef.MAXPLAYERS;

			// Set the state, but do not use P_SetMobjState, because action
			// routines can't be called yet.  If the spawnstate has an action
			// routine, it will not be called.
			st = info.states[infol.spawnstate];
			mobj.state = st;
			mobj.tics = (int)st.tics;
			mobj.sprite = st.sprite;
			mobj.frame = (int)st.frame;

			// Set subsector and/or block links.
			p_maputl.P_SetThingPosition(mobj);
			mobj.floorz = mobj.subsector.sector.floorheight;
			mobj.ceilingz = mobj.subsector.sector.ceilingheight;
			if (z == p_local.ONFLOORZ)
			{
				mobj.z = mobj.floorz;
			}
			else if (z == p_local.ONCEILINGZ)
			{
				mobj.z = mobj.ceilingz - mobj.infol.height;
			}
			else if (z == p_local.FLOATRANDZ)
			{
				space = ((mobj.ceilingz) - (mobj.infol.height)) - mobj.floorz;
				if (space > 48 * DoomDef.FRACUNIT)
				{
					space -= 40 * DoomDef.FRACUNIT;
					mobj.z = ((space * m_misc.P_Random()) >> 8) + mobj.floorz + 40 * DoomDef.FRACUNIT;
				}
				else
				{
					mobj.z = mobj.floorz;
				}
			}
			else
			{
				mobj.z = z;
			}
			if ((mobj.flags2 & DoomDef.MF2_FOOTCLIP) != 0 && P_GetThingFloorType(mobj) != p_local.FLOOR_SOLID
				&& mobj.floorz == mobj.subsector.sector.floorheight)
			{
				mobj.flags2 |= DoomDef.MF2_FEETARECLIPPED;
			}
			else
			{
				mobj.flags2 &= ~DoomDef.MF2_FEETARECLIPPED;
			}

			mobj.thinker.function = new P_MobjThinker(mobj);
			p_tick.P_AddThinker(mobj.thinker);
			return (mobj);
		}

		/*
		===============
		=
		= P_RemoveMobj
		=
		===============
		*/

		public static void P_RemoveMobj(DoomDef.mobj_t mobj)
		{
			// unlink from sector and block lists
			p_maputl.P_UnsetThingPosition(mobj);
			// stop any playing sound
			i_ibm.S_StopSound(mobj);
			// free block
			p_tick.P_RemoveThinker(mobj.thinker);
		}

		//=============================================================================


		/*
		============
		=
		= P_SpawnPlayer
		=
		= Called when a player is spawned on the level 
		= Most of the player structure stays unchanged between levels
		============
		*/

		public static void P_SpawnPlayer(DoomData.mapthing_t mthing)
		{
			DoomDef.player_t p;
			int x, y, z;
			DoomDef.mobj_t mobj;
			int i;

			if (!g_game.playeringame[mthing.type - 1])
				return;                                         // not playing
			p = g_game.players[mthing.type - 1];

			if (p.playerstate == DoomDef.playerstate_t.PST_REBORN)
				g_game.G_PlayerReborn(mthing.type - 1);

			x = mthing.x << DoomDef.FRACBITS;
			y = mthing.y << DoomDef.FRACBITS;

			z = p_local.ONFLOORZ;
			mobj = P_SpawnMobj(x, y, z, info.mobjtype_t.MT_PLAYER);
			if (mthing.type > 1)           // set color translations for player sprites
				mobj.flags |= (mthing.type - 1) << DoomDef.MF_TRANSSHIFT;

			mobj.angle = (uint)(DoomDef.ANG45 * (mthing.angle / 45));
			mobj.player = p;
			mobj.health = p.health;
			p.mo = mobj;
			p.playerstate = DoomDef.playerstate_t.PST_LIVE;
			p.refire = 0;
			p.message = "";
			p.damagecount = 0;
			p.bonuscount = 0;
			p.chickenTics = 0;
			p.rain1 = null;
			p.rain2 = null;
			p.extralight = 0;
			p.fixedcolormap = 0;
			p.viewheight = p_local.VIEWHEIGHT;
			p_pspr.P_SetupPsprites(p); // setup gun psprite        
			if (g_game.deathmatch)
			{ // Give all keys in death match mode
				for (i = 0; i < (int)DoomDef.keytype_t.NUMKEYS; i++)
				{
					p.keys[i] = true;
					if (p == g_game.players[g_game.consoleplayer])
					{
						sb_bar.playerkeys = 7;
						i_ibm.UpdateState |= DoomDef.I_STATBAR;
					}
				}
			}
			else if (p == g_game.players[g_game.consoleplayer])
			{
				sb_bar.playerkeys = 0;
				i_ibm.UpdateState |= DoomDef.I_STATBAR;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_SpawnMapThing
		//
		// The fields of the mapthing should already be in host byte order.
		//
		//----------------------------------------------------------------------------

		public static void P_SpawnMapThing(DoomData.mapthing_t mthing)
		{
			int i;
			int bit;
			DoomDef.mobj_t mobj;
			int x, y, z;

			// count deathmatch start positions
			if (mthing.type == 11)
			{
				if (p_setup.deathmatch_pi < 10)
				{
					p_setup.deathmatchstarts[p_setup.deathmatch_pi].x = mthing.x;
					p_setup.deathmatchstarts[p_setup.deathmatch_pi].y = mthing.y;
					p_setup.deathmatchstarts[p_setup.deathmatch_pi].angle = mthing.angle;
					p_setup.deathmatchstarts[p_setup.deathmatch_pi].options = mthing.options;
					p_setup.deathmatchstarts[p_setup.deathmatch_pi].type = mthing.type;
					p_setup.deathmatch_pi++;
				}
				return;
			}

			// check for players specially
			if (mthing.type <= 4)
			{
				// save spots for respawning in network games
				p_setup.playerstarts[mthing.type - 1].x = mthing.x;
				p_setup.playerstarts[mthing.type - 1].y = mthing.y;
				p_setup.playerstarts[mthing.type - 1].angle = mthing.angle;
				p_setup.playerstarts[mthing.type - 1].options = mthing.options;
				p_setup.playerstarts[mthing.type - 1].type = mthing.type;
				if (!g_game.deathmatch)
				{
					P_SpawnPlayer(mthing);
				}
				return;
			}

			// Ambient sound sequences
			if (mthing.type >= 1200 && mthing.type < 1300)
			{
				p_spec.P_AddAmbientSfx(mthing.type - 1200);
				return;
			}

			// Check for boss spots
			if (mthing.type == 56) // Monster_BossSpot
			{
				p_enemy.P_AddBossSpot(mthing.x << DoomDef.FRACBITS, mthing.y << DoomDef.FRACBITS,
					(uint)(DoomDef.ANG45 * (mthing.angle / 45)));
				return;
			}

			// check for apropriate skill level
			if (!g_game.netgame && (mthing.options & 16) != 0)
				return;

			if (g_game.gameskill == DoomDef.skill_t.sk_baby)
				bit = 1;
			else if (g_game.gameskill == DoomDef.skill_t.sk_nightmare)
				bit = 4;
			else
				bit = 1 << ((int)g_game.gameskill - 1);
			if ((mthing.options & bit) == 0)
				return;

			// find which type to spawn
			for (i = 0; i < (int)info.mobjtype_t.NUMMOBJTYPES; i++)
				if (mthing.type == info.mobjinfo[i].doomednum)
					break;

			if ((info.mobjtype_t)i == info.mobjtype_t.MT_MISC10)
			{
				// Make sure it's away from any walls
				Vector2 pos = new Vector2(
					mthing.x,
					mthing.y
					);
				foreach (r_local.seg_t seg in p_setup.segs)
				{
					Vector2 v = new Vector2(
						seg.v1.x >> DoomDef.FRACBITS,
						seg.v1.y >> DoomDef.FRACBITS
						);
					Vector2 w = new Vector2(
						seg.v2.x >> DoomDef.FRACBITS,
						seg.v2.y >> DoomDef.FRACBITS
						);
					float dis = p_maputl.minimum_distance(v, w, pos);
					if (dis < 6.0f)
					{
						Vector2 dir = w - v;
						dir.Normalize();
						Vector2 normal = new Vector2(dir.Y, -dir.X);
						pos += normal * (6.0f - dis);
						mthing.x = (short)pos.X;
						mthing.y = (short)pos.Y;
					}
				}
			}

			if (i == (int)info.mobjtype_t.NUMMOBJTYPES)
				i_ibm.I_Error("P_SpawnMapThing: Unknown type " + mthing.type + " at (" + mthing.x + ", " + mthing.y + ")");

			// don't spawn keys and players in deathmatch
			if (g_game.deathmatch && (info.mobjinfo[i].flags & DoomDef.MF_NOTDMATCH) != 0)
				return;

			// don't spawn any monsters if -nomonsters
			if (d_main.nomonsters && (info.mobjinfo[i].flags & DoomDef.MF_COUNTKILL) != 0)
				return;

			// spawn it
			switch (i)
			{ // Special stuff
				case (int)info.mobjtype_t.MT_WSKULLROD:
				case (int)info.mobjtype_t.MT_WPHOENIXROD:
				case (int)info.mobjtype_t.MT_AMSKRDWIMPY:
				case (int)info.mobjtype_t.MT_AMSKRDHEFTY:
				case (int)info.mobjtype_t.MT_AMPHRDWIMPY:
				case (int)info.mobjtype_t.MT_AMPHRDHEFTY:
				case (int)info.mobjtype_t.MT_AMMACEWIMPY:
				case (int)info.mobjtype_t.MT_AMMACEHEFTY:
				case (int)info.mobjtype_t.MT_ARTISUPERHEAL:
				case (int)info.mobjtype_t.MT_ARTITELEPORT:
				case (int)info.mobjtype_t.MT_ITEMSHIELD2:
					if (d_main.shareware)
					{ // Don't place on map in shareware version
						return;
					}
					break;
				case (int)info.mobjtype_t.MT_WMACE:
					if (!d_main.shareware)
					{ // Put in the mace spot list
						p_pspr.P_AddMaceSpot(mthing);
						return;
					}
					return;
				default:
					break;
			}
			x = mthing.x << DoomDef.FRACBITS;
			y = mthing.y << DoomDef.FRACBITS;
			if ((info.mobjinfo[i].flags & DoomDef.MF_SPAWNCEILING) != 0)
			{
				z = p_local.ONCEILINGZ;
			}
			else if ((info.mobjinfo[i].flags2 & DoomDef.MF2_SPAWNFLOAT) != 0)
			{
				z = p_local.FLOATRANDZ;
			}
			else
			{
				z = p_local.ONFLOORZ;
			}
			mobj = p_mobj.P_SpawnMobj(x, y, z, (info.mobjtype_t)i);
			if ((mobj.flags2 & DoomDef.MF2_FLOATBOB) != 0)
			{ // Seed random starting index for bobbing motion
				mobj.health = m_misc.P_Random();
			}
			if (mobj.tics > 0)
			{
				mobj.tics = 1 + (m_misc.P_Random() % mobj.tics);
			}
			if ((mobj.flags & DoomDef.MF_COUNTKILL) != 0)
			{
				g_game.totalkills++;
				mobj.spawnpoint.x = mthing.x;
				mobj.spawnpoint.y = mthing.y;
				mobj.spawnpoint.angle = mthing.angle;
				mobj.spawnpoint.options = mthing.options;
				mobj.spawnpoint.type = mthing.type;

			}
			if ((mobj.flags & DoomDef.MF_COUNTITEM) != 0)
			{
				g_game.totalitems++;
			}
			mobj.angle = (uint)(DoomDef.ANG45 * (mthing.angle / 45));
			if ((mthing.options & DoomData.MTF_AMBUSH) != 0)
			{
				mobj.flags |= DoomDef.MF_AMBUSH;
			}
		}
		/*
		===============================================================================

								GAME SPAWN FUNCTIONS

		===============================================================================
		*/

		//---------------------------------------------------------------------------
		//
		// PROC P_SpawnPuff
		//
		//---------------------------------------------------------------------------

		public static void P_SpawnPuff(int x, int y, int z)
		{
			DoomDef.mobj_t puff;

			z += ((m_misc.P_Random() - m_misc.P_Random()) << 10);
			puff = P_SpawnMobj(x, y, z, PuffType);
			if (puff.infol.attacksound != 0)
			{
				i_ibm.S_StartSound(puff, puff.infol.attacksound);
			}
			switch (PuffType)
			{
				case info.mobjtype_t.MT_BEAKPUFF:
				case info.mobjtype_t.MT_STAFFPUFF:
					puff.momz = DoomDef.FRACUNIT;
					break;
				case info.mobjtype_t.MT_GAUNTLETPUFF1:
				case info.mobjtype_t.MT_GAUNTLETPUFF2:
					puff.momz = (int)(.8 * (double)DoomDef.FRACUNIT);
					break;
				default:
					break;
			}
		}


		//---------------------------------------------------------------------------
		//
		// PROC P_BloodSplatter
		//
		//---------------------------------------------------------------------------

		public static void P_BloodSplatter(int x, int y, int z, DoomDef.mobj_t originator)
		{
			DoomDef.mobj_t mo;

			mo = P_SpawnMobj(x, y, z, info.mobjtype_t.MT_BLOODSPLATTER);
			mo.target = originator;
			mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 9;
			mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 9;
			mo.momz = DoomDef.FRACUNIT * 2;
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_RipperBlood
		//
		//---------------------------------------------------------------------------

		public static void P_RipperBlood(DoomDef.mobj_t mo)
		{
			DoomDef.mobj_t th;
			int x, y, z;

			x = mo.x + ((m_misc.P_Random() - m_misc.P_Random()) << 12);
			y = mo.y + ((m_misc.P_Random() - m_misc.P_Random()) << 12);
			z = mo.z + ((m_misc.P_Random() - m_misc.P_Random()) << 12);
			th = P_SpawnMobj(x, y, z, info.mobjtype_t.MT_BLOOD);
			th.flags |= DoomDef.MF_NOGRAVITY;
			th.momx = mo.momx >> 1;
			th.momy = mo.momy >> 1;
			th.tics += m_misc.P_Random() & 3;
		}

		//---------------------------------------------------------------------------
		//
		// FUNC P_GetThingFloorType
		//
		//---------------------------------------------------------------------------

		public static int P_GetThingFloorType(DoomDef.mobj_t thing)
		{
			return (p_spec.TerrainTypes[thing.subsector.sector.floorpic]);
		}

		//---------------------------------------------------------------------------
		//
		// FUNC P_HitFloor
		//
		//---------------------------------------------------------------------------

		public static int P_HitFloor(DoomDef.mobj_t thing)
		{
			DoomDef.mobj_t mo;

			if (thing.floorz != thing.subsector.sector.floorheight)
			{ // don't splash if landing on the edge above water/lava/etc....
				return (p_local.FLOOR_SOLID);
			}
			switch (P_GetThingFloorType(thing))
			{
				case p_local.FLOOR_WATER:
					P_SpawnMobj(thing.x, thing.y, p_local.ONFLOORZ, info.mobjtype_t.MT_SPLASHBASE);
					mo = P_SpawnMobj(thing.x, thing.y, p_local.ONFLOORZ, info.mobjtype_t.MT_SPLASH);
					mo.target = thing;
					mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 8;
					mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 8;
					mo.momz = 2 * DoomDef.FRACUNIT + (m_misc.P_Random() << 8);
					i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_gloop);
					return (p_local.FLOOR_WATER);
				case p_local.FLOOR_LAVA:
					P_SpawnMobj(thing.x, thing.y, p_local.ONFLOORZ, info.mobjtype_t.MT_LAVASPLASH);
					mo = P_SpawnMobj(thing.x, thing.y, p_local.ONFLOORZ, info.mobjtype_t.MT_LAVASMOKE);
					mo.momz = DoomDef.FRACUNIT + (m_misc.P_Random() << 7);
					i_ibm.S_StartSound(mo, (int)sounds.sfxenum_t.sfx_burn);
					return (p_local.FLOOR_LAVA);
				case p_local.FLOOR_SLUDGE:
					P_SpawnMobj(thing.x, thing.y, p_local.ONFLOORZ, info.mobjtype_t.MT_SLUDGESPLASH);
					mo = P_SpawnMobj(thing.x, thing.y, p_local.ONFLOORZ, info.mobjtype_t.MT_SLUDGECHUNK);
					mo.target = thing;
					mo.momx = (m_misc.P_Random() - m_misc.P_Random()) << 8;
					mo.momy = (m_misc.P_Random() - m_misc.P_Random()) << 8;
					mo.momz = DoomDef.FRACUNIT + (m_misc.P_Random() << 8);
					return (p_local.FLOOR_SLUDGE);
			}
			return (p_local.FLOOR_SOLID);
		}

		//---------------------------------------------------------------------------
		//
		// FUNC P_CheckMissileSpawn
		//
		// Returns true if the missile is at a valid spawn point, otherwise
		// explodes it and returns false.
		//
		//---------------------------------------------------------------------------

		public static bool P_CheckMissileSpawn(DoomDef.mobj_t missile)
		{
			//missile.tics -= P_Random()&3;

			// move a little forward so an angle can be computed if it
			// immediately explodes
			missile.x += (missile.momx >> 1);
			missile.y += (missile.momy >> 1);
			missile.z += (missile.momz >> 1);
			if (!p_map.P_TryMove(missile, missile.x, missile.y))
			{
				P_ExplodeMissile(missile);
				return (false);
			}
			return (true);
		}

		//---------------------------------------------------------------------------
		//
		// FUNC P_SpawnMissile
		//
		// Returns NULL if the missile exploded immediately, otherwise returns
		// a mobj_t pointer to the missile.
		//
		//---------------------------------------------------------------------------

		public static DoomDef.mobj_t P_SpawnMissile(DoomDef.mobj_t source, DoomDef.mobj_t dest, info.mobjtype_t type)
		{
			int z;
			DoomDef.mobj_t th;
			uint an;
			int dist;

			switch (type)
			{
				case info.mobjtype_t.MT_MNTRFX1: // Minotaur swing attack missile
					z = source.z + 40 * DoomDef.FRACUNIT;
					break;
				case info.mobjtype_t.MT_MNTRFX2: // Minotaur floor fire missile
					z = p_local.ONFLOORZ;
					break;
				case info.mobjtype_t.MT_SRCRFX1: // Sorcerer Demon fireball
					z = source.z + 48 * DoomDef.FRACUNIT;
					break;
				case info.mobjtype_t.MT_KNIGHTAXE: // Knight normal axe
				case info.mobjtype_t.MT_REDAXE: // Knight red power axe
					z = source.z + 36 * DoomDef.FRACUNIT;
					break;
				default:
					z = source.z + 32 * DoomDef.FRACUNIT;
					break;
			}
			if ((source.flags2 & DoomDef.MF2_FEETARECLIPPED) != 0)
			{
				z -= p_local.FOOTCLIPSIZE;
			}
			th = P_SpawnMobj(source.x, source.y, z, type);
			if (th.infol.seesound != 0)
			{
				i_ibm.S_StartSound(th, th.infol.seesound);
			}
			th.target = source; // Originator
			an = r_main.R_PointToAngle2(source.x, source.y, dest.x, dest.y);
			if ((dest.flags & DoomDef.MF_SHADOW) != 0)
			{ // Invisible target
				an += (uint)((m_misc.P_Random() - m_misc.P_Random()) << 21);
			}
			th.angle = an;
			an >>= (int)DoomDef.ANGLETOFINESHIFT;
			th.momx = DoomDef.FixedMul(th.infol.speed, r_main.finecosine(an));
			th.momy = DoomDef.FixedMul(th.infol.speed, tables.finesine[an]);
			dist = p_maputl.P_AproxDistance(dest.x - source.x, dest.y - source.y);
			dist = dist / th.infol.speed;
			if (dist < 1)
			{
				dist = 1;
			}
			th.momz = (dest.z - source.z) / dist;
			return (P_CheckMissileSpawn(th) ? th : null);
		}

		//---------------------------------------------------------------------------
		//
		// FUNC P_SpawnMissileAngle
		//
		// Returns NULL if the missile exploded immediately, otherwise returns
		// a mobj_t pointer to the missile.
		//
		//---------------------------------------------------------------------------

		public static DoomDef.mobj_t P_SpawnMissileAngle(DoomDef.mobj_t source, info.mobjtype_t type,
			uint angle, int momz)
		{
			int z;
			DoomDef.mobj_t mo;

			switch (type)
			{
				case info.mobjtype_t.MT_MNTRFX1: // Minotaur swing attack missile
					z = source.z + 40 * DoomDef.FRACUNIT;
					break;
				case info.mobjtype_t.MT_MNTRFX2: // Minotaur floor fire missile
					z = p_local.ONFLOORZ;
					break;
				case info.mobjtype_t.MT_SRCRFX1: // Sorcerer Demon fireball
					z = source.z + 48 * DoomDef.FRACUNIT;
					break;
				default:
					z = source.z + 32 * DoomDef.FRACUNIT;
					break;
			}
			if ((source.flags2 & DoomDef.MF2_FEETARECLIPPED) != 0)
			{
				z -= p_local.FOOTCLIPSIZE;
			}
			mo = P_SpawnMobj(source.x, source.y, z, type);
			if (mo.infol.seesound != 0)
			{
				i_ibm.S_StartSound(mo, mo.infol.seesound);
			}
			mo.target = source; // Originator
			mo.angle = angle;
			angle >>= (int)DoomDef.ANGLETOFINESHIFT;
			mo.momx = DoomDef.FixedMul(mo.infol.speed, r_main.finecosine(angle));
			mo.momy = DoomDef.FixedMul(mo.infol.speed, tables.finesine[angle]);
			mo.momz = momz;
			return (P_CheckMissileSpawn(mo) ? mo : null);
		}


		/*
		================
		=
		= P_SpawnPlayerMissile
		=
		= Tries to aim at a nearby monster
		================
		*/

		public static DoomDef.mobj_t P_SpawnPlayerMissile(DoomDef.mobj_t source, info.mobjtype_t type)
		{
			uint an;
			int x, y, z, slope;

			// Try to find a target
			an = source.angle;
			slope = p_map.P_AimLineAttack(source, an, 16 * 64 * DoomDef.FRACUNIT);
			if (p_map.linetarget == null)
			{
				an += 1 << 26;
				slope = p_map.P_AimLineAttack(source, an, 16 * 64 * DoomDef.FRACUNIT);
				if (p_map.linetarget == null)
				{
					an -= 2 << 26;
					slope = p_map.P_AimLineAttack(source, an, 16 * 64 * DoomDef.FRACUNIT);
				}
				if (p_map.linetarget == null)
				{
					an = source.angle;
					slope = ((source.player.lookdir) << DoomDef.FRACBITS) / 173;
				}
			}
			x = source.x;
			y = source.y;
			z = source.z + 4 * 8 * DoomDef.FRACUNIT + ((source.player.lookdir) << DoomDef.FRACBITS) / 173;
			if ((source.flags2 & DoomDef.MF2_FEETARECLIPPED) != 0)
			{
				z -= p_local.FOOTCLIPSIZE;
			}
			MissileMobj = P_SpawnMobj(x, y, z, type);
			if (MissileMobj.infol.seesound != 0)
			{
				i_ibm.S_StartSound(MissileMobj, MissileMobj.infol.seesound);
			}
			MissileMobj.target = source;
			MissileMobj.angle = an;
			MissileMobj.momx = DoomDef.FixedMul(MissileMobj.infol.speed,
				r_main.finecosine(an >> (int)DoomDef.ANGLETOFINESHIFT));
			MissileMobj.momy = DoomDef.FixedMul(MissileMobj.infol.speed,
				tables.finesine[an >> (int)DoomDef.ANGLETOFINESHIFT]);
			MissileMobj.momz = DoomDef.FixedMul(MissileMobj.infol.speed, slope);
			if (MissileMobj.type == info.mobjtype_t.MT_BLASTERFX1)
			{ // Ultra-fast ripper spawning missile
				MissileMobj.x += (MissileMobj.momx >> 3);
				MissileMobj.y += (MissileMobj.momy >> 3);
				MissileMobj.z += (MissileMobj.momz >> 3);
			}
			else
			{ // Normal missile
				MissileMobj.x += (MissileMobj.momx >> 1);
				MissileMobj.y += (MissileMobj.momy >> 1);
				MissileMobj.z += (MissileMobj.momz >> 1);
			}
			if (!p_map.P_TryMove(MissileMobj, MissileMobj.x, MissileMobj.y))
			{ // Exploded immediately
				P_ExplodeMissile(MissileMobj);
				return (null);
			}
			return (MissileMobj);
		}

		//---------------------------------------------------------------------------
		//
		// PROC P_SPMAngle
		//
		//---------------------------------------------------------------------------

		public static DoomDef.mobj_t P_SPMAngle(DoomDef.mobj_t source, info.mobjtype_t type, uint angle)
		{
			DoomDef.mobj_t th;
			uint an;
			int x, y, z, slope;

			//
			// see which target is to be aimed at
			//
			an = angle;
			slope = p_map.P_AimLineAttack(source, an, 16 * 64 * DoomDef.FRACUNIT);
			if (p_map.linetarget == null)
			{
				an += 1 << 26;
				slope = p_map.P_AimLineAttack(source, an, 16 * 64 * DoomDef.FRACUNIT);
				if (p_map.linetarget == null)
				{
					an -= 2 << 26;
					slope = p_map.P_AimLineAttack(source, an, 16 * 64 * DoomDef.FRACUNIT);
				}
				if (p_map.linetarget == null)
				{
					an = angle;
					slope = ((source.player.lookdir) << DoomDef.FRACBITS) / 173;
				}
			}
			x = source.x;
			y = source.y;
			z = source.z + 4 * 8 * DoomDef.FRACUNIT + ((source.player.lookdir) << DoomDef.FRACBITS) / 173;
			if ((source.flags2 & DoomDef.MF2_FEETARECLIPPED) != 0)
			{
				z -= p_local.FOOTCLIPSIZE;
			}
			th = P_SpawnMobj(x, y, z, type);
			if (th.infol.seesound != 0)
			{
				i_ibm.S_StartSound(th, th.infol.seesound);
			}
			th.target = source;
			th.angle = an;
			th.momx = DoomDef.FixedMul(th.infol.speed, r_main.finecosine(an >> (int)DoomDef.ANGLETOFINESHIFT));
			th.momy = DoomDef.FixedMul(th.infol.speed, tables.finesine[an >> (int)DoomDef.ANGLETOFINESHIFT]);
			th.momz = DoomDef.FixedMul(th.infol.speed, slope);
			return (P_CheckMissileSpawn(th) ? th : null);
		}

		//---------------------------------------------------------------------------
		//
		// PROC A_ContMobjSound
		//
		//---------------------------------------------------------------------------
		public class A_ContMobjSound : info.state_t_actionDelegate
		{
			public override void action(DoomDef.mobj_t thing)
			{
				//switch (actor.type)
				//{
				//    case MT_KNIGHTAXE:
				//        S_StartSound(actor, sfx_kgtatk);
				//        break;
				//    case MT_MUMMYFX1:
				//        S_StartSound(actor, sfx_mumhed);
				//        break;
				//    default:
				//        break;
				//}
			}
		}


	}
}
