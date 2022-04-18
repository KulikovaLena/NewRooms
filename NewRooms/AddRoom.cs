using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewRooms
{
    [TransactionAttribute(TransactionMode.Manual)]

    public class AddRoom : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            List<Level> levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .OfType<Level>()
                .ToList();

            FamilySymbol familySymbol = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_RoomTags)
                .OfType<FamilySymbol>()
                .Where(x=>x.FamilyName.Equals("номерЭтажа_номерПомещения"))
                .FirstOrDefault();
            if(familySymbol==null)
            {
                TaskDialog.Show("Ошибка", "Не найдено семейство \"номерЭтажа_номерПомещения\"");
                return Result.Cancelled;
            }

            Transaction transaction0 = new Transaction(doc, "Create");
            transaction0.Start();
            if (!familySymbol.IsActive)
            {
                familySymbol.Activate();
            }
            transaction0.Commit();

            Transaction transaction = new Transaction(doc, "Create");
            transaction.Start();

            for (int i = 0; i < levels.Count; i++)
            {
                PlanTopology pt = doc.get_PlanTopology(levels[i]);

                foreach (PlanCircuit pc in pt.Circuits)
                {
                    if (!pc.IsRoomLocated)
                    {
                        Room r = doc.Create.NewRoom(null, pc);
                        //XYZ roomCenter = GetElementCenter(r);
                        //FamilyInstance roomTag = doc.Create.NewFamilyInstance(roomCenter, familySymbol, levels[i], StructuralType.NonStructural);
                    }
                }
            }

            transaction.Commit();

            return Result.Succeeded;
        }

        public XYZ GetElementCenter(Room room)
        {
            BoundingBoxXYZ bounding = room.get_BoundingBox(null);
            return (bounding.Max + bounding.Min)/2;
        }
    }
}
