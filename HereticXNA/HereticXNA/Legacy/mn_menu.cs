using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace HereticXNA
{
	static public class mn_menu
	{
		public const int LEFT_DIR = 0;
		public const int RIGHT_DIR = 1;
		public const int ITEM_HEIGHT = 20;
		public const int SELECTOR_XOFFSET = (-28);
		public const int SELECTOR_YOFFSET = (-1);
		public const int SLOTTEXTLEN = 16;
		public const char ASCII_CURSOR = '[';

		// Types

		enum ItemType_t
		{
			ITT_EMPTY,
			ITT_EFUNC,
			ITT_LRFUNC,
			ITT_SETMENU,
			ITT_INERT
		};

		enum MenuType_t
		{
			MENU_MAIN,
			MENU_EPISODE,
			MENU_SKILL,
			MENU_OPTIONS,
			MENU_OPTIONS2,
			MENU_FILES,
			MENU_LOAD,
			MENU_SAVE,
			MENU_NONE
		};

		class MenuItem_t_func_delegate
		{
			public virtual bool func(int option) { return false; }
		}
		class MenuItem_t
		{
			public ItemType_t type;
			public string text;
			public MenuItem_t_func_delegate func;
			public int option;
			public MenuType_t menu;
		}

		class Menu_t_drawFunc_delegate
		{
			public virtual void drawFunc() { }
		}
		class Menu_t
		{
			public int x;
			public int y;
			public Menu_t_drawFunc_delegate drawFunc;
			public int itemCount;
			public MenuItem_t[] items;
			public int oldItPos;
			public MenuType_t prevMenu;
		}

		// Private Functions

		// External Data
		// Public Data

		public static bool MenuActive;
		public static int InfoType;
		public static bool messageson;

		// Private Data

		static int FontABaseLump;
		static int FontBBaseLump;
		static int SkullBaseLump;
		static Menu_t CurrentMenu = MainMenu;
		static int CurrentItPos;
		static int MenuEpisode;
		static int MenuTime = 0;
		static bool soundchanged;

		public static bool askforquit;
		public static int typeofask; // [dsl] Doom had it as a boolean, but its a define to int for them
		static bool FileMenuKeySteal;
		static bool slottextloaded;
		static string[] SlotText = new string[6];
		static string oldSlotText;
		static int[] SlotStatus = new int[6];
		static int slotptr;
		static int currentSlot;
		static int quicksave;
		static int quickload;

		static MenuItem_t[] MainItems = new MenuItem_t[]
{
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "NEW GAME", func = new SCNetCheck(), option = 1, menu = MenuType_t.MENU_EPISODE },
	new MenuItem_t { type = ItemType_t.ITT_SETMENU, text = "OPTIONS", func = null, option = 0, menu = MenuType_t.MENU_OPTIONS },
	new MenuItem_t { type = ItemType_t.ITT_SETMENU, text = "GAME FILES", func = null, option = 0, menu = MenuType_t.MENU_FILES },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "INFO", func = new SCInfo(), option = 0, menu = MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "QUIT GAME", func = new SCQuitGame(), option = 0, menu = MenuType_t.MENU_NONE },
};

		static Menu_t MainMenu = new Menu_t
		{
			x = 110,
			y = 56,
			drawFunc = new DrawMainMenu(),
			itemCount = 5,
			items = MainItems,
			oldItPos = 0,
			prevMenu = MenuType_t.MENU_NONE
		};

		static MenuItem_t[] EpisodeItems = new MenuItem_t[]
{
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "CITY OF THE DAMNED", func = new SCEpisode(), option = 1, menu = MenuType_t.MENU_NONE },
	new MenuItem_t{ type = ItemType_t.ITT_EFUNC, text = "HELL'S MAW", func = new SCEpisode(), option = 2, menu = MenuType_t.MENU_NONE },
	new MenuItem_t{ type = ItemType_t.ITT_EFUNC, text = "THE DOME OF D'SPARIL", func = new SCEpisode(), option = 3, menu = MenuType_t.MENU_NONE },
	new MenuItem_t{ type = ItemType_t.ITT_EFUNC, text = "THE OSSUARY", func = new SCEpisode(), option = 4, menu = MenuType_t.MENU_NONE },
	new MenuItem_t{ type = ItemType_t.ITT_EFUNC, text = "THE STAGNANT DEMESNE", func = new SCEpisode(), option = 5, menu = MenuType_t.MENU_NONE }
};
		static Menu_t EpisodeMenu = new Menu_t
		{
			x = 80,
			y = 50,
			drawFunc = new DrawEpisodeMenu(),
			itemCount = 3,
			items = EpisodeItems,
			oldItPos = 0,
			prevMenu = MenuType_t.MENU_MAIN
		};

		static MenuItem_t[] FilesItems = new MenuItem_t[]
{
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "LOAD GAME", func = new SCNetCheck(), option = 2,menu= MenuType_t.MENU_LOAD },
	new MenuItem_t { type = ItemType_t.ITT_SETMENU, text = "SAVE GAME", func = null, option = 0,menu= MenuType_t.MENU_SAVE }
};

		static Menu_t FilesMenu = new Menu_t
		{
			x = 110,
			y = 60,
			drawFunc = new DrawFilesMenu(),
			itemCount = 2,
			items = FilesItems,
			oldItPos = 0,
			prevMenu = MenuType_t.MENU_MAIN
		};

		static MenuItem_t[] LoadItems = new MenuItem_t[]
{
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCLoadGame(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCLoadGame(), option = 1,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCLoadGame(), option = 2,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCLoadGame(), option = 3,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCLoadGame(), option = 4,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCLoadGame(), option = 5,menu= MenuType_t.MENU_NONE },
};

		static Menu_t LoadMenu = new Menu_t
		{
			x = 70,
			y = 30,
			drawFunc = new DrawLoadMenu(),
			itemCount = 6,
			items = LoadItems,
			oldItPos = 0,
			prevMenu = MenuType_t.MENU_FILES
		};

		static MenuItem_t[] SaveItems = new MenuItem_t[]
{
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCSaveGame(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCSaveGame(), option = 1,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCSaveGame(), option = 2,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCSaveGame(), option = 3,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCSaveGame(), option = 4,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "", func = new SCSaveGame(), option = 5,menu= MenuType_t.MENU_NONE },
};

		static Menu_t SaveMenu = new Menu_t
		{
			x = 70,
			y = 30,
			drawFunc = new DrawSaveMenu(),
			itemCount = 6,
			items = SaveItems,
			oldItPos = 0,
			prevMenu = MenuType_t.MENU_FILES
		};

		static MenuItem_t[] SkillItems = new MenuItem_t[]
{
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "THOU NEEDETH A WET-NURSE", func = new SCSkill(), option = (int)DoomDef.skill_t.sk_baby,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "YELLOWBELLIES-R-US", func = new SCSkill(), option = (int)DoomDef.skill_t.sk_easy,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "BRINGEST THEM ONETH", func = new SCSkill(), option = (int)DoomDef.skill_t.sk_medium,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "THOU ART A SMITE-MEISTER", func = new SCSkill(), option = (int)DoomDef.skill_t.sk_hard,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "BLACK PLAGUE POSSESSES THEE", func = new SCSkill(), option = (int)DoomDef.skill_t.sk_nightmare,menu= MenuType_t.MENU_NONE },
};

		static Menu_t SkillMenu = new Menu_t
		{
			x = 38,
			y = 30,
			drawFunc = new DrawSkillMenu(),
			itemCount = 5,
			items = SkillItems,
			oldItPos = 2,
			prevMenu = MenuType_t.MENU_EPISODE
		};

		static MenuItem_t[] OptionsItems = new MenuItem_t[]
{
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "END GAME", func = new SCEndGame(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "MESSAGES : ", func = new SCMessages(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_LRFUNC, text = "MOUSE SENSITIVITY", func = new SCMouseSensi(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EMPTY, text = "", func = null, option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_LRFUNC, text = "SFX VOLUME", func = new SCSfxVolume(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EMPTY, text = "", func = null, option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_LRFUNC, text = "MUSIC VOLUME", func = new SCMusicVolume(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EMPTY, text = "", func = null, option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_SETMENU, text = "MORE...", func = null, option = 0,menu= MenuType_t.MENU_OPTIONS2 },
};

		static Menu_t OptionsMenu = new Menu_t
		{
			x = 88,
			y = 10,
			drawFunc = new DrawOptionsMenu(),
			itemCount = 9,
			items = OptionsItems,
			oldItPos = 0,
			prevMenu = MenuType_t.MENU_MAIN
		};

		static MenuItem_t[] Options2Items = new MenuItem_t[]
{
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "FULLSCREEN : ", func = new SCFullScreen(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "RESOLUTION : ", func = new SCResolution(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "AMBIENT : ", func = new SCAmbient(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "DEFERRED : ", func = new SCDeferred(), option = 0,menu= MenuType_t.MENU_NONE },
	new MenuItem_t { type = ItemType_t.ITT_EFUNC, text = "POST PROCESS : ", func = new SCPostProcess(), option = 0,menu= MenuType_t.MENU_NONE },
};

		static Menu_t Options2Menu = new Menu_t
		{
			x = 70,
			y = 20,
			drawFunc = new DrawOptions2Menu(),
			itemCount = 5,
			items = Options2Items,
			oldItPos = 0,
			prevMenu = MenuType_t.MENU_OPTIONS
		};

		static Menu_t[] Menus = new Menu_t[]
{
	MainMenu,
	EpisodeMenu,
	SkillMenu,
	OptionsMenu,
	Options2Menu,
	FilesMenu,
	LoadMenu,
	SaveMenu
};

		//---------------------------------------------------------------------------
		//
		// PROC MN_Init
		//
		//---------------------------------------------------------------------------
		public static void MN_Init()
		{
			InitFonts();
			MenuActive = false;
			messageson = true;
			SkullBaseLump = w_wad.W_GetNumForName("M_SKL00");
			if (d_main.ExtendedWAD)
			{ // Add episodes 4 and 5 to the menu
				EpisodeMenu.itemCount = 5;
				EpisodeMenu.y -= ITEM_HEIGHT;
			}
		}
		//---------------------------------------------------------------------------
		//
		// PROC InitFonts
		//
		//---------------------------------------------------------------------------

		public static void InitFonts()
		{
			FontABaseLump = w_wad.W_GetNumForName("FONTA_S") + 1;
			FontBBaseLump = w_wad.W_GetNumForName("FONTB_S") + 1;
		}

		//---------------------------------------------------------------------------
		//
		// PROC MN_DrTextA
		//
		// Draw text using font A.
		//
		//---------------------------------------------------------------------------

		public static void MN_DrTextA(string text, int x, int y)
		{
			w_wad.CacheInfo p;

			foreach (char c in text)
			{
				if (c < 33)
				{
					x += 5;
				}
				else
				{
					p = w_wad.W_CacheLumpNum(FontABaseLump + c - 33, DoomDef.PU_CACHE);
					v_video.V_DrawPatch(x, y, p);
					x += (p.cache as v_video.PatchTexture).patch.width - 1;
				}
			}
		}

		//---------------------------------------------------------------------------
		//
		// FUNC MN_TextAWidth
		//
		// Returns the pixel width of a string using font A.
		//
		//---------------------------------------------------------------------------

		public static int MN_TextAWidth(string text)
		{
			int width;
			w_wad.CacheInfo p;

			width = 0;
			foreach (char c in text)
			{
				if (c < 33)
				{
					width += 5;
				}
				else
				{
					p = w_wad.W_CacheLumpNum(FontABaseLump + c - 33, DoomDef.PU_CACHE);
					v_video.BuildPatchTextureIfNot(p);
					width += (p.cache as v_video.PatchTexture).patch.width - 1;
				}
			}
			return (width);
		}

		//---------------------------------------------------------------------------
		//
		// PROC MN_DrTextB
		//
		// Draw text using font B.
		//
		//---------------------------------------------------------------------------

		static public void MN_DrTextB(string text, int x, int y)
		{
			w_wad.CacheInfo p;

			foreach (char c in text)
			{
				if (c < 33)
				{
					x += 8;
				}
				else
				{
					p = w_wad.W_CacheLumpNum(FontBBaseLump + c - 33, DoomDef.PU_CACHE);
					v_video.V_DrawPatch(x, y, p);
					x += (p.cache as v_video.PatchTexture).patch.width - 1;
				}
			}
		}

		//---------------------------------------------------------------------------
		//
		// FUNC MN_TextBWidth
		//
		// Returns the pixel width of a string using font B.
		//
		//---------------------------------------------------------------------------

		public static int MN_TextBWidth(string text)
		{
			int width;
			w_wad.CacheInfo p;

			width = 0;
			foreach (char c in text)
			{
				if (c < 33)
				{
					width += 5;
				}
				else
				{
					p = w_wad.W_CacheLumpNum(FontBBaseLump + c - 33, DoomDef.PU_CACHE);
					v_video.BuildPatchTextureIfNot(p);
					width += (p.cache as v_video.PatchTexture).patch.width - 1;
				}
			}
			return (width);
		}

		//---------------------------------------------------------------------------
		//
		// PROC MN_Ticker
		//
		//---------------------------------------------------------------------------

		public static void MN_Ticker()
		{
			if (MenuActive == false)
			{
				return;
			}
			MenuTime++;
		}

		//---------------------------------------------------------------------------
		//
		// PROC MN_Drawer
		//
		//---------------------------------------------------------------------------

		static string[] QuitEndMsg = new string[]
{
	"ARE YOU SURE YOU WANT TO QUIT?",
	"ARE YOU SURE YOU WANT TO END THE GAME?",
	"DO YOU WANT TO QUICKSAVE THE GAME NAMED",
	"DO YOU WANT TO QUICKLOAD THE GAME NAMED"
};
		public static void MN_Drawer()
		{
			int i;
			int x;
			int y;
			string selName;

			if (MenuActive == false)
			{
				if (askforquit)
				{
					MN_DrTextA(QuitEndMsg[typeofask - 1], 160 -
						MN_TextAWidth(QuitEndMsg[typeofask - 1]) / 2, 80);
					if (typeofask == 3)
					{
						MN_DrTextA(SlotText[quicksave - 1], 160 -
							MN_TextAWidth(SlotText[quicksave - 1]) / 2, 90);
						MN_DrTextA("?", 160 +
							MN_TextAWidth(SlotText[quicksave - 1]) / 2, 90);
					}
					if (typeofask == 4)
					{
						MN_DrTextA(SlotText[quickload - 1], 160 -
							MN_TextAWidth(SlotText[quickload - 1]) / 2, 90);
						MN_DrTextA("?", 160 +
							MN_TextAWidth(SlotText[quickload - 1]) / 2, 90);
					}
					i_ibm.UpdateState |= DoomDef.I_FULLSCRN;
				}
				return;
			}
			else
			{
				if (InfoType != 0)
				{
					MN_DrawInfo();
					return;
				}
#if DOS
		if(screenblocks < 10)
		{
			BorderNeedRefresh = true;
		}
#endif
				if (CurrentMenu.drawFunc != null)
				{
					CurrentMenu.drawFunc.drawFunc();
				}
				x = CurrentMenu.x;
				y = CurrentMenu.y;
				for (i = 0; i < CurrentMenu.itemCount; i++)
				{
					MenuItem_t item = CurrentMenu.items[i];
					if (item.type != ItemType_t.ITT_EMPTY && item.text != "")
					{
						MN_DrTextB(item.text, x, y);
					}
					y += ITEM_HEIGHT;
				}
				y = CurrentMenu.y + (CurrentItPos * ITEM_HEIGHT) + SELECTOR_YOFFSET;
				selName = (MenuTime & 16) != 0 ? "M_SLCTR1" : "M_SLCTR2";
				v_video.V_DrawPatch(x + SELECTOR_XOFFSET, y, w_wad.W_CacheLumpName(selName, DoomDef.PU_CACHE));
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawMainMenu
		//
		//---------------------------------------------------------------------------

		class DrawMainMenu : Menu_t_drawFunc_delegate
		{
			public override void drawFunc()
			{
				int frame;

				frame = (MenuTime / 3) % 18;
				v_video.V_DrawPatch(88, 0, w_wad.W_CacheLumpName("M_HTIC", DoomDef.PU_CACHE));
				v_video.V_DrawPatch(40, 10, w_wad.W_CacheLumpNum(SkullBaseLump + (17 - frame), DoomDef.PU_CACHE));
				v_video.V_DrawPatch(232, 10, w_wad.W_CacheLumpNum(SkullBaseLump + frame, DoomDef.PU_CACHE));
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawEpisodeMenu
		//
		//---------------------------------------------------------------------------

		class DrawEpisodeMenu : Menu_t_drawFunc_delegate
		{
			public override void drawFunc()
			{
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawSkillMenu
		//
		//---------------------------------------------------------------------------

		class DrawSkillMenu : Menu_t_drawFunc_delegate
		{
			public override void drawFunc()
			{
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawFilesMenu
		//
		//---------------------------------------------------------------------------

		class DrawFilesMenu : Menu_t_drawFunc_delegate
		{
			public override void drawFunc()
			{
				// clear out the quicksave/quickload stuff
				quicksave = 0;
				quickload = 0;
				g_game.players[g_game.consoleplayer].message = "";
				g_game.players[g_game.consoleplayer].messageTics = 1;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawLoadMenu
		//
		//---------------------------------------------------------------------------
		class DrawLoadMenu : Menu_t_drawFunc_delegate
		{
			public override void drawFunc()
			{
				MN_DrTextB("LOAD GAME", 160 - MN_TextBWidth("LOAD GAME") / 2, 10);
				if (!slottextloaded)
				{
					MN_LoadSlotText();
				}
				DrawFileSlots(LoadMenu);
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawSaveMenu
		//
		//---------------------------------------------------------------------------

		class DrawSaveMenu : Menu_t_drawFunc_delegate
		{
			public override void drawFunc()
			{
				MN_DrTextB("SAVE GAME", 160 - MN_TextBWidth("SAVE GAME") / 2, 10);
				if (!slottextloaded)
				{
					MN_LoadSlotText();
				}
				DrawFileSlots(SaveMenu);
			}
		}


		//===========================================================================
		//
		// MN_LoadSlotText
		//
		//              Loads in the text message for each slot
		//===========================================================================

		public static void MN_LoadSlotText()
		{
			int count = 0;
			int i;
			string name;

			for (i = 0; i < 6; i++)
			{
				if (d_main.cdrom)
				{
					name = DoomDef.SAVEGAMENAMECD + i + ".hsg";
				}
				else
				{
					name = DoomDef.SAVEGAMENAME + i + ".hsg";
				}

				BinaryReader br;
				try
				{
					br = new BinaryReader(new FileStream(name, FileMode.Open));
					SlotText[i] = "";
					for (int j = 0; j < SLOTTEXTLEN; ++j)
					{
						char c = (char)br.ReadByte();
						if (c == '\0') continue;
						SlotText[i] += c;
					}
					++count;
					br = null;
					SlotStatus[i] = 1;
				}
				catch (Exception)
				{
					SlotText[i] = ""; // empty the string
					SlotStatus[i] = 0;
					continue;
				}
			}
			slottextloaded = true;
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawFileSlots
		//
		//---------------------------------------------------------------------------

		static void DrawFileSlots(Menu_t menu)
		{
			int i;
			int x;
			int y;

			x = menu.x;
			y = menu.y;
			for (i = 0; i < 6; i++)
			{
				v_video.V_DrawPatch(x, y, w_wad.W_CacheLumpName("M_FSLOT", DoomDef.PU_CACHE));
				if (SlotStatus[i] != 0)
				{
					MN_DrTextA(SlotText[i], x + 5, y + 5);
				}
				y += ITEM_HEIGHT;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawOptionsMenu
		//
		//---------------------------------------------------------------------------
		class DrawOptionsMenu : Menu_t_drawFunc_delegate
		{
			public override void drawFunc()
			{
				if (messageson)
				{
					MN_DrTextB("ON", 196, 30);
				}
				else
				{
					MN_DrTextB("OFF", 196, 30);
				}
				DrawSlider(OptionsMenu, 3, 10, Settings.Default.mouse_sensitivity);
				DrawSlider(OptionsMenu, 5, 16, Settings.Default.sfx_volume);
				DrawSlider(OptionsMenu, 7, 16, Settings.Default.music_volume);
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawOptions2Menu
		//
		//---------------------------------------------------------------------------

		class DrawOptions2Menu : Menu_t_drawFunc_delegate
		{
			public override void drawFunc()
			{
				if (Settings.Default.fullscreen)
				{
					MN_DrTextB("ON", 210, 20);
				}
				else
				{
					MN_DrTextB("OFF", 210, 20);
				}
				MN_DrTextB(Settings.Default.resolution.X + " X " + Settings.Default.resolution.Y, 180, 40);
				if (Settings.Default.ambient_enabled)
				{
					MN_DrTextB("ON", 210, 60);
				}
				else
				{
					MN_DrTextB("OFF", 210, 60);
				}
				if (Settings.Default.use_deferred)
				{
					MN_DrTextB("ON", 210, 80);
				}
				else
				{
					MN_DrTextB("OFF", 210, 80);
				}
				if (Settings.Default.postprocess_enabled)
				{
					MN_DrTextB("ON", 210, 100);
				}
				else
				{
					MN_DrTextB("OFF", 210, 100);
				}
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SCNetCheck
		//
		//---------------------------------------------------------------------------

		class SCNetCheck : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				if (!g_game.netgame)
				{ // okay to go into the menu
					return true;
				}
				switch (option)
				{
					case 1:
						p_inter.P_SetMessage(g_game.players[g_game.consoleplayer],
							"YOU CAN'T START A NEW GAME IN NETPLAY!", true);
						break;
					case 2:
						p_inter.P_SetMessage(g_game.players[g_game.consoleplayer],
							"YOU CAN'T LOAD A GAME IN NETPLAY!", true);
						break;
					default:
						break;
				}
				MenuActive = false;
				return false;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SCQuitGame
		//
		//---------------------------------------------------------------------------

		class SCQuitGame : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				MenuActive = false;
				askforquit = true;
				typeofask = 1; //quit game
				if (!g_game.netgame && !g_game.demoplayback)
				{
					g_game.paused = true;
				}
				return true;
			}
		}
		//---------------------------------------------------------------------------
		//
		// PROC SCEndGame
		//
		//---------------------------------------------------------------------------
		class SCEndGame : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				if (g_game.demoplayback || g_game.netgame)
				{
					return false;
				}
				MenuActive = false;
				askforquit = true;
				typeofask = 2; //endgame
				if (!g_game.netgame && !g_game.demoplayback)
				{
					g_game.paused = true;
				}
				return true;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SCMessages
		//
		//---------------------------------------------------------------------------

		class SCMessages : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				messageson = !messageson;
				if (messageson)
				{
					p_inter.P_SetMessage(g_game.players[g_game.consoleplayer], "MESSAGES ON", true);
				}
				else
				{
					p_inter.P_SetMessage(g_game.players[g_game.consoleplayer], "MESSAGES OFF", true);
				}
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_chat);
				return true;
			}
		}

		// Extra stuff
		class SCFullScreen : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				Settings.Default.fullscreen = !Settings.Default.fullscreen;
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_chat);
				if (Game1.instance.graphics.IsFullScreen != Settings.Default.fullscreen)
				{
					Game1.instance.graphics.ToggleFullScreen();
				}
				return true;
			}
		}
		class SCResolution : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				int current = 0;
				foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
				{
					if (mode.Width == Settings.Default.resolution.X &&
						mode.Height == Settings.Default.resolution.Y)
					{
						break;
					}
					++current;
				}
				if (current == GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.Count())
				{
					// start at 0
					current = 0;
				}
				int nextMode = (current + 1) % GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.Count();
				foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
				{
					if (nextMode == 0)
					{
						Settings.Default.resolution = new System.Drawing.Point(mode.Width, mode.Height);
						Game1.instance.graphics.IsFullScreen = Settings.Default.fullscreen;
						Game1.instance.graphics.PreferredBackBufferWidth = Settings.Default.resolution.X;
						Game1.instance.graphics.PreferredBackBufferHeight = Settings.Default.resolution.Y;
						Game1.instance.graphics.ApplyChanges();

						Deferred.FullscreenQuad.updateSettings();
						Deferred.GBuffer.updateSettings();
						Deferred.Effects.updateSettings();
						break;
					}
					--nextMode;
				}
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_chat);
				return true;
			}
		}
		class SCAmbient : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				Settings.Default.ambient_enabled = !Settings.Default.ambient_enabled;
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_chat);

				// Invalidate sectors, they need to be rebuilt
				if (p_setup.sectors != null)
				{
					foreach (r_local.sector_t sec in p_setup.sectors)
					{
						sec.invalidate(false);
					}
				}
				
				// Update renderers
				Standard.Effects.updateSettings();
				Deferred.Effects.updateSettings();

				// Finally, we need to enable the AmbientMap
				AmbientMap.updateSettings();

				return true;
			}
		}
		class SCDeferred : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				Settings.Default.use_deferred = !Settings.Default.use_deferred;
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_chat);

				// Invalidate sectors, they need to be rebuilt
				if (p_setup.sectors != null)
				{
					foreach (r_local.sector_t sec in p_setup.sectors)
					{
						sec.invalidate(false);
					}
				}

				// Update renderers
				Standard.Effects.updateSettings();
				Deferred.Effects.updateSettings();

				return true;
			}
		}
		class SCPostProcess : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				Settings.Default.postprocess_enabled = !Settings.Default.postprocess_enabled;
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_chat);
				return true;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SCLoadGame
		//
		//---------------------------------------------------------------------------
		class SCLoadGame : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
#if DOS
				string name;

		if(!SlotStatus[option])
		{ // slot's empty...don't try and load
			return false;
		}
		if(cdrom)
		{
			sprintf(name, SAVEGAMENAMECD"%d.hsg", option);
		}
		else
		{
			sprintf(name, SAVEGAMENAME"%d.hsg", option);
		}
		G_LoadGame(name);
		MN_DeactivateMenu();
		BorderNeedRefresh = true;
		if(quickload == -1)
		{
			quickload = option+1;
			players[consoleplayer].message = NULL;
			players[consoleplayer].messageTics = 1;
		}
#endif
				return true;
			}
		}


		//---------------------------------------------------------------------------
		//
		// PROC SCSaveGame
		//
		//---------------------------------------------------------------------------

		class SCSaveGame : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
#if DOS
				char* ptr;

				if (!FileMenuKeySteal)
				{
					FileMenuKeySteal = true;
					strcpy(oldSlotText, SlotText[option]);
					ptr = SlotText[option];
					while (*ptr)
					{
						ptr++;
					}
					*ptr = '[';
					*(ptr + 1) = 0;
					SlotStatus[option]++;
					currentSlot = option;
					slotptr = ptr - SlotText[option];
					return false;
				}
				else
				{
					G_SaveGame(option, SlotText[option]);
					FileMenuKeySteal = false;
					MN_DeactivateMenu();
				}
				BorderNeedRefresh = true;
				if (quicksave == -1)
				{
					quicksave = option + 1;
					players[consoleplayer].message = NULL;
					players[consoleplayer].messageTics = 1;
				}
#endif
				return true;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SCEpisode
		//
		//---------------------------------------------------------------------------
		class SCEpisode : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				if (d_main.shareware && option > 1)
				{
					p_inter.P_SetMessage(g_game.players[g_game.consoleplayer],
						"ONLY AVAILABLE IN THE REGISTERED VERSION", true);
				}
				else
				{
					MenuEpisode = option;
					SetMenu(MenuType_t.MENU_SKILL);
				}
				return true;
			}
		}
		//---------------------------------------------------------------------------
		//
		// PROC SCSkill
		//
		//---------------------------------------------------------------------------

		class SCSkill : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				g_game.G_DeferedInitNew((DoomDef.skill_t)option, MenuEpisode, 1); //CHANGE_MAP
				MN_DeactivateMenu();
				return true;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SCMouseSensi
		//
		//---------------------------------------------------------------------------

		class SCMouseSensi : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				if (option == RIGHT_DIR)
				{
					if (Settings.Default.mouse_sensitivity < 9)
					{
						Settings.Default.mouse_sensitivity++;
					}
				}
				else if (Settings.Default.mouse_sensitivity != 0)
				{
					Settings.Default.mouse_sensitivity--;
				}
				return true;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SCSfxVolume
		//
		//---------------------------------------------------------------------------

		class SCSfxVolume : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				if (option == RIGHT_DIR)
				{
					if (Settings.Default.sfx_volume < 15)
					{
						Settings.Default.sfx_volume++;
					}
				}
				else if (Settings.Default.sfx_volume != 0)
				{
					Settings.Default.sfx_volume--;
				}
				i_ibm.S_SetMaxVolume(false); // don't recalc the sound curve, yet
				soundchanged = true; // we'll set it when we leave the menu
				return true;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SCMusicVolume
		//
		//---------------------------------------------------------------------------

		class SCMusicVolume : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				if (option == RIGHT_DIR)
				{
					if (Settings.Default.music_volume < 15)
					{
						Settings.Default.music_volume++;
						if (i_sound.musicAudioOut != null)
							i_sound.musicAudioOut.Volume = (float)Settings.Default.music_volume / 16.0f;
					}
				}
				else if (Settings.Default.music_volume != 0)
				{
					Settings.Default.music_volume--;
					if (i_sound.musicAudioOut != null)
						i_sound.musicAudioOut.Volume = (float)Settings.Default.music_volume / 16.0f;
				}
				i_ibm.S_SetMusicVolume();
				return true;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SCScreenSize
		//
		//---------------------------------------------------------------------------

		class SCScreenSize : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				if (option == RIGHT_DIR)
				{
					if (r_main.screenblocks.val < 11)
					{
						r_main.screenblocks.val++;
					}
				}
				else if (r_main.screenblocks.val > 3)
				{
					r_main.screenblocks.val--;
				}
				r_main.R_SetViewSize(r_main.screenblocks.val, r_main.detailLevel);
				return true;
			}
		}

		//---------------------------------------------------------------------------
		//
		// PROC SCInfo
		//
		//---------------------------------------------------------------------------
		class SCInfo : MenuItem_t_func_delegate
		{
			public override bool func(int option)
			{
				InfoType = 1;
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
				if (!g_game.netgame && !g_game.demoplayback)
				{
					g_game.paused = true;
				}
				return true;
			}
		}

		//---------------------------------------------------------------------------
		//
		// FUNC MN_Responder
		//
		//---------------------------------------------------------------------------

		static bool shiftdown;
		static public bool MN_Responder(DoomDef.event_t ev)
		{
			Keys key;
			int i;
			MenuItem_t item;
			string textBuffer;

			if ((Keys)ev.data1 == Keys.RightShift)
			{
				shiftdown = (ev.type == DoomDef.evtype_t.ev_keydown);
			}
			if (ev.type != DoomDef.evtype_t.ev_keydown)
			{
				return (false);
			}
			key = (Keys)ev.data1;
			if (InfoType != 0)
			{
				if (d_main.shareware)
				{
					InfoType = (InfoType + 1) % 5;
				}
				else
				{
					InfoType = (InfoType + 1) % 4;
				}
				if (key == Keys.Escape)
				{
					InfoType = 0;
				}
				if (InfoType == 0)
				{
					g_game.paused = false;
					MN_DeactivateMenu();
#if DOS
			//TODO:PORT SB
			SB_state = -1; //refresh the statbar
#endif
				}
				i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
				return (true); //make the info screen eat the keypress
			}

			if (d_main.ravpic && key == Keys.F1)
			{
#if DOS
		G_ScreenShot();
#endif
				return (true);
			}

			if (askforquit)
			{
				switch (key)
				{
					case Keys.Y:
						if (askforquit)
						{
							switch (typeofask)
							{
								case 1:
#if DOS
							G_CheckDemoStatus();
#endif
									i_ibm.I_Quit();
									break;
								case 2:
									g_game.players[g_game.consoleplayer].messageTics = 0;
									//set the msg to be cleared
									g_game.players[g_game.consoleplayer].message = "";
									typeofask = 0;
									askforquit = false;
									g_game.paused = false;
#if DOS
							I_SetPalette(w_wad.W_CacheLumpName("PLAYPAL", DoomDef.PU_CACHE));
#endif
									d_main.D_StartTitle(); // go to intro/demo mode.
									break;
								case 3:
									p_inter.P_SetMessage(g_game.players[g_game.consoleplayer], "QUICKSAVING....", false);
									FileMenuKeySteal = true;
#if DOS
							SCSaveGame(quicksave-1);
#endif
									askforquit = false;
									typeofask = 0;
									return true;
								case 4:
									p_inter.P_SetMessage(g_game.players[g_game.consoleplayer], "QUICKLOADING....", false);
#if DOS
							SCLoadGame(quickload-1);
#endif
									askforquit = false;
									typeofask = 0;
									return true;
								default:
									return true; // eat the 'y' keypress
							}
						}
						return false;
					case Keys.N:
					case Keys.Escape:
						if (askforquit)
						{
							g_game.players[g_game.consoleplayer].messageTics = 1; //set the msg to be cleared
							askforquit = false;
							typeofask = 0;
							g_game.paused = false;
							i_ibm.UpdateState |= DoomDef.I_FULLSCRN;
							return true;
						}
						return false;
				}
				return false; // don't let the keys filter thru
			}
			if (MenuActive == false && !ct_chat.chatmodeon)
			{
				switch (key)
				{
					case Keys.OemMinus:
						{
							//TODO:PORT Automap
#if DOS
				if(automapactive)
				{ // Don't screen size in automap
					return(false);
				}
#endif
							SCScreenSize sc = new SCScreenSize();
							sc.func(LEFT_DIR);
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_keyup);
							i_ibm.UpdateState |= DoomDef.I_FULLSCRN;
							return (true);
						}
					case Keys.OemPlus:
						{
							//TODO:PORT Automap
#if DOS
				if(automapactive)
				{ // Don't screen size in automap
					return(false);
				}
#endif
							SCScreenSize sc = new SCScreenSize();
							sc.func(RIGHT_DIR);
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_keyup);
							i_ibm.UpdateState |= DoomDef.I_FULLSCRN;
							return (true);
						}
					case Keys.F1: // help screen
						{
							SCInfo sc = new SCInfo();
							sc.func(0);
						}
						MenuActive = true;
						return (true);
					case Keys.F2: // save game
						if (g_game.gamestate == DoomDef.gamestate_t.GS_LEVEL && !g_game.demoplayback)
						{
							MenuActive = true;
							FileMenuKeySteal = false;
							MenuTime = 0;
							CurrentMenu = SaveMenu;
							CurrentItPos = CurrentMenu.oldItPos;
							if (!g_game.netgame && !g_game.demoplayback)
							{
								g_game.paused = true;
							}
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
							slottextloaded = false; //reload the slot text, when needed
						}
						return true;
					case Keys.F3: // load game
						{
							SCNetCheck sc = new SCNetCheck();
							sc.func(RIGHT_DIR);

							if (sc.func(2))
							{
								MenuActive = true;
								FileMenuKeySteal = false;
								MenuTime = 0;
								CurrentMenu = LoadMenu;
								CurrentItPos = CurrentMenu.oldItPos;
								if (!g_game.netgame && !g_game.demoplayback)
								{
									g_game.paused = true;
								}
								i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
								slottextloaded = false; //reload the slot text, when needed
							}
							return true;
						}
					case Keys.F4: // volume
						MenuActive = true;
						FileMenuKeySteal = false;
						MenuTime = 0;
						CurrentMenu = Options2Menu;
						CurrentItPos = CurrentMenu.oldItPos;
						if (!g_game.netgame && !g_game.demoplayback)
						{
							g_game.paused = true;
						}
						i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
						slottextloaded = false; //reload the slot text, when needed
						return true;
					case Keys.F5: // F5 isn't used in Heretic. (detail level)
						return true;
					case Keys.F6: // quicksave
						if (g_game.gamestate == DoomDef.gamestate_t.GS_LEVEL && !g_game.demoplayback)
						{
							if (quicksave == 0 || quicksave == -1)
							{
								MenuActive = true;
								FileMenuKeySteal = false;
								MenuTime = 0;
								CurrentMenu = SaveMenu;
								CurrentItPos = CurrentMenu.oldItPos;
								if (!g_game.netgame && !g_game.demoplayback)
								{
									g_game.paused = true;
								}
								i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
								slottextloaded = false; //reload the slot text, when needed
								quicksave = -1;
								p_inter.P_SetMessage(g_game.players[g_game.consoleplayer], "CHOOSE A QUICKSAVE SLOT", true);
							}
							else
							{
								askforquit = true;
								typeofask = 3;
								if (!g_game.netgame && !g_game.demoplayback)
								{
									g_game.paused = true;
								}
								i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_chat);
							}
						}
						return true;
					case Keys.F7: // endgame
						if (g_game.gamestate == DoomDef.gamestate_t.GS_LEVEL && !g_game.demoplayback)
						{
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_chat);
							SCEndGame sc = new SCEndGame();
							sc.func(0);
						}
						return true;
					case Keys.F8: // toggle messages
						{
							SCMessages sc = new SCMessages();
							sc.func(0);
							return true;
						}
#if DOS // Added this so it doesn't pop when I start/stop fraps recording
					case Keys.F9: // quickload
						if (quickload == 0 || quickload == -1)
						{
							MenuActive = true;
							FileMenuKeySteal = false;
							MenuTime = 0;
							CurrentMenu = LoadMenu;
							CurrentItPos = CurrentMenu.oldItPos;
							if (!g_game.netgame && !g_game.demoplayback)
							{
								g_game.paused = true;
							}
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
							slottextloaded = false; //reload the slot text, when needed
							quickload = -1;
							p_inter.P_SetMessage(g_game.players[g_game.consoleplayer], "CHOOSE A QUICKLOAD SLOT", true);
						}
						else
						{
							askforquit = true;
							if (!g_game.netgame && !g_game.demoplayback)
							{
								g_game.paused = true;
							}
							typeofask = 4;
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_chat);
						}
						return true;
#endif
					case Keys.F10: // quit
						if (g_game.gamestate == DoomDef.gamestate_t.GS_LEVEL)
						{
							SCQuitGame sc = new SCQuitGame();
							sc.func(0);
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_chat);
						}
						return true;
					case Keys.F11: // F11 - gamma mode correction
						v_video.usegamma.val++;
						if (v_video.usegamma.val > 4)
						{
							v_video.usegamma.val = 0;
						}
						//	I_SetPalette((byte *)W_CacheLumpName("PLAYPAL", PU_CACHE));
						return true;
				}
			}
			if (MenuActive == false)
			{
				if (key == Keys.Escape || g_game.gamestate == DoomDef.gamestate_t.GS_DEMOSCREEN || g_game.demoplayback)
				{
					MN_ActivateMenu();
					return (true);
				}
				return (false);
			}
			if (!FileMenuKeySteal)
			{
				item = CurrentMenu.items[CurrentItPos];
				switch (key)
				{
					case Keys.Down:
					case Keys.S:
						do
						{
							if (CurrentItPos + 1 > CurrentMenu.itemCount - 1)
							{
								CurrentItPos = 0;
							}
							else
							{
								CurrentItPos++;
							}
						} while (CurrentMenu.items[CurrentItPos].type == ItemType_t.ITT_EMPTY);
						i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_switch);
						return (true);
					case Keys.Up:
					case Keys.W:
						do
						{
							if (CurrentItPos == 0)
							{
								CurrentItPos = CurrentMenu.itemCount - 1;
							}
							else
							{
								CurrentItPos--;
							}
						} while (CurrentMenu.items[CurrentItPos].type == ItemType_t.ITT_EMPTY);
						i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_switch);
						return (true);
					case Keys.Left:
					case Keys.A:
						if (item.type == ItemType_t.ITT_LRFUNC && item.func != null)
						{
							item.func.func(LEFT_DIR);
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_keyup);
						}
						return (true);
					case Keys.Right:
					case Keys.F:
						if (item.type == ItemType_t.ITT_LRFUNC && item.func != null)
						{
							item.func.func(RIGHT_DIR);
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_keyup);
						}
						return (true);
					case Keys.Enter:
						if (item.type == ItemType_t.ITT_SETMENU)
						{
							SetMenu(item.menu);
						}
						else if (item.func != null)
						{
							CurrentMenu.oldItPos = CurrentItPos;
							if (item.type == ItemType_t.ITT_LRFUNC)
							{
								item.func.func(RIGHT_DIR);
							}
							else if (item.type == ItemType_t.ITT_EFUNC)
							{
								if (item.func.func(item.option))
								{
									if (item.menu != MenuType_t.MENU_NONE)
									{
										SetMenu(item.menu);
									}
								}
							}
						}
						i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
						return (true);
					case Keys.Back:
					case Keys.Escape:
						if (CurrentMenu == MainMenu)
						{
							MN_DeactivateMenu();
							return (true);
						}
						else
						{
							if (CurrentMenu == OptionsMenu) Settings.Default.Save();
							i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_switch);
							if (CurrentMenu.prevMenu == MenuType_t.MENU_NONE)
							{
								MN_DeactivateMenu();
							}
							else
							{
								SetMenu(CurrentMenu.prevMenu);
							}
							return (true);
						}
					default:
						for (i = 0; i < CurrentMenu.itemCount; i++)
						{
							if (CurrentMenu.items[i].text != "")
							{
								if ((int)key - (int)Keys.A + (int)'A' ==
									(int)char.ToUpper(CurrentMenu.items[i].text[0]))
								{
									CurrentItPos = i;
									return (true);
								}
							}
						}
						break;
				}
				return (false);
			}
			else
			{ // Editing file names
				textBuffer = SlotText[currentSlot]; //slotptr
				if (key == Keys.Back)
				{
					if (slotptr > 0)
					{
						textBuffer = textBuffer.Substring(0, textBuffer.Length - 1);
						slotptr--;
					}
					return (true);
				}
				if (key == Keys.Escape)
				{
					SlotText[currentSlot] = oldSlotText;
					SlotStatus[currentSlot]--;
					MN_DeactivateMenu();
					return (true);
				}
				if (key == Keys.Enter)
				{
					item = CurrentMenu.items[CurrentItPos];
					CurrentMenu.oldItPos = CurrentItPos;
					if (item.type == ItemType_t.ITT_EFUNC)
					{
						item.func.func(item.option);
						if (item.menu != MenuType_t.MENU_NONE)
						{
							SetMenu(item.menu);
						}
					}
					return (true);
				}
				if (slotptr < SLOTTEXTLEN && key != Keys.Back)
				{
					if ((key >= Keys.A && key <= Keys.Z))
					{
						textBuffer += (char)((int)'A' + ((int)key - (int)Keys.A));
						slotptr++;
						return (true);
					}
					if ((key >= Keys.D0 && key <= Keys.D9))
					{
						textBuffer += (char)((int)'0' + ((int)key - (int)Keys.D0));
						slotptr++;
						return (true);
					}
					if (key == Keys.Space)
					{
						textBuffer += ' ';
						slotptr++;
						return (true);
					}
					if (key == Keys.OemComma)
					{
						textBuffer += ',';
						slotptr++;
						return (true);
					}
					if (key == Keys.OemPeriod)
					{
						textBuffer += '.';
						slotptr++;
						return (true);
					}
					if (key == Keys.OemMinus)
					{
						textBuffer += '-';
						slotptr++;
						return (true);
					}
					if (shiftdown && key == Keys.D1)
					{
						textBuffer += '!';
						slotptr++;
						return (true);
					}
				}
				return (true);
			}
		//	return (false); // [dsl] Unreachable?
		}

		//---------------------------------------------------------------------------
		//
		// PROC MN_ActivateMenu
		//
		//---------------------------------------------------------------------------

		public static void MN_ActivateMenu()
		{
			if (MenuActive)
			{
				return;
			}
			if (g_game.paused)
			{
				i_ibm.S_ResumeSound();
			}
			MenuActive = true;
			FileMenuKeySteal = false;
			MenuTime = 0;
			CurrentMenu = MainMenu;
			CurrentItPos = CurrentMenu.oldItPos;
			if (!g_game.netgame && !g_game.demoplayback)
			{
				g_game.paused = true;
			}
			i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
			slottextloaded = false; //reload the slot text, when needed
		}

		//---------------------------------------------------------------------------
		//
		// PROC MN_DeactivateMenu
		//
		//---------------------------------------------------------------------------

		public static void MN_DeactivateMenu()
		{
			CurrentMenu.oldItPos = CurrentItPos;
			MenuActive = false;
			if (!g_game.netgame)
			{
				g_game.paused = false;
			}
			i_ibm.S_StartSound(null, (int)sounds.sfxenum_t.sfx_dorcls);
			if (soundchanged)
			{
				i_ibm.S_SetMaxVolume(true); //recalc the sound curve
				soundchanged = false;
			}
			g_game.players[g_game.consoleplayer].message = "";
			g_game.players[g_game.consoleplayer].messageTics = 1;
		}

		//---------------------------------------------------------------------------
		//
		// PROC MN_DrawInfo
		//
		//---------------------------------------------------------------------------

		public static void MN_DrawInfo()
		{
			v_video.V_DrawRawScreen(w_wad.W_CacheLumpNum(w_wad.W_GetNumForName("TITLE") + InfoType, DoomDef.PU_CACHE));
		}

		//---------------------------------------------------------------------------
		//
		// PROC SetMenu
		//
		//---------------------------------------------------------------------------

		static void SetMenu(MenuType_t menu)
		{
			CurrentMenu.oldItPos = CurrentItPos;
			CurrentMenu = Menus[(int)menu];
			CurrentItPos = CurrentMenu.oldItPos;
		}

		//---------------------------------------------------------------------------
		//
		// PROC DrawSlider
		//
		//---------------------------------------------------------------------------

		static void DrawSlider(Menu_t menu, int item, int width, int slot)
		{
			int x;
			int y;
			int x2;
			int count;

			x = menu.x + 24;
			y = menu.y + 2 + (item * ITEM_HEIGHT);
			v_video.V_DrawPatch(x - 32, y, w_wad.W_CacheLumpName("M_SLDLT", DoomDef.PU_CACHE));
			for (x2 = x, count = width; count-- != 0; x2 += 8)
			{
				v_video.V_DrawPatch(x2, y, w_wad.W_CacheLumpName((count & 1) != 0 ? "M_SLDMD1"
					: "M_SLDMD2", DoomDef.PU_CACHE));
			}
			v_video.V_DrawPatch(x2, y, w_wad.W_CacheLumpName("M_SLDRT", DoomDef.PU_CACHE));
			v_video.V_DrawPatch(x + 4 + slot * 8, y + 7, w_wad.W_CacheLumpName("M_SLDKB", DoomDef.PU_CACHE));
		}

	}
}
