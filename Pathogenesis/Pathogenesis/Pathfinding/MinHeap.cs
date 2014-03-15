using System;
using System.Collections.Generic;
using Pathogenesis.Pathfinding;

namespace Pathogenesis
{
    /// <summary>
    /// MinHeap from ZeraldotNet (http://zeraldotnet.codeplex.com/)
    /// Modified by Roy Triesscheijn (http://roy-t.nl)    
    /// -Moved method variables to class variables
    /// -Added English Exceptions and comments (instead of Chinese)    
    /// </summary>    
    public class MinHeap
    {        
        private PathNode listHead;

        public bool HasNext()
        {
            return listHead != null;
        }

        public void Add(PathNode item)
        {
            if (listHead == null)
            {
                listHead = item;
            }
            else if (listHead.next == null && item.Cost <= listHead.Cost)
            {
                item.next = listHead;
                listHead = item;
            }
            else
            {
                PathNode ptr = listHead;
                while (ptr.nextListElem != null && ptr.nextListElem.cost < item.cost)
                    ptr = ptr.nextListElem;
                item.nextListElem = ptr.nextListElem;
                ptr.nextListElem = item;
            }
        }

        public PathNode ExtractFirst()
        {
            PathNode result = listHead;
            listHead = listHead.nextListElem;
            return result;
        }
    }
}