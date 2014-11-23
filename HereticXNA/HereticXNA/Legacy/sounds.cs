using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

// sounds.c

namespace HereticXNA
{
	public static class sounds
	{

		// sounds.h

		public const int MAX_SND_DIST = 1600;
		public const int MAX_CHANNELS = 16;

		// Music identifiers

		public enum musicenum_t
		{
			mus_e1m1,
			mus_e1m2,
			mus_e1m3,
			mus_e1m4,
			mus_e1m5,
			mus_e1m6,
			mus_e1m7,
			mus_e1m8,
			mus_e1m9,

			mus_e2m1,
			mus_e2m2,
			mus_e2m3,
			mus_e2m4,
			mus_e2m5,
			mus_e2m6,
			mus_e2m7,
			mus_e2m8,
			mus_e2m9,

			mus_e3m1,
			mus_e3m2,
			mus_e3m3,
			mus_e3m4,
			mus_e3m5,
			mus_e3m6,
			mus_e3m7,
			mus_e3m8,
			mus_e3m9,

			mus_e4m1,
			mus_e4m2,
			mus_e4m3,
			mus_e4m4,
			mus_e4m5,
			mus_e4m6,
			mus_e4m7,
			mus_e4m8,
			mus_e4m9,

			mus_e5m1,
			mus_e5m2,
			mus_e5m3,
			mus_e5m4,
			mus_e5m5,
			mus_e5m6,
			mus_e5m7,
			mus_e5m8,
			mus_e5m9,

			mus_e6m1,
			mus_e6m2,
			mus_e6m3,

			mus_titl,
			mus_intr,
			mus_cptd,
			NUMMUSIC
		}

		public class musicinfo_t
		{
			public string name;
			public int p1;
		}

		public class sfxinfo_t
		{
			public string name;
			public sfxinfo_t link; // Make alias for another sound
			public ushort priority; // Higher priority takes precendence
			public int usefulness; // Determines when a sound should be cached out
			public SoundEffect snd_ptr;
			public int lumpnum;
			public int numchannels; // total number of channels a sound type may occupy
		}

		public class channel_t
		{
			public DoomDef.mobj_t mo;
			public long sound_id;
			public long handle;
			public long pitch;
			public int priority;

			public void reset()
			{
				mo = null;
				sound_id = 0;
				handle = 0;
				pitch = 0;
				priority = 0;
			}
		}

		public class ChanInfo_t
		{
			public long id;
			public ushort priority;
			public string name;
			public DoomDef.mobj_t mo;
			public int distance;
		}

		public class SoundInfo_t
		{
			public int channelCount;
			public int musicVolume;
			public int soundVolume;
			public ChanInfo_t[] chan = new ChanInfo_t[]{
		new ChanInfo_t(),
		new ChanInfo_t(),
		new ChanInfo_t(),
		new ChanInfo_t(),
		new ChanInfo_t(),
		new ChanInfo_t(),
		new ChanInfo_t(),
		new ChanInfo_t()
	};
		}

		// Sound identifiers

		public enum sfxenum_t
		{
			sfx_None,
			sfx_gldhit,
			sfx_gntful,
			sfx_gnthit,
			sfx_gntpow,
			sfx_gntact,
			sfx_gntuse,
			sfx_phosht,
			sfx_phohit,
			sfx_phopow,
			sfx_lobsht,
			sfx_lobhit,
			sfx_lobpow,
			sfx_hrnsht,
			sfx_hrnhit,
			sfx_hrnpow,
			sfx_ramphit,
			sfx_ramrain,
			sfx_bowsht,
			sfx_stfhit,
			sfx_stfpow,
			sfx_stfcrk,
			sfx_impsit,
			sfx_impat1,
			sfx_impat2,
			sfx_impdth,
			sfx_impact,
			sfx_imppai,
			sfx_mumsit,
			sfx_mumat1,
			sfx_mumat2,
			sfx_mumdth,
			sfx_mumact,
			sfx_mumpai,
			sfx_mumhed,
			sfx_bstsit,
			sfx_bstatk,
			sfx_bstdth,
			sfx_bstact,
			sfx_bstpai,
			sfx_clksit,
			sfx_clkatk,
			sfx_clkdth,
			sfx_clkact,
			sfx_clkpai,
			sfx_snksit,
			sfx_snkatk,
			sfx_snkdth,
			sfx_snkact,
			sfx_snkpai,
			sfx_kgtsit,
			sfx_kgtatk,
			sfx_kgtat2,
			sfx_kgtdth,
			sfx_kgtact,
			sfx_kgtpai,
			sfx_wizsit,
			sfx_wizatk,
			sfx_wizdth,
			sfx_wizact,
			sfx_wizpai,
			sfx_minsit,
			sfx_minat1,
			sfx_minat2,
			sfx_minat3,
			sfx_mindth,
			sfx_minact,
			sfx_minpai,
			sfx_hedsit,
			sfx_hedat1,
			sfx_hedat2,
			sfx_hedat3,
			sfx_heddth,
			sfx_hedact,
			sfx_hedpai,
			sfx_sorzap,
			sfx_sorrise,
			sfx_sorsit,
			sfx_soratk,
			sfx_soract,
			sfx_sorpai,
			sfx_sordsph,
			sfx_sordexp,
			sfx_sordbon,
			sfx_sbtsit,
			sfx_sbtatk,
			sfx_sbtdth,
			sfx_sbtact,
			sfx_sbtpai,
			sfx_plroof,
			sfx_plrpai,
			sfx_plrdth,		// Normal
			sfx_gibdth,		// Extreme
			sfx_plrwdth,	// Wimpy
			sfx_plrcdth,	// Crazy
			sfx_itemup,
			sfx_wpnup,
			sfx_telept,
			sfx_doropn,
			sfx_dorcls,
			sfx_dormov,
			sfx_artiup,
			sfx_switch,
			sfx_pstart,
			sfx_pstop,
			sfx_stnmov,
			sfx_chicpai,
			sfx_chicatk,
			sfx_chicdth,
			sfx_chicact,
			sfx_chicpk1,
			sfx_chicpk2,
			sfx_chicpk3,
			sfx_keyup,
			sfx_ripslop,
			sfx_newpod,
			sfx_podexp,
			sfx_bounce,
			sfx_volsht,
			sfx_volhit,
			sfx_burn,
			sfx_splash,
			sfx_gloop,
			sfx_respawn,
			sfx_blssht,
			sfx_blshit,
			sfx_chat,
			sfx_artiuse,
			sfx_gfrag,
			sfx_waterfl,

			// Monophonic sounds

			sfx_wind,
			sfx_amb1,
			sfx_amb2,
			sfx_amb3,
			sfx_amb4,
			sfx_amb5,
			sfx_amb6,
			sfx_amb7,
			sfx_amb8,
			sfx_amb9,
			sfx_amb10,
			sfx_amb11,
			NUMSFX
		}


		// Music info

		static public musicinfo_t[] S_music = new musicinfo_t[]
{
	new musicinfo_t { name = "MUS_E1M1", p1 = 0 }, // 1-1
	new musicinfo_t { name = "MUS_E1M2", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M3", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M4", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M5", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M6", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M7", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M8", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M9", p1 = 0 },

	new musicinfo_t { name = "MUS_E2M1", p1 = 0 }, // 2-1
	new musicinfo_t { name = "MUS_E2M2", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M3", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M4", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M4", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M6", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M7", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M8", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M9", p1 = 0 },

	new musicinfo_t { name = "MUS_E1M1", p1 = 0 }, // 3-1
	new musicinfo_t { name = "MUS_E3M2", p1 = 0 },
	new musicinfo_t { name = "MUS_E3M3", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M6", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M3", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M2", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M5", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M9", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M6", p1 = 0 },

	new musicinfo_t { name = "MUS_E1M6", p1 = 0 }, // 4-1
	new musicinfo_t { name = "MUS_E1M2", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M3", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M4", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M5", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M1", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M7", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M8", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M9", p1 = 0 },

	new musicinfo_t { name = "MUS_E2M1", p1 = 0 }, // 5-1
	new musicinfo_t { name = "MUS_E2M2", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M3", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M4", p1 = 0 },
	new musicinfo_t { name = "MUS_E1M4", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M6", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M7", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M8", p1 = 0 },
	new musicinfo_t { name = "MUS_E2M9", p1 = 0 },

	new musicinfo_t { name = "MUS_E3M2", p1 = 0 }, // 6-1
	new musicinfo_t { name = "MUS_E3M3", p1 = 0 }, // 6-2
	new musicinfo_t { name = "MUS_E1M6", p1 = 0 }, // 6-3

	new musicinfo_t { name = "MUS_TITL", p1 = 0 },
	new musicinfo_t { name = "MUS_INTR", p1 = 0 },
	new musicinfo_t { name = "MUS_CPTD", p1 = 0 }
};

		public static sfxinfo_t[] S_sfx = new sfxinfo_t[]
{
	new sfxinfo_t { name = "", link = null, priority = 0, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 0 },
	new sfxinfo_t { name = "gldhit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "gntful", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = -1 },
	new sfxinfo_t { name = "gnthit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = -1 },
	new sfxinfo_t { name = "gntpow", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = -1 },
	new sfxinfo_t { name = "gntact", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = -1 },
	new sfxinfo_t { name = "gntuse", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = -1 },
	new sfxinfo_t { name = "phosht", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "phohit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = -1 },
	new sfxinfo_t { name = "hedat1", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "lobsht", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "lobhit", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "lobpow", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "hrnsht", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "hrnhit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "hrnpow", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "ramphit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "ramrain", link = null, priority = 10, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "bowsht", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "stfhit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "stfpow", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "stfcrk", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "impsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "impat1", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "impat2", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "impdth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "impsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "imppai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "mumsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "mumat1", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "mumat2", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "mumdth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "mumsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "mumpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "mumhed", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "bstsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "bstatk", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "bstdth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "bstact", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "bstpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "clksit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "clkatk", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "clkdth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "clkact", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "clkpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "snksit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "snkatk", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "snkdth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "snkact", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "snkpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "kgtsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "kgtatk", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "kgtat2", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "kgtdth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "kgtsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "kgtpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "wizsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "wizatk", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "wizdth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "wizact", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "wizpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "minsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "minat1", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "minat2", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "minat3", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "mindth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "minact", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "minpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "hedsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "hedat1", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "hedat2", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "hedat3", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "heddth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "hedact", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "hedpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "sorzap", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "sorrise", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "sorsit", link = null, priority = 200, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "soratk", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "soract", link = null, priority = 200, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "sorpai", link = null, priority = 200, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "sordsph", link = null, priority = 200, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "sordexp", link = null, priority = 200, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "sordbon", link = null, priority = 200, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "bstsit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "bstatk", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "sbtdth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "sbtact", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "sbtpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "plroof", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "plrpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
 	new sfxinfo_t { name = "plrdth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "gibdth", link = null, priority = 100, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "plrwdth", link = null, priority = 80, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "plrcdth", link = null, priority = 100, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "itemup", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "wpnup", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "telept", link = null, priority = 50, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "doropn", link = null, priority = 40, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "dorcls", link = null, priority = 40, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "dormov", link = null, priority = 40, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "artiup", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "switch", link = null, priority = 40, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "pstart", link = null, priority = 40, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "pstop", link = null, priority = 40, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "stnmov", link = null, priority = 40, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "chicpai", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "chicatk", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "chicdth", link = null, priority = 40, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "chicact", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "chicpk1", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "chicpk2", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "chicpk3", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "keyup", link = null, priority = 50, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "ripslop", link = null, priority = 16, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "newpod", link = null, priority = 16, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = -1 },
	new sfxinfo_t { name = "podexp", link = null, priority = 40, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = -1 },
	new sfxinfo_t { name = "bounce", link = null, priority = 16, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "bstatk", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "lobhit", link = null, priority = 20, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "burn", link = null, priority = 10, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "splash", link = null, priority = 10, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "gloop", link = null, priority = 10, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "respawn", link = null, priority = 10, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "blssht", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "blshit", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },
	new sfxinfo_t { name = "chat", link = null, priority = 100, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "artiuse", link = null, priority = 32, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "gfrag", link = null, priority = 100, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "waterfl", link = null, priority = 16, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 2 },

	// Monophonic sounds

	new sfxinfo_t { name = "wind", link = null, priority = 16, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb1", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb2", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb3", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb4", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb5", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb6", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb7", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb8", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb9", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb10", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 1 },
	new sfxinfo_t { name = "amb11", link = null, priority = 1, usefulness = -1, snd_ptr = null, lumpnum = 0, numchannels = 0 }
};

	}
}
