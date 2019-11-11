using AutoFixture;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FrequentItemFinder
{
    /**
     * A simple class to hold one data item and the count of
     * how many times it has occurred
     */
    public class ItemCount
    {
        private String item;
        private int count;
        public ItemCount(String item, int count)
        {
            this.item = item;
            this.count = count;
        }
        public ItemCount(String item)
        {
            this.item = item;
            this.count = 0;
        }
        public String getItem() { return item; }
        public int getCount() { return count; }
        public void increment() { count += 1; }
    }

    /**
     * Take an array of items and return an array of unique items 
     * with counts
     */
    public interface ItemCounter
    {
        /**
         * Analyze the list, figure out the number of times
         * each unique data item appears in the list, and return
         * a list of ItemCount objects corresponding to each
         * unique data item and the number of times it occurs
         * in the list
         */
        List<ItemCount> analyze(List<String> list);
        Dictionary<string, ItemCount> analyzeDict(List<String> list);
    }

    /**
     * An implementation of ItemCounter interface which
     * sorts the input list using Collections.sort and then
     * counts the number of occurrences of each item by
     * making one pass through the sorted list
     */
    public class SortBasedItemCounter : ItemCounter
    {
        public SortBasedItemCounter() { }
        public List<ItemCount> analyze(List<String> list)
        {
            list.Sort();
            List<ItemCount> icList = new List<ItemCount>();
            String prev = null;
            ItemCount counter = null;
            foreach (var s in list)
            {
                if (s.Equals(prev))
                {
                    // Same item. Just increment the count
                    counter.increment();
                }
                else
                {
                    // Item changed. Add previous item count to icList
                    if (counter != null)
                        icList.Add(counter);
                    counter = new ItemCount(s, 1);
                    prev = s;
                }
            }
            // The last item has not yet been added to the list
            if (counter != null)
            {
                icList.Add(counter);
            }
            return icList;
        }

        public Dictionary<string, ItemCount> analyzeDict(List<string> list)
        {
            throw new NotImplementedException();
        }
    }

    public class DictionaryBasedItemCounter : ItemCounter
    {
        public DictionaryBasedItemCounter() { }
        public Dictionary<string, ItemCount> analyzeDict(List<String> list)
        {
            list.Sort();
            Dictionary<string, ItemCount> icList = new Dictionary<string, ItemCount>();
            String prev = null;
            ItemCount counter = null;
            foreach (var s in list)
            {
                if (s.Equals(prev))
                {
                    // Same item. Just increment the count
                    counter.increment();
                }
                else
                {
                    // Item changed. Add previous item count to icList
                    if (counter != null)
                        icList.Add(s+"_"+counter.getCount().ToString(),counter);
                    counter = new ItemCount(s, 1);
                    prev = s;
                }
            }
            // The last item has not yet been added to the list
            if (counter != null)
            {
                icList.Add(counter.getItem() + "_" + counter.getCount().ToString(), counter);
            }
            return icList;
        }

        public List<ItemCount> analyze(List<string> list)
        {
            throw new NotImplementedException();
        }

    }

    /**
     * A class that allows us to choose between different
     * implementations of the ItemCounter interface based on the name
     * All the different implementations must be registered with this
     * class before they can be used.
     */
    public class ItemCounterChooser
    {
        Dictionary<String, Type> registeredCounters =
            new Dictionary<String, Type>();
        public void register(String name, Type counterType)
        {
            registeredCounters.Add(name, counterType);
        }

        /**
         * Return an instance of the ItemCounter class that was
         * registered using `name`
         * Return null if `name` was not registered at all.
         */
        public ItemCounter getItemCounterInstance(String name)
        {
            Type icType = registeredCounters[name];
            if (icType == null)
                return null;
            return (ItemCounter)Activator.CreateInstance(icType);
        }
    }

    private ItemCounterChooser itemCounterChooser;
    private ItemCounter itemCounter;

    public FrequentItemFinder()
    {
        itemCounterChooser = new ItemCounterChooser();
        itemCounter = null;
        itemCounterChooser.register("SortBased",
                                    typeof(SortBasedItemCounter));
        itemCounterChooser.register("DictionaryBased",
                                    typeof(DictionaryBasedItemCounter));
    }

    public void selectItemCounterChooser(String itemCounterName)
    {
        itemCounter = itemCounterChooser.getItemCounterInstance(
                                                 itemCounterName);
    }

    /**
     * Find which string appears the most number of times in list
     * Use the ItemCounter to get a list of name,count pairs
     * and then find which one appears the maximum number of times.
     * In case of a tie, pick any one randomly
     */
    public String findFrequentString(List<String> list)
    {
        if (itemCounter == null)
            selectItemCounterChooser("SortBased");
        int max = -1;
        String frequentItem = null;
        foreach (var icount in itemCounter.analyze(list))
        {
            if (icount.getCount() > max)
            {
                max = icount.getCount();
                frequentItem = icount.getItem();
            }
        }
        return frequentItem;
    }

    public Dictionary<int,string> findFrequentDictString(List<String> list)
    {
        if (itemCounter == null)
            selectItemCounterChooser("DictionaryBased");
        int max = -1;
        String frequentItem = null;
        Dictionary<int, string> dictLst = new Dictionary<int, string>();
        foreach (var icount in itemCounter.analyzeDict(list).Values)
        {
            if (icount.getCount() > max)
            {
                max = icount.getCount();
                frequentItem = icount.getItem();
            }
        }
        dictLst.Add(max, frequentItem);
        return dictLst;
    }
    /**
     * A simple program that find which of the names in the `names`
     * array occurs the maximum number of times
     */
    public static void Main(String[] args)
    {
        FrequentItemFinder frequentItemFinder =
            new FrequentItemFinder();
        List<String> names = new List<String>{
                          "Navin Kabra",
                          "Amit Paranjape",
                          "Navin Kabra",
                          "Amit Paranjape1",
                          "Navin Kotkar",
                          "Gaurav Kotkar"};
        String f = string.Empty;
        Dictionary<int,string> g;
        if (names.Count <= 50)
        {
            f = frequentItemFinder.findFrequentString(names);
            Console.WriteLine("The most frequent name is: " + f);
        }
        else
        {
            g = frequentItemFinder.findFrequentDictString(names);
            Console.WriteLine(string.Format("The most frequent name count: {0} , The most frequent name is: {1}", g.Keys.ToList()[0], g.Values.ToList()[0]));
        } 
        Console.ReadLine();
    }
}