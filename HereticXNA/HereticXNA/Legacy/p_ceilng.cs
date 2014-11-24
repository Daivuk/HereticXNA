using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//==================================================================
// Port - Done
//==================================================================

namespace HereticXNA
{
	public static class p_ceilng
	{
		//==================================================================
		//==================================================================
		//
		//							CEILINGS
		//
		//==================================================================
		//==================================================================

		public static p_spec.ceiling_t[] activeceilings = new p_spec.ceiling_t[p_spec.MAXCEILINGS];

		//==================================================================
		//
		//	T_MoveCeiling
		//
		//==================================================================

		public class T_MoveCeiling : DoomDef.think_t_delegate
		{
			public T_MoveCeiling(object in_obj) : base(in_obj) { }

			public override void function(object obj)
			{
				p_spec.ceiling_t ceiling = obj as p_spec.ceiling_t;
				p_spec.result_e res;

				switch (ceiling.direction)
				{
					case 0:		// IN STASIS
						break;
					case 1:		// UP
						res = p_floor.T_MovePlane(ceiling.sector, ceiling.speed,
								ceiling.topheight, false, 1, ceiling.direction);
						if ((p_tick.leveltime & 7) == 0)
							i_ibm.S_StartSound(
								ceiling.sector.soundorg.x,
								ceiling.sector.soundorg.y,
								ceiling.sector.soundorg.x,
								(int)sounds.sfxenum_t.sfx_dormov);

						if (res == p_spec.result_e.pastdest)
							switch (ceiling.type)
							{
								case p_spec.ceiling_e.raiseToHighest:
									P_RemoveActiveCeiling(ceiling);
									break;
								case p_spec.ceiling_e.fastCrushAndRaise:
								case p_spec.ceiling_e.crushAndRaise:
									ceiling.direction = -1;
									break;
								default:
									break;
							}
						break;
					case -1:	// DOWN
						res = p_floor.T_MovePlane(ceiling.sector, ceiling.speed,
							ceiling.bottomheight, ceiling.crush, 1, ceiling.direction);
						if ((p_tick.leveltime & 7) == 0)
							i_ibm.S_StartSound(
								ceiling.sector.soundorg.x,
								ceiling.sector.soundorg.y,
								ceiling.sector.soundorg.x,
								(int)sounds.sfxenum_t.sfx_dormov);
						if (res == p_spec.result_e.pastdest)
							switch (ceiling.type)
							{
								case p_spec.ceiling_e.crushAndRaise:
									ceiling.speed = p_spec.CEILSPEED;
									ceiling.direction = 1;
									break;
								case p_spec.ceiling_e.fastCrushAndRaise:
									ceiling.direction = 1;
									break;
								case p_spec.ceiling_e.lowerAndCrush:
								case p_spec.ceiling_e.lowerToFloor:
									P_RemoveActiveCeiling(ceiling);
									break;
								default:
									break;
							}
						else
							if (res == p_spec.result_e.crushed)
								switch (ceiling.type)
								{
									case p_spec.ceiling_e.crushAndRaise:
									case p_spec.ceiling_e.lowerAndCrush:
										ceiling.speed = p_spec.CEILSPEED / 8;
										break;
									default:
										break;
								}
						break;
				}
			}
		}

		//==================================================================
		//
		//		EV_DoCeiling
		//		Move a ceiling up/down and all around!
		//
		//==================================================================
		public static int EV_DoCeiling(r_local.line_t line, p_spec.ceiling_e type)
		{
			int secnum, rtn;
			r_local.sector_t sec;
			p_spec.ceiling_t ceiling;

			secnum = -1;
			rtn = 0;

			//
			//	Reactivate in-stasis ceilings...for certain types.
			//
			switch (type)
			{
				case p_spec.ceiling_e.fastCrushAndRaise:
				case p_spec.ceiling_e.crushAndRaise:
					P_ActivateInStasisCeiling(line);
					break;
				default:
					break;
			}

			while ((secnum = p_spec.P_FindSectorFromLineTag(line, secnum)) >= 0)
			{
				sec = p_setup.sectors[secnum];
				if (sec.specialdata != null)
					continue;

				//
				// new door thinker
				//
				rtn = 1;
				ceiling = new p_spec.ceiling_t();
				p_tick.P_AddThinker(ceiling.thinker);
				sec.specialdata = ceiling;
				ceiling.thinker.function = new T_MoveCeiling(ceiling);
				ceiling.sector = sec;
				ceiling.crush = false;
				switch (type)
				{
					case p_spec.ceiling_e.fastCrushAndRaise:
						ceiling.crush = true;
						ceiling.topheight = sec.ceilingheight;
						ceiling.bottomheight = sec.floorheight + (8 * DoomDef.FRACUNIT);
						ceiling.direction = -1;
						ceiling.speed = p_spec.CEILSPEED * 2;
						break;
					case p_spec.ceiling_e.crushAndRaise:
						ceiling.crush = true;
						ceiling.topheight = sec.ceilingheight;
						ceiling.bottomheight = sec.floorheight;
						if (type != p_spec.ceiling_e.lowerToFloor)
							ceiling.bottomheight += 8 * DoomDef.FRACUNIT;
						ceiling.direction = -1;
						ceiling.speed = p_spec.CEILSPEED;
						break;
					case p_spec.ceiling_e.lowerAndCrush:
					case p_spec.ceiling_e.lowerToFloor:
						ceiling.bottomheight = sec.floorheight;
						if (type != p_spec.ceiling_e.lowerToFloor)
							ceiling.bottomheight += 8 * DoomDef.FRACUNIT;
						ceiling.direction = -1;
						ceiling.speed = p_spec.CEILSPEED;
						break;
					case p_spec.ceiling_e.raiseToHighest:
						ceiling.topheight = p_spec.P_FindHighestCeilingSurrounding(sec);
						ceiling.direction = 1;
						ceiling.speed = p_spec.CEILSPEED;
						break;
				}

				ceiling.tag = sec.tag;
				ceiling.type = type;
				P_AddActiveCeiling(ceiling);
			}
			return rtn;
		}


		//==================================================================
		//
		//		Add an active ceiling
		//
		//==================================================================
		public static void P_AddActiveCeiling(p_spec.ceiling_t c)
		{
			int i;
			for (i = 0; i < p_spec.MAXCEILINGS; i++)
				if (activeceilings[i] == null)
				{
					activeceilings[i] = c;
					return;
				}
		}

		//==================================================================
		//
		//		Remove a ceiling's thinker
		//
		//==================================================================
		public static void P_RemoveActiveCeiling(p_spec.ceiling_t c)
		{
			int i;

			for (i = 0; i < p_spec.MAXCEILINGS; i++)
				if (activeceilings[i] == c)
				{
					activeceilings[i].sector.specialdata = null;
					p_tick.P_RemoveThinker(activeceilings[i].thinker);
					activeceilings[i] = null;
					break;
				}
		}

		//==================================================================
		//
		//		Restart a ceiling that's in-stasis
		//
		//==================================================================
		public static void P_ActivateInStasisCeiling(r_local.line_t line)
		{
			int i;

			for (i = 0; i < p_spec.MAXCEILINGS; i++)
				if (activeceilings[i] != null && (activeceilings[i].tag == line.tag) &&
					(activeceilings[i].direction == 0))
				{
					activeceilings[i].direction = activeceilings[i].olddirection;
					activeceilings[i].thinker.function = new T_MoveCeiling(activeceilings[i]);
				}
		}

		//==================================================================
		//
		//		EV_CeilingCrushStop
		//		Stop a ceiling from crushing!
		//
		//==================================================================
		public static int EV_CeilingCrushStop(r_local.line_t line)
		{
			int i;
			int rtn;

			rtn = 0;
			for (i = 0; i < p_spec.MAXCEILINGS; i++)
				if (activeceilings[i] != null && (activeceilings[i].tag == line.tag) &&
					(activeceilings[i].direction != 0))
				{
					activeceilings[i].olddirection = activeceilings[i].direction;
					activeceilings[i].thinker.function = null;
					activeceilings[i].direction = 0;		// in-stasis
					rtn = 1;
				}

			return rtn;
		}
	}
}
