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
    public class GetBasePoint : IExternalCommand
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
            string result = "";

            //Get current BasePoint Coord's
            //ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_ProjectBasePoint);
            FilteredElementCollector fec = new FilteredElementCollector(doc)
                                                    .OfCategory(BuiltInCategory.OST_ProjectBasePoint);
            IList<Element> basePointElements = fec.ToElements();

            foreach (Element item in basePointElements)
            {
                currentBP_X = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
                currentBP_Y = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
                currentBP_Elevetion = UnitUtils.ConvertFromInternalUnits(item.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
            }
            result = $"Текущее значение Базовой точки \nX : {Math.Round(currentBP_X,0)} Y : {Math.Round(currentBP_Y, 0)} Elevation : {Math.Round(currentBP_Elevetion, 0)}\n";
            TaskDialog.Show("Revit", result);
            FilteredElementCollector colls = new FilteredElementCollector(doc)
                                                    .OfCategory(BuiltInCategory.OST_RvtLinks)
                                                    .OfClass(typeof(Instance));
            IList<RevitLinkInstance> links = colls.Cast<RevitLinkInstance>().ToList();
            if (links == null)
            {
                TaskDialog.Show("revit", "Не найдены прикрепленные файлы");
                return Result.Failed;
            }
            TaskDialog.Show("Количество прикрепленных файлов", $"Найдено {links.Count.ToString()} ссылок");
                                                    
            foreach (var link in links)
            {
                Document currentDoc = link.GetLinkDocument();
                if ((currentDoc!=null) && (currentDoc is Document))
                {
                    IList<Element> elems = new FilteredElementCollector(currentDoc)
                        .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
                        .ToElements();

                    int i = currentDoc.PathName.LastIndexOf("\\") + 1;
                    string name = currentDoc.PathName.Substring(i);
                    //result += currentDoc.PathName + "\n";
                    if (elems != null)
                    {
                        foreach (Element element in elems)
                        {
                            double x = UnitUtils.ConvertFromInternalUnits(element.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
                            double y = UnitUtils.ConvertFromInternalUnits(element.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
                            double elevation = UnitUtils.ConvertFromInternalUnits(element.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble(), DisplayUnitType.DUT_MILLIMETERS);
                            if ((x != currentBP_X) || (y != currentBP_Y) || (elevation != currentBP_Elevetion))
                            {
                                result += name + "\n";
                                result += $"X : {Math.Round(x, 0)} Y : {Math.Round(y, 0)} Elevation : {Math.Round(elevation, 0)}\n";
                            }
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Revit", $"{name} - Не найдена базовая точка !");
                    }
                    result += "\n";
                }
            }

            TaskDialog.Show("revit",result);
            // Modify document within a transaction

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");
                tx.Commit();
            }
            return Result.Succeeded;
        }
    }
}
