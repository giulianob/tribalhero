using Game.Data;

namespace Game.Setup {
    public class Loader {
        public static void loadResource(int wood, int iron, int crop, Global global) {
            /*          Player resourcePlayer = new Player(0);
                      City resourceCity = new City(0,resourcePlayer);            
                      Global.World.Cities.Add(resourceCity.Id, resourceCity);

                      for (int i = 0; i < wood; ++i) {
                          NaturalResource item = new NaturalResource(ResourceType.WOOD,0);
                          item.X = (uint)random.Next((int)global.World.World_Width);
                          item.Y = (uint)random.Next((int)global.World.World_Height);
                          global.World.add(item);
                          resourceCity.add(item);
                          item.City = resourceCity;
                      }
                      for (int i = 0; i < iron; ++i) {
                          NaturalResource item = new NaturalResource(ResourceType.IRON, 0);
                          item.X = (uint)random.Next((int)global.World.World_Width);
                          item.Y = (uint)random.Next((int)global.World.World_Height);
                          global.World.add(item);
                          resourceCity.add(item);
                          item.City = resourceCity;
                      }
                      for (int i = 0; i < crop; ++i) {
                          NaturalResource item = new NaturalResource(ResourceType.CROP, 0);
                          item.X = (uint)random.Next((int)global.World.World_Width);
                          item.Y = (uint)random.Next((int)global.World.World_Height);
                          global.World.add(item);
                          resourceCity.add(item);
                          item.City = resourceCity;
                      }
                  }
                  public static void loadCity1(Global global) {
              /*        City city = new City(1337, null);
                      Iron iron = new Iron(1, 1, 32, 0, 0, 0, 0, 0, 0, 0);
                      city.add(structure);
                      global.World.add(*/
        }
    }
}