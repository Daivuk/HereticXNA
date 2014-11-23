using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// P_spec.h
// P_Spec.c

namespace HereticXNA
{
	public static class p_spec
	{


		/*
		===============================================================================

									P_SPEC

		===============================================================================
		*/

		//
		//	Animating textures and planes
		//
		public class anim_t
		{
			public bool istexture;
			public int picnum;
			public int basepic;
			public int numpics;
			public int speed;
		} ;

		//
		//	source animation definition
		//
		public class animdef_t
		{
			public bool istexture;		// if false, it's a flat
			public string endname;
			public string startname;
			public int speed;
		}
		public const int MAXANIMS = 32;

		//
		//	Animating line specials
		//
		public const int MAXLINEANIMS = 64;

		//	Define values for map objects
		public const int MO_TELEPORTMAN = 14;

		//
		//	SPECIAL
		//

		/*
		===============================================================================

									P_LIGHTS

		===============================================================================
		*/
		public class lightflash_t
		{
			public DoomDef.thinker_t thinker = new DoomDef.thinker_t();
			public r_local.sector_t sector;
			public int count;
			public int maxlight;
			public int minlight;
			public int maxtime;
			public int mintime;
		}
		public class strobe_t
		{
			public DoomDef.thinker_t thinker = new DoomDef.thinker_t();
			public r_local.sector_t sector;
			public int count;
			public int minlight;
			public int maxlight;
			public int darktime;
			public int brighttime;
		}

		public class glow_t
		{
			public DoomDef.thinker_t thinker = new DoomDef.thinker_t();
			public r_local.sector_t sector;
			public int minlight;
			public int maxlight;
			public int direction;
		}

		public const int GLOWSPEED = 8;
		public const int STROBEBRIGHT = 5;
		public const int FASTDARK = 15;
		public const int SLOWDARK = 35;
		/*
===============================================================================

							P_SWITCH

===============================================================================
*/
		public class switchlist_t
		{
			public string name1;
			public string name2;
			public short episode;
		}
		public enum bwhere_e
		{
			top,
			middle,
			bottom
		} ;

		public class button_t
		{
			public r_local.line_t line;
			public bwhere_e where;
			public int btexture;
			public int btimer;
			public DoomDef.mobj_t soundorg;
		} ;

		public const int MAXSWITCHES = 50;	// max # of wall switches in a level
		public const int MAXBUTTONS = 16;	// 4 players, 4 buttons each at once, max.
		public const int BUTTONTIME = 35;		// 1 second

		/*
===============================================================================

							P_PLATS

===============================================================================
*/
		public enum plat_e
		{
			up,
			down,
			waiting,
			in_stasis
		} ;

		public enum plattype_e
		{
			perpetualRaise,
			downWaitUpStay,
			raiseAndChange,
			raiseToNearestAndChange
		} ;

		public class plat_t
		{
			public DoomDef.thinker_t thinker = new DoomDef.thinker_t();
			public r_local.sector_t sector;
			public int speed;
			public int low;
			public int high;
			public int wait;
			public int count;
			public plat_e status;
			public plat_e oldstatus;
			public bool crush;
			public int tag;
			public plattype_e type;
		} ;

		public const int PLATWAIT = 3;
		public const int PLATSPEED = DoomDef.FRACUNIT;
		public const int MAXPLATS = 30;
		/*
		===============================================================================

									P_DOORS

		===============================================================================
		*/
		public enum vldoor_e
		{
			normal,
			close30ThenOpen,
			close,
			open,
			raiseIn5Mins
		} ;

		public class vldoor_t
		{
			public DoomDef.thinker_t thinker = new DoomDef.thinker_t();
			public vldoor_e type;
			public r_local.sector_t sector;
			public int topheight;
			public int speed;
			public int direction;		// 1 = up, 0 = waiting at top, -1 = down
			public int topwait;		// tics to wait at the top
			// (keep in case a door going down is reset)
			public int topcountdown;	// when it reaches 0, start going down
		} ;

		public const int VDOORSPEED = DoomDef.FRACUNIT * 2;
		public const int VDOORWAIT = 150;


		/*
===============================================================================

							P_CEILNG

===============================================================================
*/
		public enum ceiling_e
		{
			lowerToFloor,
			raiseToHighest,
			lowerAndCrush,
			crushAndRaise,
			fastCrushAndRaise
		} ;

		public class ceiling_t
		{
			public DoomDef.thinker_t thinker = new DoomDef.thinker_t();
			public ceiling_e type;
			public r_local.sector_t sector;
			public int bottomheight, topheight;
			public int speed;
			public bool crush;
			public int direction;		// 1 = up, 0 = waiting, -1 = down
			public int tag;			// ID
			public int olddirection;
		} ;

		public const int CEILSPEED = DoomDef.FRACUNIT;
		public const int CEILWAIT = 150;
		public const int MAXCEILINGS = 30;

		/*
		===============================================================================

									P_FLOOR

		===============================================================================
		*/
		public enum floor_e
		{
			lowerFloor,			// lower floor to highest surrounding floor
			lowerFloorToLowest,	// lower floor to lowest surrounding floor
			turboLower,			// lower floor to highest surrounding floor VERY FAST
			raiseFloor,			// raise floor to lowest surrounding CEILING
			raiseFloorToNearest,	// raise floor to next highest surrounding floor
			raiseToTexture,		// raise floor to shortest height texture around it
			lowerAndChange,		// lower floor to lowest surrounding floor and change
			// floorpic
			raiseFloor24,
			raiseFloor24AndChange,
			raiseFloorCrush,
			donutRaise,
			raiseBuildStep		// One step of a staircase
		} ;

		public class floormove_t
		{
			public DoomDef.thinker_t thinker = new DoomDef.thinker_t();
			public floor_e type;
			public bool crush;
			public r_local.sector_t sector;
			public int direction;
			public int newspecial;
			public short texture;
			public int floordestheight;
			public int speed;
		};


		public const int FLOORSPEED = DoomDef.FRACUNIT;

		public enum result_e
		{
			ok,
			crushed,
			pastdest
		};


		/*
		===============================================================================

									P_TELEPT

		===============================================================================
		*/

		// Macros

		public static int MAX_AMBIENT_SFX = 8; // Per level

		// Types

		public enum afxcmd_t
		{
			afxcmd_play,		// (sound)
			afxcmd_playabsvol,	// (sound, volume)
			afxcmd_playrelvol,	// (sound, volume)
			afxcmd_delay,		// (ticks)
			afxcmd_delayrand,	// (andbits)
			afxcmd_end			// ()
		}
		// Data

		public static int[][] LevelAmbientSfx = new int[MAX_AMBIENT_SFX][];
		public static int[] AmbSfxPtr;
		public static int AmbSfxPtri = 0;
		public static int AmbSfxCount;
		public static int AmbSfxTics;
		public static int AmbSfxVolume;

		public static int[] AmbSndSeqInit = new int[] { (int)afxcmd_t.afxcmd_end };

		public static int[] AmbSndSeq1 = new int[]
{ // Scream
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb1,
	(int)afxcmd_t.afxcmd_end
};
		public static int[] AmbSndSeq2 = new int[]
{ // Squish
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb2,
	(int)afxcmd_t.afxcmd_end
};
		public static int[] AmbSndSeq3 = new int[]
{ // Drops
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb3,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_delayrand, 31,
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb7,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_delayrand, 31,
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb3,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_delayrand, 31,
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb7,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_delayrand, 31,
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb3,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_delayrand, 31,
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb7,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_delayrand, 31,
	(int)afxcmd_t.afxcmd_end
};
		public static int[] AmbSndSeq4 = new int[]
{ // SlowFootSteps
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb4,
	(int)afxcmd_t.afxcmd_delay, 15,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb11, -3,
	(int)afxcmd_t.afxcmd_delay, 15,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb4, -3,
	(int)afxcmd_t.afxcmd_delay, 15,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb11, -3,
	(int)afxcmd_t.afxcmd_delay, 15,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb4, -3,
	(int)afxcmd_t.afxcmd_delay, 15,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb11, -3,
	(int)afxcmd_t.afxcmd_delay, 15,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb4, -3,
	(int)afxcmd_t.afxcmd_delay, 15,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb11, -3,
	(int)afxcmd_t.afxcmd_end
};
		public static int[] AmbSndSeq5 = new int[]
{ // Heartbeat
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb5,
	(int)afxcmd_t.afxcmd_delay, 35,
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb5,
	(int)afxcmd_t.afxcmd_delay, 35,
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb5,
	(int)afxcmd_t.afxcmd_delay, 35,
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb5,
	(int)afxcmd_t.afxcmd_end
};
		public static int[] AmbSndSeq6 = new int[]
{ // Bells
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb6,
	(int)afxcmd_t.afxcmd_delay, 17,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb6, -8,
	(int)afxcmd_t.afxcmd_delay, 17,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb6, -8,
	(int)afxcmd_t.afxcmd_delay, 17,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb6, -8,
	(int)afxcmd_t.afxcmd_end
};
		public static int[] AmbSndSeq7 = new int[]
{ // Growl
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_bstsit,
	(int)afxcmd_t.afxcmd_end
};
		public static int[] AmbSndSeq8 = new int[]
{ // Magic
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb8,
	(int)afxcmd_t.afxcmd_end
};
		public static int[] AmbSndSeq9 = new int[]
{ // Laughter
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb9,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb9, -4,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb9, -4,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb10, -4,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb10, -4,
	(int)afxcmd_t.afxcmd_delay, 16,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb10, -4,
	(int)afxcmd_t.afxcmd_end
};
		public static int[] AmbSndSeq10 = new int[]
{ // FastFootsteps
	(int)afxcmd_t.afxcmd_play, (int)sounds.sfxenum_t.sfx_amb4,
	(int)afxcmd_t.afxcmd_delay, 8,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb11, -3,
	(int)afxcmd_t.afxcmd_delay, 8,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb4, -3,
	(int)afxcmd_t.afxcmd_delay, 8,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb11, -3,
	(int)afxcmd_t.afxcmd_delay, 8,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb4, -3,
	(int)afxcmd_t.afxcmd_delay, 8,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb11, -3,
	(int)afxcmd_t.afxcmd_delay, 8,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb4, -3,
	(int)afxcmd_t.afxcmd_delay, 8,
	(int)afxcmd_t.afxcmd_playrelvol, (int)sounds.sfxenum_t.sfx_amb11, -3,
	(int)afxcmd_t.afxcmd_end
};
		public static int[][] AmbientSfx = new int[][]
{
	AmbSndSeq1,		// Scream
	AmbSndSeq2,		// Squish
	AmbSndSeq3,		// Drops
	AmbSndSeq4,		// SlowFootsteps
	AmbSndSeq5,		// Heartbeat
	AmbSndSeq6,		// Bells
	AmbSndSeq7,		// Growl
	AmbSndSeq8,		// Magic
	AmbSndSeq9,		// Laughter
	AmbSndSeq10		// FastFootsteps
};

		public static animdef_t[] animdefs = new animdef_t[]
{
	// false = flat
	// true = texture
	new animdef_t{istexture=false, endname="FLTWAWA3", startname="FLTWAWA1", speed=8}, // Water
	new animdef_t{istexture=false, endname="FLTSLUD3", startname="FLTSLUD1", speed=8}, // Sludge
	new animdef_t{istexture=false, endname="FLTTELE4", startname="FLTTELE1", speed=6}, // Teleport
	new animdef_t{istexture=false, endname="FLTFLWW3", startname="FLTFLWW1", speed=9}, // River - West
	new animdef_t{istexture=false, endname="FLTLAVA4", startname="FLTLAVA1", speed=8}, // Lava
	new animdef_t{istexture=false, endname="FLATHUH4", startname="FLATHUH1", speed=8}, // Super Lava
	new animdef_t{istexture=true, endname="LAVAFL3", startname="LAVAFL1", speed=6}, // Texture: Lavaflow
	new animdef_t{istexture=true, endname="WATRWAL3", startname="WATRWAL1",speed= 4}, // Texture: Waterfall
//	new animdef_t{-1} // [dsl] We don't need to do this, because C# is magic
};

		public static anim_t[] anims = new anim_t[MAXANIMS] {
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t(),
	new anim_t()
};
		public static int lastanimi;
		public static int[] TerrainTypes;
		public class sTerrainTypeDefs
		{
			public string name;
			public int type;
		}

		public static sTerrainTypeDefs[] TerrainTypeDefs = new sTerrainTypeDefs[]
{
	new sTerrainTypeDefs{ name="FLTWAWA1", type=p_local.FLOOR_WATER },
	new sTerrainTypeDefs{ name="FLTFLWW1", type=p_local.FLOOR_WATER },
	new sTerrainTypeDefs{name= "FLTLAVA1", type=p_local.FLOOR_LAVA },
	new sTerrainTypeDefs{ name="FLATHUH1", type=p_local.FLOOR_LAVA },
	new sTerrainTypeDefs{ name="FLTSLUD1", type=p_local.FLOOR_SLUDGE }
	//{ "END", -1 } // Don't need this we know the size of the array
};

		public static DoomDef.mobj_t LavaInflictor = new DoomDef.mobj_t();

		//----------------------------------------------------------------------------
		//
		// PROC P_InitLava
		//
		//----------------------------------------------------------------------------

		public static void P_InitLava()
		{
			LavaInflictor.type = info.mobjtype_t.MT_PHOENIXFX2;
			LavaInflictor.flags2 = DoomDef.MF2_FIREDAMAGE | DoomDef.MF2_NODMGTHRUST;
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_InitTerrainTypes
		//
		//----------------------------------------------------------------------------

		public static void P_InitTerrainTypes()
		{
			int i;
			int lump;
			int size;

			size = (r_data.numflats + 1) * sizeof(int);
			TerrainTypes = new int[size];
			for (i = 0; i < TerrainTypeDefs.Length; i++)
			{
				lump = w_wad.W_CheckNumForName(TerrainTypeDefs[i].name);
				if (lump != -1)
				{
					TerrainTypes[lump - r_data.firstflat] = TerrainTypeDefs[i].type;
				}
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_InitPicAnims
		//
		//----------------------------------------------------------------------------

		public static void P_InitPicAnims()
		{
			int i;

			lastanimi = 0;
			for (i = 0; i < animdefs.Length; i++)
			{
				if (animdefs[i].istexture)
				{ // Texture animation
					if (r_data.R_CheckTextureNumForName(animdefs[i].startname) == -1)
					{ // Texture doesn't exist
						continue;
					}
					anims[lastanimi].picnum = r_data.R_TextureNumForName(animdefs[i].endname);
					anims[lastanimi].basepic = r_data.R_TextureNumForName(animdefs[i].startname);
				}
				else
				{ // Flat animation
					if (w_wad.W_CheckNumForName(animdefs[i].startname) == -1)
					{ // Flat doesn't exist
						continue;
					}
					anims[lastanimi].picnum = r_data.R_FlatNumForName(animdefs[i].endname);
					anims[lastanimi].basepic = r_data.R_FlatNumForName(animdefs[i].startname);
				}
				anims[lastanimi].istexture = animdefs[i].istexture;
				anims[lastanimi].numpics = anims[lastanimi].picnum - anims[lastanimi].basepic + 1;
				if (anims[lastanimi].numpics < 2)
				{
					i_ibm.I_Error("P_InitPicAnims: bad cycle from " + animdefs[i].startname + " to " + animdefs[i].endname);
				}
				anims[lastanimi].speed = animdefs[i].speed;
				lastanimi++;
			}
		}

		/*
		==============================================================================

									UTILITIES

		==============================================================================
		*/

		//
		//	Will return a side_t* given the number of the current sector,
		//		the line number, and the side (0/1) that you want.
		//
		public static r_local.side_t getSide(int currentSector, int line, int side)
		{
			return p_setup.sides[(p_setup.linebuffer[p_setup.sectors[currentSector].linesi + line]).sidenum[side]];
		}

		//
		//	Will return a sector_t* given the number of the current sector,
		//		the line number and the side (0/1) that you want.
		//
		public static r_local.sector_t getSector(int currentSector, int line, int side)
		{
			return p_setup.sides[(p_setup.linebuffer[p_setup.sectors[currentSector].linesi + line]).sidenum[side]].sector;
		}

		//
		//	Given the sector number and the line number, will tell you whether
		//		the line is two-sided or not.
		//
		public static int twoSided(int sector, int line)
		{
			return (p_setup.linebuffer[p_setup.sectors[sector].linesi + line]).flags & DoomData.ML_TWOSIDED;
		}


		//==================================================================
		//
		//	Return sector_t * of sector next to current. NULL if not two-sided line
		//
		//==================================================================
		public static r_local.sector_t getNextSector(r_local.line_t line, r_local.sector_t sec)
		{
			if ((line.flags & DoomData.ML_TWOSIDED) == 0)
				return null;

			if (line.frontsector == sec)
				return line.backsector;

			return line.frontsector;
		}

		//==================================================================
		//
		//	FIND LOWEST FLOOR HEIGHT IN SURROUNDING SECTORS
		//
		//==================================================================
		public static int P_FindLowestFloorSurrounding(r_local.sector_t sec)
		{
			int i;
			r_local.line_t check;
			r_local.sector_t other;
			int floor = sec.floorheight;

			for (i = 0; i < sec.linecount; i++)
			{
				check = p_setup.linebuffer[sec.linesi + i];
				other = getNextSector(check, sec);
				if (other == null)
					continue;
				if (other.floorheight < floor)
					floor = other.floorheight;
			}
			return floor;
		}

		//==================================================================
		//
		//	FIND HIGHEST FLOOR HEIGHT IN SURROUNDING SECTORS
		//
		//==================================================================
		static public int P_FindHighestFloorSurrounding(r_local.sector_t sec)
		{
			int i;
			r_local.line_t check;
			r_local.sector_t other;
			int floor = -500 * DoomDef.FRACUNIT;

			for (i = 0; i < sec.linecount; i++)
			{
				check = p_setup.linebuffer[sec.linesi + i];
				other = getNextSector(check, sec);
				if (other == null)
					continue;
				if (other.floorheight > floor)
					floor = other.floorheight;
			}
			return floor;
		}

		//==================================================================
		//
		//	FIND NEXT HIGHEST FLOOR IN SURROUNDING SECTORS
		//
		//==================================================================
		public static int P_FindNextHighestFloor(r_local.sector_t sec, int currentheight)
		{
			int i;
			int h;
			int min;
			r_local.line_t check;
			r_local.sector_t other;
			int height = currentheight;
			int[] heightlist = new int[20];		// 20 adjoining sectors max! // [dsl] This is not the case for most maps! Map designer had to be careful here

			for (i = 0, h = 0; i < sec.linecount; i++)
			{
				check = p_setup.linebuffer[sec.linesi + i];
				other = getNextSector(check, sec);
				if (other == null)
					continue;
				if (other.floorheight > height)
					if (h < heightlist.Count())
						heightlist[h++] = other.floorheight;
			}

			//
			// Find lowest height in list
			//
			min = heightlist[0];
			for (i = 1; i < h; i++)
				if (heightlist[i] < min)
					min = heightlist[i];

			return min;
		}


		//==================================================================
		//
		//	FIND LOWEST CEILING IN THE SURROUNDING SECTORS
		//
		//==================================================================
		public static int P_FindLowestCeilingSurrounding(r_local.sector_t sec)
		{
			int i;
			r_local.line_t check;
			r_local.sector_t other;
			int height = DoomDef.MAXINT;

			for (i = 0; i < sec.linecount; i++)
			{
				check = p_setup.linebuffer[sec.linesi + i];
				other = getNextSector(check, sec);
				if (other == null)
					continue;
				if (other.ceilingheight < height)
					height = other.ceilingheight;
			}
			return height;
		}

		//==================================================================
		//
		//	FIND HIGHEST CEILING IN THE SURROUNDING SECTORS
		//
		//==================================================================
		public static int P_FindHighestCeilingSurrounding(r_local.sector_t sec)
		{
			int i;
			r_local.line_t check;
			r_local.sector_t other;
			int height = 0;

			for (i = 0; i < sec.linecount; i++)
			{
				check = p_setup.linebuffer[sec.linesi + i];
				other = getNextSector(check, sec);
				if (other == null)
					continue;
				if (other.ceilingheight > height)
					height = other.ceilingheight;
			}
			return height;
		}

		//==================================================================
		//
		//	RETURN NEXT SECTOR # THAT LINE TAG REFERS TO
		//
		//==================================================================
		public static int P_FindSectorFromLineTag(r_local.line_t line, int start)
		{
			int i;

			for (i = start + 1; i < p_setup.numsectors; i++)
				if (p_setup.sectors[i].tag == line.tag)
					return i;
			return -1;
		}


		//==================================================================
		//
		//	Find minimum light from an adjacent sector
		//
		//==================================================================
		public static int P_FindMinSurroundingLight(r_local.sector_t sector, int max)
		{
			int i;
			int min;
			r_local.line_t line;
			r_local.sector_t check;

			min = max;
			for (i = 0; i < sector.linecount; i++)
			{
				line = p_setup.linebuffer[sector.linesi + i];
				check = getNextSector(line, sector);
				if (check == null)
					continue;
				if (check.lightlevel < min)
					min = check.lightlevel;
			}
			return min;
		}

		/*
		==============================================================================

									EVENTS

		Events are operations triggered by using, crossing, or shooting special lines, or by timed thinkers

		==============================================================================
		*/

		/*
		===============================================================================
		=
		= P_CrossSpecialLine - TRIGGER
		=
		= Called every time a thing origin is about to cross
		= a line with a non 0 special
		=
		===============================================================================
		*/

		public static void P_CrossSpecialLine(int linenum, int side, DoomDef.mobj_t thing)
		{
			r_local.line_t line;

			line = p_setup.lines[linenum];
			if (thing.player == null)
			{ // Check if trigger allowed by non-player mobj
				switch (line.special)
				{
					case 39:	// Trigger_TELEPORT
					case 97:	// Retrigger_TELEPORT
					case 4:		// Trigger_Raise_Door
						// [dsl] That was commented out already
						//case 10:	// PLAT DOWN-WAIT-UP-STAY TRIGGER 
						//case 88:	// PLAT DOWN-WAIT-UP-STAY RETRIGGER
						break;
					default:
						return;
						break;
				}
			}
			switch (line.special)
			{
				//====================================================
				// TRIGGERS
				//====================================================
				case 2: // Open Door
					p_doors.EV_DoDoor(line, vldoor_e.open, VDOORSPEED);
					line.special = 0;
					break;
				case 3: // Close Door
					p_doors.EV_DoDoor(line, vldoor_e.close, VDOORSPEED);
					line.special = 0;
					break;
				case 4: // Raise Door
					p_doors.EV_DoDoor(line, vldoor_e.normal, VDOORSPEED);
					line.special = 0;
					break;
				case 5: // Raise Floor
					p_floor.EV_DoFloor(line, floor_e.raiseFloor);
					line.special = 0;
					break;
				case 6: // Fast Ceiling Crush & Raise
					p_ceilng.EV_DoCeiling(line, ceiling_e.fastCrushAndRaise);
					line.special = 0;
					break;
				case 8: // Trigger_Build_Stairs (8 pixel steps)
					p_floor.EV_BuildStairs(line, 8 * DoomDef.FRACUNIT);
					line.special = 0;
					break;
				case 106: // Trigger_Build_Stairs_16 (16 pixel steps)
					p_floor.EV_BuildStairs(line, 16 * DoomDef.FRACUNIT);
					line.special = 0;
					break;
				case 10: // PlatDownWaitUp
					p_plats.EV_DoPlat(line, p_spec.plattype_e.downWaitUpStay, 0);
					line.special = 0;
					break;
				case 12: // Light Turn On - brightest near
					p_lights.EV_LightTurnOn(line, 0);
					line.special = 0;
					break;
				case 13: // Light Turn On 255
					p_lights.EV_LightTurnOn(line, 255);
					line.special = 0;
					break;
				case 16: // Close Door 30
					p_doors.EV_DoDoor(line, vldoor_e.close30ThenOpen, VDOORSPEED);
					line.special = 0;
					break;
				case 17: // Start Light Strobing
					p_lights.EV_StartLightStrobing(line);
					line.special = 0;
					break;
				case 19: // Lower Floor
					p_floor.EV_DoFloor(line, floor_e.lowerFloor);
					line.special = 0;
					break;
				case 22: // Raise floor to nearest height and change texture
					p_plats.EV_DoPlat(line, p_spec.plattype_e.raiseToNearestAndChange, 0);
					line.special = 0;
					break;
				case 25: // Ceiling Crush and Raise
					p_ceilng.EV_DoCeiling(line, ceiling_e.crushAndRaise);
					line.special = 0;
					break;
				case 30:		// Raise floor to shortest texture height
					// on either side of lines
					p_floor.EV_DoFloor(line, floor_e.raiseToTexture);
					line.special = 0;
					break;
				case 35: // Lights Very Dark
					p_lights.EV_LightTurnOn(line, 35);
					line.special = 0;
					break;
				case 36: // Lower Floor (TURBO)
					p_floor.EV_DoFloor(line, floor_e.turboLower);
					line.special = 0;
					break;
				case 37: // LowerAndChange
					p_floor.EV_DoFloor(line, floor_e.lowerAndChange);
					line.special = 0;
					break;
				case 38: // Lower Floor To Lowest
					p_floor.EV_DoFloor(line, floor_e.lowerFloorToLowest);
					line.special = 0;
					break;
				case 39: // TELEPORT!
					p_telept.EV_Teleport(line, side, thing);
					line.special = 0;
					break;
				case 40: // RaiseCeilingLowerFloor
					p_ceilng.EV_DoCeiling(line, ceiling_e.raiseToHighest);
					p_floor.EV_DoFloor(line, floor_e.lowerFloorToLowest);
					line.special = 0;
					break;
				case 44: // Ceiling Crush
					p_ceilng.EV_DoCeiling(line, ceiling_e.lowerAndCrush);
					line.special = 0;
					break;
				case 52: // EXIT!
					g_game.G_ExitLevel();
					line.special = 0;
					break;
				case 53: // Perpetual Platform Raise
					p_plats.EV_DoPlat(line, p_spec.plattype_e.perpetualRaise, 0);
					line.special = 0;
					break;
				case 54: // Platform Stop
					p_plats.EV_StopPlat(line);
					line.special = 0;
					break;
				case 56: // Raise Floor Crush
					p_floor.EV_DoFloor(line, floor_e.raiseFloorCrush);
					line.special = 0;
					break;
				case 57: // Ceiling Crush Stop
					p_ceilng.EV_CeilingCrushStop(line);
					line.special = 0;
					break;
				case 58: // Raise Floor 24
					p_floor.EV_DoFloor(line, floor_e.raiseFloor24);
					line.special = 0;
					break;
				case 59: // Raise Floor 24 And Change
					p_floor.EV_DoFloor(line, floor_e.raiseFloor24AndChange);
					line.special = 0;
					break;
				case 104: // Turn lights off in sector(tag)
					p_lights.EV_TurnTagLightsOff(line);
					line.special = 0;
					break;
				case 105: // Trigger_SecretExit
					//G_SecretExitLevel();
					//line.special = 0;
					break;

				//====================================================
				// RE-DOABLE TRIGGERS
				//====================================================

				case 72:		// Ceiling Crush
					p_ceilng.EV_DoCeiling(line, ceiling_e.lowerAndCrush);
					break;
				case 73:		// Ceiling Crush and Raise
					p_ceilng.EV_DoCeiling(line, ceiling_e.crushAndRaise);
					break;
				case 74:		// Ceiling Crush Stop
					p_ceilng.EV_CeilingCrushStop(line);
					break;
				case 75:			// Close Door
					p_doors.EV_DoDoor(line, vldoor_e.close, VDOORSPEED);
					break;
				case 76:		// Close Door 30
					p_doors.EV_DoDoor(line, vldoor_e.close30ThenOpen, VDOORSPEED);
					break;
				case 77:			// Fast Ceiling Crush & Raise
					p_ceilng.EV_DoCeiling(line, ceiling_e.fastCrushAndRaise);
					break;
				case 79:		// Lights Very Dark
					p_lights.EV_LightTurnOn(line, 35);
					break;
				case 80:		// Light Turn On - brightest near
					p_lights.EV_LightTurnOn(line, 0);
					break;
				case 81:		// Light Turn On 255
					p_lights.EV_LightTurnOn(line, 255);
					break;
				case 82:		// Lower Floor To Lowest
					p_floor.EV_DoFloor(line, floor_e.lowerFloorToLowest);
					break;
				case 83:		// Lower Floor
					p_floor.EV_DoFloor(line, floor_e.lowerFloor);
					break;
				case 84:		// LowerAndChange
					p_floor.EV_DoFloor(line, floor_e.lowerAndChange);
					break;
				case 86:			// Open Door
					p_doors.EV_DoDoor(line, vldoor_e.open, VDOORSPEED);
					break;
				case 87:		// Perpetual Platform Raise
					p_plats.EV_DoPlat(line, p_spec.plattype_e.perpetualRaise, 0);
					break;
				case 88:		// PlatDownWaitUp
					p_plats.EV_DoPlat(line, p_spec.plattype_e.downWaitUpStay, 0);
					break;
				case 89:		// Platform Stop
					p_plats.EV_StopPlat(line);
					break;
				case 90:			// Raise Door
					p_doors.EV_DoDoor(line, vldoor_e.normal, VDOORSPEED);
					break;
				case 100: // Retrigger_Raise_Door_Turbo
					p_doors.EV_DoDoor(line, vldoor_e.normal, VDOORSPEED * 3);
					break;
				case 91:			// Raise Floor
					p_floor.EV_DoFloor(line, floor_e.raiseFloor);
					break;
				case 92:		// Raise Floor 24
					p_floor.EV_DoFloor(line, floor_e.raiseFloor24);
					break;
				case 93:		// Raise Floor 24 And Change
					p_floor.EV_DoFloor(line, floor_e.raiseFloor24AndChange);
					break;
				case 94:		// Raise Floor Crush
					p_floor.EV_DoFloor(line, floor_e.raiseFloorCrush);
					break;
				case 95:		// Raise floor to nearest height and change texture
					p_plats.EV_DoPlat(line, p_spec.plattype_e.raiseToNearestAndChange, 0);
					break;
				case 96:		// Raise floor to shortest texture height
					// on either side of lines
					p_floor.EV_DoFloor(line, floor_e.raiseToTexture);
					break;
				case 97:		// TELEPORT!
					p_telept.EV_Teleport(line, side, thing);
					break;
				case 98:		// Lower Floor (TURBO)
					p_floor.EV_DoFloor(line, floor_e.turboLower);
					break;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_ShootSpecialLine
		//
		// Called when a thing shoots a special line.
		//
		//----------------------------------------------------------------------------

		public static void P_ShootSpecialLine(DoomDef.mobj_t thing, r_local.line_t line)
		{
			if (thing.player == null)
			{ // Check if trigger allowed by non-player mobj
				switch (line.special)
				{
					case 46: // Impact_OpenDoor
						break;
					default:
						return;
						break;
				}
			}

			switch (line.special)
			{
				case 24: // Impact_RaiseFloor
					p_floor.EV_DoFloor(line, floor_e.raiseFloor);
					p_switch.P_ChangeSwitchTexture(line, 0);
					break;
				case 46: // Impact_OpenDoor
					p_doors.EV_DoDoor(line, vldoor_e.open, VDOORSPEED);
					p_switch.P_ChangeSwitchTexture(line, 1);
					break;
				case 47: // Impact_RaiseFloorNear&Change
					p_plats.EV_DoPlat(line, plattype_e.raiseToNearestAndChange, 0);
					p_switch.P_ChangeSwitchTexture(line, 0);
					break;
			}
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_PlayerInSpecialSector
		//
		// Called every tic frame that the player origin is in a special sector.
		//
		//----------------------------------------------------------------------------
		static int[] pushTab = new int[5] {
		2048*5,
		2048*10,
		2048*25,
		2048*30,
		2048*35
	};

		public static void P_PlayerInSpecialSector(DoomDef.player_t player)
		{
			r_local.sector_t sector;

			sector = player.mo.subsector.sector;
			if (player.mo.z != sector.floorheight)
			{ // Player is not touching the floor
				return;
			}
			switch (sector.special)
			{
				case 7: // Damage_Sludge
					if ((p_tick.leveltime & 31) == 0)
					{
						p_inter.P_DamageMobj(player.mo, null, null, 4);
					}
					break;
				case 5: // Damage_LavaWimpy
					if ((p_tick.leveltime & 15) == 0)
					{
						p_inter.P_DamageMobj(player.mo, LavaInflictor, null, 5);
						p_mobj.P_HitFloor(player.mo);
					}
					break;
				case 16: // Damage_LavaHefty
					if ((p_tick.leveltime & 15) == 0)
					{
						p_inter.P_DamageMobj(player.mo, LavaInflictor, null, 8);
						p_mobj.P_HitFloor(player.mo);
					}
					break;
				case 4: // Scroll_EastLavaDamage
					p_user.P_Thrust(player, 0, 2048 * 28);
					if ((p_tick.leveltime & 15) == 0)
					{
						p_inter.P_DamageMobj(player.mo, LavaInflictor, null, 5);
						p_mobj.P_HitFloor(player.mo);
					}
					break;
				case 9: // SecretArea
					player.secretcount++;
					sector.special = 0;
					//	p_inter.P_SetMessage(player, "YOU FOUND A SECRET", false); // [dsl] I added that
					break;
				case 11: // Exit_SuperDamage (DOOM E1M8 finale)
					break;

				case 25:
				case 26:
				case 27:
				case 28:
				case 29: // Scroll_North
					p_user.P_Thrust(player, DoomDef.ANG90, pushTab[sector.special - 25]);
					break;
				case 20:
				case 21:
				case 22:
				case 23:
				case 24: // Scroll_East
					p_user.P_Thrust(player, 0, pushTab[sector.special - 20]);
					break;
				case 30:
				case 31:
				case 32:
				case 33:
				case 34: // Scroll_South
					p_user.P_Thrust(player, DoomDef.ANG270, pushTab[sector.special - 30]);
					break;
				case 35:
				case 36:
				case 37:
				case 38:
				case 39: // Scroll_West
					p_user.P_Thrust(player, DoomDef.ANG180, pushTab[sector.special - 35]);
					break;

				case 40:
				case 41:
				case 42:
				case 43:
				case 44:
				case 45:
				case 46:
				case 47:
				case 48:
				case 49:
				case 50:
				case 51:
					// Wind specials are handled in (P_mobj):P_XYMovement
					break;

				case 15: // Friction_Low
					// Only used in (P_mobj):P_XYMovement and (P_user):P_Thrust
					break;

				default:
					i_ibm.I_Error("P_PlayerInSpecialSector: unknown special " + sector.special);
					break;
			}
		}
		//----------------------------------------------------------------------------
		//
		// PROC P_UpdateSpecials
		//
		// Animate planes, scroll walls, etc.
		//
		//----------------------------------------------------------------------------

		public static void P_UpdateSpecials()
		{
			int i;
			int pic;
			anim_t anim;
			r_local.line_t line;

			// Animate flats and textures
			for (i = 0; i < lastanimi; i++)
			{
				anim = anims[i];
				for (int j = anim.basepic; j < anim.basepic + anim.numpics; j++)
				{
					pic = anim.basepic + ((p_tick.leveltime / anim.speed + i) % anim.numpics);
					if (anim.istexture)
					{
						if (r_data.texturetranslation[j] != pic)
							r_data.texturetranslationPrev[j] = r_data.texturetranslation[j];
						r_data.texturetranslation[j] = pic;
						r_data.texturetranslationDeltas[j] = (float)(p_tick.leveltime % anim.speed) / (float)anim.speed;
					}
					else
					{
						if (r_data.flattranslation[j] != pic)
							r_data.flattranslationPrev[j] = r_data.flattranslation[j];
						r_data.flattranslation[j] = pic;
						r_data.flattranslationDeltas[j] = (float)(p_tick.leveltime % anim.speed) / (float)anim.speed;
					}
				}
			}
			// Update scrolling texture offsets
			for (i = 0; i < numlinespecials; i++)
			{
				line = linespeciallist[i];
				switch (line.special)
				{
					case 48: // Effect_Scroll_Left
						p_setup.sides[line.sidenum[0]].textureoffset += DoomDef.FRACUNIT;
						break;
					case 99: // Effect_Scroll_Right
						p_setup.sides[line.sidenum[0]].textureoffset -= DoomDef.FRACUNIT;
						break;
				}
			}
			// Handle buttons
			for (i = 0; i < MAXBUTTONS; i++)
			{
				if (p_switch.buttonlist[i].btimer != 0)
				{
					p_switch.buttonlist[i].btimer--;
					if (p_switch.buttonlist[i].btimer == 0)
					{
						switch (p_switch.buttonlist[i].where)
						{
							case bwhere_e.top:
								p_setup.sides[p_switch.buttonlist[i].line.sidenum[0]].toptexture =
									(short)p_switch.buttonlist[i].btexture;
								break;
							case bwhere_e.middle:
								p_setup.sides[p_switch.buttonlist[i].line.sidenum[0]].midtexture =
									(short)p_switch.buttonlist[i].btexture;
								break;
							case bwhere_e.bottom:
								p_setup.sides[p_switch.buttonlist[i].line.sidenum[0]].bottomtexture =
									(short)p_switch.buttonlist[i].btexture;
								break;
						}
						i_ibm.S_StartSound(
							p_switch.buttonlist[i].soundorg.x,
							p_switch.buttonlist[i].soundorg.y,
							p_switch.buttonlist[i].soundorg.z,
							(int)sounds.sfxenum_t.sfx_switch);
						p_switch.buttonlist[i].line = null;
						p_switch.buttonlist[i].where = bwhere_e.top;
						p_switch.buttonlist[i].btexture = 0;
						p_switch.buttonlist[i].btimer = 0;
						p_switch.buttonlist[i].soundorg = null;
					}
				}
			}
		}
#if DOS
//============================================================
//
//	Special Stuff that can't be categorized
//
//============================================================
int EV_DoDonut(line_t *line)
{
	sector_t	*s1;
	sector_t	*s2;
	sector_t	*s3;
	int			secnum;
	int			rtn;
	int			i;
	floormove_t		*floor;
	
	secnum = -1;
	rtn = 0;
	while ((secnum = P_FindSectorFromLineTag(line,secnum)) >= 0)
	{
		s1 = &sectors[secnum];
		
		//	ALREADY MOVING?  IF SO, KEEP GOING...
		if (s1.specialdata)
			continue;
			
		rtn = 1;
		s2 = getNextSector(s1.lines[0],s1);
		for (i = 0;i < s2.linecount;i++)
		{
			if ((!s2.lines[i].flags & ML_TWOSIDED) ||
				(s2.lines[i].backsector == s1))
				continue;
			s3 = s2.lines[i].backsector;

			//
			//	Spawn rising slime
			//
			floor = Z_Malloc (sizeof(*floor), PU_LEVSPEC, 0);
			P_AddThinker (&floor.thinker);
			s2.specialdata = floor;
			floor.thinker.function = T_MoveFloor;
			floor.type = donutRaise;
			floor.crush = false;
			floor.direction = 1;
			floor.sector = s2;
			floor.speed = FLOORSPEED / 2;
			floor.texture = s3.floorpic;
			floor.newspecial = 0;
			floor.floordestheight = s3.floorheight;
			
			//
			//	Spawn lowering donut-hole
			//
			floor = Z_Malloc (sizeof(*floor), PU_LEVSPEC, 0);
			P_AddThinker (&floor.thinker);
			s1.specialdata = floor;
			floor.thinker.function = T_MoveFloor;
			floor.type = lowerFloor;
			floor.crush = false;
			floor.direction = -1;
			floor.sector = s1;
			floor.speed = FLOORSPEED / 2;
			floor.floordestheight = s3.floorheight;
			break;
		}
	}
	return rtn;
}
#endif
		/*
==============================================================================

							SPECIAL SPAWNING

==============================================================================
*/
		/*
		================================================================================
		= P_SpawnSpecials
		=
		= After the map has been loaded, scan for specials that
		= spawn thinkers
		=
		===============================================================================
		*/

		public static short numlinespecials;
		public static r_local.line_t[] linespeciallist = new r_local.line_t[p_spec.MAXLINEANIMS];

		public static void P_SpawnSpecials()
		{
			r_local.sector_t sector;
			int i;
			int episode;

			episode = 1;
			if (w_wad.W_CheckNumForName("texture2") >= 0)
				episode = 2;

			//
			//	Init special SECTORs
			//

			for (i = 0; i < p_setup.numsectors; i++)
			{
				sector = p_setup.sectors[i];
				if (sector.special == 0)
					continue;
				switch (sector.special)
				{
					case 1:		// FLICKERING LIGHTS
						p_lights.P_SpawnLightFlash(sector);
						break;
					case 2:		// STROBE FAST
						p_lights.P_SpawnStrobeFlash(sector, FASTDARK, 0);
						break;
					case 3:		// STROBE SLOW
						p_lights.P_SpawnStrobeFlash(sector, SLOWDARK, 0);
						break;
					case 4:		// STROBE FAST/DEATH SLIME
						p_lights.P_SpawnStrobeFlash(sector, FASTDARK, 0);
						sector.special = 4;
						break;
					case 8:		// GLOWING LIGHT
						p_lights.P_SpawnGlowingLight(sector);
						break;
					case 9:		// SECRET SECTOR
						g_game.totalsecret++;
						break;
					case 10:	// DOOR CLOSE IN 30 SECONDS
						//TODO: P_SpawnDoorCloseIn30 (sector);
						break;
					case 12:	// SYNC STROBE SLOW
						p_lights.P_SpawnStrobeFlash(sector, SLOWDARK, 1);
						break;
					case 13:	// SYNC STROBE FAST
						p_lights.P_SpawnStrobeFlash(sector, FASTDARK, 1);
						break;
					case 14:	// DOOR RAISE IN 5 MINUTES
						//TODO: P_SpawnDoorRaiseIn5Mins (sector, i);
						break;
				}
			}


			//
			//	Init line EFFECTs
			//
			numlinespecials = 0;
			for (i = 0; i < p_setup.numlines; i++)
				switch (p_setup.lines[i].special)
				{
					case 48: // Effect_Scroll_Left
					case 99: // Effect_Scroll_Right
						linespeciallist[numlinespecials] = p_setup.lines[i];
						numlinespecials++;
						break;
					default:
						break;
				}

			//
			//	Init other misc stuff
			//
			for (i = 0; i < MAXCEILINGS; i++)
				p_ceilng.activeceilings[i] = null;
			for (i = 0; i < MAXPLATS; i++)
				p_plats.activeplats[i] = null;
			for (i = 0; i < MAXBUTTONS; i++)
			{
				p_switch.buttonlist[i].line = null;
				p_switch.buttonlist[i].where = bwhere_e.top;
				p_switch.buttonlist[i].btexture = 0;
				p_switch.buttonlist[i].btimer = 0;
				p_switch.buttonlist[i].soundorg = null;
			}
		}


		//----------------------------------------------------------------------------
		//
		// PROC P_InitAmbientSound
		//
		//----------------------------------------------------------------------------

		public static void P_InitAmbientSound()
		{
			AmbSfxCount = 0;
			AmbSfxVolume = 0;
			AmbSfxTics = 10 * DoomDef.TICSPERSEC;
			AmbSfxPtr = AmbSndSeqInit;
			AmbSfxPtri = 0;
		}


		//----------------------------------------------------------------------------
		//
		// PROC P_AddAmbientSfx
		//
		// Called by (P_mobj):P_SpawnMapThing during (P_setup):P_SetupLevel.
		//
		//----------------------------------------------------------------------------

		public static void P_AddAmbientSfx(int sequence)
		{
			if (AmbSfxCount == MAX_AMBIENT_SFX)
			{
				i_ibm.I_Error("Too many ambient sound sequences");
			}
			p_spec.LevelAmbientSfx[AmbSfxCount++] = AmbientSfx[sequence];
		}

		//----------------------------------------------------------------------------
		//
		// PROC P_AmbientSound
		//
		// Called every tic by (P_tick):P_Ticker.
		//
		//----------------------------------------------------------------------------

		public static void P_AmbientSound()
		{
			afxcmd_t cmd;
			int sound;
			bool done;

			if (AmbSfxCount == 0)
			{ // No ambient sound sequences on current level
				return;
			}
			if ((--AmbSfxTics) != 0)
			{
				return;
			}
			done = false;
			do
			{
				cmd = (afxcmd_t)AmbSfxPtr[AmbSfxPtri++];
				switch (cmd)
				{
					case afxcmd_t.afxcmd_play:
						AmbSfxVolume = m_misc.P_Random() >> 2;
						i_ibm.S_StartSoundAtVolume(null, AmbSfxPtr[AmbSfxPtri++], AmbSfxVolume);
						//AmbSfxVolume = P_Random()>>2;
						//S_StartSoundAtVolume(NULL, *AmbSfxPtr++, AmbSfxVolume);
						break;
					case afxcmd_t.afxcmd_playabsvol:
						sound = AmbSfxPtr[AmbSfxPtri++];
						AmbSfxVolume = AmbSfxPtr[AmbSfxPtri++];
						i_ibm.S_StartSoundAtVolume(null, sound, AmbSfxVolume);
						//sound = *AmbSfxPtr++;
						//AmbSfxVolume = *AmbSfxPtr++;
						//S_StartSoundAtVolume(NULL, sound, AmbSfxVolume);
						break;
					case afxcmd_t.afxcmd_playrelvol:
						sound = AmbSfxPtr[AmbSfxPtri++];
						AmbSfxVolume += AmbSfxPtr[AmbSfxPtri++];
						if (AmbSfxVolume < 0)
						{
							AmbSfxVolume = 0;
						}
						else if (AmbSfxVolume > 127)
						{
							AmbSfxVolume = 127;
						}
						i_ibm.S_StartSoundAtVolume(null, sound, AmbSfxVolume);
						//sound = *AmbSfxPtr++;
						//AmbSfxVolume += *AmbSfxPtr++;
						//if(AmbSfxVolume < 0)
						//{
						//    AmbSfxVolume = 0;
						//}
						//else if(AmbSfxVolume > 127)
						//{
						//    AmbSfxVolume = 127;
						//}			
						//S_StartSoundAtVolume(NULL, sound, AmbSfxVolume);
						break;
					case afxcmd_t.afxcmd_delay:
						AmbSfxTics = AmbSfxPtr[AmbSfxPtri++];
						done = true;
						//AmbSfxTics = *AmbSfxPtr++;
						//done = true;
						break;
					case afxcmd_t.afxcmd_delayrand:
						AmbSfxTics = m_misc.P_Random() & (AmbSfxPtr[AmbSfxPtri++]);
						done = true;
						//AmbSfxTics = P_Random()&(*AmbSfxPtr++);
						//done = true;
						break;
					case afxcmd_t.afxcmd_end:
						AmbSfxTics = 6 * DoomDef.TICSPERSEC + m_misc.P_Random();
						AmbSfxPtr = LevelAmbientSfx[m_misc.P_Random() % AmbSfxCount];
						AmbSfxPtri = 0;
						done = true;
						//AmbSfxTics = 6*TICSPERSEC+P_Random();
						//AmbSfxPtr = LevelAmbientSfx[P_Random()%AmbSfxCount];
						//done = true;
						break;
					default:
						i_ibm.I_Error("P_AmbientSound: Unknown afxcmd " + cmd.ToString());
						break;
				} // LevelAmbientSfx
			} while (done == false);
		}
	}
}
