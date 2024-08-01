using StardewValley;
using System.Collections;

namespace AutoTrash2.Data;

/// <summary>
/// Holds recently-trashed items for short-term recovery. Not a real "bin", just temporary data storage.
/// </summary>
internal class RecoveryBin(Func<TimeSpan> maxAge)
{
    private readonly Func<TimeSpan> maxAge = maxAge;

    class Slot(Item item)
    {
        public TimeSpan Age { get; set; }
        public Item Item { get; } = item;
    }

    private readonly List<Slot> slots = [];

    public void Add(Item item)
    {
        if (maxAge() == TimeSpan.Zero)
        {
            return;
        }
        int remainingCount = item.Stack;
        for (int i = 0; i < slots.Count && remainingCount > 0; i++)
        {
            var itemInSlot = slots[i].Item;
            if (!item.canStackWith(itemInSlot))
            {
                continue;
            }
            var availableStack = itemInSlot.maximumStackSize() - itemInSlot.Stack;
            if (availableStack > 0)
            {
                var additionalStack = Math.Min(availableStack, item.Stack);
                itemInSlot.Stack += additionalStack;
                remainingCount -= additionalStack;
                slots[i].Age = TimeSpan.Zero;
            }
        }
        if (remainingCount > 0)
        {
            item.Stack = remainingCount;
            slots.Insert(0, new(item));
        }
    }

    public void Clear()
    {
        slots.Clear();
    }

    public IList<Item> GetItems()
    {
        slots.Sort((a, b) => a.Age.CompareTo(b.Age));
        return new ItemListAdapter(slots);
    }

    public bool IsEmpty()
    {
        return slots.Count == 0;
    }

    public void Trim(TimeSpan elapsed)
    {
        // We could do a lot of work to make this a sorted/LRU type data structure with highly optimized removals.
        // Practically, it's probably useless since the list should have no more than a handful of items.
        var maxAge = this.maxAge();
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if ((slots[i].Age += elapsed) > maxAge)
            {
                slots.RemoveAt(i);
            }
        }
    }

    class ItemListAdapter(IList<Slot> slots) : IList<Item>
    {
        public Item this[int index]
        {
            get => slots[index].Item;
            set
            {
                if (value is null)
                {
                    slots.RemoveAt(index);
                }
                else
                {
                    throw new InvalidOperationException("Trashed items cannot be changed");
                }
            }
        }

        public int Count => slots.Count;

        public bool IsReadOnly => false;

        public void Add(Item item)
        {
            throw new InvalidOperationException("Items cannot be directly added to recently-trashed list.");
        }

        public void Clear()
        {
            slots.Clear();
        }

        public bool Contains(Item item)
        {
            return slots.Any(slot => slot.Item == item);
        }

        public void CopyTo(Item[] array, int arrayIndex)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                array[arrayIndex + i] = slots[i].Item;
            }
        }

        public IEnumerator<Item> GetEnumerator()
        {
            return slots.Select(slot => slot.Item).GetEnumerator();
        }

        public int IndexOf(Item item)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].Item == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, Item item)
        {
            throw new InvalidOperationException("Items cannot be directly inserted into recently-trashed list.");
        }

        public bool Remove(Item item)
        {
            var index = IndexOf(item);
            if (index >= 0)
            {
                slots.RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            slots.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
