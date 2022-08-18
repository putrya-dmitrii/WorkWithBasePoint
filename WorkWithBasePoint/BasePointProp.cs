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
    public class BasePointProp : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            string result = "";
            result = GetProjectLocationInfo(doc);
            var baseProjectPoint = GetBasePoint(doc, false);
            result += GetBasePointInfo(baseProjectPoint);
            var surveyProjectPoint = GetBasePoint(doc, true);
            result += GetBasePointInfo(surveyProjectPoint);
            TaskDialog.Show("Revit", result);
            //var linkInstance = uidoc
            //            .Selection
            //            .GetElementIds()
            //            .Select(doc.GetElement)
            //            .OfType<RevitLinkInstance>()
            //            .FirstOrDefault();
            var linkInstance = GetRevitLinkInstance(doc);

            if (linkInstance == null)
                return Result.Cancelled;
            var linkDocument = linkInstance.GetLinkDocument();
            result = "";
            result = GetProjectLocationInfo(linkDocument);
            baseProjectPoint = GetBasePoint(linkDocument, false);
            result += GetBasePointInfo(baseProjectPoint);
            surveyProjectPoint = GetBasePoint(linkDocument, true);
            result += GetBasePointInfo(surveyProjectPoint);
            TaskDialog.Show("Revit", result);
            //var sharedSiteId = new LinkElementId(linkInstance.Id, FindProjectLocation(linkInstance.GetLinkDocument()));

            //using (var transaction = new Transaction(doc, "publish coordinates"))
            //{
            //    transaction.Start();

            //    doc.PublishCoordinates(sharedSiteId);

            //    transaction.Commit();
            //} 

            return Result.Succeeded;
        }

        private static ElementId FindProjectLocation(Document document)
        {
            var collector = new FilteredElementCollector(document);

            return collector
                    .OfClass(typeof(ProjectLocation))
                    .FirstElementId();
        }

        public List<RevitLinkInstance> GetRevitLinkInstances(Document doc)
        { 
            return new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_RvtLinks)
                        .OfClass(typeof(Instance))
                        .Cast<RevitLinkInstance>()
                        .ToList();
        }

        public RevitLinkInstance GetRevitLinkInstance(Document doc)
        {
            return new FilteredElementCollector(doc)
                        .OfCategory(BuiltInCategory.OST_RvtLinks)
                        .OfClass(typeof(Instance))
                        .Cast<RevitLinkInstance>()
                        .FirstOrDefault();
        }

        //Recieve BasePoint if isSurvey == true  return survey point, else return base point
        public BasePoint GetBasePoint(Document doc, bool isSurvey)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(BasePoint))
                .Cast<BasePoint>()
                .Where(x => x.IsShared==isSurvey)
                .FirstOrDefault();
        }

        public XYZ GetBasePointCoords(BasePoint basePoint)
        {
            return new XYZ( basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble(),
                basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble(),
                basePoint.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble()
                );
        }

        public string GetBasePointInfo(BasePoint basePoint)
        {
            
            string result = $"\n\n{basePoint.Category.Name}\n";
             result += $"Parameters value :\n" +
                        $"WE_direction {FootToMillimeter(basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble())}\n" +
                        $"NS_direction {FootToMillimeter(basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble())}\n" +
                        $"Elevation {FootToMillimeter(basePoint.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble())}\n";
            Parameter angle = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM);
            if (angle != null)
            {
                result += $"{UnitUtils.ConvertFromInternalUnits(angle.AsDouble(),DisplayUnitType.DUT_DEGREES_AND_MINUTES)}\n";
            }
            XYZ itemBoundinBox = basePoint.get_BoundingBox(null).Max;
            if (itemBoundinBox != null)
                result += $"BoundingBox coords :\n " +
                    $"X : { FootToMillimeter(itemBoundinBox.X)} \n" +
                    $"Y : { FootToMillimeter(itemBoundinBox.Y)} \n" +
                    $"Z : { FootToMillimeter(itemBoundinBox.Z)} \n";
            return result;
        }


        public string GetProjectLocationInfo(Document doc)
        {

            ProjectLocation projectLocation = doc.ActiveProjectLocation;

            // Show the information of current project location
            XYZ origin = new XYZ(0, 0, 0);
            ProjectPosition position = projectLocation.GetProjectPosition(origin);
            if (null == position)
            {
                throw new Exception("No project position in origin point.");
            }

            // Format the prompt string to show the message.
            String prompt = "Project location information:\n";
            prompt += $"Document name : {doc.PathName.Substring(doc.PathName.LastIndexOf("\\") + 1)}\n";
            prompt += $"Shared coords name : {projectLocation.Name} \n";
            prompt += "\t" + "Origin point position:";
            prompt += "\n\t\t" + "Angle: " + UnitUtils.ConvertFromInternalUnits(position.Angle, DisplayUnitType.DUT_DEGREES_AND_MINUTES);
            prompt += "\n\t\t" + "East to West offset: " + FootToMillimeter(position.EastWest);
            prompt += "\n\t\t" + "North to South offset: " + FootToMillimeter(position.NorthSouth);
            prompt += "\n\t\t" + "Elevation: " + FootToMillimeter(position.Elevation);

            // Angles are in radians when coming from Revit API, so we 
            // convert to degrees for display
            const double angleRatio = Math.PI / 180;   // angle conversion factor

            SiteLocation site = projectLocation.GetSiteLocation();
            prompt += "\n\t" + "Site location:";
            prompt += "\n\t\t" + "Latitude: " + site.Latitude / angleRatio + "��";
            prompt += "\n\t\t" + "Longitude: " + site.Longitude / angleRatio + "��";
            prompt += "\n\t\t" + "TimeZone: " + site.TimeZone;

            return prompt;
        }

        public double FootToMillimeter(double inFoot)
        {
            return UnitUtils.ConvertFromInternalUnits(inFoot, DisplayUnitType.DUT_MILLIMETERS);
        }
    }
}
