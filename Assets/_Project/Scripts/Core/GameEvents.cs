using System;

namespace EduGallery
{
    /// <summary>
    /// Central list of the events the systems use to talk to each other.
    /// A system raises an event or listens to it; it never calls another system directly.
    /// </summary>
    public static class GameEvents
    {
        // Puzzles
        public static event Action<PuzzleBase> PuzzleSolved;
        public static event Action<PuzzleBase> WrongAnswer;
        public static event Action<PuzzleBase> HintShown;

        // Rooms and game flow
        public static event Action<RoomManager> RoomEntered;
        public static event Action<RoomManager> RoomCompleted;
        public static event Action<GameState> GameStateChanged;

        // Score and learning
        public static event Action<int> ScoreChanged;
        public static event Action<ArtworkData> ArtworkInspected;

        // Objects (used for sound and touch feedback)
        public static event Action<Grabbable> ObjectGrabbed;
        public static event Action<Grabbable> ObjectSnapped;

        public static void RaiseObjectGrabbed(Grabbable grabbable) => ObjectGrabbed?.Invoke(grabbable);
        public static void RaiseObjectSnapped(Grabbable grabbable) => ObjectSnapped?.Invoke(grabbable);
        public static void RaisePuzzleSolved(PuzzleBase puzzle) => PuzzleSolved?.Invoke(puzzle);
        public static void RaiseWrongAnswer(PuzzleBase puzzle) => WrongAnswer?.Invoke(puzzle);
        public static void RaiseHintShown(PuzzleBase puzzle) => HintShown?.Invoke(puzzle);
        public static void RaiseRoomEntered(RoomManager room) => RoomEntered?.Invoke(room);
        public static void RaiseRoomCompleted(RoomManager room) => RoomCompleted?.Invoke(room);
        public static void RaiseGameStateChanged(GameState state) => GameStateChanged?.Invoke(state);
        public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
        public static void RaiseArtworkInspected(ArtworkData artwork) => ArtworkInspected?.Invoke(artwork);
    }
}
