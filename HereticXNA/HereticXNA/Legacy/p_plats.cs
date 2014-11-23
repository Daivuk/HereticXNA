using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
		
// P_plats.c

namespace HereticXNA
{
	class p_plats
	{
		public static p_spec.plat_t[] activeplats = new p_spec.plat_t[p_spec.MAXPLATS];

		//==================================================================
		//
		//	Move a plat up and down
		//
		//==================================================================
		public class T_PlatRaise : DoomDef.think_t_delegate
		{
			public T_PlatRaise(object in_obj) : base(in_obj) { }
			public override void function(object obj)
			{
				p_spec.plat_t plat = obj as p_spec.plat_t;
				p_spec.result_e res;

				switch (plat.status)
				{
					case p_spec.plat_e.up:
						res = p_floor.T_MovePlane(plat.sector, plat.speed,
								plat.high, plat.crush, 0, 1);
						if ((p_tick.leveltime & 31) == 0)
						{
							i_ibm.S_StartSound(
								plat.sector.soundorg.x,
								plat.sector.soundorg.y,
								plat.sector.soundorg.z,
								(int)sounds.sfxenum_t.sfx_stnmov);
						}
						if (plat.type == p_spec.plattype_e.raiseAndChange
							|| plat.type == p_spec.plattype_e.raiseToNearestAndChange)
						{
							if ((p_tick.leveltime & 7) == 0)
							{
								i_ibm.S_StartSound(
									plat.sector.soundorg.x,
									plat.sector.soundorg.y,
									plat.sector.soundorg.z,
									(int)sounds.sfxenum_t.sfx_stnmov);
							}
						}
						if (res == p_spec.result_e.crushed && (!plat.crush))
						{
							plat.count = plat.wait;
							plat.status = p_spec.plat_e.down;
							i_ibm.S_StartSound(
								plat.sector.soundorg.x,
								plat.sector.soundorg.y,
								plat.sector.soundorg.z,
								(int)sounds.sfxenum_t.sfx_pstart);
						}
						else
							if (res == p_spec.result_e.pastdest)
							{
								plat.count = plat.wait;
								plat.status = p_spec.plat_e.waiting;
								i_ibm.S_StartSound(
									plat.sector.soundorg.x,
									plat.sector.soundorg.y,
									plat.sector.soundorg.z,
									(int)sounds.sfxenum_t.sfx_pstop);
								switch (plat.type)
								{
									case p_spec.plattype_e.downWaitUpStay:
										P_RemoveActivePlat(plat);
										break;
									case p_spec.plattype_e.raiseAndChange:
										P_RemoveActivePlat(plat);
										break;
									default:
										break;
								}
							}
						break;
					case p_spec.plat_e.down:
						res = p_floor.T_MovePlane(plat.sector, plat.speed, plat.low, false, 0, -1);
						if (res == p_spec.result_e.pastdest)
						{
							plat.count = plat.wait;
							plat.status = p_spec.plat_e.waiting;
							i_ibm.S_StartSound(
								plat.sector.soundorg.x,
								plat.sector.soundorg.y,
								plat.sector.soundorg.z,
								(int)sounds.sfxenum_t.sfx_pstop);
						}
						else
						{
							if ((p_tick.leveltime & 31) == 0)
							{
								i_ibm.S_StartSound(
									plat.sector.soundorg.x,
									plat.sector.soundorg.y,
									plat.sector.soundorg.z,
									(int)sounds.sfxenum_t.sfx_stnmov);
							}
						}
						break;
					case p_spec.plat_e.waiting:
						if ((--plat.count) == 0)
						{
							if (plat.sector.floorheight == plat.low)
								plat.status = p_spec.plat_e.up;
							else
								plat.status = p_spec.plat_e.down;
							i_ibm.S_StartSound(
								plat.sector.soundorg.x,
								plat.sector.soundorg.y,
								plat.sector.soundorg.z,
								(int)sounds.sfxenum_t.sfx_pstart);
						}
						break;
					case p_spec.plat_e.in_stasis:
						break;
				}
			}
		}

		//==================================================================
		//
		//	Do Platforms
		//	"amount" is only used for SOME platforms.
		//
		//==================================================================
		public static int EV_DoPlat(r_local.line_t line, p_spec.plattype_e type, int amount)
		{
			p_spec.plat_t plat;
			int secnum;
			int rtn;
			r_local.sector_t sec;

			secnum = -1;
			rtn = 0;

			//
			//	Activate all <type> plats that are in_stasis
			//
			switch (type)
			{
				case p_spec.plattype_e.perpetualRaise:
					P_ActivateInStasis(line.tag);
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
				// Find lowest & highest floors around sector
				//
				rtn = 1;
				plat = new p_spec.plat_t();
				p_tick.P_AddThinker(plat.thinker);

				plat.type = type;
				plat.sector = sec;
				plat.sector.specialdata = plat;
				plat.thinker.function = new T_PlatRaise(plat);
				plat.crush = false;
				plat.tag = line.tag;
				switch (type)
				{
					case p_spec.plattype_e.raiseToNearestAndChange:
						plat.speed = p_spec.PLATSPEED / 2;
						sec.floorpic = p_setup.sides[line.sidenum[0]].sector.floorpic;
						plat.high = p_spec.P_FindNextHighestFloor(sec, sec.floorheight);
						plat.wait = 0;
						plat.status = p_spec.plat_e.up;
						sec.special = 0;		// NO MORE DAMAGE, IF APPLICABLE
						i_ibm.S_StartSound(sec.soundorg.x, sec.soundorg.y, sec.soundorg.z, (int)sounds.sfxenum_t.sfx_stnmov);
						break;
					case p_spec.plattype_e.raiseAndChange:
						plat.speed = p_spec.PLATSPEED / 2;
						sec.floorpic = p_setup.sides[line.sidenum[0]].sector.floorpic;
						plat.high = sec.floorheight + amount * DoomDef.FRACUNIT;
						plat.wait = 0;
						plat.status = p_spec.plat_e.up;
						i_ibm.S_StartSound(sec.soundorg.x, sec.soundorg.y, sec.soundorg.z, (int)sounds.sfxenum_t.sfx_stnmov);
						break;
					case p_spec.plattype_e.downWaitUpStay:
						plat.speed = p_spec.PLATSPEED * 4;
						plat.low = p_spec.P_FindLowestFloorSurrounding(sec);
						if (plat.low > sec.floorheight)
							plat.low = sec.floorheight;
						plat.high = sec.floorheight;
						plat.wait = 35 * p_spec.PLATWAIT;
						plat.status = p_spec.plat_e.down;
						i_ibm.S_StartSound(sec.soundorg.x, sec.soundorg.y, sec.soundorg.z, (int)sounds.sfxenum_t.sfx_pstart);
						break;
					case p_spec.plattype_e.perpetualRaise:
						plat.speed = p_spec.PLATSPEED;
						plat.low = p_spec.P_FindLowestFloorSurrounding(sec);
						if (plat.low > sec.floorheight)
							plat.low = sec.floorheight;
						plat.high = p_spec.P_FindHighestFloorSurrounding(sec);
						if (plat.high < sec.floorheight)
							plat.high = sec.floorheight;
						plat.wait = 35 * p_spec.PLATWAIT;
						plat.status = (p_spec.plat_e)(m_misc.P_Random() & 1);
						i_ibm.S_StartSound(sec.soundorg.x, sec.soundorg.y, sec.soundorg.z, (int)sounds.sfxenum_t.sfx_pstart);
						break;
				}
				P_AddActivePlat(plat);
			}
			return rtn;
		}

		public static void P_ActivateInStasis(int tag)
		{
			int i;

			for (i = 0; i < p_spec.MAXPLATS; i++)
				if (activeplats[i] != null &&
					(activeplats[i]).tag == tag &&
					(activeplats[i]).status == p_spec.plat_e.in_stasis)
				{
					(activeplats[i]).status = (activeplats[i]).oldstatus;
					(activeplats[i]).thinker.function = new T_PlatRaise(activeplats[i]);
				}
		}

		public static void EV_StopPlat(r_local.line_t line)
		{
			int j;

			for (j = 0; j < p_spec.MAXPLATS; j++)
				if (activeplats[j] != null && ((activeplats[j]).status != p_spec.plat_e.in_stasis) &&
					((activeplats[j]).tag == line.tag))
				{
					(activeplats[j]).oldstatus = (activeplats[j]).status;
					(activeplats[j]).status = p_spec.plat_e.in_stasis;
					(activeplats[j]).thinker.function = null;
				}
		}

		public static void P_AddActivePlat(p_spec.plat_t plat)
		{
			int i;
			for (i = 0; i < p_spec.MAXPLATS; i++)
				if (activeplats[i] == null)
				{
					activeplats[i] = plat;
					return;
				}
			i_ibm.I_Error("P_AddActivePlat: no more plats!");
		}

		public static void P_RemoveActivePlat(p_spec.plat_t plat)
		{
			int i;
			for (i = 0; i < p_spec.MAXPLATS; i++)
				if (plat == activeplats[i])
				{
					(activeplats[i]).sector.specialdata = null;
					p_tick.P_RemoveThinker((activeplats[i]).thinker);
					activeplats[i] = null;
					return;
				}
			i_ibm.I_Error("P_RemoveActivePlat: can't find plat!");
		}
	}
}
