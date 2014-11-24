using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

///////////////////////////////
// PORT - DONE
///////////////////////////////

// P_telept.c

namespace HereticXNA
{
	public static class p_telept
	{
		//----------------------------------------------------------------------------
		//
		// FUNC P_Teleport
		//
		//----------------------------------------------------------------------------

		public static bool P_Teleport(DoomDef.mobj_t thing, int x, int y, uint angle)
		{
			int oldx;
			int oldy;
			int oldz;
			int aboveFloor;
			int fogDelta;
			DoomDef.player_t player;
			uint an;
			DoomDef.mobj_t fog;

			oldx = thing.x;
			oldy = thing.y;
			oldz = thing.z;
			aboveFloor = thing.z - thing.floorz;
			if (!p_map.P_TeleportMove(thing, x, y))
			{
				return (false);
			}
			if (thing.player != null)
			{
				player = thing.player;
				if (player.powers[(int)DoomDef.powertype_t.pw_flight] != 0 && aboveFloor != 0)
				{
					thing.z = thing.floorz + aboveFloor;
					if (thing.z + thing.height > thing.ceilingz)
					{
						thing.z = thing.ceilingz - thing.height;
					}
					player.viewz = thing.z + player.viewheight;
				}
				else
				{
					thing.z = thing.floorz;
					player.viewz = thing.z + player.viewheight;
					player.lookdir = 0;
				}
			}
			else if ((thing.flags & DoomDef.MF_MISSILE) != 0)
			{
				thing.z = thing.floorz + aboveFloor;
				if (thing.z + thing.height > thing.ceilingz)
				{
					thing.z = thing.ceilingz - thing.height;
				}
			}
			else
			{
				thing.z = thing.floorz;
			}
			// Spawn teleport fog at source and destination
			fogDelta = (thing.flags & DoomDef.MF_MISSILE) != 0 ? 0 : DoomDef.TELEFOGHEIGHT;
			fog = p_mobj.P_SpawnMobj(oldx, oldy, oldz + fogDelta, info.mobjtype_t.MT_TFOG);
			i_ibm.S_StartSound(fog, (int)sounds.sfxenum_t.sfx_telept);
			an = angle >> (int)DoomDef.ANGLETOFINESHIFT;
			fog = p_mobj.P_SpawnMobj(x + 20 * r_main.finecosine(an),
				y + 20 * tables.finesine[an], thing.z + fogDelta, info.mobjtype_t.MT_TFOG);
			i_ibm.S_StartSound(fog, (int)sounds.sfxenum_t.sfx_telept);
			if (thing.player != null && thing.player.powers[(int)DoomDef.powertype_t.pw_weaponlevel2] == 0)
			{ // Freeze player for about .5 sec
				thing.reactiontime = 18;
			}
			thing.angle = angle;
			if ((thing.flags2 & DoomDef.MF2_FOOTCLIP) != 0 && p_mobj.P_GetThingFloorType(thing) != p_local.FLOOR_SOLID)
			{
				thing.flags2 |= DoomDef.MF2_FEETARECLIPPED;
			}
			else if ((thing.flags2 & DoomDef.MF2_FEETARECLIPPED) != 0)
			{
				thing.flags2 &= ~DoomDef.MF2_FEETARECLIPPED;
			}
			if ((thing.flags & DoomDef.MF_MISSILE) != 0)
			{
				angle >>= (int)DoomDef.ANGLETOFINESHIFT;
				thing.momx = DoomDef.FixedMul(thing.infol.speed, r_main.finecosine(angle));
				thing.momy = DoomDef.FixedMul(thing.infol.speed, tables.finesine[angle]);
			}
			else
			{
				thing.momx = thing.momy = thing.momz = 0;
			}
			return (true);
		}

		//----------------------------------------------------------------------------
		//
		// FUNC EV_Teleport
		//
		//----------------------------------------------------------------------------

		public static bool EV_Teleport(r_local.line_t line, int side, DoomDef.mobj_t thing)
		{
			int i;
			int tag;
			DoomDef.mobj_t m;
			DoomDef.thinker_t thinker;
			r_local.sector_t sector;

			if ((thing.flags2 & DoomDef.MF2_NOTELEPORT) != 0)
			{
				return (false);
			}
			if (side == 1)
			{ // Don't teleport when crossing back side
				return (false);
			}
			tag = line.tag;
			for (i = 0; i < p_setup.numsectors; i++)
			{
				if (p_setup.sectors[i].tag == tag)
				{
					thinker = p_tick.thinkercap.next;
					for (thinker = p_tick.thinkercap.next; thinker != p_tick.thinkercap;
						thinker = thinker.next)
					{
						if (!(thinker.function is p_mobj.P_MobjThinker))
						{ // Not a mobj
							continue;
						}
						m = thinker.function.obj as DoomDef.mobj_t;
						if (m.type != info.mobjtype_t.MT_TELEPORTMAN)
						{ // Not a teleportman
							continue;
						}
						sector = m.subsector.sector;
						if (Array.IndexOf(p_setup.sectors, sector) != i)
						{ // Wrong sector
							continue;
						}
						return (P_Teleport(thing, m.x, m.y, m.angle));
					}
				}
			}
			return (false);
		}
	}
}
