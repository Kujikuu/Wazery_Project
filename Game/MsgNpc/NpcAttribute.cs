using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightConquer_Project.Game.MsgNpc
{
    public class NpcAttribute : Attribute
    {
        public static readonly Func<NpcAttribute, NpcID> Translator = (a) => a.Type;
        public NpcID Type { get; private set; }
        public NpcAttribute(NpcID type)
        {
            this.Type = type;
        }
    }
}
