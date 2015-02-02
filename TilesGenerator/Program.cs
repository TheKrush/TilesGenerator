/*
    Copyright (C) 2011  Mike Gleason jr Couturier (http://blog.mikecouturier.com/2011/07/create-zoomable-images-using-google.html)
    This file is part of TilesGenerator.

    TilesGenerator is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    TilesGenerator is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with TilesGenerator.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace blog_mikecouturier_com
{
    class Program
    {
        const int TILE_SIZE = 256;
        const string TILE_FILENAME = @"{0}\{1}\{2}.png";
        const string OUTPUT_DIR = @"output\";

        static void Main(string[] args)
        {
            int level = 0;

            if (args.Length != 2 || !Int32.TryParse(args[0], out level) || level < 1 || level > 5 || !File.Exists(args[1]))
            {
                Console.WriteLine("usage: \"TilesGenerator.exe [1-5] [filename]\"");
                return;
            }

            try
            {
                int mapWidth = GetMapWidth(level);

                if (!Directory.Exists(OUTPUT_DIR))
                    Directory.CreateDirectory(OUTPUT_DIR);

                using (Stream stream = File.OpenRead(args[1]))
                using (Image original = Bitmap.FromStream(stream))
                {
                    SplitTilesRecursive(original, level);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void SplitTilesRecursive(Image original, int level)
        {
            int mapWidth = GetMapWidth(level);
            int tilesOnSide = mapWidth / TILE_SIZE;

            using (Image resized = ResizeImage(original, new Size(mapWidth, mapWidth)))
            {
                for (int x = 0; x < tilesOnSide; x++)
                    for (int y = 0; y < tilesOnSide; y++)
                        CropAndSaveTile(resized, x, y, level);
            }

            if (level > 0)
                SplitTilesRecursive(original, level - 1);
        }

        static int GetMapWidth(int level)
        {
            return TILE_SIZE * (int)Math.Pow(2, level);
        }

        private static void CropAndSaveTile(Image image, int x, int y, int level)
        {
            Rectangle cropArea = new Rectangle(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE);

            using (Bitmap bmpImage = new Bitmap(image))
            using (Bitmap bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat))
            {
                string filename = String.Format(TILE_FILENAME, level, x, y);
                string directory = Path.GetDirectoryName(filename);
                if (!Directory.Exists(OUTPUT_DIR + directory))
                    Directory.CreateDirectory(OUTPUT_DIR + directory);

                // the Portable Network Graphics (PNG) encoder is used implicitly
                bmpCrop.Save(Path.Combine(OUTPUT_DIR, filename));
                Console.WriteLine("Processed " + filename);
            }
        }

        private static Image ResizeImage(Image toResize, Size size)
        {
            Bitmap b = new Bitmap(size.Width, size.Height);

            using (Graphics g = Graphics.FromImage((Image)b))
            {
                g.InterpolationMode = InterpolationMode.Default;
                g.DrawImage(toResize, 0, 0, size.Width, size.Height);

                return (Image)b;
            }
        }
    }
}
