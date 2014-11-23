using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.IO;

namespace HereticXNA
{
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		public bool showTextures = true;
		public bool useFreeCam = false;
		public bool musicEnabled = true;
		public bool wireframe = false;
		public bool showSectorUpdate = false;
		public bool showGBuffer = false;
		public bool forcedPause = false;
		public bool showDebugHDR = false;
		public bool usePostProcess = true;

		public class Floorlightinfo
		{
			public float distance = 256;
			public Vector4 color;
			public int saturation = 50;
			public int hue;
			public float brightness = 1.0f;
			public float spread = .2f;

			public void makeColor()
			{
				double r, g, b;
				double s = (double)saturation / 100.0;
				double h = (double)hue;
				double v = (double)brightness;

				int i;
				double f, p, q, t;
				if (s == 0)
				{
					r = g = b = v;
				}
				else
				{
					h /= 60.0;			// sector 0 to 5
					i = (int)h;
					f = h - (double)i;			// factorial part of h
					p = v * (1 - s);
					q = v * (1 - s * f);
					t = v * (1 - s * (1 - f));
					switch (i)
					{
						case 0:
							r = v;
							g = t;
							b = p;
							break;
						case 1:
							r = q;
							g = v;
							b = p;
							break;
						case 2:
							r = p;
							g = v;
							b = t;
							break;
						case 3:
							r = p;
							g = q;
							b = v;
							break;
						case 4:
							r = t;
							g = p;
							b = v;
							break;
						default:		// case 5:
							r = v;
							g = p;
							b = q;
							break;
					}
				}

				color.X = (float)r;
				color.Y = (float)g;
				color.Z = (float)b;
				color.W = 1;
			}
		}

		public List<Texture2D> allTextures = new List<Texture2D>();
		public List<Texture2D> spriteTextures = new List<Texture2D>();
		public List<Texture2D> wallTextures = new List<Texture2D>();
		public List<Texture2D> floorTextures = new List<Texture2D>();
		public Dictionary<int, Texture2D> floorTexturesById = new Dictionary<int, Texture2D>();
		public Dictionary<int, Texture2D> wallTexturesById = new Dictionary<int, Texture2D>();
		public Dictionary<string, Texture2D> patchTextures = new Dictionary<string, Texture2D>();
		public Dictionary<int, float> flatSelfIllumById = new Dictionary<int, float>();
		public Dictionary<int, Floorlightinfo> flatLightInfoById = new Dictionary<int, Floorlightinfo>();
		public Dictionary<int, Floorlightinfo> flatCLightInfoById = new Dictionary<int, Floorlightinfo>();
		public GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch;
		public static Random random = new Random();
		string[] args;
		public static Game1 instance;
		public int textureViewerOffset = 0;
		public int mouseWheel = Mouse.GetState().ScrollWheelValue;
		public Texture2D texBlank;
		public SpriteFont font;
		public MouseState oldMouseState = Mouse.GetState();
		public AudioListener audioListener = new AudioListener();
		public Matrix view = Matrix.Identity;
		public Matrix proj = Matrix.Identity;
		public Effect fxWall;
		public Effect fxSprite;
		public Effect fxSky;
		public Effect fxPlane;
		public Effect fxPlaneAmbient;
		public Effect fxDeferred;
		public Effect fxSectorLightInfo;
		public Effect fxHDR;
		public Effect fxSelectedLines;
		public Effect fxShadowPass;
		public float angleX = 0;
		public float angleZ = 0;
		public Vector3 pos = Vector3.Zero;
		public float dt;
		public Matrix freeCam = Matrix.Identity;
		public Vector3 camRight = Vector3.UnitX;
		public Vector3 camPos = Vector3.Zero;
		public Texture2D crosshair;
		public RenderTarget2D frameBuffer;
		public RenderTarget2D frameBuffer2;
		public RenderTarget2D lastFrameLevels;
		public RenderTarget2D bloom;
		public RenderTarget2D bloom2;

		public WeakReference selectedMob = new WeakReference(null);
		public Vector3 selectedPos = Vector3.Zero;
		public DoomDef.mobj_t mouseHoverMob = null;
		private bool m_needSave = false;
		public bool NeedSave
		{
			get { return m_needSave; }
			set
			{
				m_needSave = value;
				System.Windows.Forms.Control mainForm = System.Windows.Forms.Control.FromHandle(Window.Handle);
				if (m_needSave)
				{
					mainForm.Text = "HereticXNA*";
				}
				else
				{
					mainForm.Text = "HereticXNA";
				}
			}
		}

		// Editor windows
		public FrmMobj frmMobj;
		public FrmSector frmSector;
		public FrmFloor frmFloor;
		public FrmGlobalAmbient frmGlobalAmbient;

		public Game1(string[] in_args)
		{
			instance = this;
			args = in_args;
			graphics = new GraphicsDeviceManager(this);
			IsMouseVisible = true;

			graphics.PreferredBackBufferWidth = Settings.Default.resolution.X;
			graphics.PreferredBackBufferHeight = Settings.Default.resolution.Y;
			graphics.IsFullScreen = Settings.Default.fullscreen;

			// Not sure that's a good idea, but it fixes the culling
			//DoomDef.SCREENWIDTH = graphics.PreferredBackBufferWidth = 1024;
			//DoomDef.SCREENHEIGHT = graphics.PreferredBackBufferHeight = 768;
			//r_local.CENTERY = (DoomDef.SCREENHEIGHT / 2);
			//r_local.MAXOPENINGS = DoomDef.SCREENWIDTH * 64;

			Content.RootDirectory = "Content";
			this.TargetElapsedTime = TimeSpan.FromSeconds(1.0f / (float)DoomDef.TICSPERSEC);
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			// Init standard rendering classes
			Standard.Renderer.init(GraphicsDevice);

			// Init deferred rendering classes
			Deferred.Renderer.init(GraphicsDevice);

			// Init ambient map
			AmbientMap.init(GraphicsDevice);

			frameBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.HdrBlendable, DepthFormat.None);
			frameBuffer2 = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, true, SurfaceFormat.Color, DepthFormat.None);
			lastFrameLevels = new RenderTarget2D(GraphicsDevice, 4, 4);
			bloom = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height / 4);
			bloom2 = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height / 4);

			//--- Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			//--- Needed resources
			texBlank = new Texture2D(GraphicsDevice, 1, 1);
			texBlank.SetData(new Color[] { Color.White });
			crosshair = Content.Load<Texture2D>("crosshair");
			font = Content.Load<SpriteFont>("SpriteFont1");
			fxWall = Content.Load<Effect>("fxWall");
			fxSprite = Content.Load<Effect>("fxSprite");
			fxSky = Content.Load<Effect>("fxSky");
			fxPlane = Content.Load<Effect>("fxPlane");
			fxPlaneAmbient = Content.Load<Effect>("fxPlaneAmbient");
			fxDeferred = Content.Load<Effect>("fxDeferred");
			fxSectorLightInfo = Content.Load<Effect>("fxSectorLightInfo");
			fxHDR = Content.Load<Effect>("fxHDR");
			fxSelectedLines = Content.Load<Effect>("fxSelectedLines");
			fxShadowPass = Content.Load<Effect>("fxShadowPass");

			r_main.scalelight = new int[r_local.LIGHTLEVELS][];
			for (int i = 0; i < r_local.LIGHTLEVELS; ++i)
			{
				r_main.scalelight[i] = new int[r_local.MAXLIGHTSCALE];
			}

			loadGlobalSurfaceInfo();

			i_ibm.main(args);

			info.loadMobType();
		}

		protected override void UnloadContent()
		{
		}

		public bool intersectTriangle(
			Vector3 p1,
			Vector3 p2,
			Vector3 p3,
			ref Vector3 rayNear,
			ref Vector3 rayFar,
			ref Vector3 intersectPoint)
		{
			Vector3 normal = Vector3.Cross(p1 - p2, p3 - p2);
			normal.Normalize();
			float d = Vector3.Dot(normal, p1);

			float d1 = Vector3.Dot(normal, rayNear) - d;
			if (d1 < 0) return false;
			float d2 = Vector3.Dot(normal, rayFar) - d;
			if (d2 >= 0) return false;

			float percent = d1 / (d1 - d2);
			intersectPoint = rayNear + (rayFar - rayNear) * percent;

			if (Vector3.Dot(Vector3.Cross(p2 - p1, normal), intersectPoint - p1) < 0) return false;
			if (Vector3.Dot(Vector3.Cross(p3 - p2, normal), intersectPoint - p2) < 0) return false;
			if (Vector3.Dot(Vector3.Cross(p1 - p3, normal), intersectPoint - p3) < 0) return false;

			return true;
		}

		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (/*Keyboard.GetState().IsKeyDown(Keys.F12) ||*/
				GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Back))
				this.Exit();

			dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			for (DoomDef.thinker_t think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
			{
				if (think == null) break;
				if (think.function == null) continue;
				DoomDef.mobj_t mo = think.function.obj as DoomDef.mobj_t;
				if (mo == null) continue;
				++mo.rnd;
			}

			d_main.D_Update();

#if DEBUG
			if (useFreeCam && !i_ibm.keyboardState.IsKeyDown(Keys.LeftAlt))
			{
				if (i_ibm.mouseState.MiddleButton == ButtonState.Pressed)
				{
					IsMouseVisible = false;
					angleZ -= (float)i_ibm.mouseDelta.X * .03f * dt * MathHelper.Pi;
					angleX -= (float)i_ibm.mouseDelta.Y * .03f * dt * MathHelper.Pi;

					angleZ = MathHelper.WrapAngle(angleZ);
					angleX = MathHelper.Clamp(angleX, -MathHelper.PiOver2 + .01f, MathHelper.PiOver2 * .65f);
					freeCam = Matrix.Identity;
					freeCam *= Matrix.CreateRotationX(angleX);
					freeCam *= Matrix.CreateRotationZ(angleZ);
					freeCam.Translation = pos;
					Vector3 right = freeCam.Right;
					right.Z = 0;
					right.Normalize();
					Vector3 front = freeCam.Up;
					front.Z = 0;
					front.Normalize();
					if (Keyboard.GetState().IsKeyDown(Keys.D))
						pos += right * dt * 512.0f;
					if (Keyboard.GetState().IsKeyDown(Keys.A))
						pos -= right * dt * 512.0f;
					if (Keyboard.GetState().IsKeyDown(Keys.W))
						pos += front * dt * 512.0f;
					if (Keyboard.GetState().IsKeyDown(Keys.S))
						pos -= front * dt * 512.0f;
					if (Keyboard.GetState().IsKeyDown(Keys.Space))
						pos.Z += dt * 512.0f;
					if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
						pos.Z -= dt * 512.0f;
				}
				else
				{
					IsMouseVisible = true;

					// Find the mouse over sector or entity
					Vector3 near = new Vector3((float)i_ibm.mouseState.X, (float)i_ibm.mouseState.Y, 0);
					Vector3 far = new Vector3((float)i_ibm.mouseState.X, (float)i_ibm.mouseState.Y, 1);
					Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(near, proj, view, Matrix.Identity);
					Vector3 farPoint = GraphicsDevice.Viewport.Unproject(far, proj, view, Matrix.Identity);
					Vector3 direction = farPoint - nearPoint;
					direction.Normalize();
					Ray pickRay = new Ray(nearPoint, direction);

					// Check for mobs first
					int lump;
					r_local.spritedef_t sprdef;
					r_local.spriteframe_t sprframe;
					float closest = 10000;
					mouseHoverMob = null;
					Vector3 hoverPos = Vector3.Zero;
					for (DoomDef.thinker_t think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
					{
						if (think == null) break;
						if (think.function == null) continue;
						DoomDef.mobj_t mo = think.function.obj as DoomDef.mobj_t;
						if (mo == null) continue;
						info.mobjinfo_t mobT = info.mobjinfo[(int)mo.type];
						sprdef = r_thing.sprites[(int)mo.sprite];
						sprframe = sprdef.spriteframes[mo.frame & DoomDef.FF_FRAMEMASK];
						lump = sprframe.lump[0];
						Texture2D texture = w_wad.W_CacheLumpNum(lump + r_data.firstspritelump, DoomDef.PU_CACHE).cache as Texture2D;
						Vector3 mid = new Vector3(
							(mo.x >> DoomDef.FRACBITS),
							(mo.y >> DoomDef.FRACBITS),
							((mo.z + r_data.spritetopoffset[lump]) >> DoomDef.FRACBITS) - texture.Height / 2
							);
						BoundingSphere bs = new BoundingSphere(mid, 16.0f);
						float? dis = bs.Intersects(pickRay);
						if (dis != null)
						{
							if (dis < closest)
							{
								closest = dis.Value;
								hoverPos = mid;
								mouseHoverMob = mo;
							}
						}
					}

					if (i_ibm.mouseState.LeftButton == ButtonState.Pressed &&
						i_ibm.oldMouseState.LeftButton == ButtonState.Released &&
						new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height).Contains(new Point(i_ibm.mouseState.X, i_ibm.mouseState.Y)))
					{
						// Find closest surface
						Vector3 intersectPoint = Vector3.Zero;
						foreach (r_local.sector_t sec in p_setup.sectors)
						{
							if (sec.floorBatch == null) continue;

							if (sec.floorBatch.vertsPNCT.Count > 0)
							{
								// Floor
								float dis = 0;
								for (int i = 0; i < sec.floorBatch.vertsPNCT.Count; i += 3)
								{
									if (intersectTriangle(sec.floorBatch.vertsPNCT[i + 0].Position, sec.floorBatch.vertsPNCT[i + 1].Position, sec.floorBatch.vertsPNCT[i + 2].Position,
										ref nearPoint, ref farPoint, ref intersectPoint))
									{
										dis = Vector3.Distance(intersectPoint, nearPoint);
										if (dis < closest)
										{
											closest = dis;
											mouseHoverMob = null;
											mouseHoverSector = sec;
											break;
										}
									}
								}

								// Ceiling
								for (int i = 0; i < sec.ceilingBatch.vertsPNCT.Count; i += 3)
								{
									if (intersectTriangle(sec.ceilingBatch.vertsPNCT[i + 0].Position, sec.ceilingBatch.vertsPNCT[i + 1].Position, sec.ceilingBatch.vertsPNCT[i + 2].Position,
										ref nearPoint, ref farPoint, ref intersectPoint))
									{
										dis = Vector3.Distance(intersectPoint, nearPoint);
										if (dis < closest)
										{
											closest = dis;
											mouseHoverMob = null;
											mouseHoverSector = sec;
											break;
										}
									}
								}
							}
							else if (sec.floorBatch.vertsPCT.Count > 0)
							{
								// Floor
								float dis = 0;
								for (int i = 0; i < sec.floorBatch.vertsPCT.Count; i += 3)
								{
									if (intersectTriangle(sec.floorBatch.vertsPCT[i + 0].Position, sec.floorBatch.vertsPCT[i + 1].Position, sec.floorBatch.vertsPCT[i + 2].Position,
										ref nearPoint, ref farPoint, ref intersectPoint))
									{
										dis = Vector3.Distance(intersectPoint, nearPoint);
										if (dis < closest)
										{
											closest = dis;
											mouseHoverMob = null;
											mouseHoverSector = sec;
											break;
										}
									}
								}

								// Ceiling
								for (int i = 0; i < sec.ceilingBatch.vertsPCT.Count; i += 3)
								{
									if (intersectTriangle(sec.ceilingBatch.vertsPCT[i + 0].Position, sec.ceilingBatch.vertsPCT[i + 1].Position, sec.ceilingBatch.vertsPCT[i + 2].Position,
										ref nearPoint, ref farPoint, ref intersectPoint))
									{
										dis = Vector3.Distance(intersectPoint, nearPoint);
										if (dis < closest)
										{
											closest = dis;
											mouseHoverMob = null;
											mouseHoverSector = sec;
											break;
										}
									}
								}
							}
							else if (sec.floorBatch.vertsPT.Count > 0)
							{
								// Floor
								float dis = 0;
								for (int i = 0; i < sec.floorBatch.vertsPT.Count; i += 3)
								{
									if (intersectTriangle(sec.floorBatch.vertsPT[i + 0].Position, sec.floorBatch.vertsPT[i + 1].Position, sec.floorBatch.vertsPT[i + 2].Position,
										ref nearPoint, ref farPoint, ref intersectPoint))
									{
										dis = Vector3.Distance(intersectPoint, nearPoint);
										if (dis < closest)
										{
											closest = dis;
											mouseHoverMob = null;
											mouseHoverSector = sec;
											break;
										}
									}
								}

								// Ceiling
								for (int i = 0; i < sec.ceilingBatch.vertsPT.Count; i += 3)
								{
									if (intersectTriangle(sec.ceilingBatch.vertsPT[i + 0].Position, sec.ceilingBatch.vertsPT[i + 1].Position, sec.ceilingBatch.vertsPT[i + 2].Position,
										ref nearPoint, ref farPoint, ref intersectPoint))
									{
										dis = Vector3.Distance(intersectPoint, nearPoint);
										if (dis < closest)
										{
											closest = dis;
											mouseHoverMob = null;
											mouseHoverSector = sec;
											break;
										}
									}
								}
							}
						}

						if (mouseHoverMob != null)
						{
							selectedSector = null;
							selectedMob = new WeakReference(mouseHoverMob);
							selectedPos = hoverPos;
							DoomDef.mobj_t mo = selectedMob.Target as DoomDef.mobj_t;
							frmMobj.fillUIWithMob(mo);
							if (mo.infol.light != null &&
								mo.subsector != null &&
								mo.sectorsInRadius == null)
							{
								mo.sectorsInRadius = new List<r_local.sector_t>();
								p_maputl.GatherSectorsInRadius(
									new Vector2(
										mo.x >> DoomDef.FRACBITS,
										mo.y >> DoomDef.FRACBITS),
									mo.subsector.sector,
									mo.infol.light.radius,
									ref mo.sectorsInRadius);
							}
						}
						else if (mouseHoverSector != null)
						{
							selectedMob.Target = null;
							selectedSector = mouseHoverSector;
							frmSector.fillUIWithSector(selectedSector);
							frmFloor.fillUIWithSector(selectedSector);
						}
					}
				}
			}

			if (i_ibm.keyboardState.IsKeyDown(Keys.LeftControl) &&
				i_ibm.mouseState.MiddleButton != ButtonState.Pressed)
			{
				if (i_ibm.keyboardState.IsKeyDown(Keys.S) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.S) &&
					NeedSave)
				{
					NeedSave = false;
					// Save mobtype stuff
					info.saveMobType();
					// Save global surfaces info
					saveGlobalSurfaceInfo();
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.D) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.D))
				{
					Settings.Default.use_deferred = !Settings.Default.use_deferred;
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.G) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.G))
				{
					showGBuffer = !showGBuffer;
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.T) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.T))
				{
					showTextures = !showTextures;
					Standard.Effects.updateSettings();
					Deferred.Effects.updateSettings();
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.R) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.R))
				{
					Settings.Default.ambient_enabled = !Settings.Default.ambient_enabled;
					foreach (r_local.sector_t sec in p_setup.sectors)
					{
						sec.invalidate(false);
					}
					Standard.Effects.updateSettings();
					Deferred.Effects.updateSettings();
					AmbientMap.updateSettings();
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.H) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.H))
				{
					showDebugHDR = !showDebugHDR;
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.P) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.P))
				{
					usePostProcess = !usePostProcess;
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.F) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.F))
				{
					useFreeCam = !useFreeCam;
					if (useFreeCam)
					{
						System.Windows.Forms.Control mainForm = System.Windows.Forms.Control.FromHandle(Window.Handle);

						if (frmSector == null)
						{
							frmSector = new FrmSector();
							frmSector.Location = new System.Drawing.Point(mainForm.Left - 10 - frmSector.Width, mainForm.Top);
							frmSector.Show(mainForm);
						}

						if (frmFloor == null)
						{
							frmFloor = new FrmFloor();
							frmFloor.Location = new System.Drawing.Point(frmSector.Left, frmSector.Bottom + 10);
							frmFloor.Show(mainForm);
						}

						if (frmGlobalAmbient == null)
						{
							frmGlobalAmbient = new FrmGlobalAmbient();
							frmGlobalAmbient.Location = new System.Drawing.Point(mainForm.Right + 10, mainForm.Top);
							frmGlobalAmbient.Show(mainForm);
						}

						if (frmMobj == null)
						{
							frmMobj = new FrmMobj();
							frmMobj.Location = new System.Drawing.Point(frmGlobalAmbient.Left, frmGlobalAmbient.Bottom + 10);
							frmMobj.Show(mainForm);
						}

					}
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.M) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.M))
				{
					musicEnabled = !musicEnabled;
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.W) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.W))
				{
					wireframe = !wireframe;
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.NumPad8) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.NumPad8))
				{
					r_segs.ambientSize *= 2;
					Standard.Effects.updateSettings();
					Deferred.Effects.updateSettings();
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.NumPad2) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.NumPad2))
				{
					r_segs.ambientSize /= 2;
					Standard.Effects.updateSettings();
					Deferred.Effects.updateSettings();
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.NumPad9) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.NumPad9))
				{
					r_segs.ambientAmount += .1f;
					Standard.Effects.updateSettings();
					Deferred.Effects.updateSettings();
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.NumPad3) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.NumPad3))
				{
					r_segs.ambientAmount -= .1f;
					Standard.Effects.updateSettings();
					Deferred.Effects.updateSettings();
				}
				if (i_ibm.keyboardState.IsKeyDown(Keys.K) &&
					i_ibm.oldKeyboardState.IsKeyUp(Keys.K))
				{
					sb_bar.Cheats[0].func.func(g_game.players[g_game.consoleplayer], sb_bar.Cheats[0]);
					sb_bar.Cheats[2].func.func(g_game.players[g_game.consoleplayer], sb_bar.Cheats[2]);
					sb_bar.Cheats[5].func.func(g_game.players[g_game.consoleplayer], sb_bar.Cheats[5]);
					sb_bar.Cheats[13].func.func(g_game.players[g_game.consoleplayer], sb_bar.Cheats[13]);
				}
			}

			if (i_ibm.keyboardState.IsKeyDown(Keys.Space) &&
				i_ibm.oldKeyboardState.IsKeyUp(Keys.Space) &&
				i_ibm.mouseState.MiddleButton != ButtonState.Pressed)
			{
				forcedPause = !forcedPause;
			}

			textureViewerOffset = mouseWheel - Mouse.GetState().ScrollWheelValue;
			if (i_ibm.oldKeyboardState.IsKeyUp(Keys.P) &&
				i_ibm.keyboardState.IsKeyDown(Keys.P))
			{
				targetSector += i_ibm.keyboardState.IsKeyDown(Keys.RightShift)?10:1;
				if (targetSector >= p_setup.sectors.Length) targetSector = 0;
			}
			if (i_ibm.oldKeyboardState.IsKeyUp(Keys.O) &&
				i_ibm.keyboardState.IsKeyDown(Keys.O))
			{
				targetSector -= i_ibm.keyboardState.IsKeyDown(Keys.RightShift)?10:1;
				if (targetSector < 0) targetSector = p_setup.sectors.Length - 1;
			}

			SoundEffect.MasterVolume = 1;
			int y = graphics.PreferredBackBufferHeight;
			bool isMouseDown = false;
			if (oldMouseState.LeftButton == ButtonState.Released &&
				Mouse.GetState().LeftButton == ButtonState.Pressed)
			{
				isMouseDown = true;
			}
		/*	foreach (sounds.sfxinfo_t sfx in sounds.S_sfx)
			{
				if (sfx.snd_ptr == null) continue;
				if (isMouseDown)
				{
					if (Mouse.GetState().X < 100 &&
						Mouse.GetState().Y >= y - textureViewerOffset &&
						Mouse.GetState().Y < y - textureViewerOffset + 32)
					{
						sfx.snd_ptr.Play();
					}
				}
				y += 32;
			}*/
			oldMouseState = Mouse.GetState();

			if (selectedMob.Target != null)
			{
				DoomDef.mobj_t selectedMo = selectedMob.Target as DoomDef.mobj_t;
				bool doIt = true;
				int speed = 1;
				if (i_ibm.keyboardState.IsKeyDown(Keys.LeftShift)) speed = 5;
				if (i_ibm.keyboardState.IsKeyDown(Keys.Left)) selectedMo.x += -speed << 16;
				else if (i_ibm.keyboardState.IsKeyDown(Keys.Right)) selectedMo.x += speed << 16;
				else if (i_ibm.keyboardState.IsKeyDown(Keys.Up)) selectedMo.y += speed << 16;
				else if (i_ibm.keyboardState.IsKeyDown(Keys.Down)) selectedMo.y += -speed << 16;
				else if (i_ibm.keyboardState.IsKeyDown(Keys.OemPlus)) selectedMo.z += speed << 16;
				else if (i_ibm.keyboardState.IsKeyDown(Keys.OemMinus)) selectedMo.z += -speed << 16;
				else doIt = false;
				if (doIt)
				{
					selectedMo.infol.flags |= DoomDef.MF_NOGRAVITY;
					for (DoomDef.thinker_t think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
					{
						if (think == null) break;
						if (think.function == null) continue;
						DoomDef.mobj_t mo = think.function.obj as DoomDef.mobj_t;
						if (mo == null) continue;
						if (mo.type != selectedMo.type) continue;
						if (mo.shadowInfo == null) continue;
						mo.shadowInfo.needUpdate = true;
					}
				}
			}
			else
			{
				if (i_ibm.keyboardState.IsKeyDown(Keys.Left))
					map_camera.X += dt * 1024.0f / map_zoom;
				if (i_ibm.keyboardState.IsKeyDown(Keys.Right))
					map_camera.X -= dt * 1024.0f / map_zoom;
				if (i_ibm.keyboardState.IsKeyDown(Keys.Up))
					map_camera.Y += dt * 1024.0f / map_zoom;
				if (i_ibm.keyboardState.IsKeyDown(Keys.Down))
					map_camera.Y -= dt * 1024.0f / map_zoom;
				if (i_ibm.keyboardState.IsKeyDown(Keys.OemPlus))
					map_zoom += 1 * dt;
				if (i_ibm.keyboardState.IsKeyDown(Keys.OemMinus))
				{
					map_zoom -= 1 * dt;
					if (map_zoom <= 1) map_zoom = 1;
				}
			}
#endif

			base.Update(gameTime);
		}

		const int CHUNK_SELF_ILLUM_FLAT1 = 1;
		const int CHUNK_LIGHT_INFO_FLAT1 = 2;
		const int CHUNK_LIGHT_INFO_FLATC1 = 4;

		private void saveGlobalSurfaceInfo()
		{
			FileStream fs = new FileStream("../../../../HereticXNAContent/surfaces.bin", FileMode.Create);
			BinaryWriter bw = new BinaryWriter(fs);

			foreach (KeyValuePair<int, float> selfIllum in flatSelfIllumById)
			{
				bw.Write(CHUNK_SELF_ILLUM_FLAT1);
				bw.Write(selfIllum.Key);
				bw.Write(selfIllum.Value);
			}
			foreach (KeyValuePair<int, Floorlightinfo> lightInfo in flatLightInfoById)
			{
				bw.Write(CHUNK_LIGHT_INFO_FLAT1);
				bw.Write(lightInfo.Key);
				bw.Write(lightInfo.Value.distance);
				bw.Write(lightInfo.Value.saturation);
				bw.Write(lightInfo.Value.hue);
				bw.Write(lightInfo.Value.brightness);
				bw.Write(lightInfo.Value.spread);
			}
			foreach (KeyValuePair<int, Floorlightinfo> lightInfo in flatCLightInfoById)
			{
				bw.Write(CHUNK_LIGHT_INFO_FLATC1);
				bw.Write(lightInfo.Key);
				bw.Write(lightInfo.Value.distance);
				bw.Write(lightInfo.Value.saturation);
				bw.Write(lightInfo.Value.hue);
				bw.Write(lightInfo.Value.brightness);
				bw.Write(lightInfo.Value.spread);
			}

			fs.Flush();
		}

		private void loadGlobalSurfaceInfo()
		{
			BinaryReader br = new BinaryReader(new FileStream("Content/surfaces.bin", FileMode.Open));
			while (br.BaseStream.Position < br.BaseStream.Length)
			{
				int chunk = br.ReadInt32();
				switch (chunk)
				{
					case CHUNK_SELF_ILLUM_FLAT1:
					{
						int key = br.ReadInt32();
						flatSelfIllumById[key] = br.ReadSingle();
						break;
					}
					case CHUNK_LIGHT_INFO_FLAT1:
					{
						int key = br.ReadInt32();
						Floorlightinfo lightInfo = new Floorlightinfo();
						lightInfo.distance = br.ReadSingle();
						lightInfo.saturation = br.ReadInt32();
						lightInfo.hue = br.ReadInt32();
						lightInfo.brightness = br.ReadSingle();
						lightInfo.spread = br.ReadSingle();
						flatLightInfoById[key] = lightInfo;
						break;
					}
					case CHUNK_LIGHT_INFO_FLATC1:
					{
						int key = br.ReadInt32();
						Floorlightinfo lightInfo = new Floorlightinfo();
						lightInfo.distance = br.ReadSingle();
						lightInfo.saturation = br.ReadInt32();
						lightInfo.hue = br.ReadInt32();
						lightInfo.brightness = br.ReadSingle();
						lightInfo.spread = br.ReadSingle();
						flatCLightInfoById[key] = lightInfo;
						break;
					}
				}
			}
		}

		public void loadSounds()
		{
			foreach (sounds.sfxinfo_t sfx in sounds.S_sfx)
			{
				sfx.lumpnum = i_sound.I_GetSfxLumpNum(sfx);
				if (sfx.lumpnum <= 0) continue;
				w_wad.CacheInfo cache = w_wad.W_CacheLumpNum(sfx.lumpnum, DoomDef.PU_SOUND);
				BinaryReader bs = new BinaryReader(new MemoryStream(cache.data));
				short useless1 = bs.ReadInt16();
				if (useless1 != 3) continue;
				short sampleRate = bs.ReadInt16();
				ushort n = bs.ReadUInt16();
				short useless2 = bs.ReadInt16();
				byte[] audioData8 = new byte[n * 2];
				for (int i = 0; i < n; ++i)
				{
					int sample = (int)cache.data[8 + i] * 128;
					sample -= 256 * 128 / 2;
					byte[] bytes = BitConverter.GetBytes(sample);
					audioData8[i * 2 + 0] = bytes[0];
					audioData8[i * 2 + 1] = bytes[1];
				}

				sfx.snd_ptr = new SoundEffect(audioData8, 0, audioData8.Length, sampleRate, AudioChannels.Mono, 0, n); 
			}
		}

		int targetSector = 0;
		Vector2 map_camera = Vector2.Zero;
		float map_zoom = 4;
		public r_local.sector_t mouseHoverSector;
		public r_local.sector_t selectedSector;

		protected override void Draw(GameTime gameTime)
		{
			Color backColor = Color.Black;
			GraphicsDevice.Clear(backColor);

#if DEBUG__
			Matrix mapMatrix = Matrix.CreateTranslation((float)graphics.PreferredBackBufferWidth / 2, (float)graphics.PreferredBackBufferHeight / 2, 0);
			mapMatrix *= Matrix.CreateScale(1.0f / map_zoom);
			mapMatrix *= Matrix.CreateTranslation((float)(int)map_camera.X, (float)(int)map_camera.Y, 0);

			// Draw grid
	/*		if (p_setup.lines != null)
			{
				Color veryDarkGrey = new Color(.15f, .15f, .15f);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, mapMatrix);
				for (int i = -32768; i <= 32768; i += 128)
				{
					spriteBatch.Draw(texBlank,
						new Rectangle(
							-32768 ,
							i ,
							32768 * 2, (int)map_zoom + 1),
							veryDarkGrey);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							i ,
							-32768,
							(int)map_zoom + 1, 32768 * 2),
							veryDarkGrey);
				}
				spriteBatch.End();
			}*/

			// Draw bounding boxes
		/*	if (p_setup.nodes != null)
			{
				foreach (r_local.node_t node in p_setup.nodes)
				{
					int top = (-node.bbox[0][(int)DoomData.eUnknownEnumType2.BOXTOP] >> DoomDef.FRACBITS) ;
					int bottom = (-node.bbox[0][(int)DoomData.eUnknownEnumType2.BOXBOTTOM] >> DoomDef.FRACBITS) ;
					int left = (node.bbox[0][(int)DoomData.eUnknownEnumType2.BOXLEFT] >> DoomDef.FRACBITS) ;
					int right = (node.bbox[0][(int)DoomData.eUnknownEnumType2.BOXRIGHT] >> DoomDef.FRACBITS) ;
					spriteBatch.Begin();
					spriteBatch.Draw(texBlank,
						new Rectangle(
							left,
							top,
							right - left,
							1),
							Color.Blue);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							left,
							bottom,
							right - left,
							1),
							Color.Blue);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							left,
							top,
							1,
							bottom - top),
							Color.Blue);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							right,
							top,
							1,
							bottom - top),
							Color.Blue);
					spriteBatch.End();
				}
			}*/

			// Draw map DEBUG
			if (p_setup.lines != null)
			{
				Color stairsColor = new Color(.5f, .5f, .5f);
				Color wallColor = Color.Red;
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, mapMatrix);
				foreach (r_local.line_t line in p_setup.lines)
				{
					Vector2 p1 = new Vector2(
						(line.v1.x >> DoomDef.FRACBITS) ,
						(-line.v1.y >> DoomDef.FRACBITS));
					Vector2 p2 = new Vector2(
						(line.v2.x >> DoomDef.FRACBITS) ,
						(-line.v2.y >> DoomDef.FRACBITS) );
					float size = Vector2.Distance(p1, p2);
					float angle = (float)Math.Atan2((double)(p2.Y - p1.Y), (double)(p2.X - p1.X));
					spriteBatch.Draw(texBlank,
						p1, null,
						(line.backsector != null)?stairsColor:wallColor,
						angle,
						new Vector2(.5f / size, 0),
						new Vector2(size, (int)map_zoom + 1), 
						SpriteEffects.None, 0);
				}
				{
					r_local.sector_t sector = p_setup.sectors[targetSector];
					Dictionary<r_local.vertex_t, int> vertReferences = new Dictionary<r_local.vertex_t, int>();
					for (int i = 0; i < sector.linecount; ++i)
					{
						r_local.line_t line = p_setup.linebuffer[sector.linesi + i];
						vertReferences[line.v1] = 0;
						vertReferences[line.v2] = 0;
					}
					for (int i = 0; i < sector.linecount; ++i)
					{
						r_local.line_t line = p_setup.linebuffer[sector.linesi + i];
						vertReferences[line.v1]++;
						vertReferences[line.v2]++;
					}
					foreach (KeyValuePair<r_local.vertex_t, int> vert in vertReferences)
					{
						Point p1 = new Point(
							(vert.Key.x >> DoomDef.FRACBITS) ,
							(-vert.Key.y >> DoomDef.FRACBITS) );
						Color color = Color.Yellow;
						if (vert.Value == 3)
						{
							color = Color.Red;
						}
						else if (vert.Value == 4)
						{
							color = Color.Magenta;
						}
						spriteBatch.Draw(texBlank,
							new Rectangle(p1.X - 2, p1.Y - 2, 3, 3),
							color);
					}
					if (sector.vertGroups != null)
					{
						foreach (List<r_local.vertex_t> verts in sector.vertGroups)
						{
							for (int i = 0; i < verts.Count; ++i)
							{
								r_local.vertex_t v1 = verts[i];
								r_local.vertex_t v2 = verts[(i + 1) % verts.Count];
								Vector2 p1 = new Vector2(
									(v1.x >> DoomDef.FRACBITS),
									(-v1.y >> DoomDef.FRACBITS));
								Vector2 p2 = new Vector2(
									(v2.x >> DoomDef.FRACBITS),
									(-v2.y >> DoomDef.FRACBITS));
								float size = Vector2.Distance(p1, p2);
								float angle = (float)Math.Atan2((double)(p2.Y - p1.Y), (double)(p2.X - p1.X));
								spriteBatch.Draw(texBlank,
									p1, null,
									Color.Yellow,
									angle,
									new Vector2(.5f / size, 0),
									new Vector2(size, (int)map_zoom + 1),
									SpriteEffects.None, 0);
							}
						}
					}
					for (int i = 0 ; i < sector.linecount; ++i)
					{
						r_local.line_t line = p_setup.linebuffer[sector.linesi + i];
						r_local.vertex_t v1 = line.v1;
						r_local.vertex_t v2 = line.v2;
						Vector2 p1 = new Vector2(
							(v1.x >> DoomDef.FRACBITS),
							(-v1.y >> DoomDef.FRACBITS));
						Vector2 p2 = new Vector2(
							(v2.x >> DoomDef.FRACBITS),
							(-v2.y >> DoomDef.FRACBITS));
						float size = Vector2.Distance(p1, p2);
						float angle = (float)Math.Atan2((double)(p2.Y - p1.Y), (double)(p2.X - p1.X));
						spriteBatch.Draw(texBlank,
							p1, null,
							Color.Blue,
							angle,
							new Vector2(.5f / size, 0),
							new Vector2(size, (int)map_zoom + 3),
							SpriteEffects.None, 0);
						spriteBatch.DrawString(font, "" + i, (p1 + p2) * .5f, Color.White,
							0, Vector2.Zero, map_zoom * .75f, SpriteEffects.None, 0);
					}

					// Render point groups
					if (sector.vertGroups != null)
					{
						foreach (List<r_local.vertex_t> verts in sector.vertGroups)
						{
							int i = 0;
							foreach (r_local.vertex_t vert in verts)
							{
								Vector2 p = new Vector2(
									(vert.x >> DoomDef.FRACBITS),
									(-vert.y >> DoomDef.FRACBITS));
								spriteBatch.DrawString(font, "" + i, p, Color.White,
									0, Vector2.Zero, map_zoom * .75f, SpriteEffects.None, 0);
								++i;
							}
						}
					}
				}

				spriteBatch.End();
			}

			// Draw BSP
			if (r_data.bspLines != null)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, mapMatrix);
			/*	foreach (r_data.Segment seg in r_data.bspLines)
				{
					Vector2 p1 = new Vector2((float)seg.x1, (float)-seg.y1);
					Vector2 p2 = new Vector2((float)seg.x2, (float)-seg.y2);
					float size = Vector2.Distance(p1, p2);
					float angle = (float)Math.Atan2((double)(p2.Y - p1.Y), (double)(p2.X - p1.X));
					spriteBatch.Draw(texBlank,
						p1, null,
						Color.Coral,
						angle,
						new Vector2(.5f / size, 0),
						new Vector2(size, (int)map_zoom + 1),
						SpriteEffects.None, 0);
				}*/
				if (p_setup.subsectors != null)
				{
					foreach (r_local.subsector_t ssector in p_setup.subsectors)
					{
						if (ssector.bspSegs != null)
						{
							foreach (r_data.Segment seg in ssector.bspSegs)
							{
								Vector2 p1 = new Vector2((float)seg.x1, (float)-seg.y1);
								Vector2 p2 = new Vector2((float)seg.x2, (float)-seg.y2);
								float size = Vector2.Distance(p1, p2);
								float angle = (float)Math.Atan2((double)(p2.Y - p1.Y), (double)(p2.X - p1.X));
								spriteBatch.Draw(texBlank,
									p1, null,
									ssector.debugColor,
									angle,
									new Vector2(.5f / size, 0),
									new Vector2(size, (int)map_zoom + 1),
									SpriteEffects.None, 0);
								spriteBatch.Draw(texBlank,
									(p1 + p2) * .5f, null,
									ssector.debugColor,
									angle + MathHelper.PiOver2,
									new Vector2(0, 0),
									new Vector2(8, (int)map_zoom + 1),
									SpriteEffects.None, 0);
							}
						}
					}
				}
				spriteBatch.End();
			}

			// Draw mobs

			// Draw player camera
			if (p_setup.lines != null)
			{
				float playerX = (float)((r_main.viewx >> DoomDef.FRACBITS) );
				float playerY = (float)((-r_main.viewy >> DoomDef.FRACBITS) );
				float playerAngle = (float)Math.Atan2(
					(double)r_main.viewcos / 65536.0,
					(double)r_main.viewsin / 65536.0
					);
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, mapMatrix);
				spriteBatch.Draw(texBlank,
					new Vector2(playerX, playerY), null, Color.Yellow,
					playerAngle, new Vector2(.5f, -4), 
					new Vector2(8, 1), 
					SpriteEffects.None, 0);
				spriteBatch.Draw(texBlank,
					new Vector2(playerX, playerY), null, Color.Yellow,
					playerAngle, new Vector2(.5f, .5f),
					new Vector2(1, 8),
					SpriteEffects.None, 0);
				spriteBatch.End();
			}

#endif
			d_main.D_Display();

			if (selectedMob.Target != null &&
				useFreeCam)
			{
				Vector3 screenPos = GraphicsDevice.Viewport.Project(selectedPos, proj, view, Matrix.Identity);
				if (screenPos.Z < 1)
				{
					DoomDef.mobj_t mo = selectedMob.Target as DoomDef.mobj_t;
					info.mobjinfo_t mobT = info.mobjinfo[(int)mo.type];
					int lump;
					r_local.spritedef_t sprdef;
					r_local.spriteframe_t sprframe;
					sprdef = r_thing.sprites[(int)mo.sprite];
					sprframe = sprdef.spriteframes[mo.frame & DoomDef.FF_FRAMEMASK];
					lump = sprframe.lump[0];
					Microsoft.Xna.Framework.Graphics.Texture2D texture = w_wad.W_CacheLumpNum(lump + r_data.firstspritelump, DoomDef.PU_CACHE).cache as Microsoft.Xna.Framework.Graphics.Texture2D;

					Vector3 screenPos2 = GraphicsDevice.Viewport.Project(
						selectedPos +
						camRight * (float)texture.Width * .5f -
						Vector3.UnitZ * (float)texture.Height * .5f
						, proj, view, Matrix.Identity);
					int sizeX = (int)(screenPos2.X - screenPos.X);
					int sizeY = (int)(screenPos2.Y - screenPos.Y);
					int extendX = Math.Min(64, sizeX) / 2;
					int extendY = Math.Min(64, sizeY) / 2;
					spriteBatch.Begin();
					spriteBatch.Draw(texBlank,
						new Rectangle(
							(int)screenPos.X - sizeX,
							(int)screenPos.Y - sizeY,
							1,
							extendY),
						Color.Yellow);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							(int)screenPos.X - sizeX,
							(int)screenPos.Y - sizeY,
							extendX,
							1),
						Color.Yellow);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							(int)screenPos.X - sizeX,
							(int)screenPos.Y + sizeY - extendY,
							1,
							extendY),
						Color.Yellow);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							(int)screenPos.X - sizeX,
							(int)screenPos.Y + sizeY,
							extendX,
							1),
						Color.Yellow);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							(int)screenPos.X + sizeX,
							(int)screenPos.Y - sizeY,
							1,
							extendY),
						Color.Yellow);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							(int)screenPos.X + sizeX - extendX,
							(int)screenPos.Y - sizeY,
							extendX,
							1),
						Color.Yellow);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							(int)screenPos.X + sizeX,
							(int)screenPos.Y + sizeY - extendY,
							1,
							extendY),
						Color.Yellow);
					spriteBatch.Draw(texBlank,
						new Rectangle(
							(int)screenPos.X + sizeX - extendX,
							(int)screenPos.Y + sizeY,
							extendX,
							1),
						Color.Yellow);
					spriteBatch.End();
				}
			}

#if DEBUG

			if (textureViewerOffset > 0)
			{
				int x = 0;
				int y = graphics.PreferredBackBufferHeight;
				int biggestOnRow = 0;
				spriteBatch.Begin();

				/*	int sndId = 0;
					foreach (sounds.sfxinfo_t sfx in sounds.S_sfx)
					{
						if (sfx.snd_ptr == null) continue;
						spriteBatch.DrawString(font, sfx.name + ": " + sndId, new Vector2(x, y - textureViewerOffset), Color.White);
						++sndId;
						y += 32;
					}*/

				foreach (KeyValuePair<int, Texture2D> texture in wallTexturesById)
				{
					if (x + texture.Value.Width > graphics.PreferredBackBufferWidth)
					{
						x = 0;
						y += biggestOnRow;
						biggestOnRow = 0;
					}
					if (biggestOnRow < texture.Value.Height + 1) biggestOnRow = texture.Value.Height + 1;
					spriteBatch.Draw(texBlank,
						new Rectangle(x, y - textureViewerOffset, texture.Value.Width + 2, texture.Value.Height + 2),
						Color.Magenta);
					spriteBatch.Draw(texBlank,
						new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Value.Width, texture.Value.Height),
						backColor);
					spriteBatch.Draw(texture.Value,
						new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Value.Width, texture.Value.Height),
						Color.White);

					spriteBatch.DrawString(font, texture.Key.ToString(), new Vector2(x, y - textureViewerOffset), Color.White);

					x += texture.Value.Width + 1;
				}

				foreach (KeyValuePair<int, Texture2D> texture in floorTexturesById)
				{
					if (x + texture.Value.Width > graphics.PreferredBackBufferWidth)
					{
						x = 0;
						y += biggestOnRow;
						biggestOnRow = 0;
					}
					if (biggestOnRow < texture.Value.Height + 1) biggestOnRow = texture.Value.Height + 1;
					spriteBatch.Draw(texBlank,
						new Rectangle(x, y - textureViewerOffset, texture.Value.Width + 2, texture.Value.Height + 2),
						Color.Magenta);
					spriteBatch.Draw(texBlank,
						new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Value.Width, texture.Value.Height),
						backColor);
					spriteBatch.Draw(texture.Value,
						new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Value.Width, texture.Value.Height),
						Color.White);

					spriteBatch.DrawString(font, texture.Key.ToString(), new Vector2(x, y - textureViewerOffset), Color.White);

					x += texture.Value.Width + 1;
				}

				foreach (KeyValuePair<string, Texture2D> texture in patchTextures)
				{
					if (x + texture.Value.Width > graphics.PreferredBackBufferWidth)
					{
						x = 0;
						y += biggestOnRow;
						biggestOnRow = 0;
					}
					if (biggestOnRow < texture.Value.Height + 1) biggestOnRow = texture.Value.Height + 1;
					spriteBatch.Draw(texBlank,
						new Rectangle(x, y - textureViewerOffset, texture.Value.Width + 2, texture.Value.Height + 2),
						Color.Magenta);
					spriteBatch.Draw(texBlank,
						new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Value.Width, texture.Value.Height),
						backColor);
					spriteBatch.Draw(texture.Value,
						new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Value.Width, texture.Value.Height),
						Color.White);

					spriteBatch.DrawString(font, texture.Key.ToString(), new Vector2(x, y - textureViewerOffset), Color.White);

					x += texture.Value.Width + 1;
				}

				if (AmbientMap.ambientTexture != null)
				{
					Texture2D texture = AmbientMap.ambientTexture;
					if (x + texture.Width / 4 > graphics.PreferredBackBufferWidth)
					{
						x = 0;
						y += biggestOnRow;
						biggestOnRow = 0;
					}

					if (biggestOnRow < texture.Height + 1) biggestOnRow = texture.Height + 1;
					spriteBatch.Draw(texBlank,
						new Rectangle(x, y - textureViewerOffset, texture.Width / 4 + 2, texture.Height / 4 + 2),
						Color.Magenta);
					spriteBatch.Draw(texBlank,
						new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Width / 4, texture.Height / 4),
						backColor);
					spriteBatch.Draw(texture,
						new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Width / 4, texture.Height / 4),
						Color.White);

					spriteBatch.DrawString(font, "AmbientMap", new Vector2(x, y - textureViewerOffset), Color.White);

					x += texture.Width / 4 + 1;
				}

				if (p_setup.sectors != null)
					foreach (r_local.sector_t sector in p_setup.sectors)
					{
						if (sector.lightData == null) continue;
						if (sector.lightData.textureSpread == null) continue;
						Texture2D texture = sector.lightData.textureSpread;

						if (x + texture.Width > graphics.PreferredBackBufferWidth)
						{
							x = 0;
							y += biggestOnRow;
							biggestOnRow = 0;
						}
						if (biggestOnRow < texture.Height + 1) biggestOnRow = texture.Height + 1;
						spriteBatch.Draw(texBlank,
							new Rectangle(x, y - textureViewerOffset, texture.Width + 2, texture.Height + 2),
							Color.Magenta);
						spriteBatch.Draw(texBlank,
							new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Width, texture.Height),
							backColor);
						spriteBatch.Draw(texture,
							new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Width, texture.Height),
							Color.White);

						x += texture.Width + 1;
					}
				if (p_setup.sectors != null)
					foreach (r_local.sector_t sector in p_setup.sectors)
					{
						if (sector.lightDataC == null) continue;
						if (sector.lightDataC.textureSpread == null) continue;
						Texture2D texture = sector.lightDataC.textureSpread;

						if (x + texture.Width > graphics.PreferredBackBufferWidth)
						{
							x = 0;
							y += biggestOnRow;
							biggestOnRow = 0;
						}
						if (biggestOnRow < texture.Height + 1) biggestOnRow = texture.Height + 1;
						spriteBatch.Draw(texBlank,
							new Rectangle(x, y - textureViewerOffset, texture.Width + 2, texture.Height + 2),
							Color.Magenta);
						spriteBatch.Draw(texBlank,
							new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Width, texture.Height),
							backColor);
						spriteBatch.Draw(texture,
							new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Width, texture.Height),
							Color.White);

						x += texture.Width + 1;
					}

				/*	for (DoomDef.thinker_t think = p_tick.thinkercap.next; think != p_tick.thinkercap; think = think.next)
					{
						if (think == null) break;
						if (think.function == null) continue;
						DoomDef.mobj_t mo = think.function.obj as DoomDef.mobj_t;
						if (mo == null) continue;
						if (mo.shadowInfo == null) continue;
						if (mo.shadowInfo.shadowMap == null) continue;

						TextureCube textureCube	 = mo.shadowInfo.shadowMap;

						for (int i = 0; i < 6; ++i)
						{
							if (x + textureCube.Size > graphics.PreferredBackBufferWidth)
							{
								x = 0;
								y += biggestOnRow;
								biggestOnRow = 0;
							}
							if (biggestOnRow < texture.Height + 1) biggestOnRow = texture.Height + 1;
							spriteBatch.Draw(texBlank,
								new Rectangle(x, y - textureViewerOffset, texture.Width + 2, texture.Height + 2),
								Color.Magenta);
							spriteBatch.Draw(texBlank,
								new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Width, texture.Height),
								backColor);
							spriteBatch.Draw(texture,
								new Rectangle(x + 1, y + 1 - textureViewerOffset, texture.Width, texture.Height),
								Color.White);

							x += texture.Width + 1;
						}
					}*/

				spriteBatch.End();
			}

			if (Settings.Default.use_deferred && showGBuffer)
			{
				Deferred.GBuffer.displayDebug();
			}

			Frame.displayDebug();

			// Show selected sector, or affected sector for selected entity
			Game1.instance.fxSelectedLines.Parameters["View"].SetValue(view);
			Game1.instance.fxSelectedLines.Parameters["Projection"].SetValue(proj);
			Game1.instance.fxSelectedLines.Parameters["World"].SetValue(Matrix.Identity);
			if (selectedMob.Target != null)
			{
				DoomDef.mobj_t mo = selectedMob.Target as DoomDef.mobj_t;
				if (mo.sectorsInRadius != null)
				{
					Game1.instance.fxSelectedLines.Parameters["SelectionColor"].SetValue(new Vector4(1, 1, 0, 1));
					Game1.instance.fxSelectedLines.CurrentTechnique = Game1.instance.fxSelectedLines.Techniques[0];
					Game1.instance.fxSelectedLines.CurrentTechnique.Passes[0].Apply();
					GraphicsDevice.DepthStencilState = DepthStencilState.None;
					GraphicsDevice.BlendState = BlendState.AlphaBlend;
					GraphicsDevice.RasterizerState = RasterizerState.CullNone;
					foreach (r_local.sector_t sec in mo.sectorsInRadius)
					{
						foreach (r_local.seg_t seg in p_setup.segs)
						{
							if (seg.frontsector == sec)
							{
								Vector3 vp1 = new Vector3(
									seg.v1.x >> DoomDef.FRACBITS,
									seg.v1.y >> DoomDef.FRACBITS,
									sec.floorheight >> DoomDef.FRACBITS);
								Vector3 vp2 = new Vector3(
									seg.v2.x >> DoomDef.FRACBITS,
									seg.v2.y >> DoomDef.FRACBITS,
									vp1.Z);

								debugVerts[0] = new VertexPositionColor(vp1, Color.White);
								debugVerts[1] = new VertexPositionColor(vp2, Color.White);

								GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, debugVerts, 0, 1);
							}
						}
					}
				}
			}
			if (selectedSector != null)
			{
				Game1.instance.fxSelectedLines.Parameters["SelectionColor"].SetValue(new Vector4(1, 0, 1, .5f));
				Game1.instance.fxSelectedLines.CurrentTechnique = Game1.instance.fxSelectedLines.Techniques[0];
				Game1.instance.fxSelectedLines.CurrentTechnique.Passes[0].Apply();
				GraphicsDevice.DepthStencilState = DepthStencilState.None;
				GraphicsDevice.BlendState = BlendState.AlphaBlend;
				GraphicsDevice.RasterizerState = RasterizerState.CullNone;
				foreach (r_local.seg_t seg in p_setup.segs)
				{
					if (seg.frontsector == selectedSector)
					{
						debugVerts[0] = new VertexPositionColor(new Vector3(
							seg.v1.x >> DoomDef.FRACBITS,
							seg.v1.y >> DoomDef.FRACBITS,
							selectedSector.floorheight >> DoomDef.FRACBITS), Color.White);
						debugVerts[1] = new VertexPositionColor(new Vector3(
							seg.v2.x >> DoomDef.FRACBITS,
							seg.v2.y >> DoomDef.FRACBITS,
							selectedSector.floorheight >> DoomDef.FRACBITS), Color.White);
						debugVerts[2] = new VertexPositionColor(new Vector3(
							seg.v1.x >> DoomDef.FRACBITS,
							seg.v1.y >> DoomDef.FRACBITS,
							selectedSector.ceilingheight >> DoomDef.FRACBITS), Color.White);
						debugVerts[3] = new VertexPositionColor(new Vector3(
							seg.v2.x >> DoomDef.FRACBITS,
							seg.v2.y >> DoomDef.FRACBITS,
							selectedSector.ceilingheight >> DoomDef.FRACBITS), Color.White);

						GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, debugVerts, 0, 2);
					}
				}
			}

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
			foreach (Vector4 bb in Frame.debugRects)
			{
				spriteBatch.Draw(texBlank,
					new Rectangle((int)bb.X, (int)bb.Y, 2, (int)(bb.W - bb.Y)),
					new Color(.2f, 0, 0));
				spriteBatch.Draw(texBlank,
					new Rectangle((int)bb.Z, (int)bb.Y, 2, (int)(bb.W - bb.Y)),
					new Color(.2f, 0, 0));
				spriteBatch.Draw(texBlank,
					new Rectangle((int)bb.X, (int)bb.Y, (int)(bb.Z - bb.X), 2),
					new Color(.2f, 0, 0));
				spriteBatch.Draw(texBlank,
					new Rectangle((int)bb.X, (int)bb.W, (int)(bb.Z - bb.X), 2),
					new Color(.2f, 0, 0));
			}
			spriteBatch.End();

			if (showDebugHDR)
			{
				spriteBatch.Begin(
					SpriteSortMode.Immediate,
					BlendState.Opaque,
					SamplerState.PointClamp,
					DepthStencilState.None,
					RasterizerState.CullNone);
				spriteBatch.Draw(
					lastFrameLevels,
					new Rectangle(GraphicsDevice.Viewport.Width - 64, 0, 64, 64),
					Color.White);
				spriteBatch.Draw(
					texBlank,
					new Rectangle(GraphicsDevice.Viewport.Width - 64, 64, 64, 64),
					new Color(new Vector4(r_main.lumLast, r_main.lumLast, r_main.lumLast, 1)));
				spriteBatch.Draw(
					texBlank,
					new Rectangle(GraphicsDevice.Viewport.Width - 64, 128, 64, 64),
					new Color(new Vector4(r_main.LUM_TARGET, r_main.LUM_TARGET, r_main.LUM_TARGET, 1)));
				spriteBatch.Draw(
					bloom,
					new Rectangle(GraphicsDevice.Viewport.Width - 64 - bloom.Width / 2, 0, bloom.Width / 2, bloom.Height / 2),
					Color.White);
				spriteBatch.End();
			}

			if (showDebugHDR)
			{
				spriteBatch.Begin();
				spriteBatch.DrawString(font, "lumLast: " + r_main.lumLast, Vector2.Zero, Color.Yellow);
				spriteBatch.DrawString(font, "lumTarget: " + r_main.LUM_TARGET, new Vector2(0, 16), Color.Yellow);
				spriteBatch.DrawString(font, "lumMultiplier: " + r_main.lumMultiplier, new Vector2(0, 32), Color.Yellow);
				spriteBatch.End();
			}
#endif

			base.Draw(gameTime);
		}

#if DEBUG
		VertexPositionColor[] debugVerts = new VertexPositionColor[4];
		public Vector2 cameraAngles;
#endif
	}
}
