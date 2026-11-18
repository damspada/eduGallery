# EduGallery

A VR escape room for Meta Quest 3 set in an art-history museum. Students aged 10–15 explore galleries from the Renaissance to Modern Art, solve puzzles about the artworks and unlock the next era.

Team Arcadia (Team 2) · IG.3504 3D and Immersive Reality · ISEP 2026 · [Miro board](https://miro.com/app/board/uXjVERl2Llg=/)

## Tech

- Unity **6000.6.0f1** (everyone uses this exact version)
- URP, Input System, OpenXR + XR Interaction Toolkit
- Git LFS for images, models and audio

## Setup

1. Install Unity 6000.6.0f1 from Unity Hub (add **Android Build Support** to build for the Quest).
2. Install Git LFS: `brew install git-lfs` (macOS) or `sudo apt install git-lfs` (Linux), then `git lfs install`.
3. Clone: `git clone git@github.com:damspada/eduGallery.git`
4. Open the folder with Unity Hub → *Add project from disk*.

## Rules

- Work on your own branch (`feature/<name>`), never directly on `main`.
- Test in your own scene: `Assets/_Project/Scenes/Sandbox/<YourName>.unity`.
- Ask the team before editing the main scene.
- Commit `.meta` files with their assets. Write commit messages in English.
- Add external assets to [CREDITS.md](CREDITS.md).
- Update your tasks on the Miro board.

## Team

| System | Member |
|---|---|
| 1. Environment (room, lighting, door) | Minju Kim |
| 2. Artworks & Data | Sounika |
| 3. Puzzles | Naomie Marday (content) · Damiano Spadaccini (logic) |
| 4. Player (desktop) | Isaac Lee |
| 5. Game Logic | Ange Nolwen Djuissi K. |
| 6. UI | TBD |
| 7. Score & Audio | Damiano Spadaccini |
