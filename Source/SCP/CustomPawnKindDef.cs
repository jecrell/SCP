using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SCP
{
    public enum ObjectClass
    {
        Safe,
        Euclid,
        Keter,
        Thaumiel,
        Neutralized
    }


    public static class ObjectClassUtility
    {
        public static string GetObjectClassString(this ObjectClass objectClass)
        {
            return ("SCP_Class" + objectClass.ToString()).Translate();
        }

        public static string GetObjectClassDesc(this ObjectClass objectClass)
        {
            return ("SCP_Class" + objectClass.ToString() + "Desc").Translate();
        }
    }

    public class CustomPawnKindDef : Verse.PawnKindDef
    {
        public bool isUnique = false;
        public bool isSCP = true;        public bool trainableMechanoid = false;
        public ObjectClass objectClass = ObjectClass.Safe;
    }
}
