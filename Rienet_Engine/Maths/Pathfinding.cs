using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using System.Linq;
using BenchmarkDotNet.Columns;

namespace Rienet
{
    /// <summary>
    /// a mixed code half pasted from github (github links in specified methods below)
    /// </summary>
    public class Pathfinding
    {
        /// <summary>
        /// Done in the way using NextMoveDirectionByPathLine in the way i said so, by removing the first element in the path, remember convert the stack to array before using
        /// </summary>
        /// <returns>if item has reached absolute end of path</returns>
        public static bool NextMoveDirectionByPath(List<Line> path, Vector2 currentPos, Vector2 lastDirection, out Vector2 direction)
        {
            //
            //foreach (Line line in path)
               // Debug.Write(" " + "[" + (line.x2 - line.x1) + ", " + (line.y2 - line.y1) + "]");
            //Debug.WriteLine("");
            if (path.Count == 0)
            {
                direction = Vector2.Zero;
                return true;
            }

            if (NextMoveDirectionByPathLine(path[0], currentPos, lastDirection, out direction))
            {
                if (path.Count > 1)
                {
                    path.RemoveAt(0);
                    return false;
                }
                else return true;
            }
            else return false;
        }

        //TEST IF THE RETURNED PATH INNATELY SETS SOME SECTIONS DIAGONALLY, IF NOT THEN CHECK THE NEXT TWO TILES EACH TIME FOR A VELOCITY since next two can only be in one direction or diagonal
        /// <summary>
        /// To make it easier to move along path, decides next movement direction based on a body's position on its supposed path (just because something like a body is low level enough).
        /// ALWAYS MOVES ALONG THE FIRST LINE IN ARRAY< RETURNS A BOOLEAN INDICATING IF THE FIRST ELEMENT IN ARRAY SHOULD BE REMOVED, REMEMBER TO MANUALLY DO SO WHEN IT HAPPENS, AND TO ALSO SET WHATEVER INPUTTED TO LASTDIRECTION AS DIRECTION (Vector2.Zero)
        /// </summary>
        /// <param name="path">the path, by pathlines (use GetPathLines for convertion from stack of nodes to pathlines)</param>
        /// <param name="currentPos">pos representing object moving along path</param>
        /// <param name="rounding">rounding distance sqaured for how far the body can deviate from its path to count</param>
        /// <returns>if the path had reached its end, use this condition to choose line in path (usually just by removing the first element)</returns>
        public static bool NextMoveDirectionByPathLine(Line currentPathLine, Vector2 currentPos, Vector2 lastDirection, out Vector2 direction)
        {
            //direct to current end, if past it then consider it done (lastDirection x or (bc direct direction) y have opposite signs)
            direction = currentPathLine.Pos2 - currentPos;
            //if done
            if (OtherMaths.NumOppositeSigns(direction.X, lastDirection.X) || OtherMaths.NumOppositeSigns(direction.Y, lastDirection.Y))
            {
                direction = Vector2.Zero;
                return true;
            }
            //else normalize
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }
            return false;
        }

        /// <summary>
        /// simplifies path to basic lines for easier rounding and for entity to follow (if finding next line segment just remove the first)
        /// diagonal is to check whether to add diagonal lines (true for now)
        /// </summary>
        public static List<Line> GetPathLines(Vector2 trueStart, Vector2 trueEnd, bool diagonal, Stack<Node> path)
        {
            //run through path and find sharp turns, if just one by one turn then add a diagonal line, else straight lines to end target
            List<Line> pathLines = new();
            //set start, and before start add a line to direct to start
            Vector2 start = path.ElementAt(0).Position;
            pathLines.Add(new Line(trueStart, start));

            if (diagonal)
            {
                Vector2 currentStart = start;
                Vector2 currentEnd = currentStart; //not counted until different pattern in path is found
                //add by 2 to make sure ends meet
                for (int i = 1; i < path.Count - 2; i += 2)
                {
                    //used to determine current direction of line (to compare with later possible break offs)
                    var currentDif = currentEnd - currentStart;

                    //var node1 = path.ElementAt(i);
                    //get next 2 nodes
                    var node2 = path.ElementAt(i + 1);

                    // (get individual distances to two next nodes, check if their added result is perfectly diagnoal)
                    var add = node2.Position - currentEnd;

                    //first check if added result (next directions predicted from next two nodes) are in same direction as currentdif
                    if (currentDif.X * add.Y == currentDif.Y * add.X)
                    {
                        //add to line
                        //add added result to currentEnd
                        currentEnd += add;
                    }
                    //for next two cases, there is a breakoff, so end the current line and start the next
                    else
                    {
                        //end current line
                        pathLines.Add(new Line(currentStart, currentEnd));
                        currentStart = currentEnd;
                        //start new line (set currentEnd based on d2)
                        currentEnd = node2.Position;
                    }
                }
                pathLines.Add(new Line(currentEnd, trueEnd));
            }
            else
            {

            }

            return pathLines;
        }

        /// <summary>
        /// For future use within Rienet, use this method instead of FindPath as it's more specific and adapted to the engine (literally just enter two tileplaces (virtual tiles))
        /// </summary>
        /// <returns>Path, collection of tiles</returns>
        /// <param name="start">Starting tile</param>
        /// <param name="destination">Destination tile</param>
        static public Stack<Node> AdaptedFindPath(Vector2 start, Vector2 destination, Tile[,] layer)
        {
            AStar aStar = new(TilesToNodeMap(layer));
            return aStar.FindPath(start, destination);
        }

        /// <summary>
        /// just converting layer to node map, y reversed so it adapts to the search correctly
        /// </summary>
        static public List<List<Node>> TilesToNodeMap(Tile[,] layer)
        {
            List<List<Node>> nodeMap = new();
            //fill y reversely
            for (int x = 0; x < layer.GetLength(0); x++)
            {
                List<Node> column = new();

                for (int y = 0; y < layer.GetLength(1); y++)
                //for (int y = layer.GetLength(1) - 1; y >= 0; y--)
                {
                    Tile tile = layer[x, y];
                    //node 2d list is arranged by x, y as well
                    column.Add(tile != null ? new Node(tile.pos, tile.body == null, tile.travelCost) : new Node(new Vector2(x, y), true, 1));
                }

                nodeMap.Add(column);
            }

            return nodeMap;
        }
        /*
                /// <summary>
                /// Finds the optimal path between start and destionation TNode.
                /// 
                /// Maybe just put the estimation function as given??? also remember to account size in practical use. DON'T USE THIS FUNCTION, USE THE ADAPTED ONE INSTEAD, THIS ONE IS TOO ABSTRACT AND PAINSTAKING -Lazullien
                /// </summary>
                /// <returns>The path.</returns>
                /// <param name="dataStructure">Literally any Node object to invoke finding neighbors on, even for other Nodes "Description written by Lazullien"</param>
                /// <param name="start">Starting Node.</param>
                /// <param name="destination">Destination Node.</param>
                /// <param name="distance">Function to compute distance beween nodes.</param>
                /// <param name="estimate">Function to estimate the remaining cost for the goal. Added one more TNode to accept end Tile as parameter. "Sceond half added by Lazullien"</param>
                /// <typeparam name="TNode">Any class implement IHasNeighbours.</typeparam>
                static public Path<TNode> FindPath<TNode>(
                    IHasNeighbours<TNode> dataStructure,
                    TNode start,
                    TNode destination,
                    Func<TNode, TNode, double> distance,
                    Func<TNode, TNode, double> estimate)
                {
                    // Profiling Information
                    float expandedNodes = 0;
                    float elapsedTime = 0; //originally here but it was never used, probably used to make sure to break out when can't find path in time
                    Stopwatch st = new Stopwatch();
                    //----------------------
                    var closed = new HashSet<TNode>();
                    var queue = new PriorityQueue<Path<TNode>, double>();
                    queue.Enqueue(new Path<TNode>(start), 0);
                    if (CollectProfiling) st.Start();

                    while (queue.Count > 0)
                    {
                        elapsedTime += st.ElapsedMilliseconds;
                        var path = queue.Dequeue();

                        if (closed.Contains(path.LastStep))
                            continue;
                        //the flaw here is the destination might not be completely the same as laststep, only the position
                        //if (path.LastStep is TilePlace t1 && destination is TilePlace t2 && t1.pos == t2.pos)
                        //Debug.WriteLine("SUpposed to be here");
                        if (path.LastStep is TilePlace t1)
                            Debug.WriteLine(t1.pos);
                        if (path.LastStep.Equals(destination))
                        {
                            if (CollectProfiling)
                            {
                                st.Stop();
                                LastRunProfilingInfo["Expanded Nodes"] = expandedNodes;
                                LastRunProfilingInfo["Elapsed Time"] = st.ElapsedTicks;
                            }
                            return path;
                        }
                        closed.Add(path.LastStep);
                        expandedNodes++;

        //so it is a process of removing routes, but then it's adding 3 but only removing 1, in this case it's just infinitely expanding to the entire room (but not reaching the end????)
                        foreach (TNode n in dataStructure.Neighbours(path.LastStep))
                        {
                            if (closed.Contains(n))
                                goto skip;

                            double d = distance(path.LastStep, n);
                            if (n.Equals(destination))
                                d = 0;
                            var newPath = path.AddStep(n, d);

                            queue.Enqueue(newPath, newPath.TotalCost + estimate(n, destination));

                            skip:;
                        }
                    }
                    return null;
                }
            }

            /// <summary>
            /// https://gist.github.com/THeK3nger/7734169
            /// Interface that rapresent data structures that has the ability to find node neighbours.
            /// </summary>
            public interface IHasNeighbours<T>
            {
                /// <summary>
                /// Gets the neighbours of the instance.
                /// 
                /// Apparently T, as a generic, is too abstract to even be assumed an actual object, remember to add the parameter when implmenting this (unless you really don't need to) -Lazullien
                /// </summary>
                /// <value>The neighbours.</value>
                IEnumerable<T> Neighbours(T node);
            }

            /// <summary>
            /// https://gist.github.com/THeK3nger/7734169
            /// Represent a generic Path along a graph.
            /// </summary>
            public class Path<TNode> : IEnumerable<TNode>
            {

                #region PublicProperties
                /// <summary>
                /// Gets the last step.
                /// </summary>
                /// <value>The last step.</value>
                public TNode LastStep { get; private set; }

                /// <summary>
                /// Gets the previous steps.
                /// </summary>
                /// <value>The previous steps.</value>
                public Path<TNode> PreviousSteps { get; private set; }

                /// <summary>
                /// Gets the total cost.
                /// </summary>
                /// <value>The total cost.</value>
                public double TotalCost { get; private set; }
                #endregion

                #region Constructors
                /// <summary>
                /// Initializes a new instance of the <see cref="Path`1"/> class.
                /// </summary>
                /// <param name="lastStep">Last step.</param>
                /// <param name="previousSteps">Previous steps.</param>
                /// <param name="totalCost">Total cost.</param>
                Path(TNode lastStep, Path<TNode> previousSteps, double totalCost)
                {
                    LastStep = lastStep;
                    PreviousSteps = previousSteps;
                    TotalCost = totalCost;
                }

                /// <summary>
                /// Initializes a new instance of the <see cref="Path`1"/> class.
                /// </summary>
                /// <param name="start">Start.</param>
                public Path(TNode start) : this(start, null, 0) { }
                #endregion

                /// <summary>
                /// Adds a step to the path.
                /// </summary>
                /// <returns>The new path.</returns>
                /// <param name="step">The step.</param>
                /// <param name="stepCost">The step cost.</param>
                public Path<TNode> AddStep(TNode step, double stepCost)
                {
                    return new Path<TNode>(step, this, TotalCost + stepCost);
                }

                #region	EnumerableImplementation
                /// <summary>
                /// Gets the enumerator.
                /// </summary>
                /// <returns>The enumerator.</returns>
                public IEnumerator<TNode> GetEnumerator()
                {
                    for (Path<TNode> p = this; p != null; p = p.PreviousSteps)
                        yield return p.LastStep;
                }

                /// <summary>
                /// Gets the enumerator.
                /// </summary>
                /// <returns>The enumerator.</returns>
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.GetEnumerator();
                }
                #endregion

            }*/
    }

    /// <summary>
    /// https://github.com/davecusatis/A-Star-Sharp/blob/master/Astar.cs
    /// most abstract form of a tile, literally just a grid of position
    /// </summary>
    public class Node
    {
        // Change this depending on what the desired size is for each element in the grid (1 for TB)
        public static int NODE_SIZE = 1;
        public Node Parent;
        public Vector2 Position;
        public Vector2 Center
        {
            get
            {
                return new Vector2(Position.X + NODE_SIZE / 2, Position.Y + NODE_SIZE / 2);
            }
        }
        public float DistanceToTarget;
        public float Cost;
        public float Weight;
        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                else
                    return -1;
            }
        }
        public bool Walkable;

        public Node(Vector2 pos, bool walkable, float weight = 1)
        {
            Parent = null;
            Position = pos;
            DistanceToTarget = -1;
            Cost = 1;
            Weight = weight;
            Walkable = walkable;
        }
    }

    /// <summary>
    /// https://github.com/davecusatis/A-Star-Sharp/blob/master/Astar.cs
    /// the other one was just an unfinished fucking mess
    /// Each time in use an instance of this is needed to map things out apparantly (basically to fit tiles into the grid of nodes)
    /// </summary>
    public class AStar
    {
        List<List<Node>> Grid;
        int GridRows
        {
            get
            {
                return Grid[0].Count;
            }
        }
        int GridCols
        {
            get
            {
                return Grid.Count;
            }
        }

        public AStar(List<List<Node>> grid)
        {
            Grid = grid;
        }

        public Stack<Node> FindPath(Vector2 Start, Vector2 End)
        {
            Node start = new Node(new Vector2((int)(Start.X / Node.NODE_SIZE), (int)(Start.Y / Node.NODE_SIZE)), true);
            Node end = new Node(new Vector2((int)(End.X / Node.NODE_SIZE), (int)(End.Y / Node.NODE_SIZE)), true);

            Stack<Node> Path = new Stack<Node>();
            PriorityQueue<Node, float> OpenList = new PriorityQueue<Node, float>();
            List<Node> ClosedList = new List<Node>();
            List<Node> adjacencies;
            Node current = start;

            // add start node to Open List
            OpenList.Enqueue(start, start.F);

            while (OpenList.Count != 0 && !ClosedList.Exists(x => x.Position == end.Position))
            {
                current = OpenList.Dequeue();
                ClosedList.Add(current);
                adjacencies = GetAdjacentNodes(current);

                foreach (Node n in adjacencies)
                {
                    if (!ClosedList.Contains(n) && n.Walkable)
                    {
                        bool isFound = false;
                        foreach (var oLNode in OpenList.UnorderedItems)
                        {
                            if (oLNode.Element == n)
                            {
                                isFound = true;
                            }
                        }
                        if (!isFound)
                        {
                            n.Parent = current;
                            n.DistanceToTarget = Math.Abs(n.Position.X - end.Position.X) + Math.Abs(n.Position.Y - end.Position.Y);
                            n.Cost = n.Weight + n.Parent.Cost;
                            OpenList.Enqueue(n, n.F);
                        }
                    }
                }
            }

            // construct path, if end was not closed return null
            if (!ClosedList.Exists(x => x.Position == end.Position))
            {
                return null;
            }

            // if all good, return path
            Node temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null) return null;
            do
            {
                Path.Push(temp);
                temp = temp.Parent;
            } while (temp != start && temp != null);
            return Path;
        }

        private List<Node> GetAdjacentNodes(Node n)
        {
            List<Node> temp = new List<Node>();

            int row = (int)n.Position.Y;
            int col = (int)n.Position.X;

            if (row + 1 < GridRows)
            {
                temp.Add(Grid[col][row + 1]);
            }
            if (row - 1 >= 0)
            {
                temp.Add(Grid[col][row - 1]);
            }
            if (col - 1 >= 0)
            {
                temp.Add(Grid[col - 1][row]);
            }
            if (col + 1 < GridCols)
            {
                temp.Add(Grid[col + 1][row]);
            }

            return temp;
        }
    }
}