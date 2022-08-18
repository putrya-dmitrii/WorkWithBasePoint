#region Namespaces
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#endregion

namespace WorkWithBasePoint
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
            {
                const string RIBBON_TAB = "ООО Феррострой";
                const string RIBBON_PANEL = "BIM Tools";
                //Add a new ribbon tab
                try
                {
                    application.CreateRibbonTab(RIBBON_TAB);
                }
                catch { }
                // Add a new ribbon panel
                RibbonPanel panel = null;
                List<RibbonPanel> panels = application.GetRibbonPanels(RIBBON_TAB);
                foreach (RibbonPanel rbnPnl in panels)
                {
                    if (rbnPnl.Name == RIBBON_PANEL)
                    {
                        panel = rbnPnl;
                        break;
                    }
                }
                if (panel == null)
                    panel = application.CreateRibbonPanel(RIBBON_TAB, RIBBON_PANEL);
                // Get the image for the button
                Image imgSmall = Properties.Resources.basePoint24;
                ImageSource imgSrcS = GetImageSource(imgSmall);
                Image imgLarge = Properties.Resources.basePoint32;
                ImageSource imgSrcL = GetImageSource(imgLarge);
                // Create a push button to trigger a command add it to the ribbon panel.
                PushButtonData btnData = new PushButtonData("cmdGetBasePoint",
                                                            "Get Base Point",
                                                            Assembly.GetExecutingAssembly().Location,
                                                            "WorkWithBasePoint.GetBasePoint")
                // Optionally, other properties may be assigned to the button
                {
                    ToolTip = "Получение базовой точки проекта",
                    LongDescription = "Получение базовой точки проекта",
                    Image = imgSrcS,
                    LargeImage = imgSrcL
                };
                PushButton button = panel.AddItem(btnData) as PushButton;

            ////*************************************************************

            //imgSmall = Properties.Resources.moveBasePoint24;
            //imgSrcS = GetImageSource(imgSmall);
            //imgLarge = Properties.Resources.moveBasePoint32;
            //imgSrcL = GetImageSource(imgLarge);
            //// Create a push button to trigger a command add it to the ribbon panel.
            //btnData = new PushButtonData("cmdMoveLinkBasePoint",
            //                                            "Move Link Base Point",
            //                                            Assembly.GetExecutingAssembly().Location,
            //                                            "WorkWithBasePoint.ChangeLinksBP")
            //// Optionally, other properties may be assigned to the button
            //{
            //    ToolTip = "Изменение координаты базовой точки ссылки",
            //    LongDescription = "Изменение координаты базовой точки ссылки",
            //    Image = imgSrcS,
            //    LargeImage = imgSrcL
            //};
            //button = panel.AddItem(btnData) as PushButton;

            ////*************************************************************

            //imgSmall = Properties.Resources.test24;
            //imgSrcS = GetImageSource(imgSmall);
            //imgLarge = Properties.Resources.test32;
            //imgSrcL = GetImageSource(imgLarge);
            //// Create a push button to trigger a command add it to the ribbon panel.
            //btnData = new PushButtonData("cmdTestBP",
            //                                "Test button",
            //                                Assembly.GetExecutingAssembly().Location,
            //                                "WorkWithBasePoint.BasePointProp")
            //// Optionally, other properties may be assigned to the button
            //{
            //    ToolTip = "Тестовый метод",
            //    LongDescription = "Тестовый метод",
            //    Image = imgSrcS,
            //    LargeImage = imgSrcL
            //};
            //button = panel.AddItem(btnData) as PushButton;

            //IList<RibbonItem> stackPanel = panel.AddStackedItems(btnData, btnData, btnData) ;
            return Result.Succeeded;
            }

            public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        private BitmapSource GetImageSource(Image img)
        {
            BitmapImage bmp = new BitmapImage();
            using (MemoryStream ms = new MemoryStream())
            {
                img.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                bmp.BeginInit();

                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = null;
                bmp.StreamSource = ms;
                bmp.EndInit();
            }
            return bmp;
        }

    }
}
