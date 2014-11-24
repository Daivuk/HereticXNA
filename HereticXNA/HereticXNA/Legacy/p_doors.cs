using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

		
// P_doors.c

namespace HereticXNA
{
	public static class p_doors
	{
//==================================================================
//==================================================================
//
//							VERTICAL DOORS
//
//==================================================================
//==================================================================

//==================================================================
//
//	T_VerticalDoor
//
//==================================================================
		public class T_VerticalDoor : DoomDef.think_t_delegate
		{
			public T_VerticalDoor(object in_obj) : base(in_obj) {}
			public override void function(object obj)
			{
				p_spec.vldoor_t door = obj as p_spec.vldoor_t;
				p_spec.result_e res;

				switch (door.direction)
				{
					case 0: // WAITING
						if ((--door.topcountdown) == 0)
							switch (door.type)
							{
								case p_spec.vldoor_e.normal:
									door.direction = -1; // time to go back down
									i_ibm.S_StartSound(
										door.sector.soundorg.x,
										door.sector.soundorg.y,
										door.sector.soundorg.z, (int)sounds.sfxenum_t.sfx_doropn);
									break;
								case p_spec.vldoor_e.close30ThenOpen:
									door.direction = 1;
									i_ibm.S_StartSound(
										door.sector.soundorg.x,
										door.sector.soundorg.y,
										door.sector.soundorg.z, (int)sounds.sfxenum_t.sfx_doropn);
									break;
								default:
									break;
							}
						break;
					case 2: // INITIAL WAIT
						if ((--door.topcountdown) == 0)
						{
							switch (door.type)
							{
								case p_spec.vldoor_e.raiseIn5Mins:
									door.direction = 1;
									door.type = p_spec.vldoor_e.normal;
									i_ibm.S_StartSound(
										door.sector.soundorg.x,
										door.sector.soundorg.y,
										door.sector.soundorg.z, (int)sounds.sfxenum_t.sfx_doropn);
									break;
								default:
									break;
							}
						}
						break;
					case -1: // DOWN
						res = p_floor.T_MovePlane(door.sector, door.speed,
							door.sector.floorheight, false, 1, door.direction);
						if (res == p_spec.result_e.pastdest)
						{
							switch (door.type)
							{
								case p_spec.vldoor_e.normal:
								case p_spec.vldoor_e.close:
									door.sector.specialdata = null;
									p_tick.P_RemoveThinker(door.thinker);  // unlink and free
									i_ibm.S_StartSound(
										door.sector.soundorg.x,
										door.sector.soundorg.y,
										door.sector.soundorg.z, (int)sounds.sfxenum_t.sfx_dorcls);
									break;
								case p_spec.vldoor_e.close30ThenOpen:
									door.direction = 0;
									door.topcountdown = 35 * 30;
									break;
								default:
									break;
							}
						}
						else if (res == p_spec.result_e.crushed)
						{
							switch (door.type)
							{
								case p_spec.vldoor_e.close: // DON'T GO BACK UP!
									break;
								default:
									door.direction = 1;
									i_ibm.S_StartSound(
										door.sector.soundorg.x,
										door.sector.soundorg.y,
										door.sector.soundorg.z, (int)sounds.sfxenum_t.sfx_doropn);
									break;
							}
						}
						break;
					case 1: // UP
						res = p_floor.T_MovePlane(door.sector, door.speed,
							door.topheight, false, 1, door.direction);
						if (res == p_spec.result_e.pastdest)
						{
							switch (door.type)
							{
								case p_spec.vldoor_e.normal:
									door.direction = 0; // wait at top
									door.topcountdown = door.topwait;
									break;
								case p_spec.vldoor_e.close30ThenOpen:
								case p_spec.vldoor_e.open:
									door.sector.specialdata = null;
									p_tick.P_RemoveThinker(door.thinker); // unlink and free
									//S_StopSound((mobj_t*)&door.sector.soundorg);
									break;
								default:
									break;
							}
						}
						break;
				}
			}
		}


//----------------------------------------------------------------------------
//
// EV_DoDoor
//
// Move a door up/down
//
//----------------------------------------------------------------------------

public static int EV_DoDoor(r_local.line_t line, p_spec.vldoor_e type, int speed)
{
	int secnum;
	int retcode;
	r_local.sector_t sec;
	p_spec.vldoor_t door;

	secnum = -1;
	retcode = 0;
	while((secnum = p_spec.P_FindSectorFromLineTag(line, secnum)) >= 0)
	{
		sec = p_setup.sectors[secnum];
		if(sec.specialdata != null)
		{
			continue;
		}
		// Add new door thinker
		retcode = 1;
		door = new p_spec.vldoor_t();
		p_tick.P_AddThinker(door.thinker);
		sec.specialdata = door;
		door.thinker.function = new T_VerticalDoor(door);
		door.sector = sec;
		switch(type)
		{
			case p_spec.vldoor_e.close:
				door.topheight = p_spec.P_FindLowestCeilingSurrounding(sec);
				door.topheight -= 4*DoomDef.FRACUNIT;
				door.direction = -1;
				i_ibm.S_StartSound(
					door.sector.soundorg.x,
					door.sector.soundorg.y,
					door.sector.soundorg.z, 
					(int)sounds.sfxenum_t.sfx_doropn);
				break;
			case p_spec.vldoor_e.close30ThenOpen:
				door.topheight = sec.ceilingheight;
				door.direction = -1;
				i_ibm.S_StartSound(
					door.sector.soundorg.x,
					door.sector.soundorg.y,
					door.sector.soundorg.z,
					(int)sounds.sfxenum_t.sfx_doropn);
				break;
			case p_spec.vldoor_e.normal:
			case p_spec.vldoor_e.open:
				door.direction = 1;
				door.topheight = p_spec.P_FindLowestCeilingSurrounding(sec);
				door.topheight -= 4*DoomDef.FRACUNIT;
				if(door.topheight != sec.ceilingheight)
				{
					i_ibm.S_StartSound(
						door.sector.soundorg.x,
						door.sector.soundorg.y,
						door.sector.soundorg.z,
						(int)sounds.sfxenum_t.sfx_doropn);
				}
				break;
			default:
				break;
		}
		door.type = type;
		door.speed = speed;
		door.topwait = p_spec.VDOORWAIT;
	}
	return(retcode);
}


//==================================================================
//
//	EV_VerticalDoor : open a door manually, no tag value
//
//==================================================================
public static void EV_VerticalDoor(r_local.line_t line, DoomDef.mobj_t thing)
{
	DoomDef.player_t		player;
	int				secnum;
	r_local.sector_t		sec;
	p_spec.vldoor_t		door;
	int				side;
	
	side = 0; // only front sides can be used
//
//	Check for locks
//
	player = thing.player;
	switch(line.special)
	{
		case 26: // Blue Lock
		case 32:
			if(player == null)
			{
				return;
			}
			if(!player.keys[(int)DoomDef.keytype_t.key_blue])
			{
				p_inter.P_SetMessage(player, dstring.TXT_NEEDBLUEKEY, false);
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_plroof);
				return;
			}
			break;
		case 27: // Yellow Lock
		case 34:
			if(player == null)
			{
				return;
			}
			if(!player.keys[(int)DoomDef.keytype_t.key_yellow])
			{
				p_inter.P_SetMessage(player, dstring.TXT_NEEDYELLOWKEY, false);
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_plroof);
				return;
			}
			break;
		case 28: // Green Lock
		case 33:
			if(player == null)
			{
				return;
			}
			if(!player.keys[(int)DoomDef.keytype_t.key_green])
			{
				p_inter.P_SetMessage(player, dstring.TXT_NEEDGREENKEY, false);
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_plroof);
				return;
			}
			break;
	}
	// if the sector has an active thinker, use it
	sec = p_setup.sides[line.sidenum[side^1]].sector;
	secnum = Array.IndexOf(p_setup.sectors, sec);
	if(sec.specialdata != null)
	{
		door = sec.specialdata as p_spec.vldoor_t;
		switch(line.special)
		{
			case 1: // ONLY FOR "RAISE" DOORS, NOT "OPEN"s
			case 26:
			case 27:
			case 28:
				if(door.direction == -1)
				{
					door.direction = 1; // go back up
				}
				else
				{
					if(thing.player == null)
					{ // Monsters don't close doors
						return;
					}
					door.direction = -1; // start going down immediately
				}
				return;
		}
	}

	// for proper sound
	switch(line.special)
	{
		case 1: // NORMAL DOOR SOUND
		case 31:
			i_ibm.S_StartSound(
				sec.soundorg.x,
				sec.soundorg.y,
				sec.soundorg.z,
				(int)sounds.sfxenum_t.sfx_doropn);
			break;
		default: // LOCKED DOOR SOUND
			i_ibm.S_StartSound(
				sec.soundorg.x,
				sec.soundorg.y,
				sec.soundorg.z,
				(int)sounds.sfxenum_t.sfx_doropn);
			break;
	}

	//
	// new door thinker
	//
	door = new p_spec.vldoor_t();
	p_tick.P_AddThinker(door.thinker);
	sec.specialdata = door;
	door.thinker.function = new T_VerticalDoor(door);
	door.sector = sec;
	door.direction = 1;
	switch(line.special)
	{
		case 1:
		case 26:
		case 27:
		case 28:
			door.type = p_spec.vldoor_e.normal;
			break;
		case 31:
		case 32:
		case 33:
		case 34:
			door.type = p_spec.vldoor_e.open;
			line.special = 0;
			break;
	}
	door.speed = p_spec.VDOORSPEED;
	door.topwait = p_spec.VDOORWAIT;
	
	//
	// find the top and bottom of the movement range
	//
	door.topheight = p_spec.P_FindLowestCeilingSurrounding(sec);
	door.topheight -= 4*DoomDef.FRACUNIT;
}

#if DOS

//==================================================================
//
//	Spawn a door that closes after 30 seconds
//
//==================================================================
void P_SpawnDoorCloseIn30(sector_t *sec)
{
	vldoor_t *door;

	door = Z_Malloc(sizeof(*door), PU_LEVSPEC, 0);
	P_AddThinker(&door.thinker);
	sec.specialdata = door;
	sec.special = 0;
	door.thinker.function = T_VerticalDoor;
	door.sector = sec;
	door.direction = 0;
	door.type = normal;
	door.speed = VDOORSPEED;
	door.topcountdown = 30*35;
}

//==================================================================
//
//	Spawn a door that opens after 5 minutes
//
//==================================================================
void P_SpawnDoorRaiseIn5Mins(sector_t *sec, int secnum)
{
	vldoor_t *door;

	door = Z_Malloc(sizeof(*door), PU_LEVSPEC, 0);
	P_AddThinker(&door.thinker);
	sec.specialdata = door;
	sec.special = 0;
	door.thinker.function = T_VerticalDoor;
	door.sector = sec;
	door.direction = 2;
	door.type = raiseIn5Mins;
	door.speed = VDOORSPEED;
	door.topheight = P_FindLowestCeilingSurrounding(sec);
	door.topheight -= 4*FRACUNIT;
	door.topwait = VDOORWAIT;
	door.topcountdown = 5*60*35;
}

#endif
	}
}
