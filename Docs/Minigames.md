# Minigames Documentation

---

## Abbreviations

| **Abbreviation** | **Full Form**                        |
| ---------------- | ------------------------------------ |
| **HOC**          | History of Computing                 |
| **EOCS**         | Elements of Computer Systems         |
| **NS**           | Number System                        |
| **ITP**          | Introduction to Programming          |

---

## Commonly Shared Variables

- **`correct`**: Stores how many times the player answered correctly.
- **`score`**: Varies depending on the minigame's gameplay.
- **`total`**: Total number of questions that will show, which varies depending on gameplay length.
- **`topic points`**: The currently selected topic, which increases the player's stats (topic) at the end of the minigame.

---

## Minigames

### 1. Runner

- **total**: Set to **10**.

#### Gameplay Scoring

##### Logic

- **Correct Answer**: Increases `score` by **100** for answering correctly.
- **Wrong Answer**: Decreases `score` by **20** for answering incorrectly.

##### Additional Points

Awarded when reaching the finish line:

| **Position** | **Points** |
| ------------ | ---------- |
| 1st          | 100        |
| 2nd          | 75         |
| 3rd          | 50         |
| 4th          | 25         |

---

#### Stat Scoring

##### Computation

| **Attempts**       | **Points Awarded** |
| ------------------ | ------------------ |
| 0 attempts         | 1 point            |
| 1 attempt          | 0.75 points        |
| 2 attempts         | 0.50 points        |
| 3 attempts         | 0.25 points        |
| More than 3 attempts | 0 points          |

##### Note

- **Max Score**: The maximum score a player can gain is **10 topic points** (excluding additional points).
- **Stat Increase**: At the end, the stat increase is based on the player's `topic points`.

---

### 2. Trivia Showdown

- **total**: Set to **15**.

#### Gameplay Scoring

- **Correct Answer**: Increases both `score` and `correct` by 1.
- **Max Score**: The maximum score a player can gain is **15 quiz points**.
- **Stat Increase**: At the end, stat increase is based on the player's `topic points`.

---

### 3. Territory Conquest

- **total**: Depends on how many tiles the player interacted with, computed at the end of the minigame.
- **Correct Answer**: Increases `correct` by 1, then occupies the tile.
- **Wrong Answer**: Only records the answer attempt.
- **Tile Update**: If another player answers correctly, the player's score is decreased by 1, and the other player's score is increased by 1, updating the tile's ownership.
- **Stat Increase**: At the end, computation for stat increase will base on the player's topic points.

---

## Unlocking New Topics

| **Topic**   | **Requirement**                    |
| ----------- | ---------------------------------- |
| **NS**      | Requires `100 HOC topic points`    |
| **ITP**     | Requires `150 EOCS topic points`   |

- The score ceiling will depends on the `total` of each minigame.
---

