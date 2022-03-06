using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
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
            Group group;
            XYZ point;
            try
            {
                var refGroup = uiDocument.Selection.PickObject(ObjectType.Element, new GroupFilter(), "Выберите группу");
                  group = doc.GetElement(refGroup) as Group;
                  point = uiDocument.Selection.PickPoint("Выберите точку вставки");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException e) {
                return Result.Cancelled;
            }
            
            using (var ts = new Transaction(doc, "copy group"))
            {
                ts.Start();
                doc.Create.PlaceGroup(point, group.GroupType);
                ts.Commit();
            }


            return Result.Succeeded;
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
