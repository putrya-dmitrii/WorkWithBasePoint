#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#endregion

namespace WorkWithBasePoint
{
    [Transaction(TransactionMode.Manual)]
    public class ChangeLinksBP : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            double currentBP_X = 0;
            double currentBP_Y = 0;
            double currentBP_Elevetion = 0;

            
            // Access current selection 

            //Selection sel = uidoc.Selection;
            //string result = "";

            //Get current BasePoint Coord's
            ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ProjectBasePoint);
            FilteredElementCollector fec = new FilteredElementCollector(doc)
                                                    .OfCategory(BuiltInCategory.OST_ProjectBasePoint);
            IList<Element> basePointElements = fec.ToElements();
            XYZ newLinkPosition = null;
            BasePoint openFileBP = new FilteredElementCollector(doc).OfClass(typeof(BasePoint)).Cast<BasePoint>().FirstOrDefault(x => x.IsShared);
            if (openFileBP != null)
            {
                newLinkPosition = new XYZ(openFileBP.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble(),
                    openFileBP.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble(),
                    openFileBP.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble());
                //string result = $"X: {locationPointBP.Point.X}\t Y: {locationPointBP.Point.Y}\t Z: {locationPointBP.Point.Z}\n";
                //result += $"EW: {openFileBP.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble()} \t";
                //result += $"NS: {openFileBP.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble()} \t";
                //result += $"Elevetion: {openFileBP.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble()}";

                //TaskDialog.Show("revit", result);
                //return Result.Succeeded;
            }
            else
            {
                return Result.Failed;
            }
            

            //foreach (Element item in basePointElements)
            //{
            //    currentBP_X = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
            //    currentBP_Y = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
            //    currentBP_Elevetion = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
            //}
            //result = $"Текущее значение Базовой точки \nX : {currentBP_X} Y : {currentBP_Y} Elevation : {currentBP_Elevetion}\n";
            FilteredElementCollector colls = new FilteredElementCollector(doc)
                                                    .OfCategory(BuiltInCategory.OST_RvtLinks)
                                                    .OfClass(typeof(Instance));
            IList<RevitLinkInstance> links = colls.Cast<RevitLinkInstance>().ToList();
            if (links == null)
            {
                TaskDialog.Show("revit", "Not find linked documents");
                return Result.Failed;
            }

            foreach (var link in links)
            {
                Location linkPosition = link.Location;


                Document currentDoc = link.GetLinkDocument();
                FilteredElementCollector collector = new FilteredElementCollector(currentDoc)
                                                    .OfCategory(BuiltInCategory.OST_ProjectBasePoint);
                IList<Element> elems = collector.ToElements();

                //int i = currentDoc.PathName.LastIndexOf("\\") + 1;
                //string name = currentDoc.PathName.Substring(i);
                ////result += currentDoc.PathName + "\n";

                foreach (Element element in elems)
                {
                    double x = UnitUtils.ConvertFromInternalUnits(element.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
                    double y = UnitUtils.ConvertFromInternalUnits(element.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
                    double elevation = UnitUtils.ConvertFromInternalUnits(element.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
                    TaskDialog.Show("Old coords", x + "\t" + y  + "\t" + elevation);
                    //    if ((x!=currentBP_X)||(y!=currentBP_Y)||(elevation!=currentBP_Elevetion))
                    //    {
                    //        result += name + "\n";
                    //        result += $"X : {x} Y : {y} Elevation : {elevation}\n";
                    //    }
                    //}
                    //result += "\n";
                }

                // Modify document within a transaction
                bool result = linkPosition.Move(newLinkPosition);
                if (!result)
                {
                    throw new Exception("Move link location failed.");
                }

                //using (Transaction tx = new Transaction(doc))
                //{
                //    tx.Start("Move linked file");

                //    tx.Commit();
                //}
            }



            return Result.Succeeded;
        }

    }

    //public static class LinksHelper 
    //{
    //    public static Dictionary<ElementId, string> GetLinkedFilePaths(Application app, bool onlyImportedFiles)
    //    {
    //        DocumentSet docs = app.Documents;
    //        int n = docs.Size;

    //        Dictionary<string, string> dict
    //          = new Dictionary<string, string>(n);

    //        foreach (Document doc in docs)
    //        {
    //            if (!onlyImportedFiles
    //              || (null == doc.ActiveView))
    //            {
    //                string path = doc.PathName;
    //                //int i = path.LastIndexOf("\\") + 1;
    //                //string name = path.Substring(i);
    //                //dict.Add(name, path);
    //            }
    //        }
    //        return dict;
    //    }
    //}
}
