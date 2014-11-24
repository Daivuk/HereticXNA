using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace HereticXNA.Deferred
{
	static class Effects
	{
		private static GraphicsDevice m_device;

		public static Effect fxWall;
		public static EffectPass fxWallPass;
		public static EffectParameter fxWall_uniform_matWorldViewProj;
		public static EffectParameter fxWall_uniform_DiffuseTexture;
		public static EffectParameter fxWall_uniform_ambientEpsilonSizeAmount;

		public static Effect fxAnimatedWall;
		public static EffectPass fxAnimatedWallPass;
		public static EffectParameter fxAnimatedWall_uniform_matWorldViewProj;
		public static EffectParameter fxAnimatedWall_uniform_DiffuseTexture1;
		public static EffectParameter fxAnimatedWall_uniform_DiffuseTexture2;
		public static EffectParameter fxAnimatedWall_uniform_ambientEpsilonSizeAmount;
		public static EffectParameter fxAnimatedWall_uniform_animDelta;

		public static Effect fxPlane;
		public static EffectPass fxPlanePass;
		public static EffectParameter fxPlane_uniform_matWorldViewProj;
		public static EffectParameter fxPlane_uniform_DiffuseTexture;
		public static EffectParameter fxPlane_uniform_AmbientTexture;
		public static EffectParameter fxPlane_uniform_ambientLimits;
		public static EffectParameter fxPlane_uniform_ambientLimitsZPixelSize;
		public static EffectParameter fxPlane_uniform_ambientEpsilonSizeAmount;

		public static Effect fxAnimatedFloor;
		public static EffectPass fxAnimatedFloorPass;
		public static EffectParameter fxAnimatedFloor_uniform_matWorldViewProj;
		public static EffectParameter fxAnimatedFloor_uniform_DiffuseTexture1;
		public static EffectParameter fxAnimatedFloor_uniform_DiffuseTexture2;
		public static EffectParameter fxAnimatedFloor_uniform_AmbientTexture;
		public static EffectParameter fxAnimatedFloor_uniform_animDelta;
		public static EffectParameter fxAnimatedFloor_uniform_ambientLimits;
		public static EffectParameter fxAnimatedFloor_uniform_ambientLimitsZPixelSize;
		public static EffectParameter fxAnimatedFloor_uniform_ambientEpsilonSizeAmount;

		public static Effect fxSky;
		public static EffectPass fxSkyPass;
		public static EffectParameter fxSky_uniform_matWorldViewProj;
		public static EffectParameter fxSky_uniform_SkyTexture;
		public static EffectParameter fxSky_uniform_cameraAngles;

		//--- Deferred shaders
		public static Effect fxAmbient;
		public static EffectPass fxAmbientPass;
		public static EffectParameter fxAmbient_uniform_ColorTexture;
		public static EffectParameter fxAmbient_uniform_DepthTexture;
		public static EffectParameter fxAmbient_uniform_NormalTexture;
		public static EffectParameter fxAmbient_uniform_ambientColor;

		public static void init(GraphicsDevice in_device)
		{
			m_device = in_device;

			// The shader that renders the walls, and the floors/ceilings without ambient
			fxWall = Game1.instance.Content.Load<Effect>("Deferred/fxWall");
			fxWallPass = fxWall.CurrentTechnique.Passes.First();
			fxWall_uniform_matWorldViewProj = fxWall.Parameters["matWorldViewProj"];
			fxWall_uniform_DiffuseTexture = fxWall.Parameters["DiffuseTexture"];
			fxWall_uniform_ambientEpsilonSizeAmount = fxWall.Parameters["ambientEpsilonSizeAmount"];

			// The shader that renders the animated walls, and the animated floors/ceilings without ambient
			fxAnimatedWall = Game1.instance.Content.Load<Effect>("Deferred/fxAnimatedWall");
			fxAnimatedWallPass = fxAnimatedWall.CurrentTechnique.Passes.First();
			fxAnimatedWall_uniform_matWorldViewProj = fxAnimatedWall.Parameters["matWorldViewProj"];
			fxAnimatedWall_uniform_DiffuseTexture1 = fxAnimatedWall.Parameters["DiffuseTexture1"];
			fxAnimatedWall_uniform_DiffuseTexture2 = fxAnimatedWall.Parameters["DiffuseTexture2"];
			fxAnimatedWall_uniform_ambientEpsilonSizeAmount = fxAnimatedWall.Parameters["ambientEpsilonSizeAmount"];
			fxAnimatedWall_uniform_animDelta = fxAnimatedWall.Parameters["animDelta"];

			// The shader that renders the floors
			fxPlane = Game1.instance.Content.Load<Effect>("Deferred/fxPlane");
			fxPlanePass = fxPlane.CurrentTechnique.Passes.First();
			fxPlane_uniform_matWorldViewProj = fxPlane.Parameters["matWorldViewProj"];
			fxPlane_uniform_DiffuseTexture = fxPlane.Parameters["DiffuseTexture"];
			fxPlane_uniform_AmbientTexture = fxPlane.Parameters["AmbientTexture"];
			fxPlane_uniform_ambientLimits = fxPlane.Parameters["ambientLimits"];
			fxPlane_uniform_ambientLimitsZPixelSize = fxPlane.Parameters["ambientLimitsZPixelSize"];
			fxPlane_uniform_ambientEpsilonSizeAmount = fxPlane.Parameters["ambientEpsilonSizeAmount"];

			// The shader that renders the animated floors
			fxAnimatedFloor = Game1.instance.Content.Load<Effect>("Deferred/fxAnimatedFloor");
			fxAnimatedFloorPass = fxAnimatedFloor.CurrentTechnique.Passes.First();
			fxAnimatedFloor_uniform_matWorldViewProj = fxAnimatedFloor.Parameters["matWorldViewProj"];
			fxAnimatedFloor_uniform_DiffuseTexture1 = fxAnimatedFloor.Parameters["DiffuseTexture1"];
			fxAnimatedFloor_uniform_DiffuseTexture2 = fxAnimatedFloor.Parameters["DiffuseTexture2"];
			fxAnimatedFloor_uniform_AmbientTexture = fxAnimatedFloor.Parameters["AmbientTexture"];
			fxAnimatedFloor_uniform_animDelta = fxAnimatedFloor.Parameters["animDelta"];
			fxAnimatedFloor_uniform_ambientLimits = fxAnimatedFloor.Parameters["ambientLimits"];
			fxAnimatedFloor_uniform_ambientLimitsZPixelSize = fxAnimatedFloor.Parameters["ambientLimitsZPixelSize"];
			fxAnimatedFloor_uniform_ambientEpsilonSizeAmount = fxAnimatedFloor.Parameters["ambientEpsilonSizeAmount"];

			// The shader that renders the sky
			fxSky = Game1.instance.Content.Load<Effect>("Deferred/fxSky");
			fxSkyPass = fxSky.CurrentTechnique.Passes.First();
			fxSky_uniform_matWorldViewProj = fxSky.Parameters["matWorldViewProj"];
			fxSky_uniform_SkyTexture = fxSky.Parameters["SkyTexture"];
			fxSky_uniform_cameraAngles = fxSky.Parameters["cameraAngles"];

			// Used to render the intial ambient color before applying lights
			fxAmbient = Game1.instance.Content.Load<Effect>("Deferred/fxAmbient");
			fxAmbientPass = fxAmbient.CurrentTechnique.Passes.First();
			fxAmbient_uniform_ColorTexture = fxAmbient.Parameters["ColorTexture"];
			fxAmbient_uniform_DepthTexture = fxAmbient.Parameters["DepthTexture"];
			fxAmbient_uniform_NormalTexture = fxAmbient.Parameters["NormalTexture"];
			fxAmbient_uniform_ambientColor = fxAmbient.Parameters["ambientColor"];

			updateSettings();
		}

		public static void prepareLevel()
		{
			// This is called after a level is loaded because
			// some shaders have fix parameters per level.
			fxSky_uniform_SkyTexture.SetValue(Game1.instance.wallTexturesById[r_plane.skytexture]);
			fxAmbient_uniform_ambientColor.SetValue(new Vector3(.2f, .1f, .3f));
		}

		public static void prepare()
		{
			Matrix matWorldViewProj = Game1.instance.view * Game1.instance.proj;

			// Set matrices
			fxWall_uniform_matWorldViewProj.SetValue(matWorldViewProj);
			fxAnimatedWall_uniform_matWorldViewProj.SetValue(matWorldViewProj);
			fxPlane_uniform_matWorldViewProj.SetValue(matWorldViewProj);
			fxAnimatedFloor_uniform_matWorldViewProj.SetValue(matWorldViewProj);
			fxSky_uniform_matWorldViewProj.SetValue(matWorldViewProj);

			// Camera angles
			fxSky_uniform_cameraAngles.SetValue(Game1.instance.cameraAngles);
		}

		public static void updateSettings()
		{
			if (Game1.instance.showTextures)
			{
				if (Settings.Default.ambient_enabled)
				{
					fxWall.CurrentTechnique = fxWall.Techniques[0];
					fxAnimatedWall.CurrentTechnique = fxAnimatedWall.Techniques[0];
					fxPlane.CurrentTechnique = fxPlane.Techniques[0];
					fxAnimatedFloor.CurrentTechnique = fxAnimatedFloor.Techniques[0];
				}
				else
				{
					fxWall.CurrentTechnique = fxWall.Techniques[1];
					fxAnimatedWall.CurrentTechnique = fxAnimatedWall.Techniques[1];
					fxPlane.CurrentTechnique = fxPlane.Techniques[1];
					fxAnimatedFloor.CurrentTechnique = fxAnimatedFloor.Techniques[1];
				}
			}
			else
			{
				if (Settings.Default.ambient_enabled)
				{
					fxWall.CurrentTechnique = fxWall.Techniques[2];
					fxAnimatedWall.CurrentTechnique = fxAnimatedWall.Techniques[2];
					fxPlane.CurrentTechnique = fxPlane.Techniques[2];
					fxAnimatedFloor.CurrentTechnique = fxAnimatedFloor.Techniques[2];
				}
				else
				{
					fxWall.CurrentTechnique = fxWall.Techniques[3];
					fxAnimatedWall.CurrentTechnique = fxAnimatedWall.Techniques[3];
					fxPlane.CurrentTechnique = fxPlane.Techniques[3];
					fxAnimatedFloor.CurrentTechnique = fxAnimatedFloor.Techniques[3];
				}
			}

			fxWallPass = fxWall.CurrentTechnique.Passes.First();
			fxAnimatedWallPass = fxAnimatedWall.CurrentTechnique.Passes.First();
			fxPlanePass = fxPlane.CurrentTechnique.Passes.First();
			fxAnimatedFloorPass = fxAnimatedFloor.CurrentTechnique.Passes.First();

			if (Settings.Default.ambient_enabled)
			{
				Vector3 ambientEpsilonSizeAmount;
				ambientEpsilonSizeAmount.X = r_segs.ambientEpsilon;
				ambientEpsilonSizeAmount.Y = r_segs.ambientSize;
				ambientEpsilonSizeAmount.Z = r_segs.ambientAmount;

				fxWall_uniform_ambientEpsilonSizeAmount.SetValue(ambientEpsilonSizeAmount);
				fxAnimatedWall_uniform_ambientEpsilonSizeAmount.SetValue(ambientEpsilonSizeAmount);
				fxPlane_uniform_ambientEpsilonSizeAmount.SetValue(ambientEpsilonSizeAmount);
				fxAnimatedFloor_uniform_ambientEpsilonSizeAmount.SetValue(ambientEpsilonSizeAmount);
			}

			// Apply gbuffer
			fxAmbient_uniform_ColorTexture.SetValue(GBuffer.ColorTexture);
			fxAmbient_uniform_DepthTexture.SetValue(GBuffer.DepthTexture);
			fxAmbient_uniform_NormalTexture.SetValue(GBuffer.NormalTexture);
		}
	}
}
