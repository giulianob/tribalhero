package {
    import src.Util.StringHelper;

    public function t(msgId: String, ... params): String {
        return StringHelper.localize(msgId, params);
    }
}
