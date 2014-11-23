using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
		
// P_main.c

namespace HereticXNA
{
	class p_setup
	{
		public static int numvertexes;
		public static r_local.vertex_t[] vertexes;

		public static int numsegs;
		public static r_local.seg_t[] segs;

		public static int numsectors;
		public static r_local.sector_t[] sectors;

		public static int numsubsectors;
		public static r_local.subsector_t[] subsectors;

		public static int numnodes;
		public static r_local.node_t[] nodes;

		public static int numlines;
		public static r_local.line_t[] lines;

		public static int numsides;
		public static r_local.side_t[] sides;
		public static short[] blockmaplump;			// offsets in blockmap are from here
		public static int blockmap;
		public static int bmapwidth, bmapheight;	// in mapblocks
		public static int bmaporgx, bmaporgy;		// origin of block map
		public static DoomDef.mobj_t[] blocklinks;			// for thing chains

		public static byte[] rejectmatrix;			// for fast sight rejection

		public static DoomData.mapthing_t[] deathmatchstarts = new DoomData.mapthing_t[10]{
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t()
};
		public static int deathmatch_pi;
		public static DoomData.mapthing_t[] playerstarts = new DoomData.mapthing_t[DoomDef.MAXPLAYERS] {
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t(),
	new DoomData.mapthing_t()
};

		/*
		=================
		=
		= P_LoadVertexes
		=
		=================
		*/

		public static void P_LoadVertexes(int lump)
		{
			w_wad.CacheInfo data;
			int i;
			numvertexes = w_wad.W_LumpLength(lump) / 4;
			vertexes = new r_local.vertex_t[numvertexes];
			data = w_wad.W_CacheLumpNum(lump, DoomDef.PU_STATIC);
			BinaryReader br = new BinaryReader(new MemoryStream(data.data));
			for (i = 0; i < numvertexes; ++i)
			{
				vertexes[i] = new r_local.vertex_t();
				vertexes[i].x = (int)br.ReadInt16() << DoomDef.FRACBITS;
				vertexes[i].y = (int)br.ReadInt16() << DoomDef.FRACBITS;
			}
		}

		/*
		=================
		=
		= P_LoadSegs
		=
		=================
		*/

		public static void P_LoadSegs(int lump)
		{
			w_wad.CacheInfo data;
			int i, j;
			DoomData.mapseg_t ml;
			r_local.seg_t li, lj;
			r_local.line_t ldef;
			int linedef, side;

			numsegs = w_wad.W_LumpLength(lump) / 12;
			segs = new r_local.seg_t[numsegs];
			data = w_wad.W_CacheLumpNum(lump, DoomDef.PU_STATIC);
			BinaryReader br = new BinaryReader(new MemoryStream(data.data));
			for (i = 0; i < numsegs; i++)
			{
				ml = new DoomData.mapseg_t();
				ml.v1 = br.ReadInt16();
				ml.v2 = br.ReadInt16();
				ml.angle = br.ReadInt16();
				ml.linedef = br.ReadInt16();
				ml.side = br.ReadInt16();
				ml.offset = br.ReadInt16();

				li = new r_local.seg_t();
				li.v1 = vertexes[ml.v1];
				li.v2 = vertexes[ml.v2];

				li.angle = (uint)ml.angle << 16;
				li.offset = ml.offset << 16;
				linedef = ml.linedef;
				ldef = lines[linedef];
				li.linedef = ldef;
				side = ml.side;
				li.sidedef = sides[ldef.sidenum[side]];
				li.frontsector = sides[ldef.sidenum[side]].sector;
				if ((ldef.flags & DoomData.ML_TWOSIDED) != 0)
					li.backsector = sides[ldef.sidenum[side ^ 1]].sector;
				else
					li.backsector = null;
				Vector2 dir =
					new Vector2(li.v2.x >> DoomDef.FRACBITS, li.v2.y >> DoomDef.FRACBITS) -
					new Vector2(li.v1.x >> DoomDef.FRACBITS, li.v1.y >> DoomDef.FRACBITS);
				dir.Normalize();
				li.normal.X = dir.Y;
				li.normal.Y = -dir.X;
				li.dir = dir;
				segs[i] = li;
			}

			// [dsl] Search for segment neighbors. We do this for the ambient occlusion parts
			for (i = 0; i < numsegs; i++)
			{
				li = segs[i];
				if (li.frontsector == li.backsector) continue;
				for (j = 0; j < numsegs; j++)
				{
					if (i == j) continue;
					lj = segs[j];
					if (lj.frontsector == lj.backsector) continue;
					if (Vector2.Dot(li.normal, lj.normal) < 0) continue;
					if (lj.v2.x == li.v1.x &&
						lj.v2.y == li.v1.y)
					{
						if (Vector2.Dot(lj.normal, li.dir) > 0)
						{
							li.left.Add(lj);
						}
					}
					if (lj.v1.x == li.v2.x &&
						lj.v1.y == li.v2.y)
					{
						if (Vector2.Dot(li.normal, lj.dir) > 0)
						{
							li.right.Add(lj);
						}
					}
				}
			}
		}

		/*
		=================
		=
		= P_LoadSubsectors
		=
		=================
		*/

		public static void P_LoadSubsectors(int lump)
		{
			w_wad.CacheInfo data;
			int i;
			DoomData.mapsubsector_t ms;
			r_local.subsector_t ss;

			numsubsectors = w_wad.W_LumpLength(lump) / 4;
			subsectors = new r_local.subsector_t[numsubsectors];
			data = w_wad.W_CacheLumpNum(lump, DoomDef.PU_STATIC);
			BinaryReader br = new BinaryReader(new MemoryStream(data.data));

			for (i = 0; i < numsubsectors; i++)
			{
				ms = new DoomData.mapsubsector_t();
				ms.numsegs = br.ReadInt16();
				ms.firstseg = br.ReadInt16();

				ss = new r_local.subsector_t();
				ss.numlines = ms.numsegs;
				ss.firstline = ms.firstseg;
				subsectors[i] = ss;
			}
		}


		/*
		=================
		=
		= P_LoadSectors
		=
		=================
		*/

		public static void P_LoadSectors(int lump)
		{
			w_wad.CacheInfo data;
			int i;
			DoomData.mapsector_t ms;
			r_local.sector_t ss;

			numsectors = w_wad.W_LumpLength(lump) / 26;
			if (sectors != null)
			{
				foreach (r_local.sector_t sector in sectors)
				{
					sector.Dispose();
				}
			}
			sectors = new r_local.sector_t[numsectors];
			data = w_wad.W_CacheLumpNum(lump, DoomDef.PU_STATIC);

			byte[] bytes;
			BinaryReader br = new BinaryReader(new MemoryStream(data.data));
			for (i = 0; i < numsectors; i++)
			{
				ms = new DoomData.mapsector_t();
				ms.floorheight = br.ReadInt16();
				ms.ceilingheight = br.ReadInt16();
				bytes = br.ReadBytes(8);
				foreach (byte b in bytes) { char c = (char)b; if (c == '\0') continue; ms.floorpic += c; }
				bytes = br.ReadBytes(8);
				foreach (byte b in bytes) { char c = (char)b; if (c == '\0') continue; ms.ceilingpic += c; }
				ms.lightlevel = br.ReadInt16();
				ms.special = br.ReadInt16();
				ms.tag = br.ReadInt16();

				ss = new r_local.sector_t();
				ss.floorheight = ms.floorheight << DoomDef.FRACBITS;
				ss.ceilingheight = ms.ceilingheight << DoomDef.FRACBITS;
				ss.floorpic = (short)r_data.R_FlatNumForName(ms.floorpic);
				ss.ceilingpic = (short)r_data.R_FlatNumForName(ms.ceilingpic);
				ss.lightlevel = ms.lightlevel;
				ss.special = ms.special;
				ss.tag = ms.tag;
				ss.thinglist = null;
				sectors[i] = ss;
			}
		}

		/*
		=================
		=
		= P_LoadNodes
		=
		=================
		*/

		/*
public short x, y, dx, dy;	8
public short[,] bbox = new short[2, 4];		16
public ushort[] children = new ushort[2]; 4
		*/
		public static void P_LoadNodes(int lump)
		{
			w_wad.CacheInfo data;
			int i, j, k;
			DoomData.mapnode_t mn;
			r_local.node_t no;

			numnodes = w_wad.W_LumpLength(lump) / 28;
			nodes = new r_local.node_t[numnodes];
			data = w_wad.W_CacheLumpNum(lump, DoomDef.PU_STATIC);
			BinaryReader br = new BinaryReader(new MemoryStream(data.data));

			for (i = 0; i < numnodes; i++)
			{
				mn = new DoomData.mapnode_t();
				mn.x = br.ReadInt16();
				mn.y = br.ReadInt16();
				mn.dx = br.ReadInt16();
				mn.dy = br.ReadInt16();
				mn.bbox[0, 0] = br.ReadInt16();
				mn.bbox[0, 1] = br.ReadInt16();
				mn.bbox[0, 2] = br.ReadInt16();
				mn.bbox[0, 3] = br.ReadInt16();
				mn.bbox[1, 0] = br.ReadInt16();
				mn.bbox[1, 1] = br.ReadInt16();
				mn.bbox[1, 2] = br.ReadInt16();
				mn.bbox[1, 3] = br.ReadInt16();
				mn.children[0] = br.ReadUInt16();
				mn.children[1] = br.ReadUInt16();

				no = new r_local.node_t();
				no.x = mn.x << DoomDef.FRACBITS;
				no.y = mn.y << DoomDef.FRACBITS;
				no.dx = mn.dx << DoomDef.FRACBITS;
				no.dy = mn.dy << DoomDef.FRACBITS;
				for (j = 0; j < 2; j++)
				{
					no.children[j] = mn.children[j];
					for (k = 0; k < 4; k++)
						no.bbox[j][k] = mn.bbox[j, k] << DoomDef.FRACBITS;
				}
				nodes[i] = no;
			}
		}



		/*
		=================
		=
		= P_LoadThings
		=
		=================
		*/

		public static void P_LoadThings(int lump)
		{
			w_wad.CacheInfo data;
			int i;
			DoomData.mapthing_t mt;
			int numthings;

			data = w_wad.W_CacheLumpNum(lump, DoomDef.PU_STATIC);
			numthings = w_wad.W_LumpLength(lump) / 10;

			BinaryReader br = new BinaryReader(new MemoryStream(data.data));
			for (i = 0; i < numthings; i++)
			{
				mt = new DoomData.mapthing_t();
				mt.x = br.ReadInt16();
				mt.y = br.ReadInt16();
				mt.angle = br.ReadInt16();
				mt.type = br.ReadInt16();
				mt.options = br.ReadInt16();

				p_mobj.P_SpawnMapThing(mt);
			}
		}


		/*
		=================
		=
		= P_LoadLineDefs
		=
		= Also counts secret lines for intermissions [dsl] No it doesn't?
		=================
		*/

		public static void P_LoadLineDefs(int lump)
		{
			w_wad.CacheInfo data;
			int i;
			DoomData.maplinedef_t mld;
			r_local.line_t ld;
			r_local.vertex_t v1, v2;

			numlines = w_wad.W_LumpLength(lump) / 14;
			lines = new r_local.line_t[numlines];
			data = w_wad.W_CacheLumpNum(lump, DoomDef.PU_STATIC);
			BinaryReader br = new BinaryReader(new MemoryStream(data.data));
			for (i = 0; i < numlines; i++)
			{
				mld = new DoomData.maplinedef_t();
				mld.v1 = br.ReadInt16();
				mld.v2 = br.ReadInt16();
				mld.flags = br.ReadInt16();
				mld.special = br.ReadInt16();
				mld.tag = br.ReadInt16();
				mld.sidenum[0] = br.ReadInt16();
				mld.sidenum[1] = br.ReadInt16();

				ld = new r_local.line_t();
				ld.flags = mld.flags;
				ld.special = mld.special;
				ld.tag = mld.tag;
				v1 = ld.v1 = vertexes[mld.v1];
				v2 = ld.v2 = vertexes[mld.v2];
				ld.dx = v2.x - v1.x;
				ld.dy = v2.y - v1.y;
				if (ld.dx == 0)
					ld.slopetype = r_local.slopetype_t.ST_VERTICAL;
				else if (ld.dy == 0)
					ld.slopetype = r_local.slopetype_t.ST_HORIZONTAL;
				else
				{
					if (d_main.FixedDiv(ld.dy, ld.dx) > 0)
						ld.slopetype = r_local.slopetype_t.ST_POSITIVE;
					else
						ld.slopetype = r_local.slopetype_t.ST_NEGATIVE;
				}

				if (v1.x < v2.x)
				{
					ld.bbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] = v1.x;
					ld.bbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] = v2.x;
				}
				else
				{
					ld.bbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] = v2.x;
					ld.bbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] = v1.x;
				}
				if (v1.y < v2.y)
				{
					ld.bbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] = v1.y;
					ld.bbox[(int)DoomData.eUnknownEnumType2.BOXTOP] = v2.y;
				}
				else
				{
					ld.bbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] = v2.y;
					ld.bbox[(int)DoomData.eUnknownEnumType2.BOXTOP] = v1.y;
				}
				ld.sidenum[0] = (mld.sidenum[0]);
				ld.sidenum[1] = (mld.sidenum[1]);
				if (ld.sidenum[0] != -1)
					ld.frontsector = sides[ld.sidenum[0]].sector;
				else
					ld.frontsector = null;
				if (ld.sidenum[1] != -1)
					ld.backsector = sides[ld.sidenum[1]].sector;
				else
					ld.backsector = null;
				lines[i] = ld;
			}
		}


		/*
		=================
		=
		= P_LoadSideDefs
		=
		=================
		*/
		public static void P_LoadSideDefs(int lump)
		{
			w_wad.CacheInfo data;
			int i;
			DoomData.mapsidedef_t msd;
			r_local.side_t sd;

			numsides = w_wad.W_LumpLength(lump) / 30;
			sides = new r_local.side_t[numsides];
			data = w_wad.W_CacheLumpNum(lump, DoomDef.PU_STATIC);
			byte[] bytes;
			BinaryReader br = new BinaryReader(new MemoryStream(data.data));

			for (i = 0; i < numsides; i++)
			{
				msd = new DoomData.mapsidedef_t();
				msd.textureoffset = br.ReadInt16();
				msd.rowoffset = br.ReadInt16();
				bytes = br.ReadBytes(8);
				foreach (byte b in bytes) { char c = (char)b; if (c == '\0') continue; msd.toptexture += c; }
				bytes = br.ReadBytes(8);
				foreach (byte b in bytes) { char c = (char)b; if (c == '\0') continue; msd.bottomtexture += c; }
				bytes = br.ReadBytes(8);
				foreach (byte b in bytes) { char c = (char)b; if (c == '\0') continue; msd.midtexture += c; }
				msd.sector = br.ReadInt16();

				sd = new r_local.side_t();
				sd.textureoffset = (msd.textureoffset) << DoomDef.FRACBITS;
				sd.rowoffset = (msd.rowoffset) << DoomDef.FRACBITS;
				sd.toptexture = (short)r_data.R_TextureNumForName(msd.toptexture);
				sd.bottomtexture = (short)r_data.R_TextureNumForName(msd.bottomtexture);
				sd.midtexture = (short)r_data.R_TextureNumForName(msd.midtexture);
				sd.sector = sectors[msd.sector];
				sides[i] = sd;
			}
		}


		/*
		=================
		=
		= P_LoadBlockMap
		=
		=================
		*/

		public static void P_LoadBlockMap(int lump)
		{
			int i, count;

			w_wad.CacheInfo cache = w_wad.W_CacheLumpNum(lump, DoomDef.PU_LEVEL);
			blockmap = 4;//blockmaplump + 4;
			count = w_wad.W_LumpLength(lump) / 2;
			blockmaplump = new short[count];
			BinaryReader br = new BinaryReader(new MemoryStream(cache.data));
			for (i = 0; i < count; i++)
			{
				blockmaplump[i] = br.ReadInt16();
			}

			bmaporgx = blockmaplump[0] << DoomDef.FRACBITS;
			bmaporgy = blockmaplump[1] << DoomDef.FRACBITS;
			bmapwidth = blockmaplump[2];
			bmapheight = blockmaplump[3];


			// clear out mobj chains
			count = bmapwidth * bmapheight;
			blocklinks = new DoomDef.mobj_t[count];
		}



		/*
		=================
		=
		= P_GroupLines
		=
		= Builds sector line lists and subsector sector numbers
		= Finds block bounding boxes for sectors
		=================
		*/

		public static r_local.line_t[] linebuffer; // [dsl] In our case this has to be global, because each sectors reference it with an index

		public static void P_GroupLines()
		{
			int i, j, total;
			r_local.line_t li;
			r_local.sector_t sector;
			r_local.subsector_t ss;
			r_local.seg_t seg;
			int[] bbox = new int[4];
			int block;

			// look up sector number for each subsector
			for (i = 0; i < numsubsectors; i++)
			{
				ss = subsectors[i];
				seg = segs[ss.firstline];
				ss.sector = seg.sidedef.sector;
			}

			// count number of lines in each sector
			total = 0;
			for (i = 0; i < numlines; i++)
			{
				li = lines[i];
				total++;
				li.frontsector.linecount++;
				if (li.backsector != null && li.backsector != li.frontsector)
				{
					li.backsector.linecount++;
					total++;
				}
			}

			// build line tables for each sector	
			linebuffer = new r_local.line_t[total];
			int linebufferi = 0;
			for (i = 0; i < numsectors; i++)
			{
				sector = sectors[i];
				m_misc.M_ClearBox(bbox);
				sector.linesi = linebufferi;
				for (j = 0; j < numlines; j++)
				{
					li = lines[j];
					if (li.frontsector == sector || li.backsector == sector)
					{
						linebuffer[linebufferi++] = li;
						m_misc.M_AddToBox(bbox, li.v1.x, li.v1.y);
						m_misc.M_AddToBox(bbox, li.v2.x, li.v2.y);
					}
				}
				if (linebufferi - sector.linesi != sector.linecount)
					i_ibm.I_Error("P_GroupLines: miscounted");

				// set the degenmobj_t to the middle of the bounding box
				sector.soundorg.x = (bbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] + bbox[(int)DoomData.eUnknownEnumType2.BOXLEFT]) / 2;
				sector.soundorg.y = (bbox[(int)DoomData.eUnknownEnumType2.BOXTOP] + bbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM]) / 2;

				// adjust bounding box to map blocks
				block = (bbox[(int)DoomData.eUnknownEnumType2.BOXTOP] - bmaporgy + p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
				block = block >= bmapheight ? bmapheight - 1 : block;
				sector.blockbox[(int)DoomData.eUnknownEnumType2.BOXTOP] = block;

				block = (bbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] - bmaporgy - p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
				block = block < 0 ? 0 : block;
				sector.blockbox[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] = block;

				block = (bbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] - bmaporgx + p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
				block = block >= bmapwidth ? bmapwidth - 1 : block;
				sector.blockbox[(int)DoomData.eUnknownEnumType2.BOXRIGHT] = block;

				block = (bbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] - bmaporgx - p_local.MAXRADIUS) >> p_local.MAPBLOCKSHIFT;
				block = block < 0 ? 0 : block;
				sector.blockbox[(int)DoomData.eUnknownEnumType2.BOXLEFT] = block;
			}
		}

		//=============================================================================

		/*
		=================
		=
		= P_SetupLevel
		=
		=================
		*/

		public static void P_SetupLevel(int episode, int map, int playermask, DoomDef.skill_t skill)
		{
			int i;
			int parm;
			string lumpname;
			int lumpnum;
			DoomDef.mobj_t mobj;

			g_game.totalkills = g_game.totalitems = g_game.totalsecret = 0;
			for (i = 0; i < DoomDef.MAXPLAYERS; i++)
			{
				g_game.players[i].killcount = g_game.players[i].secretcount
				= g_game.players[i].itemcount = 0;
			}
			g_game.players[g_game.consoleplayer].viewz = 1; // will be set by player think

			i_ibm.S_Start();			// make sure all sounds are stopped before Z_FreeTags

			//Z_FreeTags (PU_LEVEL, PU_PURGELEVEL-1);

			p_tick.P_InitThinkers();

			//
			// look for a regular (development) map first
			//
			lumpname = "E" + episode + "M" + map;

			p_tick.leveltime = 0;

			lumpnum = w_wad.W_GetNumForName(lumpname);

			// note: most of this ordering is important	
			P_LoadBlockMap(lumpnum + (int)DoomData.eUnknownEnumType1.ML_BLOCKMAP);
			P_LoadVertexes(lumpnum + (int)DoomData.eUnknownEnumType1.ML_VERTEXES);
			P_LoadSectors(lumpnum + (int)DoomData.eUnknownEnumType1.ML_SECTORS);
			P_LoadSideDefs(lumpnum + (int)DoomData.eUnknownEnumType1.ML_SIDEDEFS);

			P_LoadLineDefs(lumpnum + (int)DoomData.eUnknownEnumType1.ML_LINEDEFS);
			P_LoadSubsectors(lumpnum + (int)DoomData.eUnknownEnumType1.ML_SSECTORS);
			P_LoadNodes(lumpnum + (int)DoomData.eUnknownEnumType1.ML_NODES);
			P_LoadSegs(lumpnum + (int)DoomData.eUnknownEnumType1.ML_SEGS);

			rejectmatrix = w_wad.W_CacheLumpNum(lumpnum + (int)DoomData.eUnknownEnumType1.ML_REJECT, DoomDef.PU_LEVEL).data;
			P_GroupLines();

			p_enemy.bodyqueslot = 0;
			deathmatch_pi = 0;
			p_spec.P_InitAmbientSound();
			p_enemy.P_InitMonsters();
			p_pspr.P_OpenWeapons();
			p_setup.P_LoadThings(lumpnum + (int)DoomData.eUnknownEnumType1.ML_THINGS);
			p_pspr.P_CloseWeapons();

			//
			// if deathmatch, randomly spawn the active players
			//
			p_tick.TimerGame = 0;
			if (g_game.deathmatch)
			{
				for (i = 0; i < DoomDef.MAXPLAYERS; i++)
				{
					if (g_game.playeringame[i])
					{	// must give a player spot before deathmatchspawn
						mobj = p_mobj.P_SpawnMobj(playerstarts[i].x << 16,
						playerstarts[i].y << 16, 0, info.mobjtype_t.MT_PLAYER);
						g_game.players[i].mo = mobj;
						g_game.G_DeathMatchSpawnPlayer(i);
						p_mobj.P_RemoveMobj(mobj);
					}
				}
				parm = m_misc.M_CheckParm("-timer");
				if (parm != 0 && parm < m_misc.args.Length - 1)
				{
					p_tick.TimerGame = int.Parse(m_misc.args[parm + 1]) * 35 - 60;
				}
			}

			// set up world state
			p_spec.P_SpawnSpecials();

			// preload graphics
			if (g_game.precache)
				r_data.R_PrecacheLevel();
		}


		/*
		=================
		=
		= P_Init
		=
		=================
		*/

		public static void P_Init()
		{
			p_switch.P_InitSwitchList();
			p_spec.P_InitPicAnims();
			p_spec.P_InitTerrainTypes();
			p_spec.P_InitLava();
			r_thing.R_InitSprites(info.sprnames);
		}
	}
}
