using System;
using System.Linq;
using DeSuPatcher.Core;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DeSuPatcher.Controllers
{
    public class HomeController : Controller
    {
        public static Main Core { get; set; }
        public static string Folder { get; set; }

        // GET: HomeController
        public ActionResult Index()
        {
            Core = new Main();
            if (HybridSupport.IsElectronActive)
            {
                Electron.IpcMain.On("select-rom", async (args) => {
                    var mainWindow = Electron.WindowManager.BrowserWindows.First();
                    var options = new OpenDialogOptions
                    {
                        Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openFile
                        },
                        Filters = new []{new FileFilter {Extensions = new []{"nds"},Name = "Nintendo DS ROM"}}
                    };

                    string[] files = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                    Electron.IpcMain.Send(mainWindow, "select-rom-reply", files);
                });

                Electron.IpcMain.On("open-file", async (args) =>
                {
                    await Electron.Shell.ShowItemInFolderAsync(Folder);

                });
            }
            return View(this);
        }

        public ActionResult CheckGame(string file)
        {
            return Json(new {check = Core.CheckGame(file)});
        }

        public ActionResult PatchGame(string file)
        {
            var result = Core.PatchGame(file);
            Folder = file;
            return Json(new { check = result });
        }

        public ActionResult CheckInternet()
        {
            return Json(new { check = Core.Internet });
        }
    }
}
