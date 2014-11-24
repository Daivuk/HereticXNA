using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HereticXNA
{
	public static class m_misc
	{
		public static string [] args;

		//---------------------------------------------------------------------------
		//
		// FUNC M_ValidEpisodeMap
		//
		//---------------------------------------------------------------------------

		public static bool M_ValidEpisodeMap(int episode, int map)
		{
			if(episode < 1 || map < 1 || map > 9)
			{
				return false;
			}
			if(d_main.shareware)
			{ // Shareware version checks
				if(episode != 1)
				{
					return false;
				}
			}
			else if(d_main.ExtendedWAD)
			{ // Extended version checks
				if(episode == 6)
				{
					if(map > 3)
					{
						return false;
					}
				}
				else if(episode > 5)
				{
					return false;
				}
			}
			else
			{ // Registered version checks
				if(episode == 4)
				{
					if(map != 1)
					{
						return false;
					}
				}
				else if(episode > 3)
				{
					return false;
				}
			}
			return true;
		}

/*
=================
=
= M_CheckParm
=
= Checks for the given parameter in the program's command line arguments
=
= Returns the argument number (1 to argc-1) or 0 if not present
=
=================
*/

public static int M_CheckParm (string check)
{
	int     i;

	for (i = 1;i<args.Length;i++)
	{
		if (string.Equals(check, args[i], StringComparison.OrdinalIgnoreCase))
			return i;
	}

	return 0;
}


/*
===============
=
= M_Random
=
= Returns a 0-255 number
=
===============
*/

static public byte[] rndtable = new byte[256] {
	0,   8, 109, 220, 222, 241, 149, 107,  75, 248, 254, 140,  16,  66,
	74,  21, 211,  47,  80, 242, 154,  27, 205, 128, 161,  89,  77,  36,
	95, 110,  85,  48, 212, 140, 211, 249,  22,  79, 200,  50,  28, 188,
	52, 140, 202, 120,  68, 145,  62,  70, 184, 190,  91, 197, 152, 224,
	149, 104,  25, 178, 252, 182, 202, 182, 141, 197,   4,  81, 181, 242,
	145,  42,  39, 227, 156, 198, 225, 193, 219,  93, 122, 175, 249,   0,
	175, 143,  70, 239,  46, 246, 163,  53, 163, 109, 168, 135,   2, 235,
	25,  92,  20, 145, 138,  77,  69, 166,  78, 176, 173, 212, 166, 113,
	94, 161,  41,  50, 239,  49, 111, 164,  70,  60,   2,  37, 171,  75,
	136, 156,  11,  56,  42, 146, 138, 229,  73, 146,  77,  61,  98, 196,
	135, 106,  63, 197, 195,  86,  96, 203, 113, 101, 170, 247, 181, 113,
	80, 250, 108,   7, 255, 237, 129, 226,  79, 107, 112, 166, 103, 241,
	24, 223, 239, 120, 198,  58,  60,  82, 128,   3, 184,  66, 143, 224,
	145, 224,  81, 206, 163,  45,  63,  90, 168, 114,  59,  33, 159,  95,
	28, 139, 123,  98, 125, 196,  15,  70, 194, 253,  54,  14, 109, 226,
	71,  17, 161,  93, 186,  87, 244, 138,  20,  52, 123, 251,  26,  36,
	17,  46,  52, 231, 232,  76,  31, 221,  84,  37, 216, 165, 212, 106,
	197, 242,  98,  43,  39, 175, 254, 145, 190,  84, 118, 222, 187, 136,
	120, 163, 236, 249
};

public static int rndindex = 0;
public static int prndindex = 0;

public static int P_Random ()
{
	prndindex = (prndindex+1)&0xff;
	return rndtable[prndindex];
}

public static int P_RandomWithSeed(int in_offset)
{
	return rndtable[in_offset % 0xff];
}

public static int M_Random ()
{
	rndindex = (rndindex+1)&0xff;
	return rndtable[rndindex];
}

public static void M_ClearRandom ()
{
	rndindex = prndindex = 0;
}


public static void M_ClearBox(int[] box)
{
	box[(int)DoomData.eUnknownEnumType2.BOXTOP] = box[(int)DoomData.eUnknownEnumType2.BOXRIGHT] = DoomDef.MININT;
	box[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] = box[(int)DoomData.eUnknownEnumType2.BOXLEFT] = DoomDef.MAXINT;
}

public static void M_AddToBox(int[] box, int x, int y)
{
	if (x < box[(int)DoomData.eUnknownEnumType2.BOXLEFT])
		box[(int)DoomData.eUnknownEnumType2.BOXLEFT] = x;
	else if (x > box[(int)DoomData.eUnknownEnumType2.BOXRIGHT])
		box[(int)DoomData.eUnknownEnumType2.BOXRIGHT] = x;
	if (y < box[(int)DoomData.eUnknownEnumType2.BOXBOTTOM])
		box[(int)DoomData.eUnknownEnumType2.BOXBOTTOM] = y;
	else if (y > box[(int)DoomData.eUnknownEnumType2.BOXTOP])
		box[(int)DoomData.eUnknownEnumType2.BOXTOP] = y;
}

#if DOS


/*
==================
=
= M_WriteFile
=
==================
*/

//TODO: public const int O_BINARY 0

boolean M_WriteFile (char const *name, void *source, int length)
{
	int handle, count;

	handle = open (name, O_WRONLY | O_CREAT | O_TRUNC | O_BINARY, 0666);
	if (handle == -1)
		return false;
	count = write (handle, source, length);
	close (handle);

	if (count < length)
		return false;

	return true;
}


/*
==================
=
= M_ReadFile
=
==================
*/

int M_ReadFile (char const *name, byte **buffer)
{
	int handle, count, length;
	struct stat fileinfo;
	byte        *buf;

	handle = open (name, O_RDONLY | O_BINARY, 0666);
	if (handle == -1)
		I_Error ("Couldn't read file %s", name);
	if (fstat (handle,&fileinfo) == -1)
		I_Error ("Couldn't read file %s", name);
	length = fileinfo.st_size;
	buf = Z_Malloc (length, PU_STATIC, NULL);
	count = read (handle, buf, length);
	close (handle);

	if (count < length)
		I_Error ("Couldn't read file %s", name);

	*buffer = buf;
	return length;
}
#endif
		//---------------------------------------------------------------------------
		//
		// PROC M_FindResponseFile
		//
		//---------------------------------------------------------------------------
		public const int MAXARGVS = 100;

		static public void M_FindResponseFile()
		{
			return; // [dsl] Ignore this for now
			/*	int i;

				for(i = 1; i < args.Length; i++)
				{
					if(args[i][0] == '@')
					{
						FILE *handle;
						int size;
						int k;
						int index;
						int indexinfile;
						char *infile;
						char *file;
						char *moreargs[20];
						char *firstargv;

						// READ THE RESPONSE FILE INTO MEMORY
						handle = fopen(&myargv[i][1], "rb");
						if(!handle)
						{

							printf("\nNo such response file!");
							exit(1);
						}
						printf("Found response file %s!\n",&myargv[i][1]);
						fseek (handle,0,SEEK_END);
						size = ftell(handle);
						fseek (handle,0,SEEK_SET);
						file = malloc (size);
						fread (file,size,1,handle);
						fclose (handle);

						// KEEP ALL CMDLINE ARGS FOLLOWING @RESPONSEFILE ARG
						for (index = 0,k = i+1; k < myargc; k++)
							moreargs[index++] = myargv[k];
			
						firstargv = myargv[0];
						myargv = malloc(sizeof(char *)*MAXARGVS);
						memset(myargv,0,sizeof(char *)*MAXARGVS);
						myargv[0] = firstargv;
			
						infile = file;
						indexinfile = k = 0;
						indexinfile++;  // SKIP PAST ARGV[0] (KEEP IT)
						do
						{
							myargv[indexinfile++] = infile+k;
							while(k < size &&  

								((*(infile+k)>= ' '+1) && (*(infile+k)<='z')))
								k++;
							*(infile+k) = 0;
							while(k < size &&
								((*(infile+k)<= ' ') || (*(infile+k)>'z')))
								k++;
						} while(k < size);
			
						for (k = 0;k < index;k++)
							myargv[indexinfile++] = moreargs[k];
						myargc = indexinfile;
						// DISPLAY ARGS
						if(M_CheckParm("-debug"))
						{
							printf("%d command-line args:\n", myargc);
							for(k = 1; k < myargc; k++)
							{
								printf("%s\n", myargv[k]);
							}
						}
						break;
					}
				}*/
		}
#if DOS
//---------------------------------------------------------------------------
//
// PROC M_ForceUppercase
//
// Change string to uppercase.
//
//---------------------------------------------------------------------------

void M_ForceUppercase(char *text)
{
	char c;

	while((c = *text) != 0)
	{
		if(c >= 'a' && c <= 'z')
		{
			*text++ = c-('a'-'A');
		}
		else
		{
			text++;
		}
	}
}
		#endif

/*
==============================================================================

							DEFAULTS

==============================================================================
*/

		public static var_int usemouse = new var_int();
		public static var_int usejoystick = new var_int();

public class Var
{
	virtual public void copy(Var in_other) { }
}
public class var_int : Var
{
	public var_int() {}
	public var_int(int in_val) {val = in_val;}
	public int val;
	override public void copy(Var in_other) { val = (in_other as var_int).val; }
}
public class var_bool : Var
{
	public var_bool() {}
	public var_bool(bool in_val) { val = in_val; }
	public bool val;
	override public void copy(Var in_other) { val = (in_other as var_bool).val; }
}
public class var_string : Var
{
	public var_string() { }
	public var_string(string in_val) { val = in_val; }
	public string val;
	override public void copy(Var in_other) { val = (in_other as var_string).val; }
}
//public static var_int mouseSensitivity = new var_int();

public class default_t
{
	public default_t(string in_name, Var in_location, Var in_defaultvalue)
	{
		name = in_name;
		location = in_location;
		defaultvalue = in_defaultvalue; // [dsl] The settings specify it now
	}
	public string   name;
	public Var		location;
	public Var     defaultvalue;
	public int     scantranslate;      // PC scan code hack
	public int     untranslated;       // lousy hack
}

static public default_t[] defaults = new default_t[]
{
	//new default_t("mouse_sensitivity", mouseSensitivity, new var_int(3)),

	//new default_t( "sfx_volume", i_sound.snd_MaxVolume,new var_int( 10)),
	//new default_t( "music_volume", i_sound.snd_MusicVolume, new var_int(10)),


	new default_t( "use_mouse", usemouse, new var_int(1) ),
	new default_t( "mouseb_fire", g_game.mousebfire, new var_int(0) ),
	new default_t( "mouseb_strafe", g_game.mousebstrafe, new var_int(1 )),
	new default_t( "mouseb_forward", g_game.mousebforward, new var_int(2) ),

	new default_t( "use_joystick", usejoystick, new var_int(0 )),
	new default_t( "joyb_fire", g_game.joybfire, new var_int(0) ),
	new default_t( "joyb_strafe", g_game.joybstrafe, new var_int(1 )),
	new default_t( "joyb_use", g_game.joybuse,new var_int( 3) ),
	new default_t( "joyb_speed", g_game.joybspeed, new var_int(2 )),

	new default_t( "screenblocks", r_main.screenblocks, new var_int(10 )),

	new default_t( "snd_channels", i_sound.snd_Channels,new var_int( 3) ),
	new default_t( "snd_musicdevice", i_sound.snd_DesiredMusicDevice, new var_int(0) ),
	new default_t( "snd_sfxdevice", i_sound.snd_DesiredSfxDevice, new var_int(0 )),
	new default_t( "snd_sbport", i_sound.snd_SBport,new var_int( 544) ),
	new default_t( "snd_sbirq", i_sound.snd_SBirq, new var_int(-1) ),
	new default_t( "snd_sbdma", i_sound.snd_SBdma,new var_int( -1) ),
	new default_t( "snd_mport", i_sound.snd_Mport, new var_int(-1) ),

	new default_t( "usegamma", v_video.usegamma,new var_int( 0 )),

	new default_t( "chatmacro0", ct_chat.chat_macros[0], new var_string(dstring.HUSTR_CHATMACRO0) ),
	new default_t( "chatmacro1", ct_chat.chat_macros[1], new var_string(dstring.HUSTR_CHATMACRO1 )),
	new default_t( "chatmacro2", ct_chat.chat_macros[2], new var_string(dstring.HUSTR_CHATMACRO2 )),
	new default_t( "chatmacro3", ct_chat.chat_macros[3], new var_string(dstring.HUSTR_CHATMACRO3 )),
	new default_t( "chatmacro4", ct_chat.chat_macros[4], new var_string(dstring.HUSTR_CHATMACRO4 )),
	new default_t( "chatmacro5", ct_chat.chat_macros[5], new var_string(dstring.HUSTR_CHATMACRO5 )),
	new default_t( "chatmacro6", ct_chat.chat_macros[6], new var_string(dstring.HUSTR_CHATMACRO6 )),
	new default_t( "chatmacro7", ct_chat.chat_macros[7], new var_string(dstring.HUSTR_CHATMACRO7 )),
	new default_t( "chatmacro8", ct_chat.chat_macros[8], new var_string(dstring.HUSTR_CHATMACRO8 )),
	new default_t( "chatmacro9", ct_chat.chat_macros[9], new var_string(dstring.HUSTR_CHATMACRO9 )),

	//new default_t( "ambient_enabled", v_video.ambient_enabled, new var_bool( true )),
	//new default_t( "use_deferred", v_video.use_deferred, new var_bool( true )),
	//new default_t( "postprocess_enabled", v_video.postprocess_enabled, new var_bool( true ))
};
public static int numdefaults;
public static string defaultfile;
#if DOS

/*
==============
=
= M_SaveDefaults
=
==============
*/

void M_SaveDefaults (void)
{
	int     i,v;
	FILE    *f;

	f = fopen (defaultfile, "w");
	if (!f)
		return;         // can't write the file, but don't complain

	for (i=0 ; i<numdefaults ; i++)
	{
		if (defaults[i].defaultvalue > -0xfff
		  && defaults[i].defaultvalue < 0xfff)
		{
			v = *defaults[i].location;
			fprintf (f,"%s\t\t%i\n",defaults[i].name,v);
		} else {
			fprintf (f,"%s\t\t\"%s\"\n",defaults[i].name,
			  * (char **) (defaults[i].location));
		}
	}

	fclose (f);
}

#endif
/*
==============
=
= M_LoadDefaults
=
==============
*/

public static void M_LoadDefaults ()
{
	int     i;
	string    def;
	string        strparm = "";
	string    newstring = "";
	int     parm = 0;
	bool     isstring;

//
// set everything to base values
//
	numdefaults = defaults.Length;
	for (i = 0; i < numdefaults; i++)
		defaults[i].location.copy(defaults[i].defaultvalue);


//
// check for a custom default file
//
/*	i = M_CheckParm("-config");
	if(i != 0 && i<args.Length-1)
	{
		defaultfile = args[i + 1];
		Console.Write("default file: " + defaultfile + "\n");
	}
	else if(d_main.cdrom)
	{
		defaultfile = "c:\\heretic.cd\\heretic.cfg";
	}
	else
	{
		defaultfile = d_main.basedefault;
	}

//
// read the file in, overriding any set defaults
//
	if (File.Exists(defaultfile))
	{
		System.IO.StreamReader f =
			new System.IO.StreamReader(defaultfile);
		while (!f.EndOfStream)
		{
			isstring = false;
			string line = f.ReadLine();
			string[] split = line.Split(new char[]{' '});
			if (split.Length == 2)
			{
				def = split[0];
				strparm = split[1];
			  if (strparm[0] == '"')
			  {
				// get a string default
				isstring = true;
				newstring = strparm.Substring(1, strparm.Length - 2);
			  }
			  else if (strparm[0] == '0' && strparm[1] == 'x')
				  parm = int.Parse(strparm, System.Globalization.NumberStyles.AllowHexSpecifier);
			  else
				  parm = int.Parse(strparm);
			  for (i=0 ; i<numdefaults ; i++)
				  if (def == defaults[i].name)
				  {
					  if (!isstring)
						  (defaults[i].location as var_int).val = parm;
					  else
						  (defaults[i].location as var_string).val = newstring;
					  break;
				  }
			}
		}
	}*/

}
#if DOS

/*
==============================================================================

						SCREEN SHOTS

==============================================================================
*/


typedef struct
{
	char    manufacturer;
	char    version;
	char    encoding;
	char    bits_per_pixel;
	unsigned short  xmin,ymin,xmax,ymax;
	unsigned short  hres,vres;
	unsigned char   palette[48];
	char    reserved;
	char    color_planes;
	unsigned short  bytes_per_line;
	unsigned short  palette_type;
	char    filler[58];
	unsigned char   data;           // unbounded
} pcx_t;

/*
==============
=
= WritePCXfile
=
==============
*/

void WritePCXfile (char *filename, byte *data, int width, int height, byte *palette)
{
	int     i, length;
	pcx_t   *pcx;
	byte        *pack;
	
	pcx = Z_Malloc (width*height*2+1000, PU_STATIC, NULL);

	pcx.manufacturer = 0x0a;   // PCX id
	pcx.version = 5;           // 256 color
	pcx.encoding = 1;      // uncompressed
	pcx.bits_per_pixel = 8;        // 256 color
	pcx.xmin = 0;
	pcx.ymin = 0;
	pcx.xmax = SHORT(width-1);
	pcx.ymax = SHORT(height-1);
	pcx.hres = SHORT(width);
	pcx.vres = SHORT(height);
	memset (pcx.palette,0,sizeof(pcx.palette));
	pcx.color_planes = 1;      // chunky image
	pcx.bytes_per_line = SHORT(width);
	pcx.palette_type = SHORT(2);       // not a grey scale
	memset (pcx.filler,0,sizeof(pcx.filler));

//
// pack the image
//
	pack = &pcx.data;

	for (i=0 ; i<width*height ; i++)
		if ( (*data & 0xc0) != 0xc0)
			*pack++ = *data++;
		else
		{
			*pack++ = 0xc1;
			*pack++ = *data++;
		}

//
// write the palette
//
	*pack++ = 0x0c; // palette ID byte
	for (i=0 ; i<768 ; i++)
		*pack++ = *palette++;

//
// write output file
//
	length = pack - (byte *)pcx;
	M_WriteFile (filename, pcx, length);

	Z_Free (pcx);
}


//==============================================================================

/*
==================
=
= M_ScreenShot
=
==================
*/

void M_ScreenShot (void)
{
	int     i;
	byte    *linear;
	char    lbmname[12];
	byte *pal;

//
// munge planar buffer to linear
//
	linear = screen;
//
// find a file name to save it to
//
	strcpy(lbmname,"HRTIC00.pcx");

	for (i=0 ; i<=99 ; i++)
	{
		lbmname[5] = i/10 + '0';
		lbmname[6] = i%10 + '0';
		if (access(lbmname,0) == -1)
			break;  // file doesn't exist
	}
	if (i==100)
		I_Error ("M_ScreenShot: Couldn't create a PCX");

//
// save the pcx file
//
	pal = (byte *)W_CacheLumpName("PLAYPAL", PU_CACHE);

	WritePCXfile (lbmname, linear, SCREENWIDTH, SCREENHEIGHT
		, pal);

	players[consoleplayer].message = "SCREEN SHOT";
}

#endif
	}
}
