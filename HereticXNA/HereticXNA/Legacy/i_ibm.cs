﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using System.Runtime.InteropServices;
		
// I_IBM.C

namespace HereticXNA
{
	public static class i_ibm
	{

		// Macros

		public const int DPMI_INT = 0x31;

		// Public Data

		static public int DisplayTicker = 0;

		static public float soundScale = 200.0f;

		// Code

		public static void main(string[] args)
		{
			m_misc.args = args;
			d_main.D_DoomMain();
		}

#if DOS

typedef struct
{
	unsigned        edi, esi, ebp, reserved, ebx, edx, ecx, eax;
	unsigned short  flags, es, ds, fs, gs, ip, cs, sp, ss;
} dpmiregs_t;

extern  dpmiregs_t      dpmiregs;

extern  int     usemouse, usejoystick;

extern void **lumpcache;
#endif

/*
===============================================================================

		MUSIC & SFX API

===============================================================================
*/

		static sounds.channel_t[] channel = new sounds.channel_t[sounds.MAX_CHANNELS] {
			new sounds.channel_t(),			new sounds.channel_t(),			new sounds.channel_t(),			new sounds.channel_t(),
			new sounds.channel_t(),			new sounds.channel_t(),			new sounds.channel_t(),			new sounds.channel_t(),
			new sounds.channel_t(),			new sounds.channel_t(),			new sounds.channel_t(),			new sounds.channel_t(),
			new sounds.channel_t(),			new sounds.channel_t(),			new sounds.channel_t(),			new sounds.channel_t()
		};

static int rs; //the current registered song.
public static int mus_song = -1;
public static int mus_lumpnum;
public static w_wad.CacheInfo mus_sndptr;
#if DOS
public static byte* soundCurve;
#endif

public static int AmbChan;

		public static void S_Start()
		{
			int i;

			S_StartSong((g_game.gameepisode - 1) * 9 + g_game.gamemap - 1, true);

			// stop all sounds
			for(i=0; i < i_sound.snd_Channels.val; i++)
			{
			    if(i_ibm.channel[i].handle != 0)
			    {
			        S_StopSound(channel[i].mo);
			    }
				channel[i].reset();
			}
		}

	public static void S_StartSong(int song, bool loop)
	{
		if(song == mus_song)
		{ // don't replay an old song
			return;
		}
		if(rs != 0)
		{
			i_sound.I_StopSong(rs);
			i_sound.I_UnRegisterSong(rs);
		}
		if (song < (int)sounds.musicenum_t.mus_e1m1 || song > (int)sounds.musicenum_t.NUMMUSIC)
		{
			return;
		}
		mus_lumpnum = w_wad.W_GetNumForName(sounds.S_music[song].name);
		mus_sndptr = w_wad.W_CacheLumpNum(mus_lumpnum, DoomDef.PU_MUSIC);
		rs = i_sound.I_RegisterSong(mus_sndptr);
		i_sound.I_PlaySong(rs, loop); //'true' denotes endless looping.
		mus_song = song;
	}

	public static void S_StartSound(int x, int y, int z, int sound_id)
	{
		//DoomDef.mobj_t origin = new DoomDef.mobj_t();
		//origin.x = x;
		//origin.y = y;
		//origin.z = z;
		//S_StartSound(origin, sound_id);

		// Play 3D sound
		SoundEffectInstance instance = sounds.S_sfx[sound_id].snd_ptr.CreateInstance();
		AudioEmitter emitter = new AudioEmitter();
		emitter.Position = new Vector3(
			(x >> DoomDef.FRACBITS),
			(y >> DoomDef.FRACBITS),
			(z >> DoomDef.FRACBITS)) / soundScale;
		instance.Apply3D(Game1.instance.audioListener, emitter);
		instance.Play();
		sndInstances.Add(instance);
		sndEmitters.Add(emitter);
		sndOrigins.Add(new WeakReference(null));
	}

		//static int sndcount = 0;

		static List<SoundEffectInstance> sndInstances = new List<SoundEffectInstance>();
		static List<AudioEmitter> sndEmitters = new List<AudioEmitter>();
		static List<WeakReference> sndOrigins = new List<WeakReference>();

		public static void S_StartSound(DoomDef.mobj_t origin, int sound_id)
		{
			if (sound_id == 50)
			{
				int tmp;
				tmp = 5;
			}
			if (origin == null)
			{
				// Play 2D sound
				sounds.S_sfx[sound_id].snd_ptr.Play((float)(Settings.Default.sfx_volume) / 16.0f, 0, 0);
				return;
			}

			// Play 3D sound
			SoundEffectInstance instance = sounds.S_sfx[sound_id].snd_ptr.CreateInstance();
			AudioEmitter emitter = new AudioEmitter();
			emitter.Position = new Vector3(
				(origin.x >> DoomDef.FRACBITS),
				(origin.y >> DoomDef.FRACBITS),
				(origin.z >> DoomDef.FRACBITS)) / soundScale;
			instance.Apply3D(Game1.instance.audioListener, emitter);
			instance.Play();
			sndInstances.Add(instance);
			sndEmitters.Add(emitter);
			sndOrigins.Add(new WeakReference(origin));

#if DOS
			int dist, vol;
			int i;
			int sound;
			int priority;
			int sep;
			int angle;
			int absx;
			int absy;

			int chan;

			if (sound_id == 0 || i_sound.snd_MaxVolume.val == 0)
				return;
			//if(origin == NULL)
			//{
			//    origin = players[consoleplayer].mo;
			//}

		// calculate the distance before other stuff so that we can throw out
		// sounds that are beyond the hearing range.
			absx = Math.Abs(x-g_game.players[g_game.consoleplayer].mo.x);
			absy = Math.Abs(y - g_game.players[g_game.consoleplayer].mo.y);
			dist = absx+absy-(absx > absy ? absy>>1 : absx>>1);
			dist >>= DoomDef.FRACBITS;

			if(dist >= sounds.MAX_SND_DIST)
			{
			  return; //sound is beyond the hearing range...
			}
			if(dist < 0)
			{
				dist = 0;
			}
			priority = sounds.S_sfx[sound_id].priority;
			priority *= (10 - (dist/160));
			if(!S_StopSoundID(sound_id, priority))
			{
				return; // other sounds have greater priority
			}
			for (i = 0; i < i_sound.snd_Channels.val; i++)
			{
				if(origin.player)
				{
					i = i_sound.snd_Channels.val;
					break; // let the player have more than one sound.
				}
				if(origin == channel[i].mo)
				{ // only allow other mobjs one sound
					S_StopSound(channel[i].mo);
					break;
				}
			}
			if (i >= i_sound.snd_Channels.val)
			{
				if(sound_id >= sfx_wind)
				{
					if(AmbChan != -1 && S_sfx[sound_id].priority <=
						S_sfx[channel[AmbChan].sound_id].priority)
					{
						return; //ambient channel already in use
					}
					else
					{
						AmbChan = -1;
					}
				}
				for (i = 0; i < i_sound.snd_Channels.val; i++)
				{
					if(channel[i].mo == null)
					{
						break;
					}
				}
				if (i >= i_sound.snd_Channels.val)
				{
					//look for a lower priority sound to replace.
					sndcount++;
					if (sndcount >= i_sound.snd_Channels.val)
					{
						sndcount = 0;
					}
					for (chan = 0; chan < i_sound.snd_Channels.val; chan++)
					{
						i = (sndcount + chan) % i_sound.snd_Channels.val;
						if(priority >= channel[i].priority)
						{
							chan = -1; //denote that sound should be replaced.
							break;
						}
					}
					if(chan != -1)
					{
						return; //no free channels.
					}
					else //replace the lower priority sound.
					{
						if(channel[i].handle)
						{
							if(I_SoundIsPlaying(channel[i].handle))
							{
								I_StopSound(channel[i].handle);
							}
							if(S_sfx[channel[i].sound_id].usefulness > 0)
							{
								S_sfx[channel[i].sound_id].usefulness--;
							}

							if(AmbChan == i)
							{
								AmbChan = -1;
							}
						}
					}
				}
			}
			if(S_sfx[sound_id].lumpnum == 0)
			{
				S_sfx[sound_id].lumpnum = I_GetSfxLumpNum(&S_sfx[sound_id]);
			}
			if(S_sfx[sound_id].snd_ptr == NULL)
			{
				S_sfx[sound_id].snd_ptr = W_CacheLumpNum(S_sfx[sound_id].lumpnum,
					PU_SOUND);
			}

			// calculate the volume based upon the distance from the sound origin.
			vol = soundCurve[dist];

			if(origin == players[consoleplayer].mo)
			{
				sep = 128;
			}
			else
			{
				angle = R_PointToAngle2(players[consoleplayer].mo.x,
					players[consoleplayer].mo.y, channel[i].mo.x, channel[i].mo.y);
				angle = (angle-viewangle)>>24;
				sep = angle*2-128;
				if(sep < 64)
					sep = -sep;
				if(sep > 192)
					sep = 512-sep;
			}

			channel[i].pitch = (byte)(127+(M_Random()&7)-(M_Random()&7));
			channel[i].handle = I_StartSound(sound_id, S_sfx[sound_id].snd_ptr, vol, sep, channel[i].pitch, 0);
			channel[i].mo = origin;
			channel[i].sound_id = sound_id;
			channel[i].priority = priority;
			if(sound_id >= sfx_wind)
			{
				AmbChan = i;
			}
			if(S_sfx[sound_id].usefulness == -1)
			{
				S_sfx[sound_id].usefulness = 1;
			}
			else
			{
				S_sfx[sound_id].usefulness++;
			}
#endif
		}
		
		public static void S_StartSoundAtVolume(DoomDef.mobj_t origin, int sound_id, int volume)
		{
			if (origin == null)
			{
				// Play 2D sound
				sounds.S_sfx[sound_id].snd_ptr.Play((float)volume / 127.0f, 0, 0);
				return;
			}

			// Play 3D sound
			SoundEffectInstance instance = sounds.S_sfx[sound_id].snd_ptr.CreateInstance();
			AudioEmitter emitter = new AudioEmitter();
			emitter.Position = new Vector3(
				(origin.x >> DoomDef.FRACBITS),
				(origin.y >> DoomDef.FRACBITS),
				(origin.z >> DoomDef.FRACBITS)) / soundScale;
			instance.Apply3D(Game1.instance.audioListener, emitter);
			instance.Volume = (float)volume / 127.0f;
			instance.Play();
			sndInstances.Add(instance);
			sndEmitters.Add(emitter);
			sndOrigins.Add(new WeakReference(origin));

#if DOS
	int dist;
	int i;
	int sep;

	static int sndcount;
	int chan;

	if(sound_id == 0 || snd_MaxVolume == 0)
		return;
	if(origin == NULL)
	{
		origin = players[consoleplayer].mo;
	}

	if(volume == 0)
	{
		return;
	}
	volume = (volume*(snd_MaxVolume+1)*8)>>7;

// no priority checking, as ambient sounds would be the LOWEST.
	for(i=0; i<snd_Channels; i++)
	{
		if(channel[i].mo == NULL)
		{
			break;
		}
	}
	if(i >= snd_Channels)
	{
		return;
	}
	if(S_sfx[sound_id].lumpnum == 0)
	{
		S_sfx[sound_id].lumpnum = I_GetSfxLumpNum(&S_sfx[sound_id]);
	}
	if(S_sfx[sound_id].snd_ptr == NULL)
	{
		S_sfx[sound_id].snd_ptr = W_CacheLumpNum(S_sfx[sound_id].lumpnum,
			PU_SOUND);
	}
	channel[i].pitch = (byte)(127-(M_Random()&3)+(M_Random()&3));
	channel[i].handle = I_StartSound(sound_id, S_sfx[sound_id].snd_ptr, volume, 128, channel[i].pitch, 0);
	channel[i].mo = origin;
	channel[i].sound_id = sound_id;
	channel[i].priority = 1; //super low priority.
	if(S_sfx[sound_id].usefulness == -1)
	{
		S_sfx[sound_id].usefulness = 1;
	}
	else
	{
		S_sfx[sound_id].usefulness++;
	}
#endif
}

public static bool S_StopSoundID(int sound_id, int priority)
{
	int i;
	int lp; //least priority
	int found;

	if(sounds.S_sfx[sound_id].numchannels == -1)
	{
		return(true);
	}
	lp = -1; //denote the argument sound_id
	found = 0;
	for(i=0; i<i_sound.snd_Channels.val; i++)
	{
		if(channel[i].sound_id == sound_id && channel[i].mo != null)
		{
			found++; //found one.  Now, should we replace it??
			if(priority >= channel[i].priority)
			{ // if we're gonna kill one, then this'll be it
				lp = i;
				priority = channel[i].priority;
			}
		}
	}
	if (found < sounds.S_sfx[sound_id].numchannels)
	{
		return(true);
	}
	else if(lp == -1)
	{
		return(false); // don't replace any sounds
	}
	if(channel[lp].handle != 0)
	{
		if (i_sound.I_SoundIsPlaying((int)channel[lp].handle) != 0)
		{
			i_sound.I_StopSound((int)channel[lp].handle);
		}
		if(sounds.S_sfx[channel[i].sound_id].usefulness > 0)
		{
			sounds.S_sfx[channel[i].sound_id].usefulness--;
		}
		channel[lp].mo = null;
	}
	return(true);
}

public static void S_StopSound(DoomDef.mobj_t origin)
{
	//TODO: channel stuff
#if DOS
	int i;

	for(i=0;i<snd_Channels;i++)
	{
		if(channel[i].mo == origin)
		{
			I_StopSound(channel[i].handle);
			if(S_sfx[channel[i].sound_id].usefulness > 0)
			{
				S_sfx[channel[i].sound_id].usefulness--;
			}
			channel[i].handle = 0;
			channel[i].mo = NULL;
			if(AmbChan == i)
			{
				AmbChan = -1;
			}
		}
	}
#endif
}
#if DOS
void S_SoundLink(mobj_t *oldactor, mobj_t *newactor)
{
	int i;

	for(i=0;i<snd_Channels;i++)
	{
		if(channel[i].mo == oldactor)
			channel[i].mo = newactor;
	}
}

void S_PauseSound(void)
{
	I_PauseSong(rs);
}
#endif
		public static void S_ResumeSound()
		{
			//	I_ResumeSong(rs);
		}

static int nextcleanup;

		public static void S_UpdateSounds(DoomDef.mobj_t listener)
		{
			//int i, dist, vol;
			//int angle;
			//int sep;
			//int priority;
			//int absx;
			//int absy;

			i_sound.UpdateSong(); // [dsl] This will update the midi player

			listener = g_game.players[g_game.consoleplayer].mo;
			if (Settings.Default.sfx_volume == 0)
			{
				return;
			}
			if (g_game.players[g_game.consoleplayer].mo == null) return;

			// [dsl] Set it up using XNA
			Vector3 pos = Vector3.Zero;
			Matrix viewWorld = Matrix.Identity;
			if (!Game1.instance.useFreeCam)
			{
				float playerX = (float)(listener.x >> DoomDef.FRACBITS);
				float playerY = (float)(listener.y >> DoomDef.FRACBITS);
				float playerZ = (float)(listener.z >> DoomDef.FRACBITS);
				float camAngleZ = (float)(((double)r_main.viewangle / ((double)DoomDef.ANG90 * 4.0))) * MathHelper.TwoPi;
				float camAngleX = (float)(((double)r_main.viewanglez / ((double)DoomDef.ANG90 * 4.0))) * MathHelper.TwoPi;
				camAngleX = MathHelper.WrapAngle(camAngleX);
				pos = new Vector3(playerX, playerY, playerZ);
				Vector3 forward = Vector3.UnitX;
				forward = Vector3.Transform(forward, Matrix.CreateRotationY(-camAngleX));
				forward = Vector3.Transform(forward, Matrix.CreateRotationZ(camAngleZ));
				viewWorld = Matrix.CreateWorld(pos, forward, Vector3.UnitZ);
			}
			else
			{
				pos = Game1.instance.freeCam.Translation;
				r_main.viewx = (int)(pos.X * (float)DoomDef.FRACUNIT);
				r_main.viewy = (int)(pos.Y * (float)DoomDef.FRACUNIT);
				r_main.viewz = (int)(pos.Z * (float)DoomDef.FRACUNIT);
				float camAngleZ = Game1.instance.angleZ;
				float camAngleX = Game1.instance.angleX;
				viewWorld = Matrix.CreateWorld(Game1.instance.freeCam.Translation, Game1.instance.freeCam.Up, Vector3.UnitZ);
			}

			Game1.instance.audioListener.Position = Game1.instance.camPos / soundScale;
			Game1.instance.audioListener.Forward = viewWorld.Forward;
			Game1.instance.audioListener.Up = viewWorld.Up;

			for (int i = 0; i < sndInstances.Count; ++i)
			{
				SoundEffectInstance instance = sndInstances[i];
				if (instance.State != SoundState.Playing)
				{
					instance.Dispose();
					sndInstances.RemoveAt(i);
					sndEmitters.RemoveAt(i);
					sndOrigins.RemoveAt(i);
					--i;
					continue;
				}
				// Update emitter position
				DoomDef.mobj_t origin = sndOrigins[i].Target as DoomDef.mobj_t;
				if (origin != null)
				{
					sndEmitters[i].Position = new Vector3(
						(origin.x >> DoomDef.FRACBITS),
						(origin.y >> DoomDef.FRACBITS),
						(origin.z >> DoomDef.FRACBITS)) / soundScale;
				}
				instance.Apply3D(Game1.instance.audioListener, sndEmitters[i]);
			}

			// [dsl] Ignore that part. We are going to play the sounds in 3D using XNA. 
			// We might not get the same sound curve, but we dont get same rendering tech
			// either so I think it's fair :)
			/*	if(nextcleanup < gametic)
				{
					for(i=0; i < NUMSFX; i++)
					{
						if(S_sfx[i].usefulness == 0 && S_sfx[i].snd_ptr)
						{
							if(lumpcache[S_sfx[i].lumpnum])
							{
								if(((memblock_t *)((byte *)(lumpcache[S_sfx[i].lumpnum])-
									sizeof(memblock_t))).id == 0x1d4a11)
								{ // taken directly from the Z_ChangeTag macro
									Z_ChangeTag2(lumpcache[S_sfx[i].lumpnum], PU_CACHE);
								}
							}
							S_sfx[i].usefulness = -1;
							S_sfx[i].snd_ptr = NULL;
						}
					}
					nextcleanup = gametic+35; //CLEANUP DEBUG cleans every second
				}
				for(i=0;i<snd_Channels;i++)
				{
					if(!channel[i].handle || S_sfx[channel[i].sound_id].usefulness == -1)
					{
						continue;
					}
					if(!I_SoundIsPlaying(channel[i].handle))
					{
						if(S_sfx[channel[i].sound_id].usefulness > 0)
						{
							S_sfx[channel[i].sound_id].usefulness--;
						}
						channel[i].handle = 0;
						channel[i].mo = NULL;
						channel[i].sound_id = 0;
						if(AmbChan == i)
						{
							AmbChan = -1;
						}
					}
					if(channel[i].mo == NULL || channel[i].sound_id == 0
						|| channel[i].mo == players[consoleplayer].mo)
					{
						continue;
					}
					else
					{
						absx = abs(channel[i].mo.x-players[consoleplayer].mo.x);
						absy = abs(channel[i].mo.y-players[consoleplayer].mo.y);
						dist = absx+absy-(absx > absy ? absy>>1 : absx>>1);
						dist >>= FRACBITS;

						if(dist >= MAX_SND_DIST)
						{
							S_StopSound(channel[i].mo);
							continue;
						}
						if(dist < 0)
							dist = 0;

			// calculate the volume based upon the distance from the sound origin.
						vol = soundCurve[dist];

						angle = R_PointToAngle2(players[consoleplayer].mo.x,
							players[consoleplayer].mo.y, channel[i].mo.x, channel[i].mo.y);
						angle = (angle-viewangle)>>24;
						sep = angle*2-128;
						if(sep < 64)
							sep = -sep;
						if(sep > 192)
							sep = 512-sep;
						I_UpdateSoundParams(channel[i].handle, vol, sep, channel[i].pitch);
						priority = S_sfx[channel[i].sound_id].priority;
						priority *= (10 - (dist>>8));
						channel[i].priority = priority;
					}
				}*/
		}
#if DOS

void S_Init(void)
{
	soundCurve = Z_Malloc(MAX_SND_DIST, PU_STATIC, NULL);
	I_StartupSound();
	if(snd_Channels > 8)
	{
		snd_Channels = 8;
	}
	I_SetChannels(snd_Channels);
	I_SetMusicVolume(snd_MusicVolume);
	S_SetMaxVolume(true);
}

void S_GetChannelInfo(SoundInfo_t *s)
{
	int i;
	ChanInfo_t *c;

	s.channelCount = snd_Channels;
	s.musicVolume = snd_MusicVolume;
	s.soundVolume = snd_MaxVolume;
	for(i = 0; i < snd_Channels; i++)
	{
		c = &s.chan[i];
		c.id = channel[i].sound_id;
		c.priority = channel[i].priority;
		c.name = S_sfx[c.id].name;
		c.mo = channel[i].mo;
		c.distance = P_AproxDistance(c.mo.x-viewx, c.mo.y-viewy)
			>>FRACBITS;
	}
}
#endif
		public static void S_SetMaxVolume(bool fullprocess)
		{
			int i;

			SoundEffect.MasterVolume = (float)(Settings.Default.sfx_volume) / 16.0f;

			if (!fullprocess)
			{
				//	soundCurve[0] = (*((byte *)W_CacheLumpName("SNDCURVE", PU_CACHE))*(snd_MaxVolume*8))>>7;
			}
			else
			{
				/*	for(i = 0; i < MAX_SND_DIST; i++)
					{
						soundCurve[i] = (*((byte *)W_CacheLumpName("SNDCURVE", PU_CACHE)+i)*(snd_MaxVolume*8))>>7;
					}*/
			}
		}

		public static void S_SetMusicVolume()
		{
#if DOS
	I_SetMusicVolume(i_sound.snd_MusicVolume.val);
	if (i_sound.snd_MusicVolume.val == 0)
	{
		I_PauseSong(rs);
		musicPaused = true;
	}
	else if(musicPaused)
	{
		musicPaused = false;
		I_ResumeSong(rs);
	}
#endif
		}
#if DOS

void S_ShutDown(void)
{
	extern int tsm_ID;
	if(tsm_ID != -1)
	{
  		I_StopSong(rs);
  		I_UnRegisterSong(rs);
  		I_ShutdownSound();
	}
}

/*
=============================================================================

							CONSTANTS

=============================================================================
*/

//TODO: public const int SC_INDEX                0x3C4
//TODO: public const int SC_RESET                0
//TODO: public const int SC_CLOCK                1
//TODO: public const int SC_MAPMASK              2
//TODO: public const int SC_CHARMAP              3
//TODO: public const int SC_MEMMODE              4

//TODO: public const int CRTC_INDEX              0x3D4
//TODO: public const int CRTC_H_TOTAL    0
//TODO: public const int CRTC_H_DISPEND  1
//TODO: public const int CRTC_H_BLANK    2
//TODO: public const int CRTC_H_ENDBLANK 3
//TODO: public const int CRTC_H_RETRACE  4
//TODO: public const int CRTC_H_ENDRETRACE 5
//TODO: public const int CRTC_V_TOTAL    6
//TODO: public const int CRTC_OVERFLOW   7
//TODO: public const int CRTC_ROWSCAN    8
//TODO: public const int CRTC_MAXSCANLINE 9
//TODO: public const int CRTC_CURSORSTART 10
//TODO: public const int CRTC_CURSOREND  11
//TODO: public const int CRTC_STARTHIGH  12
//TODO: public const int CRTC_STARTLOW   13
//TODO: public const int CRTC_CURSORHIGH 14
//TODO: public const int CRTC_CURSORLOW  15
//TODO: public const int CRTC_V_RETRACE  16
//TODO: public const int CRTC_V_ENDRETRACE 17
//TODO: public const int CRTC_V_DISPEND  18
//TODO: public const int CRTC_OFFSET             19
//TODO: public const int CRTC_UNDERLINE  20
//TODO: public const int CRTC_V_BLANK    21
//TODO: public const int CRTC_V_ENDBLANK 22
//TODO: public const int CRTC_MODE               23
//TODO: public const int CRTC_LINECOMPARE 24


//TODO: public const int GC_INDEX                0x3CE
//TODO: public const int GC_SETRESET             0
//TODO: public const int GC_ENABLESETRESET 1
//TODO: public const int GC_COLORCOMPARE 2
//TODO: public const int GC_DATAROTATE   3
//TODO: public const int GC_READMAP              4
//TODO: public const int GC_MODE                 5
//TODO: public const int GC_MISCELLANEOUS 6
//TODO: public const int GC_COLORDONTCARE 7
//TODO: public const int GC_BITMASK              8

//TODO: public const int ATR_INDEX               0x3c0
//TODO: public const int ATR_MODE                16
//TODO: public const int ATR_OVERSCAN    17
//TODO: public const int ATR_COLORPLANEENABLE 18
//TODO: public const int ATR_PELPAN              19
//TODO: public const int ATR_COLORSELECT 20

//TODO: public const int STATUS_REGISTER_1    0x3da

//TODO: public const int PEL_WRITE_ADR   0x3c8
//TODO: public const int PEL_READ_ADR    0x3c7
//TODO: public const int PEL_DATA                0x3c9
//TODO: public const int PEL_MASK                0x3c6

boolean grmode;

//==================================================
//
// joystick vars
//
//==================================================

boolean         joystickpresent;
extern  unsigned        joystickx, joysticky;
boolean I_ReadJoystick (void);          // returns false if not connected


//==================================================

//TODO: public const int VBLCOUNTER              34000           // hardware tics to a frame


//TODO: public const int TIMERINT 8
//TODO: public const int KEYBOARDINT 9

//TODO: public const int CRTCOFF (_inbyte(STATUS_REGISTER_1)&1)
//TODO: public const int CLI     _disable()
//TODO: public const int STI     _enable()

//TODO: public const int _outbyte(x,y) (outp(x,y))
//TODO: public const int _outhword(x,y) (outpw(x,y))

//TODO: public const int _inbyte(x) (inp(x))
//TODO: public const int _inhword(x) (inpw(x))

//TODO: public const int MOUSEB1 1
//TODO: public const int MOUSEB2 2
//TODO: public const int MOUSEB3 4

boolean mousepresent;
//static  int tsm_ID = -1; // tsm init flag

//===============================
#endif
public static int             ticcount;
#if DOS

// REGS stuff used for int calls
union REGS regs;
struct SREGS segregs;

boolean novideo; // if true, stay in text mode for debugging

//TODO: public const int KBDQUESIZE 32
byte keyboardque[KBDQUESIZE];
int kbdtail, kbdhead;

//TODO: public const int KEY_LSHIFT      0xfe

//TODO: public const int KEY_INS         (0x80+0x52)
//TODO: public const int KEY_DEL         (0x80+0x53)
//TODO: public const int KEY_PGUP        (0x80+0x49)
//TODO: public const int KEY_PGDN        (0x80+0x51)
//TODO: public const int KEY_HOME        (0x80+0x47)
//TODO: public const int KEY_END         (0x80+0x4f)

//TODO: public const int SC_RSHIFT       0x36
//TODO: public const int SC_LSHIFT       0x2a

byte        scantokey[128] =
					{
//  0           1       2       3       4       5       6       7
//  8           9       A       B       C       D       E       F
	0  ,    27,     '1',    '2',    '3',    '4',    '5',    '6',
	'7',    '8',    '9',    '0',    '-',    '=',    KEY_BACKSPACE, 9, // 0
	'q',    'w',    'e',    'r',    't',    'y',    'u',    'i',
	'o',    'p',    '[',    ']',    13 ,    KEY_RCTRL,'a',  's',      // 1
	'd',    'f',    'g',    'h',    'j',    'k',    'l',    ';',
	39 ,    '`',    KEY_LSHIFT,92,  'z',    'x',    'c',    'v',      // 2
	'b',    'n',    'm',    ',',    '.',    '/',    KEY_RSHIFT,'*',
	KEY_RALT,' ',   0  ,    KEY_F1, KEY_F2, KEY_F3, KEY_F4, KEY_F5,   // 3
	KEY_F6, KEY_F7, KEY_F8, KEY_F9, KEY_F10,0  ,    0  , KEY_HOME,
	KEY_UPARROW,KEY_PGUP,'-',KEY_LEFTARROW,'5',KEY_RIGHTARROW,'+',KEY_END, //4
	KEY_DOWNARROW,KEY_PGDN,KEY_INS,KEY_DEL,0,0,             0,              KEY_F11,
	KEY_F12,0  ,    0  ,    0  ,    0  ,    0  ,    0  ,    0,        // 5
	0  ,    0  ,    0  ,    0  ,    0  ,    0  ,    0  ,    0,
	0  ,    0  ,    0  ,    0  ,    0  ,    0  ,    0  ,    0,        // 6
	0  ,    0  ,    0  ,    0  ,    0  ,    0  ,    0  ,    0,
	0  ,    0  ,    0  ,    0  ,    0  ,    0  ,    0  ,    0         // 7
					};

//==========================================================================
#endif
//--------------------------------------------------------------------------
//
// FUNC I_GetTime
//
// Returns time in 1/35th second tics.
//
//--------------------------------------------------------------------------

public static int I_GetTime ()
{
	return(ticcount);
}
#if DOS
//--------------------------------------------------------------------------
//
// PROC I_ColorBorder
//
//--------------------------------------------------------------------------

void I_ColorBorder(void)
{
	int i;

	I_WaitVBL(1);
	_outbyte(PEL_WRITE_ADR, 0);
	for(i = 0; i < 3; i++)
	{
		_outbyte(PEL_DATA, 63);
	}
}

//--------------------------------------------------------------------------
//
// PROC I_UnColorBorder
//
//--------------------------------------------------------------------------

void I_UnColorBorder(void)
{
	int i;

	I_WaitVBL(1);
	_outbyte(PEL_WRITE_ADR, 0);
	for(i = 0; i < 3; i++)
	{
		_outbyte(PEL_DATA, 0);
	}
}

/*
============================================================================

								USER INPUT

============================================================================
*/

//--------------------------------------------------------------------------
//
// PROC I_WaitVBL
//
//--------------------------------------------------------------------------

void I_WaitVBL(int vbls)
{
	int i;
	int old;
	int stat;

	if(novideo)
	{
		return;
	}
	while(vbls--)
	{
		do
		{
			stat = inp(STATUS_REGISTER_1);
			if(stat&8)
			{
				break;
			}
		} while(1);
		do
		{
			stat = inp(STATUS_REGISTER_1);
			if((stat&8) == 0)
			{
				break;
			}
		} while(1);
	}
}

//--------------------------------------------------------------------------
//
// PROC I_SetPalette
//
// Palette source must use 8 bit RGB elements.
//
//--------------------------------------------------------------------------

void I_SetPalette(byte *palette)
{
	int i;

	if(novideo)
	{
		return;
	}
	I_WaitVBL(1);
	_outbyte(PEL_WRITE_ADR, 0);
	for(i = 0; i < 768; i++)
	{
		_outbyte(PEL_DATA, (gammatable[usegamma][*palette++])>>2);
	}
}

/*
============================================================================

							GRAPHICS MODE

============================================================================
*/

byte *pcscreen, *destscreen, *destview;


/*
==============
=
= I_Update
=
==============
*/
#endif
		public static int UpdateState;

public static void I_Update ()
{
#if DOS
	int i;
	byte *dest;
	int tics;
	static int lasttic;

//
// blit screen to video
//
	if(DisplayTicker)
	{
		if(screenblocks > 9 || UpdateState&(I_FULLSCRN|I_MESSAGES))
		{
			dest = (byte *)screen;
		}
		else
		{
			dest = (byte *)pcscreen;
		}
		tics = ticcount-lasttic;
		lasttic = ticcount;
		if(tics > 20)
		{
			tics = 20;
		}
		for(i = 0; i < tics; i++)
		{
			*dest = 0xff;
			dest += 2;
		}
		for(i = tics; i < 20; i++)
		{
			*dest = 0x00;
			dest += 2;
		}
	}
	if(UpdateState == I_NOUPDATE)
	{
		return;
	}
	if(UpdateState&I_FULLSCRN)
	{
		memcpy(pcscreen, screen, SCREENWIDTH*SCREENHEIGHT);
		UpdateState = I_NOUPDATE; // clear out all draw types
	}
	if(UpdateState&I_FULLVIEW)
	{
		if(UpdateState&I_MESSAGES && screenblocks > 7)
		{
			for(i = 0; i <
				(viewwindowy+viewheight)*SCREENWIDTH; i += SCREENWIDTH)
			{
				memcpy(pcscreen+i, screen+i, SCREENWIDTH);
			}
			UpdateState &= ~(I_FULLVIEW|I_MESSAGES);
		}
		else
		{
			for(i = viewwindowy*SCREENWIDTH+viewwindowx; i <
				(viewwindowy+viewheight)*SCREENWIDTH; i += SCREENWIDTH)
			{
				memcpy(pcscreen+i, screen+i, viewwidth);
			}
			UpdateState &= ~I_FULLVIEW;
		}
	}
	if(UpdateState&I_STATBAR)
	{
		memcpy(pcscreen+SCREENWIDTH*(SCREENHEIGHT-SBARHEIGHT),
			screen+SCREENWIDTH*(SCREENHEIGHT-SBARHEIGHT),
			SCREENWIDTH*SBARHEIGHT);
		UpdateState &= ~I_STATBAR;
	}
	if(UpdateState&I_MESSAGES)
	{
		memcpy(pcscreen, screen, SCREENWIDTH*28);
		UpdateState &= ~I_MESSAGES;
	}

//  memcpy(pcscreen, screen, SCREENHEIGHT*SCREENWIDTH);
#endif
}

#if DOS

//--------------------------------------------------------------------------
//
// PROC I_InitGraphics
//
//--------------------------------------------------------------------------

void I_InitGraphics(void)
{
	if(novideo)
	{
		return;
	}
	grmode = true;
	regs.w.ax = 0x13;
	int386(0x10, (const union REGS *)&regs, &regs);
	pcscreen = destscreen = (byte *)0xa0000;
	I_SetPalette(W_CacheLumpName("PLAYPAL", PU_CACHE));
	I_InitDiskFlash();
}

//--------------------------------------------------------------------------
//
// PROC I_ShutdownGraphics
//
//--------------------------------------------------------------------------

void I_ShutdownGraphics(void)
{

	if(*(byte *)0x449 == 0x13) // don't reset mode if it didn't get set
	{
		regs.w.ax = 3;
		int386(0x10, &regs, &regs); // back to text mode
	}
}

//--------------------------------------------------------------------------
//
// PROC I_ReadScreen
//
// Reads the screen currently displayed into a linear buffer.
//
//--------------------------------------------------------------------------

void I_ReadScreen(byte *scr)
{
	memcpy(scr, screen, SCREENWIDTH*SCREENHEIGHT);
}


//===========================================================================

#endif
		/*
===================
=
= I_StartTic
=
// called by D_DoomLoop
// called before processing each tic in a frame
// can call D_PostEvent
// asyncronous interrupt functions should maintain private ques that are
// read by the syncronous functions to be converted into events
===================
*/


		//TODO: public const int  SC_UPARROW              0x48
		//TODO: public const int  SC_DOWNARROW    0x50
		//TODO: public const int  SC_LEFTARROW            0x4b
		//TODO: public const int  SC_RIGHTARROW   0x4d

		public static MouseState oldMouseState = Mouse.GetState();
		public static KeyboardState oldKeyboardState = Keyboard.GetState();
		public static MouseState mouseState = oldMouseState;
		public static KeyboardState keyboardState = oldKeyboardState;
		public static Point mouseDelta = Point.Zero;

		public static void I_StartTic()
		{
			oldKeyboardState = keyboardState;
			keyboardState = Keyboard.GetState();

			DoomDef.event_t ev;

			I_ReadMouse();

			if (Game1.instance.useFreeCam) return;

			//
			// keyboard events
			//
			var keys = Enum.GetValues(typeof(Keys));
			foreach (Keys key in keys)
			{
				if (oldKeyboardState.IsKeyUp(key) &&
					keyboardState.IsKeyDown(key))
				{
					ev = new DoomDef.event_t();
					ev.data1 = (int)key;
					ev.type = DoomDef.evtype_t.ev_keydown;
					d_main.D_PostEvent(ev);
				}
				else if (oldKeyboardState.IsKeyDown(key) &&
					keyboardState.IsKeyUp(key))
				{
					ev = new DoomDef.event_t();
					ev.data1 = (int)key;
					ev.type = DoomDef.evtype_t.ev_keyup;
					d_main.D_PostEvent(ev);
				}
			}
		}
#if DOS

void   I_ReadKeys (void)
{
	int             k;
	event_t ev;


	while (1)
	{
	   while (kbdtail < kbdhead)
	   {
		   k = keyboardque[kbdtail&(KBDQUESIZE-1)];
		   kbdtail++;
		   printf ("0x%x\n",k);
		   if (k == 1)
			   I_Quit ();
	   }
	}
}

/*
===============
=
= I_StartFrame
=
===============
*/

void I_StartFrame (void)
{
	I_JoystickEvents ();
	I_ReadExternDriver();
}

/*
============================================================================

					TIMER INTERRUPT

============================================================================
*/

void I_ColorBlack (int r, int g, int b)
{
_outbyte (PEL_WRITE_ADR,0);
_outbyte(PEL_DATA,r);
_outbyte(PEL_DATA,g);
_outbyte(PEL_DATA,b);
}

#endif
/*
================
=
= I_TimerISR
=
================
*/

public static int I_TimerISR ()
{
	ticcount++;
	return 0;
}
#if DOS
/*
============================================================================

						KEYBOARD

============================================================================
*/

void (__interrupt __far *oldkeyboardisr) () = NULL;

int lastpress;

/*
================
=
= I_KeyboardISR
=
================
*/

void __interrupt I_KeyboardISR (void)
{
// Get the scan code

	keyboardque[kbdhead&(KBDQUESIZE-1)] = lastpress = _inbyte(0x60);
	kbdhead++;

// acknowledge the interrupt

	_outbyte(0x20,0x20);
}



/*
===============
=
= I_StartupKeyboard
=
===============
*/

void I_StartupKeyboard (void)
{
	oldkeyboardisr = _dos_getvect(KEYBOARDINT);
	_dos_setvect (0x8000 | KEYBOARDINT, I_KeyboardISR);
}


void I_ShutdownKeyboard (void)
{
	if (oldkeyboardisr)
		_dos_setvect (KEYBOARDINT, oldkeyboardisr);
	*(short *)0x41c = *(short *)0x41a;      // clear bios key buffer
}



/*
============================================================================

							MOUSE

============================================================================
*/


int I_ResetMouse (void)
{
	regs.w.ax = 0;                  // reset
	int386 (0x33, &regs, &regs);
	return regs.w.ax;
}



/*
================
=
= StartupMouse
=
================
*/

void I_StartupCyberMan(void);

void I_StartupMouse (void)
{
   int  (far *function)();

   //
   // General mouse detection
   //
	mousepresent = 0;
	if ( M_CheckParm ("-nomouse") || !usemouse )
		return;

	if (I_ResetMouse () != 0xffff)
	{
		tprintf ("Mouse: not present ",0);
		return;
	}
	tprintf ("Mouse: detected ",0);

	mousepresent = 1;

	I_StartupCyberMan();
}


/*
================
=
= ShutdownMouse
=
================
*/

void I_ShutdownMouse (void)
{
	if (!mousepresent)
	  return;

	I_ResetMouse ();
}

#endif
		/*
================
=
= I_ReadMouse
=
================
*/
static bool menuWasActive = false;
static Point oldMouse = Point.Zero;
static bool wasFreeCam = false;

		public static void I_ReadMouse()
		{
			oldMouseState = mouseState;
			mouseState = Mouse.GetState();
			Point midScreen = new Point(
				Game1.instance.GraphicsDevice.Viewport.Width / 2,
				Game1.instance.GraphicsDevice.Viewport.Height / 2);
			mouseDelta = new Point(mouseState.X - midScreen.X, mouseState.Y - midScreen.Y);

			if (Game1.instance.useFreeCam && mouseState.MiddleButton != ButtonState.Pressed)
			{
				if (wasFreeCam)
				{
					Mouse.SetPosition(oldMouse.X, oldMouse.Y);
					mouseState = Mouse.GetState();
				}
				oldMouse.X = mouseState.X;
				oldMouse.Y = mouseState.Y;
				wasFreeCam = false;
				return;
			}

			if (!wasFreeCam)
			{
				mouseDelta = Point.Zero;
				wasFreeCam = true;
			}

			if (mn_menu.MenuActive)
			{
				mouseDelta = Point.Zero;
				menuWasActive = true;
				Game1.instance.IsMouseVisible = true;
			}
			else
			{
				Game1.instance.IsMouseVisible = false;
				if (menuWasActive)
				{
					menuWasActive = false;
					mouseDelta = Point.Zero;
				}
				Mouse.SetPosition(midScreen.X, midScreen.Y);
			}

			if (Game1.instance.useFreeCam) return;

			DoomDef.event_t ev = new DoomDef.event_t();

			//
			// mouse events
			//
			//	if (!mousepresent)
			//		return;

			ev.type = DoomDef.evtype_t.ev_mouse;

			ev.data1 = 0;
			if (mouseState.LeftButton == ButtonState.Pressed) ev.data1 |= 1;
			if (mouseState.RightButton == ButtonState.Pressed) ev.data1 |= 2;
			if (mouseState.MiddleButton == ButtonState.Pressed) ev.data1 |= 4;

			ev.data2 = mouseState.X;
			ev.data3 = mouseState.Y;

			d_main.D_PostEvent(ev);
		}
#if DOS

/*
============================================================================

					JOYSTICK

============================================================================
*/

int     joyxl, joyxh, joyyl, joyyh;

boolean WaitJoyButton (void)
{
	int             oldbuttons, buttons;

	oldbuttons = 0;
	do
	{
		I_WaitVBL (1);
		buttons =  ((inp(0x201) >> 4)&1)^1;
		if (buttons != oldbuttons)
		{
			oldbuttons = buttons;
			continue;
		}

		if ( (lastpress& 0x7f) == 1 )
		{
			joystickpresent = false;
			return false;
		}
	} while ( !buttons);

	do
	{
		I_WaitVBL (1);
		buttons =  ((inp(0x201) >> 4)&1)^1;
		if (buttons != oldbuttons)
		{
			oldbuttons = buttons;
			continue;
		}

		if ( (lastpress& 0x7f) == 1 )
		{
			joystickpresent = false;
			return false;
		}
	} while ( buttons);

	return true;
}



/*
===============
=
= I_StartupJoystick
=
===============
*/

int             basejoyx, basejoyy;

void I_StartupJoystick (void)
{
	int     buttons;
	int     count;
	int     centerx, centery;

	joystickpresent = 0;
	if ( M_CheckParm ("-nojoy") || !usejoystick )
		return;

	if (!I_ReadJoystick ())
	{
		joystickpresent = false;
		tprintf ("joystick not found ",0);
		return;
	}
	printf("joystick found\n");
	joystickpresent = true;

	printf("CENTER the joystick and press button 1:");
	if (!WaitJoyButton ())
		return;
	I_ReadJoystick ();
	centerx = joystickx;
	centery = joysticky;

	printf("\nPush the joystick to the UPPER LEFT corner and press button 1:");
	if (!WaitJoyButton ())
		return;
	I_ReadJoystick ();
	joyxl = (centerx + joystickx)/2;
	joyyl = (centerx + joysticky)/2;

	printf("\nPush the joystick to the LOWER RIGHT corner and press button 1:");
	if (!WaitJoyButton ())
		return;
	I_ReadJoystick ();
	joyxh = (centerx + joystickx)/2;
	joyyh = (centery + joysticky)/2;
	printf("\n");
}

/*
===============
=
= I_JoystickEvents
=
===============
*/

void I_JoystickEvents (void)
{
	event_t ev;

//
// joystick events
//
	if (!joystickpresent)
		return;

	I_ReadJoystick ();
	ev.type = ev_joystick;
	ev.data1 =  ((inp(0x201) >> 4)&15)^15;

	if (joystickx < joyxl)
		ev.data2 = -1;
	else if (joystickx > joyxh)
		ev.data2 = 1;
	else
		ev.data2 = 0;
	if (joysticky < joyyl)
		ev.data3 = -1;
	else if (joysticky > joyyh)
		ev.data3 = 1;
	else
		ev.data3 = 0;

	D_PostEvent (&ev);
}



/*
============================================================================

					DPMI STUFF

============================================================================
*/

//TODO: public const int  REALSTACKSIZE   1024

dpmiregs_t      dpmiregs;

unsigned                realstackseg;

void I_DivException (void);
int I_SetDivException (void);

void DPMIFarCall (void)
{
	segread (&segregs);
	regs.w.ax = 0x301;
	regs.w.bx = 0;
	regs.w.cx = 0;
	regs.x.edi = (unsigned)&dpmiregs;
	segregs.es = segregs.ds;
	int386x( DPMI_INT, &regs, &regs, &segregs );
}


void DPMIInt (int i)
{
	dpmiregs.ss = realstackseg;
	dpmiregs.sp = REALSTACKSIZE-4;

	segread (&segregs);
	regs.w.ax = 0x300;
	regs.w.bx = i;
	regs.w.cx = 0;
	regs.x.edi = (unsigned)&dpmiregs;
	segregs.es = segregs.ds;
	int386x( DPMI_INT, &regs, &regs, &segregs );
}


/*
==============
=
= I_StartupDPMI
=
==============
*/

void I_StartupDPMI (void)
{
	extern char __begtext;
	extern char ___argc;
	int     n,d;

//
// allocate a decent stack for real mode ISRs
//
	realstackseg = (int)I_AllocLow (1024) >> 4;

//
// lock the entire program down
//

//      _dpmi_lockregion (&__begtext, &___argc - &__begtext);


//
// catch divide by 0 exception
//
}



/*
============================================================================

					TIMER INTERRUPT

============================================================================
*/

void (__interrupt __far *oldtimerisr) ();


void IO_ColorBlack (int r, int g, int b)
{
_outbyte (PEL_WRITE_ADR,0);
_outbyte(PEL_DATA,r);
_outbyte(PEL_DATA,g);
_outbyte(PEL_DATA,b);
}


/*
================
=
= IO_TimerISR
=
================
*/

//void __interrupt IO_TimerISR (void)

void __interrupt __far IO_TimerISR (void)
{
	ticcount++;
	_outbyte(0x20,0x20);                            // Ack the interrupt
}

/*
=====================
=
= IO_SetTimer0
=
= Sets system timer 0 to the specified speed
=
=====================
*/

void IO_SetTimer0(int speed)
{
	if (speed > 0 && speed < 150)
		I_Error ("INT_SetTimer0: %i is a bad value",speed);

	_outbyte(0x43,0x36);                            // Change timer 0
	_outbyte(0x40,speed);
	_outbyte(0x40,speed >> 8);
}



/*
===============
=
= IO_StartupTimer
=
===============
*/

void IO_StartupTimer (void)
{
	oldtimerisr = _dos_getvect(TIMERINT);

	_dos_setvect (0x8000 | TIMERINT, IO_TimerISR);
	IO_SetTimer0 (VBLCOUNTER);
}

void IO_ShutdownTimer (void)
{
	if (oldtimerisr)
	{
		IO_SetTimer0 (0);              // back to 18.4 ips
		_dos_setvect (TIMERINT, oldtimerisr);
	}
}

//===========================================================================

#endif

/*
===============
=
= I_Init
=
= hook interrupts and set graphics mode
=
===============
*/

public static void I_Init ()
{
	//novideo = M_CheckParm("novideo");
	//tprintf("I_StartupDPMI",1);
	//I_StartupDPMI();
	//tprintf("I_StartupMouse ",1);
	//I_StartupMouse();
	//tprintf("S_Init... ",1);
	//S_Init();
	S_Start();
}


#if DOS	


/*
===============
=
= I_Shutdown
=
= return to default system state
=
===============
*/

void I_Shutdown (void)
{
	I_ShutdownGraphics ();
	IO_ShutdownTimer ();
	S_ShutDown ();
	I_ShutdownMouse ();
	I_ShutdownKeyboard ();

	IO_SetTimer0 (0);
}
#endif

		/*
================
=
= I_Error
=
================
*/

		public static void I_Error(string error)
		{
			Console.WriteLine(error);

			//TODO:
			//D_QuitNetGame ();
			//I_Shutdown ();

			//	Game1.instance.Exit();
		}


		//--------------------------------------------------------------------------
		//
		// I_Quit
		//
		// Shuts down net game, saves defaults, prints the exit text message,
		// goes to text mode, and exits.
		//
		//--------------------------------------------------------------------------

		public static void I_Quit()
		{
			Settings.Default.Save();
			Game1.instance.Exit();
#if DOS
	byte *scr;
	char *lumpName;
	int r;

	D_QuitNetGame();
	M_SaveDefaults();
	scr = (byte *)W_CacheLumpName("ENDTEXT", PU_CACHE);
	I_Shutdown();
	memcpy((void *)0xb8000, scr, 80*25*2);
	regs.w.ax = 0x0200;
	regs.h.bh = 0;
	regs.h.dl = 0;
	regs.h.dh = 23;
	int386(0x10, (const union REGS *)&regs, &regs); // Set text pos
	_settextposition(24, 1);
	exit(0);
#endif
		}
#if DOS

/*
===============
=
= I_ZoneBase
=
===============
*/

byte *I_ZoneBase (int *size)
{
	int             meminfo[32];
	int             heap;
	int             i;
	int                             block;
	byte                    *ptr;

	memset (meminfo,0,sizeof(meminfo));
	segread(&segregs);
	segregs.es = segregs.ds;
	regs.w.ax = 0x500;      // get memory info
	regs.x.edi = (int)&meminfo;
	int386x( 0x31, &regs, &regs, &segregs );

	heap = meminfo[0];
	printf ("DPMI memory: 0x%x, ",heap);

	do
	{
		heap -= 0x10000;                // leave 64k alone
		if (heap > 0x800000)
			heap = 0x800000;
		ptr = malloc (heap);
	} while (!ptr);

	printf ("0x%x allocated for zone\n", heap);
	if (heap < 0x180000)
		I_Error ("Insufficient DPMI memory!");

	*size = heap;
	return ptr;
}

/*
=============================================================================

					DISK ICON FLASHING

=============================================================================
*/

void I_InitDiskFlash (void)
{
}

#endif

		// draw disk icon
		public static void I_BeginRead()
		{
		}

		// erase disk icon
		public static void I_EndRead()
		{
		}

#if DOS

/*
=============
=
= I_AllocLow
=
=============
*/

byte *I_AllocLow (int length)
{
	byte    *mem;

	// DPMI call 100h allocates DOS memory
	segread(&segregs);
	regs.w.ax = 0x0100;          // DPMI allocate DOS memory
	regs.w.bx = (length+15) / 16;
	int386( DPMI_INT, &regs, &regs);
//      segment = regs.w.ax;
//   selector = regs.w.dx;
	if (regs.w.cflag != 0)
		I_Error ("I_AllocLow: DOS alloc of %i failed, %i free",
			length, regs.w.bx*16);


	mem = (void *) ((regs.x.eax & 0xFFFF) << 4);

	memset (mem,0,length);
	return mem;
}

/*
============================================================================

						NETWORKING

============================================================================
*/

/*
====================
=
= I_InitNetwork
=
====================
*/

void I_InitNetwork (void)
{
	int             i;

	i = M_CheckParm ("-net");
	if (!i)
	{
	//
	// single player game
	//
		doomcom = malloc (sizeof (*doomcom) );
		memset (doomcom, 0, sizeof(*doomcom) );
		netgame = false;
		doomcom.id = DOOMCOM_ID;
		doomcom.numplayers = doomcom.numnodes = 1;
		doomcom.deathmatch = false;
		doomcom.consoleplayer = 0;
		doomcom.ticdup = 1;
		doomcom.extratics = 0;
		return;
	}

	netgame = true;
	doomcom = (doomcom_t *)atoi(myargv[i+1]);
//DEBUG
doomcom.skill = startskill;
doomcom.episode = startepisode;
doomcom.map = startmap;
doomcom.deathmatch = deathmatch;

}

void I_NetCmd (void)
{
	if (!netgame)
		I_Error ("I_NetCmd when not in netgame");
	DPMIInt (doomcom.intnum);
}

int i_Vector;
externdata_t *i_ExternData;
boolean useexterndriver;

//=========================================================================
//
// I_CheckExternDriver
//
//		Checks to see if a vector, and an address for an external driver
//			have been passed.
//=========================================================================

void I_CheckExternDriver(void)
{
	int i;

	if(!(i = M_CheckParm("-externdriver")))
	{
		return;
	}
	i_ExternData = (externdata_t *)atoi(myargv[i+1]);
	i_Vector = i_ExternData.vector;

	useexterndriver = true;
}

//=========================================================================
//
// I_ReadExternDriver
//
//		calls the external interrupt, which should then update i_ExternDriver
//=========================================================================

void I_ReadExternDriver(void)
{
	event_t ev;

	if(useexterndriver)
	{
		DPMIInt(i_Vector);
	}
}

#endif
	}
}
