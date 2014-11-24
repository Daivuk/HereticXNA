using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

///////////////////////////////
// PORT - DONE
///////////////////////////////
		
// P_local.h

namespace HereticXNA
{
	public static class p_local
	{

		public const int STARTREDPALS = 1;
		public const int STARTBONUSPALS = 9;
		public const int NUMREDPALS = 8;
		public const int NUMBONUSPALS = 4;

		public const int FOOTCLIPSIZE = 10 * DoomDef.FRACUNIT;

		public const int TOCENTER = -8;
		public const int FLOATSPEED = (DoomDef.FRACUNIT * 4);

		public const int MAXHEALTH = 100;
		public const int MAXCHICKENHEALTH = 30;
		public const int VIEWHEIGHT = (41 * DoomDef.FRACUNIT);

		// mapblocks are used to check movement against lines and things
		public const int MAPBLOCKUNITS = 128;
		public const int MAPBLOCKSIZE = (MAPBLOCKUNITS * DoomDef.FRACUNIT);
		public const int MAPBLOCKSHIFT = (DoomDef.FRACBITS + 7);
		public const int MAPBMASK = (MAPBLOCKSIZE - 1);
		public const int MAPBTOFRAC = (MAPBLOCKSHIFT - DoomDef.FRACBITS);

		// player radius for movement checking
		public const int PLAYERRADIUS = 16 * DoomDef.FRACUNIT;

		// MAXRADIUS is for precalculated sector block boxes
		// the spider demon is larger, but we don't have any moving sectors
		// nearby
		public const int MAXRADIUS = 32 * DoomDef.FRACUNIT;
		public const int GRAVITY = DoomDef.FRACUNIT;
		public const int MAXMOVE = (30 * DoomDef.FRACUNIT);

		public const int USERANGE = (64 * DoomDef.FRACUNIT);
		public const int MELEERANGE = (64 * DoomDef.FRACUNIT);
		public const int MISSILERANGE = (32 * 64 * DoomDef.FRACUNIT);

		public enum dirtype_t
		{
			DI_EAST,
			DI_NORTHEAST,
			DI_NORTH,
			DI_NORTHWEST,
			DI_WEST,
			DI_SOUTHWEST,
			DI_SOUTH,
			DI_SOUTHEAST,
			DI_NODIR,
			NUMDIRS
		}

		public const int BASETHRESHOLD = 100; // follow a player exlusively for 3 seconds

		// ***** P_TICK *****

		// ***** P_PSPR *****

		public const int USE_GWND_AMMO_1 = 1;
		public const int USE_GWND_AMMO_2 = 1;
		public const int USE_CBOW_AMMO_1 = 1;
		public const int USE_CBOW_AMMO_2 = 1;
		public const int USE_BLSR_AMMO_1 = 1;
		public const int USE_BLSR_AMMO_2 = 5;
		public const int USE_SKRD_AMMO_1 = 1;
		public const int USE_SKRD_AMMO_2 = 5;
		public const int USE_PHRD_AMMO_1 = 1;
		public const int USE_PHRD_AMMO_2 = 1;
		public const int USE_MACE_AMMO_1 = 1;
		public const int USE_MACE_AMMO_2 = 5;

		public const int FLOOR_SOLID = 0;
		public const int FLOOR_WATER = 1;
		public const int FLOOR_LAVA = 2;
		public const int FLOOR_SLUDGE = 3;
		public const int ONFLOORZ = DoomDef.MININT;
		public const int ONCEILINGZ = DoomDef.MAXINT;
		public const int FLOATRANDZ = (DoomDef.MAXINT - 1);

		public class divline_t
		{
			public int x, y, dx, dy;
		}

		public class intercept_t
		{
			public int frac;		// along trace line
			public bool isaline;
			public DoomDef.mobj_t thing;
			public r_local.line_t line;
		} ;

		public const int MAXINTERCEPTS = 128;
		public class traverser_t
		{
			public virtual bool func(intercept_t in_) { return false; }
		}

		public const int PT_ADDLINES = 1;
		public const int PT_ADDTHINGS = 2;
		public const int PT_EARLYOUT = 4;
	}
}
