using System.IO;
using Xdelta;

namespace DeSuPatcher.Core.FileManipulation
{
    public class Xdelta
    {
        public static void Apply(FileStream file, byte[] xdeltaFile, string outfile)
        {
            var outStream = new FileStream(outfile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            var xdelta = new MemoryStream(xdeltaFile);

            var decoder = new Decoder(file, xdelta, outStream);
            decoder.Run();
            outStream.Close();
        }
    }
}
