using System;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.Linq;

namespace Homm.Client
{
	class WayMaker
	{
		public static Direction GetDirection(Location currentLocation, Location location, Graph graph)
		{
			var point = graph.Dijkstra(graph[graph.GetIndexOfLocation(currentLocation)], graph[graph.GetIndexOfLocation(location)]);
			return currentLocation.GetDirectionTo(point.Location);
		}

		public static Location GetLocation(HommSensorData sensorData, Graph graph, Func<Graph, HommSensorData, IEnumerable<MapObjectData>> getResource, out double maxDistance)
		{
			var node = graph[sensorData.Location.ToLocation()];
			Graph.Node nearestNode = null;
			var distances = graph.Dijkstra(node);
			maxDistance = double.MaxValue;
			foreach (var locationData in getResource(graph, sensorData))
			{
				if (!(distances[graph.GetIndexOfData(locationData)] < maxDistance)) continue;
				maxDistance = distances[graph.GetIndexOfData(locationData)];
				nearestNode = graph[locationData];
			}
			return nearestNode?.Location;
		}

		public static IEnumerable<MapObjectData> GetResourcePile(Graph graph, HommSensorData sensorData)
		{
			return graph.Nodes.Select(x => x.GetMapObjectData(sensorData)).Where(x => x.ResourcePile != null);
		}

		public static IEnumerable<MapObjectData> GetMine(Graph graph, HommSensorData sensorData)
		{
			return graph.Nodes.Select(x => x.GetMapObjectData(sensorData)).Where(x => x.Mine.Owner == null);
		}

		public static IEnumerable<MapObjectData> GetDwellingToConquer(Graph graph, HommSensorData sensorData)
		{
			return graph.Nodes.Select(x => x.GetMapObjectData(sensorData)).Where(x => x.Dwelling.Owner == null);
		}

		public static IEnumerable<MapObjectData> GetDwellingToHire(Graph graph, HommSensorData sensorData)
		{
			return graph.Nodes.Select(x => x.GetMapObjectData(sensorData)).Where(x => x.Dwelling.Owner != null && x.Dwelling.AvailableToBuyCount > 0);
		}
	}
}
