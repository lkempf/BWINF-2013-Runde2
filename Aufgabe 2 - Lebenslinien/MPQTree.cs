using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace PLG 
{
    class MPQTree
    {
        #region Nodes
        abstract class BaseNode
        {
            protected MPQTree tree;

            protected BaseNode father;
            public BaseNode Father
            {
                get { return father; }
                [DebuggerStepThrough]
                set
                {
                    //Prevent a QNode from being a father, else possibility for a wrong implementation later on
                    if (!(value is QNode) || this is Section)
                    {
                        if (father != null)
                            father.RemoveSon(this);

                        father = value;

                        if (value == null)
                            tree.root = this;

                        else value.SetSon(this);

                        if (this is PNode)
                            tree.manipulatedPNodes.Add(this as PNode);
                    }

                    else if (!(this is Section))
                        throw new FormatException("Father may not be a QNode");
                }
            }

            public LinkedList<Person> ContainedPersons;

            [DebuggerStepThrough]
            public BaseNode(MPQTree tree)
            {
                ContainedPersons = new LinkedList<Person>();
                this.tree = tree;
            }

            [DebuggerStepThrough]
            public virtual void AddContainedPerson(Person p)
            {
                tree.nodePointer[p] = this;
                tree.positionPointers[p] = ContainedPersons.AddLast(p);
            }

            [DebuggerStepThrough]
            public void AddContainedRange(IEnumerable<Person> persons)
            {
                foreach (Person p in persons)
                    AddContainedPerson(p);
            }

            [DebuggerStepThrough]
            protected virtual void RemoveSon(BaseNode son)
            {

            }

            [DebuggerStepThrough]
            protected virtual void SetSon(BaseNode son)
            {

            }

            [DebuggerStepThrough]
            public void Vanish()
            {
                Father = this;
            }
        }

        class PNode : BaseNode
        {
            private bool ignoreSetSon = false;

            public LinkedList<BaseNode> Children;
            public BaseNode SomeSon;

            [DebuggerStepThrough]
            public PNode(MPQTree tree)
                : base(tree)
            {
                Children = new LinkedList<BaseNode>();
                tree.manipulatedPNodes.Add(this);
            }

            [DebuggerStepThrough]
            public void AddLastChild(BaseNode child)
            {
                if (child is Section)
                    throw new FormatException("child mustn't be a section");

                ignoreSetSon = true;
                child.Father = this;
                ignoreSetSon = false;

                Children.AddLast(child);
            }

            [DebuggerStepThrough]
            public void AddFirstChild(BaseNode child)
            {
                if (child is Section)
                    throw new FormatException("child mustn't be a section");

                ignoreSetSon = true;
                child.Father = this;
                ignoreSetSon = false;

                Children.AddFirst(child);
            }

            [DebuggerStepThrough]
            protected override void SetSon(BaseNode son)
            {
                if (!ignoreSetSon)
                {
                    if (son is Section)
                        throw new FormatException("child mustn't be a section");

                    Children.AddLast(son);
                }

                SomeSon = son;
            }

            [DebuggerStepThrough]
            protected override void RemoveSon(BaseNode son)
            {
                if (Children.Remove(son))
                {
                    tree.manipulatedPNodes.Add(this);
                }

                else throw new InvalidOperationException("Son is not in Children");
            }
        }

        class QNode : BaseNode
        {
            public List<Section> Sections;
            public Section OuterSectionRight, OuterSectionLeft;

            [DebuggerStepThrough]
            public QNode(MPQTree tree)
                : base(tree)
            {
                Sections = new List<Section>();
            }

            [DebuggerStepThrough]
            public Section AddNewLastSection()
            {
                Section section = new Section(tree);
                if (Sections.Count != 0)
                {
                    section.LeftNeighbor = OuterSectionRight;
                    OuterSectionRight.RightNeighbor = section;
                }

                else
                    OuterSectionLeft = section;

                OuterSectionRight = section;
                Sections.Add(section);

                section.Father = this;

                return section;
            }

            [DebuggerStepThrough]
            public Section AddNewFirstSection()
            {
                Section section = new Section(tree);
                if (Sections.Count != 0)
                {
                    section.RightNeighbor = OuterSectionLeft;
                    OuterSectionLeft.LeftNeighbor = section;
                }

                else
                    OuterSectionRight = section;

                OuterSectionLeft = section;
                Sections.Add(section);

                section.Father = this;

                return section;
            }

            [DebuggerStepThrough]
            protected override void SetSon(BaseNode son)
            {
                if (!(son is Section))
                    throw new FormatException("Only sections may be sons of QNodes");
            }

            [DebuggerStepThrough]
            protected override void RemoveSon(BaseNode son)
            {
                if (!(son is Section))
                    throw new FormatException("Only sections may be sons of QNodes");

                if (son.Father != this)
                    throw new InvalidOperationException("Son must be part of current QNode");

                RemoveSection(son as Section);
            }

            [DebuggerStepThrough]
            public void RemoveSection(Section section)
            {
                Sections.Remove(section);

                if (section.RightNeighbor == null)
                    OuterSectionRight = section.LeftNeighbor;
                else
                    section.RightNeighbor.LeftNeighbor = section.LeftNeighbor;

                if (section.LeftNeighbor == null)
                    OuterSectionLeft = section.RightNeighbor;
                else
                    section.LeftNeighbor.RightNeighbor = section.RightNeighbor;

                Sections.Remove(section);
            }
        }

        class Section : BaseNode
        {
            public BaseNode Son;
            public Section LeftNeighbor, RightNeighbor;

            [DebuggerStepThrough]
            public Section(MPQTree tree)
                : base(tree)
            { }

            [DebuggerStepThrough]
            protected override void SetSon(BaseNode son)
            {
                if (son is Section)
                    throw new FormatException("child mustn't be a section");

                Son = son;
            }

            [DebuggerStepThrough]
            public override void AddContainedPerson(Person p)
            {
                //Only the outer section is important if the person has already been added
                if (LeftNeighbor == null || RightNeighbor == null || !tree.positionPointers.ContainsKey(p))
                    base.AddContainedPerson(p);

                else
                    ContainedPersons.AddLast(p);
            }

            [DebuggerStepThrough]
            protected override void RemoveSon(BaseNode son)
            {
                if (son is Section)
                    throw new FormatException("child mustn't be a section");

                if (Son == son)
                    Son = null;
            }
        }

        class Leaf : BaseNode
        {
            [DebuggerStepThrough]
            public Leaf(MPQTree tree)
                : base(tree)
            { }
        }
        #endregion

        BaseNode root;
        Dictionary<Person, BaseNode> nodePointer;
        Dictionary<Person, LinkedListNode<Person>> positionPointers;
        HashSet<PNode> manipulatedPNodes;

        public MPQTree()
        {
            nodePointer = new Dictionary<Person, BaseNode>();
            positionPointers = new Dictionary<Person, LinkedListNode<Person>>();
            manipulatedPNodes = new HashSet<PNode>();
        }

        /// <summary>
        /// Add a person to the MPQ-Tree
        /// If this fails the graph is no interval graph
        /// </summary>
        /// <param name="person"></param>
        /// <returns>If false adding the person fails</returns>
        public bool AddVertice(Person person)
        {
            //Can be omitted if person has no neighbors or there are no nodes
            if (person.Neighbors.Count > 0 && root != null)
            {
                //nDown is the lowest node on P with label 1 or infinity
                //If there is a node with the label 0 or 1 let nUp be the highest such node else nUp is nDown
                //nCurrent is the current node between nDown and nUp
                BaseNode nUp = null, nDown, nCurrent;

                //List of the persons in A for a certain node
                Dictionary<BaseNode, List<Person>> ALists = new Dictionary<BaseNode, List<Person>>();

                //Represents a path P from the root to a positively labeled leaf
                Dictionary<BaseNode, BaseNode> rootedPath = new Dictionary<BaseNode, BaseNode>();

                Dictionary<BaseNode, bool> flagMap = new Dictionary<BaseNode, bool>();

                foreach (BaseNode node in nodePointer.Values)
                {
                    flagMap[node] = false;
                }

                //All nodes that have to be check whether they are contained on a path
                Queue<BaseNode> QueueToCheck = new Queue<BaseNode>();

                //Check all neighbors
                foreach (Person neighbor in person.Neighbors)
                {
                    //Ignore persons who are not part of the tree
                    if (neighbor.Position < person.Position)
                    {
                        //The node the current neighbor belongs to
                        BaseNode associatedNode = nodePointer[neighbor];

                        if (associatedNode is Section)
                        {
                            Section associatedSection = associatedNode as Section;

                            //The neighbor must be contained in an outer section
                            if (associatedSection.RightNeighbor == null && associatedSection.LeftNeighbor == null)
                                return false;
                        }

                        //Creates a new A list for the current node
                        if (!ALists.ContainsKey(associatedNode))
                            ALists.Add(associatedNode, new List<Person>());

                        //Adds the current neighbor to the A list
                        ALists[associatedNode].Add(neighbor);
                        //Remove the current neighbor from the B list
                        associatedNode.ContainedPersons.Remove(positionPointers[neighbor]);

                        QueueToCheck.Enqueue(associatedNode);
                    }
                }

                //Current person has no neighbors, which are in the tree
                if (QueueToCheck.Count == 0)
                {
                    //We have already a PNode without contained persons as root -> no need to create one
                    if (root is PNode && root.ContainedPersons.Count == 0)
                    {
                        Leaf leaf = new Leaf(this);
                        leaf.AddContainedPerson(person);
                        leaf.Father = root;
                    }

                    //Create a new PNode without contained persons
                    else
                    {
                        PNode newRoot = new PNode(this);
                        newRoot.AddFirstChild(root);
                        newRoot.Father = null;

                        Leaf leaf = new Leaf(this);
                        leaf.Father = newRoot;
                        leaf.AddContainedPerson(person);
                    }

                    return true;
                }

                BaseNode currentNode;

                //Flag all elements on QueueToCheck
                while (QueueToCheck.Count > 0)
                {
                    currentNode = QueueToCheck.Dequeue();

                    //Node not yet visited
                    flagMap[currentNode] = true;

                    if (currentNode.Father != null)
                    {
                        //Father has to be flagged too
                        QueueToCheck.Enqueue(currentNode.Father);

                        //All nodes must be on a path -> if the current node is not a duplicate it may not be on the rooted path
                        if (!(currentNode is Section))
                        {
                            if (rootedPath.ContainsKey(currentNode.Father) && rootedPath[currentNode.Father] != currentNode)
                                return false;
                        }

                        //Things are different with section: There may be multiple sections of one QNode, but only one of them may be a outer section
                        else if (rootedPath.ContainsKey(currentNode.Father) && currentNode != rootedPath[currentNode.Father])
                        {
                            Section currentSection = currentNode as Section;
                            Section otherContainedSection = rootedPath[currentNode.Father] as Section;

                            if (currentSection.LeftNeighbor == null || currentSection.RightNeighbor == null)
                                if (!(otherContainedSection.LeftNeighbor == null || otherContainedSection.RightNeighbor == null))
                                    rootedPath[currentNode.Father] = currentNode;
                                else return false; //There is no path
                        }

                        //Keep a link from the father to its son
                        rootedPath[currentNode.Father] = currentNode;
                    }
                }

                //The path must start at the root
                currentNode = root;

                //Travers the rooted path downwards to find nUp
                while (rootedPath.ContainsKey(currentNode))
                {
                    //nUp not found yet
                    if (nUp == null)
                    {
                        //B mustn't be empty (-> label is not Infinity)
                        if (currentNode.ContainedPersons.Count > 0)
                        {
                            if (currentNode is Section)
                            {
                                //The path must go through a outer section
                                Section currentSection = currentNode as Section;
                                if (currentSection.LeftNeighbor != null && currentSection.RightNeighbor != null)
                                    return false;

                                //nUp must be a qNode containing the Section fulfilling the condition
                                nUp = currentNode.Father as QNode;
                            }

                            else if (currentNode is PNode)
                            {
                                nUp = currentNode;
                            }
                        }
                    }

                    currentNode = rootedPath[currentNode];
                }

                //If currentNode is a Section get the associated QNode
                if (currentNode is Section)
                    currentNode = currentNode.Father;

                //nDown is the lowest node on the path P
                nDown = currentNode;

                //If there is no node matching the conditions for nUp, nUp is nDown 
                if (nUp == null)
                    nUp = nDown;

                //nCurrent must be traversed bottom-up (-> is set to nDown at start)
                nCurrent = nDown;

                //The path between nDown and nUp
                Dictionary<BaseNode, BaseNode> progressionPath = new Dictionary<BaseNode, BaseNode>();

                currentNode = nDown;

                //Build a path from nDown to nUp
                if (nUp == nDown)
                    progressionPath.Add(nDown, null);

                else
                {
                    while(currentNode != null)
                    {
                        progressionPath.Add(currentNode, currentNode.Father);
                        currentNode = currentNode.Father;
                    }
                }

                //Apply templates
                do
                {
                    if (!ALists.ContainsKey(nCurrent))
                    {
                        //AList are generated for Sections -> need to be transformed to QNodes
                        if (nCurrent is QNode)
                        {
                            QNode currentQNode = nCurrent as QNode;

                            if (ALists.ContainsKey(currentQNode.OuterSectionLeft))
                            {
                                ALists[nCurrent] = ALists[currentQNode.OuterSectionLeft];
                                ALists.Remove(currentQNode.OuterSectionLeft);
                            }

                            else if (ALists.ContainsKey(currentQNode.OuterSectionRight))
                            {
                                ALists[nCurrent] = ALists[currentQNode.OuterSectionRight];
                                ALists.Remove(currentQNode.OuterSectionRight);
                            }

                            else
                                ALists[nCurrent] = new List<Person>();
                        }

                        else
                            ALists[nCurrent] = new List<Person>();
                    }

                    //Templates L1 and L2
                    if (nCurrent is Leaf)
                    {
                        //Template L1
                        if (nUp == nDown)
                        {
                            //B is empty
                            if (nCurrent.ContainedPersons.Count == 0)
                            {
                                nCurrent.AddContainedRange(ALists[nCurrent]);

                                nCurrent.AddContainedPerson(person);
                            }

                            //B is not empty
                            else
                            {
                                PNode pNode = new PNode(this);
                                pNode.Father = nCurrent.Father;

                                Leaf leaf = new Leaf(this);
                                leaf.Father = pNode;

                                leaf.AddContainedPerson(person);

                                pNode.AddLastChild(nCurrent);

                                pNode.AddContainedRange(ALists[nCurrent]);
                            }
                        }

                        //Template L2
                        else
                        {
                            QNode qNode = new QNode(this);
                            qNode.Father = nCurrent.Father;

                            Section section1 = qNode.AddNewLastSection();
                            Section section2 = qNode.AddNewLastSection();

                            section1.AddContainedRange(ALists[nCurrent]);
                            section2.AddContainedRange(ALists[nCurrent]);

                            Leaf leaf = new Leaf(this);
                            leaf.Father = section1;
                            leaf.AddContainedPerson(person);

                            nCurrent.Father = section2;
                        }
                    }

                    //Templates P1, P2 and P3
                    else if (nCurrent is PNode)
                    {
                        //Template P1
                        if (nUp == nDown)
                        {
                            //B is empty
                            if (nCurrent.ContainedPersons.Count == 0)
                            {
                                Leaf leaf = new Leaf(this);
                                leaf.AddContainedPerson(person);
                                leaf.Father = nCurrent;

                                nCurrent.AddContainedRange(ALists[nCurrent]);
                            }

                            //B is not empty
                            else
                            {
                                PNode pNode = new PNode(this);
                                pNode.Father = nCurrent.Father;

                                Leaf leaf = new Leaf(this);
                                leaf.AddContainedPerson(person);

                                leaf.Father = pNode;
                                nCurrent.Father = pNode;
                            }
                        }

                        //Template P2
                        else if (nCurrent == nDown) //No need to check nDown != nUp (previously checked)
                        {
                            QNode qNode = new QNode(this);
                            qNode.Father = nCurrent.Father;

                            Section section1 = qNode.AddNewLastSection();
                            Section section2 = qNode.AddNewLastSection();

                            Leaf leaf = new Leaf(this);
                            leaf.AddContainedPerson(person);
                            leaf.Father = section1;

                            nCurrent.Father = section2;

                            section1.AddContainedRange(ALists[nCurrent]);
                            section2.AddContainedRange(ALists[nCurrent]);
                        }

                        //Template P3
                        else if (nCurrent != nDown)
                        {
                            PNode currentPNode = nCurrent as PNode;
                            QNode currentQNode = currentPNode.SomeSon as QNode;

                            //No need to edit the father correctly; this will happen in Q3
                            if (currentPNode.Father is Section)
                                foreach (Person p in currentPNode.Father.ContainedPersons)
                                    currentPNode.ContainedPersons.AddLast(p);

                            if (currentQNode != null && currentQNode.OuterSectionLeft.Son.ContainedPersons.First.Value == person)
                            {
                                QNode newQNode = new QNode(this);
                                newQNode.Father = currentPNode.Father;

                                Section currentSection = currentQNode.OuterSectionLeft.RightNeighbor;
                                while (currentSection != null)
                                {
                                    Section newSection = newQNode.AddNewLastSection();
                                    currentSection.Son.Father = newSection;

                                    IEnumerable<Person> newContainedPersons = ALists[nCurrent].Union(nCurrent.ContainedPersons).Union(currentSection.ContainedPersons);
                                    newSection.AddContainedRange(newContainedPersons);

                                    currentSection = currentSection.RightNeighbor;
                                }

                                Section lastNewSection = newQNode.AddNewLastSection();

                                Section section1 = newQNode.AddNewFirstSection();
                                currentQNode.OuterSectionLeft.Son.Father = section1;

                                section1.AddContainedRange(ALists[nCurrent].Union(currentQNode.OuterSectionLeft.ContainedPersons));

                                PNode newPNode = new PNode(this);
                                newPNode.Father = lastNewSection;

                                lastNewSection.AddContainedRange(ALists[nCurrent].Union(nCurrent.ContainedPersons));

                                foreach (BaseNode node in currentPNode.Children.ToArray())
                                    if (node != currentQNode)
                                        newPNode.AddLastChild(node);
                            }

                            currentPNode.Vanish();
                        }
                    }

                    //Templates Q1, Q2 and Q3
                    else if (nCurrent is QNode)
                    {
                        //Templates Q1 and Q2
                        if(nCurrent == nDown)
                        {
                            QNode currentQNode = currentNode as QNode;

                            //Template Q1
                            if(!ALists[currentQNode].Except(currentQNode.OuterSectionRight.ContainedPersons).Any())
                            {
                                if(nUp == nDown)
                                {
                                    PNode pNode = new PNode(this);
                                    pNode.Father = nCurrent.Father;

                                    Leaf leaf = new Leaf(this);
                                    pNode.AddFirstChild(leaf);
                                    leaf.AddContainedPerson(person);

                                    pNode.AddContainedRange(ALists[nCurrent]);

                                    Section currentSection = currentQNode.OuterSectionLeft;
                                    while(currentSection != null)
                                    {
                                        //Removes a from all sections
                                        currentSection.ContainedPersons = new LinkedList<Person>(currentSection.ContainedPersons.Except(ALists[nCurrent]));

                                        currentSection = currentSection.RightNeighbor;
                                    }

                                    pNode.AddLastChild(currentQNode);
                                }

                                else
                                {
                                    QNode newQNode = new QNode(this);
                                    newQNode.Father = nCurrent.Father;

                                    Section firstSection = newQNode.AddNewLastSection();
                                    Section secondSection = newQNode.AddNewLastSection();

                                    Leaf leaf = new Leaf(this);
                                    leaf.Father = firstSection;
                                    leaf.AddContainedPerson(person);

                                    firstSection.AddContainedRange(ALists[nCurrent]);
                                    secondSection.AddContainedRange(ALists[nCurrent]);

                                    Section currentSection = currentQNode.OuterSectionLeft;
                                    while (currentSection != null)
                                    {
                                        //Removes a from all sections
                                        currentSection.ContainedPersons = new LinkedList<Person>(currentSection.ContainedPersons.Except(ALists[nCurrent]));

                                        currentSection = currentSection.RightNeighbor;
                                    }

                                    currentQNode.Father = secondSection;
                                }
                            }

                            //Template Q2
                            else
                            {
                                //B is empty
                                if (nCurrent.ContainedPersons.Count == 0)
                                {
                                    PNode pNode = new PNode(this);

                                    pNode.AddFirstChild(currentQNode.OuterSectionLeft.Son);
                                    pNode.Father = currentQNode.OuterSectionLeft;

                                    currentQNode.OuterSectionLeft.AddContainedRange(ALists[nCurrent]);

                                    Leaf leaf = new Leaf(this);
                                    leaf.AddContainedPerson(person);
                                    pNode.AddFirstChild(leaf);
                                }

                                else if (nCurrent.ContainedPersons.Count == 0 || nUp != nDown)
                                {
                                    Section section = currentQNode.AddNewFirstSection();
                                    section.AddContainedRange(ALists[nCurrent]);

                                    Leaf leaf = new Leaf(this);
                                    leaf.Father = section;
                                    leaf.AddContainedPerson(person);
                                }
                            }
                        }

                        //Template Q3
                        else
                        {
                            QNode currentQNode = nCurrent as QNode;
                            QNode currentLowerQNode = null;

                            Section parentSection;
                            parentSection = currentQNode.OuterSectionLeft as Section;

                            if (currentQNode.OuterSectionLeft.Son is QNode)
                            {
                                parentSection = currentQNode.OuterSectionLeft;
                                currentLowerQNode = parentSection.Son as QNode;
                            }

                            if(currentLowerQNode != null && currentLowerQNode.OuterSectionLeft.Son.ContainedPersons.First.Value == person)
                            {
                                currentQNode.RemoveSection(parentSection);

                                Section currentSection = currentLowerQNode.OuterSectionRight;
                                while (currentSection.LeftNeighbor != null)
                                {
                                    Section newSection = currentQNode.AddNewFirstSection();
                                    if (currentSection.Son != null)
                                        currentSection.Son.Father = newSection;

                                    IEnumerable<Person> newContainedPersons = ALists[nCurrent].Union(nCurrent.ContainedPersons).Union(currentSection.ContainedPersons);
                                    newSection.AddContainedRange(newContainedPersons);

                                    currentSection = currentSection.LeftNeighbor;
                                }

                                Section firstNewSection;
                                firstNewSection = currentQNode.AddNewFirstSection();

                                currentLowerQNode.OuterSectionLeft.Son.Father = firstNewSection;

                                firstNewSection.AddContainedRange(currentLowerQNode.OuterSectionLeft.ContainedPersons.Union(ALists[nCurrent]));
                            }
                        }
                    }
                    
                    //Remove the AList of nCurrent -> every AList contained in the end has to be written back
                    ALists.Remove(nCurrent);

                    //Go one node up
                    nCurrent = progressionPath[nCurrent];
                    
                    //If nCurrent now is a Section get the associated QNode
                    if (nCurrent is Section)
                    {
                        Section currentSection = nCurrent as Section;
                        nCurrent = currentSection.Father;
                    }
                }
                while (nCurrent != null);

                //Add the unused ALists
                foreach (BaseNode baseNode in ALists.Keys)
                    baseNode.AddContainedRange(ALists[baseNode]);

                //Check all manipulated PNodes
                //Helper templates might be necessary
                foreach (PNode pNode in manipulatedPNodes.ToArray())
                {
                    //Template H1
                    if (pNode.ContainedPersons.Count == 0 && pNode.Children.Count == 1 && pNode.Father != pNode)
                    {
                        pNode.Children.First.Value.Father = pNode.Father;
                        pNode.Vanish();
                    }

                    //Template H2
                    else if (pNode.Father is PNode)
                    {
                        PNode fatherPNode = pNode.Father as PNode;
                        if (fatherPNode.ContainedPersons.Count == 0 && pNode.ContainedPersons.Count == 0)
                        {
                            foreach (BaseNode node in pNode.Children.ToArray())
                            {
                                fatherPNode.AddLastChild(node);
                            }

                            pNode.Vanish();
                        }
                    }
                }

                manipulatedPNodes.Clear();
            }

            else
            {
                if (person.Neighbors.Count == 0 && root != null)
                {
                    //We have already a PNode without contained persons as root -> no need to create one
                    if (root is PNode && root.ContainedPersons.Count == 0)
                    {
                        Leaf leaf = new Leaf(this);
                        leaf.AddContainedPerson(person);
                        leaf.Father = root;
                    }

                    //Create a new PNode without contained persons
                    else
                    {
                        PNode newRoot = new PNode(this);
                        newRoot.AddFirstChild(root);
                        newRoot.Father = null;

                        Leaf leaf = new Leaf(this);
                        leaf.Father = newRoot;
                        leaf.AddContainedPerson(person);
                    }
                }

                //Create a new leaf
                else if (root == null)
                {
                    Leaf leaf = new Leaf(this);
                    leaf.Father = root;
                    leaf.AddContainedPerson(person);
                }
            }

            return true;
        }

        /// <summary>
        /// Generates the intervalrepresentation of the current MPQ-Tree
        /// </summary>
        public void GenerateIntervalRepresentation()
        {
            List<List<Person>> cliquesList = new List<List<Person>>();

            //Perform recursive backtracking on the current MPQ-Tree
            //Gets a list with all maximum cliques
            Visit(root, ref cliquesList);

            //Current position in the current interval representation
            int currentPosition = 1;

            //Enumerate through all cliques
            foreach (List<Person> section in cliquesList)
            {
                //Enumerate through all persons in the current clique
                foreach (Person person in section)
                {
                    //When person has no birth set the birth to the current position in the interval representation
                    if (person.Birth == 0)
                        person.Birth = currentPosition;

                    if (person.Death == 0)
                        person.Death = currentPosition + 1;

                    //This person occurs more than once -> increase the end in the interval representation
                    else
                        person.Death++;
                }

                currentPosition++;
            }
        }

        /// <summary>
        /// DFS used for traversing through any path of the MPQTree and get thus get the maximal cliques of the associated graph
        /// </summary>
        /// <param name="cliquesList">A reference to the clique list</param>
        /// <returns>All indices included by the current subtree</returns>
        private List<int> Visit(BaseNode currentNode, ref List<List<Person>> cliquesList)
        {
            List<int> returnList = new List<int>();

            //End of path
            if (currentNode is Leaf)
            {
                cliquesList.Add(new List<Person>());
                returnList.Add(cliquesList.Count - 1);
            }

            else if (currentNode is PNode)
            {
                PNode currentPNode = currentNode as PNode;

                //Enumerate trough all children and get their sections
                foreach (BaseNode child in currentPNode.Children)
                    returnList.AddRange(Visit(child, ref cliquesList));
            }

            else if (currentNode is QNode)
            {
                QNode currentQNode = currentNode as QNode;
                Section currentSection = currentQNode.OuterSectionLeft;

                //Enumerate through all sections from left to right
                while (currentSection != null)
                {
                    returnList.AddRange(Visit(currentSection, ref cliquesList));
                    currentSection = currentSection.RightNeighbor;
                }

                //No need to add contained persons (QNode mustn't have any)
                return returnList;
            }

            else if (currentNode is Section)
            {
                Section currentSection  = currentNode as Section;
                returnList.AddRange(Visit(currentSection.Son, ref cliquesList));
            }

            //Add the contained persons to all child cliques
            foreach (int i in returnList)
                cliquesList[i].AddRange(currentNode.ContainedPersons);

            return returnList;
        }
    }
}