using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task2
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDocument = uiApp.ActiveUIDocument;
            Document doc = uiDocument.Document;
             
        
            try
            {
                var refGroup = uiDocument.Selection.PickObject(ObjectType.Element, new GroupFilter(), "Выберите группу");
                Group group = doc.GetElement(refGroup) as Group;
                XYZ groupCenter = GetElementCenter(group);
                Room room = GetRoomByPoint(doc, groupCenter);
                XYZ roomCenter = GetElementCenter(room);
                XYZ offset = groupCenter - roomCenter;
                XYZ newPointroom = uiDocument.Selection.PickPoint("Выберите точку внутри комнаты");

                Room selectedRoom = GetRoomByPoint(doc, newPointroom);
                XYZ selectedRoomCenter = GetElementCenter(selectedRoom);


                XYZ newCenterPoint = selectedRoomCenter + offset;
                using (var ts = new Transaction(doc, "copy group"))
                {
                    ts.Start();
                    doc.Create.PlaceGroup(newCenterPoint, group.GroupType);
                    ts.Commit();
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException e)
            {
                return Result.Cancelled;
            }
            catch (Exception e) {
                message = e.Message;
                return Result.Failed;
            }

            


            return Result.Succeeded;
        }


        private Room GetRoomByPoint(Document doc, XYZ xyz)
        {
            FilteredElementCollector elements = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms);
            foreach (Element element in elements)
            {
                Room room = element as Room;
                if (room != null)
                {
                    if (room.IsPointInRoom(xyz)) return room;

                }
            }
            return null;
        }
        private XYZ GetElementCenter(Element element)
        {
            var bounding = element.get_BoundingBox(null);
            return (bounding.Max + bounding.Min) / 2;
        }
        private class GroupFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return (elem is Group);
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return true;

            }

        }
    }

}
