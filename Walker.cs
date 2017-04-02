using System;
using HoMM;
using HoMM.ClientClasses;

namespace Homm.Client
{
	class Walker
	{
		public static void NoName(HommSensorData sensorData, HommClient client)
		{
			var graph = GraphCreator.Create(sensorData.Map);
			FirstStage(sensorData, graph, client);
		}

		public static void FirstStage(HommSensorData sensorData, Graph graph, HommClient client)
		{
			foreach (var e in graph.Nodes)
			{
				if (e.GetMapObjectData(sensorData).NeutralArmy != null)
				{
					e.AddExtraWeight(
						Combat.Resolve(new ArmiesPair(sensorData.MyArmy, e.GetMapObjectData(sensorData).NeutralArmy.Army)).IsDefenderWin
							? 100000
							: 1);
				}
				if (e.GetMapObjectData(sensorData).Dwelling != null)
					e.AddExtraWeight(-0.2);
			}
			while (true)
			{
				var loc = WayMaker.GetLocation(sensorData, graph, WayMaker.GetResourcePile, out double maxDistance);
				if (!(maxDistance > 10000))
					while (sensorData.Location.ToLocation() != loc)
					{
						if (sensorData.Location.ToLocation() == loc)
						{
							throw new Exception();
						}
						sensorData = client.Move(WayMaker.GetDirection(sensorData.Location.ToLocation(), loc, graph));
						var dwelling = graph[sensorData.Location.ToLocation()].GetMapObjectData(sensorData).Dwelling;
						if (dwelling == null || dwelling.AvailableToBuyCount <= 0 || sensorData.MyTreasury[Resource.Gold] <= 0) continue;
						var cost = UnitsConstants.Current.UnitCost[dwelling.UnitType];
						var count = 0;
						switch (dwelling.UnitType)
						{
							case UnitType.Infantry:
								if (sensorData.MyTreasury[Resource.Iron] > 0)
								{
									count = Math.Max(sensorData.MyTreasury[Resource.Gold] / cost[Resource.Gold],
										sensorData.MyTreasury[Resource.Iron] / cost[Resource.Iron]);
								}
								break;
							case UnitType.Ranged:
								if (sensorData.MyTreasury[Resource.Glass] > 0)
								{
									count = Math.Max(sensorData.MyTreasury[Resource.Gold] / cost[Resource.Gold],
										sensorData.MyTreasury[Resource.Glass] / cost[Resource.Glass]);
								}
								break;
							case UnitType.Cavalry:
								if (sensorData.MyTreasury[Resource.Ebony] > 0)
								{
									count = Math.Max(sensorData.MyTreasury[Resource.Gold] / cost[Resource.Gold],
										sensorData.MyTreasury[Resource.Ebony] / cost[Resource.Ebony]);
								}
								break;
							case UnitType.Militia:
								count = sensorData.MyTreasury[Resource.Gold] / cost[Resource.Gold];
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						if (count > 0) sensorData = client.HireUnits(Math.Min(count, dwelling.AvailableToBuyCount));
					}
				else
					break;
			}
		}

		public static void SecondStage(HommSensorData sensorData, Graph graph, HommClient client)
		{
			var maxDistance = double.MinValue;
			while (maxDistance < 1000)
			{
				var loc = WayMaker.GetLocation(sensorData, graph, WayMaker.GetDwellingToConquer, out maxDistance);
				while (sensorData.Location.ToLocation() != loc)
				{
					sensorData = client.Move(WayMaker.GetDirection(sensorData.Location.ToLocation(), loc, graph));
					
				}
			}
		}
	}
}
