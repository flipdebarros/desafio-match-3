# Match-3

![Match-3](/Match3.png?raw=true "Match-3")

Developed in Unity 2022.3.62f1

## New Features

- Added simple scoring system that rewards 2 points for each matched tile.
- Added 3 new types of special matches:
  - 5-tile match (horizontal and vertical);
  - 4 tile match (horizontal and vertical);
  - T/L/+ match.
- Added 4 special items that are created when a special match is performed and have special effects:
  - Color Bomb (Star): created by a 5 tile match (horizontal and vertical) that when swapped with a normal tile removes all tiles of the same color;
  - Vertical Rocket (Ice cream): created by a 4 tile horizontal match, it removes every simple tile in the same column;
  - Horizontal Rocket (Candy): created by a 4 tile vertical match, it removes every simple tile in the same row;
  - Bomb (Circle): created by a T/L/+ shaped match that explodes and removes every simple tile in a 5x5 area.

## Organization and Performance Improvements

- Split game initialization and board logic into separate classes;
- Improved match checking by caching the position of the tiles that changed in the last iteration alongside a flood-fill technique to find relevant tiles to test instead of the whole board;
- `FindMatches` now returns a list of the positions of the tiles that were matched. This avoids the need to iterate through a board-sized matrix of booleans;
- Extracted the logic of droping the tiles and filling the board into separate methods to make `SwapTiles` more legible;
- `DropTiles` logic now first sifts through the matched tiles to find which columns have gaps and where is the lowest one. Then uses a two-pointer solution to move all the tiles in the column appropriately;
- `DropTiles` is also is able to return a list with the position of the gaps at the top of each column so that `FillBoard` can efficiently iterate through these positions without needing to check every tile on the board;
- Changed the data structure of `_boardTiles` from a List of Lists to an Array of Arrays, to try and keep the data localized and improve cache use. For this to work best `Tile` should be a `struct` instead of a `class`, this is also outlined in the future steps section;
- `BoardView` now uses an Object Pool for the tile `GameObjects` reducing drastically the number of instantiations; 

## Future Steps

### Features

- Make so that if a special item is affected by the effect of other it also activates its effect;
- Make so that the player is able to swap two special items and create a different effect;
- Add some animation to the first selected tile to indicate that it's selected;
- In addition to making a selected animation, adding a function of click and dragging the tile is also an option.

### Organization and Performance

- If there's no plan to the `Tile` class to have a more complex internal state and methods it should probably be converted to a `struct`. The same also applies to the classes `MovedTileInfo`, `AddedTileInfo`, `AddedSpecialItemInfo` and `BoardSequence`.
- The variable `_tilesVariations` (formerly `_tilesTypes`) should be passed in as a parameter and easily controllable from outside the code, preferentially from a settings file;
- The way the score is calculated should not be hard coded, it should at least have parameters that can be controlled from outside the code;
- Ideally the type of special items and the way to get them through special matches should be more easily accessible from outside the code. 

## Links
- https://unity.com/releases/editor/archive
- https://free-game-assets.itch.io/free-match-3-game-assets
