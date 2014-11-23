using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HereticXNA
{
	public static class qmus2mid
	{

		public const int NOTMUSFILE = 1;     /* Not a MUS file */
		public const int COMUSFILE = 2;      /* Can't open MUS file */
		public const int COTMPFILE = 3;     /* Can't open TMP file */
		public const int CWMIDFILE = 4;       /* Can't write MID file */
		public const int MUSFILECOR = 5;      /* MUS file corrupted */
		public const int TOOMCHAN = 6;       /* Too many channels */
		public const int MEMALLOC = 7;       /* Memory allocation error */
		/* some (old) compilers mistake the "MUS\x1A" construct (interpreting
		   it as "MUSx1A")      */

		public const string MUSMAGIC = "MUS\032";       /* this seems to work */
		public const string MIDIMAGIC = "MThd\000\000\000\006\000\001";
		//public const string TRACKMAGIC1 = "\000\377\003\035";
		//public const string TRACKMAGIC2 = "\000\377\057\000";
		//public const string TRACKMAGIC3 = "\000\377\002\026";
		//public const string TRACKMAGIC4 = "\000\377\131\002\000\000";
		//public const string TRACKMAGIC5 = "\000\377\121\003\011\243\032";
		//public const string TRACKMAGIC6 = "\000\377\057\000";


		class MUSheader
		{
			public string ID;            /* identifier "MUS" 0x1A */
			public short ScoreLength;
			public short ScoreStart;
			public short channels;         /* count of primary channels */
			public short SecChannels;      /* count of secondary channels (?) */
			public short InstrCnt;
			public short dummy;
			/* variable-length part starts here */
			public short[] instruments;
		} ;

		struct Track
		{
			public ulong current;
			public sbyte vel;
			public long DeltaTime;
			public byte LastEvent;
			public byte[] data;            /* Primary data */
		};

		/*
   Quick MUS to Midi converter.
   (C) 1995,96 Sebastien Bacquet  ( bacquet@iie.cnam.fr )
   
   Ported to unix by Hans Peter Verne ( hpv@kjemi.uio.no )

   This is free software, distributed under the terms of the
   GNU General Public License. For details see the file COPYING.

   Use gcc to compile, if possible.  Please look in  "qmus2mid.h"
   for system dependencies, in particular the int2 and int4 typedef's.

   To compile for MS-DOS, #define MSDOG or use -DMSDOG parameter

   Otherwise, vanilla unix is assumed, but it still compiles under dos.

   For the time being, this only works for little-endian machines,
   such as i86, dec-mips, alpha;  but not rs6000, sparc....

*/

		public static int TRACKBUFFERSIZE = 65536;  /* 64 Ko */

static void fwrite2(short ptr, BinaryWriter fs)
{
  int rev = 0;
  int i;
  
  for( i = 0 ; i < 2 ; i++ )
    rev = (rev << 8) + (((ptr) >> (i*8)) & 0xFF) ;

	fs.Write((short)rev);
}

		static void FreeTracks(Track[] track)
		{
			int i;

			for (i = 0; i < 16; i++)
				if (track[i].data != null)
					track[i].data = null;
		}

		static void Close() { }


		static void TWriteByte(sbyte MIDItrack, sbyte byte_, Track[] track)
		{
			int pos;

			byte_ = (sbyte)(byte)byte_;

			pos = (int)track[MIDItrack].current;
			if (pos < TRACKBUFFERSIZE)
				track[MIDItrack].data[pos] = (byte)byte_;
			else
			{
				Console.Write("ERROR : Track buffer full.\nIncrease the track buffer size (option -size).\n");
				FreeTracks(track);
				Close();
				Game1.instance.Exit();
			}
			track[MIDItrack].current++;
		}


		static void TWriteVarLen(int tracknum, int value, Track[] track)
		{
			int buffer;

			buffer = value & 0x7f;
			while ((value >>= 7) != 0)
			{
				buffer <<= 8;
				buffer |= 0x80;
				buffer += (value & 0x7f);
			}
			while (true)
			{
				TWriteByte((sbyte)tracknum, (sbyte)buffer, track);
				if ((buffer & 0x80) != 0)
					buffer >>= 8;
				else
					break;
			}
		}

		static int ReadMUSheader(MUSheader MUSh, BinaryReader file)
		{
			for (int i = 0; i < 4; ++i)
			{
				byte b = file.ReadByte();
				char c = (char)b;
				MUSh.ID += c;
			}
			//	if (MUSh.ID != MUSMAGIC) return NOTMUSFILE; // [dsl] Whatever just trust it
			MUSh.ScoreLength = file.ReadInt16();
			MUSh.ScoreStart = file.ReadInt16();
			MUSh.channels = file.ReadInt16();
			MUSh.SecChannels = file.ReadInt16();
			MUSh.InstrCnt = file.ReadInt16();
			MUSh.dummy = file.ReadInt16();
			MUSh.instruments = new short[MUSh.InstrCnt];
			for (int i = 0; i < MUSh.InstrCnt; ++i)
			{
				MUSh.instruments[i] = file.ReadInt16();
			}
			return 0;
		}


		static int WriteMIDheader(short ntrks, short division, BinaryWriter file)
		{
			file.Write((byte)0x4D);
			file.Write((byte)0x54);
			file.Write((byte)0x68);
			file.Write((byte)0x64);
			file.Write((byte)0);
			file.Write((byte)0);
			file.Write((byte)0);
			file.Write((byte)6);
			fwrite2(1, file);
			fwrite2(ntrks, file);
			fwrite2(division, file);
			return 0;
		}

		/* maybe for ms-dog too ? */
		/* Yes, why not ?... */
		static byte last(int e)
		{
			return ((byte)(e & 0x80));
		}
		static byte event_type(int e)
		{
			return ((byte)((e & 0x7F) >> 4));
		}
		static byte Channel(int e)
		{
			return ((byte)(e & 0x0F));
		}
#if DOS

void TWriteString( char tracknum, const char *string, int length,
                   struct Track track[] )
{
  register int i ;

  for( i = 0 ; i < length ; i++ )
    TWriteByte( tracknum, string[i], track ) ;
}

#endif

		static void WriteTrack(int tracknum, BinaryWriter file, Track[] track)
		{
			short size;

			/* Do we risk overflow here ? */
			size = (short)(track[tracknum].current + 4);
			file.Write((byte)0x4D);
			file.Write((byte)0x54);
			file.Write((byte)0x72);
			file.Write((byte)0x6B);

			byte[] temp = BitConverter.GetBytes((uint)size);
			Array.Reverse(temp);
			file.Write(BitConverter.ToUInt32(temp, 0));

			file.Write(track[tracknum].data, 0, (int)track[tracknum].current);

			file.Write((byte)Convert.ToInt32("0", 8));
			file.Write((byte)Convert.ToInt32("377", 8));
			file.Write((byte)Convert.ToInt32("57", 8));
			file.Write((byte)Convert.ToInt32("0", 8));
		}

		static int ReadTime(BinaryReader file)
		{
			int time = 0;
			int byte_;

			do
			{
				byte_ = file.ReadSByte();
				if (byte_ != -1) time = (time << 7) + (byte_ & 0x7F);
			} while ((byte_ != -1) && ((byte_ & 0x80) != 0));

			return time;
		}

		static sbyte FirstChannelAvailable(sbyte[] MUS2MIDchannel)
		{
			int i;
			sbyte old15 = MUS2MIDchannel[15], max = -1;

			MUS2MIDchannel[15] = -1;
			for (i = 0; i < 16; i++)
				if (MUS2MIDchannel[i] > max) max = MUS2MIDchannel[i];
			MUS2MIDchannel[15] = old15;

			return (sbyte)(max == 8 ? 10 : max + 1);
		}

		public static int mus2mid(byte[] mus, string mid, bool nodisplay,
					 short division, int BufferSize, bool nocomp)
		{
			Track[] track = new Track[16] {
	  new Track(),new Track(),new Track(),new Track(),
	  new Track(),new Track(),new Track(),new Track(),
	  new Track(),new Track(),new Track(),new Track(),
	  new Track(),new Track(),new Track(),new Track()
  };
			short TrackCnt = 0;
			//  FILE *file_mus, *file_mid ;
			byte et, MUSchannel, MIDIchannel, MIDItrack, NewEvent;
			int i, event_, data, r;
			MUSheader MUSh = new MUSheader();
			int DeltaTime, TotalTime = 0, time, min, n = 0;
			byte[] MUS2MIDcontrol = new byte[15] {
    0,                          /* Program change - not a MIDI control change */
    0x00,                       /* Bank select */
    0x01,                       /* Modulation pot */
    0x07,                       /* Volume */
    0x0A,                       /* Pan pot */
    0x0B,                       /* Expression pot */
    0x5B,                       /* Reverb depth */
    0x5D,                       /* Chorus depth */
    0x40,                       /* Sustain pedal */
    0x43,                       /* Soft pedal */
    0x78,                       /* All sounds off */
    0x7B,                       /* All notes off */
    0x7E,                       /* Mono */
    0x7F,                       /* Poly */
    0x79                        /* Reset all controllers */
  };
			byte[] MIDIchan2track = new byte[16];
			sbyte[] MUS2MIDchannel = new sbyte[16];
			sbyte ouch = 0, sec;
			/* stat file_data = new stat() ;

			 if( (file_mus = fopen( mus, "rb" )) == NULL )
			   return COMUSFILE ;
			 stat( mus, &file_data ) ;*/

			/*  Why bother with a tmp-file anyway ? */
			/*  If I could have done differently...You know, DOS is DOS... */
			BinaryWriter file_mid = null;
			try
			{
				file_mid = new BinaryWriter(new FileStream(mid, FileMode.Create));
			}
			catch (Exception _exception)
			{
				Close();
				return CWMIDFILE;
			}


			BinaryReader file_mus = new BinaryReader(new MemoryStream(mus));
			ReadMUSheader(MUSh, file_mus);
			if (file_mus.BaseStream.Length < MUSh.ScoreStart) return MUSFILECOR;
			if (MUSh.channels > 15)      /* <=> MUSchannels+drums > 16 */
			{
				Close();
				return TOOMCHAN;
			}

			for (i = 0; i < 16; i++)
			{
				MUS2MIDchannel[i] = -1;
				track[i].current = 0;
				track[i].vel = 64;
				track[i].DeltaTime = 0;
				track[i].LastEvent = 0;
				track[i].data = null;
			}
			//if( BufferSize )
			//  {
			//    TRACKBUFFERSIZE = ((int4) BufferSize) << 10 ;
			//    if( !nodisplay )
			//      printf( "Track buffer size set to %d KB.\n", BufferSize ) ;
			//  }

			event_ = (int)file_mus.ReadByte();
			et = event_type(event_);
			MUSchannel = Channel(event_);
			while ((et != 6) && /*!feof( file_mus ) &&*/ (event_ != -1))
			{
				if (MUS2MIDchannel[MUSchannel] == -1)
				{
					MUS2MIDchannel[MUSchannel] = (sbyte)(MUSchannel == 15 ? 9 : FirstChannelAvailable(MUS2MIDchannel));
					MIDIchannel = (byte)MUS2MIDchannel[MUSchannel];
					MIDItrack = MIDIchan2track[MIDIchannel] = (byte)TrackCnt++;
					track[MIDItrack].data = new byte[TRACKBUFFERSIZE];
				}
				else
				{
					MIDIchannel = (byte)MUS2MIDchannel[MUSchannel];
					MIDItrack = MIDIchan2track[MIDIchannel];
				}
				TWriteVarLen((int)MIDItrack, (int)track[MIDItrack].DeltaTime, track);
				track[MIDItrack].DeltaTime = 0;
				switch (et)
				{
					case 0:                /* release note */
						NewEvent = (byte)(0x90 | MIDIchannel);
						if ((NewEvent != track[MIDItrack].LastEvent) || nocomp)
						{
							TWriteByte((sbyte)MIDItrack, (sbyte)NewEvent, track);
							track[MIDItrack].LastEvent = NewEvent;
						}
						else
							n++;
						data = file_mus.ReadSByte();
						TWriteByte((sbyte)MIDItrack, (sbyte)data, track);
						TWriteByte((sbyte)MIDItrack, 0, track);
						break;
					case 1:
						NewEvent = (byte)(0x90 | MIDIchannel);
						if ((NewEvent != track[MIDItrack].LastEvent) || nocomp)
						{
							TWriteByte((sbyte)MIDItrack, (sbyte)NewEvent, track);
							track[MIDItrack].LastEvent = NewEvent;
						}
						else
							n++;
						data = file_mus.ReadSByte();
						TWriteByte((sbyte)MIDItrack, (sbyte)(data & 0x7F), track);
						if ((data & 0x80) != 0)
							track[MIDItrack].vel = file_mus.ReadSByte();
						TWriteByte((sbyte)MIDItrack, (sbyte)track[MIDItrack].vel, track);
						break;
					case 2:
						NewEvent = (byte)(0xE0 | MIDIchannel);
						if ((NewEvent != track[MIDItrack].LastEvent) || nocomp)
						{
							TWriteByte((sbyte)MIDItrack, (sbyte)NewEvent, track);
							track[MIDItrack].LastEvent = NewEvent;
						}
						else
							n++;
						data = file_mus.ReadSByte();
						TWriteByte((sbyte)MIDItrack, (sbyte)((data & 1) << 6), track);
						TWriteByte((sbyte)MIDItrack, (sbyte)(data >> 1), track);
						break;
					case 3:
						NewEvent = (byte)(0xB0 | MIDIchannel);
						if ((NewEvent != track[MIDItrack].LastEvent) || nocomp)
						{
							TWriteByte((sbyte)MIDItrack, (sbyte)NewEvent, track);
							track[MIDItrack].LastEvent = NewEvent;
						}
						else
							n++;
						data = file_mus.ReadSByte();
						TWriteByte((sbyte)MIDItrack, (sbyte)MUS2MIDcontrol[data], track);
						if (data == 12)
							TWriteByte((sbyte)MIDItrack, (sbyte)(MUSh.channels + 1), track);
						else
							TWriteByte((sbyte)MIDItrack, 0, track);
						break;
					case 4:
						data = file_mus.ReadSByte();
						if (data != 0)
						{
							NewEvent = (byte)(0xB0 | MIDIchannel);
							if ((NewEvent != track[MIDItrack].LastEvent) || nocomp)
							{
								TWriteByte((sbyte)MIDItrack, (sbyte)NewEvent, track);
								track[MIDItrack].LastEvent = NewEvent;
							}
							else
								n++;
							TWriteByte((sbyte)MIDItrack, (sbyte)MUS2MIDcontrol[data], track);
						}
						else
						{
							NewEvent = (byte)(0xC0 | MIDIchannel);
							if ((NewEvent != track[MIDItrack].LastEvent) || nocomp)
							{
								TWriteByte((sbyte)MIDItrack, (sbyte)NewEvent, track);
								track[MIDItrack].LastEvent = NewEvent;
							}
							else
								n++;
						}
						data = file_mus.ReadSByte();
						TWriteByte((sbyte)MIDItrack, (sbyte)data, track);
						break;
					case 5:
					case 7:
						FreeTracks(track);
						Close();
						return MUSFILECOR;
					default: break;
				}
				if (last(event_) != 0)
				{
					DeltaTime = ReadTime(file_mus);
					TotalTime += DeltaTime;
					for (i = 0; i < (int)TrackCnt; i++)
						track[i].DeltaTime += DeltaTime;
				}
				event_ = file_mus.ReadSByte();
				if (event_ != -1)
				{
					et = event_type(event_);
					MUSchannel = Channel(event_);
				}
				else
					ouch = 1;
			}
			if (!nodisplay) Console.Write("done !\n");
			if (ouch != 0)
				Console.Write("WARNING : There are bytes missing at the end of mus.\n          The end of the MIDI file might not fit the original one.\n");
			if (division == 0)
				division = 89;
			else
				if (!nodisplay) Console.Write("Ticks per quarter note set to " + division + ".\n");
			if (!nodisplay)
			{
				if (division != 89)
				{
					time = TotalTime / 140;
					min = time / 60;
					sec = (sbyte)(time - min * 60);
					Console.Write("Playing time of the MUS file : " + min + "'" + sec + "''.\n");
				}
				time = (TotalTime * 89) / (140 * division);
				min = time / 60;
				sec = (sbyte)(time - min * 60);
				if (division != 89)
					Console.Write("                    MID file");
				else
					Console.Write("Playing time");
				Console.Write(" : " + min + "'" + sec + "''.\n");
			}
			if (!nodisplay)
			{
				Console.Write("Writing...");
				//   fflush( stdout ) ;
			}
			WriteMIDheader((short)(TrackCnt/* + 1*/), (short)division, file_mid);
		//	WriteFirstTrack(file_mid);
			for (i = 0; i < (int)TrackCnt; i++)
				WriteTrack(i, file_mid, track);
			if (!nodisplay)
				Console.Write("done !\n");
			if (!nodisplay && !nocomp)
				Console.Write("Compression : " + ((100 * n) / (n + (int)(file_mid.BaseStream.Length))) + ".\n");

			file_mid.Close();
			return 0;
		}
#if DOS

int convert( const char *mus, const char *mid, int nodisplay, int div,
            int size, int nocomp, int *ow )
{
  FILE *file ;
  int error;
  struct stat file_data ;
  char buffer[30] ;


  /* we don't need _all_ that checking, do we ? */
  /* Answer : it's more user-friendly */
  if ( !*ow ) {
    file = fopen(mid, "r");
    if ( file ) {
      fclose(file);
      printf( "qmus2mid: file %s exists, not removed.\n", mid ) ;
      return 2 ;
    }
  }

  error = qmus2mid( mus, mid, nodisplay, div, size, nocomp ) ;

  if( error )
    {
      printf( "ERROR : " ) ;
      switch( error )
        {
        case NOTMUSFILE :
          printf( "%s is not a MUS file.\n", mus ) ; break ;
        case COMUSFILE :
          printf( "Can't open %s for read.\n", mus ) ; break ;
        case COTMPFILE :
          printf( "Can't open temp file.\n" ) ; break  ;
        case CWMIDFILE :
          printf( "Can't write %s (?).\n", mid ) ; break ;
        case MUSFILECOR :
          printf( "%s is corrupted.\n", mus ) ; break ;
        case TOOMCHAN :
          printf( "%s contains more than 16 channels.\n", mus ) ; break ;
        case MEMALLOC :
          printf( "Not enough memory.\n" ) ; break ;
        default : break ;
        }
      return 4 ;
    }

  if( !nodisplay )
    {
      printf( "%s converted successfully.\n", mus ) ;
      if( (file = fopen( mid, "rb" )) != NULL )
        {
          stat( mid, &file_data ) ;
          fclose( file ) ;
          sprintf( buffer, " : %lu bytes", (unsigned long) file_data.st_size ) ;
        }
      printf( "%s (%scompressed) written%s.\n", mid, nocomp ? "NOT " : "",
             file ? buffer : ""  ) ;
    }

  return 0 ;
}


int CheckParm( char *check, int argc, char *argv[] )
{
  int i;

  for ( i = 1 ; i<argc ; i++ )
    if( !strcmp( check, argv[i] ) )
      return i ;

  return 0;
}


void PrintHeader( void )
{
  printf( "===============================================================================\n"
         "              Quick MUS.MID v2.0 ! (C) 1995,96 Sebastien Bacquet\n"
         "                        E-mail : bacquet@iie.cnam.fr\n"
         "===============================================================================\n" ) ;
}


void PrintSyntax( void )
{
  PrintHeader() ;
  printf( 
         "\nSyntax : QMUS2MID musfile midifile [options]\n"
         "   Options are :\n"
         "     -noow     : Don't overwrite !\n"
         "     -nodisp   : Display nothing ! (except errors)\n"
         "     -nocomp   : Don't compress !\n"
         "     -size ### : Set the track buffer size to ### (in KB). "
         "Default = 64 KB\n"
         "     -t ###    : Ticks per quarter note. Default = 89\n" 
         ) ;
}


int main( int argc, char *argv[] )
{
  int div = 0, ow = 1, nodisplay = 0, nocomp = 0, size = 0, n ;
  char mus[FILENAME_MAX], mid[FILENAME_MAX];


  if ( !LittleEndian() ) {
    printf("\nSorry, this program presently only works on "
	   "little-endian machines... \n\n");
    exit( EXIT_FAILURE ) ;
  }

    if( argc < 3 )
      {
        PrintSyntax() ;
        exit( EXIT_FAILURE ) ;
      }

  strncpy( mus, argv[1], FILENAME_MAX ) ;
  strncpy( mid, argv[2], FILENAME_MAX ) ;


  if( CheckParm( "-nodisp", argc, argv ) )
    nodisplay = 1 ;
  
  if( !nodisplay )
    PrintHeader() ;
  
  if( (n = CheckParm( "-size", argc, argv )) != 0 )
    size = atoi( argv[n+1] ) ;
  if( CheckParm( "-noow", argc, argv ) )
    ow -= 1 ;
  if( (n = CheckParm( "-t", argc, argv )) != 0 )
    div = atoi( argv[n+1] ) ;
  if( CheckParm( "-nocomp", argc, argv ) )
    nocomp = 1 ;
      convert( mus, mid, nodisplay, div, size, nocomp, &ow ) ;

  return 0;
}
  

#endif
	}
}
