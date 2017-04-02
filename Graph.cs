using HoMM;
using HoMM.ClientClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Homm.Client
{
	class Graph
	{
		private readonly List<Node> nodes;

		public Graph(int nodesCount)
		{
			locationToIntDict = new Dictionary<Location, int>();
			nodes = Enumerable.Range(0, nodesCount).Select(z => new Node(z)).ToList();
		}

		public int Length => nodes.Count;

		public Node this[int index] => nodes[index];
		public Node this[Location index] => nodes[GetIndexOfLocation(index)];
		public Node this[MapObjectData index] => nodes[GetIndexOfData(index)];


		public int GetIndexOfLocation(Location location)
		{
			return locationToIntDict[location];
		}

		public bool IsLocationInDict(Location location)
		{
			return locationToIntDict.ContainsKey(location);
		}

		public int GetIndexOfData(MapObjectData mapData)
		{
			return locationToIntDict[mapData.Location.ToLocation()];
		}

		public IEnumerable<Node> Nodes => nodes;

		public void AddData(int index, int indexOfData, MapObjectData objectData)
		{
			locationToIntDict.Add(objectData.Location.ToLocation(), index);
			nodes[index].Location = objectData.Location.ToLocation();
			nodes[index].NumberOfData = indexOfData;
			nodes[index].SetWeight(objectData.Terrain);
		}

		private readonly Dictionary<Location, int> locationToIntDict;

		public void Connect(Location location1, Location location2)
		{
			Node.Connect(nodes[GetIndexOfLocation(location1)], nodes[GetIndexOfLocation(location2)]);
		}

		public void Connect(int data1, Location location2)
		{
			Node.Connect(nodes[data1], nodes[GetIndexOfLocation(location2)]);
		}

		/// <summary>
		/// Возвращает точку, в которую следует двигаться для достижения оптимального маршрута
		/// </summary>
		/// <param name="from">Начальная Точка</param>
		/// <param name="to">Конечная точка</param>
		/// <returns></returns>
		public Node Dijkstra(Node from, Node to)
		{
			var distances = new double[Length];
			var parents = new int[Length];
			var marks = new bool[Length];
			for (var i = 0; i < Length; ++i)
			{
				distances[i] = double.MaxValue;
				marks[i] = false;
			}
			distances[from.NodeNumber] = 0;
			for (var i = 0; i < Length; ++i)
			{
				var v = -1;
				for (var j = 0; j < Length; ++j)
					if (!marks[j] && (v == -1 || distances[j] < distances[v]))
						v = j;
				if (Math.Abs(distances[v] - double.MaxValue) < 0.0001)
					break;
				if (v == to.NodeNumber)
				{
					while (parents[v] != from.NodeNumber)
					{
						v = parents[v];
					}
					return nodes[v];
				}
				marks[v] = true;
				foreach (var node in nodes[v].IncidentNodes)
				{
					var num = node.NodeNumber;
					var len = node.Weight + distances[v];
					if (len >= distances[num]) continue;
					distances[num] = len;
					parents[num] = v;
				}
			}
			throw new ArgumentOutOfRangeException(nameof(to));
		}

		/// <summary>
		/// Генерирует массив минимальных расстояний до каждой точки
		/// </summary>
		/// <param name="from">Начальная точка</param>
		/// <returns>Массив минимальных расстояний</returns>
		public double[] Dijkstra(Node from)
		{
			var distances = new double[Length];
			var parents = new int[Length];
			var marks = new bool[Length];
			for (var i = 0; i < Length; ++i)
			{
				distances[i] = double.MaxValue;
				marks[i] = false;
			}
			distances[from.NodeNumber] = 0;
			for (var i = 0; i < Length; ++i)
			{
				var v = -1;
				for (var j = 0; j < Length; ++j)
					if (!marks[j] && (v == -1 || distances[j] < distances[v]))
						v = j;
				if (Math.Abs(distances[v] - double.MaxValue) < 0.0001)
					break;
				marks[v] = true;
				foreach (var node in nodes[v].IncidentNodes)
				{
					var num = node.NodeNumber;
					var len = node.Weight + distances[v];
					if (len >= distances[num]) continue;
					distances[num] = len;
					parents[num] = v;
				}
			}
			distances[from.NodeNumber] = Double.MaxValue;
			return distances;
		}

		public class Node
		{
			private readonly List<Node> nodes = new List<Node>();
			public readonly int NodeNumber;
			public Location Location { get; set; }
			public int NumberOfData;

			public Node(int number)
			{
				NodeNumber = number;
				extraWeight = 0;
			}

			public MapObjectData GetMapObjectData(HommSensorData sensorData)
			{
				return sensorData.Map.Objects[NumberOfData];
			}

			public IEnumerable<Node> IncidentNodes => nodes;

			private double weight;

			public double Weight
			{
				get { return weight + extraWeight; }
				private set { weight = value; }
			}

			private double extraWeight;

			public void SetWeight(Terrain terrain)
			{
				Weight = 1.0 + GetTerraingWeight(terrain);
			}

			private double GetTerraingWeight(Terrain terrain)
			{
				switch (terrain)
				{
					case Terrain.Grass:
						return 0.0;
					case Terrain.Snow:
						return 0.2;
					case Terrain.Desert:
						return 0.15;
					case Terrain.Marsh:
						return 0.3;
					case Terrain.Road:
						return -0.05;
					default:
						throw new ArgumentOutOfRangeException(nameof(terrain), terrain, null);
				}
			}

			public void AddExtraWeight(double value)
			{
				extraWeight += value;
			}

			public void DeleteExtraWeight()
			{
				extraWeight = 0;
			}

			public static void Connect(Node node1, Node node2)
			{
				if (node1.nodes.Contains(node2)) return;
				node1.nodes.Add(node2);
				node2.nodes.Add(node1);
			}

			public void Connect(Node anotherNode)
			{
				nodes.Add(anotherNode);
				anotherNode.nodes.Add(this);
			}

			public void Disconnect(Node anotherNode)
			{
				anotherNode.nodes.Remove(this);
				nodes.Remove(anotherNode);
			}
		}
	}
}