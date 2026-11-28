public interface IDominoObserver
{
    void OnTurnChanged(int playerIndex, string playerName, PlayerHand handToShow);
    void OnTilePlaced(DominoTile tile, int playerIndex, DominoPlacementSide side);
    void OnTilesDrawn(int playerIndex, int amount);
    void OnPlayerPassed(int playerIndex, string playerName);
    void OnReserveChanged(int remainingTiles);
    void OnGameBlocked();
    void OnFeedback(string message);
}