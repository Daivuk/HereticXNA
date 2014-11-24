using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
namespace HereticXNA
{
	// W_wad.c

	static public class w_wad
	{


		//===============
		//   TYPES
		//===============

		public class wadinfo_t
		{
			public string identification;		// should be IWAD
			public int numlumps;
			public int infotableofs;
		}


		public class filelump_t
		{
			public int filepos;
			public int size;
			public string name;
		}

		//=============
		// GLOBALS
		//=============

		public static List<DoomDef.lumpinfo_t> lumpinfo;		// location of each lump on disk
		public static int numlumps;


#if DOS
void		**lumpcache;
#endif

		//===================


		public static void ExtractFileBase(string path, out string dest)
		{
			dest = Path.GetFileNameWithoutExtension(path);
		}

		/*
		============================================================================

								LUMP BASED ROUTINES

		============================================================================
		*/
		/*
		====================
		=
		= W_AddFile
		=
		= All files are optional, but at least one file must be found
		= Files with a .wad extension are wadlink files with multiple lumps
		= Other files are single lumps with the base filename for the lump name
		=
		====================
		*/

		public static void W_AddFile(string filename)
		{
			w_wad.wadinfo_t header = new w_wad.wadinfo_t();
			uint i;
			BinaryReader handle = null;
			int startlump;
			filelump_t[] fileinfo;
			filelump_t singleinfo = new filelump_t();

			//
			// open the file and add to directory
			//	
			if (!File.Exists("Content/" + filename))
				return;

			startlump = numlumps;

			if (!filename.Substring(filename.Length - 3).Equals("wad", StringComparison.OrdinalIgnoreCase))
			{
				// single lump file
				fileinfo = new filelump_t[] { singleinfo };
				singleinfo.filepos = 0;
				singleinfo.size = (int)((new FileInfo("Content/" + filename)).Length);
				ExtractFileBase(filename, out singleinfo.name);
				numlumps++;
			}
			else
			{
				// WAD file
				handle = new BinaryReader(File.Open("Content/" + filename, FileMode.Open));
				for (i = 0; i < 4; ++i) header.identification += (char)handle.ReadByte();
				header.numlumps = handle.ReadInt32();
				header.infotableofs = handle.ReadInt32();
				if (header.identification != "IWAD")
				{
					if (header.identification != "PWAD")
						i_ibm.I_Error("Wad file " + filename + " doesn't have IWAD or PWAD id\n");
				}
				fileinfo = new filelump_t[header.numlumps];
				handle.BaseStream.Seek(header.infotableofs, SeekOrigin.Begin);
				for (i = 0; i < header.numlumps; ++i)
				{
					filelump_t fileInfo = new filelump_t();
					fileInfo.filepos = handle.ReadInt32();
					fileInfo.size = handle.ReadInt32();
					for (uint j = 0; j < 8; ++j)
					{
						char c = (char)handle.ReadByte();
						if (c != '\0') fileInfo.name += c;
					}
					fileinfo[i] = fileInfo;
				}
				numlumps += header.numlumps;
			}

			//
			// Fill in lumpinfo
			//
			foreach (filelump_t fileInfo in fileinfo)
			{
				DoomDef.lumpinfo_t lumpInfo = new DoomDef.lumpinfo_t();
				lumpInfo.handle = handle;
				lumpInfo.position = fileInfo.filepos;
				lumpInfo.size = fileInfo.size;
				lumpInfo.name = fileInfo.name;
				lumpinfo.Add(lumpInfo);
			}
		}

		/*
		====================
		=
		= W_InitMultipleFiles
		=
		= Pass a null terminated list of files to use.
		=
		= All files are optional, but at least one file must be found
		=
		= Files with a .wad extension are idlink files with multiple lumps
		=
		= Other files are single lumps with the base filename for the lump name
		=
		= Lump names can appear multiple times. The name searcher looks backwards,
		= so a later file can override an earlier one.
		=
		====================
		*/

		public static void W_InitMultipleFiles(string[] filenames)
		{
			int size;

			//
			// open all the files, load headers, and count lumps
			//
			numlumps = 0;
			lumpinfo = new List<DoomDef.lumpinfo_t>();
			foreach (string filename in filenames)
				w_wad.W_AddFile(filename);

			if (numlumps == 0)
				i_ibm.I_Error("W_InitFiles: no files found");

			//
			// set up caching
			//
			//TODO: Investigate cache later, we might not need it
	//size = numlumps * sizeof(*lumpcache);
	//lumpcache = malloc (size);
	//if (!lumpcache)
	//    I_Error ("Couldn't allocate lumpcache");
	//memset (lumpcache,0, size);
		}

#if DOS

/*
====================
=
= W_InitFile
=
= Just initialize from a single file
=
====================
*/

void W_InitFile (char *filename)
{
	char	*names[2];

	names[0] = filename;
	names[1] = NULL;
	W_InitMultipleFiles (names);
}



/*
====================
=
= W_NumLumps
=
====================
*/

int	W_NumLumps (void)
{
	return numlumps;
}


#endif
/*
====================
=
= W_CheckNumForName
=
= Returns -1 if name not found
=
====================
*/

public static int	W_CheckNumForName (string name)
{
	string	name8;

// make the name into two integers for easy compares

	name8 = name.ToUpper();

// scan backwards so patch lump files take precedence

	int i = 0;
	foreach (DoomDef.lumpinfo_t lumpInfo in lumpinfo)
	{
		if (lumpInfo.name.ToUpper() == name8) return i;
		++i;
	}

	return -1;
}

/*
====================
=
= W_GetNumForName
=
= Calls W_CheckNumForName, but bombs out if not found
=
====================
*/

public static int	W_GetNumForName (string name)
{
	int	i;

	i = W_CheckNumForName (name);
	if (i != -1)
		return i;

	i_ibm.I_Error("W_GetNumForName: " + name + " not found!");
	return -1;
}


/*
====================
=
= W_LumpLength
=
= Returns the buffer size needed to load the given lump
=
====================
*/

public static int W_LumpLength (int lump)
{
	if (lump >= numlumps)
		i_ibm.I_Error("W_LumpLength: " + lump + " >= numlumps");
	return lumpinfo[lump].size;
}


/*
====================
=
= W_ReadLump
=
= Loads the lump into the given buffer, which must be >= W_LumpLength()
=
====================
*/

public static void W_ReadLump (int lump, CacheInfo dest)
{
	DoomDef.lumpinfo_t	l;
	
	if (lump >= numlumps)
		i_ibm.I_Error("W_ReadLump: " + lump + " >= numlumps");
	l = lumpinfo[lump];
	
	i_ibm.I_BeginRead ();

	l.handle.BaseStream.Seek(l.position, SeekOrigin.Begin);
	dest.data = l.handle.ReadBytes(l.size);

	i_ibm.I_EndRead();
}


/*
====================
=
= W_CacheLumpNum
=
====================
*/

public class CacheInfo
{
	public object cache = null;
	public DoomDef.lumpinfo_t lumpInfo = null;
	public int tag = -1;
	public byte[] data = null;
}

public static Dictionary<int, CacheInfo> cacheInfos = new Dictionary<int, CacheInfo>();

public static CacheInfo W_CacheLumpNum (int lump, int tag)
{
	CacheInfo cacheInfo;

	if (lump == -1) return null;
	if ((uint)lump >= (uint)numlumps)
		i_ibm.I_Error("W_CacheLumpNum: " + lump + " >= numlumps");
		
	if (!cacheInfos.ContainsKey(lump))
	{	// read the lump in
		cacheInfo = new CacheInfo();
		cacheInfo.lumpInfo = lumpinfo[lump];
		W_ReadLump(lump, cacheInfo);
		cacheInfos[lump] = cacheInfo;
	}
	cacheInfo = cacheInfos[lump];
	cacheInfo.tag = tag;

	return cacheInfo;
}

/*
====================
=
= W_CacheLumpName
=
====================
*/

public static CacheInfo W_CacheLumpName (string name, int tag)
{
	return W_CacheLumpNum (W_GetNumForName(name), tag);
}


#if DOS
/*
====================
=
= W_Profile
=
====================
*/

// Ripped out for Heretic
/*
int	info[2500][10];
int	profilecount;

void W_Profile (void)
{
	int		i;
	memblock_t	*block;
	void	*ptr;
	char	ch;
	FILE	*f;
	int		j;
	char	name[9];
	
	
	for (i=0 ; i<numlumps ; i++)
	{	
		ptr = lumpcache[i];
		if (!ptr)
		{
			ch = ' ';
			continue;
		}
		else
		{
			block = (memblock_t *) ( (byte *)ptr - sizeof(memblock_t));
			if (block.tag < PU_PURGELEVEL)
				ch = 'S';
			else
				ch = 'P';
		}
		info[i][profilecount] = ch;
	}
	profilecount++;
	
	f = fopen ("waddump.txt","w");
	name[8] = 0;
	for (i=0 ; i<numlumps ; i++)
	{
		memcpy (name,lumpinfo[i].name,8);
		for (j=0 ; j<8 ; j++)
			if (!name[j])
				break;
		for ( ; j<8 ; j++)
			name[j] = ' ';
		fprintf (f,"%s ",name);
		for (j=0 ; j<profilecount ; j++)
			fprintf (f,"    %c",info[i][j]);
		fprintf (f,"\n");
	}
	fclose (f);
}
*/
#endif
	}
}
