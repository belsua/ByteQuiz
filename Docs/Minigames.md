**Minigames Documentation**
====================================================

**Abbreviations**
-----------------

* `HOC` - History of Computing
* `EOCS` - Elements of Computer Systems
* `NS` - Number System
* `ITP` - Introduction to Programming

**Commonly Shared Variables**
-----------------------------

### correct

Aka "quiz points" or "score" for storing how many times the player answered correctly.

### score

Aka "game points" minigame specific, varies depending to the minigame's gameplay.

### total

Total questions that will show, also minigame specific varies depending to gameplay length.

### topic points

The currently selected topic which will increases the player stat (topic) at the end of the minigame.

**Runner**
----------

### Values

#### total

Currently set to 10.

### Scoring
-------------

### Gameplay Scoring

#### Correct Answer

Increases `score` by 100 for answering correctly.

#### Wrong Answer

Decreases `score` by 20 for answering incorrectly.

#### Additional Points

Gives additional points when reached the finish line:

* 100 points for 1st.
* 75 points for 2nd.
* 50 points for 3rd.
* 25 points for 4th.

### Stat Scoring
----------------

#### Computation

Computes the score per item depending on the answer attempt and will be totaled at the end of the minigame:

* 0 attempts give 1 points.
* 1 attempt gives 0.75 points.
* 2 attempts give 0.50 points.
* 3 attempts give 0.25 points.
* More than 3 attempts give 0 points.

#### Max Score

Max score can a player can gain is `10 quiz points` or `0.025 topic points`.

#### Stat Increase

At the end - computation for stat increase `(0.25 / quiz points)`.

**Trivia Showdown**
-------------------

### Variables
-------------

#### total

Currently set to 15.

### Scoring
-------------

#### Correct Answer

Increases both `score` and `correct` by 1.

#### Max Score

Max score can a player can gain is `15 quiz points` or `0.025 topic points`.

#### Stat Increase

At the end - computation for stat increase `(quiz points / 600f)`.

**Territory Conquest**
----------------------

### Values
------------

#### total

Depends how many tiles the player interacted with, computes at the end of the minigame.

### Scoring
-------------

#### Correct Answer

Increases `correct` by 1 then occupies the tile.

#### Wrong Answer

Only records the answer attempt.

#### Tile Update

When other player answers correctly the player occupied tile it decrease the player's score by 1 and other player's score by 1 and updates the tile.

#### Stat Increase

At the end - computation for stat increase `(correct / 600f)`.

**Notes**
-------

* To unlock NS, it needs `0.2 HOC topic points`.
* To unlock ITP, it needs `0.3 EOCS topic points`.
* Standard `topic points` ceiling is `0.025`.
	+ If the player continuously gains `0.025 topic points` per minigame it will require 8 games to unlock NS and 12 games for ITP.
