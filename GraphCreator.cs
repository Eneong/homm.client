using System.Linq;
using HoMM.ClientClasses;

namespace Homm.Client
{
	class GraphCreator
	{
		public static Graph Create(MapData map)
		{
			var count = map.Objects.Count(objectData => objectData.Wall == null);
			var graph = new Graph(count);
			var objects = map.Objects;
			var t = 0;
			for (var i = 0; i < objects.Count; ++i)
			{
				if (objects[i].Wall != null) continue;

				graph.AddData(t, i, objects[i]);
				++t;
			}
			AddNeighborhood(graph, map);
			return graph;
		}

		private static void AddNeighborhood(Graph graph, MapData map)
		{
			var objects = map.Objects;
			foreach (var mapObject in objects)
			{
				if (mapObject.Wall != null) continue;
				foreach (var location in mapObject.Location.ToLocation().Neighborhood)
				{
					if (graph.IsLocationInDict(location))
						graph.Connect(mapObject.Location.ToLocation(), location);
				}
			}
		}
	}
}
