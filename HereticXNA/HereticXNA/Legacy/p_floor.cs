using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HereticXNA
{
	public static class p_floor
	{
		//==================================================================
		//==================================================================
		//
		//								FLOORS
		//
		//==================================================================
		//==================================================================



		//==================================================================
		//
		//	Move a plane (floor or ceiling) and check for crushing
		//
		//==================================================================
		public static p_spec.result_e T_MovePlane(r_local.sector_t sector, int speed,
					int dest, bool crush, int floorOrCeiling, int direction)
		{
			bool flag;
			int lastpos;

			switch (floorOrCeiling)
			{
				case 0:		// FLOOR
					switch (direction)
					{
						case -1:	// DOWN
							if (sector.floorheight - speed < dest)
							{
								lastpos = sector.floorheight;
								sector.floorheight = dest;
								flag = p_map.P_ChangeSector(sector, crush);
								if (flag == true)
								{
									sector.floorheight = lastpos;
									p_map.P_ChangeSector(sector, crush);
									//return crushed;
								}
								return p_spec.result_e.pastdest;
							}
							else
							{
								lastpos = sector.floorheight;
								sector.floorheight -= speed;
								flag = p_map.P_ChangeSector(sector, crush);
								if (flag == true)
								{
									sector.floorheight = lastpos;
									p_map.P_ChangeSector(sector, crush);
									return p_spec.result_e.crushed;
								}
							}
							break;

						case 1:		// UP
							if (sector.floorheight + speed > dest)
							{
								lastpos = sector.floorheight;
								sector.floorheight = dest;
								flag = p_map.P_ChangeSector(sector, crush);
								if (flag == true)
								{
									sector.floorheight = lastpos;
									p_map.P_ChangeSector(sector, crush);
									//return crushed;
								}
								return p_spec.result_e.pastdest;
							}
							else	// COULD GET CRUSHED
							{
								lastpos = sector.floorheight;
								sector.floorheight += speed;
								flag = p_map.P_ChangeSector(sector, crush);
								if (flag == true)
								{
									if (crush == true)
										return p_spec.result_e.crushed;
									sector.floorheight = lastpos;
									p_map.P_ChangeSector(sector, crush);
									return p_spec.result_e.crushed;
								}
							}
							break;
					}
					break;

				case 1:		// CEILING
					switch (direction)
					{
						case -1:	// DOWN
							if (sector.ceilingheight - speed < dest)
							{
								lastpos = sector.ceilingheight;
								sector.ceilingheight = dest;
								flag = p_map.P_ChangeSector(sector, crush);
								if (flag == true)
								{
									sector.ceilingheight = lastpos;
									p_map.P_ChangeSector(sector, crush);
									//return crushed;
								}
								return p_spec.result_e.pastdest;
							}
							else	// COULD GET CRUSHED
							{
								lastpos = sector.ceilingheight;
								sector.ceilingheight -= speed;
								flag = p_map.P_ChangeSector(sector, crush);
								if (flag == true)
								{
									if (crush == true)
										return p_spec.result_e.crushed;
									sector.ceilingheight = lastpos;
									p_map.P_ChangeSector(sector, crush);
									return p_spec.result_e.crushed;
								}
							}
							break;

						case 1:		// UP
							if (sector.ceilingheight + speed > dest)
							{
								lastpos = sector.ceilingheight;
								sector.ceilingheight = dest;
								flag = p_map.P_ChangeSector(sector, crush);
								if (flag == true)
								{
									sector.ceilingheight = lastpos;
									p_map.P_ChangeSector(sector, crush);
									//return crushed;
								}
								return p_spec.result_e.pastdest;
							}
							else
							{
								lastpos = sector.ceilingheight;
								sector.ceilingheight += speed;
								flag = p_map.P_ChangeSector(sector, crush);
							}
							break;
					}
					break;

			}
			return p_spec.result_e.ok;
		}

		//==================================================================
		//
		//	MOVE A FLOOR TO IT'S DESTINATION (UP OR DOWN)
		//
		//==================================================================
		public class T_MoveFloor : DoomDef.think_t_delegate
		{
			public T_MoveFloor(object in_obj) : base(in_obj) { }
			public override void function(object obj)
			{
				p_spec.floormove_t floor = obj as p_spec.floormove_t;

				p_spec.result_e res;

				res = T_MovePlane(floor.sector, floor.speed,
						floor.floordestheight, floor.crush, 0, floor.direction);
				if ((p_tick.leveltime & 7) == 0)
				{
					i_ibm.S_StartSound(
						floor.sector.soundorg.x, floor.sector.soundorg.y, floor.sector.soundorg.x,
						(int)sounds.sfxenum_t.sfx_dormov);
				}

				if (res == p_spec.result_e.pastdest)
				{
					floor.sector.specialdata = null;
					if (floor.type == p_spec.floor_e.raiseBuildStep)
					{
						i_ibm.S_StartSound(
							floor.sector.soundorg.x, floor.sector.soundorg.y, floor.sector.soundorg.x,
							(int)sounds.sfxenum_t.sfx_pstop);
					}
					if (floor.direction == 1)
						switch (floor.type)
						{
							case p_spec.floor_e.donutRaise:
								floor.sector.special = (short)floor.newspecial;
								floor.sector.floorpic = floor.texture;
								break;
							default:
								break;
						}
					else if (floor.direction == -1)
						switch (floor.type)
						{
							case p_spec.floor_e.lowerAndChange:
								floor.sector.special = (short)floor.newspecial;
								floor.sector.floorpic = floor.texture;
								break;
							default:
								break;
						}
					p_tick.P_RemoveThinker(floor.thinker);
				}
			}
		}

		//==================================================================
		//
		//	HANDLE FLOOR TYPES
		//
		//==================================================================
		public static int EV_DoFloor(r_local.line_t line, p_spec.floor_e floortype)
		{
			int secnum;
			int rtn;
			int i;
			r_local.sector_t sec;
			p_spec.floormove_t floor;

			secnum = -1;
			rtn = 0;
			while ((secnum = p_spec.P_FindSectorFromLineTag(line, secnum)) >= 0)
			{
				sec = p_setup.sectors[secnum];

				//	ALREADY MOVING?  IF SO, KEEP GOING... [dsl] WHY ARE YOU YELLING?
				if (sec.specialdata != null)
					continue;

				//
				//	new floor thinker
				//
				rtn = 1;
				floor = new p_spec.floormove_t();
				p_tick.P_AddThinker(floor.thinker);
				sec.specialdata = floor;
				floor.thinker.function = new T_MoveFloor(floor);
				floor.type = floortype;
				floor.crush = false;
				switch (floortype)
				{
					case p_spec.floor_e.lowerFloor:
						floor.direction = -1;
						floor.sector = sec;
						floor.speed = p_spec.FLOORSPEED;
						floor.floordestheight =
							p_spec.P_FindHighestFloorSurrounding(sec);
						break;
					case p_spec.floor_e.lowerFloorToLowest:
						floor.direction = -1;
						floor.sector = sec;
						floor.speed = p_spec.FLOORSPEED;
						floor.floordestheight =
							p_spec.P_FindLowestFloorSurrounding(sec);
						break;
					case p_spec.floor_e.turboLower:
						floor.direction = -1;
						floor.sector = sec;
						floor.speed = p_spec.FLOORSPEED * 4;
						floor.floordestheight = (8 * DoomDef.FRACUNIT) +
								p_spec.P_FindHighestFloorSurrounding(sec);
						break;
					case p_spec.floor_e.raiseFloorCrush:
						floor.crush = true;
						floor.direction = 1;
						floor.sector = sec;
						floor.speed = p_spec.FLOORSPEED;
						floor.floordestheight =
							p_spec.P_FindLowestCeilingSurrounding(sec);
						if (floor.floordestheight > sec.ceilingheight)
							floor.floordestheight = sec.ceilingheight;
						floor.floordestheight -= (8 * DoomDef.FRACUNIT) *
							((floortype == p_spec.floor_e.raiseFloorCrush) ? 1 : 0);
						break;
					case p_spec.floor_e.raiseFloor:
						floor.direction = 1;
						floor.sector = sec;
						floor.speed = p_spec.FLOORSPEED;
						floor.floordestheight =
							p_spec.P_FindLowestCeilingSurrounding(sec);
						if (floor.floordestheight > sec.ceilingheight)
							floor.floordestheight = sec.ceilingheight;
						floor.floordestheight -= (8 * DoomDef.FRACUNIT) *
							((floortype == p_spec.floor_e.raiseFloorCrush) ? 1 : 0);
						break;
					case p_spec.floor_e.raiseFloorToNearest:
						floor.direction = 1;
						floor.sector = sec;
						floor.speed = p_spec.FLOORSPEED;
						floor.floordestheight =
							p_spec.P_FindNextHighestFloor(sec, sec.floorheight);
						break;
					case p_spec.floor_e.raiseFloor24:
						floor.direction = 1;
						floor.sector = sec;
						floor.speed = p_spec.FLOORSPEED;
						floor.floordestheight = floor.sector.floorheight +
								24 * DoomDef.FRACUNIT;
						break;
					case p_spec.floor_e.raiseFloor24AndChange:
						floor.direction = 1;
						floor.sector = sec;
						floor.speed = p_spec.FLOORSPEED;
						floor.floordestheight = floor.sector.floorheight +
								24 * DoomDef.FRACUNIT;
						sec.floorpic = line.frontsector.floorpic;
						sec.special = line.frontsector.special;
						break;
					case p_spec.floor_e.raiseToTexture:
						{
							int minsize = DoomDef.MAXINT;
							r_local.side_t side;

							floor.direction = 1;
							floor.sector = sec;
							floor.speed = p_spec.FLOORSPEED;
							for (i = 0; i < sec.linecount; i++)
								if (p_spec.twoSided(secnum, i) != 0)
								{
									side = p_spec.getSide(secnum, i, 0);
									if (side.bottomtexture >= 0)
										if (r_data.textureheight[side.bottomtexture] <
											minsize)
											minsize =
												r_data.textureheight[side.bottomtexture];
									side = p_spec.getSide(secnum, i, 1);
									if (side.bottomtexture >= 0)
										if (r_data.textureheight[side.bottomtexture] <
											minsize)
											minsize =
												r_data.textureheight[side.bottomtexture];
								}
							floor.floordestheight = floor.sector.floorheight +
								minsize;
						}
						break;
					case p_spec.floor_e.lowerAndChange:
						floor.direction = -1;
						floor.sector = sec;
						floor.speed = p_spec.FLOORSPEED;
						floor.floordestheight =
							p_spec.P_FindLowestFloorSurrounding(sec);
						floor.texture = sec.floorpic;
						for (i = 0; i < sec.linecount; i++)
							if (p_spec.twoSided(secnum, i) != 0)
							{
								if (Array.IndexOf(p_setup.sectors, p_spec.getSide(secnum, i, 0).sector) == secnum)
								{
									sec = p_spec.getSector(secnum, i, 1);
									floor.texture = sec.floorpic;
									floor.newspecial = sec.special;
									break;
								}
								else
								{
									sec = p_spec.getSector(secnum, i, 0);
									floor.texture = sec.floorpic;
									floor.newspecial = sec.special;
									break;
								}
							}
						break;
					default:
						break;
				}
			}
			return rtn;
		}

		//==================================================================
		//
		//	BUILD A STAIRCASE!
		//
		//==================================================================
		public static int EV_BuildStairs(r_local.line_t line, int stepDelta)
		{
			int secnum;
			int height;
			int i;
			int newsecnum;
			int texture;
			int ok;
			int rtn;
			r_local.sector_t sec, tsec;
			p_spec.floormove_t floor;

			secnum = -1;
			rtn = 0;
			while ((secnum = p_spec.P_FindSectorFromLineTag(line, secnum)) >= 0)
			{
				sec = p_setup.sectors[secnum];

				// ALREADY MOVING?  IF SO, KEEP GOING...
				if (sec.specialdata != null)
					continue;

				//
				// new floor thinker
				//
				rtn = 1;
				height = sec.floorheight + stepDelta;
				floor = new p_spec.floormove_t();
				p_tick.P_AddThinker(floor.thinker);
				sec.specialdata = floor;
				floor.thinker.function = new T_MoveFloor(floor);
				floor.type = p_spec.floor_e.raiseBuildStep;
				floor.direction = 1;
				floor.sector = sec;
				floor.speed = p_spec.FLOORSPEED;
				floor.floordestheight = height;

				texture = sec.floorpic;

				//
				// Find next sector to raise
				// 1.	Find 2-sided line with same sector side[0]
				// 2.	Other side is the next sector to raise
				//
				do
				{
					ok = 0;
					for (i = 0; i < sec.linecount; i++)
					{
						if (((p_setup.linebuffer[sec.linesi + i]).flags & DoomData.ML_TWOSIDED) == 0)
							continue;

						tsec = (p_setup.linebuffer[sec.linesi + i]).frontsector;
						newsecnum = Array.IndexOf(p_setup.sectors, tsec);
						if (secnum != newsecnum)
							continue;
						tsec = (p_setup.linebuffer[sec.linesi + i]).backsector;
						newsecnum = Array.IndexOf(p_setup.sectors, tsec);
						if (tsec.floorpic != texture)
							continue;

						height += stepDelta;
						if (tsec.specialdata != null)
							continue;

						sec = tsec;
						secnum = newsecnum;
						floor = new p_spec.floormove_t();
						p_tick.P_AddThinker(floor.thinker);
						sec.specialdata = floor;
						floor.thinker.function = new T_MoveFloor(floor);
						floor.type = p_spec.floor_e.raiseBuildStep;
						floor.direction = 1;
						floor.sector = sec;
						floor.speed = p_spec.FLOORSPEED;
						floor.floordestheight = height;
						ok = 1;
						break;
					}
				} while (ok != 0);
			}
			return (rtn);
		}
	}
}
