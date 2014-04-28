using System;
using System.Collections.Generic;
using System.Linq;

namespace PLG
{
    class Plg
    {
        public List<Person> Persons
        {
            get;
            private set;
        }

        public Plg(IList<Person> persons)
        {     
            Persons = new List<Person>(persons);

            foreach (Person person in persons)
            {
                person.Flagged = false;
                person.Distance = -1;
                person.Position = -1;
            }
        }

        /// <summary>
        /// Perform an LexBFS ordering on G
        /// </summary>
        /// <param name="direction">Direction of the ordering (forwards/backwards)</param>
        /// <returns>An order of G</returns>
        private Person[] LexBFS(Direction direction)
        {
            //Initialize data structures
            LinkedList<SetList<Person>> listOfSets = new LinkedList<SetList<Person>>();
            Dictionary<Person, LinkedListNode<SetList<Person>>> setPointers = new Dictionary<Person, LinkedListNode<SetList<Person>>>();
            Dictionary<Person, LinkedListNode<Person>> positionPointers = new Dictionary<Person, LinkedListNode<Person>>();

            //Set that have to be unflagged after the iteration
            List<LinkedListNode<SetList<Person>>> setsToCheck = new List<LinkedListNode<SetList<Person>>>();

            Person[] outputArray = new Person[this.Persons.Count];

            //Generate first set (represents all persons without label)
            SetList<Person> firstSet = new SetList<Person>();
            listOfSets.AddFirst(firstSet);

            //Get all persons ready
            foreach (Person person in Persons)
            {
                person.Position = -1;
                positionPointers[person] = firstSet.AddLast(person);
                setPointers[person] = listOfSets.First;
            }

            //Determine startindex depending on direction
            int i = (direction == Direction.Backwards ? this.Persons.Count - 1 : 0);

            //Label all persons
            while(direction == Direction.Backwards ? i >= 0 : i < this.Persons.Count)
            {
                //Select current person for labeling and remove it from its set
                Person currentPerson = listOfSets.Last.Value.First.Value;
                positionPointers.Remove(currentPerson);
                listOfSets.Last.Value.RemoveFirst();

                //Remove set if empty
                if (listOfSets.Last.Value.Count == 0)
                    listOfSets.Remove(listOfSets.Last);

                //Assign label
                currentPerson.Position = i;
                outputArray[i] = currentPerson;

                //Assign new sets to all neighbors
                foreach (Person neighbor in currentPerson.Neighbors)
                {
                    //Ignore labeled neighbors
                    if (neighbor.Position == -1)
                    {
                        //Get current set of neighbor
                        LinkedListNode<SetList<Person>> currentSet = setPointers[neighbor];

                        //If currentSet has no replacement generate replacement set generate one
                        if (!currentSet.Value.HasReplacement)
                        {
                            currentSet.Value.HasReplacement = true;
                            SetList<Person> newSet = new SetList<Person>();
                            listOfSets.AddAfter(currentSet, newSet);

                            setsToCheck.Add(currentSet);
                        }

                        //Put neighbor in a set one level higher
                        currentSet.Value.Remove(positionPointers[neighbor]); 
                        setPointers[neighbor] = currentSet.Next;
                        positionPointers[neighbor] = currentSet.Next.Value.AddLast(neighbor);
                    }
                }

                //Unflag all previously flagged set and delete them if empty
                foreach (LinkedListNode<SetList<Person>> setList in setsToCheck)
                {
                    if (setList.Value.Count == 0)
                        listOfSets.Remove(setList);

                    else
                        setList.Value.HasReplacement = false;
                }

                setsToCheck.Clear();

                i = i + (direction == Direction.Backwards ? -1 : 1);
            }
            return outputArray;
        }

        /// <summary>
        /// Test if this PLG is chordal
        /// </summary>
        /// <param name="output">If this PLG is not chordal, it returns an circle (in the form of an array with more than 3 elements)</param>
        /// <returns>Returns true if the graph is chordal</returns>
        private bool TestChordal(out Person[] output)
        {
            //Generate a potential perfect elimination order (PEO) with LexBFS
            Person[] potentialPEO = LexBFS(Direction.Backwards);
            Person[] triple;

            //If potentialPEO is a PEO the graph is chordal
            if (TestPerfectEleminationOrder(potentialPEO, out triple))
            {
                //Set the PEO as output
                output = potentialPEO;

                return true;
            }

            else
            {
                //Calculate a path from u, v and w leading to an chordless cycle with more then 3 vertices
                //Used as certificate because of being a potential smallest invalid structure

                //Uses LexBfs to label the node for finding a path
                Person[] pathOrder = LexBFSFindAPath(triple);

                //If adjacent lists are sorted the first node with the lower distance is always on the path between w and v
                SortAdjLists(pathOrder);

                //List representing the found cycle
                List<Person> cycle = new List<Person>();

                Person currentPerson = triple[1];

                while (currentPerson.Distance > 0)
                {
                    cycle.Add(currentPerson);

                    //Find first neighbor with a lower distance
                    foreach (Person neighbor in currentPerson.Neighbors)
                    {
                        if (neighbor.Distance < currentPerson.Distance)
                        {
                            currentPerson = neighbor;
                            break;
                        }
                    }
                }

                cycle.Add(triple[2]);
                cycle.Add(triple[0]);

                //Return the found cycle
                output = cycle.ToArray();

                return false;
            }
        }

        /// <summary>
        /// Test if a given order is a PEO
        /// </summary>
        /// <param name="order">The order to be tested</param>
        /// <returns>Trivial</returns>
        private bool TestPerfectEleminationOrder(Person[] order, out Person[] triple)
        {
            //Initialize data structures
            Dictionary<Person, List<Person>> calcuatedLists = new Dictionary<Person, List<Person>>();
            triple = new Person[3];
            SortAdjLists(order);

            foreach (Person person in Persons)
                calcuatedLists.Add(person, new List<Person>());

            for (int i = 0; i < Persons.Count; i++)
            {
                Person currentPerson = order[i];

                //List with the right neighbors of currentPerson (all neighbors with a higher label)
                List<Person> rightNeigbors = new List<Person>();
                //Adjacent lists are ordered -> All right neighbors are in the upper part of the neighbors
                int currentPos = currentPerson.Neighbors.Count - 1;
                while (currentPos >= 0 && currentPerson.Neighbors[currentPos].Position > currentPerson.Position)
                {
                    rightNeigbors.Add(currentPerson.Neighbors[currentPos]);
                    currentPos--;
                }

                if(rightNeigbors.Count > 1)
                {
                    //Find element with the smallest position (== latest labeled) in the right neighbors
                    int positionOfLeftmostElement = 0;
                    for (int a = 0; a < rightNeigbors.Count; a++)
                    {
                        if (rightNeigbors[positionOfLeftmostElement].Position > rightNeigbors[a].Position)
                            positionOfLeftmostElement = a;
                    }

                    //Add every right neighbor to calculated adjacent list of the leftmost element while avoiding the element itself
                    for (int a = 0; a < rightNeigbors.Count; a++)
                        if (a != positionOfLeftmostElement)
                            calcuatedLists[rightNeigbors[positionOfLeftmostElement]].Add(rightNeigbors[a]);
                }
            }

            //Check all elements for differ (backwards)
            //Used for getting the highest violating triple
            for (int i = Persons.Count - 1; i >= 0; i--)
            {
                System.Diagnostics.Debug.WriteLine(calcuatedLists[order[i]].Count.ToString());
                if (Differ(order[i].Neighbors, calcuatedLists[order[i]]) != -1)
                {
                    triple[1] = order[i]; //v is the person with the different adjacent lists
                    triple[2] = Persons[Differ(order[i].Neighbors, calcuatedLists[order[i]])]; //w is the odd element
                    break; //Nothing more to check
                }
            }

            if (triple[2] != null) //Not chordal
            {
                //Common elements of the neighbors of v and w
                //One of them is u
                List<Person> potentialUs = triple[1].Neighbors.Intersect(triple[2].Neighbors).ToList();

                //Search for the element with the smallest position (u)
                int positionOfCurrentU = 0;
                for (int i = 0; i < potentialUs.Count; i++)
                {
                    if (potentialUs[i].Position < potentialUs[positionOfCurrentU].Position)
                        positionOfCurrentU = i;
                }

                //u is the common neighbor of v and w with the lowest position
                triple[0] = potentialUs[positionOfCurrentU];

                return false;
            }

            return true; //Chordal
        }

        /// <summary>
        /// Tests if two adjacent lists differ
        /// </summary>
        /// <param name="originalAdjList">Original adjacent list</param>
        /// <param name="calculatedAdjList">Calculated (by Perfect()) adjacent list</param>
        /// <returns>-1 if the list don't differ or the odd element</returns>
        private int Differ(List<Person> originalAdjList, List<Person> calculatedAdjList)
        {
            //Flag all persons
            foreach (Person person in originalAdjList)
                person.Flagged = true;

            //Check for an element only contained in the calculated adjacent list
            foreach (Person person in calculatedAdjList)
                if (!person.Flagged)
                    return person.ID; //Return the odd element

            //Unflag all persons
            foreach (Person person in originalAdjList)
                person.Flagged = false;

            return -1; //The lists don't differ
        }

        /// <summary>
        /// Sort all adjacent list in linear time
        /// </summary>
        /// <param name="order">A order of the graph</param>
        private void SortAdjLists(Person[] order)
        {
            //Initialize the required data structures
            Dictionary<Person, List<Person>> listOfAdjLists = new Dictionary<Person, List<Person>>();
            foreach (Person person in Persons)
                listOfAdjLists.Add(person, new List<Person>());

            //Put the ordered persons in the new adjacent lists of their neighbors
            foreach (Person person in order)
                foreach (Person neighbor in person.Neighbors)
                    listOfAdjLists[neighbor].Add(person);

            //Overwrite the old adjacent lists with the new ones
            foreach (Person person in Persons)
                person.Neighbors = listOfAdjLists[person];
        }

        /// <summary>
        /// Generates a LexFS ordering of G used to find a path
        /// Used for finding an circle if G is not chordal
        /// </summary>
        /// <param name="triple">A triple containing u, v and w</param>
        /// <returns>An ordering leading to a path from v to w avoiding u</returns>
        private Person[] LexBFSFindAPath(Person[] triple)
        {
            //Initialize data structures
            LinkedList<SetList<Person>> listOfSets = new LinkedList<SetList<Person>>();
            Dictionary<Person, LinkedListNode<SetList<Person>>> setPointers = new Dictionary<Person, LinkedListNode<SetList<Person>>>();
            Dictionary<Person, LinkedListNode<Person>> positionPointers = new Dictionary<Person, LinkedListNode<Person>>();

            //Set that have to be unflagged after the iteration
            List<LinkedListNode<SetList<Person>>> setsToCheck = new List<LinkedListNode<SetList<Person>>>();

            Person[] outputArray = new Person[this.Persons.Count];

            //Set for all node without N(u)\{v,w}; w is at the first position
            SetList<Person> firstSet = new SetList<Person>();
            listOfSets.AddFirst(firstSet);

            //Set for N(u)\{v,w}; after the first set -> gets labeled last
            SetList<Person> secondSet = new SetList<Person>();
            listOfSets.AddFirst(secondSet);

            //Adds w so w gets labeled first
            positionPointers[triple[2]] = firstSet.AddFirst(triple[2]);
            setPointers[triple[2]] = listOfSets.Last;

            //Adds v separately
            positionPointers[triple[1]] = firstSet.AddLast(triple[1]);
            setPointers[triple[1]] = listOfSets.Last;

            //Set origin distance
            triple[2].Distance = 0;

            //Add N(u)\{v,w}
            foreach (Person person in triple[0].Neighbors)
            {
                if (person != triple[1] || person != triple[2])
                {
                    positionPointers[person] = secondSet.AddFirst(person);
                    setPointers[person] = listOfSets.First;
                }
            }

            //Add u
            positionPointers[triple[0]] = secondSet.AddFirst(triple[0]);
            setPointers[triple[0]] = listOfSets.First;

            //Get all persons ready
            foreach (Person person in Persons)
            {
                person.Position = -1;

                //Don't add neighbors of u, u, v or w
                if (!setPointers.ContainsKey(person))
                {
                    positionPointers[person] = firstSet.AddLast(person);
                    setPointers[person] = listOfSets.Last;
                }
            }

            //Label all persons
            for (int i = 0; i < Persons.Count; i++)
            {
                //Select current person for labeling and remove it from its set
                Person currentPerson = listOfSets.Last.Value.First.Value;
                listOfSets.Last.Value.RemoveFirst();
                positionPointers.Remove(currentPerson);

                //Remove set if empty
                if (listOfSets.Last.Value.Count == 0)
                    listOfSets.Remove(listOfSets.Last);

                //Assign label
                currentPerson.Position = i;
                outputArray[i] = currentPerson;

                //Assign new sets to all neighbors
                foreach (Person neighbor in currentPerson.Neighbors)
                {
                    //Ignore labeled neighbors
                    if (neighbor.Position == -1)
                    {
                        //Get current set of neighbor
                        LinkedListNode<SetList<Person>> currentSet = setPointers[neighbor];

                        //If currentSet has no replacement generate replacement set generate one
                        if (!currentSet.Value.HasReplacement)
                        {
                            currentSet.Value.HasReplacement = true;
                            SetList<Person> newSet = new SetList<Person>();
                            listOfSets.AddAfter(currentSet, newSet);

                            setsToCheck.Add(currentSet);
                        }

                        //Put neighbor in a set one level higher
                        currentSet.Value.Remove(positionPointers[neighbor]);
                        setPointers[neighbor] = currentSet.Next;
                        positionPointers[neighbor] = currentSet.Next.Value.AddLast(neighbor);

                        //Set distance of current neighbor
                        neighbor.Distance = currentPerson.Distance + 1;
                    }
                }

                //Unflag all previously flagged set and delete them if empty
                foreach (LinkedListNode<SetList<Person>> setList in setsToCheck)
                {
                    if (setList.Value.Count == 0)
                        listOfSets.Remove(setList);

                    else
                        setList.Value.HasReplacement = false;
                }

                setsToCheck.Clear();
            }

            return outputArray;
        }

        /// <summary>
        /// Test if the given Graph is an interval graph
        /// </summary>
        /// <param name="output">Returns a invalid structure</param>
        /// <returns>true if the graph is a interval graph otherwise false</returns>
        public bool TestIntervalGraph(out Person[] output)
        {
            if (!TestChordal(out output))
                return false;

            output = LexBFS(Direction.Forwards);

            MPQTree tree = new MPQTree();
            for (int i = 0; i < output.Length; i++)
            {
                if (!tree.AddVertice(output[i]))
                    return FindAT(out output, output[i]);
            }

            tree.GenerateIntervalRepresentation();
            return true;
        }

        /// <summary>
        /// Finds a AT in a MPQTree of a chordal graph
        /// </summary>
        /// <param name="output">The AT</param>
        /// <param name="x">The person at whose consideration the building of the MPQTree failed</param>
        private bool FindAT(out Person[] output, Person x)
        {
            uint[,] connectedComponentMap = new uint[Persons.Count, Persons.Count];
            uint highestLabel = 0;

            //We must consider every component of the current graph separately
            List<List<Person>> PLGComponents = findConnectedComponents(Persons);

            foreach (List<Person> component in PLGComponents) 
            {
                foreach (Person person in component)
                {
                    foreach (Person p in component)
                        p.Flagged = false;

                    //Keep track of the persons still not labeled
                    HashSet<Person> personSet = new HashSet<Person>(component);

                    //Person is in one connected component with himself
                    personSet.Remove(person);
                    connectedComponentMap[person.ID, person.ID] = 0;
                    person.Flagged = true;

                    //Neighbors are in the same connected component as person
                    foreach (Person neighbor in person.Neighbors)
                    {
                        neighbor.Flagged = true;
                        connectedComponentMap[person.ID, neighbor.ID] = 0;
                        personSet.Remove(neighbor);
                    }

                    //Perform BFS
                    Queue<Person> queue = new Queue<Person>();

                    //Go on until all persons are considered
                    while (personSet.Count > 0)
                    {
                        if (queue.Count == 0)
                        {
                            highestLabel++;
                            queue.Enqueue(personSet.First());
                        }

                        //Get the first person (FIFO)
                        Person currentPerson = queue.Dequeue();

                        currentPerson.Flagged = true;
                        personSet.Remove(currentPerson);
                        connectedComponentMap[person.ID, currentPerson.ID] = highestLabel;

                        //Enqueue all unvisited neighbors
                        foreach (Person neighbor in currentPerson.Neighbors)
                            if (!neighbor.Flagged)
                                queue.Enqueue(neighbor);
                    }
                }
            }

            //Check all triples in the graph
            //Because x must be part of the AT we must only search for y and z
            foreach (Person y in Persons)
            {
                if (y == x)
                    continue;

                foreach (Person z in Persons)
                {
                    if (z == y || z == x)
                        continue;

                    if (connectedComponentMap[x.ID, y.ID] == connectedComponentMap[x.ID, z.ID] && connectedComponentMap[y.ID, x.ID] == connectedComponentMap[y.ID, z.ID] && connectedComponentMap[z.ID, x.ID] == connectedComponentMap[z.ID, y.ID])
                    {
                        //AT found, return it
                        output = new Person[3];
                        output[0] = x;
                        output[1] = y;
                        output[2] = z;

                        return false;
                    }
                }
            }

            throw new InvalidOperationException("Only graphs containing an AT may be checked");
        }

        /// <summary>
        /// Find all the paths between the nodes of the at
        /// </summary>
        /// <param name="at">An at</param>
        /// <returns>A list of 3 paths</returns>
        public List<List<Person>> FindATPaths(Person[] at)
        {
            //All combinations necessary for finding all paths
            Person[][] tripelCombinations = new Person[3][];

            tripelCombinations[0] = new Person[3];
            tripelCombinations[0][0] = at[2];
            tripelCombinations[0][1] = at[0];
            tripelCombinations[0][2] = at[1];

            tripelCombinations[1] = new Person[3];
            tripelCombinations[1][0] = at[1];
            tripelCombinations[1][1] = at[2];
            tripelCombinations[1][2] = at[0];

            tripelCombinations[2] = new Person[3];
            tripelCombinations[2][0] = at[0];
            tripelCombinations[2][1] = at[1];
            tripelCombinations[2][2] = at[2];

            List<List<Person>> returnList = new List<List<Person>>(3);

            //Find the path for every pair of the triple
            for (int i = 0; i < 3; i++)
            {
                returnList.Add(new List<Person>());

                Person[] order = LexBFSFindAPath(tripelCombinations[i]);
                SortAdjLists(order);

                Person currentPerson = tripelCombinations[i][1];

                while (currentPerson.Distance > 0)
                {
                    returnList[i].Add(currentPerson);

                    //Find first neighbor with a lower distance
                    foreach (Person neighbor in currentPerson.Neighbors)
                    {
                        if (neighbor.Distance < currentPerson.Distance)
                        {
                            currentPerson = neighbor;
                            break;
                        }
                    }
                }

                returnList[i].Add(tripelCombinations[i][2]);
            }

            return returnList;
        }

        /// <summary>
        /// Finds the connected components of the current graph
        /// </summary>
        /// <param name="persons">A list of persons representing a graph</param>
        public static List<List<Person>> findConnectedComponents(IEnumerable<Person> persons)
        {
            //List of anchors for the graph
            Dictionary<Person, Person> anchors = new Dictionary<Person, Person>(persons.Count());

            //Assign all anchors to the persons themselves
            foreach (Person person in persons)
                anchors.Add(person, person);

            //Enumerates through all persons
            foreach (Person person in persons)
            {
                //And their neighbors
                foreach (Person neighbor in person.Neighbors)
                {
                    //Ignore persons that have a lower ID (they were already processed)
                    if (neighbor.ID < person.ID)
                        continue;

                    //Find the anchors of the current person and its neighbor
                    Person person1 = findAnchor(person, ref anchors);
                    Person person2 = findAnchor(neighbor, ref anchors);

                    //Overwrite the anchor with the higher ID
                    if (person1.ID < person2.ID)
                        anchors[person2] = person1;

                    else if (person2.ID < person1.ID)
                        anchors[person1] = person2;
                }


            }

            List<List<Person>> returnList = new List<List<Person>>();
            foreach (Person person in anchors.Values.Distinct())
                returnList.Add((from keyValuePair in anchors where keyValuePair.Value == person select keyValuePair.Key).ToList());

            return returnList;
        }

        /// <summary>
        /// Finds the anchor of the current node
        /// Uses pathcompression
        /// </summary>
        /// <param name="node">The person of which the anchor should be searched</param>
        /// <returns>The requested anchor</returns>
        private static Person findAnchor(Person node, ref Dictionary<Person, Person> anchors)
        {
            Person start = node;
            while (node != anchors[node])
                node = anchors[node];
            anchors[start] = node;
            return node;
        }
    }

    /// <summary>
    /// Extended LinkedList
    /// Used for LexBFS
    /// </summary>
    internal class SetList<T> : LinkedList<T>
    {
        public bool HasReplacement = false;
    }

    /// <summary>
    /// Enum of sorting directions
    /// Used for LexBFS
    /// </summary>
    internal enum Direction
    {
        Forwards,
        Backwards,
    }
}
