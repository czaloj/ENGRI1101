using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using ORLabs.Graphics;

namespace ORLabs
{
    public partial class GridDialog : Form
    {
        public GridDialog()
        {
            InitializeComponent();
            imGMap.SizeMode = PictureBoxSizeMode.CenterImage;
        }

        private GraphicsDevice g;
        enum GMapOptions { None, Address, Coordinates }
        GMapOptions gmOpt = GMapOptions.None;
        ImageRequest gmImReq;
        GeoCodeRequest geoCodeReq;
        private void btnSGM_Click(object sender, EventArgs e)
        {
            switch (gmOpt)
            {
                case GMapOptions.Address: if (!imReqFromAddr()) { imGMap.Image = null; return; } break;
                case GMapOptions.Coordinates: imReqFromLL(); break;
            }
            Image im = GoogleMap.getMapImage(gmImReq);
            imGMap.Image = im;
            imGMap.AutoScrollOffset = new Point(imGMap.AutoScrollOffset.X - 10, imGMap.AutoScrollOffset.Y - 10);
        }
        private void cbMapOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbMapOptions.SelectedIndex)
            {
                case 0: gmOpt = GMapOptions.None; break;
                case 1: gmOpt = GMapOptions.Address; break;
                case 2: gmOpt = GMapOptions.Coordinates; break;
            }
        }
        private void imReqFromLL()
        {
            gmImReq = new ImageRequest(
                nsLatitude.Value, nsLongitude.Value,
                (int)Math.Round(nsZoom.Value),
                (int)Math.Round(nsPWidth.Value),(int)Math.Round(nsPHeight.Value),
                (int)Math.Round(nsDetail.Value), ImageRequest.MT_RoadMap
                );
        }
        private bool imReqFromAddr()
        {
            GeoCodeRequest gr = new GeoCodeRequest(tbMapAddress.Text);
            GeoCodeResults res;
            if (GeoCodeResults.fromRequest(gr, out res))
            {
                gmImReq = new ImageRequest(
                    res.Latitude, res.Longitude,
                    (int)Math.Round(nsZoom.Value),
                    (int)Math.Round(nsPWidth.Value), (int)Math.Round(nsPHeight.Value),
                    (int)Math.Round(nsDetail.Value), ImageRequest.MT_RoadMap
                    );
                return true;
            }
            return false;
        }

        private void nsStartX_ValueChanged(object sender, EventArgs e)
        {

        }
        private void nsStartY_ValueChanged(object sender, EventArgs e)
        {

        }
        private void nsCellSizeX_ValueChanged(object sender, EventArgs e)
        {

        }
        private void nsCellSizeY_ValueChanged(object sender, EventArgs e)
        {

        }
        private void nsCellCountX_ValueChanged(object sender, EventArgs e)
        {

        }
        private void nsCellCountY_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
