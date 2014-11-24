using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//==================================================================
// Port - Done
//==================================================================


namespace HereticXNA
{
	public static class p_lights
	{

		//==================================================================
		//==================================================================
		//
		//							BROKEN LIGHT FLASHING
		//
		//==================================================================
		//==================================================================

		//==================================================================
		//
		//	T_LightFlash
		//
		//	After the map has been loaded, scan each sector for specials
		//	that spawn thinkers
		//
		//==================================================================
		public class T_LightFlash : DoomDef.think_t_delegate
		{
			public T_LightFlash(object in_obj) : base(in_obj) { }
			public override void function(object obj)
			{
				p_spec.lightflash_t flash = obj as p_spec.lightflash_t;
				if (--flash.count != 0)
					return;

				if (flash.sector.lightlevel == flash.maxlight)
				{
					flash.sector.lightlevel = (short)flash.minlight;
					flash.count = (m_misc.P_Random() & flash.mintime) + 1;
				}
				else
				{
					flash.sector.lightlevel = (short)flash.maxlight;
					flash.count = (m_misc.P_Random() & flash.maxtime) + 1;
				}
			}
		}


		//==================================================================
		//
		//	P_SpawnLightFlash
		//
		//	After the map has been loaded, scan each sector for specials that spawn thinkers
		//
		//==================================================================
		public static void P_SpawnLightFlash(r_local.sector_t sector)
		{
			p_spec.lightflash_t flash;

			sector.special = 0;		// nothing special about it during gameplay

			flash = new p_spec.lightflash_t();
			p_tick.P_AddThinker(flash.thinker);
			flash.thinker.function = new T_LightFlash(flash);
			flash.sector = sector;
			flash.maxlight = sector.lightlevel;

			flash.minlight = p_spec.P_FindMinSurroundingLight(sector, sector.lightlevel);
			flash.maxtime = 64;
			flash.mintime = 7;
			flash.count = (m_misc.P_Random() & flash.maxtime) + 1;
		}

		//==================================================================
		//
		//							STROBE LIGHT FLASHING
		//
		//==================================================================

		//==================================================================
		//
		//	T_StrobeFlash
		//
		//	After the map has been loaded, scan each sector for specials that spawn thinkers
		//
		//==================================================================
		public class T_StrobeFlash : DoomDef.think_t_delegate
		{
			public T_StrobeFlash(object in_obj) : base(in_obj) { }
			public override void function(object obj)
			{
				p_spec.strobe_t flash = obj as p_spec.strobe_t;
				if (--flash.count != 0)
					return;

				if (flash.sector.lightlevel == flash.minlight)
				{
					flash.sector.lightlevel = (short)flash.maxlight;
					flash.count = flash.brighttime;
				}
				else
				{
					flash.sector.lightlevel = (short)flash.minlight;
					flash.count = flash.darktime;
				}
			}
		}


		//==================================================================
		//
		//	P_SpawnLightFlash
		//
		//	After the map has been loaded, scan each sector for specials that spawn thinkers
		//
		//==================================================================
		public static void P_SpawnStrobeFlash(r_local.sector_t sector, int fastOrSlow, int inSync)
		{
			p_spec.strobe_t flash;

			flash = new p_spec.strobe_t();
			p_tick.P_AddThinker(flash.thinker);
			flash.sector = sector;
			flash.darktime = fastOrSlow;
			flash.brighttime = p_spec.STROBEBRIGHT;
			flash.thinker.function = new T_StrobeFlash(flash);
			flash.maxlight = sector.lightlevel;
			flash.minlight = p_spec.P_FindMinSurroundingLight(sector, sector.lightlevel);

			if (flash.minlight == flash.maxlight)
				flash.minlight = 0;
			sector.special = 0;		// nothing special about it during gameplay

			if (inSync == 0)
				flash.count = (m_misc.P_Random() & 7) + 1;
			else
				flash.count = 1;
		}

		//==================================================================
		//
		//	Start strobing lights (usually from a trigger)
		//
		//==================================================================
		public static void EV_StartLightStrobing(r_local.line_t line)
		{
			int secnum;
			r_local.sector_t sec;

			secnum = -1;
			while ((secnum = p_spec.P_FindSectorFromLineTag(line, secnum)) >= 0)
			{
				sec = p_setup.sectors[secnum];
				if (sec.specialdata != null)
					continue;

				P_SpawnStrobeFlash(sec, p_spec.SLOWDARK, 0);
			}
		}

		//==================================================================
		//
		//	TURN LINE'S TAG LIGHTS OFF
		//
		//==================================================================
		public static void EV_TurnTagLightsOff(r_local.line_t line)
		{
			int i;
			int j;
			int min;
			r_local.sector_t sector;
			r_local.sector_t tsec;
			r_local.line_t templine;

			for (j = 0; j < p_setup.numsectors; j++)
			{
				sector = p_setup.sectors[j];
				if (sector.tag == line.tag)
				{
					min = sector.lightlevel;
					for (i = 0; i < sector.linecount; i++)
					{
						templine = p_setup.linebuffer[sector.linesi + i];
						tsec = p_spec.getNextSector(templine, sector);
						if (tsec == null)
							continue;
						if (tsec.lightlevel < min)
							min = tsec.lightlevel;
					}
					sector.lightlevel = (short)min;
				}
			}
		}


		//==================================================================
		//
		//	TURN LINE'S TAG LIGHTS ON
		//
		//==================================================================
		public static void EV_LightTurnOn(r_local.line_t line, int bright)
		{
			int i;
			int j;
			r_local.sector_t sector;
			r_local.sector_t temp;
			r_local.line_t templine;


			for (i = 0; i < p_setup.numsectors; i++)
			{
				sector = p_setup.sectors[i];
				if (sector.tag == line.tag)
				{
					//
					// bright = 0 means to search for highest
					// light level surrounding sector
					//
					if (bright == 0)
					{
						for (j = 0; j < sector.linecount; j++)
						{
							templine = p_setup.linebuffer[sector.linesi + j];
							temp = p_spec.getNextSector(templine, sector);
							if (temp == null)
								continue;
							if (temp.lightlevel > bright)
								bright = temp.lightlevel;
						}
					}
					sector.lightlevel = (short)bright;
				}
			}
		}

		//==================================================================
		//
		//	Spawn glowing light
		//
		//==================================================================
		public class T_Glow : DoomDef.think_t_delegate
		{
			public T_Glow(object in_obj) : base(in_obj) { }
			public override void function(object obj)
			{
				p_spec.glow_t g = obj as p_spec.glow_t;
				switch (g.direction)
				{
					case -1:		// DOWN
						g.sector.lightlevel -= p_spec.GLOWSPEED;
						if (g.sector.lightlevel <= g.minlight)
						{
							g.sector.lightlevel += p_spec.GLOWSPEED;
							g.direction = 1;
						}
						break;
					case 1:			// UP
						g.sector.lightlevel += p_spec.GLOWSPEED;
						if (g.sector.lightlevel >= g.maxlight)
						{
							g.sector.lightlevel -= p_spec.GLOWSPEED;
							g.direction = -1;
						}
						break;
				}
			}
		}

		public static void P_SpawnGlowingLight(r_local.sector_t sector)
		{
			p_spec.glow_t g;

			g = new p_spec.glow_t();
			p_tick.P_AddThinker(g.thinker);
			g.sector = sector;
			g.minlight = p_spec.P_FindMinSurroundingLight(sector, sector.lightlevel);
			g.maxlight = sector.lightlevel;
			g.thinker.function = new T_Glow(g);
			g.direction = -1;

			sector.special = 0;
		}
	}
}
