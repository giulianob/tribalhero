using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Game.Fighting;
using System.IO;

namespace Simulator.Viewer {
    public class ImageFactory {
        static Dictionary<ushort, Bitmap> dict;
        static Bitmap default_image;
        static readonly string ImageDirectory = "C:\\Documents and Settings\\OscarMike\\Desktop\\Blue\\";
        public static Bitmap getImage(ushort type) {
            if (dict == null) {
                dict = new Dictionary<ushort, Bitmap>();
          /*      dict[2001] = new Bitmap(ImageDirectory + "Infantry.gif");
                dict[2002] = new Bitmap(ImageDirectory + "Mech.gif");
                dict[2003] = new Bitmap(ImageDirectory + "APC.gif");
                dict[2101] = new Bitmap(ImageDirectory + "Recon.gif");
                dict[2102] = new Bitmap(ImageDirectory + "Tank.gif");
                dict[2103] = new Bitmap(ImageDirectory + "MediumTank.gif");
                dict[2104] = new Bitmap(ImageDirectory + "AntiAir.gif");
                dict[2201] = new Bitmap(ImageDirectory + "Artillery.gif");
                dict[2202] = new Bitmap(ImageDirectory + "Missile.gif");
                dict[2301] = new Bitmap(ImageDirectory + "Copter.gif");
                dict[2302] = new Bitmap(ImageDirectory + "Bomber.gif");
                dict[2303] = new Bitmap(ImageDirectory + "Fighter.gif");*/
                default_image = new Bitmap(ImageDirectory + "Infantry.gif");
            }
            Bitmap image;
            if (dict.TryGetValue(type, out image)) return image;
            return default_image;
            
        }
    }
}
