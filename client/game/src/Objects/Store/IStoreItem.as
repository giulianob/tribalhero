package src.Objects.Store {
    import flash.display.Sprite;

    public interface IStoreItem {
        function title(): String;

        function thumbnail(): Sprite;
    }
}
