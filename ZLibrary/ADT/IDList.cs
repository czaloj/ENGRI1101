using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZLibrary.ADT
{
    public class IDList<T> where T : class, IIDHolder
    {
        //Array Of All The Objects
        protected T[] obj;
        public T this[long ID]
        {
            get
            {
                return getObj(ID);
            }
        }
        public int lastID = 0;

        protected int count;
        public int Count
        {
            get
            {
                return count;
            }
        }

        //Amount For Array To Be Resized Every Time
        protected int resizeAmount;

        //List Of ID's To Be Reused
        protected List<long> recycledIDs;

        public IDList(int initialLength = 10, int resizeAmount = 5)
        {
            count = 0;
            if (initialLength < 5)
            {
                obj = new T[5];
            }
            else
            {
                obj = new T[initialLength];
            }
            recycledIDs = new List<long>(4);
            if (resizeAmount < 5)
            {
                this.resizeAmount = 5;
            }
            else
            {
                this.resizeAmount = resizeAmount;
            }
        }

        public virtual T getObj(long ID)
        {
            return obj[ID];
        }
        public long addObj(T o)
        {
            //Set Object ID
            long ID;
            if (recycledIDs.Count > 0)
            {
                ID = recycledIDs[0];
                recycledIDs.RemoveAt(0);
            }
            else
            {
                ID = lastID;
                lastID++;
            }
            o.setID(ID);

            //Put Object In Array
            if (obj.Length <= ID)
            {
                Array.Resize<T>(ref obj, obj.Length + 5);
            }
            obj[ID] = o;

            count++;

            //Return Object ID
            return ID;
        }
        public bool removeObj(long ID)
        {
            try
            {
                if (obj[ID] == null)
                {
                    return false;
                }
                else
                {
                    obj[ID] = null;
                    recycledIDs.Add(ID);
                    count--;
                    return true;
                }
            }
            catch (IndexOutOfRangeException)
            {
                //ID Not Valid
                return false;
            }
        }

        public void toArray(out T[] a)
        {
            a = new T[count];
            int ai = 0;
            for (int i = 0; i < obj.Length && ai < count; i++)
            {
                if (obj[i] != null)
                {
                    a[ai++] = obj[i];
                }
            }
        }
    }
}
