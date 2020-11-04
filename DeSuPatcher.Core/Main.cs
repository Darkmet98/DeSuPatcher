using System;
using System.IO;
using System.Net;
using DeSuPatcher.Core.FileManipulation;
using Newtonsoft.Json;

namespace DeSuPatcher.Core
{
    public class Main
    {
        // URLs for download the patch and version.json
        private string versionCheckUrl = "https://nekutranslations.es/desu/patch/patch.json";
        private string fileDownloadUrl = "https://nekutranslations.es/desu/patch/desu-esp.xdelta";
        private string fileCleanDownloadUrl = "https://nekutranslations.es/desu/patch/clean.xdelta";

        // Core variables
        private string temp;

        public bool Internet { get; }

        private Version localVersion;

        public Main()
        {
            Internet = CheckInternet();
            if (Internet)
                localVersion = GetVersionJson();
        }

        public bool CheckGame(string path)
        {
            if (!File.Exists(path))
                return false;

            var ndsMd5 = Md5.CalculateMd5(path);
            if (localVersion.Md5[0] == ndsMd5)
                return true;

            if (localVersion.Md5[1] == ndsMd5)
                return true;


            return false;
        }

        public bool PatchGame(string path)
        {
            try
            {
                FileManipulation.Internet.GetFile(fileDownloadUrl, "patch.xdelta", temp);
                var folder = Path.GetDirectoryName(path);
                var result = $"{folder}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(path)}-ES.nds";
                var ndsMd5 = Md5.CalculateMd5(path);

                if (localVersion.Md5[1] == ndsMd5)
                {
                    var tempFile = $"{temp}{Path.DirectorySeparatorChar}clean.nds";
                    FileManipulation.Internet.GetFile(fileCleanDownloadUrl, "clean.xdelta", temp);
                    FileManipulation.Xdelta.Apply(new FileStream(path, FileMode.Open),
                        File.ReadAllBytes($"{temp}{Path.DirectorySeparatorChar}clean.xdelta"),
                        tempFile
                    );
                    var stream = new FileStream(tempFile, FileMode.Open);
                    FileManipulation.Xdelta.Apply(stream,
                        File.ReadAllBytes($"{temp}{Path.DirectorySeparatorChar}patch.xdelta"),
                        result
                    );
                    stream.Dispose();
                    File.Delete(tempFile);
                }
                else
                {
                    FileManipulation.Xdelta.Apply(new FileStream(path, FileMode.Open),
                        File.ReadAllBytes($"{temp}{Path.DirectorySeparatorChar}patch.xdelta"),
                        result
                    );
                }

                

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }

        private Version GetVersionJson()
        {
            GenerateTempFolder();
            FileManipulation.Internet.GetFile(versionCheckUrl, "version.json", temp);
            return JsonConvert.DeserializeObject<Version>(
                File.ReadAllText($"{temp}{Path.DirectorySeparatorChar}version.json"));
        }

        /// <summary>
        /// Genera una carpeta temporal en %temp%
        /// </summary>
        private void GenerateTempFolder()
        {
            temp = Path.GetTempPath() + Path.DirectorySeparatorChar + Path.GetRandomFileName();
            Directory.CreateDirectory(temp);
        }

        /// <summary>
        /// Comprueba si hay internet
        /// </summary>
        private bool CheckInternet()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("https://nekutranslations.es/"))
                    return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
