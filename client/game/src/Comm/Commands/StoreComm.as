package src.Comm.Commands {
    import com.codecatalyst.promise.Deferred;
    import com.codecatalyst.promise.Promise;

    import src.Comm.Commands;

    import src.Comm.Packet;

    import src.Comm.Session;

    import src.Map.MapComm;
    import src.Objects.Store.StoreItem;
    import src.Objects.Store.StoreItemAchievement;
    import src.Objects.Store.StoreItemTheme;

    public class StoreComm {
        private var mapComm: MapComm;
        private var session: Session;

        public function StoreComm(mapComm: MapComm) {
            this.mapComm = mapComm;
            this.session = mapComm.session;
        }

        public function getItems(): Promise {
            var deferred:Deferred = new Deferred();

            var packet:Packet = new Packet();
            packet.cmd = Commands.STORE_GET_ITEMS;

            session.write(packet)
                    .then(function(result: Packet): void {
                        var items: Array = [];

                        var itemCount: int = result.readInt();
                        for (var i:int = 0; i < itemCount; i++) {

                            var itemType: int = result.readInt();
                            switch(itemType) {
                                case StoreItem.STORE_ITEM_THEME:
                                    items.push(new StoreItemTheme(result.readString(), result.readInt(), result.readDate()));
                                    break;
                                case StoreItem.STORE_ITEM_ACHIEVEMENT:
                                    items.push(new StoreItemAchievement(result.readString(), result.readInt(), result.readDate()));
                                    break;
                                default:
                                    throw new Error("Unknown item type in storeComm getItems");
                            }
                        }

                        deferred.resolve(items);
                    })
                    .otherwise(function(result: Packet): void {
                        mapComm.catchAllErrors(result);
                    })
                    .done();

            return deferred.promise;
        }

        public function purchaseItem(itemId: String): Promise {
            var packet: Packet = new Packet();
            packet.cmd = Commands.STORE_PURCHASE_ITEM;
            packet.writeString(itemId);

            return mapComm.send(session, packet);
        }

        public function setDefaultTheme(cityId: int, theme: String): Promise {
            var packet: Packet = new Packet();
            packet.cmd = Commands.STORE_SET_DEFAULT_THEME;
            packet.writeUInt(cityId);
            packet.writeString(theme);

            return mapComm.send(session, packet);
        }

        public function applyThemeToAll(cityId: int, theme: String): Promise {
            var packet: Packet = new Packet();
            packet.cmd = Commands.STORE_THEME_APPLY_TO_ALL;
            packet.writeUInt(cityId);
            packet.writeString(theme);

            return mapComm.send(session, packet);
        }

        public function setWallTheme(cityId: int, theme: String): Promise {
            var packet: Packet = new Packet();
            packet.cmd = Commands.WALL_SET_THEME;
            packet.writeUInt(cityId);
            packet.writeString(theme);

            return mapComm.send(session, packet);
        }

        public function setDefaultTroopTheme(cityId: int, theme: String): Promise {
            var packet: Packet = new Packet();
            packet.cmd = Commands.STORE_SET_TROOP_THEME;
            packet.writeUInt(cityId);
            packet.writeString(theme);

            return mapComm.send(session, packet);
        }
    }
}
