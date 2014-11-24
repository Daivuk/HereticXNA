using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace HereticXNA
{
	public partial class FrmSector : Form
	{
		r_local.sector_t m_sector;

		public FrmSector()
		{
			InitializeComponent();
		}

		internal void fillUIWithSector(r_local.sector_t sector)
		{
			m_sector = sector;

			{
				Microsoft.Xna.Framework.Graphics.Texture2D texture = Game1.instance.floorTexturesById[r_data.texturetranslation[sector.floorpic]];

				byte[] pixels = new byte[texture.Width * texture.Height * 4];
				texture.GetData<byte>(pixels);

				for (int i = 0; i < texture.Width * texture.Height; ++i)
				{
					byte tmp = pixels[i * 4 + 0];
					pixels[i * 4 + 0] = pixels[i * 4 + 2];
					pixels[i * 4 + 2] = tmp;
				}

				Bitmap bmp = new Bitmap(texture.Width, texture.Height, texture.Width * 4, PixelFormat.Format32bppArgb,
							GCHandle.Alloc(pixels, GCHandleType.Pinned).AddrOfPinnedObject());
				picFloor.Image = bmp;
			}
			{

				Microsoft.Xna.Framework.Graphics.Texture2D texture = sector.ceilingBatch.texture;

				byte[] pixels = new byte[texture.Width * texture.Height * 4];
				texture.GetData<byte>(pixels);

				for (int i = 0; i < texture.Width * texture.Height; ++i)
				{
					byte tmp = pixels[i * 4 + 0];
					pixels[i * 4 + 0] = pixels[i * 4 + 2];
					pixels[i * 4 + 2] = tmp;
				}

				Bitmap bmp = new Bitmap(texture.Width, texture.Height, texture.Width * 4, PixelFormat.Format32bppArgb,
							GCHandle.Alloc(pixels, GCHandleType.Pinned).AddrOfPinnedObject());
				picCeil.Image = bmp;
			}

			{
				Bitmap bmp = new Bitmap(picSector.Width, picSector.Height, PixelFormat.Format24bppRgb);

				Pen yellowPen = new Pen(Color.Yellow, 2);

				float bbminX = 100000;
				float bbminY = 100000;
				float bbmaxX = -100000;
				float bbmaxY = -100000;

				sector.floorBatch.boundingBox(ref bbminX, ref bbminY, ref bbmaxX, ref bbmaxY);

				float biggest = Math.Max(bbmaxX - bbminX, bbmaxY - bbminY);
				int padding = 4;
				float scale = (float)(picSector.Width - padding * 2) / biggest;

				bbminX -= (biggest - (bbmaxX - bbminX)) * .5f;
				bbminY -= (biggest - (bbmaxY - bbminY)) * .5f;

				// Draw lines to screen
				Graphics graphics = Graphics.FromImage(bmp);

				foreach (r_local.seg_t seg in p_setup.segs)
				{
					if (seg.frontsector == sector)
					{
						Microsoft.Xna.Framework.Vector2 vp1 = new Microsoft.Xna.Framework.Vector2(
							seg.v1.x >> DoomDef.FRACBITS,
							-seg.v1.y >> DoomDef.FRACBITS);
						Microsoft.Xna.Framework.Vector2 vp2 = new Microsoft.Xna.Framework.Vector2(
							seg.v2.x >> DoomDef.FRACBITS,
							-seg.v2.y >> DoomDef.FRACBITS);

						vp1.X -= bbminX;
						vp2.X -= bbminX;
						vp1.Y -= bbminY;
						vp2.Y -= bbminY;

						vp1 *= scale;
						vp2 *= scale;

						graphics.DrawLine(yellowPen, 
							(int)vp1.X + padding,
							(int)vp1.Y + padding,
							(int)vp2.X + padding,
							(int)vp2.Y + padding);
					}
				}

				picSector.Image = bmp;
			}
		}
	}
}
