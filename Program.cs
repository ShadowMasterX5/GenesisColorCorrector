using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace GenesisColorCorrector {
    class Program {
        // Base palette index.
        public static byte      BaseIndex       = 32;
        public static bool      UseBaseIndex    = false;
        public static byte[]    HardwareColors  = new byte[]{ 0, 34, 68, 102, 136, 170, 204, 238};

        public static byte NearestColor(byte color) {
            // Darkest Shade (0)
            if(color < 12)
               return 0;

            // (32)
            if(color < 48)
               return UseBaseIndex ? BaseIndex : HardwareColors[1];

            // (64)
            if(color < 76)
               return UseBaseIndex ? (byte)(BaseIndex * 2) : HardwareColors[2];

            // (96)
            if(color < 114)
               return UseBaseIndex ? (byte)(BaseIndex * 3) : HardwareColors[3];

            // (128)
            if(color < 142)
               return UseBaseIndex ? (byte)(BaseIndex * 4) : HardwareColors[4];

            // (160)
            if(color < 174)
               return UseBaseIndex ? (byte)(BaseIndex * 5) : HardwareColors[5];

            // (192)
            if(color < 208)
               return UseBaseIndex ? (byte)(BaseIndex * 6) : HardwareColors[6];

            // Lightest shade (224)
            return    UseBaseIndex ? (byte)(BaseIndex * 7) : HardwareColors[7];
        }

        static void Main(string[] args){
            // Create output directory.
            Directory.CreateDirectory(Environment.CurrentDirectory + "\\output\\");

            // Get files in current directory.
            string[] files = args.Length == 0 ? Directory.GetFiles(Environment.CurrentDirectory) : args;
            
            // Cycle through files.
            for(int index = 0; index < files.Length; index++) {
                string file = files[index];

                // Continue is file doesn't exist.
                if(!File.Exists(file))
                    continue;

                // If file isn't an image, skip it.
                if(Path.GetExtension(file) != ".png" && Path.GetExtension(file) != ".bmp" && Path.GetExtension(file) != ".gif" && Path.GetExtension(file) != ".jpg" && Path.GetExtension(file) != ".jpeg")
                    continue;

                // Load image
                Bitmap      image       = new Bitmap(file);

                // Set up for modifying pixels.
                BitmapData  pixelData   = image.LockBits(new Rectangle(new Point(0, 0), image.Size), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                unsafe {
                    byte* srcPointer = (byte*)pixelData.Scan0;

                    // Cycle through pixels
                    for(int i = 0; i < image.Size.Height; i++) {
                        for(int j = 0; j < image.Size.Width; j++) {

                            // Replace each color channel with the closest match.
                            srcPointer[0] = NearestColor(srcPointer[0]); // Blue
                            srcPointer[1] = NearestColor(srcPointer[1]); // Green
                            srcPointer[2] = NearestColor(srcPointer[2]); // Red

                            // Next pixel (ignore alpha)
                            srcPointer += 4;
                        }
                    }
                }

                // Finalize Image
                image.UnlockBits(pixelData);

                // Save image
                using(MemoryStream memory = new MemoryStream()) {
                    using(FileStream fs = new FileStream(Environment.CurrentDirectory + "\\output\\" + Path.GetFileName(file), FileMode.Create, FileAccess.ReadWrite)) {
                        image.Save(memory, ImageFormat.Png);
                        byte[] bytes = memory.ToArray();
                        fs.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }
    }
}
