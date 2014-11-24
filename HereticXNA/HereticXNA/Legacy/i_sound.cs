using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Media;
using CSharpSynth.Midi;
using CSharpSynth.Sequencer;
using CSharpSynth.Synthesis;
		
// I_SOUND.C

namespace HereticXNA
{
	static public class i_sound
	{
		[DllImport("winmm.dll")]
		static extern Int32 mciSendString(String command, StringBuilder buffer,
										  Int32 bufferSize, IntPtr hwndCallback);
#if DOS

/*
===============
=
= I_StartupTimer
=
===============
*/

int tsm_ID = -1;

void I_StartupTimer (void)
{
	extern int I_TimerISR(void);

	tprintf("I_StartupTimer()\n",0);
	// installs master timer.  Must be done before StartupTimer()!
	TSM_Install(SND_TICRATE);
	tsm_ID = TSM_NewService (I_TimerISR, 35, 255, 0); // max priority
	if (tsm_ID == -1)
	{
		I_Error("Can't register 35 Hz timer w/ DMX library");
	}
}

void I_ShutdownTimer (void)
{
	TSM_DelService(tsm_ID);
	TSM_Remove();
}

/*
 *
 *                           SOUND HEADER & DATA
 *
 *
 */

// sound information

const char snd_prefixen[] = { 'P', 'P', 'A', 'S', 'S', 'S', 'M',
  'M', 'M', 'S' };

#endif
static public m_misc.var_int snd_Channels = new m_misc.var_int();
static public m_misc.var_int snd_DesiredMusicDevice = new m_misc.var_int();
static public m_misc.var_int snd_DesiredSfxDevice = new m_misc.var_int();
static public m_misc.var_int snd_MusicDevice = new m_misc.var_int();
static public m_misc.var_int snd_SfxDevice = new m_misc.var_int();
//static public m_misc.var_int snd_MaxVolume = new m_misc.var_int();
//static public m_misc.var_int snd_MusicVolume = new m_misc.var_int();
#if DOS
int dmxCodes[NUM_SCARDS]; // the dmx code for a given card

#endif
static public m_misc.var_int snd_SBport = new m_misc.var_int();
static public m_misc.var_int snd_SBirq = new m_misc.var_int();
static public m_misc.var_int snd_SBdma = new m_misc.var_int();       // sound blaster variables
static public m_misc.var_int snd_Mport = new m_misc.var_int();       // midi variables
#if DOS

extern boolean  snd_MusicAvail, // whether music is available
		snd_SfxAvail;   // whether sfx are available

void I_PauseSong(int handle)
{
  MUS_PauseSong(handle);
}

void I_ResumeSong(int handle)
{
  MUS_ResumeSong(handle);
}

void I_SetMusicVolume(int volume)
{
  MUS_SetMasterVolume(volume*8);
//  snd_MusicVolume = volume;
}

void I_SetSfxVolume(int volume)
{
  snd_MaxVolume = volume; // THROW AWAY?
}
#endif

/*
 *
 *                              SONG API
 *
 */

public static int I_RegisterSong(w_wad.CacheInfo data)
{
	int rc = 1;// MUS_RegisterSong(data);
  return rc;
}
public static void I_UnRegisterSong(int handle)
{
 // int rc = MUS_UnregisterSong(handle);
}
#if DOS

int I_QrySongPlaying(int handle)
{
  int rc = MUS_QrySongPlaying(handle);
  return rc;
}
#endif

// Stops a song.  MUST be called before I_UnregisterSong().

static int songId = 0;
//static MediaPlayer.MediaPlayer mp = null;
static MidiSequencer midiseq = null;
static StreamSynthesizer synth = null;
public static DynamicSoundEffectInstance musicAudioOut = null;
static byte[] buffer;

public static void I_StopSong(int handle)
{
	if (midiseq != null)
	{
		midiseq.Stop(true);
	}

  //int rc;
  //rc = MUS_StopSong(handle);
  //// Fucking kluge pause
  //{
  //  int s;
  //  extern volatile int ticcount;
  //  for (s=ticcount ; ticcount - s < 10 ; );
  //}
}

public static void UpdateSong()
{
	//call update
	if (midiseq == null) return;
	if (musicAudioOut.State == SoundState.Playing && musicAudioOut.PendingBufferCount < 3)
	{
		synth.GetNext(buffer);
		musicAudioOut.SubmitBuffer(buffer);
	}
}

public static void I_PlaySong(int handle, bool looping)
{
	if (!Game1.instance.musicEnabled) return;
//	BinaryReader br = new BinaryReader(new FileStream("Content/d_e1m1.mid", FileMode.Open, FileAccess.Read, FileShare.Read));
	byte[] data = i_ibm.mus_sndptr.data;// br.ReadBytes((int)br.BaseStream.Length);
	string the_midi_file = "midi" + i_ibm.mus_sndptr.lumpInfo.position + ".mid";

	qmus2mid.mus2mid(data, the_midi_file, false, 0, 0, false);

	// Lazy loading of the sequencer
	if (midiseq == null)
	{
		//Create a new synth with 44100 sample rate, 2 channel audio, and 100ms sample buffer
		//Note: pan can not be used on 1 channel audio
		synth = new StreamSynthesizer(44100, 2, 100, 40);
		musicAudioOut = new DynamicSoundEffectInstance(synth.SampleRate, synth.Channels == 1 ? AudioChannels.Mono : AudioChannels.Stereo);
		buffer = new byte[synth.BufferSize];
		//Load a bank through the synth
		synth.LoadBank("Content/GM Bank/gm.txt");
		midiseq = new MidiSequencer(synth);
		musicAudioOut.Play();
	}
	else
	{
		midiseq.Stop(true);
	}

	midiseq.LoadMidi(the_midi_file, false);
	midiseq.Looping = looping;
	musicAudioOut.Volume = (float)Settings.Default.sfx_volume / 16.0f;
	midiseq.Play();

  //int rc;
  //rc = MUS_ChainSong(handle, looping ? handle : -1);
  //rc = MUS_PlaySong(handle, snd_MusicVolume);
}

/*
 *
 *                                 SOUND FX API
 *
 */

// Gets lump nums of the named sound.  Returns pointer which will be
// passed to I_StartSound() when you want to start an SFX.  Must be
// sure to pass this to UngetSoundEffect() so that they can be
// freed!


public static int I_GetSfxLumpNum(sounds.sfxinfo_t sound)
{
	if(sound.name == "")
		return 0;
	if (sound.link != null) sound = sound.link;
	return w_wad.W_GetNumForName(sound.name);
}

#if DOS

int I_StartSound (int id, void *data, int vol, int sep, int pitch, int priority)
{
/*
  // hacks out certain PC sounds
  if (snd_SfxDevice == PC
	&& (data == S_sfx[sfx_posact].data
	||  data == S_sfx[sfx_bgact].data
	||  data == S_sfx[sfx_dmact].data
	||  data == S_sfx[sfx_dmpain].data
	||  data == S_sfx[sfx_popain].data
	||  data == S_sfx[sfx_sawidl].data)) return -1;

  else
		*/
	return SFX_PlayPatch(data, pitch, sep, vol, 0, 0);

}

#endif

		static public int soundGlobalHandle = 0;
		static public Dictionary<int, SoundEffectInstance> soundsByHandle = new Dictionary<int, SoundEffectInstance>();

public static void I_StopSound(int handle)
{
	if (!soundsByHandle.ContainsKey(handle)) return;
	soundsByHandle[handle].Stop();
	soundsByHandle.Remove(handle);
}

public static int I_SoundIsPlaying(int handle)
{
	if (!soundsByHandle.ContainsKey(handle)) return 0;
	return (soundsByHandle[handle].State == SoundState.Playing) ? 1 : 0;
}

#if DOS

void I_UpdateSoundParams(int handle, int vol, int sep, int pitch)
{
  SFX_SetOrigin(handle, pitch, sep, vol);
}

/*
 *
 *                                                      SOUND STARTUP STUFF
 *
 *
 */

//
// Why PC's Suck, Reason #8712
//

void I_sndArbitrateCards(void)
{
	char tmp[160];
  boolean gus, adlib, pc, sb, midi;
  int i, rc, mputype, p, opltype, wait, dmxlump;

  snd_MusicDevice = snd_DesiredMusicDevice;
  snd_SfxDevice = snd_DesiredSfxDevice;

  // check command-line parameters- overrides config file
  //
  if (M_CheckParm("-nosound")) snd_MusicDevice = snd_SfxDevice = snd_none;
  if (M_CheckParm("-nosfx")) snd_SfxDevice = snd_none;
  if (M_CheckParm("-nomusic")) snd_MusicDevice = snd_none;

  if (snd_MusicDevice > snd_MPU && snd_MusicDevice <= snd_MPU3)
	snd_MusicDevice = snd_MPU;
  if (snd_MusicDevice == snd_SB)
	snd_MusicDevice = snd_Adlib;
  if (snd_MusicDevice == snd_PAS)
	snd_MusicDevice = snd_Adlib;

  // figure out what i've got to initialize
  //
  gus = snd_MusicDevice == snd_GUS || snd_SfxDevice == snd_GUS;
  sb = snd_SfxDevice == snd_SB || snd_MusicDevice == snd_SB;
  adlib = snd_MusicDevice == snd_Adlib ;
  pc = snd_SfxDevice == snd_PC;
  midi = snd_MusicDevice == snd_MPU;

  // initialize whatever i've got
  //
  if (gus)
  {
	if (GF1_Detect()) tprintf("Dude.  The GUS ain't responding.\n",1);
	else
	{
	  dmxlump = W_GetNumForName("dmxgus");
	  GF1_SetMap(W_CacheLumpNum(dmxlump, PU_CACHE), lumpinfo[dmxlump].size);
	}

  }
  if (sb)
  {
	if(debugmode)
	{
		sprintf(tmp,"cfg p=0x%x, i=%d, d=%d\n",
	  snd_SBport, snd_SBirq, snd_SBdma);
	  tprintf(tmp,0);
	}
	if (SB_Detect(&snd_SBport, &snd_SBirq, &snd_SBdma, 0))
	{
	  sprintf(tmp,"SB isn't responding at p=0x%x, i=%d, d=%d\n",
	  snd_SBport, snd_SBirq, snd_SBdma);
	  tprintf(tmp,0);
	}
	else SB_SetCard(snd_SBport, snd_SBirq, snd_SBdma);

	if(debugmode)
	{
		sprintf(tmp,"SB_Detect returned p=0x%x,i=%d,d=%d\n",
	  snd_SBport, snd_SBirq, snd_SBdma);
	  tprintf(tmp,0);
	}
  }

  if (adlib)
  {
	if (AL_Detect(&wait,0))
	  tprintf("Dude.  The Adlib isn't responding.\n",1);
	else
		AL_SetCard(wait, W_CacheLumpName("genmidi", PU_STATIC));
  }

  if (midi)
  {
	if (debugmode)
	{
		sprintf(tmp,"cfg p=0x%x\n", snd_Mport);
		tprintf(tmp,0);
	}

	if (MPU_Detect(&snd_Mport, &i))
	{
	  sprintf(tmp,"The MPU-401 isn't reponding @ p=0x%x.\n", snd_Mport);
	  tprintf(tmp,0);
	}
	else MPU_SetCard(snd_Mport);
  }

}

// inits all sound stuff

void I_StartupSound (void)
{
	char tmp[80];
  int rc, i;

  if (debugmode)
	tprintf("I_StartupSound: Hope you hear a pop.\n",1);

  // initialize dmxCodes[]
  dmxCodes[0] = 0;
  dmxCodes[snd_PC] = AHW_PC_SPEAKER;
  dmxCodes[snd_Adlib] = AHW_ADLIB;
  dmxCodes[snd_SB] = AHW_SOUND_BLASTER;
  dmxCodes[snd_PAS] = AHW_MEDIA_VISION;
  dmxCodes[snd_GUS] = AHW_ULTRA_SOUND;
  dmxCodes[snd_MPU] = AHW_MPU_401;
  dmxCodes[snd_AWE] = AHW_AWE32;

  // inits sound library timer stuff
  I_StartupTimer();

  // pick the sound cards i'm going to use
  //
  I_sndArbitrateCards();

  if (debugmode)
  {
	sprintf(tmp,"  Music device #%d & dmxCode=%d", snd_MusicDevice,
	  dmxCodes[snd_MusicDevice]);
	tprintf(tmp,0);
	sprintf(tmp,"  Sfx device #%d & dmxCode=%d\n", snd_SfxDevice,
	  dmxCodes[snd_SfxDevice]);
	tprintf(tmp,0);
  }

  // inits DMX sound library
  tprintf("  calling DMX_Init",0);
  rc = DMX_Init(SND_TICRATE, SND_MAXSONGS, dmxCodes[snd_MusicDevice],
	dmxCodes[snd_SfxDevice]);

  if (debugmode)
  {
	sprintf(tmp,"  DMX_Init() returned %d", rc);
	tprintf(tmp,0);
  }

}

// shuts down all sound stuff

void I_ShutdownSound (void)
{
  DMX_DeInit();
  I_ShutdownTimer();
}

void I_SetChannels(int channels)
{
  WAV_PlayMode(channels, SND_SAMPLERATE);
}

#endif
	}
}
