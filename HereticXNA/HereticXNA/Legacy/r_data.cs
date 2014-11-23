using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

// R_data.c

namespace HereticXNA
{
	static public class r_data
	{

		public class texpatch_t
		{
			public int originx;	// block origin (allways UL), which has allready
			public int originy;	// accounted  for the patch's internal origin
			public int patch;
		}

		// a maptexturedef_t describes a rectangular texture, which is composed of one
		// or more mappatch_t structures that arrange graphic patches
		public class texture_t
		{
			public string name;		// for switch changing, etc
			public short width;
			public short height;
			public short patchcount;
			public texpatch_t[] patches = new texpatch_t[] { new texpatch_t() };		// [patchcount] drawn back to front
			//  into the cached texture
		}



		public static int firstflat, lastflat, numflats;
		public static int firstpatch, lastpatch, numpatches;
		public static int firstspritelump, lastspritelump, numspritelumps;

		public static int numtextures;
		public static texture_t[] textures;
		public static int[] texturewidthmask;
		public static int[] textureheight;		// needed for texture pegging
		public static int[] texturecompositesize;
		public static short[][] texturecolumnlump;
		public static ushort[][] texturecolumnofs;
		public static byte[][] texturecomposite;

		public static int[] flattranslation;		// for global animation
		public static int[] flattranslationPrev;		// for global animation
		public static float[] flattranslationDeltas;
		public static int[] texturetranslation;	// for global animation
		public static int[] texturetranslationPrev;	// for global animation
		public static float[] texturetranslationDeltas;

		public static int[] spritewidth;		// needed for pre rendering
		public static int[] spriteoffset;
		public static int[] spritetopoffset;

		public static byte[] colormaps;
#if DOS


/*
==============================================================================

						MAPTEXTURE_T CACHING

when a texture is first needed, it counts the number of composite columns
required in the texture and allocates space for a column directory and any
new columns.  The directory will simply point inside other patches if there
is only one patch in a given column, but any columns with multiple patches
will have new column_ts generated.

==============================================================================
*/
#endif
		/*
===================
=
= R_DrawColumnInCache
=
= Clip and draw a column from a patch into a cached post
=
===================
*/
		public static void R_DrawColumnInCache(byte[] src, int srcStart, DoomData.post_t patch, int col, Color[] cache, int originy, int cachewidth, int cacheheight)
		{
			R_DrawColumnInCache(src, srcStart, patch, col, cache, originy, cachewidth, cacheheight, 0, 0);
		}

		public static void R_DrawColumnInCache(byte[] src, int srcStart, DoomData.post_t patch, int col, Color[] cache, int originy, int cachewidth, int cacheheight, int offsetX, int offsetY)
		{
			int count, position;
			int source;
			int dest = 3;

			w_wad.CacheInfo palette = w_wad.W_CacheLumpName("PLAYPAL", DoomDef.PU_CACHE);

			while (patch.topdelta != 0xff)
			{
				patch.length = src[srcStart + 1];
				source = srcStart + 3;
				count = patch.length;
				position = originy + patch.topdelta;
				if (position < 0)
				{
					count += position;
					position = 0;
				}
				if (position + count > cacheheight)
					count = cacheheight - position;
				if (count > 0)
				{
					for (int i = source, j = 0; i < source + count; ++i, ++j)
					{
						int component = (int)src[i];
						cache[col + offsetX + (position + j + offsetY) * cachewidth] = new Color(
							palette.data[component * 3 + 0],
							palette.data[component * 3 + 1],
							palette.data[component * 3 + 2]);
					}
				}

				srcStart += patch.length + 4;
				patch.topdelta = src[srcStart];
			}
		}

		/*
		===================
		=
		= R_GenerateComposite
		=
		===================
		*/

		public static void R_GenerateComposite(int texnum)
		{
			texture_t texture;
			texpatch_t[] patch;
			DoomData.patch_t realpatch;
			int x, x1, x2;
			int i;
			DoomData.post_t patchcol = new DoomData.post_t();
			short[] collump;
			ushort[] colofs;

			texture = textures[texnum];
			Color[] xnaTextureData = new Color[texture.width * texture.height];
			collump = texturecolumnlump[texnum];
			colofs = texturecolumnofs[texnum];

			//
			// composite the columns together
			//
			patch = texture.patches;

			for (i = 0; i < texture.patchcount; i++)
			{
				byte[] data = w_wad.W_CacheLumpNum(patch[i].patch, DoomDef.PU_CACHE).data;
				BinaryReader bs = new BinaryReader(new MemoryStream(data));
				realpatch = new DoomData.patch_t();
				realpatch.width = bs.ReadInt16();
				realpatch.height = bs.ReadInt16();
				if (realpatch.height > texture.height)
				{
					Color[] newXnaTextureData = new Color[texture.width * realpatch.height];
					Array.Copy(xnaTextureData, newXnaTextureData, texture.width * texture.height);
					texture.height = realpatch.height;
					xnaTextureData = newXnaTextureData;
				}
				realpatch.leftoffset = bs.ReadInt16();
				realpatch.topoffset = bs.ReadInt16();
				realpatch.columnofs = new int[realpatch.width];
				for (int k = 0; k < realpatch.width; ++k) realpatch.columnofs[k] = bs.ReadInt32();

				x1 = patch[i].originx;
				x2 = x1 + (int)realpatch.width;

				if (x1 < 0)
					x = 0;
				else
					x = x1;
				if (x2 > texture.width)
					x2 = texture.width;

				for (; x < x2; x++)
				{
					//	if (collump[x] >= 0)
					//		continue;		// column does not have multiple patches
					patchcol = new DoomData.post_t();
					patchcol.topdelta = data[realpatch.columnofs[x - x1]];
					patchcol.length = data[realpatch.columnofs[x - x1] + 1];

					R_DrawColumnInCache(data, realpatch.columnofs[x - x1], patchcol, x/*, colofs[x]*/, xnaTextureData, patch[i].originy, texture.width, texture.height);
				}
			}

			Texture2D xnaTexture = new Texture2D(Game1.instance.GraphicsDevice, texture.width, texture.height, true, SurfaceFormat.Color);
			xnaTexture.SetData(0, null, xnaTextureData, 0, texture.width * texture.height);
			GenerateMipMaps(Game1.instance.GraphicsDevice, Game1.instance.spriteBatch, ref xnaTexture);
			Game1.instance.allTextures.Add(xnaTexture);
			Game1.instance.wallTextures.Add(xnaTexture);
			Game1.instance.wallTexturesById[texnum] = xnaTexture;
		}

		/// Create mipmaps for a texture
		/// See: http://xboxforums.create.msdn.com/forums/p/60738/377862.aspx
		/// Make sure this is only called from the main thread or 
		/// when drawing is not already happening.
		public static void GenerateMipMaps(
		  GraphicsDevice graphicsDevice,
		  SpriteBatch spriteBatch,
		  ref Texture2D inOutImage)
		{
			RenderTarget2D target =
			  new RenderTarget2D(
				graphicsDevice,
				inOutImage.Width,
				inOutImage.Height,
				true,
				SurfaceFormat.Color,
				DepthFormat.None);
			graphicsDevice.SetRenderTarget(target);
			graphicsDevice.Clear(Color.Transparent);
			spriteBatch.Begin();
			spriteBatch.Draw(inOutImage, Vector2.Zero, Color.White);
			spriteBatch.End();
			graphicsDevice.SetRenderTarget(null);
			inOutImage.Dispose();
			inOutImage = (Texture2D)target;
		}

		/*
		===================
		=
		= R_GenerateLookup
		=
		===================
		*/

		public static void R_GenerateLookup(int texnum)
		{
			texture_t texture;
			byte[] patchcount;		// [texture.width]
			DoomData.patch_t realpatch;
			int x, x1, x2;
			int i;
			short[] collump;
			ushort[] colofs;

			texture = textures[texnum];

	/*		if (texture.name == "SKY1")
			{
				texture.height = 256;
			}*/

			texturecomposite[texnum] = null;	// composited not created yet
			texturecompositesize[texnum] = 0;
			collump = texturecolumnlump[texnum];
			colofs = texturecolumnofs[texnum];

			//
			// count the number of columns that are covered by more than one patch
			// fill in the lump / offset, so columns with only a single patch are
			// all done
			//
			patchcount = new byte[texture.width];
			for (i = 0; i < texture.patchcount; i++)
			{
				BinaryReader bs = new BinaryReader(new MemoryStream(w_wad.W_CacheLumpNum(texture.patches[i].patch, DoomDef.PU_CACHE).data));
				realpatch = new DoomData.patch_t();
				realpatch.width = bs.ReadInt16();
				realpatch.height = bs.ReadInt16();
				realpatch.leftoffset = bs.ReadInt16();
				realpatch.topoffset = bs.ReadInt16();
				realpatch.columnofs = new int[realpatch.width];
				for (int k = 0; k < realpatch.width; ++k) realpatch.columnofs[k] = bs.ReadInt32();

				x1 = texture.patches[i].originx;
				x2 = x1 + realpatch.width;
				if (x1 < 0)
					x = 0;
				else
					x = x1;
				if (x2 > texture.width)
					x2 = texture.width;
				for (; x < x2; x++)
				{
					patchcount[x]++;
					collump[x] = (short)texture.patches[i].patch;
					colofs[x] = (ushort)(realpatch.columnofs[x - x1] + 3);
				}
			}

			for (x = 0; x < texture.width; x++)
			{
				if (patchcount[x] == 0)
				{
					Console.Write("R_GenerateLookup: column without a patch (" + texture.name + ")\n");
					return;
				}

				if (patchcount[x] > 1)
				{
					collump[x] = -1;	// use the cached block
					colofs[x] = (ushort)texturecompositesize[texnum];
					if (texturecompositesize[texnum] > 0x10000 - texture.height)
						i_ibm.I_Error("R_GenerateLookup: texture " + texnum + " is >64k");
					texturecompositesize[texnum] += texture.height;
				}
			}

			R_GenerateComposite(texnum);
		}
#if DOS

/*
================
=
= R_GetColumn
=
================
*/

byte *R_GetColumn (int tex, int col)
{
	int	lump, ofs;
	
	col &= texturewidthmask[tex];
	lump = texturecolumnlump[tex][col];
	ofs = texturecolumnofs[tex][col];
	if (lump > 0)
		return (byte *)W_CacheLumpNum(lump,PU_CACHE)+ofs;
	if (!texturecomposite[tex])
		R_GenerateComposite (tex);
	return texturecomposite[tex] + ofs;
}

#endif
		/*
==================
=
= R_InitTextures
=
= Initializes the texture list with the textures from the world map
=
==================
*/

		public static void R_InitTextures()
		{
			DoomData.maptexture_t mtexture;
			texture_t texture;
			int i, j;
			w_wad.CacheInfo maptex, maptex2, maptex1;
			string name;
			w_wad.CacheInfo names;
			int name_p;
			int[] patchlookup;
			int totalwidth;
			int nummappatches;
			int offset, maxoff, maxoff2;
			int numtextures1, numtextures2;
			int directory;

			//
			// load the patch names from pnames.lmp
			//
			name = "";
			names = w_wad.W_CacheLumpName("PNAMES", DoomDef.PU_STATIC);
			BinaryReader bs = new BinaryReader(new MemoryStream(names.data));

			nummappatches = bs.ReadInt32();
			name_p = 4;
			patchlookup = new int[nummappatches];
			for (i = 0; i < nummappatches; i++)
			{
				name = "";
				for (int k = 0; k < 8; ++k)
				{
					char c = (char)(names.data[name_p + i * 8 + k]);
					if (c != '\0') name += c;
				}
				patchlookup[i] = w_wad.W_CheckNumForName(name);
			}
			names = null;

			//
			// load the map texture definitions from textures.lmp
			//
			maptex = maptex1 = w_wad.W_CacheLumpName("TEXTURE1", DoomDef.PU_STATIC);
			bs = new BinaryReader(new MemoryStream(maptex.data));
			BinaryReader bs2 = null;
			numtextures1 = bs.ReadInt32();
			maxoff = w_wad.W_LumpLength(w_wad.W_GetNumForName("TEXTURE1"));
			directory = 4;

			if (w_wad.W_CheckNumForName("TEXTURE2") != -1)
			{
				maptex2 = w_wad.W_CacheLumpName("TEXTURE2", DoomDef.PU_STATIC);
				bs2 = new BinaryReader(new MemoryStream(maptex2.data));
				numtextures2 = bs2.ReadInt32();
				maxoff2 = w_wad.W_LumpLength(w_wad.W_GetNumForName("TEXTURE2"));
			}
			else
			{
				maptex2 = null;
				numtextures2 = 0;
				maxoff2 = 0;
			}
			numtextures = numtextures1 + numtextures2;

			//
			//	Init the startup thermometer at this point...
			//
			{
				int spramount;
				spramount = w_wad.W_GetNumForName("S_END") - w_wad.W_GetNumForName("S_START") + 1;
				d_main.InitThermo(spramount + numtextures + 6);
			}

			r_data.textures = new texture_t[numtextures];
			texturecolumnlump = new short[numtextures][];
			texturecolumnofs = new ushort[numtextures][];
			texturecomposite = new byte[numtextures][];
			texturecompositesize = new int[numtextures];
			texturewidthmask = new int[numtextures];
			textureheight = new int[numtextures];

			totalwidth = 0;
			bs = new BinaryReader(new MemoryStream(maptex.data));
			for (i = 0; i < numtextures; i++, directory += 4)
			{
				d_main.IncThermo();
				if (i == numtextures1)
				{	// start looking in second texture file
					maptex = maptex2;
					maxoff = maxoff2;
					directory = 4;
					bs = new BinaryReader(new MemoryStream(maptex.data));
				}

				bs.BaseStream.Seek(directory, SeekOrigin.Begin);
				offset = bs.ReadInt32();
				if (offset > maxoff)
					i_ibm.I_Error("R_InitTextures: bad texture directory");

				bs.BaseStream.Seek(offset, SeekOrigin.Begin);
				mtexture = new DoomData.maptexture_t();
				for (int k = 0; k < 8; ++k) { char c = (char)bs.ReadByte(); if (c != '\0') mtexture.name += c; }
				mtexture.masked = bs.ReadInt32() != 0;
				mtexture.width = bs.ReadInt16();
				mtexture.height = bs.ReadInt16();
				bs.ReadInt32(); // OBSOLETE, pointer 32 bits
				mtexture.patchcount = bs.ReadInt16();
				mtexture.patches = new DoomData.mappatch_t[mtexture.patchcount];
				for (j = 0; j < mtexture.patchcount; j++)
				{
					mtexture.patches[j] = new DoomData.mappatch_t();
					mtexture.patches[j].originx = bs.ReadInt16();
					mtexture.patches[j].originy = bs.ReadInt16();
					mtexture.patches[j].patch = bs.ReadInt16();
					mtexture.patches[j].stepdir = bs.ReadInt16();
					mtexture.patches[j].colormap = bs.ReadInt16();
				}

				texture = textures[i] = new texture_t();
				texture.patches = new texpatch_t[mtexture.patchcount];
				texture.width = mtexture.width;
				texture.height = mtexture.height;
				texture.patchcount = mtexture.patchcount;
				texture.name = mtexture.name;
				for (j = 0; j < texture.patchcount; j++)
				{
					texture.patches[j] = new texpatch_t();
					texture.patches[j].originx = mtexture.patches[j].originx;
					texture.patches[j].originy = mtexture.patches[j].originy;
					texture.patches[j].patch = patchlookup[mtexture.patches[j].patch];
					if (texture.patches[j].patch == -1)
						i_ibm.I_Error("R_InitTextures: Missing patch in texture " + texture.name);
				}
				texturecolumnlump[i] = new short[texture.width];
				texturecolumnofs[i] = new ushort[texture.width];
				j = 1;
				while (j * 2 <= texture.width)
					j <<= 1;
				texturewidthmask[i] = j - 1;
				textureheight[i] = texture.height << DoomDef.FRACBITS;

				totalwidth += texture.width;
			}

			maptex1 = null; // We don't really free anything here

			if (maptex2 != null)
				maptex2 = null; // We don't really free anything here

			//
			// precalculate whatever possible
			//		
			for (i = 0; i < numtextures; i++)
			{
				R_GenerateLookup(i);
				d_main.CheckAbortStartup();
			}

			//
			// translation table for global animation
			//
			texturetranslation = new int[numtextures + 1];
			texturetranslationPrev = new int[numtextures + 1];
			texturetranslationDeltas = new float[numtextures + 1];
			for (i = 0; i < numtextures; i++)
			{
				texturetranslation[i] = i;
				texturetranslationPrev[i] = i;
				texturetranslationDeltas[i] = 0;
			}
		}

		/*
		================
		=
		= R_InitFlats
		=
		=================
		*/

		public static void R_InitFlats()
		{
			int i;

			firstflat = w_wad.W_GetNumForName("F_START") + 1;
			lastflat = w_wad.W_GetNumForName("F_END") - 1;
			numflats = lastflat - firstflat + 1;

			// translation table for global animation
			flattranslation = new int[numflats + 1];
			flattranslationPrev = new int[numflats + 1];
			flattranslationDeltas = new float[numflats + 1];
			w_wad.CacheInfo palette = w_wad.W_CacheLumpName("PLAYPAL", DoomDef.PU_CACHE);
			for (i = 1; i < numflats - 1; i++)
			{
				flattranslation[i] = i;
				flattranslationPrev[i] = i;
				flattranslationDeltas[i] = 0;
				w_wad.CacheInfo cache = w_wad.W_CacheLumpNum(firstflat + i, DoomDef.PU_CACHE);
				if (cache.data.Length < 64 * 64) continue;
				Texture2D xnaTexture = new Texture2D(Game1.instance.GraphicsDevice, 64, 64);
				Color[] xnaTextureData = new Color[64 * 64];
				for (int j = 0; j < xnaTextureData.Length; ++j)
				{
					int component = cache.data[j];
					xnaTextureData[j] = new Color(
							palette.data[component * 3 + 0],
							palette.data[component * 3 + 1],
							palette.data[component * 3 + 2]);
				}
				xnaTexture.SetData(xnaTextureData);
				GenerateMipMaps(Game1.instance.GraphicsDevice, Game1.instance.spriteBatch, ref xnaTexture);
				cache.cache = xnaTexture;
				Game1.instance.allTextures.Add(xnaTexture);
				Game1.instance.floorTextures.Add(xnaTexture);
				Game1.instance.floorTexturesById[i] = xnaTexture;
			}
		}


		/*
		================
		=
		= R_InitSpriteLumps
		=
		= Finds the width and hoffset of all sprites in the wad, so the sprite doesn't
		= need to be cached just for the header during rendering
		=================
		*/

		public static void R_InitSpriteLumps()
		{
			int i;
			DoomData.patch_t patch;

			firstspritelump = w_wad.W_GetNumForName("S_START") + 1;
			lastspritelump = w_wad.W_GetNumForName("S_END") - 1;
			numspritelumps = lastspritelump - firstspritelump + 1;
			spritewidth = new int[numspritelumps];
			spriteoffset = new int[numspritelumps];
			spritetopoffset = new int[numspritelumps];

			for (i = 0; i < numspritelumps; i++)
			{
				d_main.IncThermo();
				w_wad.CacheInfo cache = w_wad.W_CacheLumpNum(firstspritelump + i, DoomDef.PU_CACHE);
				BinaryReader bs = new BinaryReader(new MemoryStream(cache.data));
				patch = new DoomData.patch_t();
				patch.width = bs.ReadInt16();
				patch.height = bs.ReadInt16();
				patch.leftoffset = bs.ReadInt16();
				patch.topoffset = bs.ReadInt16();
				patch.columnofs = new int[patch.width];
				for (int k = 0; k < patch.width; ++k) patch.columnofs[k] = bs.ReadInt32();

				spritewidth[i] = (patch.width) << DoomDef.FRACBITS;
				spriteoffset[i] = (patch.leftoffset) << DoomDef.FRACBITS;
				spritetopoffset[i] = (patch.topoffset) << DoomDef.FRACBITS;

				Texture2D xnaTexture = new Texture2D(Game1.instance.GraphicsDevice, patch.width + 2, patch.height + 2);
				Color[] xnaTextureData = new Color[(patch.width + 2) * (patch.height + 2)];
				for (int k = 0; k < patch.width; ++k)
				{
					DoomData.post_t patchcol = new DoomData.post_t();
					patchcol.topdelta = cache.data[patch.columnofs[k]];
					patchcol.length = cache.data[patch.columnofs[k] + 1];
					R_DrawColumnInCache(cache.data, patch.columnofs[k], patchcol, k, xnaTextureData, 0, patch.width + 2, patch.height + 2, 1, 1);
				}
				// Set non alpha pixels to the closest color with alpha
				for (int y = 0; y < patch.height; ++y)
				{
					for (int x = 0; x < patch.width; ++x)
					{
						if (x == 0 || x == patch.width - 1 ||
							y == 0 || y == patch.height - 1) continue;
						Color col = xnaTextureData[x + y * patch.width];
						if (col.A == 0)
						{
							if (xnaTextureData[(x - 1) + (y) * patch.width].A == 255)
								col = xnaTextureData[(x - 1) + (y) * patch.width];
							else if (xnaTextureData[(x + 1) + (y) * patch.width].A == 255)
								col = xnaTextureData[(x + 1) + (y) * patch.width];
							else if (xnaTextureData[(x) + (y - 1) * patch.width].A == 255)
								col = xnaTextureData[(x) + (y - 1) * patch.width];
							else if (xnaTextureData[(x) + (y + 1) * patch.width].A == 255)
								col = xnaTextureData[(x) + (y + 1) * patch.width];
							else if (xnaTextureData[(x - 1) + (y - 1) * patch.width].A == 255)
								col = xnaTextureData[(x - 1) + (y - 1) * patch.width];
							else if (xnaTextureData[(x + 1) + (y - 1) * patch.width].A == 255)
								col = xnaTextureData[(x + 1) + (y - 1) * patch.width];
							else if (xnaTextureData[(x - 1) + (y + 1) * patch.width].A == 255)
								col = xnaTextureData[(x - 1) + (y + 1) * patch.width];
							else if (xnaTextureData[(x + 1) + (y + 1) * patch.width].A == 255)
								col = xnaTextureData[(x + 1) + (y + 1) * patch.width];

							col.A = 0;
							xnaTextureData[x + y * patch.width] = col;
						}
					}
				}
				xnaTexture.SetData(xnaTextureData);
				cache.cache = xnaTexture;
				Game1.instance.allTextures.Add(xnaTexture);
				Game1.instance.spriteTextures.Add(xnaTexture);
			}
		}


		/*
		================
		=
		= R_InitColormaps
		=
		=================
		*/

		public static void R_InitColormaps()
		{
			int lump, length;
			//
			// load in the light tables
			// 256 byte align tables
			//
			lump = w_wad.W_GetNumForName("COLORMAP");
			length = w_wad.W_LumpLength(lump) + 255;
			colormaps = new byte[length];
			w_wad.CacheInfo cache = new w_wad.CacheInfo();
			w_wad.W_ReadLump(lump, cache);
			for (int i = 0; i < cache.data.Length; ++i)
			{
				colormaps[i] = cache.data[i];
			}
		}

		/*
		================
		=
		= R_InitData
		=
		= Locates all the lumps that will be used by all views
		= Must be called after W_Init
		=================
		*/

		public static void R_InitData()
		{
			R_InitTextures();
			R_InitFlats();
			d_main.IncThermo();
			R_InitSpriteLumps();
			d_main.IncThermo();
			R_InitColormaps();
		}

		//=============================================================================

		/*
		================
		=
		= R_FlatNumForName
		=
		================
		*/

		public static int R_FlatNumForName(string name)
		{
			int i;

			i = w_wad.W_CheckNumForName(name);
			if (i == -1)
			{
				i_ibm.I_Error("R_FlatNumForName: " + name + " not found");
			}
			return i - firstflat;
		}


		/*
		================
		=
		= R_CheckTextureNumForName
		=
		================
		*/

		public static int R_CheckTextureNumForName(string name)
		{
			int i;

			if (name[0] == '-')		// no texture marker
				return 0;

			for (i = 0; i < numtextures; i++)
				if (name.Equals(textures[i].name, StringComparison.OrdinalIgnoreCase))
					return i;

			return -1;
		}

		/*
		================
		=
		= R_TextureNumForName
		=
		================
		*/

		public static int R_TextureNumForName(string name)
		{
			int i;

			i = R_CheckTextureNumForName(name);
			if (i == -1)
				i_ibm.I_Error("R_TextureNumForName: " + name + " not found");

			return i;
		}

		/*
		=================
		=
		= R_PrecacheLevel
		=
		= Preloads all relevent graphics for the level
		=================
		*/

		public static int flatmemory, texturememory, spritememory;

		// [dsl] XNA. This is part of the triangulation
		static void propagateLineGroupingForSector(r_local.sector_t sector, r_local.line_t line1)
		{
			int i;

			for (i = 0; i < ((sector.parent != null) ? sector.linesTemp.Count : sector.linecount); ++i)
			{
				r_local.line_t line2 = null;
				if (sector.parent != null)
				{
					line2 = sector.linesTemp[i];
				}
				else
				{
					line2 = p_setup.linebuffer[sector.linesi + i];
				}
				if (line2 == line1) continue;
				if (line1.v1 == line2.v2 ||
					line1.v2 == line2.v1 ||
					line1.v1 == line2.v1 ||
					line1.v2 == line2.v2)
				{
					// They are connected
					// Propagate groupping of line1 to line 2
					if (line1.buildGroupId != line2.buildGroupId)
					{
						line2.buildGroupId = line1.buildGroupId;
						propagateLineGroupingForSector(sector, line2);
					}
				}
			}
		}

		public class Vector2AndTags
		{
			public Vector2 p = Vector2.Zero;
			public int groupTag = 0;
			public List<r_local.sector_t> backSectors = new List<r_local.sector_t>();
		}

		static void AddTriangleToListAndTesselate(r_local.sector_t curSector,
			Vector2AndTags p1v, Vector2AndTags p2v, Vector2AndTags p3v, List<Vector2AndTags> triangleList,
			r_local.vertex_t p1, r_local.vertex_t p2, r_local.vertex_t p3)
		{
			triangleList.Add(p1v);
			triangleList.Add(p2v);
			triangleList.Add(p3v);

		/*	p1v.groupTag = p1.groupTag;
			p2v.groupTag = p2.groupTag;
			p3v.groupTag = p3.groupTag;

			if (p1.line != null)
			{
				if (p1.line.backsector == curSector) p1v.backSectors.Add(p1.line.frontsector);
				else p1v.backSectors.Add(p1.line.backsector);
			}
			if (p2.line != null)
			{
				if (p2.line.backsector == curSector) p2v.backSectors.Add(p2.line.frontsector);
				else p2v.backSectors.Add(p2.line.backsector);
			}
			if (p3.line != null)
			{
				if (p3.line.backsector == curSector) p3v.backSectors.Add(p3.line.frontsector);
				else p3v.backSectors.Add(p3.line.backsector);
			}

			Vector2AndTags mid1 = new Vector2AndTags
			{
				p = (p1v.p + p2v.p) * .5f,
				groupTag = (p1.nextId == p2.id) ? p1.groupTag : 0,
				backSectors = p1v.backSectors.ToList()
			};
			Vector2AndTags mid2 = new Vector2AndTags
			{
				p = (p2v.p + p3v.p) * .5f,
				groupTag = (p2.nextId == p3.id) ? p2.groupTag : 0,
				backSectors = p2v.backSectors.ToList()
			};
			Vector2AndTags mid3 = new Vector2AndTags
			{
				p = (p3v.p + p1v.p) * .5f,
				groupTag = (p3.nextId == p1.id) ? p3.groupTag : 0,
				backSectors = p3v.backSectors.ToList()
			};

			p1v.backSectors.AddRange(mid3.backSectors);
			p2v.backSectors.AddRange(mid1.backSectors);
			p3v.backSectors.AddRange(mid2.backSectors);

			triangleList.Add(p1v);
			triangleList.Add(mid1);
			triangleList.Add(mid3);

			triangleList.Add(p2v);
			triangleList.Add(mid2);
			triangleList.Add(mid1);

			triangleList.Add(p3v);
			triangleList.Add(mid3);
			triangleList.Add(mid2);

			triangleList.Add(mid1);
			triangleList.Add(mid2);
			triangleList.Add(mid3);*/
		}


		// [dsl] This is used in the triagulation algorithm of the sectors
		//0003   * @param x1 Starting point of Segment 1
		//0004   * @param y1 Starting point of Segment 1
		//0005   * @param x2 Ending point of Segment 1
		//0006   * @param y2 Ending point of Segment 1
		//0007   * @param x3 Starting point of Segment 2
		//0008   * @param y3 Starting point of Segment 2
		//0009   * @param x4 Ending point of Segment 2
		//0010   * @param y4 Ending point of Segment 2
		static public bool segIntersect(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
		{
			double retX = 0, retY = 0;
			return lineSegmentIntersection(x1, y1, x2, y2, x3, y3, x4, y4, ref retX, ref retY);
		}

		// [dsl] Ok this function was taken from the web. It had an error in it, then
		// a comment posted the fixed version, which STILL contained an error!!! But now
		// it's almost perfect... sigh..
		static public bool segIntersectOld(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
		{
			double d = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
			if (d == 0) return false;
			double xi = ((x3 - x4) * (x1 * y2 - y1 * x2) - (x1 - x2) * (x3 * y4 - y3 * x4)) / d;
			double yi = ((y3 - y4) * (x1 * y2 - y1 * x2) - (y1 - y2) * (x3 * y4 - y3 * x4)) / d;
			if (x3 == x4)
			{
				if (yi < Math.Min(y1, y2) || yi > Math.Max(y1, y2)) return false;
				if (yi < Math.Min(y3, y4) || yi > Math.Max(y3, y4)) return false;
				return true;
			} 
			if (xi < Math.Min(x1, x2) || xi > Math.Max(x1, x2)) return false;
			if (xi < Math.Min(x3, x4) || xi > Math.Max(x3, x4)) return false;
			return true;
		}

		// [dsl]
		// jobinelv's C# blog
		// http://csjobinelv.blogspot.ca/2013/05/this-is-algorithm-finding-whether-line.html
		// HUGE credit, because I tried my own and didn't work. I tried a couple others from the interweb
		// and they were all crap. Then this one just magically works PERFECTLY.
		public static bool lineSegmentIntersection(
						double Ax, double Ay,
						double Bx, double By,
						double Cx, double Cy,
						double Dx, double Dy,
						ref double X, ref double Y)
		{

			double distAB, theCos, theSin, newX, ABpos;

			//  Fail if either line segment is zero-length.
			if (Ax == Bx && Ay == By || Cx == Dx && Cy == Dy)
			{
				if ((Cx == Dx) && (Cx >= Ax && Dx <= Bx) && (Cy == Dy && Ay == Cy))
				{
					X = Cx;
					Y = Cy;
					return true;
				}
				if ((Ax == Bx) && (Ax >= Cx && Bx <= Dx) && (Ay == By && Cy == Ay))
				{
					X = Ax;
					Y = Ay;
					return true;
				}
				return false;
			}

			//------custom--|-|-----------------------------//end of one line on the other line

			bool IsVertical = false;

			if (IsPointOnLineSegment(Ax, Ay, Cx, Cy, Dx, Dy, ref IsVertical))
			{
				if (IsVertical)
				{
					if (Ax == Bx)
					{
						X = Cx;//D
						Y = Cy;
					}
					else
					{
						X = Ax;
						Y = Ay;
					}

				}
				else
				{
					X = Ax; Y = Ay;
				}
				return true;
			}
			if (IsPointOnLineSegment(Bx, By, Cx, Cy, Dx, Dy, ref IsVertical))
			{
				if (IsVertical)
				{
					if (Ax == Bx)
					{
						X = Dx; //C
						Y = Dy;
					}
					else
					{
						X = Bx;
						Y = By;
					}
				}
				else
				{
					X = Bx; Y = By;
				}
				return true;
			}
			if (IsPointOnLineSegment(Cx, Cy, Ax, Ay, Bx, By, ref IsVertical))
			{
				if (IsVertical)
				{
					X = Cx;
					Y = Cy;
				}
				else
				{
					X = Cx; Y = Cy;
				}
				return true;
			}
			if (IsPointOnLineSegment(Dx, Dy, Ax, Ay, Bx, By, ref IsVertical))
			{
				if (IsVertical)
				{
					X = Dx;
					Y = Dy;
				}
				else
				{
					X = Dx; Y = Dy;
				}
				return true;
			}


			//------------------------------------------------
			//  Fail if the segments share an end-point.
			if (Ax == Cx && Ay == Cy || Bx == Cx && By == Cy
			|| Ax == Dx && Ay == Dy || Bx == Dx && By == Dy)
			{
				return false;
			}

			//  (1) Translate the system so that point A is on the origin.
			Bx -= Ax; By -= Ay;
			Cx -= Ax; Cy -= Ay;
			Dx -= Ax; Dy -= Ay;

			//  Discover the length of segment A-B.
			distAB = Math.Sqrt(Bx * Bx + By * By);

			//  (2) Rotate the system so that point B is on the positive X axis.
			theCos = Bx / distAB;
			theSin = By / distAB;
			newX = Cx * theCos + Cy * theSin;
			Cy = Cy * theCos - Cx * theSin; Cx = newX;
			newX = Dx * theCos + Dy * theSin;
			Dy = Dy * theCos - Dx * theSin; Dx = newX;

			//  Fail if segment C-D doesn't cross line A-B.
			if (Cy < 0 && Dy < 0 || Cy >= 0 && Dy >= 0) return false;

			//  (3) Discover the position of the intersection point along line A-B.
			ABpos = Dx + (Cx - Dx) * Dy / (Dy - Cy);

			//  Fail if segment C-D crosses line A-B outside of segment A-B.
			if (ABpos < 0 || ABpos > distAB) return false;

			//  (4) Apply the discovered position to line A-B in the original coordinate system.
			X = Math.Round((Ax + ABpos * theCos), 3);
			Y = Math.Round((Ay + ABpos * theSin), 3);

			//  Success.
			return true;
		}


		public static bool IsPointOnLineSegment(double Px, double Py, double Ax, double Ay, double Bx, double By, ref bool IsVertical)
		{
			double least = 0;
			if (Px < least) least = Px;
			if (Ax < least) least = Ax;
			if (Bx < least) least = Bx;

			if (least < 0)
			{
				Px += Math.Abs(least);
				Ax += Math.Abs(least);
				Bx += Math.Abs(least);
			}

			if (!(Ax <= Px && Bx >= Px)) return false;
			if (Bx == Ax) //vertical line, slope = infinity
			{
				IsVertical = true;
				if ((Px == Ax && (Py >= Ay && Py <= By)) || (Px == Ax && (Py >= By && Py <= Ay)))
				{
					return true;
				}

				return false;

			}
			double S = (By - Ay) / (Bx - Ax);//S=0 horizontal line
			double Y = Ay - (S * Ax);

			if (Math.Abs(Py - (S * Px + Y)) < 0.0009) return true;   //change the precision you want

			return false;
		}

		public struct Segment
		{
			public double x1;
			public double y1;
			public double x2;
			public double y2;
		}

		static public List<Segment> bspLines = new List<Segment>();

		public static void precacheBSPNode(int bspnum)
		{
			r_local.node_t bsp;

			// Check if we found a leaf
			if ((bspnum & DoomData.NF_SUBSECTOR) != 0)
			{
				if (bspnum != -1)
				{
					precacheLeafNode(bspnum & (~DoomData.NF_SUBSECTOR));
				}
				return;
			}

			// Create an infinite line
			bsp = p_setup.nodes[bspnum];
			Segment seg;
			double p1x = (bsp.x >> DoomDef.FRACBITS);
			double p1y = (bsp.y >> DoomDef.FRACBITS);
			seg.x1 = p1x;
			seg.y1 = p1y;
			seg.x2 = ((bsp.x + bsp.dx) >> DoomDef.FRACBITS);
			seg.y2 = ((bsp.y + bsp.dy) >> DoomDef.FRACBITS);
			double dirX = seg.x2 - seg.x1;
			double dirY = seg.y2 - seg.y1;
			double len = Math.Sqrt(dirX * dirX + dirY * dirY);
			dirX /= len;
			dirY /= len;
			seg.x1 -= dirX * 32768;
			seg.y1 -= dirY * 32768;
			seg.x2 += dirX * 32768;
			seg.y2 += dirY * 32768;

			// Cut it with existing lines
			foreach (Segment otherSeg in bspLines)
			{
				double intersectionX = 0;
				double intersectionY = 0;
				if (r_data.lineSegmentIntersection(
					seg.x1, seg.y1, seg.x2, seg.y2,
					otherSeg.x1, otherSeg.y1, otherSeg.x2, otherSeg.y2,
					ref intersectionX, ref intersectionY))
				{
					// Determine best side to cut
					double otherFrontX = otherSeg.y2 - otherSeg.y1;
					double otherFrontY = -(otherSeg.x2 - otherSeg.x1);
					double dot1 =
						otherFrontX * (seg.x1 - otherSeg.x1) +
						otherFrontY * (seg.y1 - otherSeg.y1);
					double dot2 =
						otherFrontX * (p1x - otherSeg.x1) +
						otherFrontY * (p1y - otherSeg.y1);
					if (dot1 > 0)
					{
						if (dot2 > 0)
						{
							seg.x2 = (float)intersectionX;
							seg.y2 = (float)intersectionY;
						}
						else
						{
							seg.x1 = (float)intersectionX;
							seg.y1 = (float)intersectionY;
						}
					}
					else if (dot1 < 0)
					{
						if (dot2 < 0)
						{
							seg.x2 = (float)intersectionX;
							seg.y2 = (float)intersectionY;
						}
						else
						{
							seg.x1 = (float)intersectionX;
							seg.y1 = (float)intersectionY;
						}
					}
				}
			}

			// Add our new split to the list
			bspLines.Add(seg);

			// Go to children
			precacheBSPNode(bsp.children[0]);
			precacheBSPNode(bsp.children[1]);
		}

		public struct Vector2d
		{
			public Vector2d(double in_x, double in_y) { x = in_x; y = in_y; }
			public double x;
			public double y;
		}

		public static void precacheLeafNode(int bspnum)
		{
			r_local.subsector_t sub = p_setup.subsectors[bspnum];
			r_local.sector_t sector = sub.sector;
			sub.debugColor = new Color(m_misc.P_Random() % 256, m_misc.P_Random() % 256, m_misc.P_Random() % 256);

			// Do the magic here
			List<Segment> sectorSegs = new List<Segment>();
			List<Segment> sectorBspSegs = new List<Segment>();
			List<Vector2d> verts = new List<Vector2d>();
			for (int i = 0; i < sub.numlines; ++i)
			{
				r_local.seg_t seg = p_setup.segs[sub.firstline + i];
				Segment bspSeg;
				bspSeg.x1 = (seg.v1.x >> DoomDef.FRACBITS);
				bspSeg.y1 = (seg.v1.y >> DoomDef.FRACBITS);
				bspSeg.x2 = (seg.v2.x >> DoomDef.FRACBITS);
				bspSeg.y2 = (seg.v2.y >> DoomDef.FRACBITS);
				sectorSegs.Add(bspSeg);

				verts.Add(new Vector2d(bspSeg.x1, bspSeg.y1));
				verts.Add(new Vector2d(bspSeg.x2, bspSeg.y2));
			}

			// Find BSP lines that touch and split them with all other bsp lines to keep only the touching part to us
			foreach (Segment bspSeg1 in bspLines)
			{
				Segment bspSeg = bspSeg1;
				double intersectionX = 0;
				double intersectionY = 0;
				foreach (Segment seg1 in sectorSegs)
				{
					Segment seg = seg1;
					double dirX = seg.x2 - seg.x1;
					double dirY = seg.y2 - seg.y1;
					double len = Math.Sqrt(dirX * dirX + dirY * dirY);
					dirX /= len;
					dirY /= len;
					seg.x1 -= dirX * 32768;
					seg.y1 -= dirY * 32768;
					seg.x2 += dirX * 32768;
					seg.y2 += dirY * 32768;

					if (r_data.lineSegmentIntersection(
						bspSeg.x1, bspSeg.y1, bspSeg.x2, bspSeg.y2,
						seg.x1, seg.y1, seg.x2, seg.y2,
						ref intersectionX, ref intersectionY))
					{
						double otherFrontX = seg.y2 - seg.y1;
						double otherFrontY = -(seg.x2 - seg.x1);
						double dot1 =
							otherFrontX * (bspSeg.x2 - seg.x1) +
							otherFrontY * (bspSeg.y2 - seg.y1);
						if (dot1 > 0)
						{
							bspSeg.x2 = intersectionX;
							bspSeg.y2 = intersectionY;
							sectorBspSegs.Add(bspSeg);
						}
						else if (dot1 < 0)
						{
							bspSeg.x1 = intersectionX;
							bspSeg.y1 = intersectionY;
							sectorBspSegs.Add(bspSeg);
						}
					}
				}
			}

			sectorSegs.AddRange(sectorBspSegs);
			sub.bspSegs = sectorSegs;
		}

		public static void R_PrecacheLevel()
		{
			string flatpresent;
			string texturepresent;
			string spritepresent;
			int i, j, k, lump;
			texture_t texture;
			DoomDef.thinker_t th;
			r_local.spriteframe_t sf;

			if (g_game.demoplayback)
				return;

			// [dsl] Build our segments list for sectors
			foreach (r_local.sector_t sector in p_setup.sectors)
			{
				if (sector.segs == null)
				{
					sector.segs = new List<r_local.seg_t>();
					foreach (r_local.seg_t seg in p_setup.segs)
					{
						if (seg.frontsector == sector)
						{
							sector.segs.Add(seg);
						}
					}
				}
			}

			Standard.Effects.prepareLevel();
			Deferred.Effects.prepareLevel();
			AmbientMap.prepareLevel();

			// Find subsectors down the binary tree
		//	bspLines.Clear();
		//	precacheBSPNode(p_setup.numnodes - 1);


			// [dsl] Create vertices list for sectors
			int currentSector = 0;
			List<r_local.sector_t> sectors = p_setup.sectors.ToList();
			for (int o = 0; o < sectors.Count(); ++o)
			{
				r_local.sector_t sector = sectors[o];
			//	if (currentSector == 25) { currentSector++; continue; }
				// First make all lines tag group -1
				List<r_local.line_t> linesFromBuffer = new List<r_local.line_t>();
				if (sector.parent != null)
				{
					foreach (r_local.line_t line in sector.linesTemp)
					{
						line.buildGroupId = -1;
						linesFromBuffer.Add(line);
					}
				}
				else
				{
					for (i = 0; i < sector.linecount; ++i)
					{
						r_local.line_t line = p_setup.linebuffer[sector.linesi + i];
						line.buildGroupId = -1;

						if (line.frontsector == sector &&
							line.backsector == sector) continue;

						linesFromBuffer.Add(line);
					}
				}

				// Then determine line grouping by tagging them. This is to differenciate
				// line groups, like holes in the middle of the floor
				int curGroup = 0;
				if (sector.parent != null)
				{
					foreach (r_local.line_t line in sector.linesTemp)
					{
						line.buildGroupId = curGroup++;
						propagateLineGroupingForSector(sector, line);
					}
				}
				else
				{
					for (i = 0; i < linesFromBuffer.Count(); ++i)
					{
						r_local.line_t line = linesFromBuffer[i];
						line.buildGroupId = curGroup++;
						propagateLineGroupingForSector(sector, line);
					}
				}

				// Create our line lists per group
				Dictionary<int, List<r_local.line_t>> groups = new Dictionary<int, List<r_local.line_t>>();
				List<List<r_local.vertex_t>> vertGroups = new List<List<r_local.vertex_t>>();
				sector.vertGroups = vertGroups;
				if (sector.parent != null)
				{
					foreach (r_local.line_t line in sector.linesTemp)
					{
						if (groups.ContainsKey(line.buildGroupId) == false) groups[line.buildGroupId] = new List<r_local.line_t>();
						groups[line.buildGroupId].Add(line);
					}
					if (groups.Count() > 0)
					{
						// Create more sectors with the other groups
						for (int m = 1; m < groups.Count(); ++m)
						{
							r_local.sector_t extraSector = new r_local.sector_t();
							extraSector.linesTemp = groups.ElementAt(m).Value;
							extraSector.parent = sector.parent;
							sectors.Add(extraSector);
							groups.Remove(groups.ElementAt(m).Key);
						}
					}
				}
				else
				{
					for (i = 0; i < linesFromBuffer.Count(); ++i)
					{
						r_local.line_t line = linesFromBuffer[i];
						if (groups.ContainsKey(line.buildGroupId) == false) groups[line.buildGroupId] = new List<r_local.line_t>();
						groups[line.buildGroupId].Add(line);
					}
				}

				// Put back the group Ids from 1
				for (int m = 0; m < groups.Count(); ++m)
				{
					List<r_local.line_t> lines = groups.ElementAt(m).Value;
					foreach (r_local.line_t line in lines)
					{
						line.buildGroupId = m + 1;
					/*	if (line.backsector == sector)
							line.v2.line = line;
						else
							line.v1.line = line;*/
						line.v1.groupTag = line.buildGroupId;
						line.v2.groupTag = line.buildGroupId;
					}
				}

				// Make sure lines are sorted clockwise for each group
				// We will change the widding order later
				for (int m = 0; m < groups.Count(); ++m)
				{
					List<r_local.line_t> lines = groups.ElementAt(m).Value;
					List<r_local.line_t> originalLines = new List<r_local.line_t>(lines);
					List<r_local.vertex_t> newList = new List<r_local.vertex_t>();
					if (sector.parent != null)
					{
						if (lines.First().frontsector == sector.parent)
						{
							newList.Add(lines.First().v1);
							newList.Add(lines.First().v2);
							lines.RemoveAt(0);
						}
						else
						{
							newList.Add(lines.First().v2);
							newList.Add(lines.First().v1);
							lines.RemoveAt(0);
						}
					}
					else
					{
						if (lines.First().frontsector == sector)
						{
							newList.Add(lines.First().v1);
							newList.Add(lines.First().v2);
							lines.RemoveAt(0);
						}
						else
						{
							newList.Add(lines.First().v2);
							newList.Add(lines.First().v1);
							lines.RemoveAt(0);
						}
					}
					bool fucked = false;
					r_local.line_t toErase = null;
					while (lines.Count() > 1)
					{
						r_local.vertex_t pprev = newList[newList.Count() - 2];
						r_local.vertex_t prev = newList.Last();
						Vector2 pprevV = new Vector2(
							pprev.x >> DoomDef.FRACBITS,
							pprev.y >> DoomDef.FRACBITS);
						Vector2 prevV = new Vector2(
							prev.x >> DoomDef.FRACBITS,
							prev.y >> DoomDef.FRACBITS);
						Vector2 dirPrev = prevV - pprevV;
						dirPrev.Normalize();
						Vector2 frontPrev;
						frontPrev.X = dirPrev.Y;
						frontPrev.Y = -dirPrev.X;
						r_local.vertex_t bestSoFar = null;
						r_local.vertex_t vert = null;
						Vector2 dirBest = Vector2.Zero;
						toErase = null;
						for (i = 0; i < lines.Count(); ++i)
						{
							r_local.line_t line = lines[i];
							vert = null;
							if (prev == line.v2) vert = line.v1;
							else if (prev == line.v1) vert = line.v2;
							if (vert != null)
							{
								Vector2 vertV = new Vector2(
									vert.x >> DoomDef.FRACBITS,
									vert.y >> DoomDef.FRACBITS);
								// Acutest angle wins
								if (bestSoFar == null)
								{
									bestSoFar = vert;
									toErase = line;
									dirBest = vertV - prevV;
									dirBest.Normalize();
								}
								else
								{
									Vector2 dir = vertV - prevV;
									dir.Normalize();
									float bestDot = Vector2.Dot(frontPrev, dirBest);
									float vertDot = Vector2.Dot(frontPrev, dir);
									if (vertDot > bestDot)
									{
										// Check to make sure with the dir
										bestDot = Vector2.Dot(dirPrev, dirBest);
										vertDot = Vector2.Dot(dirPrev, dir);
										if (vertDot <= bestDot)
										{
											bestSoFar = vert;
											toErase = line;
											dirBest = vertV - prevV;
											dirBest.Normalize();
										}
									}
								}
							}
						}
						if (bestSoFar == newList.First())
						{
							// We have closed the loop back to first point, this must be wrong..
							bestSoFar = null;
							toErase = null;
						}
						if (toErase != null)
						{
							lines.Remove(toErase);
						}
						if (bestSoFar != null) // No choice
						{
							newList.Add(bestSoFar);
						}
						else
						{
							r_local.sector_t extraSector = new r_local.sector_t();
							extraSector.linesTemp = lines;
							if (sector.parent != null) extraSector.parent = sector.parent;
							else extraSector.parent = sector;
							sectors.Add(extraSector);
							fucked = true;

							break;
						}
					}
					vertGroups.Add(newList);
					if (sector.parent != null)
					{
						sector.parent.vertGroups.Add(newList);
					}
					if (fucked) break;
				}

				// Id our vertices
				j = 0;
				for (int m = 0; m < sector.vertGroups.Count(); ++m)
				{
					List<r_local.vertex_t> verts = sector.vertGroups[m];
					for (i = 0; i < verts.Count; ++i)
					{
						r_local.vertex_t vert = verts[i];
						vert.id = j + i;
						vert.nextId = j + (i + 1) % verts.Count();
						for (k = 0; k < linesFromBuffer.Count(); ++k)
						{
							r_local.line_t line = linesFromBuffer[k];
							if (vert.x == line.v1.x &&
								vert.y == line.v1.y &&
								line.frontsector == sector) vert.line = line;
							if (vert.x == line.v2.x &&
								vert.y == line.v2.y &&
								line.backsector == sector) vert.line = line;
						}
					}
					j += verts.Count();
				}

				// [dsl] Now we need to attach holes to create 1 main polygon
				List<r_local.vertex_t> mainPoly = sector.vertGroups.First();
				List<r_local.vertex_t> alreadyJoined = new List<r_local.vertex_t>();
				int failCount = 0;
				int cur = 1;
				while (sector.vertGroups.Count() > 1)
				{
					if (cur >= sector.vertGroups.Count()) cur = 1;
					List<r_local.vertex_t> verts = sector.vertGroups[cur];
					if (verts == mainPoly) continue; // We ignore the first one, that one is "perfect"
					int vertFrom = -1;
					int vertTo = -1;
					int besti = -1;
					int bestk = -1;
					int bestDis = -1;
					for (i = 0; i < verts.Count; ++i)
					{
						r_local.vertex_t vert3 = verts[(i + verts.Count - 1) % verts.Count];
						r_local.vertex_t vert = verts[i];
						r_local.vertex_t vert2 = verts[(i + 1) % verts.Count];
						if (alreadyJoined.Contains(vert)) continue;
						for (k = 0; k < mainPoly.Count; ++k)
						{
							r_local.vertex_t vertMain3 = mainPoly[(k + mainPoly.Count - 1) % mainPoly.Count];
							r_local.vertex_t vertMain = mainPoly[k];
							r_local.vertex_t vertMain2 = mainPoly[(k + 1) % mainPoly.Count];
							if (alreadyJoined.Contains(vertMain)) continue;

							// Make sure that the line are drawn inside the shape, not outside
							Vector2 lp1 = new Vector2(vert.x >> DoomDef.FRACBITS, vert.y >> DoomDef.FRACBITS);
							Vector2 lp2 = new Vector2(vertMain.x >> DoomDef.FRACBITS, vertMain.y >> DoomDef.FRACBITS);
							Vector2 dir = lp2 - lp1;
							dir.Normalize();
							Vector2 dir1 =
								new Vector2(vert2.x >> DoomDef.FRACBITS, vert2.y >> DoomDef.FRACBITS) -
								new Vector2(vert.x >> DoomDef.FRACBITS, vert.y >> DoomDef.FRACBITS);
							Vector2 dir2 =
								new Vector2(vertMain2.x >> DoomDef.FRACBITS, vertMain2.y >> DoomDef.FRACBITS) -
								new Vector2(vertMain.x >> DoomDef.FRACBITS, vertMain.y >> DoomDef.FRACBITS);
							Vector2 dir3 =
								new Vector2(vert.x >> DoomDef.FRACBITS, vert.y >> DoomDef.FRACBITS) -
								new Vector2(vert3.x >> DoomDef.FRACBITS, vert3.y >> DoomDef.FRACBITS);
							Vector2 dir4 =
								new Vector2(vertMain.x >> DoomDef.FRACBITS, vertMain.y >> DoomDef.FRACBITS) -
								new Vector2(vertMain3.x >> DoomDef.FRACBITS, vertMain3.y >> DoomDef.FRACBITS);
							dir1.Normalize();
							dir2.Normalize();
							dir3.Normalize();
							dir4.Normalize();
							Vector2 front1, front2, front3, front4;
							front1.X = dir1.Y;
							front1.Y = -dir1.X;
							front2.X = dir2.Y;
							front2.Y = -dir2.X;
							front3.X = dir3.Y;
							front3.Y = -dir3.X;
							front4.X = dir4.Y;
							front4.Y = -dir4.X;
							if (!(
								(Vector2.Dot(dir, front1) > 0 || Vector2.Dot(dir, front3) > 0) &&
								(Vector2.Dot(-dir, front2) > 0 || Vector2.Dot(-dir, front4) > 0))
								)
							{
								continue;
							}

							bool intersect = false;
							// Test intersection against all other poly including ours
							foreach (List<r_local.vertex_t> otherVerts in sector.vertGroups)
							{
								for (j = 0; j < otherVerts.Count; ++j)
								{
									r_local.vertex_t edgeP1 = otherVerts[j];
									r_local.vertex_t edgeP2 = otherVerts[(j + 1) % otherVerts.Count];
									if ((vert.x == edgeP1.x && vert.y == edgeP1.y) ||
										(vert.x == edgeP2.x && vert.y == edgeP2.y) ||
										(vertMain.x == edgeP1.x && vertMain.y == edgeP1.y) ||
										(vertMain.x == edgeP2.x && vertMain.y == edgeP2.y)) continue;

									// Check for intersection
									if (segIntersectOld(
										vert.x >> DoomDef.FRACBITS, vert.y >> DoomDef.FRACBITS,
										vertMain.x >> DoomDef.FRACBITS, vertMain.y >> DoomDef.FRACBITS,
										edgeP1.x >> DoomDef.FRACBITS, edgeP1.y >> DoomDef.FRACBITS,
										edgeP2.x >> DoomDef.FRACBITS, edgeP2.y >> DoomDef.FRACBITS))
									{
										intersect = true;
										break;
									}
								}
								if (intersect) break;
							}
							if (!intersect)
							{
								if (besti != -1)
								{
									int dis =
										((vert.x >> DoomDef.FRACBITS) - (vertMain.x >> DoomDef.FRACBITS)) *
										((vert.x >> DoomDef.FRACBITS) - (vertMain.x >> DoomDef.FRACBITS)) +
										((vert.y >> DoomDef.FRACBITS) - (vertMain.y >> DoomDef.FRACBITS)) *
										((vert.y >> DoomDef.FRACBITS) - (vertMain.y >> DoomDef.FRACBITS));
									if (dis < bestDis)
									{
										besti = i;
										bestk = k;
										bestDis = dis;
									}
								}
								else
								{
									besti = i;
									bestk = k;
									bestDis =
										((vert.x >> DoomDef.FRACBITS) - (vertMain.x >> DoomDef.FRACBITS)) *
										((vert.x >> DoomDef.FRACBITS) - (vertMain.x >> DoomDef.FRACBITS)) +
										((vert.y >> DoomDef.FRACBITS) - (vertMain.y >> DoomDef.FRACBITS)) *
										((vert.y >> DoomDef.FRACBITS) - (vertMain.y >> DoomDef.FRACBITS));
								}
							}
						}
					}
					if (besti != -1)
					{
						vertFrom = besti;
						vertTo = bestk;
					}
					if (vertFrom != -1)
					{
						// We will insert 2 new vertices
						r_local.vertex_t newVertFrom = new r_local.vertex_t { 
							x = verts[vertFrom].x, 
							y = verts[vertFrom].y, 
							groupTag = verts[vertFrom].groupTag, 
							id = verts[vertFrom].id, 
							nextId = verts[vertFrom].nextId,
							line = verts[vertFrom].line
						};
						r_local.vertex_t newVertTo = new r_local.vertex_t { 
							x = mainPoly[vertTo].x, 
							y = mainPoly[vertTo].y, 
							groupTag = mainPoly[vertTo].groupTag,
							id = mainPoly[vertTo].id,
							nextId = mainPoly[vertTo].nextId,
							line = mainPoly[vertTo].line
						};

						// Make sure we never link them again
						alreadyJoined.Add(verts[vertFrom]);
						alreadyJoined.Add(mainPoly[vertTo]);
						alreadyJoined.Add(newVertFrom);
						alreadyJoined.Add(newVertTo);

						List<r_local.vertex_t> toInsert = new List<r_local.vertex_t>();
						if (vertFrom < verts.Count()) toInsert.AddRange(verts.GetRange(vertFrom, verts.Count() - vertFrom));
						if (vertFrom > 0) toInsert.AddRange(verts.GetRange(0, vertFrom));
						toInsert.Add(newVertFrom);
						mainPoly.Insert(vertTo, newVertTo);
						mainPoly.InsertRange(vertTo + 1, toInsert);
						sector.vertGroups.RemoveAt(cur);
						failCount = 0;
					} // If that failed, we try next one then we might come back after with new solution. Or the triangulation failed :(
					else
					{
						failCount++;
						if (failCount == sector.vertGroups.Count() - 1)
						{
							while (sector.vertGroups.Count() > 1)
							{
								sector.vertGroups.RemoveAt(sector.vertGroups.Count() - 1);
							}
							break;
						}
					}
					cur++;
				}

				// [dsl] Now that we have only 1 group left, we can triangulate!
				// We are using what we call Ear Clipping
				i = 0;
				List<r_local.vertex_t> vertices = new List<r_local.vertex_t>(mainPoly);
				int lastValid = -1;
				List<Vector2AndTags> triangleList = new List<Vector2AndTags>();
				List<int> ambientTagList = new List<int>();
			//	if (currentSector != 306)
				while (vertices.Count > 2)
				{
					// Check the 2 edges in front of it
					r_local.vertex_t p1 = vertices[i];
					r_local.vertex_t p2 = vertices[(i + 1) % vertices.Count];
					r_local.vertex_t p3 = vertices[(i + 2) % vertices.Count];

					Vector2 p1v = new Vector2(
						p1.x >> DoomDef.FRACBITS,
						p1.y >> DoomDef.FRACBITS);
					Vector2 p2v = new Vector2(
						p2.x >> DoomDef.FRACBITS,
						p2.y >> DoomDef.FRACBITS);
					Vector2 p3v = new Vector2(
						p3.x >> DoomDef.FRACBITS,
						p3.y >> DoomDef.FRACBITS);

					Vector2 dir = p2v - p1v;
					dir.Normalize();
					Vector2 front;
					front.X = dir.Y;
					front.Y = -dir.X;
					dir = p3v - p2v;
					dir.Normalize();

					float dot = Vector2.Dot(dir, front);
					if (dot <= 0)
					{
						// We are not allowed to add this triangle for now, check the next one
						i = (i + 1) % vertices.Count;     
						if (lastValid == i)
						{
							// Biplanar triangles, we can remove them
							break;
						}
						continue;
					}

					if (vertices.Count == 3)
					{
						// Last triangle
						AddTriangleToListAndTesselate(
							sector,
							new Vector2AndTags { p = p1v, },
							new Vector2AndTags { p = p2v, },
							new Vector2AndTags { p = p3v, },
							triangleList,
							p1, p2, p3);
						break;
					}

					// Check that we dont intersect with any of the remaining edges
					bool intersect = false;
					for (j = 0; j < vertices.Count; ++j)
					{
						r_local.vertex_t edgeP1 = vertices[j];
						r_local.vertex_t edgeP2 = vertices[(j + 1) % vertices.Count];
							if (
								(p2.x == edgeP1.x && p2.y == edgeP1.y) ||
								(p2.x == edgeP2.x && p2.y == edgeP2.y) ||
								(p1.x == edgeP1.x && p1.y == edgeP1.y) ||
								(p1.x == edgeP2.x && p1.y == edgeP2.y) ||
								(p3.x == edgeP1.x && p3.y == edgeP1.y) ||
								(p3.x == edgeP2.x && p3.y == edgeP2.y)) continue;
						// Check for intersection
						if (segIntersect(
							p1.x >> DoomDef.FRACBITS, p1.y >> DoomDef.FRACBITS,
							p3.x >> DoomDef.FRACBITS, p3.y >> DoomDef.FRACBITS,
							edgeP1.x >> DoomDef.FRACBITS, edgeP1.y >> DoomDef.FRACBITS,
							edgeP2.x >> DoomDef.FRACBITS, edgeP2.y >> DoomDef.FRACBITS))
						{
							intersect = true;
							i = (i + 1) % vertices.Count;
							break;
						}
					}

					// Perfect! Remove p2, store that triangle, and move on
					if (!intersect)
					{
						// Now to make sure, we check with previous and next of that triangle. Some dot products
						// to see we don't directly intersect with it
						r_local.vertex_t p0 = vertices[(i + (vertices.Count - 1)) % vertices.Count];
						r_local.vertex_t p4 = vertices[(i + 3) % vertices.Count];
						Vector2 p0v = new Vector2(
							p0.x >> DoomDef.FRACBITS,
							p0.y >> DoomDef.FRACBITS);
						Vector2 p4v = new Vector2(
							p4.x >> DoomDef.FRACBITS,
							p4.y >> DoomDef.FRACBITS);
						dir = p3v - p1v;
						dir.Normalize();
						front.X = dir.Y;
						front.Y = -dir.X;
						dir = p4v - p3v;
						dir.Normalize();
						dot = Vector2.Dot(dir, front);
						if (dot <= 0)
						{
							Vector2 mydir = p3v - p1v;
							dir.Normalize();
							if (Vector2.Dot(dir, mydir) <= 0)
							{
								// We are not allowed to add this triangle for now, check the next one
								i = (i + 1) % vertices.Count;
								if (lastValid == i)
								{
									// Biplanar triangles, we can remove them
									break;
								}
								continue;
							}
						}
						dir = p0v - p1v;
						dir.Normalize();
						dot = Vector2.Dot(dir, front);
						if (dot <= 0)
						{
							Vector2 mydir = p1v - p3v;
							dir.Normalize();
							if (Vector2.Dot(dir, mydir) <= 0)
							{
								// We are not allowed to add this triangle for now, check the next one
								i = (i + 1) % vertices.Count;
								if (lastValid == i)
								{
									// Biplanar triangles, we can remove them
									break;
								}
								continue;
							}
						}

						// Add it
						AddTriangleToListAndTesselate(
							sector,
							new Vector2AndTags { p = p1v, },
							new Vector2AndTags { p = p2v, },
							new Vector2AndTags { p = p3v, },
							triangleList,
							p1, p2, p3);
						int toRemove = (i + 1) % vertices.Count;
						if (toRemove < i) --i;
						lastValid = i;
						vertices.RemoveAt(toRemove);
					}
				}

				if (sector.parent != null)
				{
					sector.parent.triangleList.AddRange(triangleList);
					sector.parent.ambientTagList.AddRange(ambientTagList);
				}
				else
				{
					sector.triangleList = triangleList;
					sector.ambientTagList = ambientTagList;
				}

				++currentSector;
			}

			// [dsl] We already loaded everything at game start on XNA.

			//
			// precache flats
			//	
			/*	flatpresent = alloca(numflats);
				memset (flatpresent,0,numflats);	
				for (i=0 ; i<numsectors ; i++)
				{
					flatpresent[sectors[i].floorpic] = 1;
					flatpresent[sectors[i].ceilingpic] = 1;
				}
	
				flatmemory = 0;
				for (i=0 ; i<numflats ; i++)
					if (flatpresent[i])
					{
						lump = firstflat + i;
						flatmemory += lumpinfo[lump].size;
						W_CacheLumpNum(lump, PU_CACHE);
					}
		
			//
			// precache textures
			//
				texturepresent = alloca(numtextures);
				memset (texturepresent,0, numtextures);
	
				for (i=0 ; i<numsides ; i++)
				{
					texturepresent[sides[i].toptexture] = 1;
					texturepresent[sides[i].midtexture] = 1;
					texturepresent[sides[i].bottomtexture] = 1;
				}
	
				texturepresent[skytexture] = 1;
	
				texturememory = 0;
				for (i=0 ; i<numtextures ; i++)
				{
					if (!texturepresent[i])
						continue;
					texture = textures[i];
					for (j=0 ; j<texture.patchcount ; j++)
					{
						lump = texture.patches[j].patch;
						texturememory += lumpinfo[lump].size;
						W_CacheLumpNum(lump , PU_CACHE);
					}
				}
	
			//
			// precache sprites
			//
				spritepresent = alloca(numsprites);
				memset (spritepresent,0, numsprites);
	
				for (th = thinkercap.next ; th != &thinkercap ; th=th.next)
				{
					if (th.function == P_MobjThinker)
						spritepresent[((mobj_t *)th).sprite] = 1;
				}
	
				spritememory = 0;
				for (i=0 ; i<numsprites ; i++)
				{
					if (!spritepresent[i])
						continue;
					for (j=0 ; j<sprites[i].numframes ; j++)
					{
						sf = &sprites[i].spriteframes[j];
						for (k=0 ; k<8 ; k++)
						{
							lump = firstspritelump + sf.lump[k];
							spritememory += lumpinfo[lump].size;
							W_CacheLumpNum(lump , PU_CACHE);
						}
					}
				}*/
		}
	}
}
